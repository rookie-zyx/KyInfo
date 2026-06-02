# KyInfo

KyInfo 是一套面向**考研志愿**场景的 Web 应用：后端提供基于 JWT 的 REST API，前端为 Blazor Server，覆盖院校与专业、招生信息、分数线、考试成绩、志愿推荐、讨论区（含表情贴纸）、可选 AI 对话及管理端能力（如成绩导入）等。

## 技术栈

| 类别 | 选型 |
|------|------|
| 运行时 | .NET 8（C#） |
| 后端 | ASP.NET Core Web API（Development 下启用 Swagger） |
| 前端 | Blazor Server（交互式服务端渲染） |
| 数据库 | **PostgreSQL 16** + EF Core 8（Npgsql） |
| 认证与安全 | JWT、FluentValidation、固定窗口限流 |
| 容器 | Docker / Docker Compose |
| 可选 | OpenAI 兼容 HTTP API（`Ai` 配置节，如阿里云百炼） |

## 仓库结构

| 路径 | 说明 |
|------|------|
| [KyInfo.Api](KyInfo.Api) | HTTP API 宿主：控制器、中间件、OpenAPI、开发种子数据 |
| [KyInfo.Blazor](KyInfo.Blazor) | Blazor Server 站点；通过 `ApiBaseUrl` 调用后端，登录走 BFF（`/bff/auth/*`） |
| [src/KyInfo.Domain](src/KyInfo.Domain) | 领域实体与规则 |
| [src/KyInfo.Application](src/KyInfo.Application) | 应用服务与用例编排 |
| [src/KyInfo.Infrastructure](src/KyInfo.Infrastructure) | EF Core `AppDbContext`、仓储实现；迁移位于 `Migrations/` |
| [src/KyInfo.Contracts](src/KyInfo.Contracts) | 跨层契约 / DTO |
| [tests/KyInfo.Tests](tests/KyInfo.Tests) | 集成测试 |
| [docker-compose.yml](docker-compose.yml) | 本地/生产 Compose：PostgreSQL + API + Blazor |
| [scripts/](scripts/) | 本地启动、清理等辅助脚本 |

解决方案入口：[KyInfo.sln](KyInfo.sln)

