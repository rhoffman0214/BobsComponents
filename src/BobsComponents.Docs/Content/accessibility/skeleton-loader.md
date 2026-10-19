# SkeletonLoader
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)" and "Reduced motion (2.3.3)".

- The group is `role="status"` with `aria-busy="true"`, so the wait is announced once without moving focus (4.1.3). [test: SkeletonLoaderTests.IsOneBusyStatusGroup]
- The shapes are `aria-hidden`; only the visually hidden label is read, so five placeholder lines do not become five announcements (1.1.1). [test: SkeletonLoaderTests.ShapesAreHidden_AndOnlyTheLabelIsRead]
- Width, height and radius are sanitized to CSS length characters before they reach a style, so a parameter cannot add declarations. [test: SkeletonLoaderTests.SizeValues_AreSanitized]
- A count outside 1 to 100 is a parameter error. [test: SkeletonLoaderTests.CountOutOfRange_IsAParameterError]
- Under `prefers-reduced-motion: reduce` the shimmer is skipped and the grey blocks stay. The rule lives once in base.css. [test: SkeletonLoaderTests.ReducedMotionRuleExists]
