// Theme Management
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    updateThemeButton(savedTheme);
}

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    updateThemeButton(newTheme);
}

function updateThemeButton(theme) {
    const button = document.getElementById('theme-toggle');
    if (button) {
        button.textContent = theme === 'dark' ? '☀️ Light' : '🌙 Dark';
    }
}

// Copy to Clipboard
function initCopyButtons() {
    document.querySelectorAll('pre code').forEach((codeBlock) => {
        const pre = codeBlock.parentElement;
        if (!pre.querySelector('.copy-button')) {
            const button = document.createElement('button');
            button.className = 'copy-button';
            button.textContent = 'Copy';
            button.style.position = 'absolute';
            button.style.top = '0.5rem';
            button.style.right = '0.5rem';

            button.addEventListener('click', async () => {
                const code = codeBlock.textContent;
                try {
                    await navigator.clipboard.writeText(code);
                    button.textContent = 'Copied!';
                    setTimeout(() => {
                        button.textContent = 'Copy';
                    }, 2000);
                } catch (err) {
                    console.error('Failed to copy:', err);
                    button.textContent = 'Failed';
                    setTimeout(() => {
                        button.textContent = 'Copy';
                    }, 2000);
                }
            });

            pre.style.position = 'relative';
            pre.appendChild(button);
        }
    });
}

// Collapsible Sections
function initCollapsibles() {
    document.querySelectorAll('.collapsible-header').forEach((header) => {
        header.addEventListener('click', () => {
            const collapsible = header.parentElement;
            collapsible.classList.toggle('open');
        });
    });
}

// Active Navigation Link
function updateActiveNavLink() {
    const currentPath = window.location.pathname;
    const currentFile = currentPath.substring(currentPath.lastIndexOf('/') + 1);

    document.querySelectorAll('.nav-link').forEach((link) => {
        link.classList.remove('active');
        if (link.getAttribute('href') === currentFile) {
            link.classList.add('active');
        }
    });
}

// Mermaid Initialization
function initMermaid() {
    if (typeof mermaid !== 'undefined') {
        mermaid.initialize({
            startOnLoad: true,
            theme: document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'default',
            securityLevel: 'loose',
            flowchart: {
                useMaxWidth: true,
                htmlLabels: true,
                curve: 'basis'
            }
        });
    }
}

// Diagram Zoom Modal
function initDiagramZoom() {
    // Create modal HTML if it doesn't exist
    if (!document.getElementById('diagram-modal')) {
        const modalHTML = `
      <div id="diagram-modal" class="diagram-modal">
        <div class="diagram-modal-content">
          <div class="diagram-modal-controls">
            <button id="zoom-in" class="diagram-control-btn">🔍+</button>
            <button id="zoom-out" class="diagram-control-btn">🔍−</button>
            <button id="zoom-reset" class="diagram-control-btn">↺ Reset</button>
            <button id="close-modal" class="diagram-control-btn">✕ Close</button>
          </div>
          <div class="diagram-modal-body">
            <div id="diagram-container"></div>
          </div>
        </div>
      </div>
    `;
        document.body.insertAdjacentHTML('beforeend', modalHTML);

        // Add CSS for modal
        const style = document.createElement('style');
        style.textContent = `
      .diagram-modal {
        display: none;
        position: fixed;
        z-index: 10000;
        left: 0;
        top: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.9);
        overflow: hidden;
      }
      
      .diagram-modal.active {
        display: flex;
        align-items: center;
        justify-content: center;
      }
      
      .diagram-modal-content {
        width: 95%;
        height: 95%;
        display: flex;
        flex-direction: column;
      }
      
      .diagram-modal-controls {
        background: var(--card-bg);
        padding: 1rem;
        display: flex;
        gap: 0.5rem;
        border-radius: 8px 8px 0 0;
        box-shadow: 0 2px 8px rgba(0,0,0,0.2);
      }
      
      .diagram-control-btn {
        background: var(--primary-color);
        color: white;
        border: none;
        padding: 0.5rem 1rem;
        border-radius: 4px;
        cursor: pointer;
        font-size: 0.9rem;
        font-weight: 600;
        transition: all 0.2s;
      }
      
      .diagram-control-btn:hover {
        background: var(--primary-hover);
        transform: translateY(-1px);
      }
      
      .diagram-modal-body {
        flex: 1;
        background: var(--body-bg);
        overflow: auto;
        padding: 2rem;
        border-radius: 0 0 8px 8px;
        position: relative;
      }
      
      #diagram-container {
        display: inline-block;
        min-width: 100%;
        transform-origin: top left;
        transition: transform 0.3s ease;
      }
      
      #diagram-container svg {
        max-width: none !important;
        height: auto !important;
      }
      
      .mermaid {
        cursor: zoom-in;
        transition: opacity 0.2s;
      }
      
      .mermaid:hover {
        opacity: 0.9;
      }
    `;
        document.head.appendChild(style);
    }

    // Add click handlers to all Mermaid diagrams
    document.querySelectorAll('.mermaid').forEach((diagram) => {
        if (!diagram.hasAttribute('data-zoom-enabled')) {
            diagram.setAttribute('data-zoom-enabled', 'true');
            diagram.style.cursor = 'zoom-in';
            diagram.addEventListener('click', () => openDiagramModal(diagram));
        }
    });

    // Modal controls
    let currentScale = 1;
    const scaleStep = 0.2;
    const modal = document.getElementById('diagram-modal');
    const container = document.getElementById('diagram-container');
    const modalBody = modal?.querySelector('.diagram-modal-body');

    document.getElementById('zoom-in')?.addEventListener('click', () => {
        currentScale += scaleStep;
        container.style.transform = `scale(${currentScale})`;
    });

    document.getElementById('zoom-out')?.addEventListener('click', () => {
        currentScale = Math.max(0.5, currentScale - scaleStep);
        container.style.transform = `scale(${currentScale})`;
    });

    document.getElementById('zoom-reset')?.addEventListener('click', () => {
        currentScale = 1;
        container.style.transform = 'scale(1)';
        if (modalBody) {
            modalBody.scrollTop = 0;
            modalBody.scrollLeft = 0;
        }
    });

    document.getElementById('close-modal')?.addEventListener('click', closeDiagramModal);

    // Close on background click
    modal?.addEventListener('click', (e) => {
        if (e.target === modal) {
            closeDiagramModal();
        }
    });

    // Close on ESC key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal?.classList.contains('active')) {
            closeDiagramModal();
        }
    });

    function openDiagramModal(diagram) {
        const modal = document.getElementById('diagram-modal');
        const container = document.getElementById('diagram-container');

        // Clone the diagram
        const clone = diagram.cloneNode(true);
        container.innerHTML = '';
        container.appendChild(clone);

        // Reset scale
        currentScale = 1;
        container.style.transform = 'scale(1)';

        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeDiagramModal() {
        const modal = document.getElementById('diagram-modal');
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// Smooth Scrolling for Anchor Links
function initSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                const headerOffset = 80;
                const elementPosition = target.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Syntax Highlighting (optional, if using highlight.js)
function initSyntaxHighlighting() {
    if (typeof hljs !== 'undefined') {
        document.querySelectorAll('pre code').forEach((block) => {
            hljs.highlightBlock(block);
        });
    }
}

// Back to Top Button
function initBackToTop() {
    const backToTopButton = document.getElementById('back-to-top');
    if (backToTopButton) {
        window.addEventListener('scroll', () => {
            if (window.pageYOffset > 300) {
                backToTopButton.style.display = 'block';
            } else {
                backToTopButton.style.display = 'none';
            }
        });

        backToTopButton.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }
}

// Search Functionality (simple client-side search)
function initSearch() {
    const searchInput = document.getElementById('search-input');
    const searchResults = document.getElementById('search-results');

    if (searchInput && searchResults) {
        let searchTimeout;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            const query = e.target.value.toLowerCase().trim();

            if (query.length < 2) {
                searchResults.innerHTML = '';
                searchResults.style.display = 'none';
                return;
            }

            searchTimeout = setTimeout(() => {
                performSearch(query, searchResults);
            }, 300);
        });

        // Close search results when clicking outside
        document.addEventListener('click', (e) => {
            if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
                searchResults.style.display = 'none';
            }
        });
    }
}

