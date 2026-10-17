// Custom Swagger UI JavaScript

(function() {
    'use strict';

    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        console.log('Swagger UI Custom JS Loaded');

        // Add custom title animation
        setTimeout(function() {
            const title = document.querySelector('.info .title');
            if (title) {
                title.style.animation = 'fadeIn 1s ease-in';
            }
        }, 100);

        // Add version selector if multiple versions exist
        addVersionSelector();

        // Add export functionality
        addExportButton();

        // Add search functionality enhancement
        enhanceSearch();

        // Add keyboard shortcuts
        addKeyboardShortcuts();

        // Add response time tracking
        trackResponseTimes();

        // Add copy to clipboard for curl commands
        addCopyToClipboard();
    });

    // Add version selector
    function addVersionSelector() {
        const schemeContainer = document.querySelector('.scheme-container');
        if (schemeContainer) {
            const versionSelector = document.createElement('select');
            versionSelector.id = 'api-version-selector';
            versionSelector.style.cssText = 'margin-left: 10px; padding: 5px; border-radius: 3px; border: 1px solid #ccc;';
            
            const versions = ['v1', 'v2', 'v3'];
            versions.forEach(version => {
                const option = document.createElement('option');
                option.value = version;
                option.textContent = `API ${version}`;
                versionSelector.appendChild(option);
            });

            versionSelector.addEventListener('change', function(e) {
                console.log('API Version changed to:', e.target.value);
                // Add version switching logic here
            });

            const label = document.createElement('label');
            label.textContent = 'API Version: ';
            label.appendChild(versionSelector);

            schemeContainer.insertBefore(label, schemeContainer.firstChild);
        }
    }

    // Add export button
    function addExportButton() {
        const info = document.querySelector('.info');
        if (info) {
            const exportBtn = document.createElement('button');
            exportBtn.textContent = 'Export API Documentation';
            exportBtn.style.cssText = `
                background: #4CAF50;
                color: white;
                border: none;
                padding: 10px 20px;
                border-radius: 5px;
                cursor: pointer;
                margin-top: 20px;
                font-size: 14px;
            `;

            exportBtn.addEventListener('click', function() {
                const swaggerData = getSwaggerData();
                downloadJSON(swaggerData, 'api-documentation.json');
            });

            info.appendChild(exportBtn);
        }
    }

    // Get Swagger data
    function getSwaggerData() {
        // This would normally fetch the actual Swagger JSON
        return {
            message: 'API Documentation Export',
            timestamp: new Date().toISOString(),
            version: document.querySelector('#api-version-selector')?.value || 'v1'
        };
    }

    // Download JSON file
    function downloadJSON(data, filename) {
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }

    // Enhance search functionality
    function enhanceSearch() {
        const searchBox = document.querySelector('.filter-container input');
        if (searchBox) {
            searchBox.placeholder = 'Search endpoints (use / for regex)...';
            
            // Add search suggestions
            const suggestions = ['GET', 'POST', 'PUT', 'DELETE', 'auth', 'user', 'admin'];
            const datalist = document.createElement('datalist');
            datalist.id = 'search-suggestions';
            
            suggestions.forEach(suggestion => {
                const option = document.createElement('option');
                option.value = suggestion;
                datalist.appendChild(option);
            });
            
            searchBox.setAttribute('list', 'search-suggestions');
            searchBox.parentNode.appendChild(datalist);
        }
    }

    // Add keyboard shortcuts
    function addKeyboardShortcuts() {
        document.addEventListener('keydown', function(e) {
            // Ctrl/Cmd + K for search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchBox = document.querySelector('.filter-container input');
                if (searchBox) {
                    searchBox.focus();
                    searchBox.select();
                }
            }

            // Ctrl/Cmd + E to expand all
            if ((e.ctrlKey || e.metaKey) && e.key === 'e') {
                e.preventDefault();
                const expandButtons = document.querySelectorAll('.expand-operation');
                expandButtons.forEach(btn => btn.click());
            }

            // Ctrl/Cmd + C to collapse all
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'C') {
                e.preventDefault();
                const collapseButtons = document.querySelectorAll('.collapse-operation');
                collapseButtons.forEach(btn => btn.click());
            }
        });

        // Add keyboard shortcuts help
        const shortcutsHelp = document.createElement('div');
        shortcutsHelp.innerHTML = `
            <details style="margin: 20px; padding: 10px; background: #f0f0f0; border-radius: 5px;">
                <summary style="cursor: pointer; font-weight: bold;">⌨️ Keyboard Shortcuts</summary>
                <ul style="margin-top: 10px;">
                    <li><kbd>Ctrl</kbd> + <kbd>K</kbd> - Focus search</li>
                    <li><kbd>Ctrl</kbd> + <kbd>E</kbd> - Expand all operations</li>
                    <li><kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>C</kbd> - Collapse all operations</li>
                </ul>
            </details>
        `;
        
        const container = document.querySelector('.swagger-ui');
        if (container) {
            container.insertBefore(shortcutsHelp, container.firstChild);
        }
    }

    // Track response times
    function trackResponseTimes() {
        const originalFetch = window.fetch;
        window.fetch = function(...args) {
            const startTime = performance.now();
            
            return originalFetch.apply(this, args).then(response => {
                const endTime = performance.now();
                const duration = Math.round(endTime - startTime);
                
                console.log(`API Request took ${duration}ms:`, args[0]);
                
                // Display response time in UI
                const responseTimeDisplay = document.createElement('div');
                responseTimeDisplay.style.cssText = `
                    position: fixed;
                    bottom: 20px;
                    right: 20px;
                    background: ${duration < 500 ? '#4CAF50' : duration < 1000 ? '#FFC107' : '#F44336'};
                    color: white;
                    padding: 10px 15px;
                    border-radius: 5px;
                    z-index: 9999;
                    animation: fadeInOut 3s ease-in-out;
                `;
                responseTimeDisplay.textContent = `Response time: ${duration}ms`;
                document.body.appendChild(responseTimeDisplay);
                
                setTimeout(() => {
                    responseTimeDisplay.remove();
                }, 3000);
                
                return response;
            });
        };
    }

    // Add copy to clipboard functionality
    function addCopyToClipboard() {
        // Watch for curl command elements
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                mutation.addedNodes.forEach(function(node) {
                    if (node.nodeType === 1 && node.classList && node.classList.contains('curl-command')) {
                        addCopyButton(node);
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    // Add copy button to element
    function addCopyButton(element) {
        const copyBtn = document.createElement('button');
        copyBtn.textContent = '📋 Copy';
        copyBtn.style.cssText = `
            margin-left: 10px;
            padding: 5px 10px;
            background: #007bff;
            color: white;
            border: none;
            border-radius: 3px;
            cursor: pointer;
        `;

        copyBtn.addEventListener('click', function() {
            const text = element.textContent;
            navigator.clipboard.writeText(text).then(() => {
                copyBtn.textContent = '✅ Copied!';
                setTimeout(() => {
                    copyBtn.textContent = '📋 Copy';
                }, 2000);
            });
        });

        element.appendChild(copyBtn);
    }

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(-20px); }
            to { opacity: 1; transform: translateY(0); }
        }
        
        @keyframes fadeInOut {
            0% { opacity: 0; transform: translateX(100px); }
            20% { opacity: 1; transform: translateX(0); }
            80% { opacity: 1; transform: translateX(0); }
            100% { opacity: 0; transform: translateX(100px); }
        }
        
        .swagger-ui .info .title {
            animation: fadeIn 1s ease-in;
        }
    `;
    document.head.appendChild(style);

    // Log successful initialization
    console.log('Swagger UI customization complete!');
})();