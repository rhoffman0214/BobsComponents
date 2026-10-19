# SelectField
Seeded from BobFit ACCESSIBILITY.md: "Forms and labels (1.3.1, 3.3.2)".

- The control is a native `select` inside a `label` with visible text, so it has an accessible name and clicking the text focuses it (1.3.1, 3.3.2). [test: SelectFieldTests.LabelWrapsANativeSelect]
- It is a real `select`, so the keyboard behavior and the touch picker come from the browser (2.1.1). [test: SelectFieldTests.LabelWrapsANativeSelect]
- `Required` and `Disabled` reach the native control, so assistive tech reports them. [test: SelectFieldTests.RequiredAndDisabled_ReachTheSelect]
- Choosing an option raises `ValueChanged` with its value; the placeholder reports null. [test: SelectFieldTests.ChoosingAnOption_RaisesValueChanged]
- The label is required; an empty one is a parameter error, so no select ships unnamed. [test: SelectFieldTests.MissingLabel_IsAParameterError]
