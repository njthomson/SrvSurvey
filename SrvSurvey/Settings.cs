using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.units;
using SrvSurvey.widgets;

namespace SrvSurvey
{
    class Settings
    {
        public bool buildProjects_TEST = false;
        public Color defaultOrange = GameColors.Defaults.Orange;
        public Color defaultOrangeDim = GameColors.Defaults.OrangeDim;
        public Color defaultCyan = GameColors.Defaults.Cyan;
        public Color defaultDarkCyan = GameColors.Defaults.DarkCyan;

        public string? preferredCommander = null;
        public string? lastCommander = null;
        public string? lastFid = null;
        public string watchedJournalFolder = JournalFile.journalFolder;
        public bool hideJournalWriteTimer = false;
        public string? lang;

        public LatLong2 targetLatLong = LatLong2.Empty;
        public bool targetLatLongActive = false;

        public int fadeInDuration = 150;

        public bool autoShowBioSummary = true;
        public bool autoShowBioPlot = true;
        public int bioPlotSize = 3;
        public bool autoHideBioPlotNoGear = false;
        public bool autoShowPlotFSS = true;
        public bool autoShowPlotFSSInfo = true;
        public bool autoShowPlotFSSInfoInSystemMap = false;
        public bool autoShowGuardianSummary = true;
        public bool autoShowRamTah = true;
        public bool autoShowPlotSysStatus = true;
        public bool autoShowPlotBioSystem = true;
        public bool drawBodyBiosOnlyWhenNear = true;
        public bool highlightRegionalFirsts = false;
        public bool autoShowPlotGalMap = true;
        public bool autoShowPlotJumpInfo = true;
        public bool showPlotJumpInfoIfNextHop = false;
        public bool useLastUpdatedFromSpanshNotEDSM = false;

        public bool autoShowPlotBodyInfo = true;
        public bool autoShowPlotBodyInfoInMap = true;
        public bool autoShowPlotBodyInfoInOrbit = true;
        public bool autoHidePlotBodyInfoInBubble = true;
        public int bodyInfoBubbleSize = 200;
        public bool autoShowPlotBodyInfoAtSurface = false;

        public bool autoShowPlotMassacre_TEST = false;
        public bool autoShowPlotMiniTrack_TEST = false;
        public bool autoShowPlotStationInfo_TEST = false;
        public bool autoShowFloatie_TEST = false;

        public bool autoShowHumanSitesTest = false;
        public int plotHumanSiteWidth = 500;
        public int plotHumanSiteHeight = 600;
        public float humanSiteZoomShip = 1;
        public float humanSiteZoomSRV = 1.5f;
        public float humanSiteZoomFoot = 2;
        public bool humanSiteAutoZoomInside = true;
        public float humanSiteZoomInside = 4;
        public bool humanSiteAutoZoomTool = true;
        public float humanSiteZoomTool = 6;
        public bool humanSiteShow_Medkit = true;
        public bool humanSiteShow_Battery = true;
        public bool humanSiteShow_DataTerminal = true;

        /// <summary>
        /// Whether to show dots at the locations we collected mats at a human settlement.
        /// </summary>
        public bool humanSiteDotsOnCollection = true;

        /// <summary>
        /// For Human settlements ...
        /// </summary>
        public bool collectMatsCollectionStatsTest = false;

        public bool skipGasGiantDSS = true;
        public bool skipRingsDSS = false;
        public bool skipLowValueDSS = true;
        public int skipLowValueAmount = 1_000_000;
        public int hideFssLowValueAmount = 10_000;
        public bool skipHighDistanceDSS = false;
        public int skipHighDistanceDSSValue = 100_000;
        public bool showNonBodySignals = false;
        public bool autoTrackCompBioScans = true;
        public bool skipAnalyzedCompBioScans = true;
        public bool autoRemoveTrackerOnSampling = true;
        public bool autoRemoveTrackerOnFinalSample = false;

        public double bioRingBucketOne = 3;
        public double bioRingBucketTwo = 7;
        public double bioRingBucketThree = 12;

        public bool tempRange_TEST = false;
        public bool formPredictionsCurrentBodyOnly = false;

        /// <summary>
        /// Controls if we make any calls to get exploration or bio data
        /// </summary>
        public bool useExternalData = true;
        /// <summary>
        /// Controls if use any bio data from external data calls
        /// </summary>
        public bool useExternalBioData = false;
        public bool autoLoadPriorScans = true;
        public bool skipPriorScansLowValue = false;
        public int skipPriorScansLowValueAmount = 1_000_000;
        public bool showCanonnSignalsOnRadar = true;
        public bool useSmallCirclesWithCanonn = true;
        public bool hideMyOwnCanonnSignals = true;

