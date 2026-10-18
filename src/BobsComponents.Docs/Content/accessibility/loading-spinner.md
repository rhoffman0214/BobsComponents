# LoadingSpinner
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)" and "Reduced motion (2.3.3)".

- The root is `role="status"` with `aria-live="polite"`, so a wait is announced without moving focus (WCAG 4.1.3). [test: LoadingSpinnerTests.IsAPoliteStatusRegion_WithDecorativeIcon]
- The spinning icon is `aria-hidden="true"`; the label text is the one thing a screen reader announces (1.1.1). [test: LoadingSpinnerTests.IsAPoliteStatusRegion_WithDecorativeIcon]
- `ShowLabel="false"` hides the label visually with `bob-visually-hidden` but keeps it in the DOM, so it is still announced. [test: LoadingSpinnerTests.HiddenLabel_IsStillInTheDom_ForScreenReaders]
- Under `prefers-reduced-motion: reduce` the spin is skipped; the label text still appears (2.3.3). The rule lives once in base.css. [test: LoadingSpinnerTests.ReducedMotionRuleExists]
