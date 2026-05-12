# 认证令牌策略

## 当前阶段（访问令牌 JWT）

- API 使用 **仅 Access Token（JWT）** 模式：登录成功后返回 JWT，客户端（Blazor）保存在 `localStorage` 并在请求头携带。
- 默认有效期由配置 `Jwt:ExpireMinutes` 控制（已缩短相对默认值以降低泄露窗口）；到期后需重新登录。

## 风险边界

- JWT 在过期前难以服务端“吊销”（除非维护黑名单或改签名密钥全员失效）。
- 浏览器 `localStorage` 存在 XSS 窃取风险；生产应强化 CSP、输入校验与依赖漏洞治理。

## 后续路线图（Refresh Token）

1. **Refresh Token**：随机高熵、存服务端（数据库或 Redis），与设备/会话绑定；Access Token 保持短有效期（如 15～30 分钟）。
2. **存储方式**：Refresh 建议 **HttpOnly + Secure + SameSite** Cookie，或移动端安全存储；避免与 Access 同存 XSS 可达位置。
3. **登出**：删除服务端 Refresh 记录；可选 Access 黑名单（短 TTL）。
4. **改密 / 踢人**：使该用户全部 Refresh 失效；必要时轮换 `Jwt:Key`（会导致全体重新登录）。

实施上述能力时需同步调整 Blazor 登录/刷新流程与 API 契约。
