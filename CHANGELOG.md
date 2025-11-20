# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.96] - 2025-11-20

### 修复

- **版本创建逻辑优化**：重构了 `CreateNewVersionForNodeUpdateAsync` 方法，修复了节点 ID 继承问题
  - 不再使用 `ToWkNodes` 方法，因为该方法会保留传入的节点 ID，导致新版本节点与旧版本节点关联
  - 创建新版本时，所有节点都使用新生成的 ID，确保新版本节点与旧版本节点完全独立
  - 简化了节点创建和关系建立的逻辑流程，使代码更清晰易懂

### 变更

- **删除冗余字段**：从 `DefinitionNodeUpdateDto` 中删除了 `RecreateNodes` 字段
  - 该字段在业务逻辑上没有意义，因为创建新版本时所有节点都应该是全新的
  - 简化了 API 接口，减少了不必要的参数

### 技术细节

#### 版本创建逻辑重构

`CreateNewVersionForNodeUpdateAsync` 方法的核心目的是：克隆一个新的模板定义，并把最新的节点等信息维护到模板定义上。

**关键改进点：**

1. **节点 ID 管理**

   - 创建新版本时，所有节点都使用 `GuidGenerator.Create()` 生成新 ID
   - 忽略 `inputNode.Id`，确保节点是新版本的一部分
   - 避免了新版本节点与旧版本节点的 ID 冲突

2. **节点创建流程**

   - 第一步：创建所有节点实例（使用新 ID）
   - 第二步：建立节点关系（通过节点名称）
   - 第三步：将所有节点添加到新实体
   - 使用节点名称作为字典键（因为 ID 都是新的）

3. **代码简化**

   - 移除了复杂的节点匹配和克隆逻辑
   - 移除了 `RecreateNodes` 相关的条件判断
   - 代码结构更清晰，易于理解和维护

4. **EF Core 变更跟踪**
   - 所有节点都是新实例，避免了变更跟踪冲突
   - 不再需要处理已跟踪实体的克隆问题

---

## [1.1.95] - 2025-02-24

### Added

- **Extension Properties Update**: Added support for updating workflow definition extension properties in `UpdateAsync(DefinitionNodeUpdateDto)` method
  - Extension properties are now properly updated when updating nodes in the current version
  - Ensures consistency between creating new versions and updating existing versions
- **Version Registration Check**: Added `IsRegistered` method to `IHxWorkflowManager` interface to check if a workflow definition is registered in the workflow engine
  - Allows early validation before attempting to start workflow instances
  - Provides better error messages when workflow definitions are not registered

### Fixed

- **Version-Aware Candidate Retrieval**: Fixed `GetCandidatesAsync` method to use instance version instead of latest version
  - Added optional `version` parameter to `GetCandidatesAsync` method in `IWkDefinitionRespository`
  - Modified `WorkflowAppService.GetCandidatesAsync` to pass instance version when retrieving candidates
  - Ensures candidates are retrieved from the correct template version that matches the instance
- **Version-Aware Template Definition Retrieval**: Fixed `GeneralAuditingStepBody` to use instance version when retrieving template definition
  - Changed from `FindAsync(instance.WkDifinitionId)` to `GetDefinitionAsync(instance.WkDifinitionId, instance.Version)`
  - Ensures the correct template version is used for instances, preventing version mismatches
- **Archived Definitions Filtering**: Fixed `GetListHasPermissionAsync` to exclude archived workflow definitions
  - Added `!d.IsArchived` condition to filter out archived definitions
  - Ensures only active (non-archived) definitions are returned for workflow creation

### Changed

- **Version Validation**: Enhanced version number validation in workflow startup process
  - Added validation to ensure version number is greater than 0 in `StartAsync` method
  - Added validation to ensure version number is greater than 0 in `GetDefinitionAsync` method
  - Provides clearer error messages when invalid version numbers are provided
- **Error Message Improvements**: Significantly improved error messages throughout the workflow startup process
  - All error messages now start with "流程启动失败：" prefix for consistency
  - Error messages include detailed context information (template ID, version number, candidate ID, etc.)
  - Error messages provide actionable guidance for resolving issues
  - Improved exception handling to distinguish between `UserFriendlyException` and other exceptions
