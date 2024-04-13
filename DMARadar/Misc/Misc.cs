using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace DMARadar.Misc
{
	// Small & Miscellaneous Classes/Enums Go here

	#region Program Classes
	/// <summary>
	/// Custom Debug Stopwatch class to measure performance.
	/// </summary>
	public class DebugStopwatch
	{
		private readonly Stopwatch _sw;
		private readonly string _name;

		/// <summary>
		/// Constructor. Starts stopwatch.
		/// </summary>
		/// <param name="name">(Optional) Name of stopwatch.</param>
		public DebugStopwatch(string name = null)
		{
			_name = name;
			_sw = new Stopwatch();
			_sw.Start();
		}

		/// <summary>
		/// End stopwatch and display result to Debug Output.
		/// </summary>
		public void Stop()
		{
			_sw.Stop();
			TimeSpan ts = _sw.Elapsed;
			Debug.WriteLine($"{_name} Stopwatch Runtime: {ts.Ticks} ticks");
		}
	}
	/// <summary>
	/// Global Program Configuration (Config.json)
	/// </summary>
	public class Config
	{
		[JsonIgnore]
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};
		[JsonIgnore]
		private static readonly object _lock = new();
		/// <summary>
		/// Enables Vertical Sync in GUI Render.
		/// </summary>
		[JsonPropertyName("vsyncEnabled")]
		public bool Vsync { get; set; }
		/// <summary>
		/// Player/Teammates Aimline Length (Max: 1000)
		/// </summary>
		[JsonPropertyName("playerAimLine")]
		public int PlayerAimLineLength { get; set; }
		/// <summary>
		/// Last used 'Zoom' level.
		/// </summary>
		[JsonPropertyName("defaultZoom")]
		public int DefaultZoom { get; set; }
		/// <summary>
		/// UI Scale Value (50-200 , default: 100)
		/// </summary>
		[JsonPropertyName("uiScale")]
		public int UIScale { get; set; }
		/// <summary>
		/// Enables loot output on map.
		/// </summary>
		[JsonPropertyName("lootEnabled")]
		public bool LootEnabled { get; set; }
		/// <summary>
		/// Enables Quest Helper output on map.
		/// </summary>
		[JsonPropertyName("questHelperEnabled")]
		public bool QuestHelperEnabled { get; set; }
		/// <summary>
		/// Enables Aimview window in Main Window.
		/// </summary>
		[JsonPropertyName("aimviewEnabled")]
		public bool AimviewEnabled { get; set; }
		/// <summary>
		/// Hides player names & extended player info in Radar GUI.
		/// </summary>
		[JsonPropertyName("hideNames")]
		public bool HideNames { get; set; }
		/// <summary>
		/// Enables/disables showing non-important loot
		/// </summary>
		[JsonPropertyName("importantLootOnly")]
		public bool ImportantLootOnly { get; set; }
		/// <summary>
		/// Enables/disables showing loot value
		/// </summary>
		[JsonPropertyName("hideLootValue")]
		public bool HideLootValue { get; set; }
		/// <summary>
		/// Primary Teammate ACCT ID (for secondary Aimview)
		/// </summary>
		[JsonPropertyName("primaryTeammateAcctId")]
		public string PrimaryTeammateId { get; set; }
		/// <summary>
		/// Enables logging output to 'log.txt'
		/// </summary>
		[JsonPropertyName("loggingEnabled")]
		public bool LoggingEnabled { get; set; }
		/// <summary>
		/// Max game distance to render targets in Aimview, 
		/// and to display dynamic aimlines between two players.
		/// </summary>
		[JsonPropertyName("maxDistance")]
		public float MaxDistance { get; set; }
		/// <summary>
		/// 'Field of View' in degrees to display targets in the Aimview window.
		/// </summary>
		[JsonPropertyName("aimviewFOV")]
		public float AimViewFOV { get; set; }
		/// <summary>
		/// Minimum loot value (rubles) to display 'normal loot' on map.
		/// </summary>
		[JsonPropertyName("minLootValue")]
		public int MinLootValue { get; set; }
		/// <summary>
		/// Minimum loot value (rubles) to display 'important loot' on map.
		/// </summary>
		[JsonPropertyName("minImportantLootValue")]
		public int MinImportantLootValue { get; set; }

		/// <summary>
		/// Enables / disables thermal vision.
		/// </summary>
		[JsonPropertyName("thermalVisionEnabled")]
		public bool ThermalVisionEnabled { get; set; }

		/// <summary>
		/// Enables / disables night vision.
		/// </summary>
		[JsonPropertyName("nightVisionEnabled")]
		public bool NightVisionEnabled { get; set; }

		/// <summary>
		/// Enables / disables thermal optic vision.
		/// </summary>
		[JsonPropertyName("opticThermalVisionEnabled")]
		public bool OpticThermalVisionEnabled { get; set; }

		/// <summary>
		/// Enables / disables no visor.
		/// </summary>
		[JsonPropertyName("noVisorEnabled")]
		public bool NoVisorEnabled { get; set; }

		/// <summary>
		/// Enables / disables no recoil.
		/// </summary>
		[JsonPropertyName("noRecoilEnabled")]
		public bool NoRecoilEnabled { get; set; }

		/// <summary>
		/// Enables / disables no sway.
		/// </summary>
		[JsonPropertyName("noSwayEnabled")]
		public bool NoSwayEnabled { get; set; }

		/// <summary>
		/// Enables / disables max / infinite stamina.
		/// </summary>
		[JsonPropertyName("maxStaminaEnabled")]
		public bool MaxStaminaEnabled { get; set; }

		/// <summary>
		/// Enables / disables double search.
		/// </summary>
		[JsonPropertyName("doubleSearchEnabled")]
		public bool DoubleSearchEnabled { get; set; }

		/// <summary>
		/// Enables / disables jump power modification
		/// </summary>
		[JsonPropertyName("jumpPowerEnabled")]
		public bool JumpPowerEnabled { get; set; }

		/// <summary>
		/// Changes jump power strength
		/// </summary>
		[JsonPropertyName("jumpPowerStrength")]
		public int JumpPowerStrength { get; set; }

		/// <summary>
		/// Enables / disables throw power modification.
		/// </summary>
		[JsonPropertyName("throwPowerEnabled")]
		public bool ThrowPowerEnabled { get; set; }

		/// <summary>
		/// Changes throw power strength.
		/// </summary>
		[JsonPropertyName("throwPowerStrength")]
		public int ThrowPowerStrength { get; set; }

		/// <summary>
		/// Enables / disables faster mag drills.
		/// </summary>
		[JsonPropertyName("magDrillsEnabled")]
		public bool MagDrillsEnabled { get; set; }

		/// <summary>
		/// Changes mag load/unload speed
		/// </summary>
		[JsonPropertyName("magDrillSpeed")]
		public int MagDrillSpeed { get; set; }

		/// <summary>
		/// Enables / disables juggernaut.
		/// </summary>
		[JsonPropertyName("increaseMaxWeightEnabled")]
		public bool IncreaseMaxWeightEnabled { get; set; }

		/// <summary>
		/// Enables / disables juggernaut.
		/// </summary>
		[JsonPropertyName("instantADSEnabled")]
		public bool InstantADSEnabled { get; set; }

		/// <summary>
		/// Enables / disables max / infinite stamina.
		/// </summary>
		[JsonPropertyName("showHoverArmor")]
		public bool ShowHoverArmor { get; set; }

		/// <summary>
		/// Enables / disables exfil names.
		/// </summary>
		[JsonPropertyName("hideExfilNames")]
		public bool HideExfilNames { get; set; }

		/// <summary>
		/// Enables / disables text outlines.
		/// </summary>
		[JsonPropertyName("hideTextOutline")]
		public bool HideTextOutline { get; set; }

		/// <summary>
		/// Enables / disables memory writing master switch.
		/// </summary>
		[JsonPropertyName("masterSwitchEnabled")]
		public bool MasterSwitchEnabled { get; set; }

		/// <summary>
		/// Enables / disables extended reach.
		/// </summary>
		[JsonPropertyName("extendedReachEnabled")]
		public bool ExtendedReachEnabled { get; set; }

		/// <summary>
		/// Enables / disables infinite stamina.
		/// </summary>
		[JsonPropertyName("infiniteStaminaEnabled")]
		public bool InfiniteStaminaEnabled { get; set; }

		/// <summary>
		/// Enables / disables showing corpses.
		/// </summary>
		[JsonPropertyName("showCorpsesEnabled")]
		public bool ShowCorpsesEnabled { get; set; }

		/// <summary>
		/// Enables / disables showing sub items.
		/// </summary>
		[JsonPropertyName("showSubItemsEnabled")]
		public bool ShowSubItemsEnabled { get; set; }

		/// <summary>
		/// Minimum loot value (rubles) to display 'corpses' on map.
		/// </summary>
		[JsonPropertyName("minCorpseValue")]
		public int MinCorpseValue { get; set; }

		/// <summary>
		/// Minimum loot value (rubles) to display 'sub items' on map.
		/// </summary>
		[JsonPropertyName("minSubItemValue")]
		public int MinSubItemValue { get; set; }

		/// <summary>
		/// Enables / disables auto loot refresh.
		/// </summary>
		[JsonPropertyName("autoLootRefreshEnabled")]
		public bool AutoLootRefreshEnabled { get; set; }


		public Config()
		{
			Vsync = true;
			PlayerAimLineLength = 1000;
			DefaultZoom = 100;
			UIScale = 100;
			LootEnabled = true;
			QuestHelperEnabled = true;
			AimviewEnabled = false;
			HideNames = false;
			ImportantLootOnly = false;
			HideLootValue = false;
			NoRecoilEnabled = false;
			NoSwayEnabled = false;
			MaxStaminaEnabled = false;
			LoggingEnabled = false;
			ShowHoverArmor = false;
			MaxDistance = 325;
			AimViewFOV = 30;
			MinLootValue = 90000;
			MinImportantLootValue = 300000;
			PrimaryTeammateId = null;
			NightVisionEnabled = false;
			ThermalVisionEnabled = false;
			NoVisorEnabled = false;
			OpticThermalVisionEnabled = false;
			DoubleSearchEnabled = false;
			JumpPowerEnabled = false;
			JumpPowerStrength = 4;
			ThrowPowerEnabled = false;
			ThrowPowerStrength = 4;
			MagDrillsEnabled = false;
			MagDrillSpeed = 5;
			IncreaseMaxWeightEnabled = false;
			InstantADSEnabled = false;
			HideExfilNames = false;
			HideTextOutline = false;
			MasterSwitchEnabled = false;
			InfiniteStaminaEnabled = false;
			ExtendedReachEnabled = false;
			ShowCorpsesEnabled = false;
			ShowSubItemsEnabled = false;
			MinCorpseValue = 100000;
			MinSubItemValue = 15000;
			AutoLootRefreshEnabled = false;
		}

		/// <summary>
		/// Attempt to load Config.json
		/// </summary>
		/// <param name="config">'Config' instance to populate.</param>
		/// <returns></returns>
		public static bool TryLoadConfig(out Config config)
		{
			lock (_lock)
			{
				try
				{
					if (!File.Exists("Config.json")) throw new FileNotFoundException("Config.json does not exist!");
					var json = File.ReadAllText("Config.json");
					config = JsonSerializer.Deserialize<Config>(json);
					return true;
				}
				catch
				{
					config = null;
					return false;
				}
			}
		}
		/// <summary>
		/// Save to Config.json
		/// </summary>
		/// <param name="config">'Config' instance</param>
		public static void SaveConfig(Config config)
		{
			lock (_lock)
			{
				var json = JsonSerializer.Serialize<Config>(config, _jsonOptions);
				File.WriteAllText("Config.json", json);
			}
		}
	}

	/// <summary>
	/// Defines map position for the 2D Map.
	/// </summary>
	public struct MapPosition
	{
		public MapPosition()
		{
		}
		/// <summary>
		/// Contains (UI) Scaling Value.
		/// </summary>
		public float UIScale = 0;
		/// <summary>
		/// X coordinate on Bitmap.
		/// </summary>
		public float X = 0;
		/// <summary>
		/// Y coordinate on Bitmap.
		/// </summary>
		public float Y = 0;
		/// <summary>
		/// Unit 'height' as determined by Vector3.Z
		/// </summary>
		public float Height = 0;

		private Config _config { get => Program.Config; }

	}

	/// <summary>
	/// Defines a Map for use in the GUI.
	/// </summary>
	public class Map
	{
		/// <summary>
		/// Name of map (Ex: Customs)
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// 'MapConfig' class instance
		/// </summary>
		public readonly MapConfig ConfigFile;
		/// <summary>
		/// File path to Map .JSON Config
		/// </summary>
		public readonly string ConfigFilePath;

		public Map(string name, MapConfig config, string configPath, string mapID)
		{
			Name = name;
			ConfigFile = config;
			ConfigFilePath = configPath;
			MapID = mapID;
		}

		public readonly string MapID;
	}

	/// <summary>
	/// Contains multiple map parameters used by the GUI.
	/// </summary>
	public class MapParameters
	{
		/// <summary>
		/// Contains Scaling Value.
		/// </summary>
		public float UIScale;
		/// <summary>
		/// Contains the 'index' of which map layer to display.
		/// For example: Labs has 3 floors, so there is a Bitmap image for 'each' floor.
		/// Index is dependent on LocalPlayer height.
		/// </summary>
		public int MapLayerIndex;
		/// <summary>
		/// Rectangular 'zoomed' bounds of the Bitmap to display.
		/// </summary>
		//public SKRect Bounds;
		/// <summary>
		/// Regular -> Zoomed 'X' Scale correction.
		/// </summary>
		public float XScale;
		/// <summary>
		/// Regular -> Zoomed 'Y' Scale correction.
		/// </summary>
		public float YScale;
	}

	/// <summary>
	/// Defines a .JSON Map Config File
	/// </summary>
	public class MapConfig
	{
		[JsonIgnore]
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};

		[JsonPropertyName("mapID")]
		public List<string> MapID { get; set; } // New property for map IDs

		[JsonPropertyName("x")]
		public float X { get; set; }

		[JsonPropertyName("y")]
		public float Y { get; set; }

		[JsonPropertyName("scale")]
		public float Scale { get; set; }

		// Updated to match new JSON format
		[JsonPropertyName("mapLayers")]
		public List<MapLayer> MapLayers { get; set; }

		public static MapConfig LoadFromFile(string file)
		{
			var json = File.ReadAllText(file);
			return JsonSerializer.Deserialize<MapConfig>(json, _jsonOptions);
		}

		public void Save(Map map)
		{
			var json = JsonSerializer.Serialize(this, _jsonOptions);
			File.WriteAllText(map.ConfigFilePath, json);
		}
	}

	public class MapLayer
	{
		[JsonPropertyName("minHeight")]
		public float MinHeight { get; set; }

		[JsonPropertyName("filename")]
		public string Filename { get; set; }
	}
	#endregion

	#region Custom EFT Classes
	/// <summary>
	/// Defines a piece of gear
	/// </summary>
	/// <summary>
	/// Contains weapon info for Primary Weapons.
	/// </summary>
	public struct PlayerWeaponInfo
	{
		public string ThermalScope;
		public string AmmoType;

		public override string ToString()
		{
			var result = string.Empty;
			if (AmmoType is not null) result += AmmoType;
			if (ThermalScope is not null)
			{
				if (result != string.Empty) result += $", {ThermalScope}";
				else result += ThermalScope;
			}
			if (result == string.Empty) return null;
			return result;
		}
	}
	/// <summary>
	/// Defines Player Unit Type (Player,PMC,Scav,etc.)
	/// </summary>
	public enum PlayerType
	{
		/// <summary>
		/// Default value if a type cannot be established.
		/// </summary>
		Default,
		/// <summary>
		/// The primary player running this application/radar.
		/// </summary>
		LocalPlayer,
		/// <summary>
		/// Teammate of LocalPlayer.
		/// </summary>
		Teammate,
		/// <summary>
		/// Hostile/Enemy PMC.
		/// </summary>
		PMC,
		/// <summary>
		/// Normal AI Bot Scav.
		/// </summary>
		AIScav,
		/// <summary>
		/// Difficult AI Raider.
		/// </summary>
		AIRaider,
		/// <summary>
		/// Difficult AI Rogue.
		/// </summary>
		AIRogue,
		/// <summary>
		/// Difficult AI Boss.
		/// </summary>
		AIBoss,
		/// <summary>
		/// Player controlled Scav.
		/// </summary>
		PScav,
		/// <summary>
		/// 'Special' Human Controlled Hostile PMC/Scav (on the watchlist, or a special account type).
		/// </summary>
		SpecialPlayer,
		/// <summary>
		/// Hostile/Enemy BEAR PMC.
		/// </summary>
		BEAR,
		/// <summary>
		/// Hostile/Enemy USEC PMC.
		/// </summary>
		USEC,
		/// <summary>
		/// Offline LocalPlayer.
		/// </summary>
		AIOfflineScav,
		AISniperScav,
		AIBossGuard,
		AIBossFollower,

	}
	/// <summary>
	/// Defines Role for an AI Bot Player.
	/// </summary>
	public struct AIRole
	{
		/// <summary>
		/// Name of Bot Player.
		/// </summary>
		public string Name;
		/// <summary>
		/// Type of Bot Player.
		/// </summary>
		public PlayerType Type;
	}
	#endregion

	#region EFT Enums
	[Flags]
	public enum MemberCategory : int
	{
		Default = 0, // Standard Account
		Developer = 1,
		UniqueId = 2, // EOD Account
		Trader = 4,
		Group = 8,
		System = 16,
		ChatModerator = 32,
		ChatModeratorWithPermamentBan = 64,
		UnitTest = 128,
		Sherpa = 256,
		Emissary = 512
	}
	#endregion

	#region Helpers
	public static class Helpers
	{
		/// <summary>
		/// Returns the 'type' of player based on the 'role' value.
		/// </summary>
		/// 
		public static readonly Dictionary<char, string> CyrillicToLatinMap = new Dictionary<char, string>
		{
				{'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
				{'Е', "E"}, {'Ё', "E"}, {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"},
				{'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
				{'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
				{'У', "U"}, {'Ф', "F"}, {'Х', "Kh"}, {'Ц', "Ts"}, {'Ч', "Ch"},
				{'Ш', "Sh"}, {'Щ', "Shch"}, {'Ъ', ""}, {'Ы', "Y"}, {'Ь', ""},
				{'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"},
				{'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
				{'е', "e"}, {'ё', "e"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
				{'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
				{'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
				{'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"},
				{'ш', "sh"}, {'щ', "shch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
				{'э', "e"}, {'ю', "yu"}, {'я', "ya"}
		};

		public static Dictionary<string, string> NameTranslations = new Dictionary<string, string>
		{
			{"Килла", "Killa"},
			{"Решала", "Reshala"},
			{"Глухарь", "Glukhar"},
			{"Штурман", "Shturman"},
			{"Санитар", "Sanitar"},
			{"Тагилла", "Tagilla"},
			{"Рейдеры", "Raider"},
			{"Сектант Жрец", "Cultist Priest"},
			{"Отступники", "Renegade"},
			{"Big Pipe", "Big Pipe"},
			{"Birdeye", "Birdeye"},
			{"Knight", "Knight"},
			{"Зрячий", "Zryachiy"},
			{"Кабан", "Kaban"},
			{"Коллонтай", "Kollontay"}
		};

		public static string[] RaiderGuardRogueNames = {
			"Afraid", // Rogues
            "Andresto",
			"Applejuice",
			"Arizona",
			"Auron",
			"Badboy",
			"Baddie",
			"Beard",
			"Beverly",
			"Bison",
			"Blackbird",
			"Blade",
			"Blakemore",
			"Boatswain",
			"Boogerman",
			"Brockley",
			"Browski",
			"Bullet",
			"Bunny",
			"Butcher",
			"Chester",
			"Churchill",
			"Cliffhanger",
			"Condor",
			"Cook",
			"Cougar",
			"Coyote",
			"Crooked",
			"Cross",
			"Dakota",
			"Dawg",
			"Deceit",
			"Denver",
			"Diggi",
			"Donutop",
			"Duke",
			"Dustin",
			"Enzo",
			"Esquilo",
			"Father",
			"Firion",
			"Floridaman",
			"Foxy",
			"Frenzy",
			"Garandthumb",
			"Goat",
			"Golden",
			"Grandpa",
			"Greyzone",
			"Grim",
			"Grommet",
			"Gunporn",
			"Handsome",
			"Haunted",
			"Hellshrimp",
			"Honorable",
			"Hypno",
			"Instructor",
			"Iowa",
			"Ironfists",
			"James",
			"Jeff",
			"Jersey",
			"John",
			"Juggernaut",
			"Justkilo",
			"Kanzas",
			"Kentucky",
			"Kry",
			"Lancaster",
			"Lee",
			"Legia",
			"Litton",
			"Lost",
			"Lunar",
			"Madknight",
			"Mamba",
			"Marooner",
			"Marquesses",
			"Meldon",
			"Melo",
			"Michigan",
			"Mike",
			"Momma",
			"Mortal",
			"Mother",
			"Nevada",
			"Nine-hole",
			"Noisy",
			"Nukem",
			"Ocean",
			"Oklahoma",
			"OneEye",
			"Oskar",
			"Panther",
			"Philbo",
			"Quebec",
			"Raccoon",
			"Rage",
			"Rambo",
			"Rassler",
			"Receit",
			"Rib-eye",
			"Riot",
			"Rock",
			"Rocket",
			"Ronflex",
			"Ronny",
			"Rossler",
			"RoughDog",
			"Sektant", // Cultists
            "Scar",
			"Scottsdale",
			"Seafarer",
			"Shadow",
			"SharkBait",
			"Sharkkiller",
			"Sherifu",
			"Sherman",
			"Shifty",
			"Slayer",
			"Sly",
			"Snake",
			"Sneaky",
			"Sniperlife",
			"Solem",
			"Solidus",
			"Spectator-6",
			"Spyke",
			"Stamper",
			"Striker",
			"Texas",
			"Three-Teeth",
			"Trent",
			"Trickster",
			"Triggerhappy",
			"Two-Finger",
			"Vicious",
			"Victor",
			"Voodoo",
			"Voss",
			"Wadley",
			"Walker",
			"Weasel",
			"Whale-Eye",
			"Whisky",
			"Whitemane",
			"Woodrow",
			"Wrath",
			"Zed",
			"Zero-Zero",
			"Aimbotkin",
			"Baklazhan", // kaban guards
            "Bazil",
			"Belyash",
			"Bibop",
			"Cheburek",
			"Dihlofos",
			"Docha",
			"Flamberg",
			"Gladius",
			"Gromila",
			"Gus",
			"Kapral",
			"Kartezhnik",
			"Khvost",
			"Kolt",
			"Kompot",
			"Kudeyar",
			"Mauzer",
			"Medoed",
			"Miposhka",
			"Mosin",
			"Moydodyr",
			"Naperstochnik",
			"Supermen",
			"Shtempel",
			"Tihiy",
			"Varan",
			"Vasiliy",
			"Verhniy",
			"Zevaka",
			"Afganec",
			"Alfons", // Glukhar guards
            "Assa",
			"Baks",
			"Balu",
			"Banschik",
			"Barguzin",
			"Basmach",
			"Batar",
			"Batya",
			"Belyy",
			"Bob",
			"Borec",
			"Byk",
			"BZT",
			"Calabrissa",
			"Chelovek",
			"Chempion",
			"Chepushila",
			"Dnevalnyy",
			"Drossel",
			"Dum",
			"Fedya",
			"Gepe",
			"Gepard",
			"Gorbatyy",
			"Gotka",
			"Grif",
			"Grustnyy",
			"Kadrovik",
			"Karabin",
			"Karaul",
			"Kastet",
			"Katok",
			"Kocherga",
			"Kosoy",
			"Krot",
			"Kuling",
			"Kumulyativ",
			"Kuzya",
			"Letyoha",
			"Lysyy",
			"Lyutyy",
			"Maga",
			"Matros",
			"Mihalych",
			"Mysh",
			"Nakat",
			"Nemonas",
			"Oficer",
			"Omeh",
			"Oskolochnyy",
			"Otbityy",
			"Patron",
			"Pluton",
			"Radar",
			"Rayan",
			"Rembo",
			"Ryaha",
			"Salobon",
			"Sapog",
			"Seryy",
			"Shapka",
			"Shustryy",
			"Sibiryak",
			"Signal",
			"Sobr",
			"Specnaz",
			"Stvol",
			"Sych",
			"Tankist",
			"Tihohod",
			"Toropyga",
			"Trubochist",
			"Utyug",
			"Valet",
			"Vegan",
			"Veteran",
			"Vityok",
			"Zampolit",
			"Zarya",
			"Zhirnyy",
			"Zh-12",
			"Zimniy",
			"Anton Zavodskoy", // Reshala guards
            "Damirka Zavodskoy",
			"Filya Zavodskoy",
			"Gena Zavodskoy",
			"Grisha Zavodskoy",
			"Kolyan Zavodskoy",
			"Kuling Zavodskoy",
			"Lesha Zavodskoy",
			"Nikita Zavodskoy",
			"Sanya Zavodskoy",
			"Shtopor Zavodskoy",
			"Skif Zavodskoy",
			"Stas Zavodskoy",
			"Toha Zavodskoy",
			"Torpeda Zavodskoy",
			"Vasya Zavodskoy",
			"Vitek Zavodskoy",
			"Zhora Zavodskoy",
			"Dimon Svetloozerskiy", // Shturman guards
            "Enchik Svetloozerskiy",
			"Kachok Svetloozerskiy",
			"Krysa Svetloozerskiy",
			"Malchik Svetloozerskiy",
			"Marat Svetloozerskiy",
			"Mels Svetloozerskiy",
			"Motlya Svetloozerskiy",
			"Motyl Svetloozerskiy",
			"Pashok Svetloozerskiy",
			"Plyazhnik Svetloozerskiy",
			"Robinzon Svetloozerskiy",
			"Sanya Svetloozerskiy",
			"Shmyga Svetloozerskiy",
			"Tokha Svetloozerskiy",
			"Ugryum Svetloozerskiy",
			"Vovan Svetloozerskiy",
			"Akula", // scav raiders
            "Assa",
			"BZT",
			"Balu",
			"Bankir",
			"Barrakuda",
			"Bars",
			"Berkut",
			"Bob",
			"Dikobraz",
			"Gadyuka",
			"Gepard",
			"Grif",
			"Grizzli",
			"Gyurza",
			"Irbis",
			"Jaguar",
			"Kalan",
			"Karakurt",
			"Kayman",
			"Kobra",
			"Kondor",
			"Krachun",
			"Krasnyy volk",
			"Krechet",
			"Kuling",
			"Leopard",
			"Lev",
			"Lis",
			"Loggerhed",
			"Lyutty",
			"Maga",
			"Mangust",
			"Manul",
			"Mantis",
			"Medved",
			"Nosorog",
			"Orel",
			"Orlan",
			"Padalshchik",
			"Pantera",
			"Pchel",
			"Piton",
			"Piranya",
			"Puma",
			"Radar",
			"Rosomaha",
			"Rys",
			"Sapsan",
			"Sekach",
			"Shakal",
			"Signal",
			"Skorpion",
			"Stervyatnik",
			"Tarantul",
			"Taypan",
			"Tigr",
			"Varan",
			"Vegan",
			"Vepr",
			"Veteran",
			"Volk",
			"Voron",
			"Yaguar",
			"Yastreb",
			"Zubr",
			"Akusher", // Sanitar guards
            "Khirurg",
			"Anesteziolog",
			"Dermatolog",
			"Farmacevt",
			"Feldsher",
			"Fiziolog",
			"Glavvrach",
			"Gomeopat",
			"Hirurg",
			"Immunolog",
			"Kardiolog",
			"Laborant",
			"Lasha Ortoped",
			"Lor",
			"Medbrat",
			"Medsestra",
			"Nevrolog",
			"Okulist",
			"Paracetamol",
			"Pilyulya",
			"Proktolog",
			"Propital",
			"Psihiatr",
			"Psikhiatr",
			"Pyotr Planchik",
			"Revmatolog",
			"Rodion Bubesh",
			"Scavvaf",
			"Shpric",
			"Stomatolog",
			"Terapevt",
			"Travmatolog",
			"Trupovoz",
			"Urolog",
			"Vaha Geroy",
			"Venerolog",
			"Zaveduyuschiy",
			"Zaveduyushchiy",
			"Zhgut",
			"Arsenal", // Kollontay guards
            "Basyak",
			"Dezhurka",
			"Furazhka",
			"Kozyrek Desatnik",
			"Mayor",
			"Slonolyub",
			"Sluzhebka",
			"Starley Desatnik",
			"Starley brat",
			"Starshiy brat",
			"Strelok brat",
			"Tatyanka Desatnik",
			"Visyak",
		"Baba Yaga", // Follower of Morana
        "Buran",
		"Domovoy",
		"Gamayun",
		"Gololed",
		"Gorynych",
		"Hladnik",
		"Hladovit",
		"Holodec",
		"Holodryg",
		"Holodun",
		"Ineevik",
		"Ineyko",
		"Ineynik",
		"Karachun",
		"Kikimora",
		"Koleda",
		"Kupala",
		"Ledorez",
		"Ledovik",
		"Ledyanik",
		"Ledyanoy",
		"Liho",
		"Merzlotnik",
		"Mor",
		"Morozec",
		"Morozina",
		"Morozko",
		"Moroznik",
		"Obmoroz",
		"Poludnik",
		"Serebryak",
		"Severyanin",
		"Sirin",
		"Skvoznyak",
		"Snegobur",
		"Snegoed",
		"Snegohod",
		"Snegovey",
		"Snegovik",
		"Snezhin",
		"Sosulnik",
		"Striga",
		"Studen",
		"Stuzhaylo",
		"Stuzhevik",
		"Sugrobnik",
		"Sugrobus",
		"Talasum",
		"Tryasovica",
		"Tuchevik",
		"Uraganische",
		"Vetrenik",
		"Vetrozloy",
		"Vihrevoy",
		"Viy",
		"Vodyanoy",
		"Vyugar",
		"Vyugovik",
		"Zimar",
		"Zimnik",
		"Zimobor",
		"Zimogor",
		"Zimorod",
		"Zimovey",
		"Zlomraz",
		"Zloveschun"
		};

		public static string TransliterateCyrillic(string input)
		{
			StringBuilder output = new StringBuilder();

			foreach (char c in input)
			{
				output.Append(CyrillicToLatinMap.TryGetValue(c, out var latinEquivalent) ? latinEquivalent : c.ToString());
			}

			return output.ToString();
		}
	}
}
	#endregion

