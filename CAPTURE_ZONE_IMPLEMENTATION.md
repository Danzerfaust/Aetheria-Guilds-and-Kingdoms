# Capture Zone System Implementation

## Overview
This implementation adds a comprehensive capture zone system for node wars. The system automatically tracks players in capture zones, accumulates points for their guilds, and determines victory conditions when a guild reaches the required capture points.

## Key Components

### 1. CaptureZoneSystem.cs (NEW)
A new class that handles all capture zone mechanics:

**Core Features:**
- **Automatic Player Tracking**: Continuously monitors all players participating in node wars
- **Zone Detection**: Checks if players are inside capture zones every second
- **Point Accumulation**: Awards points to guilds based on:
  - Number of players in the zone (minimum required)
  - Extra player bonuses (diminishing returns)
  - Contested status (reduced rate when multiple guilds are present)
  - Kill/death bonuses
- **Real-time Notifications**:
  - Players receive messages when entering/leaving capture zones
  - Periodic status updates every 5 seconds showing:
    - Leading guild and their progress
    - Number of players in zone
    - Contested vs. Capturing status
- **Victory Detection**: Automatically ends wars when a guild reaches the required points

**Technical Details:**
- Runs on a 1-second tick interval
- Maintains a dictionary tracking players in each active war zone
- Cleans up tracking data when wars end
- Sends appropriate notifications to all participants

### 2. NodeWarManager.cs (MODIFIED)
Updated to integrate with the capture zone system:

**Changes:**
- Added `CaptureZoneSystem` field and initialization
- New `Initialize()` method to start the capture zone system
- New `Shutdown()` method to cleanup resources
- New `GetCaptureZoneStatus()` method to query current status
- Deprecated old `CheckPlayersInZone()` method (now handled automatically)
- `UpdateCaptureProgress()` is now called by CaptureZoneSystem automatically

### 3. PVPManager.cs (MODIFIED)
Updated to properly initialize and dispose of the node war system:

**Changes:**
- Calls `nodeWarManager.Initialize()` in constructor
- New `Dispose()` method to cleanup on shutdown
- Ensures capture zone system starts with the server

### 4. PVPModSystem.cs (MODIFIED)
Updated the main mod system to handle cleanup:

**Changes:**
- Updated `Dispose()` override to call `pvpManager.Dispose()`
- Ensures proper cleanup when the server shuts down

### 5. NodeWarCommands.cs (MODIFIED)
Added a new admin command to view capture zone status:

**New Command:**
```
/nodewaradmin status [nodeId]
```
Shows detailed information about an active capture zone including:
- Guild rankings with capture points
- Number of players per guild
- Percentage progress toward victory
- Time elapsed and remaining
- Contested status
- Warnings if guilds don't have minimum players

## How It Works

### During a Node War:

1. **War Starts**: When an admin starts a war with `/nodewaradmin start [nodeId]`

2. **Players Join**: Guild members join the war (via the Guild UI)

3. **Automatic Tracking**: The CaptureZoneSystem automatically:
   - Checks every second which players are in the capture zone
   - Updates guild progress counts
   - Calculates and awards capture points based on:
     - Base rate: 1 point per second (configurable)
     - Player count bonus: Extra players beyond minimum
     - Contested penalty: 50% reduction if multiple guilds present
     - Kill/death bonuses: +50 per kill, -10 per death

4. **Player Notifications**:
   - Entry: "⚔ Entered capture zone: [Node Name]"
   - Exit: "Left capture zone: [Node Name]"
   - Status (every 5s): "🏴 [Node] - [Guild]: 45.3% (3 players) [CAPTURING]"

5. **Victory**: When a guild reaches the required points (default: 1000):
   - War automatically ends
   - Guild captures the node
   - All players notified: "🏁 Node war at [Node] has ended"

### Configuration (NodeWarConfig.cs)
All parameters are configurable per war:
- `MinPlayersToCapture`: Minimum players needed (default: 3)
- `CapturePointsNeeded`: Points to win (default: 1000)
- `PointsPerSecondBase`: Base capture rate (default: 1.0)
- `PointsPerKill`: Bonus for kills (default: 50)
- `PointsPerDeath`: Penalty for deaths (default: -10)
- `ExtraPlayerBonus`: Bonus per extra player (default: 0.1 = 10%)
- `ContestedMultiplier`: Rate when contested (default: 0.5 = 50%)
- `MaxDurationSeconds`: Time limit (default: 3600 = 1 hour)

## Testing & Monitoring

### Admin Commands for Testing:
```bash
# Schedule a war
/nodewaradmin schedule mynode 0.5

# Start immediately
/nodewaradmin start mynode

# View live status
/nodewaradmin status mynode

# Force end
/nodewaradmin end mynode [winnerGuild]
```

### Status Command Output Example:
```
=== Capture Zone Status: Ancient Ruins ===
Node ID: mynode
Status: ⚔ CONTESTED
Total Players in Zone: 8
Required Points to Win: 1000

Guild Progress:
🥇 RedGuild
   Points: 456.2/1000 (45.6%)
   Players in Zone: 5

🥈 BlueGuild
   Points: 389.7/1000 (38.9%)
   Players in Zone: 3

War Duration: 15m 23s
Time Remaining: 44m 37s
```

## Benefits

1. **Fully Automated**: No manual intervention needed - system handles everything
2. **Real-time Feedback**: Players always know what's happening
3. **Fair Mechanics**: Balanced point system with diminishing returns
4. **Contested Battles**: Multiple guilds can fight for the same zone
5. **Admin Visibility**: Detailed status command for monitoring
6. **Performance Optimized**: 1-second tick interval is efficient
7. **Clean Architecture**: Separate concerns with dedicated system class

## Future Enhancements (Optional)

Potential improvements that could be added:
- Visual boundaries showing capture zone radius
- HUD overlay with progress bars
- Sound effects for zone entry/capture events
- Customizable point formulas per node
- Capture zone "ownership" decay over time
- Multiple capture points per node
- Special abilities/buffs inside zones
