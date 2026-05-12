# KyInfo

KyInfo 是一套面向高考志愿场景的 Web 应用：后端提供基于 JWT 的 REST API，前端为 Blazor Server，覆盖院校与专业、招生信息、分数线、考试成绩、志愿推荐、可选 AI 对话及管理端能力（如成绩导入）等。

## 技术栈

- .NET 8（C#）
- ASP.NET Core Web API（Swagger 仅在 Development 环境启用）
- Blazor Server（交互式服务端渲染）
- Entity Framework Core 8 + SQL Server（默认开发连接使用 LocalDB）
- JWT 认证、FluentValidation、固定窗口限流
- 可选：OpenAI 兼容 HTTP API（如阿里云百炼等，见 `KyInfo.Api` 中 `Ai` 配置节）

## 仓库结构

| 路径 | 说明 |
|------|------|
| [KyInfo.Api](KyInfo.Api) | HTTP API 宿主：控制器、中间件、OpenAPI、种子数据入口 |
| [KyInfo.Blazor](KyInfo.Blazor) | Blazor Server 站点，通过 `ApiBaseUrl` 调用后端 |
| [src/KyInfo.Domain](src/KyInfo.Domain) | 领域实体与领域规则 |
| [src/KyInfo.Application](src/KyInfo.Application) | 应用服务与用例编排 |
| [src/KyInfo.Infrastructure](src/KyInfo.Infrastructure) | EF Core `AppDbContext`、仓储与基础设施实现；迁移源码位于 `KyInfo.Api/Migrations`，由本工程以链接方式编译进来 |
| [src/KyInfo.Contracts](src/KyInfo.Contracts) | 跨层契约/DTO |
| [tests/KyInfo.Tests](tests/KyInfo.Tests) | 集成测试等 |

解决方案文件：[KyInfo.sln](KyInfo.sln)。

## 前置要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 或 **LocalDB**（与 [KyInfo.Api/appsettings.json](KyInfo.Api/appsettings.json) 中 `ConnectionStrings:DefaultConnection` 一致）
- 若需命令行管理数据库迁移，安装 EF Core 工具（一次性）：

```powershell
dotnet tool install --global dotnet-ef
```

## 快速开始

### 1. 还原与编译

在仓库根目录执行：

```powershell
dotnet restore
dotnet build
```

### 2. 配置（切勿将真实密钥提交到 Git）

- 生产与团队开发：**不要**在仓库中存放真实 JWT 密钥、数据库密码或第三方 AI Key。
- 本地推荐：[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) 或已加入 `.gitignore` 的 `appsettings.Local.json`（API 会可选加载该文件）。

详细说明见 [docs/CONFIGURATION.md](docs/CONFIGURATION.md)。开发环境若未配置 `Jwt:Key`，API 会使用内置仅开发用占位密钥（生产环境启动会失败，属预期行为）。

### 3. 数据库迁移

迁移由 **Infrastructure** 工程编译，连接字符串等配置来自 **API** 启动项目。在仓库根目录执行：

```powershell
dotnet ef database update --project src\KyInfo.Infrastructure\KyInfo.Infrastructure.csproj --startup-project KyInfo.Api\KyInfo.Api.csproj --context AppDbContext
```

DbContext 类型全名为 `KyInfo.Infrastructure.Persistence.AppDbContext`。查看迁移列表可使用相同 `--project` / `--startup-project` 参数执行 `dotnet ef migrations list`。

### 4. 启动 API

```powershell
dotnet run --project KyInfo.Api\KyInfo.Api.csproj
```

默认 HTTPS 开发 URL 见 [KyInfo.Api/Properties/launchSettings.json](KyInfo.Api/Properties/launchSettings.json)（例如 `https://localhost:7233`）。Development 下可打开 Swagger：`https://localhost:7233/swagger`。

### 5. 启动 Blazor 前端

另开终端：

```powershell
dotnet run --project KyInfo.Blazor\KyInfo.Blazor.csproj
```

默认 HTTPS 地址见 [KyInfo.Blazor/Properties/launchSettings.json](KyInfo.Blazor/Properties/launchSettings.json)（例如 `https://localhost:7011`）。

前端通过 [KyInfo.Blazor/appsettings.json](KyInfo.Blazor/appsettings.json) 中的 `ApiBaseUrl` 指向后端。若你修改了 API 的监听地址或端口，请同步修改该值。

## 运行测试

```powershell
dotnet test tests\KyInfo.Tests\KyInfo.Tests.csproj
```

## 相关文档

- [配置与密钥](docs/CONFIGURATION.md)
- [令牌与安全](docs/SECURITY-TOKENS.md)
- [数据合规与运维基线](docs/COMPLIANCE.md)

## 生产部署提示（摘要）

- 配置长度不少于 32 字符的 `Jwt:Key`，并通过环境变量或机密管理注入；勿写入镜像或仓库。
- 配置 `ConnectionStrings:DefaultConnection`；将 `AllowedHosts` 限制为实际域名（避免长期使用 `*`）。
- 按需配置 `Ai:ApiKey` 与限流参数；细节仍以 [docs/CONFIGURATION.md](docs/CONFIGURATION.md) 为准。
