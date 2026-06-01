# Node Wars UI Integration Summary

## Overview
Successfully integrated Node Wars functionality into the Guild UI, removing chat commands in favor of a dedicated GUI tab.

## Changes Made

### 1. **Removed Player Commands** (`SRGuildsAndKingdomsPVP/src/nodewars/NodeWarCommands.cs`)
- Removed all player-facing `/nodewar` commands:
  - `/nodewar signup <nodeId>` - Sign up for a war
  - `/nodewar cancel <nodeId>` - Cancel signup
  - `/nodewar join` - Join active war
  - `/nodewar leave` - Leave war
  - `/nodewar status [nodeId]` - View war status
  - `/nodewar list` - List all wars
  - `/nodewar nodes` - List node zones
  - `/nodewar guildstatus` - View guild's war status

- **Retained Admin Commands** (still accessible via chat):
  - `/nodewaradmin register <nodeId> <nodeName> <radius>` - Register a node zone
  - `/nodewaradmin schedule <nodeId> <hoursFromNow>` - Schedule a war
  - `/nodewaradmin start <nodeId>` - Start a scheduled war
  - `/nodewaradmin end <nodeId> [winnerGuild]` - End a war
  - `/nodewaradmin cancel <nodeId>` - Cancel a scheduled war
  - `/nodewaradmin signups <nodeId>` - View signups for a war

### 2. **Added Node Wars Tab to Guild Menu** (`SRGuildsAndKingdoms/src/gui/DialogGuildMain.cs`)
- Added `TAB_NODEWARS` constant (index 4)
- Renumbered existing tabs:
  - Overview: 0
  - Members: 1
  - Lands: 2
  - Research: 3
  - **Node Wars: 4** ← NEW
  - Quests: 5 (was 5)
  - Settings: 6 (was 4)

- Added `nodeWarsTab` field and initialization
- Added tab button "Node Wars" in the UI
- Implemented tab content rendering

### 3. **Implemented Node Wars Action Handlers** (`SRGuildsAndKingdoms/src/gui/DialogGuildMain.cs`)
Added new action methods:
- `OnNodeWarSignup()` - Handle war signup (placeholder for network integration)
- `OnNodeWarCancelSignup()` - Handle canceling signup
- `OnNodeWarJoin()` - Handle joining active war
- `OnNodeWarViewDetails()` - Show detailed war information

Added helper method:
- `RefreshNodeWarsData()` - Refresh Node Wars data from PVP mod (placeholder)

### 4. **Added Helper Methods to GuildNodeWarsTab** (`SRGuildsAndKingdoms/src/gui/tabs/GuildNodeWarsTab.cs`)
- `GetSelectedWarForSignup()` - Get the selected war ID
- `GetCurrentSignup()` - Get current signup information
- `GetCurrentWar()` - Get current war information

### 5. **Created Data Provider** (`SRGuildsAndKingdomsPVP/src/nodewars/NodeWarDataProvider.cs`)
New class that bridges PVP mod data to Guild UI:
- `GetNodeWarDataForGuild(string guildName)` - Retrieves all node war data for a guild
  - Controlled nodes
  - Active wars
  - Available wars for signup
  - Current signup status

- `ConvertGameTimeToDateTime(double gameTimeHours)` - Converts game time to DateTime for UI

Includes Data Transfer Objects (DTOs):
- `NodeWarTabData` - Container for all node war UI data
- `ControlledNodeInfo` - Information about controlled nodes
- `CurrentWarInfo` - Active war information
- `GuildWarProgressInfo` - Guild's progress in a war
- `AvailableWarInfo` - Wars available for signup
- `CurrentSignupInfo` - Current signup status

### 6. **Updated PVP Manager** (`SRGuildsAndKingdomsPVP/src/pvp/PVPManager.cs`)
- Added `NodeWarManager` property
- Initialized `NodeWarManager` in constructor
- Added using directive for `SRGuildsAndKingdomsPVP.src.nodewars`

