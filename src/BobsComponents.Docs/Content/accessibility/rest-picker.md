# RestPicker
Seeded from BobFit ACCESSIBILITY.md: "Forms and labels (1.3.1, 3.3.2)" and "Toggle state (4.1.2)".

- The control is a `fieldset` with a `legend`, so the chips and the number box are announced as one named group (1.3.1). [test: RestPickerTests.IsAFieldsetWithALegend]
- Each chip is a real button with `aria-pressed` reflecting whether it is the current length (4.1.2), so the on state is not conveyed by color alone. [test: RestPickerTests.ChipsExposeTheirPressedState]
- The Other box has a visible text label wrapping the input (3.3.2). [test: RestPickerTests.OtherBoxIsLabelled]
- Disabling the picker disables the whole fieldset, so no chip or box can be operated. [test: RestPickerTests.Disabled_DisablesTheGroup]
- Chips are focusable buttons and the box is a number input, so everything works from the keyboard. [test: RestPickerTests.ChipsAreKeyboardOperableButtons]
