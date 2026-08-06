using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using SrvSurvey.Core.Colonization;
using SrvSurvey.Core.Journal;
using SrvSurvey.Core.Search;
using SrvSurvey.Core.Storage;
using SrvSurvey.Desktop.Configuration;

namespace SrvSurvey.Desktop.ViewModels;

public sealed class ColonizationViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly TimeSpan DockingRefreshDelay = TimeSpan.FromSeconds(4);
    private const int MaximumBuildSiteRepairVisits = 50;

    private readonly IRavenColonialClient client;
    private readonly ColonizationBuildCatalog buildCatalog;
    private readonly ColonizationSettingsStore settingsStore;
    private readonly CommanderProfileStore? commanderProfileStore;
    private readonly LegacyColonizationProfileStore? legacyProfileStore;
    private ColonizationOverlayPreferences overlayPreferences;
    private readonly ColonizationConstructionState constructionState = new();
    private readonly ColonizationFleetCarrierIdentityTracker
        fleetCarrierIdentityTracker = new();
    private readonly AsyncCommand refreshCommand;
    private readonly AsyncCommand saveProjectsCommand;
    private readonly AsyncCommand saveRavenApiKeyCommand;
    private readonly AsyncCommand publishFleetCarrierCommand;
    private readonly AsyncCommand syncFleetCarrierCargoCommand;
    private readonly Func<TimeSpan, CancellationToken, Task> delayAsync;
    private readonly object buildSiteRepairLock = new();
    private readonly Queue<ColonizationBuildSiteRepairVisit>
        buildSiteRepairVisits = new();
    private readonly HashSet<ColonizationBuildSiteRepairVisit>
        buildSiteRepairVisitSet = [];
    private readonly HashSet<(long SystemAddress, long MarketId, string StationKey)>
        buildSiteRepairsInFlight = [];
    private CancellationTokenSource? dockingRefreshCancellation;
    private IReadOnlyList<ColonizationProjectRowViewModel> projects = [];
    private IReadOnlyList<ColonizationResourceRowViewModel> constructionResources =
        [];
    private HashSet<string> hiddenProjectIds = new(
        StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<ColonizationFleetCarrier> fleetCarriers = [];
    private ColonizationProject? localUntrackedProject;
    private CargoSnapshot? shipCargo;
    private MarketSnapshot? currentMarket;
    private EliteStatus? latestStatus;
    private string? commanderName;
    private string? currentSystemName;
    private long? currentSystemAddress;
    private IReadOnlyList<double> currentStarPosition = [];
    private string? primaryProjectId;
    private bool isEnabled;
    private bool isBusy;
    private bool hasUnsavedProjectVisibility;
    private bool fleetCarrierCargoSyncEnabled;
    private bool shipCargoPublishingEnabled;
    private bool sharedCargoSuppressed;
    private bool isFleetCarrierSyncBusy;
    private bool isShipCargoPublishingBusy;
    private string ravenApiKey = string.Empty;
    private string? storedRavenApiKey;
    private string? profileFrontierId;
    private bool profileIsOdyssey = true;
    private (long MarketId, DateTimeOffset Timestamp)? lastSyncedMarket;
    /// <summary>
    /// When true, the next squadron cargo GetDiff is skipped because MarketBuy/Sell
    /// already sent AdjustFleetCarrierCargo for this linked squadron FC (legacy parity).
    /// </summary>
    private bool skipNextCargoEvent;
    private string ravenCredentialStatus =
        "Load a commander profile to configure a Raven API key.";
    private string fleetCarrierSyncStatus =
        "Automatic Fleet Carrier cargo sync is off.";
    private string shipCargoPublishingStatus =
        "Automatic ship cargo publishing is off.";
    private string? currentShipType;
    private string? currentShipName;
    private string statusMessage;
    private string projectSummary = "No projects loaded.";
    private string constructionTitle = "No construction depot active";
    private string constructionStatus =
        "Dock at a construction site and open Construction Services.";

    public ColonizationViewModel(
        ColonizationSettingsStore settingsStore,
        IRavenColonialClient? client = null,
        ColonizationBuildCatalog? buildCatalog = null,
        CommanderProfileStore? commanderProfileStore = null,
        LegacyColonizationProfileStore? legacyProfileStore = null,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        this.settingsStore = settingsStore
            ?? throw new ArgumentNullException(nameof(settingsStore));
        this.client = client ?? new RavenColonialClient();
        this.commanderProfileStore = commanderProfileStore;
        this.legacyProfileStore = legacyProfileStore;
        this.delayAsync = delayAsync ?? Task.Delay;
        this.buildCatalog = buildCatalog
            ?? ColonizationBuildCatalog.LoadEmbedded();
        overlayPreferences = settingsStore.LoadOverlayPreferences();
        fleetCarrierCargoSyncEnabled =
            settingsStore.LoadFleetCarrierCargoSyncEnabled();
        shipCargoPublishingEnabled =
            settingsStore.LoadShipCargoPublishingEnabled();
        isEnabled = settingsStore.LoadEnabled();
        foreach (var visit in settingsStore.LoadBuildSiteRepairVisits())
        {
            buildSiteRepairVisits.Enqueue(visit);
            buildSiteRepairVisitSet.Add(visit);
        }
        statusMessage = isEnabled
            ? "Raven Colonial access is enabled. Waiting for a commander profile."
            : "Raven Colonial access is off. No project data will be fetched or published.";
        refreshCommand = new AsyncCommand(
            RefreshAsync,
            () => IsEnabled && !IsBusy && CommanderName is not null);
        saveProjectsCommand = new AsyncCommand(
            SaveProjectVisibilityAsync,
            () => IsEnabled
                && !IsBusy
                && HasUnsavedProjectVisibility
                && CommanderName is not null);
        saveRavenApiKeyCommand = new AsyncCommand(
            SaveRavenApiKeyAsync,
            CanSaveRavenApiKey);
        publishFleetCarrierCommand = new AsyncCommand(
            PublishCurrentFleetCarrierAsync,
            CanPublishCurrentFleetCarrier);
        syncFleetCarrierCargoCommand = new AsyncCommand(
            () => SyncFleetCarrierCargoAsync(force: true),
            CanSyncFleetCarrierCargo);
        RefreshCommand = refreshCommand;
        SaveProjectsCommand = saveProjectsCommand;
        SaveRavenApiKeyCommand = saveRavenApiKeyCommand;
        PublishFleetCarrierCommand = publishFleetCarrierCommand;
        SyncFleetCarrierCargoCommand = syncFleetCarrierCargoCommand;
        ProjectEditor = new ColonizationProjectEditorViewModel(
            this.client,
            this.buildCatalog,
            OnProjectCreatedAsync);
        SystemEditor = new ColonizationSystemEditorViewModel(
            this.client,
            this.buildCatalog);
        CommodityOverlay = new ColonizationCommodityOverlayViewModel();
        CommodityOverlay.ApplyPreferences(overlayPreferences);
        UpdateProjectEditorContext();
        UpdateSystemEditorContext();
        UpdateCommodityPlan();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand RefreshCommand { get; }

    public ICommand SaveProjectsCommand { get; }

    public ICommand SaveRavenApiKeyCommand { get; }

    public ICommand PublishFleetCarrierCommand { get; }

    public ICommand SyncFleetCarrierCargoCommand { get; }

    public ColonizationProjectEditorViewModel ProjectEditor { get; }

    public ColonizationSystemEditorViewModel SystemEditor { get; }

    public ColonizationCommodityOverlayViewModel CommodityOverlay { get; }

    public bool AutoShowCommodityOverlay
    {
        get => overlayPreferences.AutoShow;
        set => SaveOverlayPreferences(
            overlayPreferences with { AutoShow = value });
    }

    public bool ShowCommodityOverlayOnRightPanel
    {
        get => overlayPreferences.ShowOnRightPanel;
        set => SaveOverlayPreferences(
            overlayPreferences with { ShowOnRightPanel = value });
    }

    public bool ShowFleetCarrierCargo
    {
        get => overlayPreferences.ShowFleetCarrierCargo;
        set => SaveOverlayPreferences(
            overlayPreferences with { ShowFleetCarrierCargo = value });
    }

    public bool ShowFleetCarrierDelta
    {
        get => overlayPreferences.ShowFleetCarrierDelta;
        set => SaveOverlayPreferences(
            overlayPreferences with { ShowFleetCarrierDelta = value });
    }

    public bool InlineFleetCarrierCargo
    {
        get => overlayPreferences.InlineFleetCarrierCargo;
        set => SaveOverlayPreferences(
            overlayPreferences with { InlineFleetCarrierCargo = value });
    }

    public bool CollapseCoveredCommodityGroups
    {
        get => overlayPreferences.CollapseCoveredGroups;
        set => SaveOverlayPreferences(
            overlayPreferences with { CollapseCoveredGroups = value });
    }

    public bool HighlightAlmostCoveredFleetCarrierLoads
    {
        get => overlayPreferences.HighlightAlmostCoveredFleetCarrierLoads;
        set => SaveOverlayPreferences(
            overlayPreferences with
            {
                HighlightAlmostCoveredFleetCarrierLoads = value,
            });
    }

    public string RavenApiKey
    {
        get => ravenApiKey;
        set
        {
            if (SetField(ref ravenApiKey, value ?? string.Empty))
            {
                saveRavenApiKeyCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasCommanderProfile => profileFrontierId is not null;

    public bool HasStoredRavenApiKey =>
        !string.IsNullOrWhiteSpace(storedRavenApiKey);

    public string RavenCredentialStatus
    {
        get => ravenCredentialStatus;
        private set => SetField(ref ravenCredentialStatus, value);
    }

    public bool FleetCarrierCargoSyncEnabled
    {
        get => fleetCarrierCargoSyncEnabled;
        set
        {
            if (value == fleetCarrierCargoSyncEnabled)
            {
                return;
            }

            try
            {
                settingsStore.SaveFleetCarrierCargoSyncEnabled(value);
                fleetCarrierCargoSyncEnabled = value;
                OnPropertyChanged();
                FleetCarrierSyncStatus = value
                    ? (HasStoredRavenApiKey) switch
                    {
                        true => "Fleet Carrier cargo will sync from matching Market.json updates.",
                        false => "Save a Raven API key before Fleet Carrier cargo can sync."
                    }
                    : "Automatic Fleet Carrier cargo sync is off.";
                syncFleetCarrierCargoCommand.RaiseCanExecuteChanged();
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or InvalidOperationException)
            {
                FleetCarrierSyncStatus =
                    "The Fleet Carrier sync preference could not be saved: "
                    + exception.Message;
            }
        }
    }

    public bool IsFleetCarrierSyncBusy
    {
        get => isFleetCarrierSyncBusy;
        private set
        {
            if (SetField(ref isFleetCarrierSyncBusy, value))
            {
                OnPropertyChanged(nameof(FleetCarrierSyncButtonText));
                OnPropertyChanged(nameof(FleetCarrierPublishButtonText));
                RaiseCommandStates();
            }
        }
    }

    public string FleetCarrierSyncButtonText =>
        IsFleetCarrierSyncBusy ? "Syncing..." : "Sync current market";

    public string FleetCarrierPublishButtonText => IsFleetCarrierSyncBusy
        ? "Working..."
        : "Publish/link current carrier";

    public string FleetCarrierSyncStatus
    {
        get => fleetCarrierSyncStatus;
        private set => SetField(ref fleetCarrierSyncStatus, value);
    }

    public bool ShipCargoPublishingEnabled
    {
        get => shipCargoPublishingEnabled;
        set
        {
            if (value == shipCargoPublishingEnabled)
            {
                return;
            }

            try
            {
                settingsStore.SaveShipCargoPublishingEnabled(value);
                shipCargoPublishingEnabled = value;
                OnPropertyChanged();
                ShipCargoPublishingStatus = GetShipCargoReadyStatus();
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or InvalidOperationException)
            {
                ShipCargoPublishingStatus =
                    "The ship cargo publishing preference could not be saved: "
                    + exception.Message;
            }
        }
    }

    public bool IsShipCargoPublishingBusy
    {
        get => isShipCargoPublishingBusy;
        private set => SetField(ref isShipCargoPublishingBusy, value);
    }

    public string ShipCargoPublishingStatus
    {
        get => shipCargoPublishingStatus;
        private set => SetField(ref shipCargoPublishingStatus, value);
    }

    public bool SharedCargoSuppressed => sharedCargoSuppressed;

    public void SetSharedCargoSuppressed(bool value)
    {
        if (sharedCargoSuppressed == value)
        {
            return;
        }

        sharedCargoSuppressed = value;
        OnPropertyChanged(nameof(SharedCargoSuppressed));
        if (value)
        {
            shipCargo = null;
            UpdateCommodityPlan();
        }

        ShipCargoPublishingStatus = GetShipCargoReadyStatus();
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (value == isEnabled)
            {
                return;
            }

            try
            {
                settingsStore.SaveEnabled(value);
                isEnabled = value;
                OnPropertyChanged();
                RaiseCommandStates();
                if (value)
                {
                    StatusMessage = CommanderName is null
                        ? "Raven Colonial access is enabled. Waiting for a commander profile."
                        : "Raven Colonial access is enabled. Select Refresh projects to fetch data.";
                }
                else
                {
                    CancelDockingRefresh();
                    ClearProjects();
                    StatusMessage = "Raven Colonial access is off. No project data will be fetched or published.";
                }

                if (ShipCargoPublishingEnabled)
                {
                    ShipCargoPublishingStatus = GetShipCargoReadyStatus();
                }

                UpdateProjectEditorContext();
                UpdateSystemEditorContext();
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or InvalidOperationException)
            {
                StatusMessage =
                    "The Raven Colonial preference could not be saved: "
                    + exception.Message;
            }
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                OnPropertyChanged(nameof(RefreshButtonText));
                OnPropertyChanged(nameof(SaveButtonText));
                RaiseCommandStates();
            }
        }
    }

    public string RefreshButtonText => IsBusy ? "Refreshing..." : "Refresh projects";

    public string SaveButtonText => IsBusy ? "Saving..." : "Save selection";

    public string? CommanderName
    {
        get => commanderName;
        private set
        {
            if (SetField(ref commanderName, value))
            {
                OnPropertyChanged(nameof(CommanderStatus));
                RaiseCommandStates();
            }
        }
    }

    public string CommanderStatus => CommanderName is null
        ? "No commander profile is active."
        : $"Commander: {CommanderName}";

    public IReadOnlyList<ColonizationProjectRowViewModel> Projects
    {
        get => projects;
        private set
        {
            if (ReferenceEquals(projects, value))
            {
                return;
            }

            projects = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasProjects));
            OnPropertyChanged(nameof(HasNoProjects));
        }
    }

    public bool HasProjects => Projects.Count > 0;

    public bool HasNoProjects => !HasProjects;

    public bool HasUnsavedProjectVisibility
    {
        get => hasUnsavedProjectVisibility;
        private set
        {
            if (SetField(ref hasUnsavedProjectVisibility, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string ProjectSummary
    {
        get => projectSummary;
        private set => SetField(ref projectSummary, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetField(ref statusMessage, value);
    }

    public string ConstructionTitle
    {
        get => constructionTitle;
        private set => SetField(ref constructionTitle, value);
    }

    public string ConstructionStatus
    {
        get => constructionStatus;
        private set => SetField(ref constructionStatus, value);
    }

    public IReadOnlyList<ColonizationResourceRowViewModel>
        ConstructionResources
    {
        get => constructionResources;
        private set
        {
            if (ReferenceEquals(constructionResources, value))
            {
                return;
            }

            constructionResources = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasConstructionResources));
        }
    }

    public bool HasConstructionResources => ConstructionResources.Count > 0;

    public void SetCommanderProfile(
        string? frontierId,
        bool isOdyssey,
        string? apiKey)
    {
        profileFrontierId = string.IsNullOrWhiteSpace(frontierId)
            ? null
            : frontierId.Trim();
        profileIsOdyssey = isOdyssey;
        storedRavenApiKey = string.IsNullOrWhiteSpace(apiKey)
            ? null
            : apiKey.Trim();
        RavenApiKey = storedRavenApiKey ?? string.Empty;
        lastSyncedMarket = null;
        RavenCredentialStatus = profileFrontierId is null
            ? "Load a commander profile to configure a Raven API key."
            : storedRavenApiKey is null
                ? "No Raven API key is saved for this commander."
                : "A Raven API key is saved for this commander.";
        if (!FleetCarrierCargoSyncEnabled)
        {
            FleetCarrierSyncStatus =
                "Automatic Fleet Carrier cargo sync is off.";
        }
        else if (storedRavenApiKey is null)
        {
            FleetCarrierSyncStatus =
                "Save a Raven API key before Fleet Carrier cargo can sync.";
        }
        else
        {
            FleetCarrierSyncStatus =
                "Fleet Carrier cargo will sync from matching Market.json updates.";
        }
        ShipCargoPublishingStatus = GetShipCargoReadyStatus();
        OnPropertyChanged(nameof(HasCommanderProfile));
        OnPropertyChanged(nameof(HasStoredRavenApiKey));
        RaiseCommandStates();
        UpdateProjectEditorContext();
        UpdateSystemEditorContext();
    }

    public async Task SetCommanderAsync(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
        if (string.Equals(
                CommanderName,
                normalized,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CancelDockingRefresh();
        CommanderName = normalized;
        ClearProjects();
        UpdateProjectEditorContext();
        UpdateSystemEditorContext();
        if (CommanderName is null)
        {
            StatusMessage = "No commander profile is active.";
            return;
        }

        if (IsEnabled)
        {
            await RefreshAsync(CancellationToken.None);
        }
    }

    public void ApplyJournalEvents(
        IReadOnlyList<JournalEventEnvelope> journalEvents)
    {
        ArgumentNullException.ThrowIfNull(journalEvents);
        SystemEditor.ApplyJournalEvents(journalEvents);
        var before = constructionState.Version;
        foreach (var journalEvent in journalEvents)
        {
            constructionState.Apply(journalEvent);
            fleetCarrierIdentityTracker.Apply(journalEvent);
            ApplyShipIdentity(journalEvent);
        }

        if (constructionState.Version != before)
        {
            UpdateConstructionDisplay();
            UpdateProjectSummary();
            UpdateProjectEditorContext();
            publishFleetCarrierCommand.RaiseCanExecuteChanged();
            syncFleetCarrierCargoCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task SynchronizeLiveProjectsAsync(
        IReadOnlyList<JournalEventEnvelope> journalEvents,
        bool allowPublishing,
        CargoInventoryState? cargoInventory = null,
        bool cargoActivity = false,
        bool preferShipCargoDiffForSquadron = true)
    {
        ArgumentNullException.ThrowIfNull(journalEvents);
        if (!allowPublishing
            || !IsEnabled
            || CommanderName is null)
        {
            ClearSquadronCargoSyncState(cargoInventory);
            return;
        }

        // When ship cargo is not current (or suppressed), squadron carriers use
        // journal transfer adjustments. Otherwise use the full GetDiff path.
        var preferSquadronCargoDiff = cargoInventory is not null
            && preferShipCargoDiffForSquadron;
        var messages = new List<string>();
        foreach (var journalEvent in journalEvents)
        {
            var message = await TrySynchronizeLiveJournalEventAsync(
                journalEvent,
                preferSquadronCargoDiff,
                cargoInventory);
            if (!string.IsNullOrWhiteSpace(message))
            {
                messages.Add(message);
            }
        }

        if (cargoInventory is { } squadronCargoInventory
            && preferSquadronCargoDiff)
        {
            var squadronMessage = await TrySynchronizeSquadronCargoDiffAsync(
                squadronCargoInventory,
                cargoActivity);
            if (!string.IsNullOrWhiteSpace(squadronMessage))
            {
                messages.Add(squadronMessage);
            }
        }

        if (messages.Count > 0)
        {
            StatusMessage = string.Join(Environment.NewLine, messages);
        }
    }

    private void ClearSquadronCargoSyncState(CargoInventoryState? cargoInventory)
    {
        // Drop held squadron state when publishing is disabled so lastInventory /
        // skipNext cannot survive across later cargo updates.
        skipNextCargoEvent = false;
        if (cargoInventory?.HasPreservedSnapshot == true)
        {
            cargoInventory.ClearPreservedSnapshot();
        }
    }

    private async Task<string?> TrySynchronizeLiveJournalEventAsync(
        JournalEventEnvelope journalEvent,
        bool preferShipCargoDiffForSquadron,
        CargoInventoryState? cargoInventory)
    {
        try
        {
            return journalEvent.EventName switch
            {
                "Docked" => CombineMessages(
                    await SynchronizeDockedProjectAsync(journalEvent),
                    await SynchronizeBuildSiteRepairAsync(journalEvent)),
                "Location" when GetJournalBoolean(
                    journalEvent.Payload,
                    "Docked") == true =>
                    await SynchronizeBuildSiteRepairAsync(journalEvent),
                "ColonisationContribution" =>
                    await SynchronizeContributionAsync(journalEvent),
                "ColonisationConstructionDepot" =>
                    await SynchronizeDepotAsync(journalEvent),
                "ColonisationBeaconDeployed" =>
                    await SynchronizeBeaconDeploymentAsync(),
                "DockingGranted" => ScheduleDockingRefresh(journalEvent),
                "MarketBuy" or "MarketSell" or "CargoTransfer" =>
                    await SynchronizeFleetCarrierCargoAdjustmentAsync(
                        journalEvent,
                        preferShipCargoDiffForSquadron,
                        cargoInventory),
                _ => null,
            };
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            return $"Raven project sync skipped {journalEvent.EventName}: "
                + exception.Message;
        }
    }

    private async Task<string?> TrySynchronizeSquadronCargoDiffAsync(
        CargoInventoryState cargoInventory,
        bool cargoActivity)
    {
        try
        {
            return await SynchronizeSquadronFleetCarrierCargoDiffAsync(
                cargoInventory,
                cargoActivity);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            return "Raven project sync skipped squadron cargo diff: "
                + exception.Message;
        }
    }

    private string? ScheduleDockingRefresh(JournalEventEnvelope journalEvent)
    {
        if (Projects.Count == 0
            && !ColonizationDockingSnapshot.IsConstructionSiteName(GetJournalString(
                journalEvent.Payload,
                "StationName")))
        {
            return null;
        }

        CancelDockingRefresh();
        dockingRefreshCancellation = new CancellationTokenSource();
        _ = RefreshAfterDockingAsync(dockingRefreshCancellation.Token);
        return null;
    }

    private async Task RefreshAfterDockingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await delayAsync(DockingRefreshDelay, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await RefreshAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // A newer docking event, commander change, disable, or shutdown superseded it.
        }
    }

    private void CancelDockingRefresh()
    {
        var cancellation = dockingRefreshCancellation;
        dockingRefreshCancellation = null;
        if (cancellation is null)
        {
            return;
        }

        cancellation.Cancel();
        cancellation.Dispose();
    }

    private async Task<string?> SynchronizeBeaconDeploymentAsync()
    {
        if (storedRavenApiKey is null)
        {
            return "Raven architect update was not sent because this commander has no saved API key.";
        }

        if (string.IsNullOrWhiteSpace(currentSystemName))
        {
            return "Raven architect update was not sent because the current system is unknown.";
        }

        await client.UpdateSystemSitesAsync(
            currentSystemName,
            new ColonizationSystemSiteUpdate
            {
                Architect = CommanderName,
            },
            storedRavenApiKey,
            CancellationToken.None);
        return $"Registered {CommanderName} as the Raven architect for {currentSystemName}.";
    }

    /// <summary>
    /// Freeze ship cargo before CargoTransfer mutates the live projection when docked on a
    /// linked squadron fleet carrier. Squadron carriers do not use journal transfer deltas;
    /// they rely on <see cref="SynchronizeSquadronFleetCarrierCargoDiffAsync"/>.
    /// </summary>
    public void PrepareSquadronCargoTransferSnapshot(CargoInventoryState cargo)
    {
        ArgumentNullException.ThrowIfNull(cargo);
        if (!FleetCarrierCargoSyncEnabled
            || storedRavenApiKey is null
            || constructionState.CurrentDock is not { } dock
            || !IsLinkedSquadronFleetCarrier(dock))
        {
            return;
        }

        // Preserve the first before-state across multiple CargoTransfer events in one poll.
        if (!cargo.HasPreservedSnapshot)
        {
            cargo.CaptureBeforeSnapshot();
        }
    }

    /// <summary>
    /// After ship cargo is updated, compute the squadron FC cargo delta from the frozen
    /// before-snapshot (or the last pre-replace inventory) and send it to Raven Colonial.
    /// </summary>
    public async Task<string?> SynchronizeSquadronFleetCarrierCargoDiffAsync(
        CargoInventoryState cargo,
        bool cargoActivity)
    {
        ArgumentNullException.ThrowIfNull(cargo);
        if (!FleetCarrierCargoSyncEnabled
            || storedRavenApiKey is null
            || constructionState.CurrentDock is not { } dock)
        {
            skipNextCargoEvent = false;
            if (cargo.HasPreservedSnapshot)
            {
                cargo.ClearPreservedSnapshot();
            }

            return null;
        }

        // MarketBuy/Sell already adjusted the FC. Suppress GetDiff only when there is
        // no preserved transfer snapshot — transfer capture baselines after market
        // events, so its GetDiff must still be sent (same-poll Market+Transfer).
        if (skipNextCargoEvent)
        {
            skipNextCargoEvent = false;
            if (!cargo.HasPreservedSnapshot)
            {
                return null;
            }
        }

        if (!IsLinkedSquadronFleetCarrier(dock))
        {
            if (cargo.HasPreservedSnapshot)
            {
                cargo.ClearPreservedSnapshot();
            }

            return null;
        }

        // Match legacy: only run after Cargo activity / preserved transfer snapshot.
        if (!cargoActivity && !cargo.HasPreservedSnapshot)
        {
            return null;
        }

        // Compute diff while inventory is stable; network I/O stays outside GetDiff's lock.
        var shipDiff = cargo.GetDiff();
        if (shipDiff.Count == 0)
        {
            return null;
        }

        var adjustments = ColonizationFleetCarrierCargoSynchronizer
            .CreateSquadronCargoDiffAdjustment(shipDiff);

        CommodityOverlay.ApplyPendingFleetCarrierCargo(adjustments.Keys);
        try
        {
            var updatedCargo = await client.AdjustFleetCarrierCargoAsync(
                dock.MarketId,
                adjustments,
                storedRavenApiKey,
                CancellationToken.None);
            var localCarrier = fleetCarriers.FirstOrDefault(carrier =>
                carrier.MarketId == dock.MarketId);
            if (localCarrier is not null)
            {
                ReplaceLocalFleetCarrier(localCarrier with
                {
                    Cargo = updatedCargo.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase),
                });
            }

            return $"Updated {adjustments.Count:N0} linked squadron Fleet Carrier cargo entry(s) from ship cargo diff.";
        }
        finally
        {
            CommodityOverlay.ApplyPendingFleetCarrierCargo(null);
        }
    }

    private async Task<string?> SynchronizeFleetCarrierCargoAdjustmentAsync(
        JournalEventEnvelope journalEvent,
        bool preferShipCargoDiffForSquadron,
        CargoInventoryState? cargoInventory = null)
    {
        if (!FleetCarrierCargoSyncEnabled
            || storedRavenApiKey is null
            || constructionState.CurrentDock is not { } dock
            || !fleetCarriers.Any(carrier => carrier.MarketId == dock.MarketId))
        {
            return null;
        }

        var adjustments = ColonizationFleetCarrierCargoSynchronizer
            .CreateJournalAdjustment(
                journalEvent,
                dock,
                latestStatus?.InMainShip == true,
                preferShipCargoDiffForSquadron);
        if (adjustments.Count == 0)
        {
            return null;
        }

        CommodityOverlay.ApplyPendingFleetCarrierCargo(adjustments.Keys);
        try
        {
            var updatedCargo = await client.AdjustFleetCarrierCargoAsync(
                dock.MarketId,
                adjustments,
                storedRavenApiKey,
                CancellationToken.None);
            if (!preferShipCargoDiffForSquadron
                && IsLinkedSquadronFleetCarrier(dock))
            {
                cargoInventory?.ClearPreservedSnapshot();
            }
            var localCarrier = fleetCarriers.FirstOrDefault(carrier =>
                carrier.MarketId == dock.MarketId);
            if (localCarrier is not null)
            {
                ReplaceLocalFleetCarrier(localCarrier with
                {
                    Cargo = updatedCargo.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase),
                });
            }

            // Market buy/sell already adjusted the FC; suppress market-only GetDiff so
            // squadron carriers are not double-counted. Transfer snapshots still send.
            if (preferShipCargoDiffForSquadron
                && journalEvent.EventName is "MarketBuy" or "MarketSell"
                && ColonizationFleetCarrierCargoSynchronizer.IsSquadronFleetCarrier(dock))
            {
                skipNextCargoEvent = true;
            }

            return $"Updated {adjustments.Count:N0} linked Fleet Carrier cargo entry(s) from {journalEvent.EventName}.";
        }
        finally
        {
            CommodityOverlay.ApplyPendingFleetCarrierCargo(null);
        }
    }

    private bool IsLinkedSquadronFleetCarrier(ColonizationDockingSnapshot dock)
    {
        return string.Equals(
                dock.StationType,
                "FleetCarrier",
                StringComparison.OrdinalIgnoreCase)
            && ColonizationFleetCarrierCargoSynchronizer.IsSquadronFleetCarrier(dock)
            && fleetCarriers.Any(carrier => carrier.MarketId == dock.MarketId);
    }

    private async Task<string?> SynchronizeDockedProjectAsync(
        JournalEventEnvelope journalEvent)
    {
        var parser = new ColonizationConstructionState();
        if (!parser.Apply(journalEvent)
            || parser.CurrentDock is not { IsConstructionSite: true } dock)
        {
            return null;
        }

        var project = await FindOrLoadProjectAsync(
            dock.SystemAddress,
            dock.MarketId);
        if (project is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(dock.FactionName)
            || string.Equals(
                dock.FactionName,
                project.FactionName,
                StringComparison.Ordinal))
        {
            return localUntrackedProject?.BuildId == project.BuildId
                ? $"Loaded untracked Raven project {project.BuildName} for this construction site."
                : null;
        }

        var updated = await client.UpdateProjectAsync(
            new ColonizationProjectUpdate
            {
                BuildId = project.BuildId,
                FactionName = dock.FactionName,
            },
            CancellationToken.None);
        UpsertProject(updated);
        return $"Updated Raven project faction for {updated.BuildName}.";
    }

    private async Task<string?> SynchronizeBuildSiteRepairAsync(
        JournalEventEnvelope journalEvent)
    {
        if (storedRavenApiKey is null)
        {
            return null;
        }

        var root = journalEvent.Payload;
        var stationName = GetJournalString(root, "StationName");
        var stationType = GetJournalString(root, "StationType");
        var systemAddress = GetJournalInt64(root, "SystemAddress");
        var marketId = GetJournalInt64(root, "MarketID");
        var isConstructionShip = stationName?.Contains(
            "ColonisationShip",
            StringComparison.Ordinal) == true;
        if (systemAddress is not > 0
            || marketId is not > 0
            || string.IsNullOrWhiteSpace(stationName)
            || !ColonizationBuildSiteRepair.IsPlayerColonyMarketId(
                marketId.Value)
            || ColonizationBuildSiteRepair.ShouldSkipDockContext(
                stationType,
                stationName,
                isConstructionShip))
        {
            return null;
        }

        var stationKey = ColonizationBuildSiteRepair
            .NormalizeDockStationName(stationName)
            .ToLowerInvariant();
        if (stationKey.Length == 0)
        {
            return null;
        }

        var visit = new ColonizationBuildSiteRepairVisit(
            marketId.Value,
            stationKey);
        var inFlight = (systemAddress.Value, marketId.Value, stationKey);
        lock (buildSiteRepairLock)
        {
            if (buildSiteRepairVisitSet.Contains(visit)
                || !buildSiteRepairsInFlight.Add(inFlight))
            {
                return null;
            }
        }

        try
        {
            var sites = await GetSystemSitesForRepairAsync(
                systemAddress.Value);
            var plan = ColonizationBuildSiteRepair.CreatePlan(
                sites,
                stationName,
                marketId.Value);
            if (plan is null || string.IsNullOrWhiteSpace(plan.Site.Id))
            {
                return null;
            }

            await client.PatchSystemSiteAsync(
                systemAddress.Value.ToString(CultureInfo.InvariantCulture),
                plan.Site.Id,
                plan.CreatePatch(),
                storedRavenApiKey,
                CancellationToken.None);
            RememberBuildSiteRepairVisit(visit);
            return plan.Field == ColonizationBuildSiteRepairField.MarketId
                ? $"Repaired Raven Market Info for {plan.NormalizedStationName}."
                : $"Repaired the Raven site name for {plan.NormalizedStationName}.";
        }
        finally
        {
            lock (buildSiteRepairLock)
            {
                buildSiteRepairsInFlight.Remove(inFlight);
            }
        }
    }

    private async Task<IReadOnlyList<ColonizationSystemSite>>
        GetSystemSitesForRepairAsync(long systemAddress)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await client.GetSystemSitesAsync(
                    systemAddress.ToString(CultureInfo.InvariantCulture),
                    CancellationToken.None);
            }
            catch (Exception exception) when (
                attempt < 2
                && exception is HttpRequestException or TaskCanceledException)
            {
                await delayAsync(
                    TimeSpan.FromSeconds(1.5 * (attempt + 1)),
                    CancellationToken.None);
                attempt++;
            }
        }
    }

    private void RememberBuildSiteRepairVisit(
        ColonizationBuildSiteRepairVisit visit)
    {
        lock (buildSiteRepairLock)
        {
            if (!buildSiteRepairVisitSet.Add(visit))
            {
                return;
            }

            if (buildSiteRepairVisits.Count
                == MaximumBuildSiteRepairVisits)
            {
                buildSiteRepairVisitSet.Remove(
                    buildSiteRepairVisits.Dequeue());
            }

            buildSiteRepairVisits.Enqueue(visit);
            try
            {
                settingsStore.SaveBuildSiteRepairVisits(
                    buildSiteRepairVisits);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                // The server repair succeeded; a cache write failure must not
                // report that successful remote change as failed.
            }
        }
    }

    private async Task<string?> SynchronizeContributionAsync(
        JournalEventEnvelope journalEvent)
    {
        var marketId = GetJournalInt64(journalEvent.Payload, "MarketID");
        var contributions = ReadJournalContributions(journalEvent.Payload);
        if (marketId is not > 0 || contributions.Count == 0)
        {
            return null;
        }

        var dock = constructionState.CurrentDock;
        var project = await FindOrLoadProjectAsync(
            dock?.MarketId == marketId
                ? dock.SystemAddress
                : currentSystemAddress,
            marketId.Value);
        if (project is null)
        {
            return "Raven did not identify a project for the recorded construction contribution.";
        }

        await client.ContributeToProjectAsync(
            project.BuildId,
            CommanderName!,
            contributions,
            CancellationToken.None);
        return $"Published {contributions.Values.Sum(value => (long)value):N0} contributed cargo units to {project.BuildName}.";
    }

    private async Task<string?> SynchronizeDepotAsync(
        JournalEventEnvelope journalEvent)
    {
        var parser = new ColonizationConstructionState();
        if (!parser.Apply(journalEvent)
            || parser.CurrentDepot is not { } depot)
        {
            return null;
        }

        var dock = constructionState.CurrentDock;
        var project = await FindOrLoadProjectAsync(
            dock?.MarketId == depot.MarketId
                ? dock.SystemAddress
                : currentSystemAddress,
            depot.MarketId);
        if (project is null)
        {
            return "Raven did not identify a project for the current construction depot.";
        }

        var remaining = depot.Resources.ToDictionary(
            resource => resource.Name,
            resource => resource.RemainingAmount,
            StringComparer.OrdinalIgnoreCase);
        var maximumRequiredLong = depot.Resources.Sum(resource =>
            (long)resource.RequiredAmount);
        if (maximumRequiredLong > int.MaxValue)
        {
            return "Raven project sync rejected construction requirements above the supported total.";
        }

        var maximumRequired = (int)maximumRequiredLong;
        var updated = project;
        if (project.MaximumRequired != maximumRequired
            || !DictionariesEqual(project.Commodities, remaining)
            || depot.IsFailed)
        {
            updated = await client.UpdateProjectAsync(
                new ColonizationProjectUpdate
                {
                    BuildId = project.BuildId,
                    MaximumRequired = maximumRequired,
                    Commodities = remaining,
                    ConstructionDepot =
                        ColonizationConstructionDepotPayload.FromSnapshot(
                            depot),
                },
                CancellationToken.None);
            UpsertProject(updated);
        }

        if (depot.IsComplete && !updated.IsComplete)
        {
            await client.MarkProjectCompleteAsync(
                updated.BuildId,
                CancellationToken.None);
            updated = updated with
            {
                IsComplete = true,
                RemainingRequired = 0,
                Commodities = new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase),
            };
            UpsertProject(updated);
            return $"Marked Raven project {updated.BuildName} complete.";
        }

        return updated == project
            ? null
            : $"Updated Raven construction requirements for {updated.BuildName}.";
    }

    private async Task<ColonizationProject?> FindOrLoadProjectAsync(
        long? systemAddress,
        long marketId)
    {
        var project = Projects
            .Select(row => row.Project)
            .FirstOrDefault(candidate => candidate.MarketId == marketId)
            ?? (localUntrackedProject?.MarketId == marketId
                ? localUntrackedProject
                : null);
        if (project is not null || systemAddress is not > 0)
        {
            return project;
        }

        project = await client.GetProjectAsync(
            systemAddress.Value,
            marketId,
            CancellationToken.None);
        if (project is not null)
        {
            localUntrackedProject = project;
            UpsertProject(project);
        }

        return project;
    }

    private void UpsertProject(ColonizationProject project)
    {
        if (localUntrackedProject?.BuildId == project.BuildId)
        {
            localUntrackedProject = project;
        }

        Projects = Projects
            .Select(row => row.Project)
            .Where(candidate => !string.Equals(
                candidate.BuildId,
                project.BuildId,
                StringComparison.OrdinalIgnoreCase))
            .Append(project)
            .OrderBy(candidate => candidate.SystemName)
            .ThenBy(candidate => candidate.BuildName)
            .Select(CreateRow)
            .ToArray();
        UpdateProjectSummary();
    }

    private static Dictionary<string, int> ReadJournalContributions(
        JsonElement root)
    {
        if (!root.TryGetProperty("Contributions", out var rows)
            || rows.ValueKind != JsonValueKind.Array)
        {
            return new Dictionary<string, int>();
        }

        var result = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows.EnumerateArray())
        {
            var name = ColonizationConstructionState.NormalizeCommodityName(
                GetJournalString(row, "Name"));
            var amount = GetJournalInt32(row, "Amount");
            if (name.Length > 0 && amount is > 0)
            {
                var existing = result.GetValueOrDefault(name);
                if (existing > int.MaxValue - amount.Value)
                {
                    return new Dictionary<string, int>();
                }

                result[name] = existing + amount.Value;
            }
        }

        return result;
    }

    private static bool DictionariesEqual(
        Dictionary<string, int> left,
        Dictionary<string, int> right)
    {
        return left.Count == right.Count
            && left.All(pair =>
                right.TryGetValue(pair.Key, out var value)
                && value == pair.Value);
    }

    public async Task UpdateCargoAsync(
        CargoSnapshot? cargo,
        bool publishCurrentShipCargo = true)
    {
        if (cargo is null || SharedCargoSuppressed)
        {
            return;
        }

        shipCargo = cargo;
        UpdateCommodityPlan();
        if (publishCurrentShipCargo)
        {
            await PublishCurrentShipCargoAsync(cargo);
        }
    }

    private async Task PublishCurrentShipCargoAsync(CargoSnapshot cargo)
    {
        if (!ShipCargoPublishingEnabled)
        {
            return;
        }

        var blockReason = GetShipCargoPublishingBlockReason();
        if (blockReason is not null)
        {
            ShipCargoPublishingStatus = blockReason;
            return;
        }

        var cargoCounts = cargo.Inventory
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => Math.Max(0, item.Count)),
                StringComparer.OrdinalIgnoreCase);
        IsShipCargoPublishingBusy = true;
        ShipCargoPublishingStatus = "Publishing current ship cargo...";
        try
        {
            await client.PublishCurrentShipAsync(
                new ColonizationCurrentShip
                {
                    CommanderName = CommanderName!,
                    Name = currentShipName ?? currentShipType!,
                    Type = currentShipType!,
                    MaximumCargo = constructionState.ShipCargoCapacity,
                    Cargo = cargoCounts,
                },
                storedRavenApiKey!,
                CancellationToken.None);
            ShipCargoPublishingStatus =
                $"Published {cargoCounts.Count:N0} ship cargo entries to Raven Colonial.";
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            ShipCargoPublishingStatus =
                "Ship cargo was not published: " + exception.Message;
        }
        finally
        {
            IsShipCargoPublishingBusy = false;
        }
    }

    public async Task UpdateMarketAsync(MarketSnapshot? market)
    {
        if (market is null)
        {
            return;
        }

        currentMarket = market;
        UpdateCommodityPlan();
        publishFleetCarrierCommand.RaiseCanExecuteChanged();
        syncFleetCarrierCargoCommand.RaiseCanExecuteChanged();
        if (FleetCarrierCargoSyncEnabled)
        {
            await SyncFleetCarrierCargoAsync(force: false);
        }
    }

    public void UpdateStatus(EliteStatus? status)
    {
        if (status is null)
        {
            return;
        }

        latestStatus = status;
        SystemEditor.UpdateStatus(status);
        UpdateCommodityPlan();
    }

    public void UpdateMusicTrack(string? musicTrack)
    {
        CommodityOverlay.UpdateMusicTrack(musicTrack);
    }

    public void UpdateSystemContext(
        string? systemName,
        GalacticCoordinate? position,
        long? systemAddress = null)
    {
        var nextSystemName = string.IsNullOrWhiteSpace(systemName)
            ? null
            : systemName.Trim();
        var nextSystemAddress = systemAddress is > 0
            ? systemAddress
            : null;
        var samePosition = position is GalacticCoordinate coordinate
            ? currentStarPosition.Count == 3
                && Math.Abs(currentStarPosition[0] - coordinate.X)
                    <= 0.0000001d
                && Math.Abs(currentStarPosition[1] - coordinate.Y)
                    <= 0.0000001d
                && Math.Abs(currentStarPosition[2] - coordinate.Z)
                    <= 0.0000001d
            : currentStarPosition.Count == 0;
        if (string.Equals(
                currentSystemName,
                nextSystemName,
                StringComparison.OrdinalIgnoreCase)
            && currentSystemAddress == nextSystemAddress
            && samePosition)
        {
            return;
        }

        currentSystemName = nextSystemName;
        currentSystemAddress = nextSystemAddress;
        currentStarPosition = position is GalacticCoordinate nextCoordinate
            ? [nextCoordinate.X, nextCoordinate.Y, nextCoordinate.Z]
            : [];
        UpdateProjectEditorContext();
        UpdateSystemEditorContext();
    }

    public void ReportLinkFailure(string message)
    {
        StatusMessage = "Raven Colonial could not be opened: " + message;
    }

    public void Dispose()
    {
        CancelDockingRefresh();
    }

    public async Task SaveRavenApiKeyAsync()
    {
        if (!CanSaveRavenApiKey()
            || commanderProfileStore is null
            || profileFrontierId is null)
        {
            return;
        }

        IsFleetCarrierSyncBusy = true;
        try
        {
            var normalized = string.IsNullOrWhiteSpace(RavenApiKey)
                ? null
                : RavenApiKey.Trim();
            string? validatedCommander = null;
            if (normalized is not null)
            {
                if (CommanderName is null)
                {
                    RavenCredentialStatus =
                        "Load the active commander before validating a Raven API key.";
                    return;
                }

                RavenCredentialStatus =
                    "Validating the Raven API key without saving it...";
                validatedCommander =
                    await client.GetCommanderByApiKeyAsync(
                        normalized,
                        CancellationToken.None);
                if (validatedCommander is null)
                {
                    RavenCredentialStatus =
                        "Raven rejected this API key. The saved key was not changed.";
                    return;
                }

                if (!string.Equals(
                        validatedCommander,
                        CommanderName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    RavenCredentialStatus =
                        $"This key belongs to {validatedCommander}, not "
                        + $"{CommanderName}. The saved key was not changed.";
                    return;
                }
            }

            await commanderProfileStore.SaveRavenColonialApiKeyAsync(
                profileFrontierId,
                CommanderName,
                profileIsOdyssey,
                normalized,
                CancellationToken.None);
            storedRavenApiKey = normalized;
            RavenApiKey = normalized ?? string.Empty;
            RavenCredentialStatus = normalized is null
                ? "The Raven API key was removed from this commander profile."
                : $"The Raven API key was validated for {validatedCommander} and saved.";
            OnPropertyChanged(nameof(HasStoredRavenApiKey));
            if (normalized is null && FleetCarrierCargoSyncEnabled)
            {
                FleetCarrierCargoSyncEnabled = false;
            }

            if (ShipCargoPublishingEnabled)
            {
                ShipCargoPublishingStatus = GetShipCargoReadyStatus();
            }

            UpdateProjectEditorContext();
            UpdateSystemEditorContext();
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidDataException
                or HttpRequestException
                or TaskCanceledException
                or ArgumentException)
        {
            RavenCredentialStatus =
                "The Raven API key was not saved: " + exception.Message;
        }
        finally
        {
            IsFleetCarrierSyncBusy = false;
            RaiseCommandStates();
        }
    }

    public async Task PublishCurrentFleetCarrierAsync()
    {
        if (!CanPublishCurrentFleetCarrier()
            || constructionState.CurrentDock is not { } dock
            || storedRavenApiKey is null)
        {
            FleetCarrierSyncStatus =
                GetFleetCarrierPublishBlockReason();
            return;
        }

        var published = false;
        IsFleetCarrierSyncBusy = true;
        FleetCarrierSyncStatus =
            $"Publishing {dock.StationName} to Raven Colonial...";
        try
        {
            var registered = await client.PublishFleetCarrierAsync(
                new ColonizationFleetCarrierRegistration
                {
                    MarketId = dock.MarketId,
                    Name = dock.StationName,
                    DisplayName = fleetCarrierIdentityTracker
                        .ResolveDisplayName(dock.StationName),
                },
                storedRavenApiKey,
                CancellationToken.None);
            published = true;
            registered = registered with
            {
                Cargo = registered.Cargo ?? new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase),
            };
            ReplaceLocalFleetCarrier(registered);

            var market = GetFreshFleetCarrierMarket(dock);
            if (market is null)
            {
                FleetCarrierSyncStatus =
                    $"Published and linked {GetCarrierName(registered)}. "
                    + "Open its commodity market to synchronize cargo.";
                return;
            }

            var replacements = ColonizationFleetCarrierCargoSynchronizer
                .CreateMarketReplacement(market, registered);
            if (replacements.Count == 0)
            {
                lastSyncedMarket = (market.MarketId, market.Timestamp);
                FleetCarrierSyncStatus =
                    $"Published and linked {GetCarrierName(registered)}; "
                    + "its cargo is already current.";
                return;
            }

            CommodityOverlay.ApplyPendingFleetCarrierCargo(
                replacements.Keys);
            var updatedCargo = await client.ReplaceFleetCarrierCargoAsync(
                dock.MarketId,
                replacements,
                storedRavenApiKey,
                CancellationToken.None);
            ReplaceLocalFleetCarrier(registered with
            {
                Cargo = updatedCargo.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.OrdinalIgnoreCase),
            });
            lastSyncedMarket = (market.MarketId, market.Timestamp);
            FleetCarrierSyncStatus =
                $"Published and linked {GetCarrierName(registered)} and "
                + $"updated {replacements.Count:N0} cargo entries.";
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            FleetCarrierSyncStatus = published
                ? "The Fleet Carrier was linked, but its current cargo was not updated: "
                    + exception.Message
                : "The Fleet Carrier was not published: " + exception.Message;
        }
        finally
        {
            CommodityOverlay.ApplyPendingFleetCarrierCargo(null);
            IsFleetCarrierSyncBusy = false;
        }
    }

    public async Task SyncFleetCarrierCargoAsync(bool force = true)
    {
        if (!TryBeginFleetCarrierCargoSync(
                force,
                out var market,
                out var apiKey,
                out var identity,
                out var localCarrier))
        {
            return;
        }

        IsFleetCarrierSyncBusy = true;
        FleetCarrierSyncStatus =
            $"Checking {GetCarrierName(localCarrier)} market cargo...";
        try
        {
            await ApplyFleetCarrierMarketCargoSyncAsync(
                market,
                apiKey,
                identity);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            FleetCarrierSyncStatus =
                "Fleet Carrier cargo was not updated: " + exception.Message;
        }
        finally
        {
            CommodityOverlay.ApplyPendingFleetCarrierCargo(null);
            IsFleetCarrierSyncBusy = false;
        }
    }

    private bool TryBeginFleetCarrierCargoSync(
        bool force,
        out MarketSnapshot market,
        out string apiKey,
        out (long MarketId, DateTimeOffset Timestamp) identity,
        out ColonizationFleetCarrier localCarrier)
    {
        market = null!;
        apiKey = null!;
        identity = default;
        localCarrier = null!;
        if (!CanSyncFleetCarrierCargo()
            || currentMarket is null
            || storedRavenApiKey is null)
        {
            if (force)
            {
                FleetCarrierSyncStatus = GetFleetCarrierSyncBlockReason();
            }

            return false;
        }

        identity = (currentMarket.MarketId, currentMarket.Timestamp);
        if (!force && lastSyncedMarket == identity)
        {
            return false;
        }

        var carrier = fleetCarriers.FirstOrDefault(candidate =>
            candidate.MarketId == currentMarket.MarketId);
        if (carrier is null)
        {
            if (force)
            {
                FleetCarrierSyncStatus = GetFleetCarrierSyncBlockReason();
            }

            return false;
        }

        market = currentMarket;
        apiKey = storedRavenApiKey;
        localCarrier = carrier;
        return true;
    }

    private async Task ApplyFleetCarrierMarketCargoSyncAsync(
        MarketSnapshot market,
        string apiKey,
        (long MarketId, DateTimeOffset Timestamp) identity)
    {
        var serverCarrier = await client.GetFleetCarrierAsync(
            market.MarketId,
            CancellationToken.None);
        if (serverCarrier is null)
        {
            FleetCarrierSyncStatus =
                "Raven Colonial does not have this Fleet Carrier.";
            return;
        }

        // Re-resolve after await: the local list may have changed while waiting.
        var localCarrier = fleetCarriers.FirstOrDefault(carrier =>
            carrier.MarketId == market.MarketId);

        var replacements =
            ColonizationFleetCarrierCargoSynchronizer
                .CreateMarketReplacement(market, serverCarrier);
        if (replacements.Count == 0)
        {
            if (localCarrier is not null)
            {
                ReplaceLocalFleetCarrier(serverCarrier);
            }

            lastSyncedMarket = identity;
            FleetCarrierSyncStatus =
                $"{GetCarrierName(serverCarrier)} cargo is already current.";
            return;
        }

        CommodityOverlay.ApplyPendingFleetCarrierCargo(replacements.Keys);
        FleetCarrierSyncStatus =
            $"Updating {replacements.Count:N0} cargo entries for "
            + GetCarrierName(serverCarrier)
            + "...";
        var updatedCargo = await client.ReplaceFleetCarrierCargoAsync(
            market.MarketId,
            replacements,
            apiKey,
            CancellationToken.None);
        localCarrier = fleetCarriers.FirstOrDefault(carrier =>
            carrier.MarketId == market.MarketId);
        if (localCarrier is not null)
        {
            ReplaceLocalFleetCarrier(serverCarrier with
            {
                Cargo = updatedCargo.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.OrdinalIgnoreCase),
            });
        }

        lastSyncedMarket = identity;
        FleetCarrierSyncStatus =
            $"Updated {replacements.Count:N0} cargo entries for "
            + GetCarrierName(serverCarrier)
            + ".";
    }

    public Task RefreshAsync()
    {
        return RefreshAsync(CancellationToken.None);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (!IsEnabled || IsBusy || CommanderName is null)
        {
            return;
        }

        var commander = CommanderName;

        if (Projects.Count == 0)
        {
            await RestoreLegacyProfileAsync();
        }

        IsBusy = true;
        StatusMessage = "Fetching active projects from Raven Colonial...";
        try
        {
            var result = await client.GetCommanderProjectsAsync(
                commander,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(
                    CommanderName,
                    commander,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            hiddenProjectIds = result.HiddenProjectIds.ToHashSet(
                StringComparer.OrdinalIgnoreCase);
            primaryProjectId = result.PrimaryProjectId;
            fleetCarriers = result.FleetCarriers;
            localUntrackedProject = null;
            Projects = result.Projects
                .OrderBy(project => project.SystemName)
                .ThenBy(project => project.BuildName)
                .Select(CreateRow)
                .ToArray();
            HasUnsavedProjectVisibility = false;
            UpdateProjectSummary();
            StatusMessage = Projects.Count switch
            {
                0 => "No active Raven Colonial projects were found for this commander.",
                1 => "Loaded 1 active Raven Colonial project.",
                _ => $"Loaded {Projects.Count:N0} active Raven Colonial projects.",
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException)
        {
            StatusMessage = "Project refresh failed without changing your selection: "
                + exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RestoreLegacyProfileAsync()
    {
        if (legacyProfileStore is null
            || profileFrontierId is null
            || CommanderName is null)
        {
            return;
        }

        var result = await legacyProfileStore.LoadAsync(
            profileFrontierId,
            CancellationToken.None);
        if (result.Error is not null)
        {
            StatusMessage = "The imported colonisation cache could not be read: "
                + result.Error;
            return;
        }

        if (result.Snapshot is not { } snapshot)
        {
            return;
        }

        hiddenProjectIds = snapshot.HiddenProjectIds.ToHashSet(
            StringComparer.OrdinalIgnoreCase);
        primaryProjectId = snapshot.PrimaryProjectId;
        fleetCarriers = snapshot.FleetCarriers;
        Projects = snapshot.Projects
            .OrderBy(project => project.SystemName)
            .ThenBy(project => project.BuildName)
            .Select(CreateRow)
            .ToArray();
        HasUnsavedProjectVisibility = false;
        UpdateProjectSummary();
        var warning = result.Warnings.Count == 0
            ? string.Empty
            : $" Ignored {result.Warnings.Count:N0} invalid cached item(s).";
        StatusMessage = $"Restored {Projects.Count:N0} imported colonisation "
            + $"project(s) from {Path.GetFileName(result.Path)}.{warning}";
    }

    public async Task SaveProjectVisibilityAsync()
    {
        if (!IsEnabled
            || IsBusy
            || CommanderName is null
            || !HasUnsavedProjectVisibility)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "Saving project visibility to Raven Colonial...";
        try
        {
            var saved = await client.SaveHiddenProjectIdsAsync(
                CommanderName,
                hiddenProjectIds,
                CancellationToken.None);
            hiddenProjectIds = saved.ToHashSet(
                StringComparer.OrdinalIgnoreCase);
            foreach (var row in Projects)
            {
                row.UpdateShown(!hiddenProjectIds.Contains(row.Project.BuildId));
            }

            HasUnsavedProjectVisibility = false;
            UpdateProjectSummary();
            StatusMessage = "Project visibility saved to Raven Colonial.";
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException)
        {
            StatusMessage = "Project visibility was not saved: "
                + exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private ColonizationProjectRowViewModel CreateRow(
        ColonizationProject project)
    {
        var matchingBuilds = buildCatalog.FindByLayout(project.BuildType);
        var build = (matchingBuilds.Count > 0 ? matchingBuilds[0] : null)
            ?? buildCatalog.FindByBuildType(project.BuildType);
        var type = project.IsFleetCarrierLoading
            ? "Fleet Carrier loading"
            : build is null
                ? project.BuildType
                : $"{build.DisplayName} ({project.BuildType})";
        return new ColonizationProjectRowViewModel(
            project,
            type,
            string.Equals(
                project.BuildId,
                primaryProjectId,
                StringComparison.OrdinalIgnoreCase),
            !hiddenProjectIds.Contains(project.BuildId),
            OnProjectShownChanged,
            TogglePrimaryProjectAsync);
    }

    public async Task TogglePrimaryProjectAsync(
        ColonizationProjectRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (!IsEnabled || IsBusy || CommanderName is null)
        {
            return;
        }

        var nextPrimaryId = row.IsPrimary ? null : row.Project.BuildId;
        IsBusy = true;
        StatusMessage = nextPrimaryId is null
            ? "Clearing the primary Raven Colonial project..."
            : $"Setting {row.BuildName} as the primary Raven Colonial project...";
        try
        {
            await client.SetPrimaryProjectAsync(
                CommanderName,
                nextPrimaryId,
                CancellationToken.None);
            primaryProjectId = nextPrimaryId;
            Projects = Projects
                .Select(project => project.Project)
                .OrderBy(project => project.SystemName)
                .ThenBy(project => project.BuildName)
                .Select(CreateRow)
                .ToArray();
            StatusMessage = nextPrimaryId is null
                ? "The primary Raven Colonial project was cleared."
                : $"{row.BuildName} is now the primary Raven Colonial project.";
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or InvalidDataException
                or TaskCanceledException
                or ArgumentException)
        {
            StatusMessage = "The primary project was not changed: "
                + exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task OnProjectCreatedAsync(ColonizationProject project)
    {
        Projects = Projects
            .Where(row => !string.Equals(
                row.Project.BuildId,
                project.BuildId,
                StringComparison.OrdinalIgnoreCase))
            .Select(row => row.Project)
            .Append(project)
            .OrderBy(candidate => candidate.SystemName)
            .ThenBy(candidate => candidate.BuildName)
            .Select(CreateRow)
            .ToArray();
        UpdateProjectSummary();
        return Task.CompletedTask;
    }

    private void UpdateProjectEditorContext()
    {
        var snapshot = constructionState.CreateSnapshot();
        ProjectEditor.UpdateContext(new ColonizationProjectEditorContext(
            IsEnabled,
            CommanderName,
            currentSystemName,
            currentStarPosition,
            snapshot.CurrentDock,
            snapshot.CurrentDepot,
            storedRavenApiKey));
    }

    private void UpdateSystemEditorContext()
    {
        SystemEditor.UpdateContext(new ColonizationSystemEditorContext(
            IsEnabled,
            CommanderName,
            currentSystemName,
            currentSystemAddress,
            storedRavenApiKey));
    }

    private void OnProjectShownChanged(
        ColonizationProjectRowViewModel row,
        bool isShown)
    {
        if (isShown)
        {
            hiddenProjectIds.Remove(row.Project.BuildId);
        }
        else
        {
            hiddenProjectIds.Add(row.Project.BuildId);
        }

        HasUnsavedProjectVisibility = true;
        UpdateProjectSummary();
    }

    private void UpdateProjectSummary()
    {
        var totals = ColonizationProjectCalculator.CalculateTotals(
            Projects.Select(row => row.Project),
            hiddenProjectIds,
            constructionState.ShipCargoCapacity);
        var trips = totals.TripsInCurrentShip is long tripCount
            ? $" | {tripCount:N0} trips in current ship"
            : string.Empty;
        ProjectSummary = $"Cargo required: {totals.RemainingCargo:N0}"
            + trips;
        UpdateCommodityPlan();
    }

    private void UpdateConstructionDisplay()
    {
        var snapshot = constructionState.CreateSnapshot();
        if (snapshot.CurrentDock is null)
        {
            ConstructionTitle = "No construction depot active";
            ConstructionStatus =
                "Dock at a construction site and open Construction Services.";
            ConstructionResources = [];
            return;
        }

        ConstructionTitle = snapshot.CurrentDock.StationName;
        if (snapshot.CurrentDepot is null)
        {
            ConstructionStatus = snapshot.CurrentDock.IsConstructionSite
                ? "Open Construction Services to load current requirements."
                : "The current station is not a colonisation construction site.";
            ConstructionResources = [];
            return;
        }

        var depot = snapshot.CurrentDepot;
        ConstructionStatus = depot.IsComplete
            ? "Construction complete."
            : (depot.IsFailed) switch
            {
                true => "Construction failed.",
                false => $"{depot.ReportedProgress:P1} complete | "
                                                                  + $"{depot.TotalRemaining:N0} cargo remaining"
            };
        ConstructionResources = depot.Resources
            .OrderByDescending(resource => resource.RemainingAmount)
            .ThenBy(resource => resource.LocalizedName)
            .Select(resource => new ColonizationResourceRowViewModel(
                resource.LocalizedName,
                resource.RemainingAmount,
                resource.ProvidedAmount,
                resource.RequiredAmount,
                resource.Payment))
            .ToArray();
    }

    private void ClearProjects()
    {
        Projects = [];
        fleetCarriers = [];
        localUntrackedProject = null;
        hiddenProjectIds.Clear();
        primaryProjectId = null;
        HasUnsavedProjectVisibility = false;
        UpdateProjectSummary();
        syncFleetCarrierCargoCommand.RaiseCanExecuteChanged();
    }

    private void UpdateCommodityPlan()
    {
        var construction = constructionState.CreateSnapshot();
        var dock = construction.CurrentDock;
        var hasMarketSinceDocking = currentMarket is not null
            && dock?.Timestamp is not null
            && dock.MarketId == currentMarket.MarketId
            && currentMarket.Timestamp > dock.Timestamp;
        CommodityOverlay.Apply(
            ColonizationCommodityPlanner.Create(
                Projects.Select(row => row.Project),
                hiddenProjectIds,
                primaryProjectId,
                CommanderName,
                fleetCarriers,
                shipCargo,
                construction,
                currentMarket),
            latestStatus,
            hasMarketSinceDocking,
            construction.IsSquadronBankOpen);
    }

    private string GetShipCargoReadyStatus()
    {
        if (SharedCargoSuppressed)
        {
            return "Ship cargo is paused while multiple Elite windows are running because Cargo.json cannot be attributed safely.";
        }

        if (!ShipCargoPublishingEnabled)
        {
            return "Automatic ship cargo publishing is off.";
        }

        if (!IsEnabled)
        {
            return "Enable Raven Colonial before publishing ship cargo.";
        }

        return HasStoredRavenApiKey
            ? "Ship cargo will publish after Cargo.json changes."
            : "Save a Raven API key before ship cargo can publish.";
    }

    private bool CanSaveRavenApiKey()
    {
        var normalized = string.IsNullOrWhiteSpace(RavenApiKey)
            ? null
            : RavenApiKey.Trim();
        return commanderProfileStore is not null
            && profileFrontierId is not null
            && !IsFleetCarrierSyncBusy
            && !string.Equals(
                normalized,
                storedRavenApiKey,
                StringComparison.Ordinal);
    }

    private bool CanPublishCurrentFleetCarrier()
    {
        return IsEnabled
            && HasStoredRavenApiKey
            && !IsFleetCarrierSyncBusy
            && constructionState.CurrentDock is
            {
                MarketId: > 0,
                StationType: not null,
            } dock
            && string.Equals(
                dock.StationType,
                "FleetCarrier",
                StringComparison.OrdinalIgnoreCase);
    }

    private string GetFleetCarrierPublishBlockReason()
    {
        if (!IsEnabled)
        {
            return "Enable Raven Colonial before publishing a Fleet Carrier.";
        }

        if (!HasStoredRavenApiKey)
        {
            return "Save a Raven API key before publishing a Fleet Carrier.";
        }

        return "Dock at the Fleet Carrier you want to publish and link.";
    }

    private MarketSnapshot? GetFreshFleetCarrierMarket(
        ColonizationDockingSnapshot dock)
    {
        return currentMarket is not null
            && dock.Timestamp is not null
            && currentMarket.MarketId == dock.MarketId
            && string.Equals(
                currentMarket.StationType,
                "FleetCarrier",
                StringComparison.OrdinalIgnoreCase)
            && currentMarket.Timestamp > dock.Timestamp
                ? currentMarket
                : null;
    }

    private bool CanSyncFleetCarrierCargo()
    {
        if (!IsEnabled
            || !FleetCarrierCargoSyncEnabled
            || !HasStoredRavenApiKey
            || IsFleetCarrierSyncBusy
            || currentMarket is null
            || !string.Equals(
                currentMarket.StationType,
                "FleetCarrier",
                StringComparison.OrdinalIgnoreCase)
            || !fleetCarriers.Any(carrier =>
                carrier.MarketId == currentMarket.MarketId))
        {
            return false;
        }

        var dock = constructionState.CurrentDock;
        return dock?.Timestamp is not null
            && dock.MarketId == currentMarket.MarketId
            && currentMarket.Timestamp > dock.Timestamp;
    }

    private string GetFleetCarrierSyncBlockReason()
    {
        if (!IsEnabled)
        {
            return "Enable Raven Colonial before syncing Fleet Carrier cargo.";
        }

        if (!FleetCarrierCargoSyncEnabled)
        {
            return "Automatic Fleet Carrier cargo sync is off.";
        }

        if (!HasStoredRavenApiKey)
        {
            return "Save a Raven API key before syncing Fleet Carrier cargo.";
        }

        if (currentMarket is null)
        {
            return "Open a Fleet Carrier commodity market in Elite first.";
        }

        if (!string.Equals(
                currentMarket.StationType,
                "FleetCarrier",
                StringComparison.OrdinalIgnoreCase))
        {
            return "The current market is not a Fleet Carrier market.";
        }

        if (!fleetCarriers.Any(carrier =>
                carrier.MarketId == currentMarket.MarketId))
        {
            return "The current Fleet Carrier is not linked to this commander in Raven Colonial.";
        }

        return "Dock at the Fleet Carrier and reopen its commodity market before syncing.";
    }

    private string? GetShipCargoPublishingBlockReason()
    {
        if (!IsEnabled)
        {
            return "Enable Raven Colonial before publishing ship cargo.";
        }

        if (storedRavenApiKey is null)
        {
            return "Save a Raven API key before ship cargo can publish.";
        }

        if (CommanderName is null)
        {
            return "Load a commander profile before publishing ship cargo.";
        }

        if (!Projects.Any(project => project.IsShown))
        {
            return "Ship cargo was not published because no visible colonisation projects are active.";
        }

        if (string.IsNullOrWhiteSpace(currentShipType))
        {
            return "Ship cargo is waiting for a Loadout journal event.";
        }

        return null;
    }

    private void ApplyShipIdentity(JournalEventEnvelope journalEvent)
    {
        var root = journalEvent.Payload;
        switch (journalEvent.EventName)
        {
            case "LoadGame":
            case "Loadout":
                currentShipType = GetJournalString(root, "Ship")
                    ?? currentShipType;
                currentShipName = GetJournalString(root, "ShipName")
                    ?? GetJournalString(root, "ShipIdent")
                    ?? currentShipName;
                break;

            case "ShipyardSwap":
                currentShipType = GetJournalString(root, "ShipType")
                    ?? currentShipType;
                break;
        }
    }

    private static string? GetJournalString(
        JsonElement root,
        string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()!.Trim()
                : null;
    }

    private static long? GetJournalInt64(
        JsonElement root,
        string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt64(out var result)
                ? result
                : null;
    }

    private static bool? GetJournalBoolean(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }

    private static string? CombineMessages(params string?[] messages)
    {
        var present = messages.Where(message =>
            !string.IsNullOrWhiteSpace(message)).ToArray();
        return present.Length == 0
            ? null
            : string.Join(Environment.NewLine, present);
    }

    private static int? GetJournalInt32(
        JsonElement root,
        string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var result)
                ? result
                : null;
    }

    private void ReplaceLocalFleetCarrier(
        ColonizationFleetCarrier updatedCarrier)
    {
        fleetCarriers = fleetCarriers
            .Where(carrier => carrier.MarketId != updatedCarrier.MarketId)
            .Append(updatedCarrier)
            .ToArray();
        UpdateCommodityPlan();
    }

    private static string GetCarrierName(ColonizationFleetCarrier carrier)
    {
        return string.IsNullOrWhiteSpace(carrier.DisplayName)
            ? carrier.Name
            : carrier.DisplayName;
    }

    private void SaveOverlayPreferences(
        ColonizationOverlayPreferences updatedPreferences,
        [CallerMemberName] string? propertyName = null)
    {
        if (updatedPreferences == overlayPreferences)
        {
            return;
        }

        try
        {
            settingsStore.SaveOverlayPreferences(updatedPreferences);
            overlayPreferences = updatedPreferences;
            CommodityOverlay.ApplyPreferences(updatedPreferences);
            OnPropertyChanged(propertyName);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or InvalidOperationException)
        {
            StatusMessage =
                "The construction overlay preference could not be saved: "
                + exception.Message;
        }
    }

    private void RaiseCommandStates()
    {
        refreshCommand.RaiseCanExecuteChanged();
        saveProjectsCommand.RaiseCanExecuteChanged();
        saveRavenApiKeyCommand.RaiseCanExecuteChanged();
        publishFleetCarrierCommand.RaiseCanExecuteChanged();
        syncFleetCarrierCargoCommand.RaiseCanExecuteChanged();
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

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    private sealed class AsyncCommand(
        Func<Task> execute,
        Func<bool> canExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute();

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                await execute();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public sealed class ColonizationProjectRowViewModel
    : INotifyPropertyChanged
{
    private readonly Action<ColonizationProjectRowViewModel, bool> changed;
    private bool isShown;

    public ColonizationProjectRowViewModel(
        ColonizationProject project,
        string typeDescription,
        bool isPrimary,
        bool isShown,
        Action<ColonizationProjectRowViewModel, bool> changed,
        Func<ColonizationProjectRowViewModel, Task> togglePrimary)
    {
        Project = project;
        TypeDescription = typeDescription;
        IsPrimary = isPrimary;
        this.isShown = isShown;
        this.changed = changed;
        TogglePrimaryCommand = new RowAsyncCommand(
            () => togglePrimary(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ColonizationProject Project { get; }

    public string BuildName => Project.BuildName;

    public string SystemName => Project.SystemName;

    public string TypeDescription { get; }

    public bool IsPrimary { get; }

    public string PrimaryLabel => IsPrimary ? "PRIMARY" : string.Empty;

    public string PrimaryActionLabel => IsPrimary
        ? "Clear primary"
        : "Make primary";

    public ICommand TogglePrimaryCommand { get; }

    public string ProgressText => Project.IsFleetCarrierLoading
        ? $"? of {Project.MaximumRequired:N0}"
        : Project.Progress switch
        {
            double progress => (progress * 100).ToString("0", CultureInfo.InvariantCulture)
                + "% of "
                + Project.MaximumRequired.ToString("N0", CultureInfo.CurrentCulture),
            null => "Progress unavailable"
        };

    public bool IsShown
    {
        get => isShown;
        set
        {
            if (value == isShown)
            {
                return;
            }

            isShown = value;
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(IsShown)));
            changed(this, value);
        }
    }

    internal void UpdateShown(bool value)
    {
        if (value == isShown)
        {
            return;
        }

        isShown = value;
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(IsShown)));
    }

    private sealed class RowAsyncCommand(Func<Task> execute) : ICommand
    {
        private bool isExecuting;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => !isExecuting;

        public async void Execute(object? parameter)
        {
            if (isExecuting)
            {
                return;
            }

            isExecuting = true;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            try
            {
                await execute();
            }
            finally
            {
                isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

public sealed record ColonizationResourceRowViewModel(
    string Name,
    int Remaining,
    int Provided,
    int Required,
    int Payment)
{
    public string RemainingText => $"{Remaining:N0} remaining";

    public string ProgressText => $"{Provided:N0} / {Required:N0}";

    public string PaymentText => $"{Payment:N0} CR/t";
}
