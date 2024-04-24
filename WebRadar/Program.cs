using System.Diagnostics;
using System.Runtime.InteropServices;
using WebRadar.Components;

namespace WebRadar
{
    public class Program
    {
        private static Config? _config;
        private static object _logLock = new();
        private static StreamWriter? _log;
        /// <summary>
        /// Global Program Configuration.
        /// </summary>
        public static Config Config
        {
            get => _config;
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.WebHost.UseUrls("http://*:5000", "https://*:5001");
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            if (Config.TryLoadConfig(out _config) is not true) _config = new Config();
            if (_config.LoggingEnabled)
            {
                _log = File.AppendText("log.txt");
                _log.AutoFlush = true;
            }
            app.Run();
        }

        #region Methods
        /// <summary>
        /// Public logging method, writes to Debug Trace, and a Log File (if enabled in Config.Json)
        /// </summary>
        public static void Log(string msg)
        {
            Debug.WriteLine(msg);
            if (_config != null)
            {
                if (_config.LoggingEnabled)
                {
                    lock (_logLock) // Sync access to File IO
                    {
                        _log.WriteLine($"{DateTime.Now}: {msg}");
                    }
                }
            }
        }
        /// <summary>
        /// Hide the 'Program Console Window'.
        /// </summary>
        public static void HideConsole()
        {
            ShowWindow(GetConsoleWindow(), 1); // 0 : SW_HIDE
        }
        #endregion

        #region P/Invokes
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        #endregion
    }
}