## 前置要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)（用于 PostgreSQL；可选全栈容器部署）
- （可选）[Visual Studio 2022](https://visualstudio.microsoft.com/) 17.8+，工作负载：**ASP.NET 和 Web 开发**
- （可选）EF Core CLI：`dotnet tool install --global dotnet-ef`

> **说明**：项目已迁移至 **PostgreSQL**，不再使用 SQL Server / LocalDB。若本机仍有旧库 `(localdb)\MSSQLLocalDB` 中的 `KyInfoDb`，可自行删除，与当前运行无关。

## 快速开始（推荐：Visual Studio + Docker 数据库）

### 1. 启动 PostgreSQL

在**仓库根目录**执行（不要在用户主目录执行）：

```powershell
cd d:\c#\KyInfo
docker compose up -d postgres
```

Windows 若提示找不到 `docker`，可先执行：

```powershell
$env:PATH = "$env:LOCALAPPDATA\Programs\DockerDesktop\resources\bin;$env:PATH"
```

### 2. 用 Visual Studio 运行

1. 打开 [KyInfo.sln](KyInfo.sln)
2. 右键解决方案 → **属性** → **多个启动项目**
3. **KyInfo.Api**、**KyInfo.Blazor** 均设为 **开始**
4. 两个项目的启动配置均选 **`http`**（不要选 `https`，避免浏览器打开 `7011` 且与 `ApiBaseUrl` 不一致）
5. 按 **F5**

| 服务 | 地址 |
|------|------|
| 前端 | http://localhost:5179 |
| API / Swagger | http://localhost:5028/swagger |

Development 下 API 启动时会自动 **迁移数据库** 并写入种子管理员（见下表）。

### 3. 开发默认账号（仅 Development）

| 用户名 | 密码 | 角色 |
|--------|------|------|
| `admin` | `Admin123!` | Root |

可在 [KyInfo.Api/appsettings.Development.json](KyInfo.Api/appsettings.Development.json) 的 `Seed:Admin` 节修改。**切勿在生产环境使用默认密码。**

## 快速开始（命令行）

```powershell
cd d:\c#\KyInfo
docker compose up -d postgres

# 终端 1：API（http，与 Blazor Development 配置一致）
dotnet run --project KyInfo.Api --urls http://127.0.0.1:5028

# 终端 2：Blazor
dotnet run --project KyInfo.Blazor
```

前端 `ApiBaseUrl` 见 [KyInfo.Blazor/appsettings.Development.json](KyInfo.Blazor/appsettings.Development.json)（默认 `http://127.0.0.1:5028`）。修改 API 端口后请同步修改该值。

也可运行辅助脚本 [scripts/start-local-dev.ps1](scripts/start-local-dev.ps1) 启动数据库并编译解决方案。

## Docker 全栈部署

在仓库根目录构建并启动 API + Blazor + PostgreSQL：

```powershell
docker compose up -d --build
```

| 服务 | 地址 |
|------|------|
| Blazor | http://localhost:5001 |
| API | http://localhost:5000 |
| PostgreSQL | localhost:5432 |

生产部署、域名与反向代理详见 [docs/服务器部署指南.md](docs/服务器部署指南.md)。

## 配置与密钥

- **不要**将真实 JWT 密钥、数据库密码、AI Key 提交到 Git。
- 本地推荐 [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) 或未跟踪的 `appsettings.Local.json`（API 会可选加载）。
- Development 下若未配置 `Jwt:Key`，API 使用内置**仅开发用**占位密钥；生产未配置将启动失败（预期行为）。

详细说明：[docs/CONFIGURATION.md](docs/CONFIGURATION.md)

### 本地 PostgreSQL 连接串（Development 默认）

```text
Host=127.0.0.1;Port=5432;Database=KyInfoDb;Username=postgres;Password=KyInfo@2026;SSL Mode=Disable
```

与 [docker-compose.yml](docker-compose.yml) 中 `postgres` 服务一致。请使用 `127.0.0.1` 而非 `localhost`，避免 Windows 上 IPv6 连错端口。

## 数据库迁移

Development 下 API 启动时自动执行 `Database.Migrate()`。手动迁移：

```powershell
dotnet ef database update `
  --project src\KyInfo.Infrastructure\KyInfo.Infrastructure.csproj `
  --startup-project KyInfo.Api\KyInfo.Api.csproj `
  --context AppDbContext
```

### 清空本地数据库

```powershell
docker compose down -v          # 删除 PostgreSQL 数据卷
docker compose up -d postgres   # 重建空库，下次启动 API 会重新迁移并种子 admin
```

## 运行测试

```powershell
dotnet test tests\KyInfo.Tests\KyInfo.Tests.csproj
```

## 常见问题

| 现象 | 处理 |
|------|------|
| 编译提示 DLL 被 **KyInfo.Api** 锁定 | VS 中 **Shift+F5** 停止调试后再生成 |
| 注册/登录失败、无法连接 API | 确认 API 已启动；Blazor 与 API 均使用 **`http`** 配置；PostgreSQL 容器已 `healthy` |
| `docker compose` 找不到配置文件 | 必须在含 `docker-compose.yml` 的仓库根目录执行 |
| 浏览器打开 **7011** | VS 启动配置改为 **`http`**，或重启 VS 以应用 csproj 中的默认 `LaunchProfile` |

## 相关文档

- [配置与密钥](docs/CONFIGURATION.md)
- [令牌与安全](docs/SECURITY-TOKENS.md)
- [数据合规与运维基线](docs/COMPLIANCE.md)
- [服务器部署指南](docs/服务器部署指南.md)

## 生产部署检查清单（摘要）

- [ ] `Jwt__Key` 长度 ≥ 32，通过环境变量或机密管理注入
- [ ] `ConnectionStrings__DefaultConnection` 指向生产 PostgreSQL
- [ ] `Cors__Origins__*` 包含实际前端域名
- [ ] Blazor 的 `ApiBaseUrl` 指向 API 对内/对外地址
- [ ] `AllowedHosts` 限制为实际域名
- [ ] 按需配置 `Ai__ApiKey` 与限流参数
