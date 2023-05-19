using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey
{
    class Settings
    {
        public string? preferredCommander = null;
        public string? lastCommander = null;
        public string? lastFid= null;

        public LatLong2 targetLatLong = LatLong2.Empty;
        public bool targetLatLongActive = false;

        public bool autoShowBioSummary = true;
        public bool autoShowBioPlot = true;
        public bool focusGameOnStart = true;
        public bool focusGameOnMinimize = true;

        public bool enableGuardianSites = false;
        public bool disableRuinsMeasurementGrid = false;
        public bool disableAerialAlignmentGrid = false;
        public bool hidePlottersFromCombatSuits = false;
        public bool hideOverlaysFromMouse = true;

        [JsonIgnore]
        public float Opacity { get => plotterOpacity / 100f; }
        public float plotterOpacity = 50;

        public Point mainLocation;
        public Rectangle logsLocation;
        public Rectangle allRuinsLocation;

        // screenshot processing
        public bool processScreenshots = false;
        public bool addBannerToScreenshots = true;
        public bool deleteScreenshotOriginal = false;
        public bool useGuardianAerialScreenshotsFolder = true;
        public string screenshotSourceFolder = Elite.defaultScreenshotFolder;
        public string screenshotTargetFolder = Path.Combine(Elite.defaultScreenshotFolder, "converted");

        public double aerialAltAlpha = 1200; // confirm this
        public double aerialAltBeta = 1550;
        public double aerialAltGamma = 1600;

        // configurable colours and fonts
        public Color GameOrange = Color.FromArgb(255, 255, 113, 00); // #FF7100

        public Font font1 = new System.Drawing.Font("Lucida Sans Typewriter", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        public Font fontSmall = new System.Drawing.Font("Lucida Sans Typewriter", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        public Font fontSmall2 = new System.Drawing.Font("Century Gothic", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        public Font fontMiddle = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        public Font font2 = new System.Drawing.Font("Century Gothic", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        public Font fontBig = new System.Drawing.Font("Century Gothic", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        public int processIdx = 0;

        #region loading /saving

        static readonly string settingsPath = Path.Combine(Application.UserAppDataPath, "settings.json");

        public static Settings Load()
        {
            // read and parse file contents into tmp object
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                try
                {
                    var settings = JsonConvert.DeserializeObject<Settings>(json)!;

                    Game.log($"Loaded settings: {json}");
                    return settings;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to read settings: {ex.Message}");
                    Game.log(json);
                }
            }

            // we reach here if the file is missing or corrupt
            Game.log($"Creating new settings file: {settingsPath}");
            var newSettings = new Settings();
            newSettings.Save();

            return newSettings;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(settingsPath, json);
        }

        #endregion
    }
}
