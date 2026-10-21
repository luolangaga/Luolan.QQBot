import DefaultTheme from 'vitepress/theme'
import { Icon } from '@iconify/vue'
import type { Theme } from 'vitepress'

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    app.component('Icon', Icon)
  },
} satisfies Theme
