/**
 * Table of Contents - Auto-collapse on scroll behavior
 * Shrinks TOC when scrolling, expands on hover
 */

window.TableOfContents = {
    scrollToSection: function(sectionId) {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    },

    init: function(tocElement) {
        if (!tocElement) return;

        let collapseTimeout;
        let lastScrollY = window.scrollY;
        const scrollThreshold = 100; // Pixels scrolled before triggering

        // Add scroll listener
        function handleScroll() {
            const currentScrollY = window.scrollY;

            if (currentScrollY > scrollThreshold) {
                tocElement.classList.add('scrolled');
            } else {
                tocElement.classList.remove('scrolled');
            }

            lastScrollY = currentScrollY;
        }

        // Add hover listeners for expand/collapse
        tocElement.addEventListener('mouseenter', function() {
            clearTimeout(collapseTimeout);
            tocElement.classList.add('expanded');
        });

        tocElement.addEventListener('mouseleave', function() {
            // Delay before collapsing
            collapseTimeout = setTimeout(function() {
                tocElement.classList.remove('expanded');
            }, 300); // 300ms delay
        });

        // Initial scroll check
        handleScroll();

        // Listen to scroll events (throttled)
        let scrollTimeout;
        window.addEventListener('scroll', function() {
            if (scrollTimeout) {
                window.cancelAnimationFrame(scrollTimeout);
            }

            scrollTimeout = window.requestAnimationFrame(function() {
                handleScroll();
            });
        }, { passive: true });
    }
};
