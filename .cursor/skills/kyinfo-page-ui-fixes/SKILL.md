---
name: kyinfo-page-ui-fixes
description: Applies page-specific UI fix checklists for KyInfo Blazor routes (scoreline-trends, admin, recommendations, schools, scorelines, account, ai-chat, discussions). Use when fixing a specific KyInfo page, doing逐页 UI 修复, or when the user attaches this skill alongside kyinfo-ui-guidelines.
---

# 逐页修复提示词

修复单页前先阅读 [kyinfo-ui-guidelines](../kyinfo-ui-guidelines/SKILL.md) 中的全局规范。每完成一页，在浏览器中验证该路由后再继续下一页。

## 分数线趋势页 (/scoreline-trends)
- [ ] 表单标签："院校 Id" → "院校名称"，"专业 Id" → "专业名称"
- [ ] 表单布局改为 flex items-end gap-4
- [ ] 添加空状态插图和引导文案

## 管理后台页 (/admin)
- [ ] "新建用户"和"批量导入"改为 Tabs 切换
- [ ] 文件上传改为 dropzone 拖拽区域样式
- [ ] "刷新"按钮加图标，移至表头左侧

## 智能志愿推荐页 (/recommendations)
- [ ] 修复 placeholder 截断："输入已录入成:" → "请输入用户ID"
- [ ] 表单使用 grid-cols-4 均分布局
- [ ] 添加结果骨架屏或空状态

## 院校查询页 (/schools)
- [ ] 列表区域宽度改为 w-1/2 或 min-w-[400px]
- [ ] "985/211" 标签改为填充样式（bg-blue-100 text-blue-700）
- [ ] 详情空状态简化 + 添加插图

## 分数线查询页 (/scorelines)
- [ ] 修复"查询"按钮异常高度 → h-10
- [ ] 类型标签颜色区分
- [ ] 表格行添加 hover 效果

## 我的档案页 (/account)
- [ ] 角色标签移至卡片顶部右侧
- [ ] "保存资料"按钮移至表单底部右侧
- [ ] 密码字段添加可见性切换图标

## AI 助手页 (/ai-chat)
- [ ] Hero 区域改为浅色背景 + subtle shadow
- [ ] 快捷问题卡片添加 hover:border-primary
- [ ] 键盘提示移至输入框底部

## 讨论区页 (/discussions)
- [ ] 合并排序 Tabs 为单组
- [ ] "学生/教师讨论区"改为普通 SegmentedControl
- [ ] 发帖区域改为 FAB 或顶部展开面板

## 页面与源文件对照

| 路由 | 组件文件 |
|------|----------|
| `/scoreline-trends` | `KyInfo.Blazor/Components/Pages/ScoreLineTrends.razor` |
| `/admin` | `KyInfo.Blazor/Components/Pages/Admin.razor` |
| `/recommendations` | `KyInfo.Blazor/Components/Pages/Recommendations.razor` |
| `/schools` | `KyInfo.Blazor/Components/Pages/Schools.razor` |
| `/scorelines` | `KyInfo.Blazor/Components/Pages/ScoreLines.razor` |
| `/account` | `KyInfo.Blazor/Components/Pages/Account.razor` |
| `/ai-chat` | `KyInfo.Blazor/Components/Pages/AiChat.razor` |
| `/discussions` | `KyInfo.Blazor/Components/Pages/Discussions.razor` |

样式优先改对应 `.razor.css` 或 `KyInfo.Blazor/wwwroot/css/` 中的共享类；Tailwind 类名映射为 Bootstrap / 自定义 CSS。
