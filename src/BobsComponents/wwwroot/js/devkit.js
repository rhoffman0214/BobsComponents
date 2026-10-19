// Helpers for the BobDevKit storage tab and "Copy report" button. Reads key names and sizes only;
// values are never sent back to .NET.
function describe(store) {
    const rows = [];
    try {
        for (let i = 0; i < store.length; i++) {
            const key = store.key(i);
            const value = store.getItem(key) ?? "";
            rows.push({ key, chars: value.length });
        }
    } catch {
        // Storage blocked (private mode, disabled site data): report an empty list.
    }
    return rows.sort((a, b) => a.key.localeCompare(b.key));
}

export function storage() {
    return { local: describe(window.localStorage), session: describe(window.sessionStorage) };
}

export function removeKey(kind, key) {
    try { (kind === "session" ? window.sessionStorage : window.localStorage).removeItem(key); } catch { }
}

export async function copyText(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        return false;
    }
}
