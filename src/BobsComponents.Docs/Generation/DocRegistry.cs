using BobsComponents.Components;
using BobsComponents.Docs.Demos;
using BobsComponents.Errors;
using BobsComponents.Services;

namespace BobsComponents.Docs.Generation;

/// <summary>
/// The list of documented components. Adding a component page = add its type, stylesheet and demo files here
/// (see "Adding a component page" in CLAUDE.md); a test fails if a library component or stylesheet is missing from it.
/// </summary>
public static class DocRegistry
{
    public static IReadOnlyList<ComponentDoc> Components { get; } =
    [
        new("loading-spinner", "LoadingSpinner", typeof(LoadingSpinner), "css/components/spinner.css",
            [new("Sizes and labels", typeof(LoadingSpinnerDemo))]),

        new("async-button", "AsyncButton", typeof(AsyncButton), "css/components/button.css",
            [new("Variants, busy and disabled", typeof(AsyncButtonDemo))]),

        new("full-screen-loader", "FullScreenLoader", typeof(FullScreenLoader), "css/components/fullscreen-loader.css",
            [new("Contained preview", typeof(FullScreenLoaderDemo), "The real component fills the viewport; this preview is clipped to the box so it stays on the page.")]),

        new("modal-sheet", "ModalSheet", typeof(ModalSheet), "css/components/modal.css",
            [new("Sheet with focus trap and stacking", typeof(ModalSheetDemo))]),

        new("confirm-dialog", "ConfirmDialog", typeof(ConfirmDialog), "css/components/confirm.css",
            [new("Destructive and neutral confirmation", typeof(ConfirmDialogDemo))]),

        new("toasts", "Toasts", typeof(ToastHost), "css/components/toast.css",
            [new("Levels and durations", typeof(ToastsDemo))],
            Related: [typeof(IBobToastService)]),

        new("elapsed-clock", "ElapsedClock", typeof(ElapsedClock), "css/components/elapsed-clock.css",
            [new("Running, paused and long-running", typeof(ElapsedClockDemo))]),

        new("rest-picker", "RestPicker", typeof(RestPicker), "css/components/rest-picker.css",
            [new("Chips, other box and custom choices", typeof(RestPickerDemo))]),

        new("segment-picker", "SegmentPicker", typeof(SegmentPicker), "css/components/segment-picker.css",
            [new("Presets, custom areas and keyboard reorder", typeof(SegmentPickerDemo))]),

        new("draft-saver", "DraftSaver", typeof(DraftSaver), "css/components/draft-saver.css",
            [new("Hot-saving a text area", typeof(DraftSaverDemo))]),

        new("progress-chart", "ProgressChart", typeof(ProgressChart), "css/components/progress-chart.css",
            [new("Series over time and the empty state", typeof(ProgressChartDemo))]),

        new("activity-heatmap", "ActivityHeatmap", typeof(ActivityHeatmap), "css/components/activity-heatmap.css",
            [new("Streaks and weeks", typeof(ActivityHeatmapDemo))]),

        new("notification-bell", "NotificationBell", typeof(NotificationBell), "css/components/notification-bell.css",
            [new("In-memory source", typeof(NotificationBellDemo))]),

        new("progress-bar", "ProgressBar", typeof(ProgressBar), "css/components/progress.css",
            [new("Variants and indeterminate", typeof(ProgressBarDemo))]),
        new("skeleton-loader", "SkeletonLoader", typeof(SkeletonLoader), "css/components/skeleton.css",
            [new("Shapes and groups", typeof(SkeletonLoaderDemo))]),
        new("select-field", "SelectField", typeof(SelectField), "css/components/select.css",
            [new("Bound select", typeof(SelectFieldDemo))]),
    ];

    /// <summary>Stylesheets that belong to components documented on hand-written pages; they still feed the CSS reference and search.</summary>
    public static IReadOnlyList<ReferenceSheet> ReferenceSheets { get; } =
    [
        new("error-card", "BobErrorCard", "BobErrorCard", "css/components/error-card.css", "bob/errors"),
        new("dev-kit", "BobDevKit", "BobDevKit", "css/components/devkit.css", "bob/dev-kit"),
    ];

    public static BobErrorCatalog Catalog { get; } = BobErrorCatalog.CreateDefault();

    private static readonly Dictionary<string, ComponentDocModel> Cache = new(StringComparer.Ordinal);

    public static ComponentDoc? Find(string slug) =>
        Components.FirstOrDefault(c => c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static ComponentDocModel Model(ComponentDoc doc)
    {
        lock (Cache)
        {
            if (!Cache.TryGetValue(doc.Slug, out var model))
                Cache[doc.Slug] = model = ComponentDocModel.Build(doc, Components, Catalog);
            return model;
        }
    }

    /// <summary>The documented components whose source can raise this code (derived, plus any listed in the catalog).</summary>
    public static IReadOnlyList<ComponentDoc> RaisedBy(string code) =>
        Components.Where(c => Model(c).Errors.Any(e => e.Entry.Code == code)).ToList();

    public static CssReference ReferenceFor(ReferenceSheet sheet) =>
        CssReference.Build(sheet.CssFile, MarkupScanner.Scan(DocSources.ComponentMarkup(sheet.MarkupComponent)), TokenSheet.Library);
}

/// <summary>A stylesheet documented outside the component template (error card, dev kit).</summary>
public sealed record ReferenceSheet(string Slug, string Title, string MarkupComponent, string CssFile, string Href);
