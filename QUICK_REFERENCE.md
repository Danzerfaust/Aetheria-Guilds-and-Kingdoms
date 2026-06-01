# Protected Zones - Quick Reference Guide

## Quick Setup (3 Steps)

1. **Edit Config File**
   - Location: `[server]/ModData/SRGuildsAndKingdoms/guild-config.json`
   - Set `enableProtectedZones`: `true`
   - Add zones to `protectedZones` array

2. **Define Zones**
   ```json
   "protectedZones": [
     {
       "name": "Spawn Area",
       "x": 0,
       "z": 0,
       "radius": 250
     }
   ]
   ```

3. **Restart Server**
   - Zones are loaded on server start
   - Check console for confirmation logs

## Finding Coordinates

### Method 1: F3 Debug Screen
- Press F3 in-game
- Look for "Block Position" or "XYZ"
- Use X and Z values (ignore Y)

### Method 2: TP Command
- Type: `/tp [show]`
- Note the X and Z coordinates
- Use these in config

### Method 3: World Map
- Open map (M key)
- Mouse over location
- Coordinates shown at bottom

## Common Zone Sizes

| Purpose | Recommended Radius |
|---------|-------------------|
| Single building | 50-75 blocks |
| Small monument | 75-100 blocks |
| Market/Shop area | 100-150 blocks |
| Community area | 150-200 blocks |
| Spawn protection | 200-300 blocks |
| Major hub/city | 300-500 blocks |

## What's Protected

? **Prevented**:
- Land claiming by ANY guild
- Block breaking by ANY player
- Block placing by ANY player

? **Allowed**:
- Opening doors
- Using chests
- Activating buttons/levers
- Using crafting stations
- All other interactions

## Visual Guide

### On World Map:
- **Purple overlay** = Protected zone
- **Purple border** = Zone boundary
- **Red hover** = Cannot claim here

### In claiming mode:
- Hover over protected zone = Red highlight
- Attempt to claim = Error message with zone name

## Troubleshooting

### Zones Not Working?
1. Check `enableProtectedZones` is `true`
2. Verify zones array has entries
3. Restart server after config changes
4. Check server console for errors

### Can't See Zones on Map?
1. Zoom in more (zones only render when large enough)
2. Check coordinates are correct
3. Verify zones are in visible map area

### Players Can Still Break Blocks?
1. Check server console for load errors
2. Verify coordinates are correct (block coords, not chunk)
3. Make sure radius is large enough
4. Restart server after config changes

## Example Configurations

### Minimal (Spawn Only)
```json
{
  "enableProtectedZones": true,
  "protectedZones": [
    {
      "name": "Spawn",
      "x": 0,
      "z": 0,
      "radius": 250
    }
  ]
}
```

### Standard (Spawn + Market)
```json
{
  "enableProtectedZones": true,
  "protectedZones": [
    {
      "name": "Spawn Area",
      "x": 0,
      "z": 0,
      "radius": 250
    },
    {
      "name": "Market",
      "x": 500,
      "z": -300,
      "radius": 150
    }
  ]
}
```

### Advanced (Multiple Zones)
```json
{
  "enableProtectedZones": true,
  "protectedZones": [
    {
      "name": "Spawn Area",
      "x": 0,
      "z": 0,
      "radius": 300
    },
    {
      "name": "Market District",
      "x": 600,
      "z": -400,
      "radius": 150
    },
    {
      "name": "Arena",
      "x": -800,
      "z": 200,
      "radius": 200
    },
    {
      "name": "Community Farm",
      "x": 300,
      "z": 800,
      "radius": 100
    },
    {
      "name": "Ancient Ruins",
      "x": 1200,
      "z": -900,
      "radius": 120
    }
  ]
}
```

## Important Notes

?? **Coordinates**:
- Use BLOCK coordinates (not chunk coordinates)
- 1 chunk = 32 blocks
- Center of map is usually (0, 0)

?? **Radius**:
- Minimum: 50 blocks (enforced automatically)
- Measured from center point
- Creates circular protected area

?? **Changes**:
- Config changes require server restart
- No in-game commands yet (future feature)
- Changes logged to server console

?? **Compatibility**:
- Works WITH territorial restrictions
- Works WITH guild claims (prevents overlap)
- Works WITH all mod features

## Admin Commands

Currently, there are no in-game commands for protected zones. Configuration is done through the guild-config.json file. This may be added in a future update.

## Support

For issues or questions:
1. Check server console logs
2. Review the PROTECTED_ZONES.md documentation
3. Check INTEGRATION_SUMMARY.md for technical details
4. Verify configuration syntax (JSON must be valid)

## Version Compatibility

- Requires: SRGuilds & Kingdoms v1.0+
- Minecraft: 1.19+
- Vintage Story: 1.19+

---

**Quick Tip**: Start with just spawn protection, then add more zones as needed. It's easier to add zones than to remove them after players have built around them!
