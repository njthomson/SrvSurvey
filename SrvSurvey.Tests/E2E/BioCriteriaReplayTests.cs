using BioCriterias;
using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Upstream #1043: DirectoryNotFoundException '...\pub\bio-criteria' from predictSpecies, "most of the times when I collect a sample".
/// The published bio-criteria are downloaded at start up - when that has not happened (yet) every prediction crashed.
/// </summary>
public sealed class BioCriteriaReplayTests
{
    private const string statusOnFoot = """{ "timestamp":"2026-08-19T07:09:20Z", "event":"Status", "Flags":2097152, "Flags2":17, "Oxygen":1.0, "Health":1.0, "Temperature":80.0, "SelectedWeapon":"", "Gravity":0.07, "Latitude":-46.624676, "Longitude":66.149605, "Heading":10, "Altitude":0, "BodyName":"Blau Euq MX-R d5-4 1 g", "PlanetRadius":1658541.375 }""";

    [Fact]
    public void Issue1043_MissingPublishedBioCriteria_ScanningDoesNotCrashAndLoadsCriteriaOnceDownloaded()
    {
        var lines = File.ReadAllLines(ReplayHost.Fixture("issue1044-Journal.2026-08-19T100112.01.log"))
            .Select(l => l.Replace("F0000001", "F0001043")).ToArray();
        var bootEnd = Array.FindIndex(lines, l => l.Contains("\"event\":\"Music\"")) + 1;
        var disembark = Array.FindIndex(lines, l => l.Contains("\"event\":\"Disembark\""));

        // as on a machine where the published data never arrived
        var parked = ReplayHost.BioCriteriaFolder + "-parked";
        Directory.Move(ReplayHost.BioCriteriaFolder, parked);
        lock (BioCriteria.allCriteria) BioCriteria.allCriteria.Clear();
        try
        {
            using var host = new ReplayHost("issue1043-no-bio-criteria", "Journal.2026-08-19T100112.01.log");
            host.Boot(lines.Take(bootEnd), "E2E TESTER");
            host.Append(lines[bootEnd..(disembark + 1)]); // SAASignalsFound, Scan, ... => predictSpecies
            host.WriteStatus(statusOnFoot);
            host.Append(lines[(disembark + 1)..]);         // ... ScanOrganic => predictSpecies again
            Thread.Sleep(500);                              // deferPredictSpecies runs 100ms later

            var missingLogged = host.CountLog("readCriteria: folder not found");
            var errors = host.AppErrors;

            // the download lands => the next prediction works
            Directory.Move(parked, ReplayHost.BioCriteriaFolder);
            // (this body predicts 0 species even with criteria - so check the criteria themselves got loaded, not the prediction count)
            var criteriaLoaded = host.OnUi(() =>
            {
                host.game!.systemBody!.predictSpecies();
                lock (BioCriteria.allCriteria) return BioCriteria.allCriteria.Count;
            });

            var artifact = host.WriteArtifact($"errors={errors.Count} missingFolderLogged={missingLogged} criteriaLoadedAfterDownload={criteriaLoaded}");
            Assert.True(errors.Count == 0, $"app raised {errors.Count} error(s), see {artifact}:\r\n{string.Join("\r\n", errors.Take(4))}");
            Assert.True(missingLogged > 0, $"predictSpecies never hit the missing folder - premise broken, see {artifact}");
            Assert.True(criteriaLoaded > 0, $"criteria never loaded after they arrived (empty cache stuck?), see {artifact}");
        }
        finally
        {
            if (Directory.Exists(parked)) Directory.Move(parked, ReplayHost.BioCriteriaFolder);
        }
    }
}
