# ElapsedClock
Seeded from BobFit ACCESSIBILITY.md: "Live regions (4.1.3)".

- The root is `role="timer"` with `aria-live="off"`, so a screen reader can read the time on demand but is not interrupted every second (4.1.3). [test: ElapsedClockTests.IsATimer_ThatDoesNotAnnounceEverySecond]
- The text is plain `mm:ss` or `h:mm:ss`, so it is readable without any visual styling (1.3.1). [test: ElapsedClockTests.FormatsMinutesAndSeconds_ThenHours]
- A paused clock stops changing, so the reading stays stable while the user works with it (2.2.2). [test: ElapsedClockTests.PausedClock_DoesNotAdvance]
- The timer is disposed with the component, so nothing keeps ticking after navigation. [test: ElapsedClockTests.DisposingStopsTheTimer]
