# AsyncButton
Seeded from BobFit ACCESSIBILITY.md: "Keyboard operability (2.1.1)", "Visible focus (2.4.7)" and "Live regions (4.1.3)".

- It is a real `<button>`, so Enter and Space work and it is in the tab order (2.1.1). [test: AsyncButtonTests.Idle_ShowsContent_AndIsEnabled]
- While Busy the button is disabled and carries `aria-busy="true"`; its content is replaced by a spinner whose label (`BusyLabel`) is announced as a polite status (4.1.3). [test: AsyncButtonTests.Busy_DisablesAndSwapsContentForAnAnnouncedSpinner]
- `Disabled` is honored natively, so a disabled button cannot be activated by keyboard or pointer. [test: AsyncButtonTests.Disabled_IsHonored]
- Visible keyboard focus comes from the shared `:focus-visible` rule in base.css (2.4.7). [test: SharedAccessibilityTests.FocusVisibleRule_AppliesToEveryBobElement_WithAnOutline]
- The minimum height defaults to 2.75rem (44px), so the touch target stays large (2.5.5). [test: SharedAccessibilityTests.ButtonMinimumHeight_IsAtLeastA44PixelTouchTarget]
- Extra attributes such as `aria-label` flow to the button, so an icon-only button can be named (4.1.2). [test: AsyncButtonTests.ExtraAttributesFlowToTheButton]
- An invalid `Type` is reported as a parameter error that names the value instead of rendering a broken button. [test: AsyncButtonTests.InvalidType_IsAParameterError_NamingTheValue]
