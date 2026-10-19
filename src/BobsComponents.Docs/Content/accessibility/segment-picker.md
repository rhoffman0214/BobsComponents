# SegmentPicker
Seeded from BobFit ACCESSIBILITY.md: "Toggle state (4.1.2)", "Keyboard (2.1.1)" and "Live regions (4.1.3)".

- Preset chips are buttons with `aria-pressed`, grouped and named by the title (4.1.2, 1.3.1). [test: SegmentPickerTests.PresetChips_ExposePressedState_AndToggle]
- Reordering has a keyboard path with no pointer needed: Space or Enter on the grip picks an area up, the arrow keys move it, Escape drops it (2.1.1). [test: SegmentPickerTests.Keyboard_PickUp_AndMove_Reorders]
- Every change is announced through a `role="status"` region (4.1.3). [test: SegmentPickerTests.Changes_AreAnnounced]
- Grip and remove buttons carry the area name in their accessible names (2.4.6). [test: SegmentPickerTests.GripAndRemove_AreNamedAfterTheArea]
- The custom-area box has an accessible name and Enter adds the area (3.3.2). [test: SegmentPickerTests.CustomArea_AddsOnEnter_AndIgnoresDuplicates]
- Disabling the picker disables every control. [test: SegmentPickerTests.Disabled_DisablesEveryControl]
