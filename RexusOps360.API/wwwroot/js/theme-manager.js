// Theme Manager for EMS Tampa-FL Amptier
class ThemeManager {
    constructor() {
        this.currentTheme = localStorage.getItem('theme') || 'light';
        this.init();
    }

    init() {
        // Apply saved theme
        this.applyTheme(this.currentTheme);
        
        // Create theme toggle button
        this.createThemeToggle();
        
        // Listen for system theme changes
        this.listenForSystemTheme();
    }

    createThemeToggle() {
        const toggle = document.createElement('div');
        toggle.className = 'theme-toggle';
        toggle.innerHTML = '<i class="fas fa-moon"></i>';
        toggle.setAttribute('title', 'Toggle Dark Mode');
        toggle.addEventListener('click', () => this.toggleTheme());
        
        document.body.appendChild(toggle);
    }

    toggleTheme() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
    }

    applyTheme(theme) {
        this.currentTheme = theme;
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
        
        // Update toggle button icon
        const toggle = document.querySelector('.theme-toggle i');
        if (toggle) {
            toggle.className = theme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
        }
        
        // Dispatch custom event for other components
        document.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    listenForSystemTheme() {
        // Check if user prefers dark mode
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)');
        
        // Only apply system preference if no theme is saved
        if (!localStorage.getItem('theme')) {
            this.applyTheme(prefersDark.matches ? 'dark' : 'light');
        }
        
        // Listen for system theme changes
        prefersDark.addEventListener('change', (e) => {
            if (!localStorage.getItem('theme')) {
                this.applyTheme(e.matches ? 'dark' : 'light');
            }
        });
    }

    getCurrentTheme() {
        return this.currentTheme;
    }
}

// Initialize theme manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.themeManager = new ThemeManager();
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ThemeManager;
} 