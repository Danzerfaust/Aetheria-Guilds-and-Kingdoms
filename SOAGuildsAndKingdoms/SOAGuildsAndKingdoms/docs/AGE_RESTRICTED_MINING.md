# Age-Restricted Mining System

## Overview

This system allows you to restrict certain blocks (primarily ores) from being mined until specific tech ages are enabled in the world. This creates a progression system where players must unlock ages through technology research before gaining access to advanced resources.

## How It Works

### 1. Block Behavior System

The system uses a custom `BlockBehavior` called `BlockBehaviorAgeRestricted` that:
- Checks if a block's required age is enabled before allowing interaction
- Prevents mining, breaking, and interacting with age-restricted blocks
- Shows a helpful message to players explaining why they can't mine the block

### 2. Configuration

Age restrictions are defined in the `techblocks.json` configuration file under the `ageRestrictedBlocks` section:

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

### 3. Wildcard Matching

The system supports wildcard patterns using `*` to match multiple blocks:
- `game:ore-*-copper-*` matches all copper ore variants (poor, medium, rich, etc.)
- `game:ore-*-iron-*` matches all iron ore variants
- `game:clay-*` matches all clay types

### 4. Default Configuration

The system ships with a default configuration that restricts common ores:

| Age | Restricted Resources |
|-----|---------------------|
| **Stone** | Basic resources (rock, clay) |
| **Copper** | Copper ores, tin ore, cassiterite |
| **Bronze** | Tin, bismuth, zinc ores |
| **Iron** | Iron ores, limonite, ilmenite |
| **Steel** | Gold, silver, lead ores |
| **Modern** | Uranium, chromium, titanium ores |
| **Future** | Platinum and exotic materials |

## Configuration Guide

### Enabling/Disabling Ages

Ages are controlled in the `techblocks.json` file:

```json
{
  "enabledAges": [
    "Stone",
    "Copper",
    "Bronze"
  ]
}
```

Only ages listed in `enabledAges` will be accessible. When a new age is unlocked through research, add it to this list.

### Adding Custom Block Restrictions

To restrict a custom block or mod block:

```json
{
  "ageRestrictedBlocks": {
    "blockAgeRestrictions": {
      "yourmod:customore-*": "Iron",
      "game:block-diamond": "Modern"
    }
  }
}
```

### Removing Restrictions

To allow unrestricted mining of certain blocks, simply remove them from the `blockAgeRestrictions` dictionary or delete the entire `ageRestrictedBlocks` section.

## Implementation Details

### Files Added

1. **`BlockBehaviorAgeRestricted.cs`**
   - Main behavior that checks age restrictions
   - Hooks into block interaction, breaking, and mining events
   - Displays user-friendly messages

2. **`AgeRestrictedBlocksConfig.cs`**
   - Configuration class for age-restricted blocks
   - Provides default configuration
   - Handles wildcard matching

3. **`TechBlocksConfig.cs` (Updated)**
   - Added `AgeRestrictedBlocks` property
   - Loads and saves age restrictions with tech blocks config

4. **`SOAGuildsAndKingdomsModSystem.cs` (Updated)**
   - Registers the block behavior
   - Applies age restrictions to blocks at runtime
   - Integrates with tech age system

### Runtime Application

The system applies age restrictions dynamically when the game loads:

```csharp
// In SOAGuildsAndKingdomsModSystem.Start()
api.RegisterBlockBehaviorClass("AgeRestricted", typeof(BlockBehaviorAgeRestricted));

// After world loads, apply restrictions to matching blocks
ApplyAgeRestrictionsToBlocks(api);
```

This approach allows:
- No need to modify game JSON files
- Works with any mod's blocks
- Can be updated without restarting the server

## Player Experience

### When Mining a Restricted Block

1. Player attempts to mine copper ore when only Stone age is enabled
2. The block behavior intercepts the interaction
3. Player sees message: "This ore cannot be mined until the Copper Age is enabled."
4. Mining does not progress

### When Age is Unlocked

1. Guild completes copper age technology research
2. Server admin enables Copper age in config or through tech system
3. Players can now mine copper ore normally
4. Visual indicators in tech tree show age is unlocked

## Server Administration

### Manually Enabling an Age

Edit `ModConfig/SOAGuildsAndKingdoms/techblocks.json`:

```json
{
  "enabledAges": [
    "Stone",
    "Copper",  // <- Add new age here
    "Bronze"   // <- Or this one
  ]
}
```

Then reload the config or restart the server.

### Checking Current Configuration

The mod logs the current configuration on startup:

```
[Notification] Loaded 15 tech blocks from configuration
[Notification] Enabled tech ages: Stone, Copper
[Notification] Applied age restrictions to 127 blocks
```

### Troubleshooting

**Issue**: Blocks aren't being restricted
- Check that the block code pattern matches in `blockAgeRestrictions`
- Verify the age is not in `enabledAges`
- Check server logs for "Applied age restrictions to X blocks"

**Issue**: Wrong blocks are being restricted
- Review your wildcard patterns
- Use more specific patterns like `game:ore-poor-copper-*` instead of `game:ore-*`

**Issue**: Players can still mine restricted blocks
- Ensure both client and server have the updated mod
- Restart the server to apply configuration changes
- Check that tech age is not accidentally enabled

## Integration with Tech Tree

This system integrates seamlessly with the guild research system:

1. Players research technologies in the tech tree
2. When a tech unlocks a new age, it can trigger age enablement
3. Ores and resources become available as ages unlock
4. Creates meaningful progression tied to guild cooperation

## Future Enhancements

Possible additions:
- Dynamic age enablement when tech is unlocked (automatic)
- Tool tier requirements tied to ages
- Crafting recipe restrictions by age
- Building restrictions by age
- Per-guild age progression (different guilds at different ages)

## Example Workflow

1. **World Start**: Only Stone Age enabled
   - Players can gather basic resources
   - Stone, flint, sticks, clay available

2. **Copper Research**: Guild researches copper technologies
   - Copper Age becomes enabled
   - Copper and tin ores become mineable
   - Bronze tools can be crafted

3. **Iron Research**: Guild advances to iron age
   - Iron Age becomes enabled
   - Iron ores become mineable
   - Steel production unlocked

4. **Continued Progression**: Each age unlocks new resources and possibilities

## Technical Notes

### Performance

- Block behaviors are applied once at world load
- Checking age restrictions is very fast (O(1) lookup)
- No performance impact during normal gameplay

### Compatibility

- Works with any mod that adds blocks
- Uses Vintage Story's standard block behavior system
- No conflicts with other mods

### Multiplayer

- Age restrictions are enforced server-side
- All players see consistent restrictions
- Configuration is server-controlled

