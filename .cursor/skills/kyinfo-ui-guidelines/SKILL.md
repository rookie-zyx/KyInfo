---
name: kyinfo-ui-guidelines
description: Applies B-end UI/UX design standards for the KyInfo 考研信息系统 Blazor frontend—color palette, spacing, buttons, forms, empty states, and tables. Use when optimizing KyInfo UI, reviewing or refactoring Blazor pages, fixing layout/visual issues, or when the user mentions UI规范、界面优化、B端设计、或 attaches this skill.
---

# 通用 UI 规范提示词

你是一位专注于 B 端产品的 UI/UX 设计师，请按照以下规范优化 KyInfo 考研信息系统的界面：

## 设计原则
- 颜色限制在 3-5 种：1 个主色（蓝色 #3B82F6）、2-3 个中性色、1 个强调色
- 字体最多 2 种，正文行高 1.5-1.6
- 优先使用 Flexbox 布局，避免绝对定位

## 全局修复清单

### 1. 移除视觉冗余
- 删除内容区重复的 "KyInfo" badge，仅保留侧边栏 logo
- 功能标签（如"按年份查看..."）改为副标题样式（text-sm text-muted-foreground）

### 2. 统一间距系统
- 卡片内边距：p-4 或 p-6
- 表单字段间距：gap-4
- 内容区左侧边距：pl-6

### 3. 按钮规范
- 详见 [kyinfo-button-guidelines](../kyinfo-button-guidelines/SKILL.md)（类型、尺寸、禁止事项）
- 禁止按钮被拉伸变形

### 4. 表单优化
- 标签使用用户语言，避免技术术语（"院校名称" 而非 "院校 Id"）
- 所有 placeholder 显示完整提示文案
- 表单按钮与输入框底部对齐（items-end）

### 5. 空状态设计
- 包含插图 + 简短说明 + 明确的操作引导
- 示例："点击左侧院校查看详情"

### 6. 表格增强
- 行添加 hover:bg-muted 状态
- 标签使用颜色区分（国家线=蓝色，院校专业线=绿色）
- 空值显示 "—" 而非 "-"

## KyInfo 实现提示

本项目前端为 Blazor + Bootstrap + 自定义 CSS（`KyInfo.Blazor/wwwroot/css/`、各组件 `.razor.css`）。应用上述规范时：

- Tailwind 类名（如 `p-4`、`h-10`）映射为等效 Bootstrap / 自定义 CSS
- 主色 `#3B82F6` 写入或复用现有 CSS 变量 / 主题色
- 修改范围优先：`Components/Pages/`、`Components/Layout/`、对应样式文件
- 改完后在浏览器中目视检查列表、表单、空状态与表格 hover 效果
