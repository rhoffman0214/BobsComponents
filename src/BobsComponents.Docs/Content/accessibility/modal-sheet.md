# ModalSheet
Seeded from BobFit ACCESSIBILITY.md: "Modal dialogs (2.4.3, 4.1.2)".

- The sheet is `role="dialog"` with `aria-modal="true"` and an accessible name from `Title` or `AriaLabel` (4.1.2). [test: ModalSheetTests.Open_IsAModalDialogWithAccessibleName]
- A custom `HeaderContent` without a `Title` needs an `AriaLabel`; otherwise the sheet reports a parameter error instead of rendering an unnamed dialog. [test: ModalSheetTests.CustomHeader_NeedsAnAriaLabel]
- Focus moves into the dialog on open and Tab and Shift+Tab are trapped inside it by `js/focusTrap.js`; `aria-modal` alone does not stop real keyboard focus leaving a dialog (2.4.3). A field marked `autofocus` is focused first. [test: ModalSheetTests.Open_LoadsBothModulesFromStaticAssetPaths_AndTrapsFocusOnTheSheet]
- Closing releases the trap, which returns focus to whatever opened the sheet (2.4.3). [test: ModalSheetTests.Closing_ReleasesTrap_AndPopsHistory]
- Escape, the backdrop and the close button all call `OnClose`; other keys do not. [test: ModalSheetTests.Escape_BackdropAndCloseButton_AllCallOnClose_ButOtherKeysDoNot]
- Stacked sheets each keep their own trap and previously focused element; only the top one reacts to Tab. [test: ModalSheetTests.StackedSheets_EachGetTheirOwnTrap]
- The browser or Android back button closes the sheet instead of leaving the page (`js/modalHistory.js`). [test: ModalSheetTests.BrowserBackButton_CallsOnClose_WithoutPoppingHistoryAgain]
- The close button is at least 44x44px, has `aria-label="Close"` and its icon is `aria-hidden`. [test: ModalSheetTests.Open_IsAModalDialogWithAccessibleName]
- A missing script surfaces as a missing-asset error that names the script path. [test: ModalSheetTests.MissingFocusTrapScript_SurfacesAsMissingAssetNamingTheScript]
- gap: focus restoration is implemented in focusTrap.js and covered here only through the interop call; there is no browser-level test yet.
