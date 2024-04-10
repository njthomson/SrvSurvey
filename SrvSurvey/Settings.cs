using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

namespace SrvSurvey
{
    class Settings
    {
        public string? preferredCommander = null;
        public string? lastCommander = null;
        public string? lastFid = null;
        public string watchedJournalFolder = JournalFile.journalFolder;
        public bool hideJournalWriteTimer = false;

        public LatLong2 targetLatLong = LatLong2.Empty;
        public bool targetLatLongActive = false;

        public bool autoShowBioSummary = true;
        public bool autoShowBioPlot = true;
        public bool autoShowPlotFSS = true;
        public bool autoShowGuardianSummary = true;
        public bool autoShowRamTah = true;
        public bool autoShowPlotSysStatus = true;
        public bool autoShowPlotBioSystem = false;
        public bool skipGasGiantDSS = true;
        public bool skipRingsDSS = false;
        public bool skipLowValueDSS = true;
        public int skipLowValueAmount = 1_000_000;
        public bool skipHighDistanceDSS = false;
        public int skipHighDistanceDSSValue = 100_000;
        public bool autoTrackCompBioScans = true;
        public bool skipAnalyzedCompBioScans = true;
        public bool autoRemoveTrackerOnSampling = true;

        public int bioBucketOne = 3_000_000;
        public int bioBucketTwo = 7_000_000;
        public int bioBucketThree = 12_000_000;

        public bool useExternalData = true;
        public bool autoLoadPriorScans = true;
        public bool skipPriorScansLowValue = false;
        public int skipPriorScansLowValueAmount = 1_000_000;
        public bool showCanonnSignalsOnRadar = true;
        public bool useSmallCirclesWithCanonn = true;
        public bool hideMyOwnCanonnSignals = true;

        public bool focusGameOnStart = true;
        public bool focusGameOnMinimize = true;

        public bool enableGuardianSites = true;
        [JsonIgnore]
        public bool enableEarlyGuardianStructures = true;
        public bool disableRuinsMeasurementGrid = false;
        public bool disableAerialAlignmentGrid = false;
        public bool hidePlottersFromCombatSuits = false;
        public bool hideOverlaysFromMouse = true;

        public bool autoShowFlightWarnings = true;
        public double highGravityWarningLevel = 1.0f;

        [JsonIgnore]
        public float Opacity { get => plotterOpacity / 100f; }
        public float plotterOpacity = 50;

        public float plotterScale = 0;

        public Point formMainLocation;
        public Rectangle formLogsLocation;
        public Rectangle formAllRuinsLocation;
        public Rectangle formRuinsLocation;
        public Rectangle formBeaconsLocation;
        public Rectangle formMapEditor;
        public Rectangle formRamTah;

        public bool mapShowNotes = true;

        public StatusFlags blinkTigger = StatusFlags.HudInAnalysisMode;
        public int blinkDuration = 3000;

        // screenshot processing
        public bool processScreenshots = false;
        public bool addBannerToScreenshots = true;
        public bool deleteScreenshotOriginal = false;
        public bool useGuardianAerialScreenshotsFolder = true;
        public string screenshotSourceFolder = Elite.defaultScreenshotFolder;
        public string screenshotTargetFolder = Path.Combine(Elite.defaultScreenshotFolder, "converted");
        public bool rotateAndTruncateAlphaAerialScreenshots = true;
        public Color screenshotBannerColor = Color.Yellow;
        public bool screenshotBannerLocalTime = false;

        public double aerialAltAlpha = 1200; // confirm this
        public double aerialAltBeta = 1550;
        public double aerialAltGamma = 1600;

        public int idxGuardianPlotter = 0;

        public bool migratedAlphaSiteHeading = false;

        public Color inferColor = Color.FromArgb(255, 102, 255, 255);
        public int inferTolerance = 25;
        public float inferThreshold = 0.002f;

        public bool dataFolder1100 = false;

        public int pubCodexRef = 0;
        public int pubDataSettlementTemplate = 0;
        public int pubDataGuardian = 0;

        public bool keepBioPlottersVisibleEnabled = true;
        public int keepBioPlottersVisibleDuration = 30;

        #region loading /saving

        static readonly string settingsPath = Path.Combine(Program.dataFolder, "settings.json");

        public static Settings Load()
        {
            // read and parse file contents into tmp object
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                if (!string.IsNullOrEmpty(json))
                {
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
