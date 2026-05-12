---
name: kyinfo-responsive-guidelines
description: Applies mobile and responsive UI fix checklists for the KyInfo Blazor frontend—drawer sidebar, stacked forms, card list tables, and vertical button groups on small screens. Use when adapting KyInfo for mobile, fixing responsive layout issues, or when the user mentions 移动端、响应式、sm/md 断点.
---

# 移动端适配提示词

修复前先阅读 [kyinfo-ui-guidelines](../kyinfo-ui-guidelines/SKILL.md) 中的全局 UI 规范。完成后在浏览器 DevTools 中分别用 375px（sm 以下）与 768px（md）宽度验证。

## 响应式修复清单
- [ ] 侧边栏改为 drawer 抽屉式（md 以下）
- [ ] 表单筛选条件改为 flex-col（sm 以下）
- [ ] 数据表格改为卡片列表视图（sm 以下）
- [ ] 按钮组改为垂直堆叠（sm 以下）

## KyInfo 实现提示

断点对照（Tailwind → Bootstrap）：`sm` ≈ `576px`（`.col-sm-*`）、`md` ≈ `768px`（`.col-md-*`）。

| 清单项 | 主要文件 | 做法 |
|--------|----------|------|
| drawer 侧边栏 | `MainLayout.razor`、`NavMenu.razor`、`.razor.css` | md 以下隐藏固定侧栏，汉堡按钮打开 overlay drawer；`position: fixed` + 遮罩层 |
| 表单 flex-col | 各 `Pages/*.razor` 筛选区 | 外层 `d-flex flex-column flex-sm-row gap-3 align-items-sm-end` |
| 表格 → 卡片 | `ScoreLines.razor`、`Admin.razor` 等 | `d-none d-sm-table` 保留表格；`d-sm-none` 渲染卡片列表，字段纵向排列 |
| 按钮垂直堆叠 | 含按钮组的表单/工具栏 | `d-flex flex-column flex-sm-row gap-2`；按钮 `w-100 w-sm-auto` |

共享响应式样式可放入 `KyInfo.Blazor/wwwroot/css/` 或 `MainLayout.razor.css`。
