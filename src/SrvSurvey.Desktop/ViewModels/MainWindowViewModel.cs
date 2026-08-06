using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using SrvSurvey.Core.Combat;
using SrvSurvey.Core.Colonization;
using SrvSurvey.Core.Diagnostics;
using SrvSurvey.Core.Exobiology;
using SrvSurvey.Core.Exploration;
using SrvSurvey.Core.Guardian;
using SrvSurvey.Core.Inara;
using SrvSurvey.Core.Journal;
using SrvSurvey.Core.Journeys;
using SrvSurvey.Core.Navigation;
using SrvSurvey.Core.Network;
using SrvSurvey.Core.Quests;
using SrvSurvey.Core.Routes;
using SrvSurvey.Core.Search;
using SrvSurvey.Core.Settlements;
using SrvSurvey.Core.Storage;
using SrvSurvey.Core.Travel;
using SrvSurvey.Core.Updates;
using SrvSurvey.Desktop.Configuration;
using SrvSurvey.Desktop.Input;
using SrvSurvey.Desktop.Platform;
using SrvSurvey.Desktop.Platform.Frontier;
using SrvSurvey.Desktop.Platform.Overlay;
using SrvSurvey.Desktop.Theming;

namespace SrvSurvey.Desktop.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly TimeSpan IdleHousekeepingInterval =
        TimeSpan.FromSeconds(5);

    private const string Unavailable = "—";

    private readonly JournalFolderResolution folderResolution;
    private readonly JournalDirectoryMonitor? journalMonitor;
    private readonly JournalSessionState journalState = new();
    private readonly ExplorationState explorationState = new();
    private readonly ExobiologyState exobiologyState;
    private readonly CommanderProfileStore commanderProfileStore;
    private readonly CommanderCodexStore commanderCodexStore;
    private readonly CommanderCodexJournalTracker commanderCodexJournalTracker;
    private readonly SystemScanPersistenceStore systemScanPersistenceStore;
    private readonly ISystemBodyDataClient? systemBodyDataClient;
    private readonly CargoInventoryState cargoInventoryState = new();
    private readonly FirstFootfallInferenceSettingsStore
        firstFootfallInferenceSettingsStore;
    private readonly IFirstFootfallInferenceService
        firstFootfallInferenceService;
    private readonly CancellationTokenSource firstFootfallInferenceCancellation =
        new();
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA2213:Disposable fields should be disposed",
        Justification = "The system-body worker disposes the captured source in its finally block.")]
    private CancellationTokenSource? systemBodyDataCancellation;
    private readonly RouteAutoCopyCoordinator routeAutoCopyCoordinator;
    private readonly GreenGasGiantPublicationCoordinator
        greenGasGiantPublicationCoordinator;
    private readonly IEddnPublisher eddnPublisher;
    private readonly IInaraPublisher inaraPublisher;
    private readonly RavenThemeService? themeService;
    private readonly LegacyProfileImporter profileImporter;
    private readonly QuestRuntimeCoordinator questRuntimeCoordinator;
    private readonly QuestSettingsStore questSettingsStore;
    private readonly HttpClient? visitedStarsHttpClient;
    private readonly ApplicationLogService? applicationLogService;
    private Func<DirectoryInfo, Task<bool>>? journalCommandDirectoryLauncher;
    private Func<Task>? journalCommandShutdownRequester;
    private Func<string, Task>? journalCommandClipboardWriter;
    private readonly AsyncCommand importLegacyProfileCommand;
    private readonly AsyncCommand resetExplorationCommand;
    private readonly AsyncCommand cancelResetExplorationCommand;
    private readonly AsyncCommand resetExobiologyCommand;
    private readonly AsyncCommand cancelResetExobiologyCommand;
    private readonly AsyncCommand clearSurfaceTrackersCommand;
    private readonly AsyncCommand toggleFirstFootfallCommand;
    private bool isBusy;
    private bool isImportingProfile;
    private string statusMessage;
    private string commanderName = Unavailable;
    private string frontierId = Unavailable;
    private string gameDescription = Unavailable;
    private string gameMode = Unavailable;
    private string systemDescription = Unavailable;
    private string bodyName = Unavailable;
    private string sessionState = "Waiting for journal";
    private string lastUpdated = string.Empty;
    private string themeStatusMessage = string.Empty;
    private string vehicleState = Unavailable;
    private string surfacePosition = Unavailable;
    private string headingAndAltitude = Unavailable;
    private string gameUiFocus = Unavailable;
    private string estimatedExplorationValue = "0 CR";
    private string explorationJumps = "0";
    private string explorationDistance = "0.0 ly";
    private string explorationBodies = "Scanned: 0, DSS: 0, Landed: 0";
    private string explorationStatusMessage = "Waiting for commander profile.";
    private bool isResetExplorationPending;
    private string unclaimedBioRewards = "0 CR";
    private string unclaimedBioScans = "0 samples";
    private string organicScanProgress = "Ready for sample 1 of 3";
    private string activeOrganicSpecies = Unavailable;
    private string organicSampleRange = Unavailable;
    private string bioFirstFootfall = "Unknown";
    private bool isCurrentBodyFirstFootfall;
    private bool canToggleCurrentBodyFirstFootfall;
    private bool isOrganicSample1Complete;
    private bool isOrganicSample2Complete;
    private string exobiologyStatusMessage = "Waiting for commander profile.";
    private string commanderCodexStatusMessage =
        "Waiting for Commander Codex journal entries.";
    private bool isResetExobiologyPending;
    private string? activeProfileFrontierId;
    private string? activeProfileCommanderName;
    private bool activeProfileIsOdyssey = true;
    private NavigationItemViewModel? selectedNavigation;
    private bool isProfileSelected;
    private ThemeOptionViewModel selectedTheme;
    private LegacyProfileOptionViewModel? selectedLegacyProfile;
    private string legacyProfileSourcePath;
    private string profileStatusMessage;
    private string settingsLinkStatusMessage = string.Empty;
    private string questStatusMessage = "Quests are disabled.";
    private string? activeProfileRavenApiKey;
    private string? surveyCodexFrontierId;
    private int? surveyCodexRegionId;
    private long? surveyCodexSystemAddress;
    private long? activeSystemVisitAddress;
    private DateTimeOffset? activeSystemVisitedAt;
    private string? loadedSystemHistoryKey;
    private string? loadedSystemBodyDataKey;
    private EliteStatus? latestStatus;
    private CargoSnapshot? latestCargo;
    private ShipLockerSnapshot? latestShipLocker;
    private bool awaitFreshCargoSnapshot;
    private DateTimeOffset? companionIdentityChangedAt;
    private DateTimeOffset lastIdleHousekeepingAt;
    private bool disposed;

    public MainWindowViewModel(
        string? configuredJournalDirectory,
        RavenThemeService? themeService = null,
        AppDataPaths? appDataPaths = null,
        LegacyProfileImporter? profileImporter = null,
        ExobiologyReferenceCatalog? exobiologyCatalog = null,
        IStarSystemResolver? starSystemResolver = null,
        IBoxelSystemResolver? boxelSystemResolver = null,
        GlobalInputSettingsViewModel? inputSettings = null,
        ColonizationViewModel? colonization = null,
        INearestSystemsClient? nearestSystemsClient = null,
        ISystemSummaryClient? systemSummaryClient = null,
        JumpInfoSettingsStore? jumpInfoSettingsStore = null,
        SystemSurveySettingsStore? systemSurveySettingsStore = null,
        BiologyPredictionsSettingsStore? biologyPredictionsSettingsStore = null,
        CombatSettingsStore? combatSettingsStore = null,
        GuardianOverlaySettingsStore? guardianOverlaySettingsStore = null,
        StationInfoSettingsStore? stationInfoSettingsStore = null,
        HumanSiteSettingsStore? humanSiteSettingsStore = null,
        ApplicationLogService? applicationLogService = null,
        LegacyOverlayLayoutStore? overlayLayoutStore = null,
        LegacyOverlayLayout? overlayLayout = null,
        IScreenshotProcessingService? screenshotProcessingService = null,
        QuestRuntimeCoordinator? questRuntimeCoordinator = null,
        QuestSettingsStore? questSettingsStore = null,
        string? targetFrontierId = null,
        ICommanderInstanceLauncher? commanderInstanceLauncher = null,
        IGameWindowSwitcher? gameWindowSwitcher = null,
        VisitedStarsCacheViewModel? visitedStarsCache = null,
        GreenGasGiantPublicationCoordinator?
            greenGasGiantPublicationCoordinator = null,
        NotificationSettingsStore? notificationSettingsStore = null,
        StreamOverlaySettingsStore? streamOverlaySettingsStore = null,
        VrOverlaySettingsStore? vrOverlaySettingsStore = null,
        VrOverlayCalibrationStore? vrOverlayCalibrationStore = null,
        GalaxyMapSettingsStore? galaxyMapSettingsStore = null,
        PulseOverlaySettingsStore? pulseOverlaySettingsStore = null,
        OverlayBehaviorSettingsStore? overlayBehaviorSettingsStore = null,
        OverlayScaleSettingsStore? overlayScaleSettingsStore = null,
        JournalSettingsStore? journalSettingsStore = null,
        SystemScanPersistenceStore? systemScanPersistenceStore = null,
        CodexImageSettingsStore? codexImageSettingsStore = null,
        DockToDockSettingsStore? dockToDockSettingsStore = null,
        DockToDockLogService? dockToDockLogService = null,
        DesktopBehaviorSettingsStore? desktopBehaviorSettingsStore = null,
        BiologyRewardSettingsStore? biologyRewardSettingsStore = null,
        CommanderPreferenceSettingsStore?
            commanderPreferenceSettingsStore = null,
        bool commanderPreferenceCommandLineOverride = false,
        string? commanderPreferenceInitialStatus = null,
        FirstFootfallInferenceSettingsStore?
            firstFootfallInferenceSettingsStore = null,
        IFirstFootfallInferenceService? firstFootfallInferenceService = null,
        RavenServiceSettingsStore? ravenServiceSettingsStore = null,
        ReleaseUpdateViewModel? releaseUpdates = null,
        ReferenceDataUpdateViewModel? referenceDataUpdates = null,
        LocalizationViewModel? localization = null,
        OverlayThemeSettingsViewModel? overlayThemeSettings = null,
        OverlayInteractionViewModel? overlayInteraction = null,
        ICanonnHumanSiteClient? canonnHumanSiteClient = null,
        ICanonnHumanSitePublisher? canonnHumanSitePublisher = null,
        IEddnPublisher? eddnPublisher = null,
        ISystemBodyDataClient? systemBodyDataClient = null,
        IInaraPublisher? inaraPublisher = null,
        CommanderProfileViewModel? frontierProfile = null)
    {
        this.themeService = themeService;
        this.profileImporter = profileImporter ?? new LegacyProfileImporter();
        this.applicationLogService = applicationLogService;
        AppDataPaths = appDataPaths ?? AppDataPaths.ResolveCurrent();
        var sharedJournalSettingsStore = journalSettingsStore
            ?? new JournalSettingsStore(AppDataPaths.UiSettingsPath);
        folderResolution = JournalFolderLocator.ResolveCurrent(
            configuredJournalDirectory
                ?? sharedJournalSettingsStore.Load().Directory);
        ICommunityGoalJournalHistoryReader? communityGoalHistoryReader =
            folderResolution.SelectedPath is { } journalPath
                ? new CommunityGoalJournalHistoryReader(journalPath)
                : null;
        FrontierProfile = frontierProfile ?? new CommanderProfileViewModel(
            FrontierAccountService.CreateCurrent(AppDataPaths.DataDirectory),
            communityGoalHistoryReader: communityGoalHistoryReader);
        var legacyReferences = LegacyReferenceCatalogLoader.Load(
            AppDataPaths.DataDirectory);
        var regionalCodexCandidates = RegionalCodexCandidateCatalog.Load(
            AppDataPaths.DataDirectory);
        var knownSystems = KnownSystemAddressCatalog.Load(
            AppDataPaths.DataDirectory);
        foreach (var warning in legacyReferences.Warnings)
        {
            applicationLogService?.Append(warning);
        }
        foreach (var warning in regionalCodexCandidates.Warnings)
        {
            applicationLogService?.Append(warning);
        }
        foreach (var warning in knownSystems.Warnings)
        {
            applicationLogService?.Append(warning);
        }

        ReferenceDataStatus = legacyReferences.LocalCatalogCount == 0
            ? "Validated embedded reference catalogs are active."
            : $"Using {legacyReferences.LocalCatalogCount:N0} validated catalog(s) "
                + "from the imported legacy profile; all others use embedded defaults.";
        if (legacyReferences.Warnings.Count > 0)
        {
            ReferenceDataStatus += $" {legacyReferences.Warnings.Count:N0} incompatible "
                + "or incomplete legacy catalog(s) were ignored safely; see logs.";
        }
        if (regionalCodexCandidates.HasData)
        {
            ReferenceDataStatus += $" Imported regional Codex candidates: "
                + $"{regionalCodexCandidates.Count:N0}.";
        }
        else if (regionalCodexCandidates.Warnings.Count > 0)
        {
            ReferenceDataStatus += " The imported regional Codex candidate "
                + "catalog was incompatible and ignored safely; see logs.";
        }
        if (knownSystems.HasData)
        {
            ReferenceDataStatus += $" Imported known system addresses: "
                + $"{knownSystems.Count:N0}.";
        }
        else if (knownSystems.Warnings.Count > 0)
        {
            ReferenceDataStatus += " The imported known-system address "
                + "catalog was incompatible and ignored safely; see logs.";
        }

        Action<string>? referenceUpdateLog = applicationLogService is null
            ? null
            : message => applicationLogService.Append(message);
        ReferenceDataUpdates = referenceDataUpdates
            ?? new ReferenceDataUpdateViewModel(
                new PublishedReferenceUpdateService(),
                AppDataPaths.DataDirectory,
                ReferenceDataStatus,
                referenceUpdateLog);
        Localization = localization ?? new LocalizationViewModel(
            new LocalizationSettingsStore(
                AppDataPaths.UiSettingsPath,
                AppDataPaths.DataDirectory));

        var ravenServiceUri = (ravenServiceSettingsStore
                ?? new RavenServiceSettingsStore(AppDataPaths.UiSettingsPath))
            .LoadServiceUri();
        this.questSettingsStore = questSettingsStore
            ?? new QuestSettingsStore(AppDataPaths.UiSettingsPath);
        this.questRuntimeCoordinator = questRuntimeCoordinator
            ?? new QuestRuntimeCoordinator(
                new LegacyQuestStateStore(AppDataPaths.DataDirectory),
                new RavenQuestClient(serviceUri: ravenServiceUri),
                message => applicationLogService?.Append(message));
        QuestWorkspace = new QuestWorkspaceViewModel(
            this.questRuntimeCoordinator,
            this.questSettingsStore);
        QuestIndicator = new QuestIndicatorViewModel();
        this.questRuntimeCoordinator.Changed += OnQuestCoordinatorChanged;
        SystemNicknames = new SystemNicknameViewModel(
            SystemNicknameCatalog.Load(AppDataPaths.DataDirectory),
            new SystemNicknameSettingsStore(AppDataPaths.UiSettingsPath));
        DiagnosticsLog = new DiagnosticsLogViewModel(applicationLogService);
        ReleaseUpdates = releaseUpdates ?? new ReleaseUpdateViewModel(
            new ReleaseUpdateService(),
            ReleaseVersion.FromAssembly(typeof(MainWindowViewModel).Assembly),
            new ReleaseUpdateSettingsStore(AppDataPaths.UiSettingsPath));
        JournalInspector = new JournalInspectorViewModel(
            ReplayQuestJournalEventAsync);
        JournalSettings = new JournalSettingsViewModel(
            sharedJournalSettingsStore,
            configuredJournalDirectory);
        commanderProfileStore = new CommanderProfileStore(
            AppDataPaths.DataDirectory);
        commanderCodexStore = new CommanderCodexStore(
            AppDataPaths.DataDirectory);
        commanderCodexJournalTracker = new CommanderCodexJournalTracker(
            commanderCodexStore);
        this.systemScanPersistenceStore = systemScanPersistenceStore
            ?? new SystemScanPersistenceStore(AppDataPaths.DataDirectory);
        this.systemBodyDataClient = systemBodyDataClient;
        this.firstFootfallInferenceSettingsStore =
            firstFootfallInferenceSettingsStore
                ?? new FirstFootfallInferenceSettingsStore(
                    AppDataPaths.UiSettingsPath);
        this.firstFootfallInferenceService = firstFootfallInferenceService
            ?? new UnavailableFirstFootfallInferenceService();
        InputSettings = inputSettings ?? new GlobalInputSettingsViewModel(
            new GlobalInputSettingsStore(AppDataPaths.UiSettingsPath),
            OverlayPlatformCapabilities.DetectCurrent());
        var sharedGameWindowSwitcher = gameWindowSwitcher
            ?? GameWindowSwitcher.CreateCurrent();
        DesktopBehavior = new DesktopBehaviorViewModel(
            desktopBehaviorSettingsStore
                ?? new DesktopBehaviorSettingsStore(AppDataPaths.UiSettingsPath),
            sharedGameWindowSwitcher);
        var sharedOverlayLayoutStore = overlayLayoutStore
            ?? new LegacyOverlayLayoutStore(AppDataPaths.DataDirectory);
        var activeOverlayLayout = overlayLayout
            ?? sharedOverlayLayoutStore.Load();
        OverlayLayout = new OverlayLayoutSettingsViewModel(
            sharedOverlayLayoutStore,
            activeOverlayLayout);
        OverlayScale = new OverlayScaleSettingsViewModel(
            overlayScaleSettingsStore
                ?? new OverlayScaleSettingsStore(AppDataPaths.UiSettingsPath),
            activeOverlayLayout);
        OverlayBehavior = new OverlayBehaviorViewModel(
            overlayBehaviorSettingsStore
                ?? new OverlayBehaviorSettingsStore(AppDataPaths.UiSettingsPath));
        OverlayInteraction = overlayInteraction ?? new OverlayInteractionViewModel(
            OverlayPlatformCapabilities.DetectCurrent());
        OverlayInteractionBinding = InputSettings.Bindings.Single(binding =>
            binding.Definition.Action
                == GlobalInputAction.ToggleOverlayInteraction);
        OverlayTheme = overlayThemeSettings ?? new OverlayThemeSettingsViewModel(
            new LegacyOverlayThemeStore(
                Path.Combine(AppDataPaths.DataDirectory, "theme.json")),
            new OverlayThemeStateStore(
                Path.Combine(
                    AppDataPaths.DataDirectory,
                    "overlay-theme-states.json")),
            themeService);
        ScreenshotProcessing = new ScreenshotProcessingViewModel(
            new ScreenshotProcessingSettingsStore(AppDataPaths.UiSettingsPath),
            screenshotProcessingService);
        DockToDock = new DockToDockViewModel(
            dockToDockSettingsStore
                ?? new DockToDockSettingsStore(AppDataPaths.UiSettingsPath),
            dockToDockLogService
                ?? new DockToDockLogService(
                    DockToDockCsvWriter.GetDefaultPath()));
        Notifications = new NotificationViewModel(
            notificationSettingsStore
                ?? new NotificationSettingsStore(AppDataPaths.UiSettingsPath));
        PulseOverlay = new PulseOverlayViewModel(
            pulseOverlaySettingsStore
                ?? new PulseOverlaySettingsStore(AppDataPaths.UiSettingsPath));
        StreamOverlay = new StreamOverlayViewModel(
            streamOverlaySettingsStore
                ?? new StreamOverlaySettingsStore(AppDataPaths.UiSettingsPath));
        VrOverlay = new VrOverlayViewModel(
            vrOverlaySettingsStore
                ?? new VrOverlaySettingsStore(AppDataPaths.UiSettingsPath),
            vrOverlayCalibrationStore
                ?? new VrOverlayCalibrationStore(AppDataPaths.DataDirectory));
        NetworkPrivacy = new NetworkPrivacyViewModel(
            new NetworkPrivacySettingsStore(AppDataPaths.UiSettingsPath));
        Inara = new InaraSettingsViewModel(
            new InaraSettingsStore(AppDataPaths.UiSettingsPath),
            commanderProfileStore);
        this.inaraPublisher = inaraPublisher ?? new InaraPublisher(
            (typeof(MainWindowViewModel).Assembly.GetName().Version
                ?? new Version(0, 0)).ToString());
        Inara.UploadDisabled += OnInaraUploadDisabled;
        this.eddnPublisher = eddnPublisher ?? new EddnPublisher(
            (typeof(MainWindowViewModel).Assembly.GetName().Version
                ?? new Version(0, 0)).ToString(),
            outboxPath: Path.Combine(
                AppDataPaths.DataDirectory,
                "eddn-outbox-v1.json"),
            log: message => applicationLogService?.Append(message));
        NetworkPrivacy.EddnUploadEnabledChanged += OnEddnUploadEnabledChanged;
        this.eddnPublisher.SetEnabled(NetworkPrivacy.EddnUploadEnabled);
        this.greenGasGiantPublicationCoordinator =
            greenGasGiantPublicationCoordinator
                ?? new GreenGasGiantPublicationCoordinator(
                    legacyReferences.GreenGasGiants,
                    new GreenGasGiantClient(serviceUri: ravenServiceUri));
        Colonization = colonization ?? new ColonizationViewModel(
            new ColonizationSettingsStore(AppDataPaths.UiSettingsPath),
            client: new RavenColonialClient(serviceUri: ravenServiceUri),
            commanderProfileStore: commanderProfileStore,
            legacyProfileStore: new LegacyColonizationProfileStore(
                AppDataPaths.DataDirectory));
        var sharedSystemResolver = starSystemResolver
            ?? new SpanshStarSystemResolver();
        var sharedExobiologyCatalog = exobiologyCatalog
            ?? legacyReferences.Exobiology;
        var defaultCodexImageCache = Path.Combine(
            AppDataPaths.CacheDirectory,
            "codex-images");
        CodexImages = new CodexImageSettingsViewModel(
            codexImageSettingsStore
                ?? new CodexImageSettingsStore(
                    AppDataPaths.UiSettingsPath,
                    defaultCodexImageCache),
            sharedExobiologyCatalog,
            defaultCodexImageCache);
        var systemNoteStore = new SystemNoteStore(AppDataPaths.DataDirectory);
        var systemNotesSettingsStore = new SystemNotesSettingsStore(
            AppDataPaths.DataDirectory);
        var journeyService = new JourneyService(
            new JourneyStore(AppDataPaths.DataDirectory),
            new JourneyJournalHistoryReader(
                folderResolution.SelectedPath
                    ?? (folderResolution.CandidatePaths.Count > 0
                        ? folderResolution.CandidatePaths[0]
                        : null)
                    ?? Path.Combine(AppDataPaths.DataDirectory, "journals")),
            commanderProfileStore,
            sharedExobiologyCatalog);
        Search = new SphereLimitViewModel(
            commanderProfileStore,
            sharedSystemResolver);
        NearestSystems = new NearestSystemsViewModel(
            nearestSystemsClient ?? new NearestSystemsClient(),
            sharedSystemResolver);
        BoxelSearch = new BoxelSearchViewModel(
            commanderProfileStore,
            new LegacySystemDataReader(AppDataPaths.DataDirectory),
            new EmptyBoxelStore(AppDataPaths.DataDirectory),
            boxelSystemResolver ?? new SpanshBoxelClient(),
            knownSystems: knownSystems);
        GroundTarget = new GroundTargetViewModel(
            new GroundTargetSettingsStore(AppDataPaths.DataDirectory));
        SystemNotes = new SystemNotesViewModel(
            systemNoteStore,
            systemNotesSettingsStore,
            journeyService);
        Journey = new JourneyWorkspaceViewModel(
            journeyService,
            sharedSystemResolver,
            systemNoteStore,
            systemNotesSettingsStore);
        var spanshRouteClient = new SpanshRouteClient();
        var routeNameImporter = new RouteNameImporter(sharedSystemResolver);
        var routeService = new FollowRouteService(
            new FollowRouteStore(AppDataPaths.DataDirectory));
        Route = new RouteWorkspaceViewModel(
            routeService,
            routeNameImporter,
            spanshRouteClient);
        RouteManager = new RouteManagerViewModel(routeService, Route);
        var fleetCarrierRouteService = new FollowRouteService(
            new FollowRouteStore(
                AppDataPaths.DataDirectory,
                FollowRouteKind.FleetCarrier));
        FleetCarrierRoute = new RouteWorkspaceViewModel(
            fleetCarrierRouteService,
            routeNameImporter,
            spanshRouteClient,
            FollowRouteKind.FleetCarrier);
        FleetCarrierRouteManager = new RouteManagerViewModel(
            fleetCarrierRouteService,
            FleetCarrierRoute);
        routeAutoCopyCoordinator = new RouteAutoCopyCoordinator(
            Route,
            FleetCarrierRoute);
        var sharedJumpInfoSettingsStore = jumpInfoSettingsStore
            ?? new JumpInfoSettingsStore(AppDataPaths.UiSettingsPath);
        var sharedSystemSummaryClient = systemSummaryClient
            ?? new SystemSummaryClient(
                useSpanshLastUpdated: () => sharedJumpInfoSettingsStore
                    .Load()
                    .UseSpanshLastUpdated);
        JumpInfo = new JumpInfoViewModel(
            sharedSystemSummaryClient,
            sharedJumpInfoSettingsStore,
            legacyReferences.GuardianSites);
        GalaxyMap = new GalaxyMapOverlayViewModel(
            sharedSystemSummaryClient,
            galaxyMapSettingsStore
                ?? new GalaxyMapSettingsStore(AppDataPaths.UiSettingsPath),
            SystemNicknames);
        StationInfo = new StationInfoViewModel(
            sharedSystemSummaryClient,
            stationInfoSettingsStore
                ?? new StationInfoSettingsStore(AppDataPaths.UiSettingsPath));
        BiologyRewards = new BiologyRewardSettingsViewModel(
            biologyRewardSettingsStore
                ?? new BiologyRewardSettingsStore(AppDataPaths.UiSettingsPath));
        SystemSurvey = new SystemSurveyViewModel(
            systemSurveySettingsStore
                ?? new SystemSurveySettingsStore(AppDataPaths.UiSettingsPath),
            biologyCatalog: sharedExobiologyCatalog,
            biologyRewardThresholds: BiologyRewards.Thresholds,
            biologyCriteria: legacyReferences.BiologyCriteria,
            regionalCodexCandidates: regionalCodexCandidates);
        HumanSite = new HumanSiteViewModel(
            humanSiteSettingsStore
                ?? new HumanSiteSettingsStore(AppDataPaths.UiSettingsPath),
            new HumanSiteKnowledgeStore(AppDataPaths.DataDirectory),
            new HumanSiteMaterialStore(AppDataPaths.DataDirectory),
            legacyReferences.HumanSiteTemplates,
            canonnClient: canonnHumanSiteClient,
            useExternalData: () => SystemSurvey.UseExternalData,
            canonnPublisher: canonnHumanSitePublisher,
            publishCanonnGeometry: () =>
                NetworkPrivacy.UploadHumanSettlementGeometry,
            reportCanonnPublication: result =>
            {
                NetworkPrivacy.ReportPublicationResult(result);
                if (!string.IsNullOrWhiteSpace(result.Warning))
                {
                    applicationLogService?.Append(result.Warning);
                }
            });
        BiologyRewards.PropertyChanged += OnBiologyRewardsChanged;
        Combat = new CombatViewModel(
            combatSettingsStore
                ?? new CombatSettingsStore(AppDataPaths.UiSettingsPath),
            commanderProfileStore);
        var systemSurfaceStore = new SystemSurfaceStore(
            AppDataPaths.DataDirectory);
        SurfaceSurvey = new SurfaceSurveyViewModel(
            SystemSurvey,
            systemSurfaceStore,
            new SurfaceSurveyJournalTracker(
                systemSurfaceStore,
                sharedExobiologyCatalog));
        BiologyPredictions = new BiologyPredictionsViewModel(
            SystemSurvey,
            biologyPredictionsSettingsStore
                ?? new BiologyPredictionsSettingsStore(
                    AppDataPaths.UiSettingsPath));
        BiologyCodex = new BiologyCodexViewModel(
            SystemSurvey,
            sharedExobiologyCatalog,
            legacyReferences.BiologyCriteria,
            () => activeProfileCommanderName ?? journalState.CommanderName);
        var journalImportDirectory = folderResolution.SelectedPath
            ?? (folderResolution.CandidatePaths.Count > 0
                ? folderResolution.CandidatePaths[0]
                : null)
            ?? Path.Combine(AppDataPaths.DataDirectory, "journals");
        ProfileBackupDirectory = Path.Combine(
            Path.GetDirectoryName(AppDataPaths.DataDirectory)
                ?? AppDataPaths.ConfigDirectory,
            "legacy-backups");
        CodexBingo = new BiologyCodexBingoViewModel(
            commanderCodexStore,
            sharedExobiologyCatalog,
            new CanonnCodexChallengeImporter(
                new CanonnCodexChallengeClient(),
                commanderCodexStore,
                sharedExobiologyCatalog),
            new CommanderCodexJournalImporter(
                journalImportDirectory,
                commanderCodexStore),
            new CodexDiscoveryLocationClient());
        JournalPostProcessor = new JournalPostProcessorViewModel(
            new CommanderProfileCatalog(AppDataPaths.DataDirectory),
            new JournalHistoryAnalyzer(journalImportDirectory),
            new LegacySystemBiologyAnalyzer(AppDataPaths.DataDirectory),
            new HistoricalSystemRebuildService(
                AppDataPaths.DataDirectory,
                journalImportDirectory,
                Path.Combine(
                    ProfileBackupDirectory,
                    "historical-systems")),
            new CommanderCodexJournalImporter(
                journalImportDirectory,
                commanderCodexStore),
            new GreenGasGiantClient(serviceUri: ravenServiceUri),
            () => NetworkPrivacy.UploadGreenGasGiantCandidates);
        RamTah = new RamTahViewModel(commanderProfileStore);
        Guardian = new GuardianViewModel(
            AppDataPaths.DataDirectory,
            references: legacyReferences.GuardianSites,
            publishedSites: legacyReferences.GuardianPublishedSites,
            templates: legacyReferences.GuardianTemplates,
            ramTah: RamTah,
            overlaySettingsStore: guardianOverlaySettingsStore
                ?? new GuardianOverlaySettingsStore(
                    AppDataPaths.UiSettingsPath),
            gesturePreferences: new GuardianGestureSettingsStore(
                AppDataPaths.UiSettingsPath).Load(),
            aerialAltitudeProvider: () => new GuardianAerialAltitudes(
                ScreenshotProcessing.AerialAltitudeAlpha,
                ScreenshotProcessing.AerialAltitudeBeta,
                ScreenshotProcessing.AerialAltitudeGamma),
            screenshotTargetFolderProvider: () =>
                ScreenshotProcessing.TargetFolder);
        ScreenshotProcessing.PropertyChanged += (_, eventArgs) =>
        {
            Guardian.RefreshAerialGuidance();
            if (eventArgs.PropertyName == nameof(
                    ScreenshotProcessingViewModel.TargetFolder))
            {
                Guardian.RefreshScreenshotAvailability();
            }
        };
        exobiologyState = new ExobiologyState(sharedExobiologyCatalog);
        LegacyProfiles = LegacyProfileLocator.Discover(
                AppDataPaths.LegacyProfileCandidates)
            .Select(discovery => new LegacyProfileOptionViewModel(discovery))
            .ToArray();
        selectedLegacyProfile = LegacyProfiles.Count > 0
            ? LegacyProfiles[0]
            : null;
        legacyProfileSourcePath = selectedLegacyProfile?.Path ?? string.Empty;
        profileStatusMessage = GetInitialProfileStatus();
        importLegacyProfileCommand = new AsyncCommand(
            ImportLegacyProfileAsync,
            CanImportLegacyProfile);
        ImportLegacyProfileCommand = importLegacyProfileCommand;
        JournalFolderPath = folderResolution.SelectedPath
            ?? (folderResolution.CandidatePaths.Count > 0
                ? folderResolution.CandidatePaths[0]
                : null)
            ?? "No journal location is configured.";
        CandidatePaths = folderResolution.CandidatePaths.Count == 0
            ? "No default locations are available for this platform."
            : string.Join(Environment.NewLine, folderResolution.CandidatePaths);
        TargetFrontierId = string.IsNullOrWhiteSpace(targetFrontierId)
            ? null
            : targetFrontierId.Trim();
        var commanderProfileCatalog = new CommanderProfileCatalog(
            AppDataPaths.DataDirectory);
        CommanderPreference = new CommanderPreferenceViewModel(
            commanderPreferenceSettingsStore
                ?? new CommanderPreferenceSettingsStore(
                    AppDataPaths.UiSettingsPath),
            commanderProfileCatalog,
            commanderPreferenceCommandLineOverride,
            commanderPreferenceInitialStatus);
        CommanderInstances = new CommanderInstancesViewModel(
            commanderProfileCatalog,
            commanderInstanceLauncher
                ?? new ApplicationCommanderInstanceLauncher(),
            JournalFolderPath,
            TargetFrontierId,
            sharedGameWindowSwitcher);
        CommanderInstances.PropertyChanged += OnCommanderInstancesPropertyChanged;
        SetSharedCargoSuppressed(CommanderInstances.HasMultipleGameWindows);
        this.eddnPublisher.SetSuspended(
            CommanderInstances.HasMultipleGameWindows);
        if (visitedStarsCache is null)
        {
            var processDetector = new EliteGameProcessDetector();
            visitedStarsHttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(45),
            };
            VisitedStarsCache = new VisitedStarsCacheViewModel(
                new CommanderProfileCatalog(AppDataPaths.DataDirectory),
                new VisitedStarsCacheService(
                    visitedStarsHttpClient,
                    Path.Combine(AppDataPaths.CacheDirectory, "star-cache"),
                    processDetector.IsRunning),
                VisitedStarsCacheTargetLocator.ResolveCurrent,
                processDetector.IsRunning);
        }
        else
        {
            VisitedStarsCache = visitedStarsCache;
        }
        statusMessage = folderResolution.IsFound
            ? (TargetFrontierId is null) switch
            {
                true => "Ready to read the newest Journal.*.log file.",
                false => $"Ready to read journals for {TargetFrontierId}."
            }
            : $"Journal folder not found. Set {JournalFolderLocator.EnvironmentVariableName} "
                + "or start with --journal-directory <path>.";
        journalMonitor = folderResolution.SelectedPath is null
            ? null
            : new JournalDirectoryMonitor(
                folderResolution.SelectedPath,
                TargetFrontierId);
        RefreshCommand = new AsyncCommand(RefreshAsync, () => !IsBusy);
        ShowProfileCommand = new AsyncCommand(ShowProfileAsync, () => true);
        resetExplorationCommand = new AsyncCommand(
            ResetExplorationAsync,
            () => activeProfileFrontierId is not null);
        ResetExplorationCommand = resetExplorationCommand;
        cancelResetExplorationCommand = new AsyncCommand(
            CancelResetExplorationAsync,
            () => IsResetExplorationPending);
        CancelResetExplorationCommand = cancelResetExplorationCommand;
        resetExobiologyCommand = new AsyncCommand(
            ResetExobiologyAsync,
            () => activeProfileFrontierId is not null);
        ResetExobiologyCommand = resetExobiologyCommand;
        cancelResetExobiologyCommand = new AsyncCommand(
            CancelResetExobiologyAsync,
            () => IsResetExobiologyPending);
        CancelResetExobiologyCommand = cancelResetExobiologyCommand;
        clearSurfaceTrackersCommand = new AsyncCommand(
            ClearSurfaceTrackersAsync,
            () => activeProfileFrontierId is not null);
        ClearSurfaceTrackersCommand = clearSurfaceTrackersCommand;
        toggleFirstFootfallCommand = new AsyncCommand(
            async () =>
            {
                await ToggleCurrentBodyFirstFootfallAsync();
            },
            () => CanToggleCurrentBodyFirstFootfall);
        ToggleFirstFootfallCommand = toggleFirstFootfallCommand;

        NavigationItems =
        [
            new("overview", "Overview", "Commander and current journal state"),
            new("exploration", "Exploration", "Trip totals and body scans"),
            new("exobiology", "Exobiology", "Organic scans and unclaimed rewards"),
            new("travel", "Travel", "Ground targets, journeys, and routes"),
            new("search", "Search", "Spherical and boxel searches"),
            new("guardian", "Guardian", "Sites, maps, and Ram Tah"),
            new("quests", "Quests", "Communications and active objectives"),
            new("colonisation", "Colonisation", "Raven Colonial projects"),
            new("diagnostics", "Diagnostics", "Journal source and parsed state"),
            new("settings", "Settings", "Appearance and application options"),
            new("guides", "Guides", "Help documentation and overlay icon glossary"),
        ];
        selectedNavigation = NavigationItems[0];
        Guides = new GuidesViewModel(GuideCatalog.Create());

        var currentTheme = themeService?.Current
            ?? RavenThemeCatalog.Get(RavenThemeCatalog.DefaultThemeKey);
        ThemeOptions = RavenThemeCatalog.All
            .Select(theme => new ThemeOptionViewModel(theme, SelectTheme))
            .ToArray();
        selectedTheme = ThemeOptions.Single(
            option => option.Definition.Key == currentTheme.Key);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<NavigationItemViewModel> NavigationItems { get; }

    public IReadOnlyList<ThemeOptionViewModel> ThemeOptions { get; }

    public GuidesViewModel Guides { get; }

    public CommanderProfileViewModel FrontierProfile { get; }

    public GlobalInputSettingsViewModel InputSettings { get; }

    public DesktopBehaviorViewModel DesktopBehavior { get; }

    public BiologyRewardSettingsViewModel BiologyRewards { get; }

    public OverlayLayoutSettingsViewModel OverlayLayout { get; }

    public OverlayScaleSettingsViewModel OverlayScale { get; }

    public OverlayBehaviorViewModel OverlayBehavior { get; }

    public OverlayInteractionViewModel OverlayInteraction { get; }

    public InputBindingViewModel OverlayInteractionBinding { get; }

    public OverlayThemeSettingsViewModel OverlayTheme { get; }

    public ScreenshotProcessingViewModel ScreenshotProcessing { get; }

    public DockToDockViewModel DockToDock { get; }

    public NotificationViewModel Notifications { get; }

    public PulseOverlayViewModel PulseOverlay { get; }

    public StreamOverlayViewModel StreamOverlay { get; }

    public VrOverlayViewModel VrOverlay { get; }

    public GalaxyMapOverlayViewModel GalaxyMap { get; }

    public NetworkPrivacyViewModel NetworkPrivacy { get; }

    public InaraSettingsViewModel Inara { get; }

    public QuestWorkspaceViewModel QuestWorkspace { get; }

    public QuestIndicatorViewModel QuestIndicator { get; }

    public CommanderInstancesViewModel CommanderInstances { get; }

    public bool IsSharedCargoSuppressed =>
        CommanderInstances.HasMultipleGameWindows;

    internal CargoSnapshot? CurrentCargo => latestCargo;

    internal bool IsWaitingForFreshCargoSnapshot => awaitFreshCargoSnapshot;

    public CommanderPreferenceViewModel CommanderPreference { get; }

    public VisitedStarsCacheViewModel VisitedStarsCache { get; }

    public IReadOnlyList<QuestRuntimeSnapshot> Quests =>
        questRuntimeCoordinator.Snapshot;

    public int QuestUnreadMessageCount => Quests.Sum(
        quest => quest.UnreadMessageCount);

    public string QuestStatusMessage
    {
        get => questStatusMessage;
        private set => SetField(ref questStatusMessage, value);
    }

    public AppDataPaths AppDataPaths { get; }

    public Task PendingSystemBodyDataLoad { get; private set; } =
        Task.CompletedTask;

    public GroundTargetViewModel GroundTarget { get; }

    public SystemNotesViewModel SystemNotes { get; }

    public JourneyWorkspaceViewModel Journey { get; }

    public RouteWorkspaceViewModel Route { get; }

    public RouteManagerViewModel RouteManager { get; }

    public RouteWorkspaceViewModel FleetCarrierRoute { get; }

    public RouteManagerViewModel FleetCarrierRouteManager { get; }

    public JumpInfoViewModel JumpInfo { get; }

    public StationInfoViewModel StationInfo { get; }

    public HumanSiteViewModel HumanSite { get; }

    public SystemSurveyViewModel SystemSurvey { get; }

    public SurfaceSurveyViewModel SurfaceSurvey { get; }

    public CombatViewModel Combat { get; }

    public BiologyPredictionsViewModel BiologyPredictions { get; }

    public BiologyCodexViewModel BiologyCodex { get; }

    public CodexImageSettingsViewModel CodexImages { get; }

    public BiologyCodexBingoViewModel CodexBingo { get; }

    public SphereLimitViewModel Search { get; }

    public BoxelSearchViewModel BoxelSearch { get; }

    public NearestSystemsViewModel NearestSystems { get; }

    public GuardianViewModel Guardian { get; }

    public RamTahViewModel RamTah { get; }

    public ColonizationViewModel Colonization { get; }

    public SystemNicknameViewModel SystemNicknames { get; }

    public DiagnosticsLogViewModel DiagnosticsLog { get; }

    public string ReferenceDataStatus { get; }

    public ReferenceDataUpdateViewModel ReferenceDataUpdates { get; }

    public LocalizationViewModel Localization { get; }

    public ReleaseUpdateViewModel ReleaseUpdates { get; }

    public JournalInspectorViewModel JournalInspector { get; }

    public JournalSettingsViewModel JournalSettings { get; }

    public JournalPostProcessorViewModel JournalPostProcessor { get; }

    public IReadOnlyList<LegacyProfileOptionViewModel> LegacyProfiles { get; }

    public string? TargetFrontierId { get; }

    public string ProfileDataDirectory => AppDataPaths.DataDirectory;

    public string ProfileBackupDirectory { get; }

    public ICommand ImportLegacyProfileCommand { get; }

    public event Func<Task>? ProfileImportPreparing;

    public event Func<Task>? ProfileImportCompleted;

    public LegacyProfileOptionViewModel? SelectedLegacyProfile
    {
        get => selectedLegacyProfile;
        set
        {
            if (SetField(ref selectedLegacyProfile, value))
            {
                if (value is not null)
                {
                    LegacyProfileSourcePath = value.Path;
                }

                importLegacyProfileCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LegacyProfileSourcePath
    {
        get => legacyProfileSourcePath;
        set
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (!SetField(ref legacyProfileSourcePath, normalized))
            {
                return;
            }

            if (!HasCompletedLegacyImport && !IsImportingProfile)
            {
                ProfileStatusMessage = string.IsNullOrWhiteSpace(normalized)
                    ? "Choose the original SrvSurvey profile folder to import."
                    : (Directory.Exists(normalized)) switch
                    {
                        true => "The selected legacy profile is ready for verified import.",
                        false => "The selected legacy profile folder does not exist or is unavailable."
                    };
            }

            importLegacyProfileCommand.RaiseCanExecuteChanged();
        }
    }

    public string ProfileStatusMessage
    {
        get => profileStatusMessage;
        private set => SetField(ref profileStatusMessage, value);
    }

    public string SettingsLinkStatusMessage
    {
        get => settingsLinkStatusMessage;
        private set => SetField(ref settingsLinkStatusMessage, value);
    }

    public void ReportSettingsLinkResult(
        string description,
        bool launched,
        string? error = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        SettingsLinkStatusMessage = launched
            ? $"Opened {description} in the default browser."
            : $"Could not open {description}: "
                + (string.IsNullOrWhiteSpace(error)
                    ? "the desktop launcher declined the request."
                    : error);
    }

    public string ImportProfileButtonText => IsImportingProfile
        ? "Importing profile..."
        : (HasCompletedLegacyImport) switch
        {
            true => "Legacy profile imported",
            false => "Back up, verify, and import"
        };

    public bool HasCompletedLegacyImport => File.Exists(
        Path.Combine(
            AppDataPaths.DataDirectory,
            LegacyProfileImporter.ManifestFileName));

    public bool IsImportingProfile
    {
        get => isImportingProfile;
        private set
        {
            if (SetField(ref isImportingProfile, value))
            {
                importLegacyProfileCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ImportProfileButtonText));
            }
        }
    }

    public string JournalFolderPath { get; }

    public string? CurrentJournalPath => journalMonitor?.CurrentJournalPath;

    public string CandidatePaths { get; }

    public ICommand RefreshCommand { get; }

    public ICommand ShowProfileCommand { get; }

    public NavigationItemViewModel? SelectedNavigation
    {
        get => selectedNavigation;
        set
        {
            if (!SetField(ref selectedNavigation, value))
            {
                return;
            }

            if (value is not null)
            {
                isProfileSelected = false;
            }

            RaiseNavigationSelectionChanged();
        }
    }

    public bool IsProfileSelected => isProfileSelected;

    public bool IsOverviewSelected => SelectedNavigation?.Key == "overview"
        && !IsProfileSelected;

    public bool IsExplorationSelected => SelectedNavigation?.Key == "exploration"
        && !IsProfileSelected;

    public bool IsExobiologySelected => SelectedNavigation?.Key == "exobiology"
        && !IsProfileSelected;

    public bool IsTravelSelected => SelectedNavigation?.Key == "travel"
        && !IsProfileSelected;

    public bool IsSearchSelected => SelectedNavigation?.Key == "search"
        && !IsProfileSelected;

    public bool IsGuardianSelected => SelectedNavigation?.Key == "guardian"
        && !IsProfileSelected;

    public bool IsQuestsSelected => SelectedNavigation?.Key == "quests"
        && !IsProfileSelected;

    public bool IsColonizationSelected =>
        SelectedNavigation?.Key == "colonisation" && !IsProfileSelected;

    public bool IsDiagnosticsSelected => SelectedNavigation?.Key == "diagnostics"
        && !IsProfileSelected;

    public bool IsSettingsSelected => SelectedNavigation?.Key == "settings"
        && !IsProfileSelected;

    public bool IsGuidesSelected => SelectedNavigation?.Key == "guides"
        && !IsProfileSelected;

    public async Task ShowProfileAsync()
    {
        if (!isProfileSelected)
        {
            isProfileSelected = true;
            selectedNavigation = null;
            OnPropertyChanged(nameof(SelectedNavigation));
            RaiseNavigationSelectionChanged();
        }

        await FrontierProfile.OpenAsync(CancellationToken.None);
    }

    private void RaiseNavigationSelectionChanged()
    {
        OnPropertyChanged(nameof(IsProfileSelected));
        OnPropertyChanged(nameof(IsOverviewSelected));
        OnPropertyChanged(nameof(IsExplorationSelected));
        OnPropertyChanged(nameof(IsExobiologySelected));
        OnPropertyChanged(nameof(IsTravelSelected));
        OnPropertyChanged(nameof(IsSearchSelected));
        OnPropertyChanged(nameof(IsGuardianSelected));
        OnPropertyChanged(nameof(IsQuestsSelected));
        OnPropertyChanged(nameof(IsColonizationSelected));
        OnPropertyChanged(nameof(IsDiagnosticsSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(IsGuidesSelected));
    }

    public void ShowDiagnostics()
    {
        SelectedNavigation = NavigationItems.Single(
            item => item.Key == "diagnostics");
    }

    public void ShowSettings()
    {
        SelectedNavigation = NavigationItems.Single(
            item => item.Key == "settings");
    }

    public bool BeginVrAdjustment()
    {
        SelectedNavigation = NavigationItems.Single(
            item => item.Key == "settings");
        return VrOverlay.BeginAdjustment();
    }

    public string? CurrentVrOverlayMode
    {
        get
        {
            var status = latestStatus;
            if (status is null)
            {
                return journalState.ShipType;
            }

            return status.GuiFocus switch
            {
                GuiFocus.GalaxyMap => "GalaxyMap",
                GuiFocus.SystemMap => "SystemMap",
                GuiFocus.Orrery => "Orrery",
                GuiFocus.Fss => "FSS",
                GuiFocus.Saa => "SAA",
                _ when status.OnFoot => "OnFoot",
                _ when status.InFighter => "fighter",
                _ when status.InSrv => journalState.ActiveSrvType
                    ?? "testbuggy",
                _ => journalState.ShipType,
            };
        }
    }

    public void ShowQuests()
    {
        SelectedNavigation = NavigationItems.Single(
            item => item.Key == "quests");
    }

    public async Task OpenCodexBingoNearestSearchAsync(
        CodexBingoNearestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SelectedNavigation = NavigationItems.Single(item => item.Key == "search");
        if (request.Mode == CodexBingoNearestMode.Signal
            && !string.IsNullOrWhiteSpace(request.Signal))
        {
            await NearestSystems.SearchCodexSignalAsync(request.Signal);
            return;
        }

        if (request.Mode == CodexBingoNearestMode.MissingVariants
            && !string.IsNullOrWhiteSpace(request.Genus)
            && !string.IsNullOrWhiteSpace(request.Species))
        {
            await NearestSystems.SearchCodexVariantsAsync(
                request.Genus,
                request.Species,
                request.Variants);
        }
    }

    public string SelectedThemeName => selectedTheme.DisplayName;

    public string ThemeStatusMessage
    {
        get => themeStatusMessage;
        private set => SetField(ref themeStatusMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                ((AsyncCommand)RefreshCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(RefreshButtonText));
            }
        }
    }

    public string RefreshButtonText => IsBusy ? "Refreshing…" : "Refresh";

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetField(ref statusMessage, value);
    }

    public string CommanderName
    {
        get => commanderName;
        private set => SetField(ref commanderName, value);
    }

    public string FrontierId
    {
        get => frontierId;
        private set => SetField(ref frontierId, value);
    }

    public string GameDescription
    {
        get => gameDescription;
        private set => SetField(ref gameDescription, value);
    }

    public string GameMode
    {
        get => gameMode;
        private set => SetField(ref gameMode, value);
    }

    public string SystemDescription
    {
        get => systemDescription;
        private set => SetField(ref systemDescription, value);
    }

    public string BodyName
    {
        get => bodyName;
        private set => SetField(ref bodyName, value);
    }

    public string SessionState
    {
        get => sessionState;
        private set => SetField(ref sessionState, value);
    }

    public string LastUpdated
    {
        get => lastUpdated;
        private set => SetField(ref lastUpdated, value);
    }

    public string VehicleState
    {
        get => vehicleState;
        private set => SetField(ref vehicleState, value);
    }

    public string SurfacePosition
    {
        get => surfacePosition;
        private set => SetField(ref surfacePosition, value);
    }

    public string HeadingAndAltitude
    {
        get => headingAndAltitude;
        private set => SetField(ref headingAndAltitude, value);
    }

    public string GameUiFocus
    {
        get => gameUiFocus;
        private set => SetField(ref gameUiFocus, value);
    }

    public string EstimatedExplorationValue
    {
        get => estimatedExplorationValue;
        private set => SetField(ref estimatedExplorationValue, value);
    }

    public string ExplorationJumps
    {
        get => explorationJumps;
        private set => SetField(ref explorationJumps, value);
    }

    public string ExplorationDistance
    {
        get => explorationDistance;
        private set => SetField(ref explorationDistance, value);
    }

    public string ExplorationBodies
    {
        get => explorationBodies;
        private set => SetField(ref explorationBodies, value);
    }

    public string ExplorationStatusMessage
    {
        get => explorationStatusMessage;
        private set => SetField(ref explorationStatusMessage, value);
    }

    public ICommand ResetExplorationCommand { get; }

    public ICommand CancelResetExplorationCommand { get; }

    public bool IsResetExplorationPending
    {
        get => isResetExplorationPending;
        private set
        {
            if (SetField(ref isResetExplorationPending, value))
            {
                OnPropertyChanged(nameof(ResetExplorationButtonText));
                cancelResetExplorationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ResetExplorationButtonText => IsResetExplorationPending
        ? "Confirm reset"
        : "Reset totals";

    public string UnclaimedBioRewards
    {
        get => unclaimedBioRewards;
        private set => SetField(ref unclaimedBioRewards, value);
    }

    public string UnclaimedBioScans
    {
        get => unclaimedBioScans;
        private set => SetField(ref unclaimedBioScans, value);
    }

    public string OrganicScanProgress
    {
        get => organicScanProgress;
        private set => SetField(ref organicScanProgress, value);
    }

    public string ActiveOrganicSpecies
    {
        get => activeOrganicSpecies;
        private set => SetField(ref activeOrganicSpecies, value);
    }

    public string OrganicSampleRange
    {
        get => organicSampleRange;
        private set => SetField(ref organicSampleRange, value);
    }

    public string BioFirstFootfall
    {
        get => bioFirstFootfall;
        private set => SetField(ref bioFirstFootfall, value);
    }

    /// <summary>
    /// Current-body first-footfall state for the Exobiology workspace checkbox
    /// (legacy Main <c>checkFirstFootFall</c>).
    /// </summary>
    public bool IsCurrentBodyFirstFootfall
    {
        get => isCurrentBodyFirstFootfall;
        private set => SetField(ref isCurrentBodyFirstFootfall, value);
    }

    public bool CanToggleCurrentBodyFirstFootfall
    {
        get => canToggleCurrentBodyFirstFootfall;
        private set
        {
            if (SetField(ref canToggleCurrentBodyFirstFootfall, value))
            {
                toggleFirstFootfallCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsOrganicSample1Complete
    {
        get => isOrganicSample1Complete;
        private set => SetField(ref isOrganicSample1Complete, value);
    }

    public bool IsOrganicSample2Complete
    {
        get => isOrganicSample2Complete;
        private set => SetField(ref isOrganicSample2Complete, value);
    }

    public bool HasActiveOrganicSample => IsOrganicSample1Complete;

    public string ExobiologyStatusMessage
    {
        get => exobiologyStatusMessage;
        private set => SetField(ref exobiologyStatusMessage, value);
    }

    public string CommanderCodexStatusMessage
    {
        get => commanderCodexStatusMessage;
        private set => SetField(ref commanderCodexStatusMessage, value);
    }

    public ICommand ResetExobiologyCommand { get; }

    public ICommand CancelResetExobiologyCommand { get; }

    public ICommand ClearSurfaceTrackersCommand { get; }

    public ICommand ToggleFirstFootfallCommand { get; }

    public bool IsResetExobiologyPending
    {
        get => isResetExobiologyPending;
        private set
        {
            if (SetField(ref isResetExobiologyPending, value))
            {
                OnPropertyChanged(nameof(ResetExobiologyButtonText));
                cancelResetExobiologyCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ResetExobiologyButtonText => IsResetExobiologyPending
        ? "Confirm clear"
        : "Clear unclaimed";

    public async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        await Task.WhenAll(
            CommanderPreference.RefreshAsync(),
            CommanderInstances.RefreshAsync(),
            VisitedStarsCache.RefreshAsync(),
            JournalPostProcessor.RefreshCommandersAsync());
        if (journalMonitor is null)
        {
            StatusMessage = $"Journal folder not found. Set "
                + $"{JournalFolderLocator.EnvironmentVariableName} or use "
                + "--journal-directory <path>.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Reading journal and status updates…";

            var update = await journalMonitor.PollAsync(
                CancellationToken.None);
            await ApplyMonitorUpdateAsync(update, isManualRefresh: true);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException)
        {
            StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
            LastUpdated = $"Last refresh: {DateTimeOffset.Now:G}";
        }
    }

    public async Task MonitorAsync(
        TimeSpan? pollingInterval = null,
        CancellationToken cancellationToken = default)
    {
        if (journalMonitor is null)
        {
            return;
        }

        var interval = pollingInterval ?? TimeSpan.FromMilliseconds(250);
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var update = await journalMonitor.PollAsync(cancellationToken);
                await ApplyMonitorUpdateAsync(update, isManualRefresh: false);
                await Task.Delay(interval, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal desktop shutdown.
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException)
        {
            StatusMessage = "Live journal monitoring stopped: " + exception.Message;
        }
    }

    public void SetJournalCommandPlatformServices(
        Func<DirectoryInfo, Task<bool>>? launchDirectory,
        Func<Task>? requestShutdown,
        Func<string, Task>? writeClipboard)
    {
        journalCommandDirectoryLauncher = launchDirectory;
        journalCommandShutdownRequester = requestShutdown;
        journalCommandClipboardWriter = writeClipboard;
    }

    public async Task ImportLegacyProfileAsync()
    {
        if (!CanImportLegacyProfile())
        {
            return;
        }

        try
        {
            IsImportingProfile = true;
            ProfileStatusMessage =
                "Creating verified backups of the legacy and current profiles...";
            await PrepareForProfileImportAsync();
            var result = await profileImporter.ImportAsync(
                LegacyProfileSourcePath,
                AppDataPaths.DataDirectory,
                ProfileBackupDirectory,
                CancellationToken.None);
            var settingsMigration = new LegacyUiSettingsMigrator()
                .MigrateIfNeeded(AppDataPaths);
            var organicMigration = await new LegacyOrganicProfileMigrator(
                    AppDataPaths.DataDirectory)
                .MigrateAsync(CancellationToken.None);
            foreach (var error in organicMigration.Errors)
            {
                applicationLogService?.Append(
                    "Legacy organic history was preserved without conversion: "
                        + error);
            }
            var retainedFiles = result.Manifest.PreviousDestinationEntries.Count
                - result.Manifest.Conflicts.Count;
            var importedBytes = result.Manifest.Entries.Sum(entry => entry.Length);
            ProfileStatusMessage = $"Imported {result.Manifest.Entries.Count:N0} legacy files, "
                + $"checksum-verified {importedBytes:N0} bytes, "
                + $"retained {retainedFiles:N0} current-only files, and recorded "
                + $"{result.Manifest.Conflicts.Count:N0} path collisions. "
                + GetSettingsMigrationStatus(settingsMigration)
                + " "
                + GetOrganicMigrationStatus(organicMigration)
                + $"Verified backups: {result.BackupDirectory}";
            OnPropertyChanged(nameof(HasCompletedLegacyImport));
            OnPropertyChanged(nameof(ImportProfileButtonText));
            await CompleteProfileImportAsync();
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException
                or InvalidOperationException)
        {
            ProfileStatusMessage = $"Profile import failed without changing the legacy data: "
                + exception.Message;
        }
        finally
        {
            IsImportingProfile = false;
            importLegacyProfileCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task PrepareForProfileImportAsync()
    {
        if (ProfileImportPreparing is not { } handlers)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Func<Task>>())
        {
            await handler();
        }
    }

    private async Task CompleteProfileImportAsync()
    {
        if (ProfileImportCompleted is not { } handlers)
        {
            ProfileStatusMessage +=
                " Restart SrvSurvey to load the migrated profile.";
            return;
        }

        ProfileStatusMessage +=
            " Verification complete; restarting SrvSurvey with the migrated profile...";
        try
        {
            foreach (var handler in handlers.GetInvocationList().Cast<Func<Task>>())
            {
                await handler();
            }
        }
        catch (Exception exception)
        {
            ProfileStatusMessage += " Automatic restart failed: "
                + exception.Message
                + " Close and reopen SrvSurvey manually; the verified import is safe.";
        }
    }

    private static string GetSettingsMigrationStatus(
        LegacyUiSettingsMigrationResult migration)
    {
        if (migration.Error is not null)
        {
            return "Player data is byte-verified, but legacy UI preferences could "
                + "not be translated; the current Avalonia settings were left "
                + $"unchanged. {migration.Error}";
        }

        return migration.Migrated
            ? $"Translated {migration.MappedPreferenceCount:N0} legacy UI preferences."
            : "No legacy UI preference translation was required.";
    }

    private static string GetOrganicMigrationStatus(
        LegacyOrganicProfileMigrationResult migration)
    {
        var status = migration.Migrated
            ? "Converted retired organic history without changing its source: "
                + $"{migration.MigratedProfileCount:N0} profile(s), "
                + $"{migration.MigratedBodyCount:N0} body file(s), "
                + $"{migration.MigratedScanCount:N0} scan(s), and "
                + $"{migration.MigratedOrganismCount:N0} organism(s). "
            : "No retired organic-history conversion was required. ";
        if (migration.Errors.Count > 0)
        {
            status += $"Preserved {migration.Errors.Count:N0} unconverted "
                + "organic-history file(s); see Diagnostics for details. ";
        }

        return status;
    }

    private bool CanImportLegacyProfile()
    {
        return !IsImportingProfile
            && Directory.Exists(LegacyProfileSourcePath)
            && !HasCompletedLegacyImport
            && !File.Exists(AppDataPaths.DataDirectory);
    }

    private string GetInitialProfileStatus()
    {
        if (HasCompletedLegacyImport)
        {
            return $"Legacy profile data has already been imported into "
                + $"{AppDataPaths.DataDirectory}. The verified backup and conflict "
                + "manifest are retained for recovery.";
        }

        if (File.Exists(AppDataPaths.DataDirectory))
        {
            return $"The cross-platform profile path is occupied by a file and cannot "
                + $"be imported: {AppDataPaths.DataDirectory}";
        }

        return LegacyProfiles.Count == 0
            ? "No legacy Windows profile was detected automatically. Choose its profile "
                + "folder manually; copied Windows profiles can also be imported on Linux."
            : $"Found {LegacyProfiles.Count:N0} legacy profile source(s). "
                + "Import creates checksum-verified backups, preserves current-only files, "
                + "records collisions, and activates the merged copy transactionally.";
    }

    private void SelectTheme(ThemeOptionViewModel option)
    {
        try
        {
            themeService?.Select(option.Definition.Key);
            selectedTheme = option;
            ThemeStatusMessage = string.Empty;
            OnPropertyChanged(nameof(SelectedThemeName));
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidOperationException)
        {
            ThemeStatusMessage = $"The theme changed for this session but could not be saved: "
                + exception.Message;
        }
    }

    private void ApplySnapshot(JournalSnapshot snapshot)
    {
        CommanderInstances.UpdateCurrent(
            snapshot.FrontierId,
            snapshot.CommanderName);
        VisitedStarsCache.UpdateContext(
            snapshot.FrontierId,
            snapshot.CommanderName,
            snapshot.SystemName);
        CommanderName = Display(snapshot.CommanderName);
        FrontierId = Display(snapshot.FrontierId);
        GameDescription = string.Join(
            " ",
            new[]
            {
                snapshot.GameVersion,
                snapshot.GameBuild is null ? null : $"({snapshot.GameBuild})",
                snapshot.IsOdyssey switch
                {
                    true => "Odyssey",
                    false => "Horizons",
                    null => null,
                },
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (string.IsNullOrWhiteSpace(GameDescription))
        {
            GameDescription = Unavailable;
        }

        GameMode = Display(snapshot.GameMode);
        SystemDescription = snapshot.SystemAddress is null
            ? Display(snapshot.SystemName)
            : $"{Display(snapshot.SystemName)} ({snapshot.SystemAddress})";
        BodyName = Display(snapshot.BodyName);
        SessionState = snapshot.IsShutdown ? "Session closed" : "Session active";

        var malformedSuffix = snapshot.MalformedLineCount == 0
            ? string.Empty
            : $"; ignored {snapshot.MalformedLineCount} malformed/partial line(s)";
        StatusMessage = $"Loaded {snapshot.ValidLineCount} events from "
            + $"{Path.GetFileName(snapshot.SourcePath)}; "
            + $"{snapshot.RecognizedEventCount} bootstrap events recognized"
            + malformedSuffix
            + ".";
    }

    private async Task ApplyMonitorUpdateAsync(
        JournalMonitorUpdate update,
        bool isManualRefresh)
    {
        if (!update.HasChanges && !isManualRefresh)
        {
            await ApplyIdleHousekeepingAsync(update);
            return;
        }

        if (update.IsBootstrapRead || update.Status is not null)
        {
            latestStatus = update.Status;
        }

        JournalInspector.ApplyUpdate(update.JournalEvents, update.Status);

        var previousFrontierId = journalState.FrontierId;
        var previousCommanderName = journalState.CommanderName;
        foreach (var journalEvent in update.JournalEvents)
        {
            journalState.Apply(journalEvent);
        }

        Colonization.UpdateMusicTrack(journalState.MusicTrack);
        StationInfo.UpdateMusicTrack(journalState.MusicTrack);
        GroundTarget.UpdateMusicTrack(journalState.MusicTrack);

        var commanderChanged = !string.Equals(
                previousFrontierId,
                journalState.FrontierId,
                StringComparison.OrdinalIgnoreCase)
            || !string.Equals(
                previousCommanderName,
                journalState.CommanderName,
                StringComparison.OrdinalIgnoreCase);
        if (commanderChanged)
        {
            awaitFreshCargoSnapshot = true;
            companionIdentityChangedAt = update.JournalEvents
                .Where(journalEvent => journalEvent.EventName is "Commander" or "LoadGame")
                .Select(journalEvent => journalEvent.Timestamp)
                .LastOrDefault(timestamp => timestamp is not null)
                ?? journalState.LastEventTimestamp;
            cargoInventoryState.Reset(null);
            latestCargo = null;
            latestShipLocker = null;
            await FrontierProfile.SetCommanderContextAsync(
                journalState.FrontierId,
                journalState.CommanderName,
                refreshIfOpen: IsProfileSelected,
                CancellationToken.None);
        }

        var allowSharedCargo = !IsSharedCargoSuppressed;
        var cargoChanged = false;
        if (!allowSharedCargo)
        {
            cargoChanged = cargoInventoryState.Reset(null);
            latestCargo = null;
            latestShipLocker = null;
        }
        else if (awaitFreshCargoSnapshot)
        {
            if (update.Cargo is not null
                && IsCurrentCommanderCompanionSnapshot(update.Cargo.Timestamp))
            {
                cargoChanged = cargoInventoryState.Reset(update.Cargo);
                awaitFreshCargoSnapshot = false;
                latestCargo = cargoInventoryState.CreateSnapshot();
            }
        }
        else
        {
            foreach (var journalEvent in update.JournalEvents)
            {
                // Squadron linked FCs freeze the true before-state before CargoTransfer mutates
                // live inventory so the later GetDiff cannot collapse to a zero delta.
                if (string.Equals(
                        journalEvent.EventName,
                        "CargoTransfer",
                        StringComparison.Ordinal))
                {
                    Colonization.PrepareSquadronCargoTransferSnapshot(
                        cargoInventoryState);
                }

                cargoChanged |= cargoInventoryState.Apply(
                    journalEvent,
                    latestStatus?.InSrv == true);
            }

            if (update.Cargo is not null
                && IsCurrentCommanderCompanionSnapshot(update.Cargo.Timestamp))
            {
                cargoChanged |= cargoInventoryState.Reset(update.Cargo);
            }

            if (cargoChanged || latestCargo is null)
            {
                latestCargo = cargoInventoryState.CreateSnapshot();
            }
        }

        if (allowSharedCargo
            && update.ShipLocker is not null
            && IsCurrentCommanderCompanionSnapshot(update.ShipLocker.Timestamp))
        {
            latestShipLocker = update.ShipLocker;
        }

        FrontierProfile.UpdateLocalInventory(
            latestCargo,
            latestShipLocker,
            isSuppressed: !allowSharedCargo);
        DockToDock.ApplyUpdate(
            update.JournalEvents,
            latestCargo,
            update.IsBootstrapRead);
        DesktopBehavior.ApplyJournalEvents(
            update.JournalEvents,
            update.IsBootstrapRead);

        if (update.Status is not null)
        {
            exobiologyState.UpdateStatus(update.Status);
            GroundTarget.UpdateStatus(update.Status);
            Colonization.UpdateStatus(update.Status);
        }
        await GroundTarget.ApplyJournalEventsAsync(
            update.JournalEvents,
            allowCommands: !update.IsBootstrapRead);

        var scansLostToDeath = new HashSet<string>(StringComparer.Ordinal);
        var greenGasGiantResult =
            await greenGasGiantPublicationCoordinator.ApplyAsync(
                update.JournalEvents,
                NetworkPrivacy.UploadGreenGasGiantCandidates,
                allowPublishing: !update.IsBootstrapRead,
                CancellationToken.None);
        NetworkPrivacy.ReportPublicationResult(greenGasGiantResult);
        if (!update.IsBootstrapRead)
        {
            Notifications.ReportGreenGasGiantUploads(greenGasGiantResult);
        }

        foreach (var warning in greenGasGiantResult.Warnings)
        {
            applicationLogService?.Append(warning);
        }
        FrontierProfile.UpdateJournalReputation(
            journalState.CommanderName,
            update.JournalEvents);
        FrontierProfile.UpdateJournalCommunityGoals(
            journalState.CommanderName,
            update.JournalEvents);
        OverlayBehavior.UpdateContext(
            journalState.CurrentSuit,
            latestStatus?.OnFoot == true);
        OverlayBehavior.UpdateSessionContext(
            latestStatus is not null,
            !string.IsNullOrWhiteSpace(journalState.CommanderName),
            journalState.IsShutdown,
            journalState.IsAtMainMenu,
            journalState.IsAtCarrierManagement);
        JournalPostProcessor.SelectCommander(journalState.FrontierId);

        var commanderCodexResult =
            await commanderCodexJournalTracker.ApplyAsync(
                update.JournalEvents,
                CancellationToken.None);
        if (commanderCodexResult.Warnings.Count > 0)
        {
            CommanderCodexStatusMessage = string.Join(
                Environment.NewLine,
                commanderCodexResult.Warnings);
        }
        else if (commanderCodexResult.DiscoveryEventCount > 0)
        {
            CommanderCodexStatusMessage = commanderCodexResult.HasChanges
                ? $"Recorded {commanderCodexResult.ChangedEntryCount:N0} "
                    + "Commander Codex ledger entries across "
                    + $"{commanderCodexResult.ChangedFileCount:N0} files."
                : "Commander Codex is current; no earlier firsts were found.";
        }

        Colonization.ApplyJournalEvents(update.JournalEvents);
        Colonization.UpdateSystemContext(
            journalState.SystemName,
            journalState.StarPosition,
            journalState.SystemAddress);

        Search.UpdateCurrentSystem(
            journalState.SystemName,
            journalState.StarPosition);
        NearestSystems.UpdateContext(
            journalState.SystemName,
            journalState.StarPosition,
            journalState.CommanderName);
        await CodexBingo.UpdateContextAsync(
            journalState.FrontierId,
            journalState.CommanderName,
            journalState.SystemName,
            journalState.StarPosition,
            forceRefresh: commanderCodexResult.DiscoveryEventCount > 0);
        SystemNotes.UpdateContext(
            journalState.FrontierId,
            journalState.CommanderName,
            journalState.SystemName,
            journalState.SystemAddress,
            journalState.StarPosition);
        BoxelSearch.UpdateCurrentSystem(
            journalState.SystemName,
            journalState.StarPosition);
        Guardian.UpdateCurrentSystem(
            journalState.SystemName,
            journalState.StarPosition);
        HumanSite.UpdateContext(
            journalState.FrontierId,
            journalState.CommanderName,
            journalState.SystemName,
            journalState.SystemAddress ?? 0,
            journalState.StarPosition);
        _ = StationInfo.UpdateCurrentSystemAsync(
            journalState.SystemName,
            journalState.SystemAddress ?? 0);

        var loadedExistingProfile = await EnsureCommanderProfileAsync();
        await ApplyQuestUpdateAsync(update, allowSharedCargo);
        await Colonization.SetCommanderAsync(journalState.CommanderName);
        var cargoActivity = allowSharedCargo
            && (cargoChanged
                || update.Cargo is not null
                || update.JournalEvents.Any(journalEvent =>
                    journalEvent.EventName is "Cargo"
                        or "CargoTransfer"
                        or "MarketBuy"
                        or "MarketSell"));
        var isCurrentCargoInventoryAvailable =
            !awaitFreshCargoSnapshot
            || update.Cargo is not null;
        await Colonization.SynchronizeLiveProjectsAsync(
            update.JournalEvents,
            allowPublishing: !update.IsBootstrapRead,
            cargoInventory: allowSharedCargo
                ? cargoInventoryState
                : null,
            preferShipCargoDiffForSquadron: isCurrentCargoInventoryAvailable,
            cargoActivity: cargoActivity);
        var initializedJourney = await Journey.UpdateContextAsync(
            journalState.FrontierId,
            journalState.CommanderName,
            journalState.IsOdyssey ?? true,
            journalState.SystemName,
            journalState.SystemAddress);
        if (!initializedJourney)
        {
            await Journey.ApplyJournalEventsAsync(update.JournalEvents);
        }

        await Route.UpdateContextAsync(
            journalState.FrontierId,
            journalState.SystemName,
            journalState.SystemAddress,
            journalState.StarPosition);
        await RouteManager.UpdateContextAsync(journalState.FrontierId);
        await FleetCarrierRoute.UpdateContextAsync(
            journalState.FrontierId,
            journalState.SystemName,
            journalState.SystemAddress,
            journalState.StarPosition);
        await FleetCarrierRouteManager.UpdateContextAsync(
            journalState.FrontierId);
        if (update.IsBootstrapRead)
        {
            FleetCarrierRoute.ApplyFleetCarrierJumpEvents(
                update.JournalEvents);
        }
        else
        {
            await Route.ApplyJournalEventsAsync(update.JournalEvents);
            await FleetCarrierRoute.ApplyJournalEventsAsync(
                update.JournalEvents);
        }

        var explorationBefore = explorationState.CreateSnapshot();
        var exobiologyVersionBefore = exobiologyState.Version;
        var boxelBefore = BoxelSearch.CreateNotificationState();
        var skipPersistedBootstrapEvents = update.IsBootstrapRead
            && loadedExistingProfile;
        if (update.NavRoute is not null)
        {
            await BoxelSearch.UpdateRouteAsync(update.NavRoute);
        }

        await Search.UpdateNavigationAsync(
            update.NavRoute,
            update.Status,
            journalState.MusicTrack);

        if (!skipPersistedBootstrapEvents)
        {
            await BoxelSearch.ApplyJournalEventsAsync(update.JournalEvents);
        }

        Notifications.ApplyJournalEvents(
            update.JournalEvents,
            allowNotifications: !update.IsBootstrapRead);
        PulseOverlay.ApplyUpdate(
            update.JournalEvents,
            update.Status,
            update.IsBootstrapRead);
        Notifications.ReportBoxelUpdate(
            boxelBefore,
            BoxelSearch.CreateNotificationState(),
            update.JournalEvents.Any(journalEvent =>
                journalEvent.EventName == "FSSAllBodiesFound"),
            allowNotifications: !update.IsBootstrapRead);

        var guardianScreenshotContexts = await Guardian.ApplyJournalEventsAsync(
            update.JournalEvents,
            activeProfileCommanderName,
            allowLiveCommands: !update.IsBootstrapRead,
            status: latestStatus,
            cancellationToken: firstFootfallInferenceCancellation.Token);
        if (!allowSharedCargo)
        {
            Guardian.ClearCargo();
        }
        else if (cargoChanged && latestCargo is not null)
        {
            Guardian.UpdateCargo(latestCargo);
        }
        if (cargoChanged && latestCargo is not null)
        {
            await Colonization.UpdateCargoAsync(
                latestCargo,
                publishCurrentShipCargo: update.Cargo is not null);
        }
        await Colonization.UpdateMarketAsync(update.Market);
        SystemSurvey.SetActiveBuildProjects(Colonization.HasProjects);
        Combat.SetActiveBuildProjects(Colonization.HasProjects);
        Guardian.SetActiveBuildProjects(Colonization.HasProjects);
        HumanSite.SetActiveBuildProjects(Colonization.HasProjects);
        await Combat.ApplyUpdateAsync(
            update.JournalEvents,
            update.Status,
            processHistoricalProgress: !skipPersistedBootstrapEvents);

        if (update.Status is not null)
        {
            await Guardian.UpdateStatusAsync(
                update.Status,
                allowGesture: !update.IsBootstrapRead,
                cancellationToken: CancellationToken.None);
            StationInfo.UpdateStatus(update.Status);
        }

        if (latestStatus is not null)
        {
            await Route.UpdateStatusAsync(
                latestStatus,
                journalState.MusicTrack);
            await FleetCarrierRoute.UpdateStatusAsync(
                latestStatus,
                journalState.MusicTrack);
            await BoxelSearch.UpdateStatusAsync(
                latestStatus,
                allowAutoCopy: !Route.ShouldAutoCopyNextHop
                    && !FleetCarrierRoute.ShouldAutoCopyNextHop,
                nextMusicTrack: journalState.MusicTrack);
        }

        await HumanSite.ApplyUpdateAsync(
            update.JournalEvents,
            update.Status,
            journalState.ShipType,
            allowExternalData: !update.IsBootstrapRead);
        var requestShutdown = !update.IsBootstrapRead
            && await ApplyDesktopTextCommandsAsync(update.JournalEvents);

        if (!update.IsBootstrapRead)
        {
            var screenshotResult =
                await ScreenshotProcessing.ProcessJournalEventsAsync(
                update.JournalEvents,
                journalState.CommanderName,
                guardianScreenshotContexts,
                latestStatus is { } screenshotStatus
                    ? new ScreenshotNavigationContext(
                        DateTimeOffset.UtcNow,
                        screenshotStatus.Latitude,
                        screenshotStatus.Longitude,
                        screenshotStatus.NormalizedHeading,
                        screenshotStatus.HasLatitudeLongitude)
                    : null,
                CancellationToken.None);
            Notifications.ReportScreenshotResult(
                screenshotResult,
                ScreenshotProcessing.AddBanner);
        }

        JumpInfo.ApplyUpdate(
            journalState.SystemName,
            journalState.SystemAddress,
            journalState.StarPosition,
            update.NavRoute,
            update.JournalEvents,
            update.Status,
            Route.CreateSnapshot(),
            update.IsBootstrapRead);
        GalaxyMap.ApplyUpdate(
            journalState.SystemName,
            journalState.SystemAddress,
            update.NavRoute,
            update.JournalEvents,
            update.Status,
            update.IsBootstrapRead,
            journalState.MusicTrack);
        foreach (var journalEvent in update.JournalEvents)
        {
            if (!skipPersistedBootstrapEvents
                || journalEvent.EventName is "Fileheader" or "LoadGame")
            {
                explorationState.Apply(journalEvent);
            }

            if (!skipPersistedBootstrapEvents
                || IsExobiologyContextEvent(journalEvent.EventName))
            {
                if (journalEvent.EventName == "Died")
                {
                    scansLostToDeath.UnionWith(
                        exobiologyState.CreateSnapshot().ScannedBioEntryIds);
                }

                exobiologyState.Apply(journalEvent);
            }
        }

        var explorationAfter = explorationState.CreateSnapshot();
        if (explorationAfter != explorationBefore)
        {
            UpdateExplorationDisplay(explorationAfter);
            await SaveExplorationAsync(explorationAfter);
        }

        var exobiologyAfter = exobiologyState.CreateSnapshot();
        var exobiologyChanged =
            exobiologyState.Version != exobiologyVersionBefore;
        if (update.JournalEvents.Count > 0
            || update.Status is not null
            || exobiologyChanged
            || isManualRefresh)
        {
            SystemSurvey.ApplyUpdate(
                update.JournalEvents,
                update.Status,
                exobiologyAfter);
        }
        await LoadCurrentSystemHistoryAsync();
        PendingSystemBodyDataLoad = LoadCurrentSystemBodyDataAsync();
        if (!update.IsBootstrapRead
            && await ApplyFirstFootfallTextCommandsAsync(update.JournalEvents) > 0)
        {
            exobiologyAfter = exobiologyState.CreateSnapshot();
            SystemSurvey.ApplyUpdate([], null, exobiologyAfter);
        }

        if (await TryInferFirstFootfallAsync(update))
        {
            exobiologyAfter = exobiologyState.CreateSnapshot();
            SystemSurvey.ApplyUpdate([], null, exobiologyAfter);
        }

        exobiologyChanged =
            exobiologyState.Version != exobiologyVersionBefore;

        await PersistSystemScanAsync(update.JournalEvents);
        await RefreshSystemSurveyCommanderCodexAsync(
            forceRefresh: commanderCodexResult.DiscoveryEventCount > 0);
        if (!update.IsBootstrapRead
            && SystemSurvey.LatestBiologyEntryId is { } entryId
            && update.JournalEvents.Any(IsShowCodexCommand))
        {
            await BiologyCodex.OpenEntryAsync(entryId);
        }
        SurfaceSurveySessionContext? surfaceSession = null;
        if (!string.IsNullOrWhiteSpace(activeProfileFrontierId)
            && !string.IsNullOrWhiteSpace(journalState.SystemName)
            && journalState.SystemAddress is > 0)
        {
            var surfaceBody = SystemSurvey.Snapshot.CurrentBodyId is { } bodyId
                ? SystemSurvey.Snapshot.Bodies.FirstOrDefault(body =>
                    body.BodyId == bodyId)
                : null;
            surfaceBody ??= latestStatus?.BodyName is { Length: > 0 } statusBodyName
                ? SystemSurvey.Snapshot.Bodies.FirstOrDefault(body =>
                    string.Equals(
                        body.Name,
                        statusBodyName,
                        StringComparison.OrdinalIgnoreCase))
                : null;
            surfaceSession = new SurfaceSurveySessionContext(
                activeProfileFrontierId,
                activeProfileCommanderName ?? journalState.CommanderName,
                journalState.SystemName,
                journalState.SystemAddress.Value,
                journalState.StarPosition,
                surfaceBody?.BodyId,
                surfaceBody?.Name,
                latestStatus?.PlanetRadius is > 0
                    ? (double)latestStatus.PlanetRadius
                    : surfaceBody?.RadiusMeters ?? 0);
        }

        if (update.JournalEvents.Count > 0
            || update.Status is not null
            || exobiologyChanged
            || isManualRefresh)
        {
            await SurfaceSurvey.ApplyUpdateAsync(
                surfaceSession,
                update.JournalEvents,
                update.Status,
                exobiologyAfter,
                processJournalMutations: !skipPersistedBootstrapEvents,
                scansLostToDeath: scansLostToDeath.ToArray(),
                cancellationToken: CancellationToken.None);
        }

        if (exobiologyChanged)
        {
            await SaveExobiologyAsync(exobiologyAfter);
        }

        if (update.JournalEvents.Count > 0 || update.Status is not null)
        {
            UpdateExobiologyDisplay(exobiologyAfter);
        }

        if (update.JournalEvents.Count > 0)
        {
            ApplySnapshot(journalState.CreateSnapshot(update.JournalPath));
        }
        else if (isManualRefresh)
        {
            StatusMessage = update.JournalPath is null
                ? $"No Journal.*.log files were found in {JournalFolderPath}."
                : $"Monitoring {Path.GetFileName(update.JournalPath)}; no new events.";
        }

        if (update.Status is not null)
        {
            ApplyStatus(update.Status);
        }

        if (update.Errors.Count > 0)
        {
            StatusMessage = string.Join(Environment.NewLine, update.Errors);
        }

        if (update.JournalEvents.Count > 0
            || update.Status is not null
            || update.NavRoute is not null
            || update.Cargo is not null
            || update.ShipLocker is not null
            || update.Market is not null
            || update.Errors.Count > 0
            || isManualRefresh)
        {
            LastUpdated = $"Last update: {DateTimeOffset.Now:G}";
        }

        // External publication runs after every local reducer and persistence
        // path so an unavailable gateway cannot delay live state projection.
        await ApplyExternalPublicationAsync(update, allowSharedCargo);

        if (requestShutdown
            && journalCommandShutdownRequester is { } requestShutdownAsync)
        {
            await requestShutdownAsync();
        }
    }

    private async Task ApplyIdleHousekeepingAsync(JournalMonitorUpdate update)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - lastIdleHousekeepingAt < IdleHousekeepingInterval)
        {
            return;
        }

        lastIdleHousekeepingAt = now;
        await ApplyExternalPublicationAsync(
            update,
            allowSharedCargo: !IsSharedCargoSuppressed);
    }

    private async Task ApplyExternalPublicationAsync(
        JournalMonitorUpdate update,
        bool allowSharedCargo)
    {
        lastIdleHousekeepingAt = DateTimeOffset.UtcNow;
        var canShareCargo = allowSharedCargo;
        try
        {
            CommanderInstances.RefreshGameWindowCount();
            var hasMultipleGameWindows =
                CommanderInstances.HasMultipleGameWindows;
            canShareCargo &= !hasMultipleGameWindows;
            eddnPublisher.SetSuspended(hasMultipleGameWindows);
            var eddnResult = await eddnPublisher.ApplyAsync(
                update.JournalEvents,
                latestStatus,
                NetworkPrivacy.EddnUploadEnabled,
                NetworkPrivacy.EddnUseTestSchemas,
                allowPublishing: !update.IsBootstrapRead
                    && !hasMultipleGameWindows,
                journalDirectory: folderResolution.SelectedPath,
                journalPath: update.JournalPath,
                allowSharedData: !hasMultipleGameWindows,
                cancellationToken: CancellationToken.None);
            NetworkPrivacy.ReportPublicationResult(eddnResult);
            foreach (var warning in eddnResult.Warnings)
            {
                applicationLogService?.Append(warning);
            }
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException)
        {
            applicationLogService?.Append(
                "EDDN processing was isolated from journal tracking: "
                    + exception.Message);
        }

        try
        {
            var inaraResult = await inaraPublisher.ApplyAsync(
                new InaraPublicationUpdate(
                    update.JournalEvents,
                    latestStatus,
                    latestCargo,
                    update.JournalPath,
                    AllowPublishing: !update.IsBootstrapRead,
                    AllowSharedData: canShareCargo,
                    journalState.SystemName,
                    journalState.StationName,
                    journalState.BodyName,
                    journalState.ShipType,
                    journalState.ShipId,
                    journalState.ShipName,
                    journalState.ShipIdent,
                    new InaraPublicationOptions(
                        Inara.UploadEnabled,
                        Inara.DeveloperTestMode,
                        Inara.StoredApiKey,
                        activeProfileCommanderName
                            ?? journalState.CommanderName,
                        activeProfileFrontierId
                            ?? journalState.FrontierId,
                        journalState.GameVersion,
                        journalState.IsOdyssey ?? true)),
                CancellationToken.None);
            Inara.ReportPublicationResult(inaraResult);
            foreach (var warning in inaraResult.Warnings)
            {
                applicationLogService?.Append(warning);
            }
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException)
        {
            Inara.ReportPublicationFailure(exception);
            applicationLogService?.Append(
                "Inara processing was isolated from journal tracking: "
                    + exception.Message);
        }
    }

    private async Task RefreshSystemSurveyCommanderCodexAsync(
        bool forceRefresh)
    {
        var resolvedFrontierId = activeProfileFrontierId ?? journalState.FrontierId;
        var resolvedCommanderName = activeProfileCommanderName
            ?? journalState.CommanderName;
        var systemAddress = journalState.SystemAddress;
        var regionId = journalState.StarPosition is { } position
            ? GalacticRegionMap.Find(position)?.Id
            : null;
        if (string.IsNullOrWhiteSpace(resolvedFrontierId)
            || systemAddress is null)
        {
            surveyCodexFrontierId = null;
            surveyCodexRegionId = null;
            surveyCodexSystemAddress = null;
            SystemSurvey.UpdateCommanderCodexContext(null, null);
            return;
        }

        if (!forceRefresh
            && string.Equals(
                surveyCodexFrontierId,
                resolvedFrontierId,
                StringComparison.OrdinalIgnoreCase)
            && surveyCodexRegionId == regionId
            && surveyCodexSystemAddress == systemAddress)
        {
            return;
        }

        var global = await commanderCodexStore.LoadAsync(
            resolvedFrontierId,
            resolvedCommanderName,
            cancellationToken: CancellationToken.None);
        var regional = regionId is > 0
            ? await commanderCodexStore.LoadAsync(
                resolvedFrontierId,
                resolvedCommanderName,
                regionId.Value,
                CancellationToken.None)
            : null;
        surveyCodexFrontierId = resolvedFrontierId;
        surveyCodexRegionId = regionId;
        surveyCodexSystemAddress = systemAddress;
        SystemSurvey.UpdateCommanderCodexContext(
            global.Data,
            regional?.Data,
            regionId);

        var warnings = global.Warnings
            .Concat(regional?.Warnings ?? [])
            .ToArray();
        if (warnings.Length > 0)
        {
            CommanderCodexStatusMessage = string.Join(
                Environment.NewLine,
                warnings);
        }
    }

    private async Task ApplyQuestUpdateAsync(
        JournalMonitorUpdate update,
        bool allowCargoFile)
    {
        if (string.IsNullOrWhiteSpace(journalState.FrontierId)
            || string.IsNullOrWhiteSpace(journalState.CommanderName)
            || folderResolution.SelectedPath is null)
        {
            QuestStatusMessage = "Waiting for a commander journal session.";
            return;
        }

        try
        {
            var enabled = questSettingsStore.LoadEnabled();
            var previousQuestSnapshot = questRuntimeCoordinator.Snapshot;
            var result = await questRuntimeCoordinator.ApplyUpdateAsync(
                new QuestRuntimeConfiguration(
                    enabled,
                    journalState.FrontierId,
                    journalState.CommanderName,
                    activeProfileRavenApiKey,
                    latestStatus),
                folderResolution.SelectedPath,
                update.JournalEvents,
                update.IsBootstrapRead,
                allowCargoFile: allowCargoFile,
                cancellationToken: CancellationToken.None);
            QuestWorkspace.ApplyRuntimeResult(result, enabled);
            if (ReferenceEquals(previousQuestSnapshot, result.Quests))
            {
                // Status can move quest overlay markers without changing the
                // quest rows. Snapshot changes are handled by the coordinator
                // event and must not be projected a second time here.
                UpdateQuestOverlayPresentation(result.Quests, enabled);
            }
            if (!enabled)
            {
                QuestStatusMessage = "Quests are disabled.";
            }
            else if (result.Warnings.Count > 0)
            {
                QuestStatusMessage = string.Join(
                    Environment.NewLine,
                    result.Warnings);
            }
            else
            {
                QuestStatusMessage = result.Quests.Count == 0
                    ? "No active quests."
                    : $"{result.Quests.Count:N0} active quest(s); "
                        + $"{QuestUnreadMessageCount:N0} unread message(s).";
            }
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException
            or ArgumentException
            or HttpRequestException)
        {
            QuestStatusMessage = "Quest update failed without changing imported "
                + "source data: " + exception.Message;
            applicationLogService?.Append(QuestStatusMessage);
        }
    }

    private async Task<QuestRuntimeUpdateResult> ReplayQuestJournalEventAsync(
        JournalEventEnvelope journalEvent)
    {
        if (folderResolution.SelectedPath is null)
        {
            throw new InvalidOperationException(
                "A journal folder is required to replay quest events.");
        }

        var enabled = questSettingsStore.LoadEnabled();
        if (!enabled)
        {
            throw new InvalidOperationException(
                "Quests must be enabled before replaying an event.");
        }

        var result = await questRuntimeCoordinator.ReplayEventAsync(
            folderResolution.SelectedPath,
            journalEvent,
            allowCargoFile: !IsSharedCargoSuppressed,
            cancellationToken: CancellationToken.None);
        QuestWorkspace.ApplyRuntimeResult(result, enabled);
        UpdateQuestOverlayPresentation(result.Quests, enabled);
        OnPropertyChanged(nameof(Quests));
        OnPropertyChanged(nameof(QuestUnreadMessageCount));
        QuestStatusMessage = result.Warnings.Count > 0
            ? string.Join(Environment.NewLine, result.Warnings)
            : (result.Quests.Count == 0) switch
            {
                true => "No active quests received the replayed event.",
                false => $"Replayed {journalEvent.EventName}; "
                                                                                           + $"{result.Quests.Count:N0} active quest(s), "
                                                                                           + $"{QuestUnreadMessageCount:N0} unread message(s)."
            };
        return result;
    }

    private async Task<bool> EnsureCommanderProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(journalState.FrontierId))
        {
            return false;
        }

        var isOdyssey = journalState.IsOdyssey ?? true;
        if (string.Equals(
                activeProfileFrontierId,
                journalState.FrontierId,
                StringComparison.OrdinalIgnoreCase)
            && activeProfileIsOdyssey == isOdyssey)
        {
            activeProfileCommanderName = journalState.CommanderName
                ?? activeProfileCommanderName;
            return false;
        }

        var result = await commanderProfileStore.LoadAsync(
            journalState.FrontierId,
            isOdyssey,
            CancellationToken.None);
        loadedSystemHistoryKey = null;
        loadedSystemBodyDataKey = null;
        CancelSystemBodyDataRequest();
        activeProfileFrontierId = journalState.FrontierId;
        activeProfileCommanderName = journalState.CommanderName
            ?? result.Data?.CommanderName;
        activeProfileIsOdyssey = isOdyssey;
        resetExplorationCommand.RaiseCanExecuteChanged();
        resetExobiologyCommand.RaiseCanExecuteChanged();
        clearSurfaceTrackersCommand.RaiseCanExecuteChanged();

        if (result.Data is null)
        {
            activeProfileRavenApiKey = null;
            Inara.SetCommanderProfile(
                null,
                journalState.CommanderName,
                isOdyssey,
                inaraApiKey: null);
            SurfaceSurvey.Reset();
            Combat.LoadProfile(null, null, isOdyssey, CombatSnapshot.Empty);
            Colonization.SetCommanderProfile(null, isOdyssey, apiKey: null);
            ExplorationStatusMessage = result.Error
                ?? "The commander profile could not be loaded.";
            ExobiologyStatusMessage = result.Error
                ?? "The commander profile could not be loaded.";
            Search.SetProfileError(
                result.Error ?? "The commander profile could not be loaded.");
            BoxelSearch.SetProfileError(
                result.Error ?? "The commander profile could not be loaded.");
            Guardian.SetProfileError(
                result.Error ?? "The commander profile could not be loaded.");
            RamTah.SetProfileError(
                result.Error ?? "The commander profile could not be loaded.");
            return false;
        }

        activeProfileRavenApiKey = result.Data.RavenColonialApiKey;
        Inara.SetCommanderProfile(
            result.Data.FrontierId,
            activeProfileCommanderName,
            result.Data.IsOdyssey,
            result.Data.InaraApiKey);
        Colonization.SetCommanderProfile(
            result.Data.FrontierId,
            result.Data.IsOdyssey,
            result.Data.RavenColonialApiKey);

        explorationState.Reset(result.Data.Exploration);
        exobiologyState.Reset(result.Data.Exobiology);
        SurfaceSurvey.Reset(result.Data.Exobiology);
        Combat.LoadProfile(
            result.Data.FrontierId,
            activeProfileCommanderName,
            result.Data.IsOdyssey,
            result.Data.Combat);
        Search.LoadProfile(
            result.Data.FrontierId,
            activeProfileCommanderName,
            result.Data.IsOdyssey,
            result.Data.SphereLimit);
        await BoxelSearch.LoadProfileAsync(
            result.Data.FrontierId,
            activeProfileCommanderName,
            result.Data.IsOdyssey,
            result.Data.BoxelSearch);
        await Guardian.LoadProfileAsync(
            result.Data.FrontierId,
            result.Data.IsOdyssey,
            CancellationToken.None);
        RamTah.LoadProfile(
            result.Data.FrontierId,
            activeProfileCommanderName,
            result.Data.IsOdyssey,
            result.Data.RamTah);
        UpdateExplorationDisplay(result.Data.Exploration);
        UpdateExobiologyDisplay(result.Data.Exobiology);
        ExplorationStatusMessage = result.Exists
            ? $"Loaded compatible totals from {Path.GetFileName(result.Path)}."
            : $"No existing profile was found; session totals will be saved to "
                + Path.GetFileName(result.Path)
                + ".";
        ExobiologyStatusMessage = result.Exists
            ? $"Loaded legacy-compatible organic scan state from "
                + Path.GetFileName(result.Path)
                + "."
            : $"No existing profile was found; organic scan state will be saved to "
                + Path.GetFileName(result.Path)
                + ".";
        return result.Exists;
    }

    private async Task SaveExplorationAsync(ExplorationSnapshot snapshot)
    {
        if (activeProfileFrontierId is null)
        {
            return;
        }

        try
        {
            await commanderProfileStore.SaveExplorationAsync(
                activeProfileFrontierId,
                activeProfileCommanderName,
                activeProfileIsOdyssey,
                snapshot,
                CancellationToken.None);
            ExplorationStatusMessage = $"Totals saved to "
                + Path.GetFileName(commanderProfileStore.GetProfilePath(
                    activeProfileFrontierId,
                    activeProfileIsOdyssey))
                + ".";
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException)
        {
            ExplorationStatusMessage = "Totals changed for this session but could not be saved: "
                + exception.Message;
        }
    }

    private void UpdateExplorationDisplay(ExplorationSnapshot snapshot)
    {
        EstimatedExplorationValue = $"{snapshot.EstimatedRewards:N0} CR";
        ExplorationJumps = snapshot.JumpCount.ToString("N0");
        ExplorationDistance = $"{snapshot.DistanceTravelled:N1} ly";
        ExplorationBodies = $"Scanned: {snapshot.ScanCount:N0}, "
            + $"DSS: {snapshot.DetailedSurfaceScanCount:N0}, "
            + $"Landed: {snapshot.LandedBodyCount:N0}";
    }

    private async Task SaveExobiologyAsync(ExobiologySnapshot snapshot)
    {
        if (activeProfileFrontierId is null)
        {
            return;
        }

        try
        {
            await commanderProfileStore.SaveExobiologyAsync(
                activeProfileFrontierId,
                activeProfileCommanderName,
                activeProfileIsOdyssey,
                snapshot,
                CancellationToken.None);
            ExobiologyStatusMessage = $"Organic scan state saved to "
                + Path.GetFileName(commanderProfileStore.GetProfilePath(
                    activeProfileFrontierId,
                    activeProfileIsOdyssey))
                + ".";
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException)
        {
            ExobiologyStatusMessage =
                "Organic scan state changed for this session but could not be saved: "
                + exception.Message;
        }
    }

    private void UpdateExobiologyDisplay(ExobiologySnapshot snapshot)
    {
        UnclaimedBioRewards = $"{snapshot.OrganicRewards:N0} CR";
        UnclaimedBioScans = snapshot.ScannedBioEntryIds.Count == 1
            ? "1 organism"
            : $"{snapshot.ScannedBioEntryIds.Count:N0} organisms";
        var activeSample = snapshot.ScanTwo ?? snapshot.ScanOne;
        ActiveOrganicSpecies = activeSample is null
            ? Unavailable
            : exobiologyState.ActiveSpeciesDisplayName
                ?? activeSample.Species;
        if (activeSample is null)
        {
            OrganicSampleRange = Unavailable;
        }
        else if (exobiologyState.NearestActiveSampleDistance is not { } distance)
        {
            OrganicSampleRange = $"{activeSample.Radius:N0} m minimum separation";
        }
        else if (exobiologyState.RemainingSampleDistance > 0)
        {
            OrganicSampleRange = $"{distance:N0} m from nearest sample · {exobiologyState.RemainingSampleDistance:N0} m remaining";
        }
        else
        {
            OrganicSampleRange = $"{distance:N0} m from nearest sample · clear to sample";
        }
        OrganicScanProgress = snapshot.ScanOne is null
            ? "Ready for sample 1 of 3"
            : snapshot.ScanTwo is null
                ? "Sample 1 of 3 recorded"
                : "Samples 1 and 2 of 3 recorded";
        BioFirstFootfall = exobiologyState.CurrentBodyFirstFootfall switch
        {
            true => "Confirmed; 5x reward applies",
            false => "Not first footfall",
            null => "Unknown for current body",
        };
        IsCurrentBodyFirstFootfall =
            exobiologyState.CurrentBodyFirstFootfall == true;
        CanToggleCurrentBodyFirstFootfall =
            exobiologyState.CurrentBodySystemAddress is not null
            && exobiologyState.CurrentBodyId is not null
            && SystemSurvey.Snapshot.SystemAddress
                == exobiologyState.CurrentBodySystemAddress;
        IsOrganicSample1Complete = snapshot.ScanOne is not null;
        IsOrganicSample2Complete = snapshot.ScanTwo is not null;
        OnPropertyChanged(nameof(HasActiveOrganicSample));
    }

    private static bool IsExobiologyContextEvent(string eventName)
    {
        return eventName is "Location"
            or "FSDJump"
            or "CarrierJump"
            or "ApproachBody"
            or "Scan"
            or "Disembark";
    }

    private async Task LoadCurrentSystemHistoryAsync()
    {
        var current = SystemSurvey.Snapshot;
        if (string.IsNullOrWhiteSpace(activeProfileFrontierId)
            || string.IsNullOrWhiteSpace(current.SystemName)
            || current.SystemAddress is not { } systemAddress
            || systemAddress <= 0)
        {
            return;
        }

        var key = activeProfileFrontierId + "\n" + systemAddress;
        if (string.Equals(
            loadedSystemHistoryKey,
            key,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        loadedSystemHistoryKey = key;
        var result = await systemScanPersistenceStore.LoadAsync(
            activeProfileFrontierId,
            activeProfileCommanderName ?? journalState.CommanderName,
            current.SystemName,
            systemAddress,
            current.StarPosition,
            CancellationToken.None);
        if (result.Error is not null)
        {
            var message = "Imported system history was preserved but could not "
                + "be loaded safely from "
                + Path.GetFileName(result.Path)
                + ": "
                + result.Error;
            applicationLogService?.Append(message);
            StatusMessage = message;
            return;
        }

        if (result.Snapshot is { } history)
        {
            SystemSurvey.MergeKnownSystemData(history);
        }
    }

    private async Task LoadCurrentSystemBodyDataAsync()
    {
        if (systemBodyDataClient is null)
        {
            return;
        }

        if (!SystemSurvey.UseExternalData)
        {
            loadedSystemBodyDataKey = null;
            CancelSystemBodyDataRequest();
            return;
        }

        var current = SystemSurvey.Snapshot;
        if (string.IsNullOrWhiteSpace(current.SystemName)
            || current.SystemAddress is not { } systemAddress
            || systemAddress <= 0)
        {
            return;
        }

        var key = systemAddress
            + "\nbiology="
            + SystemSurvey.UseExternalBioData;
        if (string.Equals(
            loadedSystemBodyDataKey,
            key,
            StringComparison.Ordinal))
        {
            return;
        }

        CancelSystemBodyDataRequest();
        var cancellation = new CancellationTokenSource();
        systemBodyDataCancellation = cancellation;
        loadedSystemBodyDataKey = key;
        try
        {
            var result = await systemBodyDataClient.GetAsync(
                current.SystemName,
                systemAddress,
                cancellation.Token);
            if (cancellation.IsCancellationRequested
                || SystemSurvey.Snapshot.SystemAddress != systemAddress
                || !SystemSurvey.UseExternalData)
            {
                return;
            }

            var changed = false;
            foreach (var provider in result.Providers)
            {
                changed |= SystemSurvey.MergeKnownSystemData(
                    provider.Snapshot,
                    SystemSurvey.UseExternalBioData);
            }

            foreach (var warning in result.Warnings)
            {
                applicationLogService?.Append(warning);
            }

            if (changed)
            {
                await PersistCurrentSystemScanAsync();
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // A newer system or preference replaced this request.
        }
        finally
        {
            if (ReferenceEquals(systemBodyDataCancellation, cancellation))
            {
                systemBodyDataCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private void CancelSystemBodyDataRequest()
    {
        var cancellation = systemBodyDataCancellation;
        systemBodyDataCancellation = null;
        cancellation?.Cancel();
    }

    private async Task PersistSystemScanAsync(
        IReadOnlyList<JournalEventEnvelope> journalEvents)
    {
        var snapshot = SystemSurvey.Snapshot;
        if (string.IsNullOrWhiteSpace(activeProfileFrontierId)
            || snapshot.SystemAddress is not { } systemAddress
            || systemAddress <= 0
            || string.IsNullOrWhiteSpace(snapshot.SystemName))
        {
            return;
        }

        foreach (var journalEvent in journalEvents)
        {
            if (!IsSystemVisitEvent(journalEvent.EventName)
                || journalEvent.Timestamp is not { } timestamp
                || !TryGetSystemAddress(journalEvent, out var eventAddress)
                || eventAddress != systemAddress)
            {
                continue;
            }

            activeSystemVisitAddress = eventAddress;
            activeSystemVisitedAt = timestamp;
        }

        if (activeSystemVisitAddress != systemAddress
            || activeSystemVisitedAt is not { } visitedAt
            || !journalEvents.Any(journalEvent =>
                IsSystemScanPersistenceEvent(journalEvent.EventName)))
        {
            return;
        }

        await PersistCurrentSystemScanAsync(snapshot, visitedAt);
    }

    private async Task PersistCurrentSystemScanAsync(
        (int BodyId, bool Value)? firstFootfallCorrection = null)
    {
        var snapshot = SystemSurvey.Snapshot;
        if (snapshot.SystemAddress is not { } systemAddress
            || activeSystemVisitAddress != systemAddress
            || activeSystemVisitedAt is not { } visitedAt)
        {
            return;
        }

        await PersistCurrentSystemScanAsync(
            snapshot,
            visitedAt,
            firstFootfallCorrection);
    }

    private async Task PersistCurrentSystemScanAsync(
        SystemScanSnapshot snapshot,
        DateTimeOffset visitedAt,
        (int BodyId, bool Value)? firstFootfallCorrection = null)
    {
        if (string.IsNullOrWhiteSpace(activeProfileFrontierId))
        {
            return;
        }

        try
        {
            var context = new SystemScanPersistenceContext(
                activeProfileFrontierId,
                activeProfileCommanderName ?? journalState.CommanderName,
                visitedAt);
            var result = firstFootfallCorrection is { } correction
                ? await systemScanPersistenceStore
                    .SaveFirstFootfallCorrectionAsync(
                        context,
                        snapshot,
                        correction.BodyId,
                        correction.Value,
                        CancellationToken.None)
                : await systemScanPersistenceStore.SaveAsync(
                    context,
                    snapshot,
                    CancellationToken.None);
            SystemSurvey.SetRepeatVisitBiologySuppression(
                result.ShouldSuppressBiologyOverlays);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException)
        {
            var message = "System survey history was not updated because its "
                + "legacy-compatible data file could not be written safely: "
                + exception.Message;
            applicationLogService?.Append(message);
            StatusMessage = message;
        }
    }

    private static bool IsSystemVisitEvent(string eventName)
    {
        return eventName is "Location" or "FSDJump" or "CarrierJump";
    }

    private static bool IsSystemScanPersistenceEvent(string eventName)
    {
        return eventName is "Location"
            or "FSDJump"
            or "CarrierJump"
            or "FSSDiscoveryScan"
            or "FSSAllBodiesFound"
            or "Scan"
            or "ScanBaryCentre"
            or "SAAScanComplete"
            or "FSSBodySignals"
            or "SAASignalsFound"
            or "ScanOrganic"
            or "CodexEntry"
            or "FSSSignalDiscovered"
            or "ApproachBody"
            or "Touchdown"
            or "SupercruiseExit"
            or "Disembark";
    }

    private static bool TryGetSystemAddress(
        JournalEventEnvelope journalEvent,
        out long systemAddress)
    {
        systemAddress = 0;
        if (!journalEvent.Payload.TryGetProperty(
                "SystemAddress",
                out var address))
        {
            return false;
        }

        if (address.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            return address.TryGetInt64(out systemAddress);
        }

        return address.ValueKind == System.Text.Json.JsonValueKind.String
            && long.TryParse(address.GetString(), out systemAddress);
    }

    public async Task ResetExplorationAsync()
    {
        if (!IsResetExplorationPending)
        {
            IsResetExplorationPending = true;
            ExplorationStatusMessage = "Select Confirm reset to clear all six exploration totals.";
            return;
        }

        explorationState.Reset();
        var snapshot = explorationState.CreateSnapshot();
        UpdateExplorationDisplay(snapshot);
        IsResetExplorationPending = false;
        await SaveExplorationAsync(snapshot);
    }

    private Task CancelResetExplorationAsync()
    {
        IsResetExplorationPending = false;
        ExplorationStatusMessage = "Reset cancelled; totals were not changed.";
        return Task.CompletedTask;
    }

    public async Task ResetExobiologyAsync()
    {
        if (!IsResetExobiologyPending)
        {
            IsResetExobiologyPending = true;
            ExobiologyStatusMessage = "Select Confirm clear to remove all unclaimed "
                + "organic rewards. Active sample progress will be kept.";
            return;
        }

        exobiologyState.ClearUnclaimedRewards();
        var snapshot = exobiologyState.CreateSnapshot();
        UpdateExobiologyDisplay(snapshot);
        IsResetExobiologyPending = false;
        await SaveExobiologyAsync(snapshot);
    }

    public async Task ClearSurfaceTrackersAsync()
    {
        try
        {
            await SurfaceSurvey.ClearAllTrackersAsync(
                firstFootfallInferenceCancellation.Token);
            ExobiologyStatusMessage = SurfaceSurvey.StatusText;
        }
        catch (OperationCanceledException)
        {
            // Disposal/cancellation must not fault the async-void command.
        }
        finally
        {
            clearSurfaceTrackersCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task<bool> ToggleCurrentBodyFirstFootfallAsync()
    {
        var system = SystemSurvey.Snapshot;
        if (exobiologyState.CurrentBodySystemAddress is not { } systemAddress
            || exobiologyState.CurrentBodyId is not { } bodyId
            || system.SystemAddress != systemAddress)
        {
            ExobiologyStatusMessage =
                "First-footfall state cannot be changed until the current body is known.";
            return false;
        }

        var value = exobiologyState.CurrentBodyFirstFootfall != true;
        if (!SystemSurvey.SetBodyFirstFootfall(bodyId, value))
        {
            ExobiologyStatusMessage =
                "First-footfall state cannot be changed until the current body is known.";
            return false;
        }

        exobiologyState.SetFirstFootfall(systemAddress, bodyId, value);
        var snapshot = exobiologyState.CreateSnapshot();
        UpdateExobiologyDisplay(snapshot);
        SystemSurvey.ApplyUpdate([], null, snapshot);
        await SaveExobiologyAsync(snapshot);
        await PersistCurrentSystemScanAsync((bodyId, value));
        return true;
    }

    private async Task<int> ApplyFirstFootfallTextCommandsAsync(
        IReadOnlyList<JournalEventEnvelope> journalEvents)
    {
        var applied = 0;
        foreach (var journalEvent in journalEvents)
        {
            if (journalEvent.EventName != "SendText"
                || !journalEvent.Payload.TryGetProperty("Message", out var value)
                || value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var message = value.GetString()?.Trim().ToLowerInvariant();
            if (message is null
                || !(message.StartsWith(".firstfoot", StringComparison.Ordinal)
                    || message.StartsWith(".ff", StringComparison.Ordinal)))
            {
                continue;
            }

            var requestedBodyName = message.Split(' ', 2) is { Length: 2 } parts
                ? parts[1].Trim()
                : null;
            var system = SystemSurvey.Snapshot;
            if (system.SystemAddress is not { } systemAddress)
            {
                ExobiologyStatusMessage =
                    "First-footfall state cannot be changed until the current system is known.";
                continue;
            }

            var body = string.IsNullOrWhiteSpace(requestedBodyName)
                ? null
                : system.Bodies.FirstOrDefault(candidate =>
                    BodyNameMatchesCommand(
                        candidate,
                        system.SystemName,
                        requestedBodyName));
            body ??= system.CurrentBodyId is { } currentBodyId
                ? system.Bodies.FirstOrDefault(candidate =>
                    candidate.BodyId == currentBodyId)
                : null;
            if (body is null)
            {
                ExobiologyStatusMessage =
                    "First-footfall state cannot be changed until the current body is known.";
                continue;
            }

            var firstFootfall = !body.IsFirstFootfall;
            if (!SystemSurvey.SetBodyFirstFootfall(body.BodyId, firstFootfall))
            {
                continue;
            }

            exobiologyState.SetFirstFootfall(
                systemAddress,
                body.BodyId,
                firstFootfall);
            await PersistCurrentSystemScanAsync((body.BodyId, firstFootfall));
            ExobiologyStatusMessage = firstFootfall
                ? $"Recorded first footfall for {body.Name}."
                : $"Cleared first footfall for {body.Name}.";
            applied++;
        }

        return applied;
    }

    private async Task<bool> ApplyDesktopTextCommandsAsync(
        IReadOnlyList<JournalEventEnvelope> journalEvents)
    {
        var requestShutdown = false;
        foreach (var journalEvent in journalEvents)
        {
            if (journalEvent.EventName != "SendText"
                || !journalEvent.Payload.TryGetProperty("Message", out var value)
                || value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            switch (value.GetString()?.Trim().ToLowerInvariant())
            {
                case ".imgs":
                    await OpenCurrentSystemScreenshotFolderAsync();
                    break;
                case ".kill":
                    if (journalCommandShutdownRequester is null)
                    {
                        StatusMessage =
                            "The desktop shutdown service is not available.";
                        break;
                    }

                    requestShutdown = true;
                    break;
                case "!" when HumanSite.ActiveSite is { } site:
                    await GroundTarget.SetTargetAsync(
                        new SurfaceCoordinate(
                            site.Location.Latitude,
                            site.Location.Longitude),
                        "The active settlement origin is now the ground target.");
                    break;
                case "@@":
                    await CaptureShipCockpitOffsetAsync();
                    break;
                case "!!":
                    await CopyGroundTargetOffsetAsync();
                    break;
                case "..":
                    await CopySettlementOffsetAsync();
                    break;
                case "//":
                    CompareSettlementOffsetCalculations();
                    break;
            }
        }

        return requestShutdown;
    }

    private async Task CaptureShipCockpitOffsetAsync()
    {
        if (!TryGetSurfaceCommandContext(
                out var currentStatus,
                out var currentLocation,
                out var radius)
            || string.IsNullOrWhiteSpace(journalState.ShipType))
        {
            StatusMessage =
                "A current ship and surface position are required to calibrate its cockpit offset.";
            return;
        }

        var shipType = journalState.ShipType;
        var offset = HumanSiteNavigation.GetSiteOffset(
            GroundTarget.Target,
            currentLocation,
            radius,
            currentStatus.NormalizedHeading);
        HumanSiteVehicleOffsets.Set(shipType, offset);
        var text = string.Create(
            CultureInfo.InvariantCulture,
            $"{{ \"{shipType}\", new HumanSiteMapPoint({offset.X:R}, {offset.Y:R}) }}, ");
        applicationLogService?.Append("Cockpit offset: " + text);
        if (await WriteJournalCommandClipboardAsync(text))
        {
            StatusMessage =
                $"Captured and copied the {shipType} cockpit offset for this session.";
        }
    }

    private async Task CopyGroundTargetOffsetAsync()
    {
        if (!TryGetAlignedSettlementCommandContext(
                out var currentStatus,
                out var currentLocation,
                out var radius,
                out var siteHeading))
        {
            return;
        }

        var offset = HumanSiteNavigation.GetSiteOffset(
            GroundTarget.Target,
            currentLocation,
            radius,
            siteHeading);
        var rotation = SurfaceNavigation.NormalizeDegrees(
            currentStatus.NormalizedHeading - siteHeading);
        var text = "\"offset\": " + FormatMapPoint(offset);
        if (rotation != 0)
        {
            text += string.Create(
                CultureInfo.InvariantCulture,
                $", \"rot\": {rotation:R}");
        }

        applicationLogService?.Append(text);
        if (await WriteJournalCommandClipboardAsync(text))
        {
            StatusMessage =
                "Copied the ground-target offset and settlement-relative rotation.";
        }
    }

    private async Task CopySettlementOffsetAsync()
    {
        if (!TryGetAlignedSettlementCommandContext(
                out _,
                out var currentLocation,
                out var radius,
                out var siteHeading)
            || HumanSite.ActiveSite is not { } site)
        {
            return;
        }

        var offset = HumanSiteNavigation.GetSiteOffset(
            new SurfaceCoordinate(
                site.Location.Latitude,
                site.Location.Longitude),
            currentLocation,
            radius,
            siteHeading);
        var text = FormatMapPoint(offset);
        applicationLogService?.Append(
            "Relative to settlement origin: " + text);
        if (await WriteJournalCommandClipboardAsync(text))
        {
            StatusMessage = "Copied the current settlement-relative offset.";
        }
    }

    private void CompareSettlementOffsetCalculations()
    {
        if (!TryGetAlignedSettlementCommandContext(
                out _,
                out var currentLocation,
                out var radius,
                out var siteHeading)
            || HumanSite.ActiveSite is not { } site)
        {
            return;
        }

        var siteLocation = new SurfaceCoordinate(
            site.Location.Latitude,
            site.Location.Longitude);
        var direct = HumanSiteNavigation.GetSiteOffset(
            siteLocation,
            currentLocation,
            radius,
            siteHeading);
        var distance = SurfaceNavigation.GetDistance(
            siteLocation,
            currentLocation,
            radius);
        var angle = SurfaceNavigation.NormalizeDegrees(
            SurfaceNavigation.GetBearing(siteLocation, currentLocation)
                - siteHeading);
        var legacyRadians = (180 - angle) * Math.PI / 180;
        var alternate = new HumanSiteMapPoint(
            Math.Sin(legacyRadians) * distance,
            Math.Cos(legacyRadians) * distance);
        applicationLogService?.Append(
            "Settlement offset comparison: alternate "
                + FormatMapPoint(alternate)
                + " vs direct "
                + FormatMapPoint(direct));
        StatusMessage =
            "Settlement offset comparison was written to the application log.";
    }

    private bool TryGetAlignedSettlementCommandContext(
        out EliteStatus currentStatus,
        out SurfaceCoordinate currentLocation,
        out double radius,
        out double siteHeading)
    {
        siteHeading = 0;
        if (!TryGetSurfaceCommandContext(
                out currentStatus,
                out currentLocation,
                out radius)
            || HumanSite.ActiveSite is not { Heading: { } heading })
        {
            StatusMessage =
                "An aligned settlement and current surface position are required for this measurement.";
            return false;
        }

        siteHeading = heading;
        return true;
    }

    private bool TryGetSurfaceCommandContext(
        out EliteStatus currentStatus,
        out SurfaceCoordinate currentLocation,
        out double radius)
    {
        currentStatus = latestStatus!;
        currentLocation = default;
        radius = 0;
        if (latestStatus is not { HasLatitudeLongitude: true } status
            || status.PlanetRadius <= 0)
        {
            return false;
        }

        try
        {
            currentLocation = new SurfaceCoordinate(
                status.Latitude,
                status.Longitude);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        currentStatus = status;
        radius = (double)status.PlanetRadius;
        return double.IsFinite(radius) && radius > 0;
    }

    private async Task<bool> WriteJournalCommandClipboardAsync(string text)
    {
        if (journalCommandClipboardWriter is null)
        {
            StatusMessage = "The desktop clipboard is not available.";
            return false;
        }

        try
        {
            await journalCommandClipboardWriter(text);
            return true;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
                or NotSupportedException
                or UnauthorizedAccessException)
        {
            StatusMessage = "The measurement could not be copied: "
                + exception.Message;
            return false;
        }
    }

    private static string FormatMapPoint(HumanSiteMapPoint point)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{{ \"X\": {point.X:R}, \"Y\": {point.Y:R} }}");
    }

    private async Task<bool> OpenCurrentSystemScreenshotFolderAsync()
    {
        if (string.IsNullOrWhiteSpace(journalState.SystemName))
        {
            StatusMessage =
                "The screenshot folder cannot be opened until the current system is known.";
            return false;
        }

        var folder = Path.Combine(
            ScreenshotProcessing.TargetFolder,
            SystemNoteStore.MakeSafeFileName(journalState.SystemName));
        if (!Directory.Exists(folder))
        {
            StatusMessage =
                $"No screenshot folder exists for {journalState.SystemName}.";
            return false;
        }

        if (journalCommandDirectoryLauncher is null)
        {
            StatusMessage = "The desktop folder launcher is not available.";
            return false;
        }

        try
        {
            var launched = await journalCommandDirectoryLauncher(
                new DirectoryInfo(folder));
            StatusMessage = launched
                ? "Opened the current system screenshot folder."
                : "The operating system could not open the screenshot folder.";
            return launched;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
                or NotSupportedException
                or UnauthorizedAccessException)
        {
            StatusMessage = "The screenshot folder could not be opened: "
                + exception.Message;
            return false;
        }
    }

    private static bool BodyNameMatchesCommand(
        SystemScanBodySnapshot body,
        string? systemName,
        string requestedName)
    {
        var localName = !string.IsNullOrWhiteSpace(systemName)
            && body.Name.StartsWith(systemName, StringComparison.OrdinalIgnoreCase)
                ? body.Name[systemName.Length..].Trim()
                : body.Name;
        return string.Equals(
                localName,
                requestedName,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                localName.Replace(" ", string.Empty, StringComparison.Ordinal),
                requestedName,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                body.ShortName,
                requestedName,
                StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> TryInferFirstFootfallAsync(
        JournalMonitorUpdate update)
    {
        var preferences = firstFootfallInferenceSettingsStore.Load();
        var system = SystemSurvey.Snapshot;
        var body = system.CurrentBodyId is { } bodyId
            ? system.Bodies.FirstOrDefault(candidate =>
                candidate.BodyId == bodyId)
            : null;
        if (update.IsBootstrapRead
            || !preferences.Enabled
            || Guardian.ActiveSite is not null
            || !update.JournalEvents.Any(IsSurfaceDisembark)
            || system.SystemAddress is not { } systemAddress
            || body is null
            || system.Population != 0
            || body.IsFirstFootfall
            || body.WasFootfalled == false
            || IsKnownLegacyValuableBody(body.Kind))
        {
            return false;
        }

        FirstFootfallInferenceResult result;
        try
        {
            result = await firstFootfallInferenceService.DetectAsync(
                preferences,
                firstFootfallInferenceCancellation.Token);
        }
        catch (OperationCanceledException) when (
            firstFootfallInferenceCancellation.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidOperationException
                or NotSupportedException)
        {
            applicationLogService?.Append(
                "First-footfall notification detection stopped safely: "
                    + exception.Message);
            return false;
        }

        if (!result.Detected)
        {
            return false;
        }

        var current = SystemSurvey.Snapshot;
        if (Guardian.ActiveSite is not null
            || current.SystemAddress != systemAddress
            || current.CurrentBodyId != body.BodyId
            || current.Population != 0)
        {
            applicationLogService?.Append(
                "Ignored a first-footfall notification because the active "
                    + "system or body changed during detection.");
            return false;
        }

        if (!SystemSurvey.SetCurrentBodyFirstFootfall(true))
        {
            return false;
        }

        exobiologyState.SetFirstFootfall(systemAddress, body.BodyId, true);
        var message = "First footfall inferred from Elite's on-screen notification "
            + $"after {result.SampleCount:N0} sample(s); match ratio "
            + $"{result.MaximumMatchRatio:P3}.";
        applicationLogService?.Append(message);
        ExobiologyStatusMessage = message;
        return true;
    }

    private static bool IsSurfaceDisembark(
        JournalEventEnvelope journalEvent)
    {
        if (journalEvent.EventName != "Disembark")
        {
            return false;
        }

        var root = journalEvent.Payload;
        return root.TryGetProperty("OnPlanet", out var onPlanet)
            && onPlanet.ValueKind is JsonValueKind.True
            && (!root.TryGetProperty("OnStation", out var onStation)
                || onStation.ValueKind is not JsonValueKind.True);
    }

    private static bool IsKnownLegacyValuableBody(SystemBodyKind kind)
    {
        return kind is SystemBodyKind.Star
            or SystemBodyKind.GasGiant
            or SystemBodyKind.Planet
            or SystemBodyKind.LandablePlanet;
    }

    private Task CancelResetExobiologyAsync()
    {
        IsResetExobiologyPending = false;
        ExobiologyStatusMessage = "Clear cancelled; unclaimed rewards were not changed.";
        return Task.CompletedTask;
    }

    private void ApplyStatus(EliteStatus status)
    {
        VehicleState = status.OnFoot
            ? "On foot"
            : (status.InSrv) switch
            {
                true => "SRV",
                false => (status.InFighter) switch
                {
                    true => "Fighter",
                    false => (status.InMainShip) switch
                    {
                        true => "Main ship",
                        false => (status.InTaxi) switch
                        {
                            true => "Taxi / shuttle",
                            false => "Unknown"
                        }
                    }
                }
            };
        SurfacePosition = status.HasLatitudeLongitude
            ? $"{status.Latitude:F6}, {status.Longitude:F6}"
            : Unavailable;
        HeadingAndAltitude = status.HasLatitudeLongitude
            ? $"{status.NormalizedHeading}° / {status.Altitude:N0} m"
            : Unavailable;
        GameUiFocus = status.GuiFocus.ToString();
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? Unavailable : value;
    }

    private void OnInaraUploadDisabled(object? sender, EventArgs eventArgs)
    {
        inaraPublisher.CancelPendingPublication();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        routeAutoCopyCoordinator.Dispose();
        BoxelSearch.CancelPendingOperations();
        JournalPostProcessor.Cancel();
        CancelSystemBodyDataRequest();
        firstFootfallInferenceCancellation.Cancel();
        firstFootfallInferenceService.Dispose();
        firstFootfallInferenceCancellation.Dispose();
        Colonization.Dispose();
        GalaxyMap.Dispose();
        Guardian.Dispose();
        QuestWorkspace.Dispose();
        Inara.UploadDisabled -= OnInaraUploadDisabled;
        inaraPublisher.Dispose();
        CommanderInstances.PropertyChanged -= OnCommanderInstancesPropertyChanged;
        CommanderInstances.Dispose();
        BiologyRewards.PropertyChanged -= OnBiologyRewardsChanged;
        OverlayInteraction.Dispose();
        FrontierProfile.Dispose();
        visitedStarsHttpClient?.Dispose();
        NetworkPrivacy.EddnUploadEnabledChanged -= OnEddnUploadEnabledChanged;
        if (eddnPublisher is IDisposable disposableEddnPublisher)
        {
            disposableEddnPublisher.Dispose();
        }
        questRuntimeCoordinator.Changed -= OnQuestCoordinatorChanged;
        questRuntimeCoordinator.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private void OnEddnUploadEnabledChanged(bool enabled)
    {
        eddnPublisher.SetEnabled(enabled);
    }

    private void OnCommanderInstancesPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName !=
            nameof(CommanderInstancesViewModel.HasMultipleGameWindows))
        {
            return;
        }

        var hasMultipleGameWindows =
            CommanderInstances.HasMultipleGameWindows;
        SetSharedCargoSuppressed(hasMultipleGameWindows);
        eddnPublisher.SetSuspended(hasMultipleGameWindows);
        OnPropertyChanged(nameof(IsSharedCargoSuppressed));
    }

    private void SetSharedCargoSuppressed(bool value)
    {
        if (value)
        {
            awaitFreshCargoSnapshot = true;
            cargoInventoryState.Reset(null);
            latestCargo = null;
            latestShipLocker = null;
            Guardian.ClearCargo();
        }

        FrontierProfile.UpdateLocalInventory(
            latestCargo,
            latestShipLocker,
            isSuppressed: value);

        DockToDock.SetSharedCargoSuppressed(value);
        Colonization.SetSharedCargoSuppressed(value);
    }

    private bool IsCurrentCommanderCompanionSnapshot(DateTimeOffset timestamp) =>
        companionIdentityChangedAt is not { } changedAt || timestamp >= changedAt;

    private void OnQuestCoordinatorChanged(object? sender, EventArgs eventArgs)
    {
        UpdateQuestOverlayPresentation(
            questRuntimeCoordinator.Snapshot,
            questSettingsStore.LoadEnabled());
        OnPropertyChanged(nameof(Quests));
        OnPropertyChanged(nameof(QuestUnreadMessageCount));
    }

    private void UpdateQuestOverlayPresentation(
        IReadOnlyList<QuestRuntimeSnapshot> quests,
        bool enabled)
    {
        QuestIndicator.Update(
            quests,
            latestStatus,
            enabled,
            journalState.MusicTrack);
        HumanSite.UpdateQuests(quests);
        var tags = enabled
            ? quests.SelectMany(quest => quest.Tags).ToArray()
            : [];
        GalaxyMap.UpdateQuestTags(tags);
        JumpInfo.UpdateQuestTags(tags);
        StationInfo.UpdateQuestTags(tags);
    }

    private void OnBiologyRewardsChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName ==
            nameof(BiologyRewardSettingsViewModel.Thresholds))
        {
            SystemSurvey.UpdateBiologyRewardThresholds(
                BiologyRewards.Thresholds);
        }
    }

    private static bool IsShowCodexCommand(JournalEventEnvelope journalEvent)
    {
        return journalEvent.EventName == "SendText"
            && journalEvent.Payload.TryGetProperty("Message", out var message)
            && message.ValueKind == System.Text.Json.JsonValueKind.String
            && string.Equals(
                message.GetString()?.Trim(),
                ".show",
                StringComparison.OrdinalIgnoreCase);
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class AsyncCommand(
        Func<Task> execute,
        Func<bool> canExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecute();
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                await execute();
            }
            catch (OperationCanceledException)
            {
                // Command disposal/cancellation is not a user-facing failure.
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
