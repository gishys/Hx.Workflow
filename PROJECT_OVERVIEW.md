# Hx.Workflow - 企业级工作流引擎系统

## 📋 项目概述

**Hx.Workflow** 是一个基于 **ABP vNext** 框架和 **WorkflowCore** 引擎构建的企业级工作流管理系统。该系统采用领域驱动设计（DDD）架构，提供完整的工作流定义、执行、监控和管理功能，支持版本控制和多租户。

### 🎯 核心特性

- ✅ **完整的工作流生命周期管理**
- ✅ **版本控制系统** - 支持工作流模板版本管理
- ✅ **多租户支持** - 基于 ABP vNext 的多租户架构
- ✅ **实时监控** - 工作流执行状态实时跟踪
- ✅ **权限控制** - 细粒度的用户权限管理
- ✅ **审计日志** - 完整的操作审计记录
- ✅ **容器化部署** - Docker 和 Kubernetes 支持
- ✅ **RESTful API** - 标准化的 API 接口
- ✅ **Swagger 文档** - 自动生成的 API 文档

## 🏗️ 技术架构

### 技术栈

| 层级           | 技术         | 版本 | 说明             |
| -------------- | ------------ | ---- | ---------------- |
| **框架**       | ABP vNext    | 8.0+ | 企业级应用框架   |
| **运行时**     | .NET 8       | 8.0+ | 跨平台运行时     |
| **工作流引擎** | WorkflowCore | 最新 | 轻量级工作流引擎 |
| **数据库**     | PostgreSQL   | 15+  | 主数据库         |
| **缓存**       | Redis        | 7+   | 分布式缓存       |
| **容器化**     | Docker       | 最新 | 容器化部署       |
| **编排**       | Kubernetes   | 最新 | 容器编排         |

### 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Web API       │  │   Swagger UI    │  │   Admin UI  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ WorkflowAppSvc  │  │ DefinitionAppSvc│  │ InstanceAppSvc│ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ HxWorkflowMgr   │  │ WkDefinition    │  │ WkInstance  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ EF Core         │  │ PostgreSQL      │  │ Redis Cache │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 📁 项目结构

```
Hx.Workflow/
├── src/
│   ├── Hx.Workflow.Api/                    # API 层 - 应用程序入口
│   │   ├── Controllers/                     # API 控制器
│   │   ├── Hx/Workflow/Api/                # API 模块配置
│   │   ├── DemoJson/                       # 示例 JSON 数据
│   │   └── Program.cs                      # 应用程序入口点
│   │
│   ├── Hx.Workflow.Application/            # 应用服务层
│   │   ├── Hx/Workflow/Application/        # 应用服务实现
│   │   ├── StepBodys/                      # 步骤体实现
│   │   └── DynamicCode/                    # 动态代码执行
│   │
│   ├── Hx.Workflow.Application.Contracts/  # 应用契约层
│   │   └── Hx/Workflow/Application/Contracts/ # 服务接口和 DTO
│   │
│   ├── Hx.Workflow.Domain/                 # 领域层
│   │   ├── Hx/Workflow/Domain/             # 领域实体和服务
│   │   ├── Persistence/                    # 持久化实体
│   │   ├── Repositories/                   # 仓储接口
│   │   └── JsonDefinition/                 # JSON 定义模型
│   │
│   ├── Hx.Workflow.Domain.Shared/         # 共享领域层
│   │   ├── Hx/Workflow/Domain/Shared/      # 共享枚举和常量
│   │   └── Localization/                   # 本地化资源
│   │
│   ├── Hx.Workflow.EntityFrameworkCore/   # 数据访问层
│   │   ├── Hx/Workflow/EntityFrameworkCore/ # EF Core 配置
│   │   └── WorkflowExtensions.cs           # 工作流扩展
│   │
│   ├── Hx.Workflow.EntityFrameworkCore.DbMigrations/ # 数据库迁移
│   │   ├── Migrations/                     # EF Core 迁移
│   │   └── Migration_AddVersion/           # 版本控制迁移脚本
│   │
│   └── Hx.Workflow.HttpApi/               # HTTP API 层
│       └── Hx/Workflow/HttpApi/            # HTTP API 控制器
│
├── .vscode/                                # VS Code 配置
├── Dockerfile                              # Docker 配置
├── docker-compose.yml                      # Docker Compose 配置
├── install-dev-tools.ps1                   # 开发工具安装脚本
├── DEVELOPMENT_SETUP.md                    # 开发环境配置指南
├── QUICK_START.md                         # 快速开始指南
└── PROJECT_OVERVIEW.md                    # 项目介绍文档
```

