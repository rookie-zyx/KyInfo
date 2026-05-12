# KyInfo 配置说明（密钥与连接）

## 原则

- **生产环境**：不得在仓库或镜像中存放真实 JWT 密钥、数据库密码、第三方 AI Key。请使用环境变量、Azure Key Vault、Kubernetes Secret 等。
- **本地开发**：优先使用 [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)；可选使用已加入 `.gitignore` 的 `appsettings.Local.json`。

## JWT（`Jwt`）

| 配置项 | 说明 |
|--------|------|
| `Key` | 对称签名密钥，**至少 32 字符**。生产必须通过 `Jwt__Key` 环境变量或机密存储提供。 |
| `Issuer` / `Audience` | 与签发、校验一致即可；可用 `Jwt__Issuer`、`Jwt__Audience` 覆盖。 |
| `ExpireMinutes` | 访问令牌有效期（分钟）；见 [SECURITY-TOKENS.md](./SECURITY-TOKENS.md)。 |

### 本地设置 User Secrets（API 项目）

在 `KyInfo.Api` 目录执行：

```bash
dotnet user-secrets set "Jwt:Key" "<至少32字符的随机字符串>"
```

可选：

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<你的连接串>"
dotnet user-secrets set "Ai:ApiKey" "<你的 API Key>"
dotnet user-secrets set "Seed:Admin:Password" "<仅开发环境种子密码>"
```

## 连接字符串

- 环境变量：`ConnectionStrings__DefaultConnection`
- 基线 `appsettings.json` 中可为空；未配置时仅在 **Development** 下可使用占位逻辑（见 `Program.cs` 注释）。生产必须在启动前配置。

## 可选本地文件

复制以下内容到 **未被 Git 跟踪** 的 `appsettings.Local.json`（已在 `.gitignore` 中）：

```json
{
  "Jwt": { "Key": "本地仅开发用-请换成长随机串至少32字符" },
  "Ai": { "ApiKey": "" }
}
```

## 生产部署检查清单

- [ ] `Jwt:Key` 已配置且长度 ≥ 32，且与镜像/仓库隔离  
- [ ] `ConnectionStrings:DefaultConnection` 使用托管身份或托管密钥  
- [ ] `Ai:ApiKey`（若启用 AI）来自机密存储  
- [ ] `AllowedHosts` 限制为实际域名（非 `*`）

## API 限流

以下路由在 [`Program.cs`](../KyInfo.Api/Program.cs) 中配置了固定窗口限流，超过配额返回 **HTTP 429**（JSON `message`）：

| 策略名 | 用途 | 默认窗口 |
|--------|------|----------|
| `auth_login` | `POST /api/Auth/login` | 每分钟每 IP 15 次 |
| `auth_register` | `POST /api/Auth/register` | 每分钟每 IP 10 次 |
| `ai_chat` | `POST /api/Ai/chat` | 每分钟每 IP 30 次 |
| `admin_upload` | `POST /api/admin/exam-scores/import` | 每分钟每 IP 20 次 |

生产环境可按流量与风控要求调整 `PermitLimit` / `Window`。

## 集成测试与 `Jwt:Key`

`tests/KyInfo.Tests` 使用 `WebApplicationFactory`，在 [`KyInfoApiFactory`](../tests/KyInfo.Tests/KyInfoApiFactory.cs) 中通过 `IWebHostBuilder.UseSetting("Jwt:Key", ...)` 注入测试密钥，避免与生产/开发配置混用；**请勿**在测试工厂中留空 `Jwt:Key`，否则与 `Program.cs` 中 JWT 注册时序不一致会导致签名校验失败。
