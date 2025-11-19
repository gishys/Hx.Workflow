# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
