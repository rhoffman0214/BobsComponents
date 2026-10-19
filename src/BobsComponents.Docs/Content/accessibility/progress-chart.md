# ProgressChart
Seeded from BobFit ACCESSIBILITY.md: "Non-text content (1.1.1)" and "Info and relationships (1.3.1)".

- The plot is an SVG with `role="img"` and an accessible name from `Label` (1.1.1). [test: ProgressChartTests.PlotIsANamedImage]
- The same data is available to assistive technology as a visually hidden table with a caption and column headers, so the values are not locked inside a picture (1.3.1). [test: ProgressChartTests.DataIsAlsoATable_InDateOrder]
- With fewer than two points the chart is replaced by plain text rather than an empty graphic. [test: ProgressChartTests.FewerThanTwoPoints_ShowsEmptyText]
- The line is not the only carrier of meaning: the lowest and highest values and the first and last dates are printed as text beside the plot. [test: ProgressChartTests.AxesShowRangeAsText]
