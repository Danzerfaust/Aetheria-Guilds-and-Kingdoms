# Tech Blocks System - Quick Start Guide

## Overview

The tech blocks system provides a complete technology tree for guilds to progress through different ages, from Stone Age to Steel Age.

## Files Created

### Configuration & Data
- **`techblocks.json`** - Main configuration file with all tech definitions
- **`TECHBLOCKS_README.md`** - Detailed documentation on JSON format
- **`TechBlocksConfig.cs`** - Config loader with validation

### Core Classes
- **`TechBlock.cs`** - Tech definition with resource requirements
- **`GuildTechProgress.cs`** - Individual guild progress tracking
- **`GuildTechManager.cs`** - Manager for all guild tech data
- **`ResourceMatcher.cs`** - (Existing) Pattern matching for resources

### Examples
- **`GuildTechUsageExamples.cs`** - Usage examples for serialization
- **`TechBlockIntegrationExample.cs`** - Integration into ModSystem

## Quick Integration

### 1. Initialize in ModSystem

```csharp
public class SRGuildsAndKingdomsModSystem : ModSystem
{
    private TechBlocksConfig techConfig;
    private GuildTechManager techManager;
    
    public override void StartServerSide(ICoreServerAPI api)
    {
        // Load tech configuration
        techConfig = TechBlocksConfig.LoadFromFile(api);
        techConfig.Validate(api);
        
        // Initialize manager
        techManager = new GuildTechManager(api);
    }
    
    public override void Dispose()
    {
        techManager?.SaveAll();
    }
}
```

### 2. Player Contributes Resources

```csharp
public void OnPlayerContribute(string guildId, int techId, Dictionary<string, int> inventory)
{
    var tech = techConfig.TechBlocks.Find(t => t.Id == techId);
    var guildData = techManager.GetGuildTechData(guildId);
    var progress = guildData.GetOrCreateProgress(techId);
    
    if (tech.CanResearchWithItems(inventory, progress))
    {
        // Remove items from inventory
        // Submit resources
        techManager.SubmitResources(guildId, techId, resourcesToSubmit);
        
        // Check if complete
        if (AllRequirementsMet())
        {
            techManager.UnlockTech(guildId, techId);
        }
    }
}
```

### 3. Check Tech Availability

```csharp
public bool CanResearch(string guildId, int techId)
{
    var tech = techConfig.TechBlocks.Find(t => t.Id == techId);
    return techManager.HasPrerequisites(guildId, tech, techConfig.TechBlocks);
}
```

## Data Storage

### Config File Location
`VintageStoryData/ModConfig/SRGuildsAndKingdoms/techblocks.json`

Auto-created with defaults if missing.

### Guild Progress Location
`VintageStoryData/ModData/SRGuildsAndKingdoms/GuildTech/guild_{guildId}_tech.json`

Auto-created per guild as needed.

## Resource Requirement Patterns

| Pattern | Example | Matches |
|---------|---------|---------|
| Exact | `"stone-granite"` | Only granite stone |
| Wildcard | `"plank-*"` | Any plank type |
| Multiple | `"ore-tin,ore-zinc"` | Tin OR zinc ore |
| Combined | `"stone-*": 20, "clay-blue,clay-red": 15` | Mix of patterns |

## Tech Tree Structure

```
Level 1: Basic Tools (Stone Age)
    ??> Level 2: Advanced Stoneworking
    ?       ??> Level 3: Copper Working
    ??> Level 2: Primitive Shelter
            ??> Level 3: Agriculture
```

## Default Tech Ages

1. **Stone** - Basic survival (Level 1-2)
2. **Copper** - First metallurgy (Level 3)
3. **Bronze** - Alloy mastery (Level 4)
4. **Iron** - Advanced metals (Level 5)
5. **Steel** - Peak technology (Level 6)

## Customization

### Add New Tech

Edit `techblocks.json`:

```json
{
  "id": 12,
  "text": "New Technology",
  "description": "Description here",
  "level": 4,
  "age": "Bronze",
  "resourcesRequired": {
    "ingot-bronze": 50,
    "gear-*": 10
  },
  "unlocksIds": [13, 14]
}
```

### Reload Config

```csharp
techConfig = TechBlocksConfig.LoadFromFile(api);
techConfig.Validate(api);
```

## API Methods

### TechBlocksConfig
- `LoadFromFile(api)` - Load from JSON
- `SaveToFile(api)` - Save to JSON
- `Validate(api)` - Validate configuration
- `CreateDefaultConfig()` - Generate defaults

### GuildTechManager
- `GetGuildTechData(guildId)` - Get/load guild data
- `UnlockTech(guildId, techId)` - Mark tech unlocked
- `SubmitResources(guildId, techId, resources)` - Add resources
- `HasPrerequisites(guildId, tech, allTechs)` - Check prerequisites
- `SaveAll()` - Save all cached data

### GuildTechData
- `GetOrCreateProgress(techId)` - Get tech progress
- `IsTechUnlocked(techId)` - Check unlock status
- `SaveToFile(api)` - Save to disk
- `LoadFromFile(api, guildId)` - Load from disk

### TechBlock
- `CanResearchWithItems(inventory, progress)` - Check if researchable
- `IsAvailableForGuild(guildData, allTechs)` - Check prerequisites
- `ValidateResourceRequirements()` - Validate patterns

## Performance Notes

- **Caching**: GuildTechManager caches loaded data
- **Lazy Loading**: Guild data loaded on first access
- **Batch Save**: Use `SaveAll()` on shutdown
- **Validation**: Done once on config load

## Troubleshooting

### Config not loading?
- Check file path: `ModConfig/SRGuildsAndKingdoms/techblocks.json`
- Check JSON syntax
- Look for validation errors in logs

### Guild progress not saving?
- Call `techManager.SaveAll()` on shutdown
- Check write permissions
- Verify guildId is set correctly

### Resources not matching?
- Check pattern syntax (wildcards, commas)
- Use `ResourceMatcher.IsValidResourceRequirement()`
- Test with `ResourceMatcher.GetMatchingItems()`

## Next Steps

1. **Integrate into UI** - Display tech tree in guild interface
2. **Add Permissions** - Control who can contribute resources
3. **Add Events** - Trigger effects when techs unlock
4. **Balance Resources** - Adjust costs in `techblocks.json`
5. **Add More Techs** - Expand the tech tree

## Example Tech Tree (Default)

```
Stone Age (1-2):
  1. Basic Tools ? 2. Advanced Stoneworking, 3. Primitive Shelter

Copper Age (3):
  4. Copper Working (from 2)
  5. Agriculture (from 3)

Bronze Age (4):
  6. Bronze Metallurgy (from 4)
  7. Advanced Agriculture (from 5)

Iron Age (5):
  8. Iron Smelting (from 6)
  9. Irrigation Systems (from 7)

Steel Age (6):
  10. Steel Production (from 8)
  11. Mechanical Engineering (from 9)
```

## Support

See full documentation in `TECHBLOCKS_README.md`

Check example code in:
- `GuildTechUsageExamples.cs`
- `TechBlockIntegrationExample.cs`
