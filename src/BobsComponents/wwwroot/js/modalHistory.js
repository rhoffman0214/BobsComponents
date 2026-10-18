// Lets a full-screen "modal" (ModalSheet) be closed by the Android/browser back
// gesture without actually leaving the page it was opened on. Opening pushes one extra history
// entry; the back gesture pops it (popstate fires) instead of navigating away, and we treat that
// exactly like the user tapping the modal's own Close button - no save. Closing via Close/Save
// consumes that same entry programmatically so a second back-press isn't needed afterward.
let armed = false;
let dotNetRef = null;

window.addEventListener("popstate", () => {
    if (armed && dotNetRef) {
        const ref = dotNetRef;
        armed = false;
        dotNetRef = null;
        ref.invokeMethodAsync("OnModalBackButton");
    }
});

export function openModal(dotNetReference) {
    dotNetRef = dotNetReference;
    armed = true;
    history.pushState({ bobModal: true }, "", location.href);
}

export function closeModal() {
    if (armed) {
        armed = false;
        dotNetRef = null;
        history.back();
    }
}
