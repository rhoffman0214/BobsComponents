# ProgressBar
Seeded from BobFit ACCESSIBILITY.md: "Name, role, value (4.1.2)" and "Reduced motion (2.3.3)".

- The root is `role="progressbar"` with `aria-valuemin`, `aria-valuemax` and `aria-valuenow`, so assistive tech reads the current value (4.1.2). [test: ProgressBarTests.IsAProgressbar_WithNameAndValue]
- The `Label` is required and becomes `aria-label`, so every bar has a name (4.1.2). [test: ProgressBarTests.MissingLabel_IsAParameterError]
- The percent text is `aria-hidden`, so the value is not read twice. [test: ProgressBarTests.PercentText_IsHiddenFromAssistiveTech]
- An indeterminate bar (no `Value`) omits `aria-valuenow`, which is how a bar of unknown length is announced. [test: ProgressBarTests.NoValue_IsIndeterminate_WithoutValueNow]
- Values outside 0 to 100 are clamped, so the announced value is always valid. [test: ProgressBarTests.Value_IsClamped]
- Under `prefers-reduced-motion: reduce` the indeterminate slide is skipped. The rule lives once in base.css. [test: ProgressBarTests.ReducedMotionRuleExists]
