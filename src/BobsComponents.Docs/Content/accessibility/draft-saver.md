# DraftSaver
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)".

- When a draft is restored, a visually hidden `role="status"` line says so, so the change is not silent for screen reader users (4.1.3). [test: DraftSaverTests.RestoresOnce_AndAnnounces]
- The component adds nothing else to the page and never moves focus. [test: DraftSaverTests.NoSavedDraft_RendersAnEmptyStatus]
- An empty form never overwrites a draft that is still waiting to be restored, so users do not lose work silently. [test: DraftSaverTests.EmptyForm_DoesNotWipeAPendingDraft]
