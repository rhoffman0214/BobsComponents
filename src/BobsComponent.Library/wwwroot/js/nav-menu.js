// NavMenu scroll collapse behavior
window.NavMenu = {
    init: function (navElement, dotNetHelper) {
        if (!navElement) return;

        let scrollThreshold = 100;
        let isCollapsed = false;
        let isHovered = false;

        // Scroll handler
        const handleScroll = () => {
            const scrolled = window.scrollY > scrollThreshold;

            if (scrolled && !isCollapsed) {
                navElement.classList.add('nav-collapsed');
                isCollapsed = true;
            } else if (!scrolled && isCollapsed && !isHovered) {
                navElement.classList.remove('nav-collapsed');
                navElement.classList.remove('nav-expanded');
                isCollapsed = false;
            }
        };

        // Hover handlers for collapsed state
        navElement.addEventListener('mouseenter', () => {
            if (isCollapsed) {
                navElement.classList.add('nav-expanded');
                isHovered = true;
            }
        });

        navElement.addEventListener('mouseleave', () => {
            if (isCollapsed) {
                navElement.classList.remove('nav-expanded');
                isHovered = false;
            }
        });

        // Click handler to toggle when collapsed
        navElement.addEventListener('click', (e) => {
            if (isCollapsed && !navElement.classList.contains('nav-expanded')) {
                navElement.classList.add('nav-expanded');
                isHovered = true;
                e.stopPropagation();
            }
        });

        // Close expanded menu when clicking outside
        document.addEventListener('click', (e) => {
            if (isCollapsed && isHovered && !navElement.contains(e.target)) {
                navElement.classList.remove('nav-expanded');
                isHovered = false;
            }
        });

        // Attach scroll listener
        window.addEventListener('scroll', handleScroll, { passive: true });

        // Initial check
        handleScroll();

        // Cleanup function
        return () => {
            window.removeEventListener('scroll', handleScroll);
        };
    }
};
