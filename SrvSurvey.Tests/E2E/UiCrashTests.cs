using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace SrvSurvey.Tests.E2E;

/// <summary>
/// Real WinForms controls on the app's UI thread, for crashes that no journal event reaches. See README "Failure modes".
/// </summary>
public sealed class UiCrashTests
{
    /// <summary> #1075: sorting the boxel search list by distance while some systems have no distance yet (Tag == null) </summary>
    [Theory]
    [InlineData(true, "unknown1,unknown2,near,far")]
    [InlineData(false, "far,near,unknown1,unknown2")]
    public void Issue1075_SortBoxelSearchByDistanceWithUnknownDistances(bool ascending, string expected)
    {
        using var host = new ReplayHost($"issue1075-boxel-sort-{(ascending ? "asc" : "desc")}", "unused.log");
        var (error, order) = host.OnUi(() =>
        {
            using var list = new ListView { View = View.Details };
            list.Columns.Add("system"); list.Columns.Add("distance");
            list.CreateControl();
            // two unknown distances, so null is compared against null as well as against a value
            foreach (var (name, dist) in new[] { ("far", (double?)42.5), ("unknown1", null), ("near", 3.25), ("unknown2", null) })
            {
                var item = new ListViewItem(name);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, dist?.ToString() ?? "") { Tag = dist });
                list.Items.Add(item);
            }
            try
            {
                // what FormBoxelSearch does when the distance column header is clicked (again, to flip the order)
                list.ListViewItemSorter = new FormBoxelSearch.ColumnSorter { column = 1, ascending = ascending };
                list.Sort();
                return ((Exception?)null, string.Join(",", list.Items.Cast<ListViewItem>().Select(i => i.Text)));
            }
            catch (Exception ex) { return (ex, ""); }
        });
        var artifact = host.WriteArtifact($"error={error?.GetType().Name}:{error?.Message} order={order}");

        Assert.True(error == null, $"sorting threw {error}, see {artifact}");
        // (ListView sorting is not stable, so equal (unknown) items may swap)
        Assert.Equal(expected.Split(',').Order(), order.Split(',').Order());
        Assert.Equal(expected.Replace("unknown1", "u").Replace("unknown2", "u"), order.Replace("unknown1", "u").Replace("unknown2", "u"));
    }

    /// <summary> #948: journey viewer selecting a system that has no (readable) system file for this commander </summary>
    [Theory]
    [InlineData("missing", null)]
    [InlineData("unreadable", "{ \"name\":\"Never Visited AB-C d1-2\", \"bodies\": [ ")]
    public void Issue948_JourneyViewerSystemWithoutDataFile(string variant, string? fileContent)
    {
        const string fid = "F0000948";
        const string name = "Never Visited AB-C d1-2";
        const long address = 123456789;
        if (fileContent != null)
            ReplayHost.WriteDataFile(Path.Combine("systems", fid, $"{name}_{address}.json"), fileContent);

        using var host = new ReplayHost($"issue948-journey-system-{variant}", "unused.log");
        var (error, shownName) = host.OnUi(() =>
        {
            Game.settings.lastFid = fid;
            Game.settings.lastCommander = "E2E TESTER";
            try
            {
                using var view = new ViewJourneySystem(null!);
                view.setSystem(new CommanderJourney.SystemStats
                {
                    starRef = new StarRef(new double[] { 1, 2, 3 }, name, address),
                    count = new(),
                });
                return ((Exception?)null, view.systemData?.name);
            }
            catch (Exception ex) { return (ex, null); }
        });
        var artifact = host.WriteArtifact($"error={error?.GetType().Name}:{error?.Message} shownName={shownName}");

        Assert.True(error == null, $"setSystem threw {error}, see {artifact}");
        Assert.Equal(name, shownName);
    }

    /// <summary> #1070 #1064: FSS screen grab while the game window reports a 0 size (being created/closed) </summary>
    [Fact]
    public void Issue1070_FssScreenGrabOfZeroSizedWindow()
    {
        using var host = new ReplayHost("issue1070-fss-zero-grab", "unused.log");
        var screenGrab = typeof(PlotFSS).GetMethod("screenGrab", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var plotter = RuntimeHelpers.GetUninitializedObject(typeof(PlotFSS)); // screenGrab uses no instance state
        var results = new List<string>();
        foreach (var rect in new[] { new Rectangle(100, 100, 0, 540), new Rectangle(100, 100, 960, 0), new Rectangle(0, 0, 64, 32) })
        {
            try
            {
                using var bmp = (Bitmap?)screenGrab.Invoke(plotter, [rect]);
                results.Add($"{rect.Size}=>{(bmp == null ? "null" : bmp.Size.ToString())}");
            }
            catch (TargetInvocationException ex) { results.Add($"{rect.Size}=>{ex.InnerException!.GetType().Name}:{ex.InnerException.Message}"); }
        }
        var artifact = host.WriteArtifact(string.Join(" | ", results));

        Assert.Equal("{Width=0, Height=540}=>null", results[0]);
        Assert.Equal("{Width=960, Height=0}=>null", results[1]);
        Assert.Equal("{Width=64, Height=32}=>{Width=64, Height=32}", results[2]);
    }
}
