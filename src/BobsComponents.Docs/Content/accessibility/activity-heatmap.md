# ActivityHeatmap
Seeded from BobFit ACCESSIBILITY.md: "Info and relationships (1.3.1)" and "Use of color (1.4.1)".

- The streak numbers are real text in a description list, each paired with its label (1.3.1). [test: ActivityHeatmapTests.StatsAreTextPairedWithLabels]
- The heatmap is one image with a text summary of totals and streaks, so the colored cells are never the only source of the information (1.4.1). [test: ActivityHeatmapTests.GridHasTextSummary]
- The loading state is announced with `role="status"`. [test: ActivityHeatmapTests.NullTimestamps_ShowsLoadingStatus]
- gap: individual cells are not focusable; their counts are only in tooltips for pointer users.