## 🔧 核心功能模块

### 1. 工作流定义管理 (Workflow Definition)

**功能描述**: 管理工作流模板的定义、版本控制和发布

**核心组件**:

- `WkDefinition` - 工作流定义实体
- `WkNode` - 工作流节点实体
- `WkStepBody` - 步骤体定义
- `WkDefinitionAppService` - 定义管理服务

**主要功能**:

- ✅ 创建工作流模板
- ✅ 版本控制和历史管理
- ✅ 节点配置和连接
- ✅ 步骤体参数配置
- ✅ 模板发布和注册

### 2. 工作流实例管理 (Workflow Instance)

**功能描述**: 管理工作流实例的创建、执行和监控

**核心组件**:

- `WkInstance` - 工作流实例实体
- `WkExecutionPointer` - 执行指针
- `WkCandidate` - 候选人管理
- `WorkflowAppService` - 实例管理服务

**主要功能**:

- ✅ 启动工作流实例
- ✅ 实例状态跟踪
- ✅ 执行指针管理
- ✅ 候选人分配
- ✅ 实例数据管理

### 3. 步骤体系统 (Step Body)

**功能描述**: 提供可扩展的步骤体框架

**核心组件**:

- `StartStepBody` - 开始步骤
- `StopStepBody` - 结束步骤
- `GeneralAuditingStepBody` - 通用审核步骤
- `ExternalInterfaceStepBody` - 外部接口步骤

**主要功能**:

- ✅ 内置步骤体
- ✅ 自定义步骤体
- ✅ 动态代码执行
- ✅ 参数配置
- ✅ 条件分支

### 4. 版本控制系统 (Version Control)

**功能描述**: 支持工作流模板的版本管理和历史追踪

**核心特性**:

- ✅ 复合主键设计 `(Id, Version)`
- ✅ 版本历史管理
- ✅ 版本比较功能
- ✅ 版本回滚支持
- ✅ 实例版本关联

**数据库设计**:

```sql
-- 支持版本控制的表
HXWKDEFINITIONS (ID, VERSION) - 工作流定义
HXDEFINITION_CANDIDATES (NODEID, CANDIDATEID, VERSION) - 定义候选人
HXNODE_CANDIDATES (NODEID, CANDIDATEID, VERSION) - 节点候选人
HXWKNODES (ID, VERSION) - 工作流节点
HXWKINSTANCES (ID, VERSION) - 工作流实例
HX_NODES_APPLICATION_FORMS (NODE_ID, APPLICATION_ID, VERSION) - 节点表单关联

-- 不需要版本控制的表（运行时数据）
HXWKEXECUTIONPOINTER (ID) - 执行指针
HXPOINTER_CANDIDATES (NODEID, CANDIDATEID) - 指针候选人
```

### 5. 权限和审计系统 (Permission & Audit)

**功能描述**: 提供细粒度的权限控制和完整的审计日志

**核心组件**:

- `WkAuditor` - 审核人实体
- `WkCandidate` - 候选人实体
- `AuditUpdateDto` - 审核更新 DTO

**主要功能**:

- ✅ 用户权限管理
- ✅ 角色权限分配
- ✅ 操作审计日志
- ✅ 审核流程管理
- ✅ 候选人管理

## 🚀 API 接口

### 工作流管理接口

| 方法   | 路径                                            | 描述                           |
| ------ | ----------------------------------------------- | ------------------------------ |
| `POST` | `/hxworkflow/workflow`                          | 启动工作流实例                 |
| `POST` | `/hxworkflow/workflow/activity`                 | 执行工作流活动                 |
| `GET`  | `/hxworkflow/workflow/mywkinstances`            | 获取我的工作流实例             |
| `GET`  | `/hxworkflow/workflow/mywkinstances/version`    | 获取我的工作流实例（支持版本） |
| `GET`  | `/hxworkflow/workflow/candidate/{wkInstanceId}` | 获取候选人列表                 |
| `GET`  | `/hxworkflow/workflow/definitionscancreate`     | 获取可创建的模板               |

### 实例管理接口

| 方法  | 路径                                         | 描述          |
| ----- | -------------------------------------------- | ------------- |
| `GET` | `/hxworkflow/workflow/workflowinstance`      | 获取实例详情  |
| `GET` | `/hxworkflow/workflow/workflowinstancenodes` | 获取实例节点  |
| `PUT` | `/hxworkflow/instance/receive`               | 接收实例      |
| `PUT` | `/hxworkflow/instance/businessdata`          | 更新业务数据  |
| `PUT` | `/hxworkflow/instance/follow`                | 关注/取消关注 |

