// Theme management JavaScript utilities for BobsComponents
// Provides safe DOM manipulation functions to avoid XSS vulnerabilities

window.BobsComponents = window.BobsComponents || {};

/**
 * Safely sets the theme attribute on the document element
 * @param {string} theme - The theme ID to apply (e.g., 'light', 'dark', 'fire-nation')
 */
window.BobsComponents.setTheme = function(theme) {
    if (typeof theme !== 'string' || theme.trim() === '') {
        console.warn('Invalid theme value provided');
        return;
    }

    // Sanitize theme value - only allow alphanumeric, hyphens, and underscores
    const sanitizedTheme = theme.replace(/[^a-zA-Z0-9\-_]/g, '');

    if (sanitizedTheme !== theme) {
        console.warn('Theme value contained invalid characters and was sanitized');
    }

    document.documentElement.setAttribute('data-theme', sanitizedTheme);
};
