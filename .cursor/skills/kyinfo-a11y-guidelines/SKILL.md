---
name: kyinfo-a11y-guidelines
description: Applies accessibility (A11y) fix checklists for the KyInfo Blazor frontend—form labels, focus rings, color contrast, aria-current navigation, and skip links. Use when improving KyInfo accessibility, fixing A11y issues, auditing UI for WCAG compliance, or when the user mentions 可访问性、a11y、无障碍.
---

# 可访问性提示词

修复前先阅读 [kyinfo-ui-guidelines](../kyinfo-ui-guidelines/SKILL.md) 中的全局 UI 规范。A11y 改动完成后，用键盘 Tab 遍历页面并检查焦点顺序与可见焦点环。

## A11y 修复清单
- [ ] 所有表单输入框添加 <label htmlFor="..."> 关联
- [ ] 按钮添加 focus-visible:ring-2 focus-visible:ring-ring
- [ ] 检查颜色对比度 ≥ 4.5:1（特别是 placeholder 文字）
- [ ] 侧边栏当前页添加 aria-current="page"
- [ ] 添加 skip-to-main-content 链接

## KyInfo 实现提示

| 清单项 | Blazor / Bootstrap 做法 |
|--------|-------------------------|
| label 关联 | 使用 `<label for="field-id">` + 输入框 `id="field-id"`；或 `<InputText @bind-Value="..." id="..." aria-label="..." />` |
| focus 环 | 在 `.razor.css` 或共享 CSS 中为 `:focus-visible` 添加 `outline` / `box-shadow`（等效 ring-2） |
| 对比度 | 主色 `#3B82F6` 与白底/灰底组合需达标；placeholder 避免 `#9CA3AF` 过浅 |
| aria-current | 在 `NavMenu.razor` 当前路由链接上加 `aria-current="page"` |
| skip 链接 | 在 `MainLayout.razor` 主内容前插入 `<a href="#main-content" class="skip-link">跳到主内容</a>`，`<main id="main-content">` |

主要修改文件：`Components/Layout/MainLayout.razor`、`Components/Layout/NavMenu.razor`、`Components/Pages/*.razor` 及对应样式。
