/**
 * Console Error Monitor for Deployment Testing
 *
 * This script captures and logs console errors, warnings, and network failures
 * to help identify deployment issues in production/staging environments.
 *
 * Features:
 * - Captures console.error, console.warn messages
 * - Logs errors to localStorage for later retrieval
 * - Tracks timestamps and stack traces
 * - Monitors unhandled promise rejections
 * - Tracks global JavaScript errors
 */

(function() {
    'use strict';

    const MAX_LOG_ENTRIES = 100;
    const STORAGE_KEY = 'console-monitor-logs';

    // Error log storage
    let errorLogs = [];

    // Load existing logs from localStorage
    function loadLogs() {
        try {
            const stored = localStorage.getItem(STORAGE_KEY);
            if (stored) {
                errorLogs = JSON.parse(stored);
            }
        } catch (e) {
            console.warn('Failed to load console monitor logs', e);
        }
    }

    // Save logs to localStorage
    function saveLogs() {
        try {
            // Keep only the last MAX_LOG_ENTRIES
            if (errorLogs.length > MAX_LOG_ENTRIES) {
                errorLogs = errorLogs.slice(-MAX_LOG_ENTRIES);
            }
            localStorage.setItem(STORAGE_KEY, JSON.stringify(errorLogs));
        } catch (e) {
            console.warn('Failed to save console monitor logs', e);
        }
    }

    // Add a log entry
    function addLog(type, message, stack, data) {
        const entry = {
            timestamp: new Date().toISOString(),
            type: type,
            message: String(message),
            stack: stack || '',
            data: data || {},
            userAgent: navigator.userAgent,
            url: window.location.href
        };

        errorLogs.push(entry);
        saveLogs();

        // Also log to original console for debugging
        if (type === 'error') {
            originalConsole.error('[Console Monitor]', entry);
        } else if (type === 'warn') {
            originalConsole.warn('[Console Monitor]', entry);
        }
    }

    // Store original console methods
    const originalConsole = {
        error: console.error,
        warn: console.warn,
        info: console.info,
        log: console.log
    };

    // Override console.error
    console.error = function(...args) {
        const message = args.map(arg => {
            if (arg instanceof Error) {
                return arg.message;
            }
            return String(arg);
        }).join(' ');

        const stack = args.find(arg => arg instanceof Error)?.stack || new Error().stack;

        addLog('error', message, stack, { args });
        originalConsole.error.apply(console, args);
    };

    // Override console.warn
    console.warn = function(...args) {
        const message = args.map(arg => String(arg)).join(' ');

        addLog('warn', message, '', { args });
        originalConsole.warn.apply(console, args);
    };

    // Capture unhandled promise rejections
    window.addEventListener('unhandledrejection', function(event) {
        const message = event.reason?.message || event.reason || 'Unhandled Promise Rejection';
        const stack = event.reason?.stack || '';

        addLog('error', `Unhandled Promise Rejection: ${message}`, stack, {
            reason: event.reason,
            promise: event.promise
        });
    });

    // Capture global JavaScript errors
    window.addEventListener('error', function(event) {
        const message = event.message || 'JavaScript Error';
        const stack = event.error?.stack || '';

        addLog('error', message, stack, {
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno,
            error: event.error
        });
    });

    // Public API for retrieving logs
    window.ConsoleMonitor = {
        // Get all logged errors
        getLogs: function() {
            return [...errorLogs];
        },

        // Get errors only
        getErrors: function() {
            return errorLogs.filter(log => log.type === 'error');
        },

        // Get warnings only
        getWarnings: function() {
            return errorLogs.filter(log => log.type === 'warn');
        },

        // Clear all logs
        clearLogs: function() {
            errorLogs = [];
            localStorage.removeItem(STORAGE_KEY);
            originalConsole.info('Console monitor logs cleared');
        },

        // Export logs as JSON
        exportLogs: function() {
            const blob = new Blob([JSON.stringify(errorLogs, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `console-logs-${new Date().toISOString()}.json`;
            a.click();
            URL.revokeObjectURL(url);
            originalConsole.info('Console logs exported');
        },

        // Print summary to console
        printSummary: function() {
            const errors = this.getErrors();
            const warnings = this.getWarnings();

            originalConsole.group('Console Monitor Summary');
            originalConsole.log(`Total Entries: ${errorLogs.length}`);
            originalConsole.log(`Errors: ${errors.length}`);
            originalConsole.log(`Warnings: ${warnings.length}`);

            if (errors.length > 0) {
                originalConsole.group('Recent Errors');
                errors.slice(-5).forEach(log => {
                    originalConsole.error(`[${log.timestamp}] ${log.message}`);
                });
                originalConsole.groupEnd();
            }

            if (warnings.length > 0) {
                originalConsole.group('Recent Warnings');
                warnings.slice(-5).forEach(log => {
                    originalConsole.warn(`[${log.timestamp}] ${log.message}`);
                });
                originalConsole.groupEnd();
            }

            originalConsole.groupEnd();
        }
    };

    // Load existing logs on initialization
    loadLogs();

    // Log initialization
    originalConsole.info('%c[Console Monitor] Initialized', 'color: #0d6efd; font-weight: bold;');
    originalConsole.info('Use window.ConsoleMonitor to access logged errors:');
    originalConsole.info('  - ConsoleMonitor.getLogs() - Get all logs');
    originalConsole.info('  - ConsoleMonitor.getErrors() - Get errors only');
    originalConsole.info('  - ConsoleMonitor.getWarnings() - Get warnings only');
    originalConsole.info('  - ConsoleMonitor.clearLogs() - Clear all logs');
    originalConsole.info('  - ConsoleMonitor.exportLogs() - Export logs as JSON');
    originalConsole.info('  - ConsoleMonitor.printSummary() - Print summary');

})();