function performSearch(query, resultsContainer) {
    const searchableElements = document.querySelectorAll('h1, h2, h3, h4, p, li, td');
    const results = [];

    searchableElements.forEach((element) => {
        const text = element.textContent.toLowerCase();
        if (text.includes(query)) {
            const context = text.substring(
                Math.max(0, text.indexOf(query) - 40),
                Math.min(text.length, text.indexOf(query) + query.length + 40)
            );

            results.push({
                element: element,
                context: context,
                heading: findNearestHeading(element)
            });
        }
    });

    displaySearchResults(results, query, resultsContainer);
}

function findNearestHeading(element) {
    let current = element;
    while (current && current !== document.body) {
        if (current.matches('h1, h2, h3, h4, h5, h6')) {
            return current.textContent;
        }
        current = current.previousElementSibling || current.parentElement;
    }
    return 'Unknown Section';
}

function displaySearchResults(results, query, container) {
    if (results.length === 0) {
        container.innerHTML = '<div class="search-no-results">No results found</div>';
        container.style.display = 'block';
        return;
    }

    const uniqueResults = results.slice(0, 10);
    container.innerHTML = uniqueResults.map(result => `
    <div class="search-result">
      <div class="search-result-heading">${result.heading}</div>
      <div class="search-result-context">...${highlightQuery(result.context, query)}...</div>
    </div>
  `).join('');

    container.style.display = 'block';

    // Add click handlers to scroll to results
    container.querySelectorAll('.search-result').forEach((resultDiv, index) => {
        resultDiv.addEventListener('click', () => {
            results[index].element.scrollIntoView({ behavior: 'smooth', block: 'center' });
            container.style.display = 'none';
        });
    });
}

function highlightQuery(text, query) {
    const regex = new RegExp(`(${query})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
}

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    initTheme();
    initCopyButtons();
    initCollapsibles();
    updateActiveNavLink();
    initMermaid();
    initSmoothScrolling();
    initSyntaxHighlighting();
    initBackToTop();
    initSearch();

    // Add theme toggle event listener
    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', toggleTheme);
    }

    // Initialize diagram zoom after a short delay to ensure Mermaid has rendered
    setTimeout(initDiagramZoom, 500);
});

// Re-initialize Mermaid diagrams when theme changes
const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
        if (mutation.attributeName === 'data-theme') {
            initMermaid();
            // Re-render all mermaid diagrams
            if (typeof mermaid !== 'undefined') {
                document.querySelectorAll('.mermaid').forEach((element, index) => {
                    const graphDefinition = element.textContent;
                    element.removeAttribute('data-processed');
                    element.innerHTML = graphDefinition;
                });
                mermaid.init(undefined, document.querySelectorAll('.mermaid'));
            }
        }
    });
});

observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['data-theme']
});
