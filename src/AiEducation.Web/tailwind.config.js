/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        background: 'var(--ai-bg)',
        surface: 'var(--ai-surface)',
        border: 'var(--ai-border)',
        primary: 'var(--ai-primary)',
        secondary: 'var(--ai-secondary)',
        accent: 'var(--ai-accent)',
        ink: 'var(--ai-text)',
        muted: 'var(--ai-muted)',
      },
      boxShadow: {
        soft: 'var(--ai-shadow-soft)',
      },
      borderRadius: {
        card: 'var(--ai-radius-md)',
        panel: 'var(--ai-radius-lg)',
      },
    },
  },
  plugins: [],
}
