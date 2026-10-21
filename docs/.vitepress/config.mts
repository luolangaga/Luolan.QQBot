import { defineConfig } from 'vitepress'

export default defineConfig({
  lang: 'zh-CN',
  title: 'Luolan.QQBot',
  description: '简洁高效的 QQ 官方机器人 .NET SDK',
  appearance: true,
  base: '/',
  cleanUrls: false,
  scrollOffset: 134,

  vite: {
    ssr: {
      noExternal: ['@iconify/vue'],
    },
  },

  themeConfig: {
    nav: [
      { text: '首页', link: '/' },
      { text: '指南', link: '/guide/getting-started' },
      { text: 'API 参考', link: '/api/overview' },
      { text: 'GitHub', link: 'https://github.com/luolangaga/Luolan.QQBot' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: '入门指南',
          collapsed: false,
          items: [
            { text: '快速开始', link: '/guide/getting-started' },
            { text: 'C# 基础知识', link: '/guide/csharp-basics' },
            { text: '项目配置', link: '/guide/configuration' },
          ],
        },
        {
          text: '核心功能',
          collapsed: false,
          items: [
            { text: '控制器模式', link: '/guide/controller-mode' },
            { text: '事件系统', link: '/guide/events' },
            { text: '消息发送', link: '/guide/messages' },
            { text: 'Markdown 与键盘', link: '/guide/markdown-keyboard' },
            { text: '群管理与群消息', link: '/guide/group-management' },
          ],
        },
        {
          text: '进阶主题',
          collapsed: false,
          items: [
            { text: 'Webhook 接入', link: '/guide/webhook' },
            { text: 'ASP.NET Core 集成', link: '/guide/aspnetcore' },
            { text: '依赖注入', link: '/guide/dependency-injection' },
            { text: '最佳实践', link: '/guide/best-practices' },
            { text: '性能优化', link: '/guide/performance' },
            { text: '从 v1.4 迁移', link: '/guide/migration' },
          ],
        },
      ],
      '/api/': [
        {
          text: 'API 参考',
          items: [
            { text: '总览', link: '/api/overview' },
            { text: 'QQBotClient', link: '/api/qqbot-client' },
            { text: 'QQBotClientBuilder', link: '/api/builder' },
            { text: 'QQBotController', link: '/api/controller' },
            { text: 'QQBotHttpClient', link: '/api/http-client' },
            { text: 'Webhook', link: '/api/webhook' },
            { text: '事件类型', link: '/api/event-types' },
            { text: '模型类型', link: '/api/models' },
            { text: '辅助工具', link: '/api/helpers' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/luolangaga/Luolan.QQBot' },
    ],

    search: {
      provider: 'local',
      options: {
        translations: {
          button: { buttonText: '搜索文档' },
          modal: {
            displayDetails: '显示详情',
            noResultsText: '未找到结果',
          },
        },
      },
    },

    editLink: {
      pattern: 'https://github.com/luolangaga/Luolan.QQBot/edit/main/docs/:path',
      text: '在 GitHub 上编辑此页',
    },

    lastUpdated: {
      text: '最后更新',
      formatOptions: { dateStyle: 'short' },
    },

    footer: {
      message: '基于 MIT 协议发布',
      copyright: 'Copyright © 2026 Luolan',
    },

    docFooter: {
      prev: '上一页',
      next: '下一页',
    },

    outline: {
      label: '本页目录',
      level: [2, 3],
    },
  },
})
