# NotificationBell
Seeded from BobFit ACCESSIBILITY.md: "Name, role, value (4.1.2)" and "Keyboard (2.1.1)".

- The bell is a real button whose name includes the unread count, and `aria-expanded` reports whether the panel is open (4.1.2). [test: NotificationBellTests.ButtonNameIncludesUnreadCount]
- The badge is hidden from assistive technology because the count is already in the button's name. [test: NotificationBellTests.ButtonNameIncludesUnreadCount]
- Escape closes the panel and returns focus to the bell (2.1.1). [test: NotificationBellTests.Escape_ClosesPanel]
- Linked entries are buttons, so they work with the keyboard. [test: NotificationBellTests.LinkedEntry_RaisesOnNavigateAndCloses]
- The loading state is announced with `role="status"`. [test: NotificationBellTests.OpeningLoadsListAndMarksRead]
- gap: focus is not moved into the panel when it opens, and it is not trapped; Tab continues through the page.
