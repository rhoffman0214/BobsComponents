using BobsComponents.Docs.Generation;

namespace BobsComponents.Tests.Docs;

/// <summary>
/// Names are public API. docs/api-surface.txt is the committed list of them; removing or renaming one without a Breaking
/// entry in the newest changelog release fails. Adding one fails until the snapshot is refreshed, so the next removal is caught too.
/// Refresh with: <c>$env:BOB_UPDATE_SURFACE=1; dotnet test BobsComponents.Tests --filter ApiSurface</c>.
/// </summary>
public class ApiSurfaceTests
{
    private static string SnapshotPath => Path.Combine(DocsDriftTests.FindRoot(), "docs", "api-surface.txt");

    [Fact]
    public void Surface_MatchesTheSnapshot_AndEveryRemovalIsInTheChangelogBreakingSection()
    {
        var current = ApiSurface.Lines();
        if (Environment.GetEnvironmentVariable("BOB_UPDATE_SURFACE") == "1")
        {
            File.WriteAllText(SnapshotPath, string.Join('\n', current) + "\n");
            return;
        }

        var snapshot = File.ReadAllLines(SnapshotPath);
        var newest = ChangelogParser.Parse(DocSources.Changelog())[0];
        var breaking = string.Join('\n', newest.Breaking.SelectMany(s => s.Items));

        var (unexplained, added) = ApiSurface.Diff(snapshot, current, breaking);

        Assert.True(unexplained.Count == 0,
            "Removed without a Breaking entry in docs/CHANGELOG.md (release " + newest.Version + "): " + string.Join("; ", unexplained));
        Assert.True(added.Count == 0,
            "New public names are not in docs/api-surface.txt yet (mention them under Added in the changelog, then refresh the snapshot with BOB_UPDATE_SURFACE=1): " + string.Join("; ", added));
    }

    [Fact]
    public void Diff_FlagsRemovalsThatTheBreakingTextDoesNotMention_AndNewNames()
    {
        string[] snapshot = ["class bob-a", "var --bob-a-bg", "param Thing.Size", "part Thing/root"];
        string[] current = ["class bob-a", "var --bob-b-bg", "param Thing.Size", "part Thing/root", "class bob-new"];

        var (unexplained, added) = ApiSurface.Diff(snapshot, current, "Renamed `--bob-a-bg` to `--bob-b-bg`.");
        Assert.Empty(unexplained);
        Assert.Equal(["class bob-new", "var --bob-b-bg"], added);

        (unexplained, _) = ApiSurface.Diff(snapshot, current, "Nothing relevant.");
        Assert.Equal(["var --bob-a-bg"], unexplained);
    }

    [Fact]
    public void Surface_ContainsTheKindsOfNamesTheContractPromises()
    {
        var lines = ApiSurface.Lines();
        Assert.Contains("class bob-modal__header", lines);
        Assert.Contains("var --bob-modal-bg", lines);
        Assert.Contains("part ModalSheet/sheet", lines);
        Assert.Contains("param ModalSheet.Show", lines);
        Assert.Contains("error BOB-PARAM-001", lines);
        Assert.Contains("part BobErrorCard/root", lines);
    }
}
