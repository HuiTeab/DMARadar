﻿using DMARadar.Misc;
using DMARadar.Tarkov;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using vmmsharp;

namespace DMARadar
{

    internal static class Memory
    {

        private static Vmm? vmmInstance;
        public static Vmm? VmmInstance => vmmInstance;
        private const int LOOP_DELAY = 100;

        private static volatile bool _running = false;
        private static volatile bool _restart = false;
        private static volatile bool _ready = false;
        private static readonly Thread _worker;
        private static uint _pid;
        public static ulong _unityBase;
        public static Game? _game;
        private static int _ticksCounter = 0;
        private static volatile int _ticks = 0;
        private static readonly Stopwatch _tickSw = new();

        public static Game.GameStatus GameStatus = Game.GameStatus.NotFound;

        #region Getters
        public static int Ticks
        {
            get => _ticks;
        }
        public static bool InGame
        {
            get => _game?.InGame ?? false;
        }
        public static bool Ready
        {
            get => _ready;
        }
        public static bool InHideout
        {
            get => _game?.InHideout ?? false;
        }
        public static bool IsScav
        {
            get => _game?.IsScav ?? false;
        }
        public static ReadOnlyDictionary<string, Player> Players
        {
            get => _game?.Players;
        }
        public static Player LocalPlayer
        {
            get
            {
                if (_game != null)
                {
                    if (_game.Players != null)
                    {
                        ReadOnlyDictionary<string, Player> players = _game.Players;
                        if (players != null)
                        {
                            return players.FirstOrDefault((KeyValuePair<string, Player> x) => x.Value.Type == PlayerType.LocalPlayer).Value;
                        }
                    }
                }
                return null;
            }
        }

        public static bool IsInitialized
        {
            get => vmmInstance != null;
        }
        #endregion

        #region Startup
        /// <summary>
        /// DMA initialization method.
        /// </summary>
        public static void Init()
        {
            // Ensures that Vmm is only initialized once
            if (vmmInstance != null)
            {
                vmmInstance = VmmInstance;
                _running = true;
                return;
            }
            else
            {
                if (!File.Exists("mmap.txt"))
                {
                    vmmInstance = new Vmm("-printf", "-v", "-device", "fpga", "-waitinitialize");
                    GetMemMap();
                }
                else
                {
                    vmmInstance = new Vmm("-printf", "-v", "-device", "fpga", "-memmap", "mmap.txt");
                    _running = true;
                }
            }
        }
        /// <summary>
        /// DMA deinitialization method.
        /// </summary>
        public static void Dispose()
        {
            // Ensures that Vmm is only deinitialized once
            if (vmmInstance != null)
            {
                vmmInstance.Dispose();
                vmmInstance = null;
                // Additional deinitialization logic can be added here
            }
        }

        /// <summary>
        /// Generates a Physical Memory Map (mmap.txt) to enhance performance/safety.
        /// </summary>
        private static void GetMemMap()
        {
            try
            {
                var map = vmmInstance.Map_GetPhysMem();
                if (map.Length == 0) throw new Exception("Map_GetPhysMem() returned no entries!");
                var sb = new StringBuilder();
                for (int i = 0; i < map.Length; i++)
                {
                    sb.AppendLine($"{i.ToString("D4")}  {map[i].pa.ToString("x")}  -  {(map[i].pa + map[i].cb - 1).ToString("x")}  ->  {map[i].pa.ToString("x")}");
                }
                File.WriteAllText("mmap.txt", sb.ToString());
            }
            catch (Exception ex)
            {
                throw new DMAException("Unable to generate MemMap!", ex);
            }
        }

