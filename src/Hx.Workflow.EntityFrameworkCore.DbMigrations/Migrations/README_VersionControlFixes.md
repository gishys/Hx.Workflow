# 版本控制外键修复说明

## 修复内容

### 1. WkNodePara 实体修复

- **文件**: `src/Hx.Workflow.Domain/Hx/Workflow/Domain/WkNodePara.cs`
- **修改**:
  - 添加 `Version` 字段
  - 更新构造函数接受 `version` 参数
  - 实现 `GetKeys()` 方法
  - 继承改为 `Entity` 而不是 `Entity<Guid>`

### 2. WkStepBodyParam 实体修复

- **文件**: `src/Hx.Workflow.Domain/Hx/Workflow/Domain/WkStepBodyParam.cs`
- **修改**:

  - 将 `WkNodeId` 字段改为 `WkStepBodyId` 字段（因为参数属于步骤体，不是节点）
  - 更新构造函数接受 `wkStepBodyId` 参数
  - 保持继承 `Entity<Guid>`

- **文件**: `src/Hx.Workflow.Application.Contracts/Hx/Workflow/Application/Contracts/WkStepBodyParamDto.cs`
- **修改**:

  - 将 `WkNodeId` 属性改为 `WkStepBodyId` 属性，以匹配实体定义

- **文件**: `src/Hx.Workflow.Application/Hx/Workflow/Application/WkStepBodyAppService.cs`
- **修改**:

  - 修复 `CreateAsync` 方法：先保存 `WkStepBody`，然后创建参数并设置正确的 `WkStepBodyId`
  - 修复 `UpdateAsync` 方法：使用 `entity.Id` 作为 `WkStepBodyId`

- **文件**: `src/Hx.Workflow.Application/Hx/Workflow/Application/HxWorkflowApplicationModule.cs`
- **修改**:
  - 修复初始化逻辑：先保存 `WkStepBody`，然后创建参数并设置正确的 `WkStepBodyId`

### 3. EF Core 配置修复

- **文件**: `src/Hx.Workflow.EntityFrameworkCore/Hx/Workflow/EntityFrameworkCore/WorkflowModelBuilderConfigrationExtensions.cs`
- **修改**:
  - `WkNodePara` 配置: 添加 `Version` 字段映射和主键配置
  - `WkStepBodyParam` 配置: 将 `WkNodeId` 字段映射改为 `WkStepBodyId` 字段映射
- `WkNode.OutcomeSteps` 外键: 从单一外键改为复合外键 `(WkNodeId, Version)`
- `WkStepBody.Inputs` 外键: 修复为正确的外键关系 `WkStepBodyId`

### 4. 数据库迁移脚本

- **文件**: `src/Hx.Workflow.EntityFrameworkCore.DbMigrations/Migrations/20250221060000_AddVersionToWkNodePara.sql`
- **功能**:

  - 为 `HXWKNODEPARAS` 表添加 `VERSION` 字段
  - 删除旧的外键约束
  - 创建新的复合外键约束

- **文件**: `src/Hx.Workflow.EntityFrameworkCore.DbMigrations/Migrations/20250221070000_FixWkStepBodyParamForeignKey.sql`
- **功能**:
  - 修复 `HXWKSTEPBODYPARAMS` 表的外键关系
  - 将 `WkNodeId` 字段重命名为 `WKSTEPBODYID`
  - 将 `StepBodyParaType` 字段重命名为 `STEPBODYPARATYPE`
  - 将 `HXWKEXECUTIONPOINTER` 表的 `Materials` 字段重命名为 `MATERIALS`
  - 更新外键约束指向 `HXWKSTEPBODYS` 表

## 执行顺序

1. 运行迁移脚本: `20250221060000_AddVersionToWkNodePara.sql`
2. 运行迁移脚本: `20250221070000_FixWkStepBodyParamForeignKey.sql`
3. 重新编译项目
4. 测试版本控制功能

## 注意事项

- 所有与 `WkNode` 相关的外键现在都使用复合键 `(WkNodeId, Version)`
- `WkStepBodyParam` 属于 `WkStepBody`，不是 `WkNode`，所以使用单一外键 `WkStepBodyId`
- 确保在创建新版本时，相关的 `WkNodePara` 记录也要创建新版本
- 外键约束名称已更新为更具描述性的名称