### 审核管理接口

| 方法  | 路径                         | 描述             |
| ----- | ---------------------------- | ---------------- |
| `PUT` | `/hxworkflow/workflow/audit` | 执行审核         |
| `GET` | `/hxworkflow/workflow/audit` | 获取审核信息     |
| `PUT` | `/hxworkflow/workflow/data`  | 更新执行指针数据 |
| `PUT` | `/hxworkflow/workflow/retry` | 重试执行         |

### 统计接口

| 方法  | 路径                                        | 描述         |
| ----- | ------------------------------------------- | ------------ |
| `GET` | `/hxworkflow/workflow/processingstatusstat` | 处理状态统计 |
| `GET` | `/hxworkflow/workflow/businesstypestat`     | 业务类型统计 |
| `GET` | `/hxworkflow/workflow/processtypestat`      | 流程类型统计 |

## 🗄️ 数据库设计

### 核心表结构

#### 1. 工作流定义表 (HXWKDEFINITIONS)

```sql
CREATE TABLE "HXWKDEFINITIONS" (
    "ID" UUID NOT NULL,
    "VERSION" INTEGER NOT NULL,
    "TITLE" VARCHAR(255),
    "DESCRIPTION" TEXT,
    "JSONDEFINITION" TEXT,
    "CREATIONTIME" TIMESTAMP,
    "CREATORID" UUID,
    "LASTMODIFICATIONTIME" TIMESTAMP,
    "LASTMODIFIERID" UUID,
    "ISDELETED" BOOLEAN,
    "DELETERID" UUID,
    "DELETIONTIME" TIMESTAMP,
    CONSTRAINT "PK_WKDEFINITION" PRIMARY KEY ("ID", "VERSION")
);
```

#### 2. 工作流节点表 (HXWKNODES)

```sql
CREATE TABLE "HXWKNODES" (
    "ID" UUID NOT NULL,
    "VERSION" INTEGER NOT NULL,
    "WKDIFINITIONID" UUID NOT NULL,
    "NAME" VARCHAR(255),
    "STEPNODETYPE" INTEGER,
    "STEPBODYID" UUID,
    "CREATIONTIME" TIMESTAMP,
    "CREATORID" UUID,
    CONSTRAINT "PK_WKNODES" PRIMARY KEY ("ID", "VERSION"),
    CONSTRAINT "FK_WKNODES_WKDEFINITION_COMPOSITE"
        FOREIGN KEY ("WKDIFINITIONID", "VERSION")
        REFERENCES "HXWKDEFINITIONS" ("ID", "VERSION")
);
```

#### 3. 工作流实例表 (HXWKINSTANCES)

```sql
CREATE TABLE "HXWKINSTANCES" (
    "ID" UUID NOT NULL,
    "VERSION" INTEGER NOT NULL,
    "WKDIFINITIONID" UUID NOT NULL,
    "WORKFLOWID" VARCHAR(255),
    "REFERENCE" VARCHAR(255),
    "STATUS" INTEGER,
    "CREATIONTIME" TIMESTAMP,
    "CREATORID" UUID,
    CONSTRAINT "PK_WKINSTANCES" PRIMARY KEY ("ID", "VERSION"),
    CONSTRAINT "FK_WKINSTANCES_WKDEFINITION_COMPOSITE"
        FOREIGN KEY ("WKDIFINITIONID", "VERSION")
        REFERENCES "HXWKDEFINITIONS" ("ID", "VERSION")
);
```

## 🔄 工作流执行流程

### 1. 工作流定义阶段

```
1. 创建 WkDefinition 实体
2. 配置 WkNode 节点
3. 设置 WkStepBody 步骤体
4. 配置节点连接和条件
5. 注册到 WorkflowCore 引擎
```

### 2. 工作流实例执行阶段

```
1. 调用 StartWorkflowAsync 启动实例
2. WorkflowCore 引擎执行工作流
3. 根据节点配置执行步骤体
4. 更新执行指针状态
5. 分配候选人进行审核
6. 记录执行日志和审计信息
```

### 3. 版本控制流程

```
1. 创建新版本时复制原版本数据
2. 修改节点和步骤体配置
3. 注册新版本到引擎
4. 保持旧版本实例继续运行
5. 新实例使用新版本模板
```

## 🛠️ 开发环境配置

### 环境要求

