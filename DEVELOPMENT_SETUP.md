# .NET 8 工作流项目开发环境配置指南

## 🚀 开发环境要求

### 必需软件

- **Visual Studio 2022** (推荐 17.8 或更高版本)
- **.NET 8 SDK** (8.0.100 或更高版本)
- **PostgreSQL** (15 或更高版本)
- **Git** (最新版本)

### 推荐软件

- **Visual Studio Code** (作为轻量级编辑器)
- **Postman** (API 测试)
- **pgAdmin** 或 **DBeaver** (数据库管理)

## 📦 Visual Studio 2022 插件推荐

### 核心插件

1. **C# Dev Kit** - 官方 C# 开发工具包
2. **IntelliCode** - AI 辅助代码补全
3. **CodeMaid** - 代码清理和格式化
4. **SonarLint** - 代码质量检查
5. **GitHub Copilot** - AI 代码助手

### 数据库相关

6. **Npgsql** - PostgreSQL 支持
7. **Entity Framework Core Tools** - EF Core 工具
8. **SQL Server Integration Services** - 数据库集成

### 调试和性能

9. **OzCode** - 高级调试工具
10. **dotMemory** - 内存分析
11. **dotTrace** - 性能分析

### 代码质量

12. **StyleCop.Analyzers** - 代码风格检查
13. **Microsoft.CodeAnalysis.NetAnalyzers** - .NET 分析器
14. **Roslynator** - 代码重构工具

### 工作流和项目管理

15. **GitLens** - Git 集成增强
16. **Team Explorer** - 团队协作
17. **Azure DevOps** - 云服务集成

## 🔧 Visual Studio Code 配置

### 必需扩展

```json
{
  "extensions": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-dotnettools.csdevkit",
    "ms-vscode.vscode-json",
    "ms-vscode.powershell",
    "ms-vscode.vscode-typescript-next",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-eslint",
    "ms-vscode.vscode-json",
    "ms-vscode.vscode-xml",
    "ms-vscode.vscode-yaml",
    "ms-vscode.vscode-docker",
    "ms-azuretools.vscode-docker",
    "ms-vscode.vscode-kubernetes-tools",
    "ms-vscode.vscode-azure-account",
    "ms-vscode.vscode-azure-resourcegroups",
    "ms-vscode.vscode-azure-storage",
    "ms-vscode.vscode-azure-extensionpack",
    "ms-vscode.vscode-azureappservice",
    "ms-vscode.vscode-azurefunctions",
    "ms-vscode.vscode-azure-iot-edge",
    "ms-vscode.vscode-azure-iot-toolkit",
    "ms-vscode.vscode-azure-iot-workbench",
    "ms-vscode.vscode-azure-iot-device-cube",
    "ms-vscode.vscode-azure-iot-device-simulator",
    "ms-vscode.vscode-azure-iot-hub",
    "ms-vscode.vscode-azure-iot-central",
    "ms-vscode.vscode-azure-iot-device-workbench",
    "ms-vscode.vscode-azure-iot-edge",
    "ms-vscode.vscode-azure-iot-toolkit",
    "ms-vscode.vscode-azure-iot-workbench",
    "ms-vscode.vscode-azure-iot-device-cube",
    "ms-vscode.vscode-azure-iot-device-simulator",
    "ms-vscode.vscode-azure-iot-hub",
    "ms-vscode.vscode-azure-iot-central",
    "ms-vscode.vscode-azure-iot-device-workbench"
  ]
}
```

### 推荐扩展

- **GitHub Copilot** - AI 代码助手
- **GitLens** - Git 集成增强
- **Thunder Client** - API 测试工具
- **PostgreSQL** - PostgreSQL 支持
- **Docker** - Docker 支持
- **Kubernetes** - K8s 支持

## 🛠️ 项目配置

### 1. 全局配置文件

创建 `.editorconfig` 文件：

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csx,vb,vbx}]
indent_style = space
indent_size = 4

[*.{json,js,jsx,ts,tsx,yml,yaml,xml,html,css,scss,less}]
indent_style = space
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

### 2. 代码分析配置

在项目文件中添加：

```xml
<PropertyGroup>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
  <AnalysisMode>AllEnabledByDefault</AnalysisMode>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.507">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

### 3. 调试配置

创建 `.vscode/launch.json`：

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Hx.Workflow.Api",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Hx.Workflow.Api/bin/Debug/net8.0/Hx.Workflow.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Hx.Workflow.Api",
      "console": "internalConsole",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": "Attach to Process",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

创建 `.vscode/tasks.json`：

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/Hx.Workflow.Api.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "publish",
      "command": "dotnet",
      "type": "process",
      "args": [
        "publish",
        "${workspaceFolder}/Hx.Workflow.Api.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": [
        "watch",
        "run",
        "--project",
        "${workspaceFolder}/src/Hx.Workflow.Api"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## 🔍 调试最佳实践

### 1. 断点策略

- 在关键业务逻辑处设置断点
- 使用条件断点进行复杂调试
- 利用日志断点记录执行路径

### 2. 性能分析

```bash
# 性能分析
dotnet tool install -g dotnet-trace
dotnet tool install -g dotnet-counters
dotnet tool install -g dotnet-dump
dotnet tool install -g dotnet-gcdump
```

### 3. 内存分析

```bash
# 内存分析
dotnet tool install -g dotMemory.Console
dotMemory.exe get-snapshot --process-id <PID>
```

## 📊 代码质量工具

### 1. 静态分析

```bash
# 安装分析器
dotnet tool install -g Microsoft.CodeAnalysis.NetAnalyzers
dotnet tool install -g StyleCop.Analyzers
```

### 2. 代码覆盖率

```bash
# 安装覆盖率工具
dotnet tool install -g coverlet.collector
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### 3. 安全扫描

