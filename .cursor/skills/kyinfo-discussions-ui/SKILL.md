---
name: kyinfo-discussions-ui
description: Applies discussion board UI optimization standards for KyInfo /discussions—filter card layout, search row, unified tabs, FAB/dialog posting, unified post cards, and meta row hierarchy. Use when optimizing the discussions page, refactoring Discussions.razor, or when the user attaches this skill or mentions 讨论区优化.
---

# 讨论区优化提示词

修复或迭代讨论区前先阅读 [kyinfo-ui-guidelines](../kyinfo-ui-guidelines/SKILL.md) 全局规范。主要文件：`KyInfo.Blazor/Components/Pages/Discussions.razor`、`wwwroot/css/discussions.css`、`Components/Discussions/`。

## 讨论区页面优化规范

### 布局结构
- 页面宽度：max-w-4xl 居中
- 内边距：px-4 py-8
- 模块间距：mb-6 或 mb-8

### 筛选区域
- 将所有筛选项（分类/排序/筛选/搜索）整合到一个 Card 内
- 第一行：搜索框（带内嵌图标）+ 刷新按钮
- 第二行：分类 Tabs | 分隔线 | 排序 Tabs | 分隔线 | 筛选 Tabs
- 分隔线使用：<div className="h-6 w-px bg-border" />
- Tabs 高度统一：h-9，字号 text-sm

### 发帖功能
- 桌面端：在页面标题右侧放置「发布新讨论」按钮
- 移动端：使用固定定位的 FAB 悬浮按钮（fixed bottom-6 right-6）
- 发帖表单使用 Dialog 弹窗，包含：
  - 标题输入框（Input）
  - 正文多行输入（Textarea min-h-[200px]）
  - 表情选择（Popover + emoji grid）
  - 字数统计（右下角显示 x / 4000）
  - 底部按钮：取消 + 发布

### 帖子卡片
- 统一使用 Card 组件，内边距 p-4
- 左侧：Avatar 头像（h-10 w-10）
- 中间：标题 + 内容预览（line-clamp-2）+ 元信息
- 右侧：ChevronRight 箭头（hover 时显示）
- 置顶帖使用橙色 Badge：bg-orange-100 text-orange-700
- hover 效果：hover:bg-muted/50

### 元信息行
- 使用 flex items-center gap-4
- 字号 text-xs，颜色 text-muted-foreground
- 显示：作者 | 时间 | 评论数
- 每项配合小图标（h-3 w-3）

### 禁止事项
- 不使用居中文字布局的帖子卡片
- 不使用渐变装饰或大面积彩色背景
- 不将发帖表单直接放在列表区域内
- 不使用多种不同风格的卡片混排

## KyInfo 实现对照

| 规范项 | Blazor / CSS 映射 |
|--------|-------------------|
| max-w-4xl | `.discussions-page { max-width: 56rem; margin: 0 auto; }` |
| px-4 py-8 | `padding: 2rem 1rem` |
| 筛选 Card | `.discussion-filter-bar` + `card-soft` |
| 搜索 + 刷新同行 | `.discussion-search-row`；搜索 `ky-btn-outline`，刷新 `ky-btn-ghost-icon` |
| h-9 Tabs | `.discussion-filter-tab-group` + `.discussion-filter-tab`（讨论区专用，不用 ky-btn-tab） |
| 分隔线 | `.discussion-filter-divider { height: 1.5rem; width: 1px; }` |
| 桌面发帖按钮 | header 右侧 `btn-ky`，`d-none d-md-inline-flex` |
| 移动 FAB | `.discussion-fab { position: fixed; bottom: 1.5rem; right: 1.5rem; }` + `d-md-none` |
| Dialog | `.discussion-composer-drawer` + backdrop |
| 表情 | 现有 `StickerPicker.razor` |
| Avatar 40px | `.discussion-post-avatar { width/height: 2.5rem; }` |
| line-clamp-2 | `.discussion-post-preview { -webkit-line-clamp: 2; }` |
| 置顶 Badge | `.pill-pin { background: #ffedd5; color: #c2410c; }` |

验证：桌面与 375px 宽度下检查筛选 Card、FAB/标题按钮切换、卡片 hover 与 Dialog 发帖流程。
