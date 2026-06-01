# Age-Restricted Mining Implementation Summary

## What Was Implemented

A complete system to restrict block mining based on tech ages configured in your world. Players cannot mine certain ores until the corresponding age is enabled.

## Files Created

1. **`BlockBehaviorAgeRestricted.cs`** - Block behavior that prevents mining age-locked blocks
2. **`AgeRestrictedBlocksConfig.cs`** - Configuration for age-restricted blocks with wildcard support
3. **`AgeRestrictionExamples.cs`** - Example code showing how to use the system
4. **`AGE_RESTRICTED_MINING.md`** - Complete documentation

## Files Modified

1. **`SRGuildsAndKingdomsModSystem.cs`** 
   - Added registration of `BlockBehaviorAgeRestricted`
   - Added `ApplyAgeRestrictionsToBlocks()` method
   - Applies restrictions dynamically at runtime

2. **`TechBlocksConfig.cs`**
   - Added `AgeRestrictedBlocks` property
   - Now loads/saves age restriction configuration

## How It Works

### Configuration
Edit `ModConfig/SRGuildsAndKingdoms/techblocks.json`:

```json
{
  "enabledAges": ["Stone", "Copper"],
  "ageRestrictedBlocks": {
    "blockAgeRestrictions": {
      "game:ore-*-copper-*": "Copper",
      "game:ore-*-iron-*": "Iron",
      "game:ore-*-gold-*": "Steel"
    }
  }
}
```

### Available Ages
- **Stone** - Basic resources
- **Copper** - Copper ores
- **Bronze** - Bronze-related ores
- **OtherBronze** - Alternative bronze materials
- **Iron** - Iron ores
- **MetoricIron** - Meteoric iron
- **Steel** - Advanced metals
- **MeteoricSteel** - Rare/advanced resources

### Wildcard Patterns
- `game:ore-*-copper-*` - Matches ALL copper ore variants (poor, medium, rich, granite, chalk, etc.)
- `game:ore-poor-copper-*` - Matches only poor copper ore in any stone type
- `game:ore-*-copper-granite` - Matches all copper ore grades in granite only

## Player Experience

When a player tries to mine a restricted ore:
1. Mining doesn't progress
2. Shows message: "This ore cannot be mined until the [Age] Age is enabled."
3. Ore remains unmined

When the age is unlocked:
- Ore mining works normally
- No special tools or permissions needed
- All guild members can access the ore

## Testing

### Test Case 1: Block Restricted Ore
1. Set `enabledAges` to `["Stone"]`
2. Restart server
3. Try to mine copper ore
4. **Expected**: Mining blocked with message

### Test Case 2: Unlock Age
1. Add `"Copper"` to `enabledAges`
2. Restart server
3. Try to mine copper ore
4. **Expected**: Mining works normally

### Test Case 3: Wildcard Matching
1. Add pattern `"game:ore-*-iron-*": "Iron"`
2. Enable only `["Stone", "Copper"]`
3. Try to mine ANY iron ore variant
4. **Expected**: All iron ores blocked

## Configuration Examples

### Relaxed Progression
```json
{
  "enabledAges": ["Stone", "Copper", "Bronze", "Iron", "Steel"]
}
```
Most resources available, good for casual servers.

### Strict Progression
```json
{
  "enabledAges": ["Stone"]
}
```
Only basic resources, forces tech progression.

### Custom Restrictions
```json
{
  "ageRestrictedBlocks": {
    "blockAgeRestrictions": {
      "game:ore-*-copper-*": "Stone",      // Copper available from start
      "game:ore-*-iron-*": "Bronze",       // Iron requires bronze age
      "game:ore-*-gold-*": "MeteoricSteel" // Gold is end-game
    }
  }
}
```

## Integration with Tech Tree

The system automatically integrates with your tech tree:
1. Players research technologies through the GUI
2. Techs unlock new ages
3. Server admin enables those ages in config
4. Ores become available to mine

### Automatic Age Unlocking (Future Enhancement)
Currently ages must be manually enabled. Future update could:
- Automatically enable ages when corresponding tech is researched
- Allow per-guild age progression
- Send notifications when new ages are unlocked

## Performance

- Restrictions applied once at world load
- O(1) lookup during mining attempts
- No performance impact on gameplay
- Works with thousands of blocks

## Troubleshooting

**Q: Blocks aren't being restricted**
- Check mod logs for "Applied age restrictions to X blocks"
- Verify wildcard patterns match block codes (use `/debug` in-game to see block codes)
- Ensure ages are not listed in `enabledAges`

**Q: Players still mine restricted blocks**
- Both client and server need the updated mod
- Restart server after config changes
- Check that block behavior is registered in logs

**Q: How do I find block codes?**
- Enable creative mode
- Look at block with `/debug` command
- Block code appears in the UI (e.g., `game:ore-poor-copper-granite`)

## Next Steps

1. ? Configure initial age restrictions
2. ? Test with a few ores
3. ? Set up tech tree to unlock ages
4. ? Plan age progression for your server
5. ? Add custom mod blocks if needed

## Quick Reference

| Action | Command/Location |
|--------|-----------------|
| Edit restrictions | `ModConfig/SRGuildsAndKingdoms/techblocks.json` |
| Enable age | Add to `enabledAges` array |
| Add block restriction | Add to `blockAgeRestrictions` dictionary |
| Check logs | Look for "Applied age restrictions" message |
| Test restriction | Try mining with only Stone age enabled |

