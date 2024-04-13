﻿using DMARadar.Misc;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

namespace DMARadar.Tarkov
{
    /// <summary>
    /// Class containing Game Player Data.
    /// </summary>
    public class Player
    {
        private static readonly FileSystemWatcher _watchlistMonitor;
        private static readonly object _watchlistLock = new();
        private static Dictionary<string, int> _groups = new(StringComparer.OrdinalIgnoreCase);

        private readonly Stopwatch _posRefreshSw = new();
        private readonly Stopwatch _kdRefreshSw = new();
        private readonly object _posLock = new(); // sync access to this.Position (non-atomic)
        private Transform _transform;

        #region PlayerProperties
        /// <summary>
        /// Player is a PMC Operator.
        /// </summary>
        public bool IsPmc { get; }
        /// <summary>
        /// Player is a Local PMC Operator.
        /// </summary>
        public bool IsLocalPlayer { get; }
        /// <summary>
        /// Player is Alive/Not Dead.
        /// </summary>
        public volatile bool IsAlive = true;
        /// <summary>
        /// Player is Active (has not exfil'd).
        /// </summary>
        public volatile bool IsActive = true;
        /// <summary>
        /// Account UUID for Human Controlled Players.
        /// </summary>
        public string AccountID { get; }
        /// <summary>
        /// Player name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Player Level (Based on experience).
        /// </summary>
        public int Lvl { get; } = 0;
        /// <summary>
        /// MemberCategory of Player Account (Developer,Sherpa,etc.) null if ordinary account/eod.
        /// </summary>
        public string Category { get; }
        /// <summary>
        /// Player's Kill/Death Average
        /// </summary>
        public float KDA { get; private set; } = -1f;
        /// <summary>
        /// Group that the player belongs to.
        /// </summary>
        public int GroupID { get; } = -1;
        /// <summary>
        /// Type of player unit.
        /// </summary>
        public PlayerType Type { get; set; }
        /// <summary>
        /// Player's current health (sum of all 7 body parts).
        /// </summary>
        public int Health { get; private set; } = -1;

        public ulong HealthController { get; }

        public ulong InventoryController { get; }

        public ulong InventorySlots { get; }

        public ulong PlayerBody { get; }

        private Vector3 _pos = new Vector3(0, 0, 0); // backing field
        /// <summary>
        /// Player's Unity Position in Local Game World.
        /// </summary>
        ///
        public Vector3 Position // 96 bits, cannot set atomically
        {
            get
            {
                lock (_posLock)
                {
                    return _pos;
                }
            }
            private set
            {
                lock (_posLock)
                {
                    _pos = value;
                }
            }
        }
        /// <summary>
        /// Cached 'Zoomed Position' on the Radar GUI. Used for mouseover events.
        /// </summary>
        public Vector2 ZoomedPosition { get; set; } = new();
        /// <summary>
        /// Player's Rotation (direction/pitch) in Local Game World.
        /// 90 degree offset ~already~ applied to account for 2D-Map orientation.
        /// </summary>
        public Vector2 Rotation { get; private set; } = new Vector2(0, 0); // 64 bits will be atomic
        /// <summary>
        /// (PMC ONLY) Player's Gear Loadout.
        /// Key = Slot Name, Value = Item 'Long Name' in Slot
        /// </summary>
        /// <summary>
        /// If 'true', Player object is no longer in the RegisteredPlayers list.
        /// Will be checked if dead/exfil'd on next loop.
        /// </summary>
        public bool LastUpdate { get; set; } = false;
        /// <summary>
        /// Consecutive number of errors that this Player object has 'errored out' while updating.
        /// </summary>
        public int ErrorCount { get; set; } = 0;
        #endregion