```bash
# 安装安全扫描工具
dotnet tool install -g Microsoft.Tye
dotnet tool install -g dotnet-outdated-tool
```

## 🗄️ 数据库开发工具

### 1. PostgreSQL 工具

- **pgAdmin 4** - 官方管理工具
- **DBeaver** - 通用数据库工具
- **Azure Data Studio** - 微软数据库工具

### 2. EF Core 工具

```bash
# 安装 EF Core 工具
dotnet tool install -g dotnet-ef
dotnet tool install -g dotnet-aspnet-codegenerator
```

### 3. 数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate --project src/Hx.Workflow.EntityFrameworkCore

# 更新数据库
dotnet ef database update --project src/Hx.Workflow.EntityFrameworkCore

# 生成 SQL 脚本
dotnet ef migrations script --project src/Hx.Workflow.EntityFrameworkCore
```

## 🚀 部署和运维工具

### 1. Docker 支持

```dockerfile
# Dockerfile 示例
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Hx.Workflow.Api/Hx.Workflow.Api.csproj", "src/Hx.Workflow.Api/"]
RUN dotnet restore "src/Hx.Workflow.Api/Hx.Workflow.Api.csproj"
COPY . .
WORKDIR "/src/src/Hx.Workflow.Api"
RUN dotnet build "Hx.Workflow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hx.Workflow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hx.Workflow.Api.dll"]
```

### 2. Kubernetes 支持

```yaml
# k8s 部署配置
apiVersion: apps/v1
kind: Deployment
metadata:
  name: workflow-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: workflow-api
  template:
    metadata:
      labels:
        app: workflow-api
    spec:
      containers:
        - name: workflow-api
          image: workflow-api:latest
          ports:
            - containerPort: 80
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: workflow-secrets
                  key: connection-string
```

## 📝 开发工作流

### 1. 日常开发流程

1. 拉取最新代码
2. 运行测试确保环境正常
3. 创建功能分支
4. 开发功能
5. 运行代码分析
6. 提交代码
7. 创建 Pull Request

### 2. 代码审查清单

- [ ] 代码风格符合规范
- [ ] 单元测试覆盖
- [ ] 集成测试通过
- [ ] 性能影响评估
- [ ] 安全风险检查
- [ ] 文档更新

### 3. 发布流程

1. 版本号更新
2. 更新日志编写
3. 数据库迁移脚本准备
4. 部署脚本更新
5. 测试环境验证
6. 生产环境部署

## 🔧 常用命令

### 开发命令

```bash
# 还原包
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test

# 运行应用
dotnet run --project src/Hx.Workflow.Api

# 发布应用
dotnet publish -c Release

# 清理构建
dotnet clean
```

### 数据库命令

```bash
# 创建迁移
dotnet ef migrations add MigrationName --project src/Hx.Workflow.EntityFrameworkCore

# 更新数据库
dotnet ef database update --project src/Hx.Workflow.EntityFrameworkCore

# 生成 SQL 脚本
dotnet ef migrations script --project src/Hx.Workflow.EntityFrameworkCore

# 删除迁移
dotnet ef migrations remove --project src/Hx.Workflow.EntityFrameworkCore
```

### 性能分析命令

```bash
# 性能分析
dotnet trace collect --name workflow-api --process-id <PID>

# 内存分析
dotnet dump collect --process-id <PID>

# 计数器监控
dotnet counters monitor --process-id <PID>
```

## 📚 学习资源

### 官方文档

- [.NET 8 文档](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core 文档](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core 文档](https://docs.microsoft.com/ef/core/)
- [ABP Framework 文档](https://docs.abp.io/)

### 最佳实践

- [C# 编码约定](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [ASP.NET Core 性能最佳实践](https://docs.microsoft.com/aspnet/core/performance/performance-best-practices)
- [EF Core 性能最佳实践](https://docs.microsoft.com/ef/core/performance/)

### 工具和插件

- [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
- [VS Code Extensions](https://marketplace.visualstudio.com/vscode)
- [JetBrains ReSharper](https://www.jetbrains.com/resharper/)

## 🆘 故障排除

### 常见问题

1. **构建失败** - 检查包引用和版本兼容性
2. **运行时错误** - 检查配置文件和环境变量
3. **数据库连接问题** - 验证连接字符串和网络
4. **性能问题** - 使用性能分析工具定位瓶颈

### 调试技巧

1. 使用 `System.Diagnostics.Debugger.Break()` 强制断点
2. 利用 `ILogger` 记录关键信息
3. 使用 `dotnet-dump` 分析崩溃转储
4. 启用详细日志记录进行问题排查

---

**注意**: 请根据项目具体需求调整配置，并定期更新工具和插件版本。
