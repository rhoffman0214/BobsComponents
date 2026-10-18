# FullScreenLoader
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)".

- The root is `role="status"` with `aria-live="polite"`, so a login redirect or route check is announced without moving focus (4.1.3). [test: FullScreenLoaderTests.ShowsAndAnnouncesMessage]
- The message is shown as visible text and is also the embedded spinner's hidden label, so the status always carries it. [test: FullScreenLoaderTests.ShowsAndAnnouncesMessage]
- An empty message is a parameter error, so the page never shows a spinner with no explanation. [test: FullScreenLoaderTests.EmptyMessage_IsAParameterError]
