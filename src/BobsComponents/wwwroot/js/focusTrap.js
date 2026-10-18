// Focus management for bob ModalSheet: moves focus into the dialog when it opens, traps Tab/Shift+Tab
// inside it while open, and restores focus to whatever triggered the modal once it closes.
// aria-modal="true" only tells assistive tech the rest of the page is inert - it does NOT actually
// stop real keyboard focus from leaving the dialog, so without this a keyboard/screen-reader user
// opening a modal got no cue focus had moved anywhere, could Tab into content
// hidden behind the backdrop, and lost their place in the page entirely once the modal closed.
//
// Stack-based (not a single set of module-level fields) because every ModalSheet instance on the
// page imports and calls into this same module - two sheets can legitimately be open at once (e.g.
// a ConfirmDialog layered over another ModalSheet), and each needs its own previously-focused
// element and its own Tab boundary rather than clobbering the other's.

const stack = [];

function focusableElements(root) {
    return Array.from(root.querySelectorAll(
        'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
    )).filter(el => el.offsetParent !== null);
}

export function trap(sheetElement) {
    const entry = {
        previouslyFocused: document.activeElement,
        sheetElement,
        keydownHandler: null,
    };

    entry.keydownHandler = (e) => {
        // Only the top-most (most recently opened) sheet should react - an outer sheet's listener
        // must stay registered (it's how focus gets restored to it later) but stay quiet while a
        // nested one is active.
        if (stack[stack.length - 1] !== entry || e.key !== "Tab") return;

        const current = focusableElements(sheetElement);
        if (current.length === 0) {
            e.preventDefault();
            return;
        }

        const first = current[0];
        const last = current[current.length - 1];

        if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        }
    };
    // Capture phase so this runs before any other Tab handling on the page.
    document.addEventListener("keydown", entry.keydownHandler, true);
    stack.push(entry);

    // A caller can mark one of its own fields [autofocus] to ask for something more specific than
    // "the first focusable thing in the header" - honor that over the generic first-focusable fallback.
    const focusables = focusableElements(sheetElement);
    const preferred = sheetElement.querySelector("[autofocus]");
    (preferred ?? focusables[0] ?? sheetElement).focus();
}

export function release() {
    const entry = stack.pop();
    if (!entry) return;

    document.removeEventListener("keydown", entry.keydownHandler, true);

    // Whatever triggered the modal (e.g. a row's delete icon) can itself have been removed from
    // the DOM by the action the modal just completed - guard with contains() rather than assuming
    // focus() is always safe to call on a stale reference.
    if (entry.previouslyFocused && document.contains(entry.previouslyFocused)) {
        entry.previouslyFocused.focus();
    }
}
