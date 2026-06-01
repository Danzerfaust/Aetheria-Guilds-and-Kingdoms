# Capture Zone Commands

## Overview
Capture zones are specific areas within a node where players must stand to accumulate capture points during a node war. You can define multiple capture zones per node to create strategic capture point scenarios.

## Why Use Capture Zones?

1. **Strategic Gameplay**: Create multiple objectives within a node that guilds must control
2. **Focused Combat**: Direct player battles to specific locations instead of the entire node area
3. **Point Multipliers**: Make certain zones more valuable than others
4. **Defensive Positions**: Create capture points at specific strategic locations (e.g., castle towers, bridges)

## Commands

### Add Capture Zone
```
/nodewaradmin addzone {nodeId} {zoneId} {zoneName} {radius}
```
Adds a capture zone to a node at your current position.

**Parameters:**
- `nodeId`: The ID of the node to add the zone to
- `zoneId`: Unique identifier for the zone (e.g., "tower1", "bridge", "courtyard")
- `zoneName`: Display name for the zone (e.g., "North Tower", "Bridge", "Courtyard")
- `radius`: Radius of the capture zone in blocks

**Example:**
```
/nodewaradmin addzone castle tower1 "North Tower" 5
```

**Requirements:**
- You must be standing within the node's boundary
- The zone ID must be unique within that node

### Remove Capture Zone
```
/nodewaradmin removezone {nodeId} {zoneId}
```
Removes a capture zone from a node.

**Parameters:**
- `nodeId`: The ID of the node
- `zoneId`: The ID of the zone to remove

**Example:**
```
/nodewaradmin removezone castle tower1
```

### List Capture Zones
```
/nodewaradmin listzones {nodeId}
```
Lists all capture zones for a node.

**Parameters:**
- `nodeId`: The ID of the node

**Example:**
```
/nodewaradmin listzones castle
```

**Output includes:**
- Zone name and ID
- Location coordinates
- Radius
- Point multiplier
- Active status
- Distance from node center

## How Capture Zones Work

### During a Node War:

1. **No Capture Zones Defined**: If a node has no capture zones, the entire node area (defined by the node radius) acts as the capture zone.

2. **With Capture Zones**: If capture zones are defined, players must be inside one of the active capture zones to accumulate points.

3. **Point Multiplier**: Each zone has a point multiplier (default: 1.0x) that affects how quickly points are gained. Future updates may allow setting custom multipliers for strategic zones.

### Capture Zone Properties:

- **ZoneId**: Unique identifier (e.g., "tower1", "gate", "center")
- **ZoneName**: Display name shown to players
- **Center**: 3D position in the world
- **Radius**: How far from center the zone extends (in blocks)
- **PointMultiplier**: Multiplier for capture points (default: 1.0)
- **IsActive**: Whether the zone is currently active
- **Description**: Additional info about the zone

## Example Scenarios

### Castle Node with Multiple Capture Points
```bash
# Register the main castle node
/nodewaradmin register castle "Ancient Castle" 100

# Add capture zones at strategic locations
/nodewaradmin addzone castle tower1 "North Tower" 8
/nodewaradmin addzone castle tower2 "South Tower" 8
/nodewaradmin addzone castle keep "Main Keep" 10
/nodewaradmin addzone castle courtyard "Courtyard" 15
```

### Bridge Node with Single Capture Point
```bash
# Register the bridge node
/nodewaradmin register bridge "Stone Bridge" 50

# Add a focused capture zone on the bridge
/nodewaradmin addzone bridge bridgecenter "Bridge Center" 5
```

### Village Node with Multiple Objectives
```bash
# Register the village node
/nodewaradmin register village "Trading Village" 80

# Add zones at key buildings
/nodewaradmin addzone village market "Market Square" 10
/nodewaradmin addzone village townhall "Town Hall" 8
/nodewaradmin addzone village church "Church" 6
```

## Workflow

1. **Plan Your Layout**: Decide where capture points should be located
2. **Register the Node**: Create the main node zone first
3. **Navigate to Locations**: Walk to each strategic location
4. **Add Capture Zones**: Use `/nodewaradmin addzone` at each position
5. **Review**: Use `/nodewaradmin listzones` to verify
6. **Test**: Schedule and start a war to test the layout
7. **Adjust**: Add/remove zones as needed

## Tips

- **Smaller Zones = More Intense**: Smaller radius zones create more focused battles
- **Spacing**: Space zones far enough apart that guilds must split forces
- **Height Matters**: Place zones at different elevations for vertical gameplay
- **Node Boundary**: All capture zones must be within the node's main radius
- **Visual Markers**: Consider placing visual markers (torches, flags) at zone centers

## Future Enhancements

Potential features that could be added:
- Custom point multipliers per zone
- Zone activation/deactivation during wars
- Sequential capture (must capture zones in order)
- Zone ownership persistence between wars
- Visual zone boundaries for players
- Zone-specific buffs or debuffs
