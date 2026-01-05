# Deployment Testing Guide

## Console Error Monitoring (Phase 3.5)

This guide helps you test deployments by monitoring console errors in real-time.

---

## Quick Start

1. **Deploy to staging/production**
2. **Open browser DevTools** (F12)
3. **Navigate through the app**
4. **Check Console Monitor** using `ConsoleMonitor.printSummary()`

---

## Console Monitor API

The Console Monitor automatically captures errors and warnings. Access it via the browser console:

### View All Logs
```javascript
ConsoleMonitor.getLogs()
// Returns: Array of all logged entries (errors + warnings)
```

### View Errors Only
```javascript
ConsoleMonitor.getErrors()
// Returns: Array of error entries only
```

### View Warnings Only
```javascript
ConsoleMonitor.getWarnings()
// Returns: Array of warning entries only
```

### Print Summary
```javascript
ConsoleMonitor.printSummary()
// Outputs: Formatted summary with recent errors and warnings
```

### Export Logs
```javascript
ConsoleMonitor.exportLogs()
// Downloads: JSON file with all captured logs
```

### Clear Logs
```javascript
ConsoleMonitor.clearLogs()
// Clears: All stored logs from localStorage
```

---

## Deployment Testing Checklist

### Before Deployment
- [ ] Build in Release mode: `dotnet build -c Release`
- [ ] Run AOT compilation (if enabled)
- [ ] Check for build warnings
- [ ] Test locally with production settings

### After Deployment
- [ ] Open browser DevTools (F12)
- [ ] Navigate to Console tab
- [ ] Clear existing console messages
- [ ] Navigate through all major pages
- [ ] Trigger all interactive features

### Pages to Test
- [ ] Home page (`/`)
- [ ] Input Examples - Buttons (`/inputexamples/button`)
- [ ] Theme picker functionality
- [ ] All navigation menu items
- [ ] Interactive playground (if applicable)

### Features to Test
- [ ] Button clicks (all styles/sizes)
- [ ] Form submissions
- [ ] Modal dialogs
- [ ] Theme switching
- [ ] Async operations
- [ ] Copy-to-clipboard functionality
- [ ] Code snippet rendering
- [ ] Responsive layout (resize browser)

### Check for Common Errors

#### 1. **CSS Bundle 404 Errors**
```
Failed to load resource: net::ERR_ABORTED 404
_content/BobsComponent.Library/BobsComponent.Library.*.bundle.scp.css
```
**Fix**: Rebuild project, verify scoped CSS files exist

#### 2. **JavaScript Interop Failures**
```
Error: Could not find 'functionName' in 'window.ModuleName'
```
**Fix**: Verify JavaScript module is loaded, check file paths

#### 3. **Missing Static Assets**
```
Failed to load resource: 404 (Not Found)
/images/logo.png
```
**Fix**: Verify wwwroot files are included in publish output

#### 4. **CORS Policy Violations**
```
Access to fetch at '...' from origin '...' has been blocked by CORS policy
```
**Fix**: Configure CORS headers on API server

#### 5. **WebSocket Connection Failures**
```
WebSocket connection to 'wss://...' failed
```
**Fix**: Check WebSocket support, proxy configuration

#### 6. **Module Loading Errors**
```
Failed to load module script: Expected a JavaScript module script
```
**Fix**: Verify module type="module" in script tags

---

## Browser DevTools Tips

### Console Tab
- **Clear Console**: Click trash icon or Ctrl+L
- **Filter Errors**: Click "Errors" button
- **Preserve Log**: Enable to keep logs across page navigations
- **Group Similar**: Enable to reduce duplicate messages

### Network Tab
- **Check Failed Requests**: Filter by "Has blocked cookies" or "4xx-5xx"
- **Check Timing**: Look for slow requests (red/orange)
- **Check Size**: Verify bundle sizes are reasonable
- **Check Cache**: Verify caching headers (304 responses)

### Application Tab
- **localStorage**: Check Console Monitor logs
- **sessionStorage**: Check for stored data
- **IndexedDB**: Check for Blazor caches
- **Service Workers**: Verify registration (if using PWA)

---

## Error Reporting Template

When reporting deployment errors, include:

### 1. Environment Info
- Deployment URL: `https://example.com`
- Browser: Chrome 120.0.6099.109
- OS: Windows 11
- Device: Desktop/Mobile/Tablet

### 2. Error Details
```javascript
// Run this in console to get error details
ConsoleMonitor.printSummary()
```

### 3. Reproduction Steps
1. Navigate to `/inputexamples/button`
2. Click "Primary Button" example
3. Error appears in console

### 4. Screenshots
- Console error screenshot
- Network tab screenshot (if relevant)
- Visual issue screenshot (if applicable)

### 5. Exported Logs
```javascript
// Export logs to file
ConsoleMonitor.exportLogs()
```
Attach the downloaded JSON file to your bug report.

---

## Automated Testing (Future Enhancement)

### Playwright Example
```javascript
test('should not have console errors', async ({ page }) => {
  const errors = [];

  page.on('console', msg => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });

  await page.goto('https://example.com');
  await page.click('button');

  expect(errors).toHaveLength(0);
});
```

### Cypress Example
```javascript
it('should not have console errors', () => {
  cy.visit('/');

  cy.window().then((win) => {
    const errors = win.ConsoleMonitor.getErrors();
    expect(errors).to.have.length(0);
  });
});
```

---

## Troubleshooting

### Console Monitor Not Working

1. **Check if loaded**:
   ```javascript
   typeof ConsoleMonitor !== 'undefined'
   // Should return: true
   ```

2. **Check initialization**:
   ```javascript
   // Look for this message in console
   // [Console Monitor] Initialized
   ```

3. **Check localStorage**:
   ```javascript
   localStorage.getItem('console-monitor-logs')
   // Should return: JSON string or null
   ```

### localStorage Full

If you see errors about localStorage quota:
```javascript
// Clear Console Monitor logs
ConsoleMonitor.clearLogs()

// Or clear all localStorage
localStorage.clear()
```

### Too Many Logs

The Console Monitor keeps only the last 100 entries. If you need more:

1. Export logs frequently: `ConsoleMonitor.exportLogs()`
2. Clear old logs: `ConsoleMonitor.clearLogs()`
3. Filter specific errors: `ConsoleMonitor.getErrors()`

---

## Best Practices

1. **Test After Every Deployment**: Always check console after deploying
2. **Test All Browsers**: Chrome, Firefox, Edge, Safari
3. **Test Responsive**: Desktop, tablet, mobile sizes
4. **Export Logs**: Save logs before clearing
5. **Document Patterns**: If you see recurring errors, document the fix
6. **Monitor Performance**: Use DevTools Performance tab for slow pages
7. **Check Lighthouse**: Run Lighthouse audit for accessibility/performance

---

## Integration with CI/CD

### GitHub Actions Example
```yaml
- name: Check for console errors
  run: |
    npx playwright test --grep "console errors"
```

### Azure DevOps Example
```yaml
- task: Npm@1
  inputs:
    command: 'custom'
    customCommand: 'run test:console-errors'
```

---

## Support

If you encounter persistent errors after deployment:

1. Export logs: `ConsoleMonitor.exportLogs()`
2. Check this guide for common errors
3. Search GitHub issues for similar problems
4. Create new issue with error template above

---

## Changelog

- **v1.0.0** (2026-01-04): Initial console monitor implementation
  - Capture console.error and console.warn
  - Track unhandled promise rejections
  - Global error handler
  - localStorage persistence
  - Export functionality
