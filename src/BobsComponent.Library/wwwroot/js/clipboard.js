// Minimal JavaScript interop for clipboard functionality
export function copyToClipboard(text) {
    return navigator.clipboard.writeText(text)
        .then(() => true)
        .catch((error) => {
            console.error('Failed to copy to clipboard:', error);
            return false;
        });
}
