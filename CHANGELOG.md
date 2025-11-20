# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