        /// <summary>
        /// Gets EFT Process ID.
        /// </summary>
        public static bool GetEFTPID()
        {
            try
            {
                ThrowIfDMAShutdown();
                if (!vmmInstance.PidGetFromName("EscapeFromTarkov.exe", out _pid))
                    throw new DMAException("Unable to obtain PID. Game is not running.");
                else
                {
                    Program.Log($"EscapeFromTarkov.exe is running at PID {_pid}");
                    return true;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting PID: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Gets EFT Process ID.
        /// </summary>
        private static bool GetPid()
        {
            try
            {
                ThrowIfDMAShutdown();
                if (!vmmInstance.PidGetFromName("EscapeFromTarkov.exe", out _pid))
                    throw new DMAException("Unable to obtain PID. Game is not running.");
                else
                {
                    Program.Log($"EscapeFromTarkov.exe is running at PID {_pid}");
                    return true;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting PID: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Gets module base entry address for UnityPlayer.dll
        /// </summary>
        public static bool GetModuleBase()
        {
            try
            {
                ThrowIfDMAShutdown();
                _unityBase = vmmInstance.ProcessGetModuleBase(_pid, "UnityPlayer.dll");
                if (_unityBase == 0) throw new DMAException("Unable to obtain Base Module Address. Game may not be running");
                else
                {
                    Program.Log($"Found UnityPlayer.dll at 0x{_unityBase.ToString("x")}");
                    return true;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting module base: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Returns the Module Base Address of mono-2.0-bdwgc.dll
        /// </summary>
        /// <returns>Module Base Address of mono-2.0-bdwgc.dll</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ulong GetMonoModule()
        {
            ulong monoBase = 0;
            try
            {
                ThrowIfDMAShutdown();
                monoBase = vmmInstance.ProcessGetModuleBase(_pid, "mono-2.0-bdwgc.dll");
                if (monoBase == 0) throw new DMAException("Unable to obtain Module Base Address. Game may not be running");
                else
                {
                    Program.Log($"Found mono-2.0-bdwgc.dll at 0x{monoBase:x}");
                    return monoBase;
                }
            }
            catch (DMAShutdown) { throw; }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting module base: {ex}");
            }
            return monoBase;
        }
        #endregion

        #region MemoryThread
        #endregion

        #region ScatterRead
        /// <summary>
        /// (Base)
        /// Performs multiple reads in one sequence, significantly faster than single reads.
        /// Designed to run without throwing unhandled exceptions, which will ensure the maximum amount of
        /// reads are completed OK even if a couple fail.
        /// </summary>
        /// <param name="pid">Process ID to read from.</param>
        /// <param name="entries">Scatter Read Entries to read from for this round.</param>
        /// <param name="useCache">Use caching for this read (recommended).</param>
        internal static void ReadScatter(ReadOnlySpan<IScatterEntry> entries)
        {
            var pagesToRead = new HashSet<ulong>(); // Will contain each unique page only once to prevent reading the same page multiple times
            foreach (var entry in entries) // First loop through all entries - GET INFO
            {
                // Parse Address and Size properties
                ulong addr = entry.ParseAddr();
                uint size = (uint)entry.ParseSize();

                // INTEGRITY CHECK - Make sure the read is valid
                if (addr == 0x0 || size == 0)
                {
                    entry.IsFailed = true;
                    continue;
                }
                // location of object
                ulong readAddress = addr + entry.Offset;
                // get the number of pages
                uint numPages = ADDRESS_AND_SIZE_TO_SPAN_PAGES(readAddress, size);
                ulong basePage = PAGE_ALIGN(readAddress);

                //loop all the pages we would need
                for (int p = 0; p < numPages; p++)
                {
                    ulong page = basePage + PAGE_SIZE * (uint)p;
                    pagesToRead.Add(page);
                }
            }
            var scatters = vmmInstance.MemReadScatter(_pid, Vmm.FLAG_NOCACHE, pagesToRead.ToArray()); // execute scatter read

            foreach (var entry in entries) // Second loop through all entries - PARSE RESULTS
            {
                if (entry.IsFailed) // Skip this entry, leaves result as null
                    continue;

                ulong readAddress = (ulong)entry.Addr + entry.Offset; // location of object
                uint pageOffset = BYTE_OFFSET(readAddress); // Get object offset from the page start address

                uint size = (uint)(int)entry.Size;
                var buffer = new byte[size]; // Alloc result buffer on heap
                int bytesCopied = 0; // track number of bytes copied to ensure nothing is missed
                uint cb = Math.Min(size, (uint)PAGE_SIZE - pageOffset); // bytes to read this page

                uint numPages = ADDRESS_AND_SIZE_TO_SPAN_PAGES(readAddress, size); // number of pages to read from (in case result spans multiple pages)
                ulong basePage = PAGE_ALIGN(readAddress);

                for (int p = 0; p < numPages; p++)
                {
                    ulong page = basePage + PAGE_SIZE * (uint)p; // get current page addr
                    var scatter = scatters.FirstOrDefault(x => x.qwA == page); // retrieve page of mem needed
                    if (scatter.f) // read succeeded -> copy to buffer
                    {
                        scatter.pb
                            .AsSpan((int)pageOffset, (int)cb)
                            .CopyTo(buffer.AsSpan(bytesCopied, (int)cb)); // Copy bytes to buffer
                        bytesCopied += (int)cb;
                    }
                    else // read failed -> set failed flag
                    {
                        entry.IsFailed = true;
                        break;
                    }

                    cb = (uint)PAGE_SIZE; // set bytes to read next page
                    if (bytesCopied + cb > size) // partial chunk last page
                        cb = size - (uint)bytesCopied;

                    pageOffset = 0x0; // Next page (if any) should start at 0x0
                }
                if (bytesCopied != size)
                    entry.IsFailed = true;
                entry.SetResult(buffer);
            }
        }
        #endregion

        #region ReadMethods
        /// <summary>
        /// Read memory into a Span.
        /// </summary>
        public static Span<byte> ReadBuffer(ulong addr, int size)
        {
            if ((uint)size > PAGE_SIZE * 1500) throw new DMAException("Buffer length outside expected bounds!");
            ThrowIfDMAShutdown();
            var buf = vmmInstance.MemRead(_pid, addr, (uint)size, Vmm.FLAG_NOCACHE);
            if (buf.Length != size) throw new DMAException("Incomplete memory read!");
            return buf;
        }

        /// <summary>
        /// Read a chain of pointers and get the final result.
        /// </summary>
        public static ulong ReadPtrChain(ulong ptr, uint[] offsets)
        {
            ulong addr = 0;
            try { addr = ReadPtr(ptr + offsets[0]); }
            catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index 0, addr 0x{ptr.ToString("X")} + 0x{offsets[0].ToString("X")}", ex); }
            for (int i = 1; i < offsets.Length; i++)
            {
                try { addr = ReadPtr(addr + offsets[i]); }
                catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index {i}, addr 0x{addr.ToString("X")} + 0x{offsets[i].ToString("X")}", ex); }
            }
            return addr;
        }
        /// <summary>
        /// Resolves a pointer and returns the memory address it points to.
        /// </summary>
        public static ulong ReadPtr(ulong ptr)
        {
            var addr = ReadValue<ulong>(ptr);
            if (addr == 0x0) throw new NullPtrException();
            else return addr;
        }

        /// <summary>
        /// Resolves a pointer and returns the memory address it points to.
        /// </summary>
        public static ulong ReadPtrNullable(ulong ptr)
        {
            var addr = ReadValue<ulong>(ptr);
            return addr;
        }

        /// <summary>
        /// Read value type/struct from specified address.
        /// </summary>
        /// <typeparam name="T">Specified Value Type.</typeparam>
        /// <param name="addr">Address to read from.</param>
        public static T ReadValue<T>(ulong addr)
            where T : struct
        {
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ThrowIfDMAShutdown();
                var buf = vmmInstance.MemRead(_pid, addr, (uint)size, Vmm.FLAG_NOCACHE);
                return MemoryMarshal.Read<T>(buf);
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR reading {typeof(T)} value at 0x{addr.ToString("X")}", ex);
            }
        }

        /// <summary>
        /// Read null terminated string.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <exception cref="DMAException"></exception>
        public static string ReadString(ulong addr, uint length) // read n bytes (string)
        {
            try
            {
                if (length > PAGE_SIZE) throw new DMAException("String length outside expected bounds!");
                ThrowIfDMAShutdown();
                var buf = vmmInstance.MemRead(_pid, addr, length, Vmm.FLAG_NOCACHE);
                return Encoding.Default.GetString(buf).Split('\0')[0];
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR reading string at 0x{addr.ToString("X")}", ex);
            }
        }

        /// <summary>
        /// Read UnityEngineString structure
        /// </summary>
        public static string ReadUnityString(ulong addr)
        {
            try
            {
                var length = (uint)ReadValue<int>(addr + Offsets.UnityString.Length);
                if (length > PAGE_SIZE) throw new DMAException("String length outside expected bounds!");
                ThrowIfDMAShutdown();
                var buf = vmmInstance.MemRead(_pid, addr + Offsets.UnityString.Value, length * 2, Vmm.FLAG_NOCACHE);
                return Encoding.Unicode.GetString(buf).TrimEnd('\0'); ;
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR reading UnityString at 0x{addr.ToString("X")}", ex);
            }
        }
        #endregion

        #region WriteMethods

        /// <summary>
        /// (Base)
        /// Write value type/struct to specified address.
        /// </summary>
        /// <typeparam name="T">Value Type to write.</typeparam>
        /// <param name="pid">Process ID to write to.</param>
        /// <param name="addr">Virtual Address to write to.</param>
        /// <param name="value"></param>
        /// <exception cref="DMAException"></exception>
        public static void WriteValue<T>(ulong addr, T value)
            where T : unmanaged
        {
            try
            {
                if (!vmmInstance.MemWriteStruct(_pid, addr, value))
                    throw new Exception("Memory Write Failed!");
            }
            catch (Exception ex)
            {
                throw new DMAException($"[DMA] ERROR writing {typeof(T)} value at 0x{addr.ToString("X")}", ex);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets restart flag to re-initialize the game/pointers from the bottom up.
        /// </summary>
        public static void Restart()
        {
            if (InGame)
            {
                _restart = true;
            }
        }

        private static void ThrowIfDMAShutdown()
        {
            if (!_running) throw new DMAShutdown("Memory Thread/DMA is shutting down!");
        }


        /// Mem Align Functions Ported from Win32 (C Macros)
        private const ulong PAGE_SIZE = 0x1000;
        private const int PAGE_SHIFT = 12;

        /// <summary>
        /// The PAGE_ALIGN macro takes a virtual address and returns a page-aligned
        /// virtual address for that page.
        /// </summary>
        private static ulong PAGE_ALIGN(ulong va)
        {
            return (va & ~(PAGE_SIZE - 1));
        }
        /// <summary>
        /// The ADDRESS_AND_SIZE_TO_SPAN_PAGES macro takes a virtual address and size and returns the number of pages spanned by the size.
        /// </summary>
        private static uint ADDRESS_AND_SIZE_TO_SPAN_PAGES(ulong va, uint size)
        {
            return (uint)((BYTE_OFFSET(va) + (size) + (PAGE_SIZE - 1)) >> PAGE_SHIFT);
        }

        /// <summary>
        /// The BYTE_OFFSET macro takes a virtual address and returns the byte offset
        /// of that address within the page.
        /// </summary>
        private static uint BYTE_OFFSET(ulong va)
        {
            return (uint)(va & (PAGE_SIZE - 1));
        }
        #endregion
    }

    #region Exceptions
    public class DMAException : Exception
    {
        public DMAException()
        {
        }

        public DMAException(string message)
            : base(message)
        {
        }

        public DMAException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class NullPtrException : Exception
    {
        public NullPtrException()
        {
        }

        public NullPtrException(string message)
            : base(message)
        {
        }

        public NullPtrException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class DMAShutdown : Exception
    {
        public DMAShutdown()
        {
        }

        public DMAShutdown(string message)
            : base(message)
        {
        }

        public DMAShutdown(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    #endregion
}