        #region Getters
        /// <summary>
        /// Contains 'Acct UUIDs' of tracked players for the Key, and the 'Reason' for the Value.
        /// </summary>
        private static ReadOnlyDictionary<string, string> Watchlist { get; set; } // init in Static Constructor
        /// <summary>
        /// Contains history of Enemy Players (human-controlled) that are allocated during program runtime.
        /// </summary>
        /// <summary>
        /// Player is a Hostile PMC Operator.
        /// </summary>
        public bool IsHostilePmc
        {
            get => this.IsPmc && (this.Type is PlayerType.PMC || this.Type is PlayerType.SpecialPlayer);
        }
        /// <summary>
        /// Player is human-controlled.
        /// </summary>
        public bool IsHuman
        {
            get => (
                this.Type is PlayerType.LocalPlayer ||
                this.Type is PlayerType.Teammate ||
                this.Type is PlayerType.PMC ||
                this.Type is PlayerType.SpecialPlayer ||
                this.Type is PlayerType.PScav ||
                this.Type is PlayerType.BEAR ||
                this.Type is PlayerType.USEC);
        }
        /// <summary>
        /// Player is human-controlled and Active/Alive.
        /// </summary>
        public bool IsHumanActive
        {
            get => (
                this.Type is PlayerType.LocalPlayer ||
                this.Type is PlayerType.Teammate ||
                this.Type is PlayerType.PMC ||
                this.Type is PlayerType.SpecialPlayer ||
                this.Type is PlayerType.PScav ||
                this.Type is PlayerType.BEAR ||
                this.Type is PlayerType.USEC) && IsActive && IsAlive;
        }
        /// <summary>
        /// Player is human-controlled & Hostile.
        /// </summary>
        public bool IsHumanHostile
        {
            get => (
                this.Type is PlayerType.PMC ||
                this.Type is PlayerType.SpecialPlayer ||
                this.Type is PlayerType.PScav ||
                this.Type is PlayerType.BEAR ||
                this.Type is PlayerType.USEC);
        }
        /// <summary>
        /// Player is human-controlled, hostile, and Active/Alive.
        /// </summary>
        public bool IsHumanHostileActive
        {
            get => (
                this.Type is PlayerType.BEAR ||
                this.Type is PlayerType.USEC ||
                this.Type is PlayerType.SpecialPlayer ||
                this.Type is PlayerType.PScav) && this.IsActive && this.IsAlive;
        }
        /// <summary>
        /// Player is AI & boss, rogue, raider etc.
        /// </summary>
        public bool IsBossRaider
        {
            get => (
                this.Type is PlayerType.AIRaider ||
                this.Type is PlayerType.AIBossFollower ||
                this.Type is PlayerType.AIBossGuard ||
                this.Type is PlayerType.AIRogue ||
                this.Type is PlayerType.AIBoss);
        }
        /// <summary>
        /// Player is AI/human-controlled and Active/Alive.
        /// </summary>
        public bool IsHostileActive
        {
            get => (
                this.Type is PlayerType.PMC ||
                this.Type is PlayerType.BEAR ||
                this.Type is PlayerType.USEC ||
                this.Type is PlayerType.SpecialPlayer ||
                this.Type is PlayerType.PScav ||
                this.Type is PlayerType.AIScav ||
                this.Type is PlayerType.AIRaider ||
                this.Type is PlayerType.AIBossFollower ||
                this.Type is PlayerType.AIBossGuard ||
                this.Type is PlayerType.AIRogue ||
                this.Type is PlayerType.AIOfflineScav ||
                this.Type is PlayerType.AIBoss) && this.IsActive && this.IsAlive;
        }
        /// <summary>
        /// Player is friendly to LocalPlayer (including LocalPlayer) and Active/Alive.
        /// </summary>
        public bool IsFriendlyActive
        {
            get => ((
                this.Type is PlayerType.LocalPlayer ||
                this.Type is PlayerType.Teammate) && this.IsActive && this.IsAlive);
        }
        /// <summary>
        /// Player has exfil'd/left the raid.
        /// </summary>
        public bool HasExfild
        {
            get => !this.IsActive && this.IsAlive;
        }
        /// <summary>
        /// Gets value of player.
        /// </summary>
        /// 
        /// <summary>
        /// EFT.Player Address
        /// </summary>
        public ulong Base { get; }
        /// <summary>
        /// EFT.Profile Address
        /// </summary>
        public ulong Profile { get; }
        /// <summary>
        /// PlayerInfo Address (GClass1044)
        /// </summary>
        public ulong NextObservedPlayerView { get; }
        public ulong Info { get; }
        public ulong TransformInternal { get; }
        public ulong VerticesAddr { get => this._transform?.VerticesAddr ?? 0x0; }
        public ulong IndicesAddr
        {
            get => this._transform?.IndicesAddr ?? 0x0;
        }
        public ulong MovementContext { get; }
        public ulong CorpsePtr
        {
            get => this.Base + Offsets.Player.Corpse;
        }
        /// <summary>
        /// IndicesAddress -> IndicesSize -> VerticesAddress -> VerticesSize
        /// </summary>
        public Tuple<ulong, int, ulong, int> TransformScatterReadParameters
        {
            get => this._transform?.GetScatterReadParameters() ?? new Tuple<ulong, int, ulong, int>(0, 0, 0, 0);
        }
        #endregion