- **Version Registration Check Timing**: Optimized version registration check to occur earlier in the workflow startup process
  - Registration check now happens immediately after template definition query and archive check
  - Provides more immediate feedback when workflow definitions are not registered
  - Better error messages that include template ID and version number

### Technical Details

#### Version-Aware Operations

All operations that retrieve workflow definitions from instances now use the instance's version number:

- **Candidate Retrieval**: `GetCandidatesAsync` now accepts an optional `version` parameter
  - When version is provided, queries the specific version
  - When version is not provided, queries the latest version (backward compatible)
- **Template Definition Retrieval**: All step bodies and services now use `GetDefinitionAsync(id, version)` instead of `FindAsync(id)`
  - Ensures consistency between instance version and template version
  - Prevents issues when template definitions have multiple versions

#### Error Message Standardization

All error messages in `StartAsync` method follow a consistent format:

- **Prefix**: "流程启动失败：" for all workflow startup errors
- **Context**: Includes relevant IDs (template ID, version number, candidate ID)
- **Guidance**: Provides actionable suggestions for resolving the issue
- **Format**: Uses consistent formatting (e.g., "模板 ID：{id}，版本号：{version}")

#### Exception Handling Improvements

- `UserFriendlyException` exceptions are re-thrown directly to preserve original error messages
- Other exceptions are wrapped with user-friendly error messages
- Better distinction between validation errors and system errors

---

## [1.1.91] - 2025-11-19

### Added

- **API Enhancement**: Added `includeArchived` parameter to `GetAllAsync` method in `HxDefinitionGroupController` to control whether archived template definitions are included in results (default: false)
- **Domain Methods**: Added new setter methods to `WkNode` entity:
  - `SetDisplayName(string displayName)` - Sets the display name of a node
  - `SetStepNodeType(StepNodeType stepNodeType)` - Sets the step node type
  - `SetLimitTime(int? limitTime)` - Sets the limit time for a node
- **Repository Enhancement**: Added `includeArchived` parameter support to `GetAllWithChildrenAsync` method in `WkDefinitionGroupRepository` to filter archived definitions

### Fixed

- **Node Update Logic**: Fixed `UpdateNodesInCurrentVersionAsync` method to properly update all node properties from `WkNodeCreateDto`:
  - Added missing updates for `DisplayName`, `StepNodeType`, and `LimitTime` properties
  - Added missing updates for collection properties: `OutcomeSteps`, `ApplicationForms`, `Params`, and `Materials`
  - Enhanced `Materials` update to properly handle nested `Children` collections recursively
- **Version Creation Logic**: Fixed `CreateNewVersionForNodeUpdateAsync` method to use node ID instead of Name for matching nodes:
  - Changed dictionary key from `n.Name` to `n.Id` to ensure correct node matching even when Name is updated
  - Improved `inputNode` lookup logic to prioritize ID matching over Name matching for better reliability
  - Added fallback to Name matching for compatibility with new nodes that don't have an ID yet

### Changed

- **Repository Query Logic**: Updated `GetAllWithChildrenAsync` in `WkDefinitionGroupRepository` to filter archived definitions based on `includeArchived` parameter:
  - When `includeArchived = false` (default): Only returns latest non-archived versions
  - When `includeArchived = true`: Returns latest versions including archived ones
  - Updated subquery logic to respect the `IsArchived` flag when calculating max version

### Technical Details

#### Node Property Updates

The `UpdateNodesInCurrentVersionAsync` method now properly updates all properties defined in `WkNodeCreateDto`:

- Basic properties: `DisplayName`, `StepNodeType`, `LimitTime`
- Step body: `WkStepBodyId` (via `SetWkStepBody`)
- Collections: `OutcomeSteps`, `ApplicationForms`, `Params`, `Materials`, `WkCandidates`
- Relationships: `NextNodes` (upstream/downstream relationships)
- Extended properties: `ExtraProperties`

All collection properties use a "clear and add" strategy to ensure deleted items are removed, modified items are updated, and new items are added.

#### Node Matching Improvements

The `CreateNewVersionForNodeUpdateAsync` method now uses stable node IDs for matching instead of names, which ensures:

- Correct node matching even when node names are updated
- Better reliability for node updates during version creation
- Backward compatibility with new nodes that don't have IDs yet

---

## Previous Versions

[Previous changelog entries can be found in the git history]
