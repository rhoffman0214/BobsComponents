# Toasts
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)".

- The host is an `aria-live="polite"` region, so a confirmation is announced without moving focus (4.1.3). [test: ToastTests.Host_IsAPoliteLiveRegion]
- Error toasts add `role="alert"`; other levels do not interrupt. The icons are `aria-hidden`, so the toast text itself must carry the meaning; color and icon only reinforce it (1.4.1). [test: ToastTests.ErrorToast_IsAnAlert_OthersAreNot_AndIconsAreDecorative]
- Toasts dismiss themselves after their duration; an explicit duration wins over the default, and `MaxVisible` drops the oldest. [test: ToastTests.ExplicitDuration_Wins_AndMaxVisibleDropsTheOldest]
- The host unsubscribes when disposed, so a removed layout never receives stale toasts. [test: ToastTests.Unsubscribes_OnDispose]
- Without a registered `IBobToastService` the host reports the missing service by name. [test: ToastTests.WithoutRegistration_ToastHostReportsTheMissingService]
- gap: toasts disappear on a timer, so a screen reader user who is mid-sentence can miss one; keep anything essential in the page too.
