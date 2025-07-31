# 多阶段构建 Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 设置环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["src/Hx.Workflow.Api/Hx.Workflow.Api.csproj", "src/Hx.Workflow.Api/"]
COPY ["src/Hx.Workflow.Application/Hx.Workflow.Application.csproj", "src/Hx.Workflow.Application/"]
COPY ["src/Hx.Workflow.Application.Contracts/Hx.Workflow.Application.Contracts.csproj", "src/Hx.Workflow.Application.Contracts/"]
COPY ["src/Hx.Workflow.Domain/Hx.Workflow.Domain.csproj", "src/Hx.Workflow.Domain/"]
COPY ["src/Hx.Workflow.Domain.Shared/Hx.Workflow.Domain.Shared.csproj", "src/Hx.Workflow.Domain.Shared/"]
COPY ["src/Hx.Workflow.EntityFrameworkCore/Hx.Workflow.EntityFrameworkCore.csproj", "src/Hx.Workflow.EntityFrameworkCore/"]
COPY ["src/Hx.Workflow.EntityFrameworkCore.DbMigrations/Hx.Workflow.EntityFrameworkCore.DbMigrations.csproj", "src/Hx.Workflow.EntityFrameworkCore.DbMigrations/"]
COPY ["src/Hx.Workflow.HttpApi/Hx.Workflow.HttpApi.csproj", "src/Hx.Workflow.HttpApi/"]

# 还原包
RUN dotnet restore "src/Hx.Workflow.Api/Hx.Workflow.Api.csproj"

# 复制所有源代码
COPY . .

# 构建应用
WORKDIR "/src/src/Hx.Workflow.Api"
RUN dotnet build "Hx.Workflow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hx.Workflow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建非 root 用户
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Hx.Workflow.Api.dll"] 