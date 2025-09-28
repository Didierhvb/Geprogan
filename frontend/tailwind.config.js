/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
    "../app/views/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          primary: '#4a7c59',
          'primary-strong': '#3d6b4a',
          secondary: '#8fbc8f',
          tertiary: '#d2b48c',
          accent: '#cd853f',
          warning: '#daa520',
          success: '#6b8e23',
          error: '#b22222'
        },
        surface: {
          DEFAULT: 'rgba(250, 252, 249, 0.92)',
          muted: 'rgba(240, 246, 237, 0.85)',
          dark: 'rgba(26, 31, 26, 0.92)',
          'dark-muted': 'rgba(36, 41, 36, 0.85)'
        },
        text: {
          DEFAULT: '#2d3828',
          muted: '#5a6b4f',
          inverse: '#fefffe',
          'dark-default': '#e8f0e8',
          'dark-muted': '#b8c8b8',
          'dark-inverse': '#1a1f1a'
        },
        border: {
          DEFAULT: 'rgba(119, 140, 96, 0.3)',
          strong: 'rgba(95, 118, 72, 0.55)',
          'dark-default': 'rgba(143, 188, 143, 0.25)',
          'dark-strong': 'rgba(111, 150, 111, 0.5)'
        },
        bg: {
          DEFAULT: '#f2f6f0',
          alt: '#ebf2e7',
          'dark-default': '#1a1f1a',
          'dark-alt': '#242924'
        }
      },
      fontFamily: {
        sans: ['Segoe UI', 'Inter', 'Roboto', 'Helvetica Neue', 'Arial', 'sans-serif']
      },
      borderRadius: {
        'xl': '28px',
        'lg': '22px',
        'md': '16px'
      },
      boxShadow: {
        'glass': '0 40px 80px -48px rgba(33, 66, 38, 0.45)',
        'soft': '0 18px 45px -32px rgba(31, 51, 36, 0.25)',
        'brand': '0 24px 60px -36px rgba(76, 187, 116, 0.35)'
      },
      backdropBlur: {
        'glass': '24px'
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
  darkMode: ['class', '[data-theme="dark"]']
}