        public bool focusGameOnStart = true;
        public bool focusGameOnMinimize = true;
        public bool focusGameAfterFsdJump = false;

        public bool enableGuardianSites = true;
        public bool autoZoomGuardianNearObelisks = true;
        public bool autoZoomGuardianInTurret = false;
        public bool guardianComponentMaterials_TEST = false;

        public bool disableRuinsMeasurementGrid = false;
        public bool disableAerialAlignmentGrid = false;
        public bool hidePlottersFromCombatSuits = false;
        public bool hidePlottersFromMaverickSuits = false;
        public bool hideOverlaysFromMouse = true;
        public bool hideOverlaysFromMouseInFSS_TEST = false;
        public bool hideGeoCountInFssInfo = false;
        public bool hideGeoCountInBioSystem = false;

        public bool autoShowFlightWarnings = true;
        public double highGravityWarningLevel = 1.0f;

        [JsonIgnore]
        public float Opacity { get => plotterOpacity / 100f; }
        public float plotterOpacity = 50;

        public float plotterScale = 0;

        public Rectangle formRuinsLocation;
        public Rectangle formMapEditor;
        public Rectangle formPostProcess;
        public Rectangle formBuilder;

        public Dictionary<string, Rectangle> formLocations = new Dictionary<string, Rectangle>();

        // FormRuins settings
        public bool mapShowNotes = true;
        public bool mapShowLegend = true;

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

        public int pubBioCriteria = 0;
        public int pubCodexRef = 0;
        public int pubDataSettlementTemplate = 0;
        public int pubDataGuardian = 0;
        public int pubSettlements = 0;

        public DateTime lastCodexRefDownload = DateTime.MinValue;
        public DateTime lastCodexNotFoundDownload = DateTime.MinValue;

        public bool keepBioPlottersVisibleEnabled = true;
        public int keepBioPlottersVisibleDuration = 120;

        public int formGenusFilter = 0;
        public int formGenusFontSize = 1;
        public bool formGenusShowRingGuide = true;

        public string? localFloraFolder = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string downloadCodexImageFolder = CodexRef.defaultCodexImagesFolder;
        public bool preDownloadCodexImages = false;

        public bool darkTheme = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public PlotFSS.WatchFssPixelSettings? watchFssSettings_TEST = new();

        public bool keyhook_TEST = false;
        public bool hookDirectX_TEST = false;
        public Guid hookDirectXDeviceId_TEST;

        public Dictionary<KeyAction, string>? keyActions_TEST = KeyChords.defaultKeys;
        /// <summary>
        /// Distance at which we remove vs replace a tracked location
        /// </summary>
        public int minimumKeyLocationTrackingDistance = 50;

        /// <summary> Use to make overlays remain even though the game loses focus </summary>
        public bool keepOverlays = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool overlayTombs = false;

        public bool systemNotesTopMost = false;
        public bool viewJourneyTopMost = false;
        public bool viewJourneyGalacticTime = true;

        public bool logDockToDockTimes = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Notifications allowNotifications = new();

        #region loading / saving

        static readonly string settingsPath = Path.Combine(Program.dataFolder, "settings.json");

        public static Settings Load()
        {
            // read and parse file contents into tmp object
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var settings = JsonConvert.DeserializeObject<Settings>(json)!;

                        Game.log($"Loaded settings: {json}");

                        // ensure some things are populated
                        if (string.IsNullOrWhiteSpace(settings.downloadCodexImageFolder))
                            settings.downloadCodexImageFolder = CodexRef.defaultCodexImagesFolder;

                        return settings;
                    }
                    catch (Exception ex)
                    {
                        Game.log($"Failed to read settings: {ex}");
                        Game.log(json);
                        MessageBox.Show("Failed to load settings. Please share the following and include the logs", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        FormErrorSubmit.Show(ex);
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
            this.Save(true);
        }

        private void Save(bool allowRetry)
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                Game.log($"Failed to write settings (allowRetry: {allowRetry}): {ex}");
                // allow a single retry if we fail to write settings
                if (allowRetry)
                    Program.control.BeginInvoke(() => this.Save(false));
            }
        }

        #endregion

        public class Notifications
        {
            public bool materialCountAfterPickup = true;
            public bool cargoMissionRemaining = true;
            public bool currentBoxelSearchStatus = true;
            public bool showNextBoxelToSearch = true;
            public bool fcMarketPurchaseBugReminder = true;
        }
    }
}
