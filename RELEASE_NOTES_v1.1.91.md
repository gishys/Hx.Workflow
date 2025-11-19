# Release Notes - Version 1.1.91

**Release Date**: 2025-11-19

## Overview

This release focuses on improving node update functionality and adding support for filtering archived workflow definitions in API queries. The main improvements include complete node property updates and more reliable node matching logic during version creation.

## New Features

### 1. Archive Filtering Support

Added the ability to control whether archived template definitions are included in query results:

- **API Endpoint**: `GET /hxworkflow/hxdefinitiongroup/all`
- **New Parameter**: `includeArchived` (boolean, default: `false`)
- **Usage**:
  - `GET /hxworkflow/hxdefinitiongroup/all` - Returns only non-archived definitions (default behavior)
  - `GET /hxworkflow/hxdefinitiongroup/all?includeArchived=true` - Returns all definitions including archived ones

This enhancement allows clients to explicitly request archived definitions when needed, while maintaining backward compatibility with the default behavior of excluding them.

## Bug Fixes

### 1. Complete Node Property Updates

**Issue**: The `UpdateNodesInCurrentVersionAsync` method was not updating all node properties defined in `WkNodeCreateDto`, specifically:

- `DisplayName`
- `StepNodeType`
- `LimitTime`
- `OutcomeSteps` (branch node parameters)
- `ApplicationForms` (form collections)
- `Params` (workflow parameters)
- `Materials` (material collections, including nested children)

**Fix**: Enhanced the update logic to properly update all properties:

- Added new setter methods to `WkNode` entity for controlled property updates
- Implemented complete collection update logic with "clear and add" strategy
- Added recursive handling for nested `Materials.Children` collections

**Impact**: Node updates now correctly reflect all changes made to node properties, ensuring data consistency.

### 2. Node Matching During Version Creation

**Issue**: The `CreateNewVersionForNodeUpdateAsync` method used node names for matching, which could fail when node names were updated.

**Fix**: Changed node matching logic to use stable node IDs:

- Updated dictionary key from `n.Name` to `n.Id` for reliable node matching
- Improved `inputNode` lookup to prioritize ID matching over name matching
- Added fallback to name matching for new nodes without IDs (backward compatibility)

**Impact**: Version creation now correctly handles node updates even when node names change, preventing data loss and ensuring correct node associations.

## Technical Improvements

### Domain Layer

Added new methods to `WkNode` entity for controlled property updates:

```csharp
Task SetDisplayName(string displayName)
Task SetStepNodeType(StepNodeType stepNodeType)
Task SetLimitTime(int? limitTime)
```

These methods provide a controlled way to update node properties while maintaining domain invariants.

### Repository Layer

Enhanced `GetAllWithChildrenAsync` method to support archive filtering:

- Updated query logic to filter by `IsArchived` flag
- Modified subquery to exclude archived versions when calculating max version
- Maintains performance with efficient database-level filtering

## Migration Notes

No database migrations are required for this release. All changes are backward compatible.

## Breaking Changes

None. This release maintains full backward compatibility.

## Deprecations

None.

## Known Issues

None.

## Contributors

- Development Team

---

For detailed technical documentation, please refer to the inline code comments and the main project documentation.