- **.NET 8 SDK** (8.0.100+)
- **Visual Studio 2022** (17.8+) 或 **VS Code**
- **PostgreSQL 15+**
- **Redis 7+** (可选)
- **Docker** (可选)

### 快速开始

```bash
# 1. 克隆项目
git clone <repository-url>
cd workflow

# 2. 安装开发工具
.\install-dev-tools.ps1

# 3. 配置数据库连接
# 编辑 src/Hx.Workflow.Api/appsettings.Development.json

# 4. 运行数据库迁移
dotnet ef database update --project src/Hx.Workflow.EntityFrameworkCore

# 5. 启动应用
dotnet run --project src/Hx.Workflow.Api
```

### Docker 部署

```bash
# 使用 Docker Compose 启动所有服务
docker-compose up -d

# 访问地址
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# pgAdmin: http://localhost:5050
```

## 📊 性能特性

### 1. 数据库优化

- ✅ 复合主键设计提高查询效率
- ✅ 索引优化支持快速检索
- ✅ 分页查询减少内存占用
- ✅ 连接池管理提高并发性能

### 2. 缓存策略

- ✅ Redis 缓存热点数据
- ✅ 工作流定义缓存
- ✅ 用户权限缓存
- ✅ 统计结果缓存

### 3. 并发控制

- ✅ 乐观锁防止并发冲突
- ✅ 事务管理保证数据一致性
- ✅ 异步处理提高响应速度
- ✅ 队列机制处理高并发

## 🔒 安全特性

### 1. 身份认证

- ✅ JWT Token 认证
- ✅ 多租户隔离
- ✅ 用户会话管理
- ✅ 密码加密存储

### 2. 权限控制

- ✅ 基于角色的权限控制 (RBAC)
- ✅ 细粒度权限管理
- ✅ 数据级权限控制
- ✅ 操作审计日志

### 3. 数据安全

- ✅ SQL 注入防护
- ✅ XSS 攻击防护
- ✅ CSRF 防护
- ✅ 敏感数据加密

## 📈 监控和运维

### 1. 日志系统

- ✅ Serilog 结构化日志
- ✅ 日志级别控制
- ✅ 日志文件轮转
- ✅ 日志聚合分析

### 2. 性能监控

- ✅ 应用性能监控 (APM)
- ✅ 数据库性能监控
- ✅ 内存使用监控
- ✅ 响应时间监控

### 3. 健康检查

- ✅ 应用健康检查
- ✅ 数据库连接检查
- ✅ 依赖服务检查
- ✅ 自定义健康检查

## 🚀 部署方案

### 1. 单机部署

```bash
# 直接运行
dotnet run --project src/Hx.Workflow.Api

# 发布后运行
dotnet publish -c Release
dotnet Hx.Workflow.Api.dll
```

### 2. Docker 部署

```bash
# 构建镜像
docker build -t workflow-api .

# 运行容器
docker run -p 5000:80 workflow-api

# 使用 Docker Compose
docker-compose up -d
```

### 3. Kubernetes 部署

```yaml
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
```

## 📚 文档资源

### 开发文档

- [快速开始指南](QUICK_START.md)
- [开发环境配置](DEVELOPMENT_SETUP.md)
- [版本控制迁移指南](src/Hx.Workflow.EntityFrameworkCore.DbMigrations/Migrations/Migration_AddVersion/README.md)

### API 文档

- **Swagger UI**: http://localhost:5000/swagger
- **API 文档**: http://localhost:5000/api-docs

### 学习资源

- [ABP Framework 文档](https://docs.abp.io/)
- [WorkflowCore 文档](https://workflowcore.io/)
- [.NET 8 文档](https://docs.microsoft.com/dotnet/)

## 🤝 贡献指南

### 开发流程

1. Fork 项目仓库
2. 创建功能分支
3. 提交代码更改
4. 创建 Pull Request
5. 代码审查和合并

### 代码规范

- 遵循 C# 编码约定
- 使用 EditorConfig 配置
- 编写单元测试
- 更新相关文档

### 问题反馈

- 使用 GitHub Issues 报告问题
- 提供详细的错误信息和复现步骤
- 标注问题类型和优先级

## 📄 许可证

本项目采用 [MIT License](LICENSE) 许可证。

## 📞 联系方式

- **项目维护者**: Hx.Workflow Team
- **邮箱**: dev@workflow.com
- **GitHub**: https://github.com/your-org/hx-workflow
- **文档**: https://docs.workflow.com

---

**Hx.Workflow** - 让工作流管理变得简单高效！ 🚀