### 7. **Exposed Node War Data in PVP Mod System** (`SRGuildsAndKingdomsPVP/PVPModSystem.cs`)
Added public accessor:
- `GetNodeWarDataForGuild(string guildName)` - Allows Guild mod to retrieve node war data
- `GetPVPManager()` - Provides access to PVP manager

## UI Flow

### Player Experience
1. Player opens Guild menu (existing hotkey/command)
2. Clicks on "Node Wars" tab
3. Sees:
   - **Controlled Nodes** - Nodes their guild owns
   - **Current War** - Active war their guild is participating in (if any)
     - Guild's progress (capture points, players in zone, K/D)
     - "Join War" button (if war is active)
     - "View Details" button
   - **Signed Up For** - War their guild has signed up for (if any)
     - War start time
     - "Cancel Signup" button (leaders only)
   - **Available Wars** - Wars available for signup
     - "Sign Up" button for each war (leaders only)

### Guild Leader Actions
- **Sign Up for War**: Click "Sign Up" next to available war
- **Cancel Signup**: Click "Cancel Signup" if already signed up
- **Join War**: Once war starts, click "Join War" to participate

### All Members Actions
- **Join War**: Join their guild's active war
- **View Details**: See detailed war information

## Technical Notes

### Current Limitations (TODOs)
1. **Network Integration**: The action handlers currently show messages but don't send network packets to the server
   - Need to implement network packets for:
     - Guild signup requests
     - Signup cancellation
     - Player joining war
     - Requesting war details

2. **Data Refresh**: `RefreshNodeWarsData()` is a placeholder
   - Client-side doesn't have direct access to server-side PVP mod
   - Need to implement network packet to request data from server
   - Server responds with `NodeWarTabData` which is set via `SetNodeWarData()`

3. **Time Conversion**: Currently using placeholders for scheduled war times
   - Need to properly convert `NodeWar.StartTime` (double) to DateTime
   - Need to track/store scheduled start times

4. **SignalR/Real-time Updates**: Consider implementing real-time updates
   - War status changes
   - Guild progress updates
   - New wars becoming available

### Data Flow
```
Server Side (PVP Mod):
  NodeWarManager → NodeWarDataProvider → GetNodeWarDataForGuild()
                                              ↓
Client Side (Guild Mod):                Network Packet (TODO)
  DialogGuildMain → RefreshNodeWarsData() ← (receives NodeWarTabData)
                           ↓
                  nodeWarsTab.SetNodeWarData(data)
                           ↓
                  GuildNodeWarsTab renders UI
```

### Integration Points
- **PVP Mod Exposes**: `PVPModSystem.GetNodeWarDataForGuild(guildName)`
- **Guild Mod Consumes**: Via network packet (to be implemented)
- **Data Structure**: `NodeWarTabData` (defined in both mods for consistency)

## Testing Recommendations

1. **Without PVP Mod**: Verify tab shows "PVP mod not available" message
2. **With PVP Mod**: 
   - Verify controlled nodes display correctly
   - Verify available wars list
   - Verify signup/cancel buttons (leaders only)
   - Verify active war display with progress
   - Verify "Join War" functionality
3. **Permissions**: Verify only guild leaders can sign up/cancel
4. **Edge Cases**:
   - Guild with no nodes
   - Guild not in any wars
   - Multiple available wars
   - War ending while viewing tab

## Future Enhancements

1. **Detailed War View Dialog**: Implement `DialogNodeWarDetails` for comprehensive war information
2. **Live Progress Updates**: Real-time capture point updates during active wars
3. **War History**: Show past wars and results
4. **Leaderboard**: Display guild rankings in wars
5. **Notifications**: Alert players when wars start, when guild signs up, etc.
6. **War Scheduling Calendar**: Visual calendar showing upcoming scheduled wars
7. **Node Map Integration**: Click on controlled nodes to view on world map

## Migration Notes

For server administrators:
- Existing admin commands remain unchanged
- Players will need to use Guild UI instead of chat commands
- No database migration required
- No config changes required
