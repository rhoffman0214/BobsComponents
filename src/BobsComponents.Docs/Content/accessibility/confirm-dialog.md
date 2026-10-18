# ConfirmDialog
Seeded from BobFit ACCESSIBILITY.md: "Modal dialogs (2.4.3, 4.1.2)".

- It is built on ModalSheet, so it is a named modal dialog with a focus trap, Escape to cancel and focus return (see ModalSheet). [test: ConfirmDialogTests.ShowsTitleMessageAndLabels_WithoutBootstrapClasses]
- Confirm and Cancel are real buttons with visible text labels (4.1.2). [test: ConfirmDialogTests.ConfirmAndCancel_CallTheirCallbacks]
- While Busy the confirm button is disabled and its busy label is announced, so a double confirm cannot happen (4.1.3). [test: ConfirmDialogTests.Busy_DisablesConfirm_AndAnnouncesItsAction]
- The destructive look (`Danger`) is never the only cue: the button text says what will happen. [test: ConfirmDialogTests.Danger_False_UsesPrimaryLook]
- A confirm handler that throws becomes a callback error card instead of a silent failure. [test: ConfirmDialogTests.ThrowingConfirmHandler_BecomesACallbackError]
