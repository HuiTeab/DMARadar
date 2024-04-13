using DMARadar.Components;
using DMARadar.Components.Services;
using DMARadar.Misc;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DMARadar
{
	public class Program
    {
		private static readonly Config _config;
		public static Config Config
		{
			get => _config;
		}
        public class OperationMessage
        {
            public string Content { get; set; }
            public MessageType Type { get; set; } // Enum for differentiating types of messages, e.g., Info, Success, Warning.

            public OperationMessage(string content, MessageType type)
            {
                Content = content;
                Type = type;
            }
        }

        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Error
        }

        public enum ApplicationState
        {
            NotInitialized,
            Initializing,
            Initialized,
            RunningTests,
            TestsCompleted
        }

        private static readonly StreamWriter? _log;
		public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<GameLoopService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();

        }

        #region Methods

        /// <summary>
        /// Public logging method, writes to Debug Trace, and a Log File (if enabled in Config.Json)
        /// </summary>
        public static void Log(string msg)
		{
			Debug.WriteLine(msg);
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
