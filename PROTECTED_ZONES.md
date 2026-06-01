# Protected Zones Feature

## Overview
The Protected Zones feature allows server administrators to define multiple areas on the map where:
- **Land cannot be claimed** by any guild
- **Blocks cannot be broken** by players
- **Blocks can still be interacted with** (doors, chests, etc.)

This is useful for creating spawn protection, community areas, or other safe zones.

## Configuration

Protected zones are configured in the `guild-config.json` file located in the mod's data directory.

### Enabling Protected Zones

```json
{
  "enableProtectedZones": true,
  "protectedZones": [
    {
      "name": "Spawn Area",
      "x": 0,
      "z": 0,
      "radius": 200
    },
    {
      "name": "Trade Hub",
      "x": 1000,
      "z": -500,
      "radius": 150
    }
  ]
}
```

### Configuration Properties

- **enableProtectedZones** (boolean): Master switch to enable/disable all protected zones
- **protectedZones** (array): List of protected zone definitions

### Protected Zone Properties

Each protected zone has the following properties:

- **name** (string): A descriptive name for the zone (e.g., "Spawn Area", "Trade Hub")
- **x** (integer): X coordinate of the zone center (in block coordinates)
- **z** (integer): Z coordinate of the zone center (in block coordinates)
- **radius** (integer): Radius of the protected area in blocks (minimum: 50 blocks)

## How It Works

### Coordinate System
- Coordinates are in **block coordinates** (not chunk coordinates)
- The center of the map is typically at (0, 0)
- Use `/tp [show]` or check your current coordinates to find positions

### Protection Mechanics

1. **Claiming Prevention**: When a guild tries to claim land that overlaps with a protected zone, the claim will be denied
2. **Block Breaking Prevention**: This needs to be integrated with your block breaking logic (see Integration Guide below)
3. **Interaction Allowed**: Players can still interact with doors, chests, and other interactive blocks

### Validation

When the server starts, the mod will:
- Validate all protected zones
- Ensure minimum radius (50 blocks)
- Assign default names to unnamed zones
- Log all active protected zones

## API Methods

The following methods are available in `GuildConfig` for checking protected zones:

```csharp
// Check if a chunk is within any protected zone
bool IsChunkWithinProtectedZone(int chunkX, int chunkZ, Vec3i mapSize)

// Check if a block position is within any protected zone
bool IsWithinProtectedZone(int blockX, int blockZ, Vec3i mapSize)

// Get the specific protected zone at a position (or null if none)
ProtectedZone? GetProtectedZoneAt(int blockX, int blockZ, Vec3i mapSize)
```

## Integration Guide

### Preventing Claims in Protected Zones

In your claim creation logic, add:

```csharp
if (guildConfig.IsChunkWithinProtectedZone(chunkX, chunkZ, mapSize))
{
    // Get the zone name for a better error message
    var zone = guildConfig.GetProtectedZoneAt(blockX, blockZ, mapSize);
    return $"Cannot claim land in protected zone: {zone?.Name ?? "Unknown"}";
}
```

### Preventing Block Breaking in Protected Zones

In your block breaking event handler or privilege patch, add:

```csharp
if (guildConfig.IsWithinProtectedZone(blockX, blockZ, mapSize))
{
    var zone = guildConfig.GetProtectedZoneAt(blockX, blockZ, mapSize);
    player.SendMessage($"You cannot break blocks in protected zone: {zone?.Name ?? "Unknown"}");
    return false; // Prevent the action
}
```

### Allowing Interactions

Interactions (UseBlock events) should NOT check protected zones, allowing players to:
- Open/close doors
- Access chests and containers
- Use crafting stations
- Interact with other game mechanics

## Example Configuration

Here's a complete example with multiple protected zones:

```json
{
  "baseMaxClaimsPerGuild": 20,
  "enableDynamicClaimLimits": true,
  "enableTerritorialRestrictions": false,
  "enableProtectedZones": true,
  "protectedZones": [
    {
      "name": "Spawn Protection",
      "x": 0,
      "z": 0,
      "radius": 250
    },
    {
      "name": "Central Market",
      "x": 500,
      "z": 500,
      "radius": 100
    },
    {
      "name": "Arena",
      "x": -1000,
      "z": 200,
      "radius": 150
    },
    {
      "name": "Community Farm",
      "x": 300,
      "z": -400,
      "radius": 75
    }
  ]
}
```

## Difference from Territorial Restrictions

| Feature | Territorial Restrictions | Protected Zones |
|---------|-------------------------|-----------------|
| Purpose | Define where claims ARE allowed | Define where claims are NOT allowed |
| Type | Single area (whitelist) | Multiple areas (blacklist) |
| Claims | Only allowed INSIDE the area | Not allowed INSIDE the zones |
| Block Breaking | Not affected | Prevented in zones |
| Use Case | Limiting claims to a region | Protecting specific locations |

You can use both features together if needed.

## Troubleshooting

### Protected Zones Not Working
1. Check that `enableProtectedZones` is set to `true`
2. Verify that `protectedZones` array is not empty
3. Check server logs for validation warnings
4. Ensure coordinates are correct (use block coordinates, not chunk coordinates)

### Finding Coordinates
1. Stand at the desired center location
2. Press F3 or use `/tp [show]` to see your coordinates
3. Use the X and Z values (ignore Y)
4. Add these values to your configuration

### Zone Too Small/Large
- Minimum radius is 50 blocks
- Consider: 1 chunk = 32 blocks
- For spawn protection, 200-300 blocks is typical
- For smaller areas like shops, 50-100 blocks works well
