using SrvSurvey.Core.Colonization;
using SrvSurvey.Core.Journal;
using SrvSurvey.Core.Search;
using SrvSurvey.Core.Storage;
using SrvSurvey.Desktop.Configuration;
using SrvSurvey.Desktop.ViewModels;

namespace SrvSurvey.Desktop.Tests.ViewModels;

public sealed class ColonizationViewModelTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "SrvSurvey-colonization-view-model-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task DoesNotFetchWithoutExplicitConsent()
    {
        var client = new StubRavenColonialClient();
        var viewModel = Create(client);

        await viewModel.SetCommanderAsync("Test Cmdr");

        Assert.False(viewModel.IsEnabled);
        Assert.Equal(0, client.LoadCount);
        Assert.False(viewModel.RefreshCommand.CanExecute(null));
        Assert.Contains("off", viewModel.StatusMessage);
    }

    [Fact]
    public async Task LoadsProjectsAndCalculatesSelectedCargoTrips()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
            [
                Project("shown", "Port", remaining: 300),
                Project("hidden", "Hub", remaining: 100),
            ],
            ["hidden"],
            "shown",
            []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.ApplyJournalEvents(
            [Event("Loadout", "\"CargoCapacity\":128")]);

        await viewModel.SetCommanderAsync("Test Cmdr");

        Assert.Equal(1, client.LoadCount);
        Assert.Equal(2, viewModel.Projects.Count);
        Assert.True(viewModel.Projects.Single(row =>
            row.Project.BuildId == "shown").IsPrimary);
        Assert.False(viewModel.Projects.Single(row =>
            row.Project.BuildId == "hidden").IsShown);
        Assert.Equal(
            "Cargo required: 300 | 3 trips in current ship",
            viewModel.ProjectSummary);
    }

    [Fact]
    public async Task SavesProjectSelectionOnlyOnExplicitSave()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", remaining: 100)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");

        viewModel.Projects[0].IsShown = false;

        Assert.True(viewModel.HasUnsavedProjectVisibility);
        Assert.Equal(0, client.SaveCount);

        await viewModel.SaveProjectVisibilityAsync();

        Assert.Equal(1, client.SaveCount);
        Assert.Equal(["build-1"], client.LastSavedHiddenIds);
        Assert.False(viewModel.HasUnsavedProjectVisibility);
    }

    [Fact]
    public async Task SetsAndClearsPrimaryProjectThroughLegacyEndpoint()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", remaining: 100)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");

        await viewModel.TogglePrimaryProjectAsync(viewModel.Projects[0]);

        Assert.Equal("build-1", Assert.Single(client.PrimaryProjectRequests));
        Assert.True(Assert.Single(viewModel.Projects).IsPrimary);

        await viewModel.TogglePrimaryProjectAsync(viewModel.Projects[0]);

        Assert.Equal(2, client.PrimaryProjectRequests.Count);
        Assert.Null(client.PrimaryProjectRequests[1]);
        Assert.False(Assert.Single(viewModel.Projects).IsPrimary);
    }

    [Fact]
    public void ProjectsLiveConstructionDepotIntoResourceRows()
    {
        var viewModel = Create(new StubRavenColonialClient());

        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":10,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"Orbital Construction Site: Hope",
                "StationServices":["colonisationcontribution"]
                """),
            Event(
                "ColonisationConstructionDepot",
                """
                "MarketID":10,"ConstructionProgress":0.25,
                "ResourcesRequired":[
                  {"Name":"$steel_name;","Name_Localised":"Steel","RequiredAmount":100,"ProvidedAmount":25,"Payment":5000},
                  {"Name":"$water_name;","Name_Localised":"Water","RequiredAmount":10,"ProvidedAmount":9,"Payment":600}
                ]
                """),
        ]);

        Assert.Equal(
            "Orbital Construction Site: Hope",
            viewModel.ConstructionTitle);
        Assert.Equal(2, viewModel.ConstructionResources.Count);
        Assert.Equal("Steel", viewModel.ConstructionResources[0].Name);
        Assert.Equal("75 remaining",
            viewModel.ConstructionResources[0].RemainingText);
        Assert.Contains("76 cargo remaining", viewModel.ConstructionStatus);
    }

    [Fact]
    public async Task FeedsConsentedLiveContextIntoProjectEditor()
    {
        var viewModel = Create(new StubRavenColonialClient());
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.UpdateSystemContext(
            "Test",
            new GalacticCoordinate(1, 2, 3));

        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":10,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"Orbital Construction Site: Hope",
                "StationServices":["colonisationcontribution"]
                """),
            Event(
                "ColonisationConstructionDepot",
                """
                "MarketID":10,"ConstructionProgress":0.25,
                "ResourcesRequired":[
                  {"Name":"$steel_name;","Name_Localised":"Steel","RequiredAmount":100,"ProvidedAmount":25,"Payment":5000}
                ]
                """),
        ]);

        Assert.True(viewModel.ProjectEditor.CanPrepare);
        Assert.True(viewModel.ProjectEditor.PrepareCommand.CanExecute(null));
    }

    [Fact]
    public async Task FeedsConsentedCommanderAndAddressIntoSystemEditor()
    {
        var viewModel = Create(new StubRavenColonialClient());
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret");
        viewModel.UpdateSystemContext(
            "Test System",
            new GalacticCoordinate(1, 2, 3),
            systemAddress: 42);

        Assert.True(viewModel.SystemEditor.CanLoad);
        Assert.True(viewModel.SystemEditor.LoadCommand.CanExecute(null));
        Assert.Equal("Test System", viewModel.SystemEditor.SystemTitle);
    }

    [Fact]
    public async Task LiveConstructionEventsSynchronizeLegacyProjectMutations()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
            [
                Project(
                    "build-1",
                    "Port",
                    remaining: 100,
                    marketId: 10,
                    systemAddress: 20,
                    factionName: "Old faction"),
            ],
            [],
            null,
            []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var events = new[]
        {
            Event(
                "Docked",
                """
                "MarketID":10,"SystemAddress":20,"StarSystem":"Test System",
                "StationName":"Orbital Construction Site: Hope",
                "StationFaction":{"Name":"New faction"},
                "StationServices":["colonisationcontribution"]
                """),
            Event(
                "ColonisationContribution",
                """
                "MarketID":10,
                "Contributions":[{"Name":"$steel_name;","Amount":25}]
                """),
            Event(
                "ColonisationConstructionDepot",
                """
                "MarketID":10,"ConstructionProgress":0.25,
                "ResourcesRequired":[
                  {"Name":"$steel_name;","Name_Localised":"Steel","RequiredAmount":100,"ProvidedAmount":25,"Payment":5000}
                ]
                """),
        };
        viewModel.ApplyJournalEvents(events);

        await viewModel.SynchronizeLiveProjectsAsync(
            events,
            allowPublishing: true);

        Assert.Equal(2, client.ProjectUpdates.Count);
        Assert.Equal(
            "New faction",
            client.ProjectUpdates[0].FactionName);
        Assert.Equal(75, client.ProjectUpdates[1].Commodities!["steel"]);
        var contribution = Assert.Single(client.Contributions);
        Assert.Equal("build-1", contribution.BuildId);
        Assert.Equal("Test Cmdr", contribution.CommanderName);
        Assert.Equal(25, contribution.Commodities["steel"]);
        Assert.Equal(75, Assert.Single(viewModel.Projects)
            .Project.RemainingRequired);
        Assert.Contains("Updated Raven construction requirements", viewModel.StatusMessage);
    }

    [Fact]
    public async Task BootstrapNeverSynchronizesProjectMutations()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", 100, 10, 20)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var depot = Event(
            "ColonisationConstructionDepot",
            """
            "MarketID":10,"ConstructionProgress":0.25,
            "ResourcesRequired":[
              {"Name":"$steel_name;","RequiredAmount":100,"ProvidedAmount":25,"Payment":5000}
            ]
            """);
        viewModel.ApplyJournalEvents([depot]);

        await viewModel.SynchronizeLiveProjectsAsync(
            [depot],
            allowPublishing: false);

        Assert.Empty(client.ProjectUpdates);
        Assert.Empty(client.Contributions);
        Assert.Equal(0, client.MarkCompleteCount);
    }

    [Fact]
    public async Task LiveBeaconDeploymentRegistersCurrentCommanderAsArchitect()
    {
        var client = new StubRavenColonialClient();
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.UpdateSystemContext(
            "Test System",
            new GalacticCoordinate(1, 2, 3),
            systemAddress: 42);
        var beacon = Event(
            "ColonisationBeaconDeployed",
            string.Empty);
        viewModel.ApplyJournalEvents([beacon]);

        await viewModel.SynchronizeLiveProjectsAsync(
            [beacon],
            allowPublishing: true);

        var call = Assert.Single(client.SystemUpdates);
        Assert.Equal("Test System", call.SystemNameOrAddress);
        Assert.Equal("Test Cmdr", call.Update.Architect);
        Assert.Empty(call.Update.UpdatedSites);
        Assert.Empty(call.Update.DeletedSiteIds);
        Assert.Equal("secret-key", call.ApiKey);
        Assert.Contains("architect", viewModel.StatusMessage);
    }

    [Fact]
    public async Task BeaconArchitectUpdateRequiresLiveEventAndSavedKey()
    {
        var client = new StubRavenColonialClient();
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.UpdateSystemContext(
            "Test System",
            new GalacticCoordinate(1, 2, 3),
            systemAddress: 42);
        var beacon = Event(
            "ColonisationBeaconDeployed",
            string.Empty);

        await viewModel.SynchronizeLiveProjectsAsync(
            [beacon],
            allowPublishing: false);
        Assert.Empty(client.SystemUpdates);

        await viewModel.SynchronizeLiveProjectsAsync(
            [beacon],
            allowPublishing: true);
        Assert.Empty(client.SystemUpdates);
        Assert.Contains("no saved API key", viewModel.StatusMessage);
    }

    [Fact]
    public async Task CompletedDepotMarksProjectCompleteOnce()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", 25, 10, 20)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var events = new[]
        {
            Event(
                "Docked",
                """
                "MarketID":10,"SystemAddress":20,"StarSystem":"Test System",
                "StationName":"Orbital Construction Site: Hope",
                "StationServices":["colonisationcontribution"]
                """),
            Event(
                "ColonisationConstructionDepot",
                """
                "MarketID":10,"ConstructionProgress":1,
                "ConstructionComplete":true,
                "ResourcesRequired":[
                  {"Name":"$steel_name;","RequiredAmount":100,"ProvidedAmount":100,"Payment":5000}
                ]
                """),
        };
        viewModel.ApplyJournalEvents(events);

        await viewModel.SynchronizeLiveProjectsAsync(
            events,
            allowPublishing: true);

        Assert.Equal(1, client.MarkCompleteCount);
        Assert.True(Assert.Single(viewModel.Projects).Project.IsComplete);
        Assert.Contains("complete", viewModel.StatusMessage);
    }

    [Fact]
    public async Task DockingLoadsUntrackedProjectBySystemAndMarket()
    {
        var client = new StubRavenColonialClient
        {
            SiteProjectResponse = Project(
                "other-build",
                "Other port",
                50,
                10,
                20),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var docked = Event(
            "Docked",
            """
            "MarketID":10,"SystemAddress":20,"StarSystem":"Test System",
            "StationName":"Orbital Construction Site: Hope",
            "StationServices":["colonisationcontribution"]
            """);
        viewModel.ApplyJournalEvents([docked]);

        await viewModel.SynchronizeLiveProjectsAsync(
            [docked],
            allowPublishing: true);

        Assert.Equal(1, client.SiteProjectLoadCount);
        Assert.Equal("other-build", Assert.Single(viewModel.Projects)
            .Project.BuildId);
        Assert.Contains("untracked Raven project", viewModel.StatusMessage);
    }

    [Fact]
    public async Task DockedLocationRepairsSiteAndPersistsRepeatGuard()
    {
        var client = new StubRavenColonialClient
        {
            SystemSitesResponse =
            [
                new ColonizationSystemSite
                {
                    Id = "&4310842115",
                    Name = "Gold Enterprise",
                    Status = ColonizationSystemSiteStatus.Complete,
                },
            ],
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        var location = Event(
            "Location",
            """
            "Docked":true,"MarketID":4310842115,
            "SystemAddress":123456789,"StarSystem":"Test System",
            "StationName":"Gold Enterprise","StationType":"Dodec"
            """);

        await viewModel.SynchronizeLiveProjectsAsync(
            [location],
            allowPublishing: true);
        await viewModel.SynchronizeLiveProjectsAsync(
            [location],
            allowPublishing: true);

        Assert.Equal(1, client.SystemSiteLoadCount);
        var patch = Assert.Single(client.SystemSitePatches);
        Assert.Equal("123456789", patch.SystemNameOrAddress);
        Assert.Equal("&4310842115", patch.SiteId);
        Assert.Equal(4_310_842_115, patch.Patch.MarketId);
        Assert.Null(patch.Patch.Name);
        Assert.Equal("secret-key", patch.ApiKey);
        Assert.Contains("Repaired Raven Market Info", viewModel.StatusMessage);

        var reloadedClient = new StubRavenColonialClient
        {
            SystemSitesResponse = client.SystemSitesResponse,
        };
        var reloaded = Create(reloadedClient);
        reloaded.IsEnabled = true;
        reloaded.SetCommanderProfile("F123", true, "secret-key");
        await reloaded.SetCommanderAsync("Test Cmdr");

        await reloaded.SynchronizeLiveProjectsAsync(
            [location],
            allowPublishing: true);

        Assert.Equal(0, reloadedClient.SystemSiteLoadCount);
        Assert.Empty(reloadedClient.SystemSitePatches);
    }

    [Fact]
    public async Task FailedOrNoMatchSiteRepairCanRetryLater()
    {
        var delays = new List<TimeSpan>();
        var client = new StubRavenColonialClient();
        client.SystemSiteFailures.Enqueue(
            new HttpRequestException("temporary one"));
        client.SystemSiteFailures.Enqueue(
            new HttpRequestException("temporary two"));
        var viewModel = Create(
            client,
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        var docked = Event(
            "Docked",
            """
            "MarketID":4310999999,"SystemAddress":20,
            "StationName":"Dampier Gateway","StationType":"Outpost"
            """);

        await viewModel.SynchronizeLiveProjectsAsync(
            [docked],
            allowPublishing: true);

        Assert.Equal(
            [TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3)],
            delays);
        Assert.Equal(3, client.SystemSiteLoadCount);
        Assert.Empty(client.SystemSitePatches);

        client.SystemSitesResponse =
        [
            new ColonizationSystemSite
            {
                Id = "x1",
                Name = "Dampier Gateway",
                MarketId = 3_963_024_386,
                Status = ColonizationSystemSiteStatus.Complete,
            },
        ];
        await viewModel.SynchronizeLiveProjectsAsync(
            [docked],
            allowPublishing: true);

        Assert.Equal(4, client.SystemSiteLoadCount);
        Assert.Equal(
            4_310_999_999,
            Assert.Single(client.SystemSitePatches).Patch.MarketId);
    }

    [Fact]
    public async Task SiteRepairHonorsBootstrapCredentialAndDockSafetyGates()
    {
        var client = new StubRavenColonialClient();
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var completedPort = Event(
            "Docked",
            """
            "MarketID":4310999999,"SystemAddress":20,
            "StationName":"Dampier Gateway","StationType":"Outpost"
            """);

        await viewModel.SynchronizeLiveProjectsAsync(
            [completedPort],
            allowPublishing: true);
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        await viewModel.SynchronizeLiveProjectsAsync(
            [completedPort],
            allowPublishing: false);
        await viewModel.SynchronizeLiveProjectsAsync(
            [Event(
                "Docked",
                """
                "MarketID":4310999999,"SystemAddress":20,
                "StationName":"ABC-123","StationType":"FleetCarrier"
                """)],
            allowPublishing: true);
        await viewModel.SynchronizeLiveProjectsAsync(
            [Event(
                "Location",
                """
                "Docked":false,"MarketID":4310999999,"SystemAddress":20,
                "StationName":"Dampier Gateway","StationType":"Outpost"
                """)],
            allowPublishing: true);

        Assert.Equal(0, client.SystemSiteLoadCount);
        Assert.Empty(client.SystemSitePatches);
    }

    [Fact]
    public async Task DockingPermissionRefreshesActiveCommanderProjectsAfterLegacyDelay()
    {
        var delays = new List<TimeSpan>();
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", remaining: 100)],
                [],
                null,
                []),
        };
        using var viewModel = Create(
            client,
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        Assert.Equal(1, client.LoadCount);

        await viewModel.SynchronizeLiveProjectsAsync(
            [Event("DockingGranted", "\"StationName\":\"Regular port\"")],
            allowPublishing: true);

        Assert.Equal([TimeSpan.FromSeconds(4)], delays);
        Assert.Equal(2, client.LoadCount);
    }

    [Fact]
    public async Task ConstructionSiteDockingPermissionRefreshesWithoutExistingProject()
    {
        var client = new StubRavenColonialClient();
        using var viewModel = Create(client, (_, _) => Task.CompletedTask);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        Assert.Empty(viewModel.Projects);

        await viewModel.SynchronizeLiveProjectsAsync(
            [Event(
                "DockingGranted",
                "\"StationName\":\"Orbital Construction Site: Hope\"")],
            allowPublishing: true);

        Assert.Equal(2, client.LoadCount);
    }

    [Fact]
    public async Task DockingPermissionDoesNotRefreshDuringBootstrapOrAtUnrelatedPort()
    {
        var delays = 0;
        var client = new StubRavenColonialClient();
        using var viewModel = Create(
            client,
            (_, _) =>
            {
                delays++;
                return Task.CompletedTask;
            });
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        var granted = Event(
            "DockingGranted",
            "\"StationName\":\"Regular port\"");

        await viewModel.SynchronizeLiveProjectsAsync(
            [granted],
            allowPublishing: false);
        await viewModel.SynchronizeLiveProjectsAsync(
            [granted],
            allowPublishing: true);

        Assert.Equal(0, delays);
        Assert.Equal(1, client.LoadCount);
    }

    [Fact]
    public async Task RefreshFailureKeepsExistingProjectRows()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", remaining: 100)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        client.Failure = new HttpRequestException("offline");

        await viewModel.RefreshAsync();

        Assert.Single(viewModel.Projects);
        Assert.Contains("offline", viewModel.StatusMessage);
    }

    [Fact]
    public async Task OfflineFirstRunKeepsImportedColonizationCache()
    {
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(
            Path.Combine(directory, "F123-colony.json"),
            """
            {
              "cmdr": "Test Cmdr",
              "primaryBuildId": "cached-build",
              "hiddenIDs": [],
              "projects": [
                {
                  "buildId": "cached-build",
                  "buildType": "no_truss",
                  "buildName": "Cached port",
                  "systemName": "Cached System",
                  "maxNeed": 1000,
                  "sumNeed": 300,
                  "commodities": {"steel": 300}
                }
              ],
              "linkedFCs": {}
            }
            """);
        var client = new StubRavenColonialClient
        {
            Failure = new HttpRequestException("offline"),
        };
        var viewModel = new ColonizationViewModel(
            new ColonizationSettingsStore(Path.Combine(directory, "ui.json")),
            client,
            ColonizationBuildCatalog.LoadEmbedded(),
            new CommanderProfileStore(directory),
            new LegacyColonizationProfileStore(directory));
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile("F123", true, apiKey: null);

        await viewModel.SetCommanderAsync("Test Cmdr");

        var project = Assert.Single(viewModel.Projects);
        Assert.Equal("cached-build", project.Project.BuildId);
        Assert.True(project.IsPrimary);
        Assert.Equal("Cargo required: 300", viewModel.ProjectSummary);
        Assert.Contains("offline", viewModel.StatusMessage);
        Assert.Equal(1, client.LoadCount);
    }

    [Fact]
    public async Task FeedsProjectsCarriersAndShipCargoIntoOverlay()
    {
        var project = Project("build-1", "Port", remaining: 100) with
        {
            Commodities = new Dictionary<string, int> { ["steel"] = 100 },
            LinkedFleetCarriers =
            [
                new ColonizationProjectFleetCarrier { MarketId = 42 },
            ],
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [project],
                [],
                null,
                [
                    new ColonizationFleetCarrier
                    {
                        MarketId = 42,
                        Name = "ABC-123",
                        Cargo = new Dictionary<string, int>
                        {
                            ["steel"] = 60,
                        },
                    },
                ]),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;

        await viewModel.SetCommanderAsync("Test Cmdr");
        await viewModel.UpdateCargoAsync(new CargoSnapshot(
            DateTimeOffset.UtcNow,
            "Cargo",
            "Ship",
            25,
            [new CargoItem("steel", "Steel", 25, 0)]));

        var row = Assert.Single(viewModel.CommodityOverlay.Plan.Rows);
        Assert.Equal(25, row.InShip);
        Assert.Equal(60, row.OnFleetCarriers);
    }

    [Fact]
    public async Task MultipleGameWindowsClearAndRejectAmbiguousShipCargo()
    {
        var project = Project("build-1", "Port", remaining: 100) with
        {
            Commodities = new Dictionary<string, int> { ["steel"] = 100 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [project],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.ShipCargoPublishingEnabled = true;
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Loadout",
                "\"Ship\":\"python\",\"CargoCapacity\":192"),
        ]);
        await viewModel.SetCommanderAsync("Test Cmdr");
        var cargo = new CargoSnapshot(
            DateTimeOffset.UtcNow,
            "Cargo",
            "Ship",
            25,
            [new CargoItem("steel", "Steel", 25, 0)]);
        await viewModel.UpdateCargoAsync(cargo);
        Assert.Equal(25, Assert.Single(
            viewModel.CommodityOverlay.Plan.Rows).InShip);
        Assert.Equal(1, client.PublishShipCount);

        viewModel.SetSharedCargoSuppressed(true);
        await viewModel.UpdateCargoAsync(cargo with
        {
            Timestamp = cargo.Timestamp.AddSeconds(1),
        });

        Assert.True(viewModel.SharedCargoSuppressed);
        Assert.Equal(0, Assert.Single(
            viewModel.CommodityOverlay.Plan.Rows).InShip);
        Assert.Equal(1, client.PublishShipCount);
        Assert.Contains(
            "multiple Elite windows",
            viewModel.ShipCargoPublishingStatus);
    }

    [Fact]
    public async Task PublishesOptedInShipCargoForVisibleProjects()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("build-1", "Port", remaining: 100)],
                [],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.ShipCargoPublishingEnabled = true;
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Loadout",
                "\"Ship\":\"python\",\"ShipName\":\"Raven One\",\"CargoCapacity\":192"),
        ]);
        await viewModel.SetCommanderAsync("Test Cmdr");

        await viewModel.UpdateCargoAsync(new CargoSnapshot(
            DateTimeOffset.UtcNow,
            "Cargo",
            "Ship",
            27,
            [new CargoItem("steel", "Steel", 27, 0)]));

        Assert.Equal(1, client.PublishShipCount);
        var ship = Assert.IsType<ColonizationCurrentShip>(
            client.LastPublishedShip);
        Assert.Equal("Test Cmdr", ship.CommanderName);
        Assert.Equal("Raven One", ship.Name);
        Assert.Equal("python", ship.Type);
        Assert.Equal(192, ship.MaximumCargo);
        Assert.Equal(27, ship.Cargo["steel"]);
        Assert.Contains("Published", viewModel.ShipCargoPublishingStatus);

        await viewModel.UpdateCargoAsync(
            new CargoSnapshot(
                DateTimeOffset.UtcNow.AddSeconds(1),
                "MarketSell",
                "Ship",
                26,
                [new CargoItem("steel", "Steel", 26, 0)]),
            publishCurrentShipCargo: false);

        Assert.Equal(26, Assert.Single(
            viewModel.CommodityOverlay.Plan.Rows).InShip);
        Assert.Equal(1, client.PublishShipCount);
    }

    [Fact]
    public async Task DoesNotPublishShipCargoWithoutOptInOrVisibleProjects()
    {
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [Project("hidden", "Port", remaining: 100)],
                ["hidden"],
                null,
                []),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile("F123", true, "secret-key");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Loadout",
                "\"Ship\":\"python\",\"CargoCapacity\":192"),
        ]);
        await viewModel.SetCommanderAsync("Test Cmdr");
        var cargo = new CargoSnapshot(
            DateTimeOffset.UtcNow,
            "Cargo",
            "Ship",
            1,
            [new CargoItem("steel", "Steel", 1, 0)]);

        await viewModel.UpdateCargoAsync(cargo);
        Assert.Equal(0, client.PublishShipCount);

        viewModel.ShipCargoPublishingEnabled = true;
        await viewModel.UpdateCargoAsync(cargo with
        {
            Timestamp = cargo.Timestamp.AddSeconds(1),
        });

        Assert.Equal(0, client.PublishShipCount);
        Assert.Contains("no visible", viewModel.ShipCargoPublishingStatus);
    }

    [Fact]
    public async Task FeedsPostDockMarketStockIntoOverlay()
    {
        var project = Project("build-1", "Port", remaining: 100) with
        {
            Commodities = new Dictionary<string, int> { ["steel"] = 100 },
            LinkedFleetCarriers =
            [
                new ColonizationProjectFleetCarrier { MarketId = 42 },
            ],
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [project],
                [],
                null,
                [
                    new ColonizationFleetCarrier
                    {
                        MarketId = 42,
                        Cargo = new Dictionary<string, int>
                        {
                            ["steel"] = 80,
                        },
                    },
                ]),
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Loadout",
                "\"CargoCapacity\":64"),
            Event(
                "Docked",
                """
                "MarketID":900,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"Supply Station","StationServices":["commodities"]
                """),
        ]);
        await viewModel.UpdateMarketAsync(new MarketSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:01Z"),
            "Market",
            900,
            "Supply Station",
            "Coriolis",
            string.Empty,
            "Test",
            [
                new MarketItem(
                    1,
                    "$Steel_Name;",
                    "Steel",
                    "$MARKET_category_metals;",
                    "Metals",
                    1,
                    1,
                    1,
                    1,
                    0,
                    50,
                    0,
                    true,
                    false,
                    false),
            ]));

        var row = Assert.Single(viewModel.CommodityOverlay.Plan.Rows);
        Assert.True(row.IsAvailableAtCurrentMarket);
        Assert.True(row.CanCompleteFleetCarrierLoad);
    }

    [Fact]
    public async Task SavesCommanderKeyWithoutExposingItInStatus()
    {
        var client = new StubRavenColonialClient();
        var viewModel = Create(client);
        viewModel.SetCommanderProfile("F123", isOdyssey: true, apiKey: null);
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.RavenApiKey = "secret-key";

        await viewModel.SaveRavenApiKeyAsync();

        var store = new CommanderProfileStore(directory);
        var profile = await store.LoadAsync("F123", isOdyssey: true);
        Assert.Equal("secret-key", profile.Data?.RavenColonialApiKey);
        Assert.True(viewModel.HasStoredRavenApiKey);
        Assert.Equal(1, client.ValidateApiKeyCount);
        Assert.DoesNotContain("secret-key", viewModel.RavenCredentialStatus);
    }

    [Fact]
    public async Task RefusesRavenKeyOwnedByDifferentCommander()
    {
        var client = new StubRavenColonialClient
        {
            ValidatedCommanderName = "Other Cmdr",
        };
        var store = new CommanderProfileStore(directory);
        await store.SaveRavenColonialApiKeyAsync(
            "F123",
            "Test Cmdr",
            isOdyssey: true,
            "existing-key");
        var profilePath = Assert.Single(Directory.GetFiles(directory));
        var originalBytes = await File.ReadAllBytesAsync(profilePath);
        var viewModel = Create(client);
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "existing-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.RavenApiKey = "wrong-key";

        await viewModel.SaveRavenApiKeyAsync();

        var profile = await store.LoadAsync("F123", isOdyssey: true);
        Assert.Equal("existing-key", profile.Data?.RavenColonialApiKey);
        Assert.True(viewModel.HasStoredRavenApiKey);
        Assert.Equal(
            originalBytes,
            await File.ReadAllBytesAsync(profilePath));
        Assert.Contains("Other Cmdr", viewModel.RavenCredentialStatus);
        Assert.Contains("Test Cmdr", viewModel.RavenCredentialStatus);
        Assert.DoesNotContain("wrong-key", viewModel.RavenCredentialStatus);
    }

    [Fact]
    public async Task SyncsLinkedCarrierOnlyAfterExplicitOptIn()
    {
        var project = Project("build-1", "Port", remaining: 100) with
        {
            Commodities = new Dictionary<string, int> { ["steel"] = 100 },
            LinkedFleetCarriers =
            [
                new ColonizationProjectFleetCarrier { MarketId = 42 },
            ],
        };
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "ABC-123",
            DisplayName = "Supply carrier",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [project],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"Supply carrier ABC-123","StationType":"FleetCarrier",
                "StationServices":["commodities"]
                """),
        ]);
        var market = new MarketSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:01Z"),
            "Market",
            42,
            "Supply carrier ABC-123",
            "FleetCarrier",
            "all",
            "Test",
            [
                new MarketItem(
                    1,
                    "$Steel_Name;",
                    "Steel",
                    "$MARKET_category_metals;",
                    "Metals",
                    1,
                    1,
                    1,
                    1,
                    0,
                    80,
                    0,
                    true,
                    false,
                    false),
            ]);

        await viewModel.UpdateMarketAsync(market);
        Assert.Equal(0, client.ReplaceCargoCount);

        viewModel.FleetCarrierCargoSyncEnabled = true;
        await viewModel.UpdateMarketAsync(market with
        {
            Timestamp = market.Timestamp.AddSeconds(1),
        });

        Assert.Equal(1, client.ReplaceCargoCount);
        Assert.Equal(80, client.LastReplacement?["steel"]);
        Assert.Contains("Updated 1 cargo", viewModel.FleetCarrierSyncStatus);
        Assert.False(viewModel.CommodityOverlay.HasPendingCargo);
    }

    [Fact]
    public async Task AdjustsLinkedCarrierFromLiveCargoEventsOnlyAfterOptIn()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "ABC-123",
            Cargo = new Dictionary<string, int>
            {
                ["steel"] = 75,
                ["water"] = 10,
            },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"ABC-123","StationType":"FleetCarrier",
                "StationServices":["commodities"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });
        var bought = Event(
            "MarketBuy",
            "\"MarketID\":42,\"Type\":\"Steel\",\"Count\":5");

        await viewModel.SynchronizeLiveProjectsAsync(
            [bought],
            allowPublishing: true);
        Assert.Empty(client.FleetCarrierAdjustments);

        viewModel.FleetCarrierCargoSyncEnabled = true;
        await viewModel.SynchronizeLiveProjectsAsync(
        [
            bought,
            Event(
                "MarketSell",
                "\"MarketID\":42,\"Type\":\"Water\",\"Count\":2"),
            Event(
                "CargoTransfer",
                """
                "Transfers":[
                  {"Type":"Steel","Count":4,"Direction":"tocarrier"},
                  {"Type":"Water","Count":3,"Direction":"toship"}]
                """),
        ],
        allowPublishing: true);

        Assert.Collection(
            client.FleetCarrierAdjustments,
            call => Assert.Equal(-5, call.Changes["steel"]),
            call => Assert.Equal(2, call.Changes["water"]),
            call =>
            {
                Assert.Equal(4, call.Changes["steel"]);
                Assert.Equal(-3, call.Changes["water"]);
            });
        Assert.Contains("CargoTransfer", viewModel.StatusMessage);
        Assert.False(viewModel.CommodityOverlay.HasPendingCargo);

        await viewModel.SynchronizeLiveProjectsAsync(
            [bought],
            allowPublishing: false);
        Assert.Equal(3, client.FleetCarrierAdjustments.Count);
    }

    [Fact]
    public async Task AdjustsLinkedSquadronCarrierFromShipCargoDiffNotTransferJournal()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int>
            {
                ["steel"] = 75,
                ["water"] = 10,
            },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        var cargo = new CargoInventoryState();
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:00Z"),
            "Cargo",
            "Ship",
            50,
            [new CargoItem("steel", "Steel", 50, 0)]));

        // Freeze before-state, then apply transfer mutation (as MainWindow does).
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.HasPreservedSnapshot);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[
              {"Type":"Steel","Count":10,"Direction":"tocarrier"},
              {"Type":"Water","Count":3,"Direction":"toship"}]
            """)));
        // Ship after: steel 40, water 3
        Assert.Equal(40, cargo.CreateSnapshot()!.GetCount("steel"));
        Assert.Equal(3, cargo.CreateSnapshot()!.GetCount("water"));

        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[
                      {"Type":"Steel","Count":10,"Direction":"tocarrier"},
                      {"Type":"Water","Count":3,"Direction":"toship"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true);

        // Journal transfer path must not fire for squadron; only inverted ship diff.
        var adjustment = Assert.Single(client.FleetCarrierAdjustments);
        Assert.Equal(10, adjustment.Changes["steel"]);
        Assert.Equal(-3, adjustment.Changes["water"]);
        Assert.Contains("squadron", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(cargo.HasPreservedSnapshot);
    }

    [Fact]
    public async Task SkipsSquadronCargoDiffAfterMarketBuyAlreadyAdjustedCarrier()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        var cargo = new CargoInventoryState();
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:00Z"),
            "Cargo",
            "Ship",
            0,
            []));
        cargo.Apply(Event(
            "MarketBuy",
            "\"MarketID\":42,\"Type\":\"Steel\",\"Count\":5"));

        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "MarketBuy",
                    "\"MarketID\":42,\"Type\":\"Steel\",\"Count\":5"),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true);

        // Market buy adjusts once; skipNext prevents a second cargo-diff adjustment.
        var adjustment = Assert.Single(client.FleetCarrierAdjustments);
        Assert.Equal(-5, adjustment.Changes["steel"]);

        // skipNext is consumed only once: a later real transfer still adjusts.
        // Use the production capture gate (sync enabled, API key, linked squadron FC).
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.HasPreservedSnapshot);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[{"Type":"Steel","Count":4,"Direction":"tocarrier"}]
            """)));
        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[{"Type":"Steel","Count":4,"Direction":"tocarrier"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true);

        Assert.Equal(2, client.FleetCarrierAdjustments.Count);
        Assert.Equal(4, client.FleetCarrierAdjustments[1].Changes["steel"]);
    }

    [Fact]
    public async Task SendsSquadronTransferDiffWhenMarketBuyAndTransferShareAPoll()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        // Market buy mutates ship cargo first; transfer capture baselines after market.
        var cargo = new CargoInventoryState();
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:00Z"),
            "Cargo",
            "Ship",
            50,
            [new CargoItem("steel", "Steel", 50, 0)]));
        Assert.True(cargo.Apply(Event(
            "MarketBuy",
            "\"MarketID\":42,\"Type\":\"Steel\",\"Count\":5")));
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
            """)));
        Assert.Equal(45, cargo.CreateSnapshot()!.GetCount("steel"));

        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "MarketBuy",
                    "\"MarketID\":42,\"Type\":\"Steel\",\"Count\":5"),
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true);

        // Market adjustment + transfer GetDiff (not suppressed by skipNext).
        Assert.Equal(2, client.FleetCarrierAdjustments.Count);
        Assert.Equal(-5, client.FleetCarrierAdjustments[0].Changes["steel"]);
        Assert.Equal(10, client.FleetCarrierAdjustments[1].Changes["steel"]);
    }

    [Fact]
    public async Task CapturesFirstSquadronBaselineAcrossMultipleTransfersInOnePoll()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        var cargo = new CargoInventoryState();
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:00Z"),
            "Cargo",
            "Ship",
            50,
            [new CargoItem("steel", "Steel", 50, 0)]));

        // Two sequential Prepare/Apply pairs must keep the first before-state.
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
            """)));
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[{"Type":"Steel","Count":5,"Direction":"tocarrier"}]
            """)));
        Assert.Equal(35, cargo.CreateSnapshot()!.GetCount("steel"));

        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
                    """),
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[{"Type":"Steel","Count":5,"Direction":"tocarrier"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true);

        var adjustment = Assert.Single(client.FleetCarrierAdjustments);
        Assert.Equal(15, adjustment.Changes["steel"]);
    }

    [Fact]
    public async Task FallsBackToJournalTransfersForSquadronWhenShipCargoDiffUnavailable()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        // No cargoInventory => journal fallback for squadron transfers.
        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[
                      {"Type":"Steel","Count":4,"Direction":"tocarrier"},
                      {"Type":"Water","Count":3,"Direction":"toship"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: null,
            cargoActivity: false);

        var adjustment = Assert.Single(client.FleetCarrierAdjustments);
        Assert.Equal(4, adjustment.Changes["steel"]);
        Assert.Equal(-3, adjustment.Changes["water"]);
    }

    [Fact]
    public async Task SkipsDuplicateSquadronDiffWhenFallbackRunsBeforeSnapshotReplay()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "SQD-001",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            Workspace = new ColonizationCommanderProjects(
                [],
                [],
                null,
                [carrier]),
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        await viewModel.SetCommanderAsync("Test Cmdr");
        viewModel.FleetCarrierCargoSyncEnabled = true;
        viewModel.ApplyJournalEvents(
        [
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"SQD-001","StationType":"FleetCarrier",
                "StationServices":["commodities","squadronBank"]
                """),
        ]);
        viewModel.UpdateStatus(new EliteStatus
        {
            Flags = StatusFlags.InMainShip,
        });

        var cargo = new CargoInventoryState();
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:00Z"),
            "Cargo",
            "Ship",
            50,
            [new CargoItem("steel", "Steel", 50, 0)]));

        // Initial transfer path uses journal fallback because squadron diff is disabled
        // for this read. Preserve the baseline so this call can still be replay-safe.
        viewModel.PrepareSquadronCargoTransferSnapshot(cargo);
        Assert.True(cargo.Apply(Event(
            "CargoTransfer",
            """
            "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
            """)));

        await viewModel.SynchronizeLiveProjectsAsync(
            [
                Event(
                    "CargoTransfer",
                    """
                    "Transfers":[{"Type":"Steel","Count":10,"Direction":"tocarrier"}]
                    """),
            ],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: true,
            preferShipCargoDiffForSquadron: false);

        Assert.Single(client.FleetCarrierAdjustments);
        Assert.False(cargo.HasPreservedSnapshot);

        // Next read with fresh cargo snapshot should use GetDiff, but not replay the
        // already synced transfer (the snapshot replay is now rebased from 40).
        cargo.Reset(new CargoSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:01Z"),
            "Cargo",
            "Ship",
            40,
            [new CargoItem("steel", "Steel", 40, 0)]));

        await viewModel.SynchronizeLiveProjectsAsync(
            [],
            allowPublishing: true,
            cargoInventory: cargo,
            cargoActivity: false);

        Assert.Single(client.FleetCarrierAdjustments);
    }

    [Fact]
    public async Task PublishesCurrentCarrierAndThenReconcilesFreshMarket()
    {
        var carrier = new ColonizationFleetCarrier
        {
            MarketId = 42,
            Name = "ABC-123",
            DisplayName = "Supply carrier",
            Cargo = new Dictionary<string, int> { ["steel"] = 75 },
        };
        var client = new StubRavenColonialClient
        {
            FleetCarrierResponse = carrier,
        };
        var viewModel = Create(client);
        viewModel.IsEnabled = true;
        viewModel.SetCommanderProfile(
            "F123",
            isOdyssey: true,
            apiKey: "secret-key");
        viewModel.ApplyJournalEvents(
        [
            Event(
                "ReceiveText",
                "\"From\":\"Supply carrier | ABC-123\""),
            Event(
                "Docked",
                """
                "MarketID":42,"SystemAddress":20,"StarSystem":"Test",
                "StationName":"ABC-123","StationType":"FleetCarrier",
                "StationServices":["commodities"]
                """),
        ]);
        await viewModel.SetCommanderAsync("Test Cmdr");
        await viewModel.UpdateMarketAsync(new MarketSnapshot(
            DateTimeOffset.Parse("2026-07-24T12:00:01Z"),
            "Market",
            42,
            "ABC-123",
            "FleetCarrier",
            "all",
            "Test",
            [
                new MarketItem(
                    1,
                    "$Steel_Name;",
                    "Steel",
                    "$MARKET_category_metals;",
                    "Metals",
                    1,
                    1,
                    1,
                    1,
                    0,
                    80,
                    0,
                    true,
                    false,
                    false),
            ]));

        Assert.True(viewModel.PublishFleetCarrierCommand.CanExecute(null));
        await viewModel.PublishCurrentFleetCarrierAsync();

        Assert.Equal(1, client.PublishCarrierCount);
        Assert.Equal(42, client.LastCarrierRegistration?.MarketId);
        Assert.Equal("ABC-123", client.LastCarrierRegistration?.Name);
        Assert.Equal(
            "Supply carrier",
            client.LastCarrierRegistration?.DisplayName);
        Assert.Null(client.LastCarrierRegistration?.Cargo);
        Assert.Equal(1, client.ReplaceCargoCount);
        Assert.Equal(80, client.LastReplacement?["steel"]);
        Assert.Contains(
            "Published and linked Supply carrier",
            viewModel.FleetCarrierSyncStatus);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private ColonizationViewModel Create(
        StubRavenColonialClient client,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        return new ColonizationViewModel(
            new ColonizationSettingsStore(
                Path.Combine(directory, "ui.json")),
            client,
            ColonizationBuildCatalog.LoadEmbedded(),
            new CommanderProfileStore(directory),
            delayAsync: delayAsync);
    }

    private static ColonizationProject Project(
        string id,
        string name,
        int remaining,
        long marketId = 0,
        long systemAddress = 0,
        string? factionName = null)
    {
        return new ColonizationProject
        {
            BuildId = id,
            BuildType = "no_truss",
            BuildName = name,
            SystemName = "Test System",
            MarketId = marketId,
            SystemAddress = systemAddress,
            FactionName = factionName,
            MaximumRequired = 1_000,
            RemainingRequired = remaining,
            Commodities = new Dictionary<string, int>
            {
                ["steel"] = remaining,
            },
        };
    }

    private static JournalEventEnvelope Event(
        string eventName,
        string properties)
    {
        var propertySuffix = string.IsNullOrWhiteSpace(properties)
            ? string.Empty
            : "," + properties;
        var json = $$"""
            {"timestamp":"2026-07-24T12:00:00Z","event":"{{eventName}}"{{propertySuffix}}}
            """;
        Assert.True(
            JournalEventEnvelope.TryParse(json, out var result, out var error),
            error);
        return result!;
    }

    private sealed class StubRavenColonialClient : IRavenColonialClient
    {
        public ColonizationCommanderProjects Workspace { get; set; } = new(
            [],
            [],
            null,
            []);

        public Exception? Failure { get; set; }

        public string? ValidatedCommanderName { get; set; } = "Test Cmdr";

        public int LoadCount { get; private set; }

        public int SaveCount { get; private set; }

        public int ValidateApiKeyCount { get; private set; }

        public int ReplaceCargoCount { get; private set; }

        public int PublishCarrierCount { get; private set; }

        public int PublishShipCount { get; private set; }

        public List<FleetCarrierAdjustmentCall> FleetCarrierAdjustments
        {
            get;
        } = [];

        public int SiteProjectLoadCount { get; private set; }

        public int SystemSiteLoadCount { get; private set; }

        public int MarkCompleteCount { get; private set; }

        public List<ColonizationProjectUpdate> ProjectUpdates { get; } = [];

        public List<ContributionCall> Contributions { get; } = [];

        public List<string?> PrimaryProjectRequests { get; } = [];

        public List<SystemUpdateCall> SystemUpdates { get; } = [];

        public List<SystemSitePatchCall> SystemSitePatches { get; } = [];

        public Queue<Exception> SystemSiteFailures { get; } = new();

        public IReadOnlyList<ColonizationSystemSite> SystemSitesResponse
        {
            get;
            set;
        } = [];

        public ColonizationCurrentShip? LastPublishedShip { get; private set; }

        public ColonizationFleetCarrier? FleetCarrierResponse { get; set; }

        public ColonizationFleetCarrierRegistration? LastCarrierRegistration
        {
            get;
            private set;
        }

        public ColonizationProject? SiteProjectResponse { get; set; }

        public IReadOnlyDictionary<string, int>? LastReplacement
        {
            get;
            private set;
        }

        public IReadOnlyList<string> LastSavedHiddenIds { get; private set; } =
            [];

        public Task<ColonizationCommanderProjects> GetCommanderProjectsAsync(
            string commanderName,
            CancellationToken cancellationToken = default)
        {
            LoadCount++;
            return Failure is null
                ? Task.FromResult(Workspace)
                : Task.FromException<ColonizationCommanderProjects>(Failure);
        }

        public Task<string?> GetCommanderByApiKeyAsync(
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            ValidateApiKeyCount++;
            return Task.FromResult(ValidatedCommanderName);
        }

        public Task<IReadOnlyList<string>> SaveHiddenProjectIdsAsync(
            string commanderName,
            IEnumerable<string> hiddenProjectIds,
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            LastSavedHiddenIds = hiddenProjectIds.ToArray();
            return Task.FromResult(LastSavedHiddenIds);
        }

        public Task<ColonizationProject?> GetProjectAsync(
            string buildId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ColonizationProject?>(null);
        }

        public Task<ColonizationProject?> GetProjectAsync(
            long systemAddress,
            long marketId,
            CancellationToken cancellationToken = default)
        {
            SiteProjectLoadCount++;
            return Task.FromResult(SiteProjectResponse);
        }

        public Task<ColonizationProject> UpdateProjectAsync(
            ColonizationProjectUpdate update,
            CancellationToken cancellationToken = default)
        {
            ProjectUpdates.Add(update);
            var source = Workspace.Projects.First(project =>
                project.BuildId == update.BuildId);
            var remaining = update.Commodities?.Values.Sum()
                ?? source.RemainingRequired;
            var updated = source with
            {
                FactionName = update.FactionName ?? source.FactionName,
                MaximumRequired = update.MaximumRequired
                    ?? source.MaximumRequired,
                RemainingRequired = remaining,
                Commodities = update.Commodities is null
                    ? source.Commodities
                    : new Dictionary<string, int>(
                        update.Commodities,
                        StringComparer.OrdinalIgnoreCase),
            };
            return Task.FromResult(updated);
        }

        public Task MarkProjectCompleteAsync(
            string buildId,
            CancellationToken cancellationToken = default)
        {
            MarkCompleteCount++;
            return Task.CompletedTask;
        }

        public Task ContributeToProjectAsync(
            string buildId,
            string commanderName,
            IReadOnlyDictionary<string, int> contributions,
            CancellationToken cancellationToken = default)
        {
            Contributions.Add(new ContributionCall(
                buildId,
                commanderName,
                contributions));
            return Task.CompletedTask;
        }

        public Task SetPrimaryProjectAsync(
            string commanderName,
            string? buildId,
            CancellationToken cancellationToken = default)
        {
            PrimaryProjectRequests.Add(buildId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ColonizationSystemSite>> GetSystemSitesAsync(
            string systemNameOrAddress,
            CancellationToken cancellationToken = default)
        {
            SystemSiteLoadCount++;
            return SystemSiteFailures.TryDequeue(out var failure)
                ? Task.FromException<IReadOnlyList<ColonizationSystemSite>>(
                    failure)
                : Task.FromResult(SystemSitesResponse);
        }

        public Task<string?> GetSystemArchitectAsync(
            string systemNameOrAddress,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }

        public Task<ColonizationSystemRecord> GetSystemAsync(
            string systemNameOrAddress,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ColonizationSystemRecord> ImportSystemBodiesAsync(
            string systemNameOrAddress,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ColonizationSystemRecord> UpdateSystemSitesAsync(
            string systemNameOrAddress,
            ColonizationSystemSiteUpdate update,
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            SystemUpdates.Add(new SystemUpdateCall(
                systemNameOrAddress,
                update,
                apiKey));
            return Task.FromResult(new ColonizationSystemRecord
            {
                SystemAddress = 42,
                Name = systemNameOrAddress,
                Architect = update.Architect,
                Sites = update.UpdatedSites,
            });
        }

        public Task PatchSystemSiteAsync(
            string systemNameOrAddress,
            string siteId,
            ColonizationSystemSitePatch patch,
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            SystemSitePatches.Add(new SystemSitePatchCall(
                systemNameOrAddress,
                siteId,
                patch,
                apiKey));
            return Task.CompletedTask;
        }

        public Task<ColonizationProject?> CreateProjectAsync(
            ColonizationProjectCreate project,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ColonizationProject?>(null);
        }

        public Task<ColonizationFleetCarrier?> GetFleetCarrierAsync(
            long marketId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FleetCarrierResponse);
        }

        public Task<ColonizationFleetCarrier> PublishFleetCarrierAsync(
            ColonizationFleetCarrierRegistration carrier,
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            PublishCarrierCount++;
            LastCarrierRegistration = carrier;
            return Task.FromResult(FleetCarrierResponse ?? new()
            {
                MarketId = carrier.MarketId,
                Name = carrier.Name,
                DisplayName = carrier.DisplayName,
            });
        }

        public Task<IReadOnlyDictionary<string, int>>
            ReplaceFleetCarrierCargoAsync(
                long marketId,
                IReadOnlyDictionary<string, int> cargo,
                string apiKey,
                CancellationToken cancellationToken = default)
        {
            ReplaceCargoCount++;
            LastReplacement = cargo;
            var updated = new Dictionary<string, int>(
                FleetCarrierResponse?.Cargo
                    ?? new Dictionary<string, int>(),
                StringComparer.OrdinalIgnoreCase);
            foreach (var pair in cargo)
            {
                updated[pair.Key] = pair.Value;
            }

            return Task.FromResult<IReadOnlyDictionary<string, int>>(updated);
        }

        public Task<IReadOnlyDictionary<string, int>>
            AdjustFleetCarrierCargoAsync(
                long marketId,
                IReadOnlyDictionary<string, int> cargoChanges,
                string apiKey,
                CancellationToken cancellationToken = default)
        {
            FleetCarrierAdjustments.Add(new FleetCarrierAdjustmentCall(
                marketId,
                new Dictionary<string, int>(
                    cargoChanges,
                    StringComparer.OrdinalIgnoreCase)));
            var updated = new Dictionary<string, int>(
                FleetCarrierResponse?.Cargo
                    ?? new Dictionary<string, int>(),
                StringComparer.OrdinalIgnoreCase);
            foreach (var pair in cargoChanges)
            {
                updated[pair.Key] = Math.Max(
                    0,
                    updated.GetValueOrDefault(pair.Key) + pair.Value);
            }

            if (FleetCarrierResponse is not null)
            {
                FleetCarrierResponse = FleetCarrierResponse with
                {
                    Cargo = updated,
                };
            }

            return Task.FromResult<IReadOnlyDictionary<string, int>>(updated);
        }

        public Task PublishCurrentShipAsync(
            ColonizationCurrentShip ship,
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            PublishShipCount++;
            LastPublishedShip = ship;
            return Task.CompletedTask;
        }
    }

    private sealed record ContributionCall(
        string BuildId,
        string CommanderName,
        IReadOnlyDictionary<string, int> Commodities);

    private sealed record SystemUpdateCall(
        string SystemNameOrAddress,
        ColonizationSystemSiteUpdate Update,
        string ApiKey);

    private sealed record SystemSitePatchCall(
        string SystemNameOrAddress,
        string SiteId,
        ColonizationSystemSitePatch Patch,
        string ApiKey);

    private sealed record FleetCarrierAdjustmentCall(
        long MarketId,
        IReadOnlyDictionary<string, int> Changes);
}
