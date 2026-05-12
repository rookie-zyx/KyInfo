---
name: kyinfo-button-guidelines
description: Applies four-tier button hierarchy for KyInfo Blazor—Primary (publish/submit), Outline (cancel/refresh/download), Tabs (filter/sort), Ghost icon (toolbar)—with h-10/h-9 sizing. Use when adding or refactoring buttons, fixing button layout, auditing page actions, or when the user mentions 按钮规范、按钮层级、Primary、Outline、Ghost、Tabs.
---

# 按钮规范

修复或新增按钮前，先阅读 [kyinfo-ui-guidelines](../kyinfo-ui-guidelines/SKILL.md) 全局规范。

## 按钮层级体系

```
主操作按钮（Primary）
├─ 发布讨论、发布评论
├─ 样式：bg-primary text-primary-foreground h-10 px-4
└─ 场景：每页最多 1-2 个

次级按钮（Secondary/Outline）
├─ 取消、刷新、下载
├─ 样式：border border-input bg-background h-10 px-4
└─ 场景：与主按钮配对

Tabs 切换（用于筛选）
├─ 分类切换、排序切换、筛选切换
├─ 样式：使用 shadcn Tabs 组件，统一高度 h-9
└─ 场景：多选一的状态切换

图标按钮（Ghost）
├─ 刷新、更多操作
├─ 样式：variant="ghost" size="icon" h-9 w-9
└─ 场景：工具栏操作
```

## 禁止事项

- 不混用填充按钮和边框按钮做同级切换
- 不使用多组视觉相似的按钮组并排
- 不将搜索按钮与输入框分离
- 不使用超大尺寸的表情/图标按钮
- 搜索、下载、取消不用 Primary 样式

## 按钮审计清单

- [ ] 每页 Primary 不超过 1-2 个（仅发布/提交/创建类）
- [ ] 取消、下载、带文字刷新 → Outline h-10
- [ ] 工具栏纯图标刷新/更多 → Ghost h-9 w-9
- [ ] 分类/排序/筛选 → Tabs h-9，不用 Primary + Outline 混搭
- [ ] 搜索按钮与输入框同一行，搜索用 Outline 非 Primary
- [ ] 表情/贴纸按钮尺寸与正文图标一致

## KyInfo 实现映射

| 层级 | Tailwind / shadcn | Blazor / CSS 类名 |
|------|-------------------|-------------------|
| Primary | `bg-primary h-10 px-4` | `btn btn-primary btn-ky` 或 `ky-btn ky-btn-primary` |
| Outline | `border border-input bg-background h-10 px-4` | `btn btn-outline-secondary btn-ky` 或 `ky-btn ky-btn-outline` |
| Tabs | shadcn Tabs, `h-9` | 讨论区 `.discussion-filter-tab`（专用样式，勿加 ky-btn）；Admin `admin-tab` / `ky-btn-tab` |
| Ghost icon | `variant="ghost" size="icon" h-9 w-9` | `ky-btn ky-btn-ghost-icon`；仅图标时加 `aria-label` |

**尺寸 token**：h-10 = `2.5rem`（`.btn-ky`）；h-9 = `2.25rem`（Tabs、Ghost icon）。

**主要样式文件**：`wwwroot/css/ky-ui.css`（全局层级）、`wwwroot/css/discussions.css`（讨论区）。

**典型页面**：

| 场景 | Primary | Outline | Tabs | Ghost |
|------|---------|---------|------|-------|
| 讨论列表 | 发布新讨论 | 搜索（桌面） | 分区/排序/范围 | 刷新 |
| 发帖 Dialog | 发布 | 取消 | — | — |
| 讨论详情 | 发布评论 | 取消 | — | — |
| Admin | 创建用户 | 下载模板、取消编辑 | 新建/导入 | 卡片头刷新（图标） |
| 筛选表单页 | 查询/加载 | — | — | — |

改完后在浏览器检查 Primary 数量、Tabs 一致性、搜索行耦合、Ghost 尺寸。
