# .NET 8 工作流项目开发工具安装脚本
# 运行此脚本前请确保已安装 .NET 8 SDK

Write-Host "🚀 开始安装 .NET 8 工作流项目开发工具..." -ForegroundColor Green

# 检查 .NET 8 SDK
Write-Host "检查 .NET 8 SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($dotnetVersion -notlike "8.*") {
    Write-Host "❌ 请先安装 .NET 8 SDK" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green

# 安装全局工具
Write-Host "安装全局工具..." -ForegroundColor Yellow

$tools = @(
    "dotnet-ef",
    "dotnet-aspnet-codegenerator",
    "dotnet-trace",
    "dotnet-counters",
    "dotnet-dump",
    "dotnet-gcdump",
    "coverlet.collector",
    "dotnet-reportgenerator-globaltool",
    "Microsoft.Tye",
    "dotnet-outdated-tool"
)

foreach ($tool in $tools) {
    Write-Host "安装 $tool..." -ForegroundColor Cyan
    dotnet tool install -g $tool
}

# 安装项目依赖
Write-Host "还原项目依赖..." -ForegroundColor Yellow
dotnet restore

# 构建项目
Write-Host "构建项目..." -ForegroundColor Yellow
dotnet build

# 运行测试
Write-Host "运行测试..." -ForegroundColor Yellow
dotnet test

Write-Host "✅ 开发工具安装完成！" -ForegroundColor Green
Write-Host ""
Write-Host "📋 下一步操作：" -ForegroundColor Cyan
Write-Host "1. 配置数据库连接字符串" -ForegroundColor White
Write-Host "2. 运行数据库迁移: dotnet ef database update" -ForegroundColor White
Write-Host "3. 启动应用: dotnet run --project src/Hx.Workflow.Api" -ForegroundColor White
Write-Host ""
Write-Host "🔧 推荐安装的 Visual Studio 插件：" -ForegroundColor Cyan
Write-Host "- C# Dev Kit" -ForegroundColor White
Write-Host "- IntelliCode" -ForegroundColor White
Write-Host "- CodeMaid" -ForegroundColor White
Write-Host "- SonarLint" -ForegroundColor White
Write-Host "- GitHub Copilot" -ForegroundColor White
Write-Host ""
Write-Host "📚 详细配置请参考: DEVELOPMENT_SETUP.md" -ForegroundColor Cyan 