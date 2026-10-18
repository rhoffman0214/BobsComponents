using System.Reflection;
using System.Text.RegularExpressions;
using BobsComponents.Components;
using Microsoft.AspNetCore.Components;

namespace BobsComponents.Tests.Contract;

/// <summary>
/// Until the generated docs site (phase 4) exists, each component's hand-written docs page must list every
/// [Parameter], every data-bob-part and every custom property its stylesheet uses, so the page cannot drift.
/// </summary>
public class DocsPageTests
{
    private static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "docs", "CSS-CONTRACT.md"))) dir = dir.Parent;
        return dir!.FullName;
    }

    public static IEnumerable<object[]> Pages() =>
    [
        [typeof(LoadingSpinner), "LoadingSpinnerDoc.razor", "spinner.css"],
        [typeof(AsyncButton), "AsyncButtonDoc.razor", "button.css"],
        [typeof(FullScreenLoader), "FullScreenLoaderDoc.razor", "fullscreen-loader.css"],
        [typeof(ModalSheet), "ModalSheetDoc.razor", "modal.css"],
        [typeof(ConfirmDialog), "ConfirmDialogDoc.razor", "confirm.css"],
        [typeof(ToastHost), "ToastsDoc.razor", "toast.css"],
    ];

    [Theory]
    [MemberData(nameof(Pages))]
    public void DocsPage_ListsEveryParameter_Part_AndVariable(Type component, string page, string css)
    {
        var text = File.ReadAllText(Path.Combine(Root, "src", "BobsComponent.Client", "Pages", "Bob", page));

        var parameters = component.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() is not null)
            .Select(p => p.Name);
        foreach (var name in parameters)
            Assert.True(text.Contains($"\"{name}\"") || text.Contains($".{name}\""), $"{page} does not document parameter {name}");

        var cssText = File.ReadAllText(Path.Combine(Root, "src", "BobsComponents", "wwwroot", "css", "components", css));
        foreach (Match m in Regex.Matches(cssText, @"--bob-[\w-]+").Select(m => m).DistinctBy(m => m.Value))
        {
            // Variables that only fall back to a global token (e.g. --bob-color-*) belong to tokens.css.
            if (m.Value.StartsWith("--bob-color-") || m.Value.StartsWith("--bob-radius-") || m.Value.StartsWith("--bob-shadow")
                || m.Value.StartsWith("--bob-font-") || m.Value.StartsWith("--bob-z-") || m.Value.StartsWith("--bob-duration")) continue;
            Assert.True(text.Contains($"\"{m.Value}\""), $"{page} does not document {m.Value}");
        }

        var markup = File.ReadAllText(Path.Combine(Root, "src", "BobsComponents", "Components", component.Name + ".razor"));
        foreach (Match m in Regex.Matches(markup, @"data-bob-part=""([\w-]+)""").DistinctBy(m => m.Groups[1].Value))
            Assert.True(text.Contains($"new(\"{m.Groups[1].Value}\""), $"{page} does not document part '{m.Groups[1].Value}'");
    }
}