        #region Static_Constructor
        static Player()
        {
            LoadWatchlist();
            _watchlistMonitor = new FileSystemWatcher(".")
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "playerWatchlist.txt",
                EnableRaisingEvents = true
            };
            _watchlistMonitor.Changed += new FileSystemEventHandler(watchlist_Changed);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Player Constructor.
        /// </summary>
        public Player(ulong playerBase, ulong playerProfile, Vector3? pos = null, string baseClassName = null)
        {
            Debug.WriteLine("Player Constructor: Initialization started.");
            try
            {
                this.Base = playerBase;
                this.Profile = playerProfile;
                //Debug.WriteLine($"Player Base Address: 0x{playerBase:X}, Profile Address: 0x{playerProfile:X}");
                if (pos is not null)
                {
                    this.Position = (Vector3)pos; // Populate provided Position (usually only for a re-alloc)
                }
                if (baseClassName is null)
                {
                    return;
                }
                else
                {
                    //Debug.WriteLine($"Base Class Name: {baseClassName}");
                }
                if (baseClassName == "ClientPlayer" || baseClassName == "LocalPlayer" || baseClassName == "HideoutPlayer")
                {
                    ulong localPlayerInfoOffset = playerProfile + Offsets.Profile.PlayerInfo;
                    this.Info = Memory.ReadPtr(localPlayerInfoOffset);
                    this.MovementContext = Memory.ReadPtr(playerBase + Offsets.Player.MovementContext);
                    this.TransformInternal = Memory.ReadPtrChain(playerBase, Offsets.Player.To_TransformInternal);
                    this._transform = new Transform(this.TransformInternal, true);
                    var namePtr = Memory.ReadPtr(this.Info + Offsets.PlayerInfo.Nickname);
                    this.Name = Memory.ReadUnityString(namePtr);
                    this.InventoryController = Memory.ReadPtr(playerBase + Offsets.Player.InventoryController);
                    var inventory = Memory.ReadPtr(InventoryController + Offsets.InventoryController.Inventory);
                    var equipment = Memory.ReadPtr(inventory + Offsets.Inventory.Equipment);
                    this.InventorySlots = Memory.ReadPtr(equipment + Offsets.Equipment.Slots);
                    this.PlayerBody = Memory.ReadPtr(playerBase + Offsets.Player.PlayerBody);

                    try
                    {
                        var gameVersionPtr = Memory.ReadPtr(Info + Offsets.PlayerInfo.GameVersion);
                        var gameVersion = Memory.ReadUnityString(gameVersionPtr);

                        this.GroupID = this.GetGroupID();
                        this.PlayerBody = Memory.ReadPtr(playerBase + 0xA8);

                        //If empty, then it's a scav
                        if (gameVersion == "")
                        {
                            //Console.WriteLine("Scav Detected");
                            this.Type = PlayerType.AIOfflineScav;
                            this.IsLocalPlayer = false;
                            this.IsPmc = false;
                            this.Name = Helpers.TransliterateCyrillic(Name);

                        }
                        else
                        {
                            this.Type = PlayerType.LocalPlayer;
                            this.IsLocalPlayer = true;
                            this.IsPmc = true;
                        }
                    }
                    catch { }
                }
                else if (baseClassName == "ObservedPlayerView")
                {
                    this.IsLocalPlayer = false;
                    //Debug.WriteLine("Processing PMC Player.");
                    var ObservedPlayerView = playerBase;
                    this.MovementContext = Memory.ReadPtrChain(ObservedPlayerView, Offsets.ObservedPlayerView.To_MovementContext);
                    this.TransformInternal = Memory.ReadPtrChain(ObservedPlayerView, Offsets.ObservedPlayerView.To_TransformInternal);
                    this._transform = new Transform(this.TransformInternal, true);
                    this.Name = Memory.ReadUnityString(Memory.ReadPtr(ObservedPlayerView + Offsets.ObservedPlayerView.NickName));
                    this.Name = Helpers.TransliterateCyrillic(this.Name);
                    this.Info = ObservedPlayerView;
                    var playerSide = GetNextObservedPlayerSide();
                    var playerIsAI = GetNextObservedPlayerIsAI();

                    this.PlayerBody = Memory.ReadPtr(ObservedPlayerView + Offsets.ObservedPlayerView.PlayerBody);
                    this.InventoryController = Memory.ReadPtrChain(ObservedPlayerView, Offsets.ObservedPlayerView.To_InventoryController);
                    var inventory = Memory.ReadPtr(this.InventoryController + Offsets.InventoryController.Inventory);
                    var equipment = Memory.ReadPtr(inventory + Offsets.Inventory.Equipment);
                    this.InventorySlots = Memory.ReadPtr(equipment + Offsets.Equipment.Slots);

                    this.AccountID = Memory.ReadUnityString(Memory.ReadPtr(ObservedPlayerView + Offsets.ObservedPlayerView.AccountID));


                    if (Helpers.NameTranslations.ContainsKey(this.Name))
                    {
                        this.Name = Helpers.NameTranslations[this.Name];
                    }


                    this.GroupID = this.GetObservedPlayerGroupID();
                    this.HealthController = Memory.ReadPtrChain(playerBase, Offsets.ObservedPlayerView.To_HealthController);

                    if ((playerSide == 1 || playerSide == 2) && !playerIsAI)
                    {
                        this.IsPmc = true;
                        this.Type = (playerSide == 1 ? PlayerType.USEC : PlayerType.BEAR);
                        //this.KDA = KDManager.GetKD(this.AccountID).Result;
                    }
                    else if (playerSide == 4 && !playerIsAI)
                    {
                        this.Type = PlayerType.PScav;
                    }
                    else if (playerSide == 4 && playerIsAI)
                    {
                        if (Helpers.NameTranslations.ContainsValue(this.Name))
                        {
                            this.Type = PlayerType.AIBoss;
                        }
                        else if (Helpers.RaiderGuardRogueNames.Contains(this.Name))
                        {
                            this.Type = PlayerType.AIRaider;
                        }
                        else
                        {
                            this.Type = PlayerType.AIScav;
                        }
                    }

                }
                else throw new ArgumentOutOfRangeException("classNameString");

                this.FinishAlloc(); // Finish allocation (check watchlist, member type,etc.)
            }
            catch (Exception ex)
            {
                throw new DMAException($"ERROR during Player constructor for base addr 0x{playerBase.ToString("X")}", ex);
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event fires when the Player Watchlist Textfile is updated.
        /// </summary>
        private static void watchlist_Changed(object sender, FileSystemEventArgs e)
        {
            LoadWatchlist();
        }
        #endregion

        #region Setters
        /// <summary>
        /// Set player health.
        /// </summary>
        public bool SetHealth(int ETagStatus)
        {
            try
            {
                float totalHealth;
                switch (ETagStatus)
                {
                    case 1024:
                        totalHealth = 100;
                        break;
                    case 2048:
                        totalHealth = 50;
                        break;
                    case 4096:
                        totalHealth = 10;
                        break;
                    case 8192:
                        totalHealth = 0;
                        break;
                    default:
                        totalHealth = -1;
                        break;
                }
                this.Health = (int)Math.Round(totalHealth);
                return true;
            }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting Player '{Name}' Health: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Set player rotation (Direction/Pitch)
        /// </summary>
        public bool SetRotation(object obj)
        {
            try
            {
                Vector2 rotation = (Vector2)obj; // unbox
                Vector2 result;
                rotation.X -= 90; // degs offset
                if (rotation.X < 0) rotation.X += 360f; // handle if neg

                if (rotation.X < 0) result.X = 360f + rotation.X;
                else result.X = rotation.X;
                if (rotation.Y < 0) result.Y = 360f + rotation.Y;
                else result.Y = rotation.Y;
                this.Rotation = result;

                return true;
            }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting Player '{Name}' Rotation: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Set player position (Vector3 X,Y,Z)
        /// </summary>
        public bool SetPosition(object[] obj)
        {
            try
            {
                if (obj is null)
                    throw new NullReferenceException();
                //Debug.WriteLine($"Player '{Name}' Position: {obj[0]}, {obj[1]}, {obj[2]}");
                this.Position = this._transform.GetPosition(obj);
                return true;
            }
            catch (Exception ex) // Attempt to re-allocate Transform on error
            {
                Program.Log($"ERROR getting Player '{Name}' Position: {ex}");
                if (!this._posRefreshSw.IsRunning)
                    this._posRefreshSw.Start();
                else if (this._posRefreshSw.ElapsedMilliseconds < 250) // Rate limit attempts on getting pos to prevent stutters
                {
                    return false;
                }
                try
                {
                    Program.Log($"Attempting to get new Transform for Player '{this.Name}'...");
                    var transform = new Transform(this.TransformInternal, true);
                    this._transform = transform;
                    Program.Log($"Player '{this.Name}' obtained new Position Transform OK.");
                }
                catch (Exception ex2)
                {
                    Program.Log($"ERROR getting new Transform for Player '{this.Name}': {ex2}");
                }
                finally
                {
                    this._posRefreshSw.Restart();
                }
                return false;
            }
        }

        /// <summary>
        /// Set PMC Player K/D.
        /// </summary>
        public async Task SetKDAsync()
        {
            try
            {
                if (this.KDA == -1f && this.IsHumanActive) // Get K/D for Hostile PMCs
                {
                    if (this._kdRefreshSw.IsRunning && this._kdRefreshSw.ElapsedMilliseconds < 20000)
                        return; // Rate-limit check

                    if (!this._kdRefreshSw.IsRunning || this._kdRefreshSw.ElapsedMilliseconds >= 20000)
                    {
                        this._kdRefreshSw.Restart();
                        //this.KDA = KDManager.GetKD(this.AccountID).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log($"ERROR getting Player '{this.Name}' K/D: {ex}");
                if (!this._kdRefreshSw.IsRunning)
                    this._kdRefreshSw.Start();
            }
        }
        #endregion

        #region Methods
        private void FinishAlloc()
        {
            if (this.IsHumanHostile) // Hostile Human Controlled Players
            {
                // build log message
                string baseMsg = $"{this.Name} ({this.Type}),  L:{this.Lvl}, "; // append name, type, level
                if (this.GroupID != -1)
                    baseMsg += $"G:{this.GroupID}, "; // append group (if in group)
                if (this.Category is not null)
                {
                    this.Type = PlayerType.SpecialPlayer; // Flag Special Account Types
                    baseMsg += $"Special Acct: {this.Category}, "; // append acct type (if special)
                }
            }
        }
        private int GetNextObservedPlayerSide()
        {
            return Memory.ReadValue<int>(Base + Offsets.ObservedPlayerView.PlayerSide);
        }

        private bool GetNextObservedPlayerIsAI()
        {
            return Memory.ReadValue<bool>(Base + Offsets.ObservedPlayerView.IsAI);
        }

        /// <summary>
        /// Get Account ID for Human-Controlled Players.
        /// </summary>
        private string GetAccountID()
        {
            var idPtr = Memory.ReadPtr(this.Profile + Offsets.Profile.AccountId);
            return Memory.ReadUnityString(idPtr);
        }

        /// <summary>
        /// Gets player's Group Number.
        /// </summary>
        private int GetGroupID()
        {
            try
            {
                var grpPtr = Memory.ReadPtr(this.Info + Offsets.PlayerInfo.GroupId);
                var grp = Memory.ReadUnityString(grpPtr);
                _groups.TryAdd(grp, _groups.Count);
                return _groups[grp];
            }
            catch
            {
                return -1; // will return null if Solo / Don't have a team
            }
        }

        private int GetObservedPlayerGroupID()
        {
            try
            {
                var grpPtr = Memory.ReadPtr(this.Info + Offsets.ObservedPlayerView.GroupID);
                var grp = Memory.ReadUnityString(grpPtr);
                _groups.TryAdd(grp, _groups.Count);
                return _groups[grp];
            }
            catch
            {
                return -1; // will return null if Solo / Don't have a team
            }
        }
        /// <summary>
        /// Reloads playerWatchlist.txt into Memory.
        /// </summary>
        public static void LoadWatchlist()
        {
            lock (_watchlistLock) // Sync access to File IO, Resources
            {
                var watchlist = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // Allocate new Dictionary (case insensitive keys)
                if (!File.Exists("playerWatchlist.txt"))
                {
                    File.WriteAllText("playerWatchlist.txt",
                        "PlayerAcctID : Watchlist reason/comment here (one entry per line)");
                }
                else
                {
                    var lines = File.ReadAllLines("playerWatchlist.txt");
                    foreach (var line in lines)
                    {
                        var split = line.Split(':'); // remove single delimiting ':' character
                        if (split.Length == 2)
                        {
                            var id = split[0].Trim();
                            var reason = split[1].Trim();
                            watchlist.TryAdd(id, reason);
                        }
                    }
                }
                Watchlist = new(watchlist); // Update ref
            }
        }
        #endregion
    }
}