# Capture Zone Database Persistence

## Overview

Capture zones are now fully persisted to the database, allowing them to survive server restarts. The system uses a schema versioning approach with automatic migrations.

## Database Schema

### Schema Version

- **Previous Version**: 4
- **Current Version**: 5
- **Migration**: Automatically applied on server startup

### capture_zones Table

```sql
CREATE TABLE capture_zones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    node_name TEXT NOT NULL,
    zone_id TEXT NOT NULL,
    zone_name TEXT NOT NULL,
    center_x REAL NOT NULL,
    center_y REAL NOT NULL,
    center_z REAL NOT NULL,
    radius INTEGER NOT NULL,
    point_multiplier REAL NOT NULL DEFAULT 1.0,
    is_active INTEGER NOT NULL DEFAULT 1,
    description TEXT,
    created_at INTEGER NOT NULL,
    UNIQUE (node_name, zone_id)
);
```

### Indexes

- `idx_capture_zones_node` - Fast lookup by node name
- `idx_capture_zones_active` - Fast filtering by node and active status

## Data Flow

### Adding a Capture Zone

1. **Command**: `/nodewaradmin addzone {nodeId} {zoneId} {zoneName} {radius}`
2. **NodeWarCommands**: Creates `CaptureZone` object
3. **NodeWarManager.AddCaptureZone()**: Adds to in-memory collection
4. **NodeManager.AddCaptureZone()**: Writes to cache (if used)
5. **NodeRepository.AddCaptureZone()**: Inserts into database
6. **Result**: Returns database ID or -1 on failure

### Removing a Capture Zone

1. **Command**: `/nodewaradmin removezone {nodeId} {zoneId}`
2. **NodeWarCommands**: Validates zone exists
3. **NodeWarManager.RemoveCaptureZone()**: Removes from memory
4. **NodeManager.RemoveCaptureZone()**: Updates cache
5. **NodeRepository.RemoveCaptureZone()**: Deletes from database
6. **Result**: Returns true/false

### Loading Capture Zones on Startup

1. **PVPModSystem.StartServerSide()**: Initializes NodeWarManager
2. **NodeWarManager.SetNodeManager()**: Calls LoadNodesFromDatabase()
3. **LoadNodesFromDatabase()**:
   - Loads all nodes from database
   - For each node, calls `GetCaptureZonesForNode()`
   - Creates `CaptureZone` objects from `CaptureZoneData`
   - Adds zones to `NodeZone.CaptureZones` dictionary
4. **Result**: All zones restored to memory

## Implementation Details

### NodeRepository Methods

```csharp
// Add a new capture zone
public int AddCaptureZone(string nodeName, string zoneId, string zoneName, 
    double centerX, double centerY, double centerZ, int radius, 
    double pointMultiplier = 1.0, bool isActive = true, string? description = null)

// Remove a capture zone
public bool RemoveCaptureZone(string nodeName, string zoneId)

// Get all zones for a node
public List<CaptureZoneData> GetCaptureZonesForNode(string nodeName)

// Get all zones from database
public List<CaptureZoneData> GetAllCaptureZones()

// Update zone properties
public bool UpdateCaptureZone(string nodeName, string zoneId, ...)

// Remove all zones for a node
public bool RemoveAllCaptureZonesForNode(string nodeName)
```

### CaptureZoneData DTO

```csharp
public class CaptureZoneData
{
    public int Id { get; set; }
    public string NodeName { get; set; }
    public string ZoneId { get; set; }
    public string ZoneName { get; set; }
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double CenterZ { get; set; }
    public int Radius { get; set; }
    public double PointMultiplier { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public long CreatedAt { get; set; }
}
```

## Migration Process

When the database is upgraded from version 4 to version 5:

1. **Schema Check**: On server startup, `DatabaseSchema.InitializeSchema()` checks current version
2. **Migration Needed**: If version < 5, calls `ApplyMigrations()`
3. **Version 5 Migration**:
   ```csharp
   if (fromVersion < 5)
   {
       // Create capture_zones table
       // Create indexes
   }
   ```
4. **Version Update**: Sets schema version to 5 in `schema_version` table
5. **Complete**: Database is now ready for capture zone persistence

## Testing Persistence

### Test Scenario 1: Add and Restart

1. Start server
2. Register a node: `/nodewaradmin register testnode TestNode 50`
3. Add a capture zone: `/nodewaradmin addzone testnode zone1 "Control Point A" 15`
4. Verify zone exists: `/nodewaradmin listzones testnode`
5. **Restart server**
6. Verify zone still exists: `/nodewaradmin listzones testnode`
7. **Expected**: Zone is restored from database

### Test Scenario 2: Remove and Restart

1. Remove a capture zone: `/nodewaradmin removezone testnode zone1`
2. Verify removal: `/nodewaradmin listzones testnode`
3. **Restart server**
4. Verify zone is gone: `/nodewaradmin listzones testnode`
5. **Expected**: Zone remains deleted after restart

### Test Scenario 3: Multiple Zones

1. Add multiple zones to one node
2. **Restart server**
3. Verify all zones are restored
4. **Expected**: All zones persist correctly

## Error Handling

### Database Save Failures

- If database save fails, in-memory change is rolled back
- Error logged to server console
- User receives error message
- System remains in consistent state

### Database Load Failures

- If loading fails, logs error but continues
- In-memory collection is empty
- New zones can still be created
- Next restart will retry loading

## Performance Considerations

### Caching

- NodeManager implements caching pattern
- All nodes loaded on first access
- Capture zones loaded with nodes
- No per-request database queries

### Indexes

- Node name indexed for fast lookups
- Active status indexed for filtering
- Unique constraint prevents duplicates

## Files Modified

### Database Layer

- `DatabaseSchema.cs`: Added capture_zones table and migration
- `NodeRepository.cs`: Added capture zone CRUD methods + `CaptureZoneData` DTO
- `NodeManager.cs`: Added capture zone wrapper methods

### Business Logic Layer

- `NodeWarManager.cs`: 
  - Modified `LoadNodesFromDatabase()` to load capture zones
  - Added `AddCaptureZone()` with database persistence
  - Added `RemoveCaptureZone()` with database deletion

### Command Layer

- `NodeWarCommands.cs`:
  - Modified `OnAdminAddCaptureZone()` to use persistent add
  - Modified `OnAdminRemoveCaptureZone()` to use persistent remove
  - Added database confirmation messages

## Rollback Instructions

If you need to rollback to non-persistent capture zones:

1. Remove the `capture_zones` table from database
2. Revert `CurrentSchemaVersion` from 5 to 4 in `DatabaseSchema.cs`
3. Remove the version 5 migration code
4. Remove capture zone methods from `NodeRepository.cs` and `NodeManager.cs`
5. Revert changes to `NodeWarManager.cs` and `NodeWarCommands.cs`

**Note**: This will lose all existing capture zone data!

## Future Enhancements

Potential improvements for capture zone persistence:

- **Audit Trail**: Track who created/modified/deleted zones
- **Soft Deletes**: Mark zones as deleted instead of removing
- **Version History**: Keep historical versions of zone configurations
- **Import/Export**: Export zones to JSON for backup/transfer
- **Bulk Operations**: Add/remove multiple zones at once
- **Zone Templates**: Reusable zone configurations
- **Zone Groups**: Organize related zones together

## Conclusion

Capture zones are now fully persistent and will survive server restarts. The database schema has been upgraded to version 5, and all operations (add, remove, list) work seamlessly with the database backend.
