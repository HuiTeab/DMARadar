using DMARadar.Components.Pages;
using System.Security.Cryptography;

namespace DMARadar.Misc
{
	public static class Extensions
	{
		/// <summary>
		/// Restarts a timer from 0. (Timer will be started if not already running)
		/// </summary>
		public static void Restart(this System.Timers.Timer t)
		{
			t.Stop();
			t.Start();
		}

		/// <summary>
		/// Converts 'Degrees' to 'Radians'.
		/// </summary>
		public static double ToRadians(this float degrees)
		{
			return (Math.PI / 180) * degrees;
		}
		/// <summary>
		/// Converts 'Radians' to 'Degrees'.
		/// </summary>
		public static double ToDegrees(this float radians)
		{
			return (180 / Math.PI) * radians;
		}
		/// <summary>
		/// Converts 'Degrees' to 'Radians'.
		/// </summary>
		public static double ToRadians(this double degrees)
		{
			return (Math.PI / 180) * degrees;
		}
		/// <summary>
		/// Converts 'Radians' to 'Degrees'.
		/// </summary>
		public static double ToDegrees(this double radians)
		{
			return (180 / Math.PI) * radians;
		}

		#region GUI Extensions
		/// <summary>
		/// Convert game position to 'Bitmap' Map Position coordinates.
		/// </summary>
		public static MapPosition ToMapPos(this System.Numerics.Vector3 vector, Map map)
		{
			return new MapPosition()
			{
				X = map.ConfigFile.X + (vector.X * map.ConfigFile.Scale),
				Y = map.ConfigFile.Y - (vector.Y * map.ConfigFile.Scale), // Invert 'Y' unity 0,0 bottom left, C# top left
				Height = vector.Z // Keep as float, calculation done later
			};
		}

		/// <summary>
		/// Gets 'Zoomed' map position coordinates.
		/// </summary>
		public static MapPosition ToZoomedPos(this MapPosition location, MapParameters mapParams)
		{
			return new MapPosition()
			{
				UIScale = mapParams.UIScale,
				//X = (location.X - mapParams.Bounds.Left) * mapParams.XScale,
				//Y = (location.Y - mapParams.Bounds.Top) * mapParams.YScale,
				Height = location.Height
			};
		}

	}

	#endregion
}
