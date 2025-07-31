# 🚀 工作流项目快速开始指南

## 📋 前置要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) 或 [VS Code](https://code.visualstudio.com/)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

## ⚡ 快速开始

### 1. 克隆项目

```bash
git clone <repository-url>
cd workflow
```

### 2. 安装开发工具（可选）

```powershell
# Windows PowerShell
.\install-dev-tools.ps1
```

### 3. 配置数据库

```bash
# 更新连接字符串
# 编辑 src/Hx.Workflow.Api/appsettings.Development.json
```

### 4. 运行数据库迁移

```bash
# 安装 EF Core 工具
dotnet tool install -g dotnet-ef

# 运行迁移
dotnet ef database update --project src/Hx.Workflow.EntityFrameworkCore
```

### 5. 启动应用

```bash
# 开发模式
dotnet run --project src/Hx.Workflow.Api

# 或使用 Docker
docker-compose up -d
```

## 🐳 Docker 快速开始

### 使用 Docker Compose

```bash
# 启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f workflow-api

# 停止服务
docker-compose down
```

### 访问地址

- **API**: http://localhost:5000
- **pgAdmin**: http://localhost:5050 (admin@workflow.com / admin123)
- **PostgreSQL**: localhost:5432

## 🔧 开发工具

### Visual Studio 2022 推荐插件

1. **C# Dev Kit** - 官方 C# 开发工具包
2. **IntelliCode** - AI 辅助代码补全
3. **CodeMaid** - 代码清理和格式化
4. **SonarLint** - 代码质量检查
5. **GitHub Copilot** - AI 代码助手

### VS Code 推荐扩展

- **C# Dev Kit** (`ms-dotnettools.csdevkit`)
- **C#** (`ms-dotnettools.csharp`)
- **GitHub Copilot** (`GitHub.copilot`)
- **GitLens** (`eamodio.gitlens`)
- **Thunder Client** (`rangav.vscode-thunder-client`)

## 📊 项目结构

```
workflow/
├── src/
│   ├── Hx.Workflow.Api/              # API 层
│   ├── Hx.Workflow.Application/       # 应用服务层
│   ├── Hx.Workflow.Application.Contracts/  # 应用契约层
│   ├── Hx.Workflow.Domain/            # 领域层
│   ├── Hx.Workflow.Domain.Shared/     # 共享领域层
│   ├── Hx.Workflow.EntityFrameworkCore/  # 数据访问层
│   ├── Hx.Workflow.EntityFrameworkCore.DbMigrations/  # 数据库迁移
│   └── Hx.Workflow.HttpApi/           # HTTP API 层
├── .vscode/                           # VS Code 配置
├── src/Hx.Workflow.EntityFrameworkCore.DbMigrations/Migrations/Migration_AddVersion/  # 版本控制迁移脚本
├── Dockerfile                         # Docker 配置
├── docker-compose.yml                 # Docker Compose 配置
└── DEVELOPMENT_SETUP.md               # 详细开发环境配置
```

## 🎯 常用命令

### 开发命令

```bash
# 构建项目
dotnet build

# 运行测试
dotnet test

# 启动应用
dotnet run --project src/Hx.Workflow.Api

# 热重载开发
dotnet watch run --project src/Hx.Workflow.Api
```

### 数据库命令

```bash
# 创建迁移
dotnet ef migrations add MigrationName --project src/Hx.Workflow.EntityFrameworkCore

# 更新数据库
dotnet ef database update --project src/Hx.Workflow.EntityFrameworkCore

# 生成 SQL 脚本
dotnet ef migrations script --project src/Hx.Workflow.EntityFrameworkCore
```

### Docker 命令

```bash
# 构建镜像
docker build -t workflow-api .

# 运行容器
docker run -p 5000:80 workflow-api

# 使用 Docker Compose
docker-compose up -d
```

## 🔍 调试技巧

### 1. 断点调试

- 在 Visual Studio 中按 F5 启动调试
- 在 VS Code 中按 F5 启动调试
- 使用 `System.Diagnostics.Debugger.Break()` 强制断点

### 2. 日志调试

```csharp
// 在代码中添加日志
_logger.LogInformation("调试信息: {Value}", value);
_logger.LogError("错误信息: {Error}", exception);
```

### 3. 性能分析

```bash
# 性能分析
dotnet trace collect --name workflow-api --process-id <PID>

# 内存分析
dotnet dump collect --process-id <PID>
```

## 📚 API 文档

启动应用后访问：

- **Swagger UI**: http://localhost:5000/swagger
- **API 文档**: http://localhost:5000/api-docs

## 🆘 常见问题

### 1. 构建失败

```bash
# 清理并重新构建
dotnet clean
dotnet restore
dotnet build
```

### 2. 数据库连接失败

- 检查 PostgreSQL 服务是否启动
- 验证连接字符串配置
- 确认数据库用户权限

### 3. 端口冲突

```bash
# 查看端口占用
netstat -ano | findstr :5000

# 修改端口配置
# 编辑 Properties/launchSettings.json
```

### 4. Docker 问题

```bash
# 清理 Docker 资源
docker system prune -a

# 重新构建镜像
docker-compose build --no-cache
```

## 📞 获取帮助

- 📖 [详细开发环境配置](DEVELOPMENT_SETUP.md)
- 🐛 [问题反馈](https://github.com/your-repo/issues)
- 📧 [联系开发团队](mailto:dev@workflow.com)

---

**提示**: 首次运行可能需要较长时间来下载依赖包和构建项目，请耐心等待。
