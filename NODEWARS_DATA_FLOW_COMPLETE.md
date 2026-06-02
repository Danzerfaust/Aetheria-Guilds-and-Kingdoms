# Node Wars Data Flow - Complete Implementation

## Overview
This document describes the complete network communication flow for displaying Node Wars data in the Guild UI, integrating the PVP mod with the Guild mod.

## Architecture

### Data Flow Sequence

1. **Client Request** (DialogGuildMain)
   - User opens Guild UI and switches to Node Wars tab
   - `RefreshNodeWarsData()` is called
   - Sends `NodeWarDataRequestPacket` to server via `GuildNetworkHandler.RequestNodeWarData()`

2. **Server-Side Routing** (GuildNetworkHandler)
   - Server receives `NodeWarDataRequestPacket`
   - Handler `OnNodeWarDataRequestReceived()` fires custom event "nodewar_data_request"
   - Event includes player and guild name

3. **PVP Mod Processing** (PVPModSystem)
   - PVP mod listens for "nodewar_data_request" event
   - `OnNodeWarDataRequested()` handler is called
   - Creates `NodeWarDataProvider` instance
   - Calls `GetNodeWarDataForGuild()` to gather data from NodeWarManager

4. **Data Conversion** (NodeWarDataProvider)
   - Collects controlled nodes, active wars, available wars, and current signup
   - Converts to `NodeWarTabData` domain model
   - Calls `ConvertToNetworkPacket()` to create `NodeWarDataResponsePacket`
   - DTOs include long timestamps (DateTime.Ticks) for serialization

5. **Server Response** (GuildNetworkHandler)
   - PVP mod calls `GuildNetworkHandler.SendNodeWarData()`
   - Sends `NodeWarDataResponsePacket` to requesting client

6. **Client Update** (DialogGuildMain)
   - Client receives packet via `OnNodeWarDataReceived()`
   - Converts DTOs back to UI data model (`NodeWarTabData`)
   - Calls `nodeWarsTab.SetNodeWarData()` to update tab
   - Refreshes dialog if currently on Node Wars tab

## Key Components

### Guild Mod (SOAGuildsAndKingdoms)

#### Network Packets (GuildPackets.cs)
```csharp
- NodeWarDataRequestPacket      // Client → Server
- NodeWarDataResponsePacket     // Server → Client  
- ControlledNodeDto             // Network DTO
- CurrentWarDto                 // Network DTO
- GuildWarProgressDto           // Network DTO
- AvailableWarDto               // Network DTO
- CurrentSignupDto              // Network DTO
```

#### Network Handler (GuildNetworkHandler.cs)
```csharp
// Client-side
- RequestNodeWarData(guildName)              // Send request
- OnNodeWarDataResponseReceived(packet)      // Handle response
- RegisterNodeWarDataCallback(callback)      // Register UI callback

// Server-side
- OnNodeWarDataRequestReceived(player, packet) // Handle request, fire event
- SendNodeWarData(player, packet)             // Send response to client
```

#### UI Components
- **DialogGuildMain.cs**
  - `RefreshNodeWarsData()` - Requests data from server
  - `OnNodeWarDataReceived()` - Handles response and updates UI
  - Registers callback in constructor

- **GuildNodeWarsTab.cs**
  - `SetNodeWarData(data)` - Updates tab with new data
  - Displays controlled nodes, active wars, available wars
  - Shows signup status and action buttons

### PVP Mod (SOAGuildsAndKingdomsPVP)

#### Data Provider (NodeWarDataProvider.cs)
```csharp
- GetNodeWarDataForGuild(guildName)    // Gather data from NodeWarManager
- ConvertToNetworkPacket(data)         // Convert to network DTOs
```

#### Mod System (PVPModSystem.cs)
```csharp
- StartServerSide()                     // Register event listener
- OnNodeWarDataRequested()              // Handle data request
```

## Data Structures

### Domain Models (UI Layer)
```csharp
NodeWarTabData
├── ControlledNodes: List<ControlledNodeInfo>
├── CurrentWar: CurrentWarInfo
├── AvailableWars: List<AvailableWarInfo>
└── CurrentSignup: CurrentSignupInfo
```

### Network DTOs
```csharp
NodeWarDataResponsePacket
├── ControlledNodes: List<ControlledNodeDto>
│   ├── NodeId: string
│   ├── NodeName: string
│   ├── CapturedAtTicks: long           // DateTime → long
│   └── InfluencePerDay: int
├── CurrentWar: CurrentWarDto
│   ├── NodeId, NodeName, Status
│   ├── PointsNeeded: double
│   └── YourGuildProgress: GuildWarProgressDto
├── AvailableWars: List<AvailableWarDto>
│   ├── NodeId, NodeName
│   ├── WarStartTimeTicks: long        // DateTime → long
│   ├── CurrentSignups, MaxGuilds
│   └── CanSignup: bool
└── CurrentSignup: CurrentSignupDto
    ├── NodeId, NodeName
    ├── SignupTimeTicks: long          // DateTime → long
    └── WarStartTimeTicks: long        // DateTime → long
```

## Event Communication

### Custom Event: "nodewar_data_request"

**Purpose**: Allows Guild mod to request data from PVP mod without direct coupling

**Event Data**:
```csharp
NodeWarDataRequest
├── Player: IServerPlayer
└── GuildName: string
```

**Registration** (PVPModSystem.cs):
```csharp
api.Event.OnPlayerEvent("nodewar_data_request", (eventName, eventData) =>
{
    if (eventData is NodeWarDataRequest request)
    {
        OnNodeWarDataRequested(request.Player, request.GuildName, guildMod);
    }
});
```

**Trigger** (GuildNetworkHandler.cs):
```csharp
serverApi.Event.PushEvent("nodewar_data_request", new NodeWarDataRequest
{
    Player = player,
    GuildName = packet.GuildName
});
```

## DateTime Serialization

**Problem**: ProtoBuf doesn't support DateTime directly

**Solution**: Convert to/from long (DateTime.Ticks)

```csharp
// Serialization (NodeWarDataProvider.cs)
CapturedAtTicks = node.CapturedAt?.Ticks ?? 0

// Deserialization (DialogGuildMain.cs)  
CapturedAt = dto.CapturedAtTicks > 0 ? new DateTime(dto.CapturedAtTicks) : null
```

## Error Handling

1. **PVP Mod Not Available**
   - NodeWarManager is null
   - Returns empty NodeWarTabData
   - UI shows "PVP mod required" message

2. **No Data Available**
   - Guild has no node war activity
   - Returns empty collections
   - UI shows appropriate "None" messages

3. **Network Errors**
   - Logged to server/client logs
   - UI callback not invoked (data remains null)

## Call Sequence Diagram

```
Client (DialogGuildMain)
    ↓ RefreshNodeWarsData()
    ↓ RequestNodeWarData(guildName)
    
GuildNetworkHandler (Client)
    ↓ SendPacket(NodeWarDataRequestPacket)
    
    [Network]
    
GuildNetworkHandler (Server)
    ↓ OnNodeWarDataRequestReceived()
    ↓ PushEvent("nodewar_data_request")
    
PVPModSystem (Server)
    ↓ OnNodeWarDataRequested()
    ↓ NodeWarDataProvider.GetNodeWarDataForGuild()
    ↓ NodeWarDataProvider.ConvertToNetworkPacket()
    ↓ GuildNetworkHandler.SendNodeWarData()
    
GuildNetworkHandler (Server)
    ↓ SendPacket(NodeWarDataResponsePacket)
    
    [Network]
    
GuildNetworkHandler (Client)
    ↓ OnNodeWarDataResponseReceived()
    ↓ Invoke callback
    
Client (DialogGuildMain)
    ↓ OnNodeWarDataReceived()
    ↓ Convert DTOs to UI models
    ↓ nodeWarsTab.SetNodeWarData()
    ↓ SetupDialog() [if on Node Wars tab]
```

## Testing Checklist

- [ ] Client can request node war data when opening Guild UI
- [ ] Server correctly routes request to PVP mod
- [ ] PVP mod gathers and sends data back
- [ ] Client correctly displays controlled nodes
- [ ] Client correctly displays active wars
- [ ] Client correctly displays available wars
- [ ] Client correctly displays current signup
- [ ] UI shows "PVP mod required" when PVP mod not installed
- [ ] UI shows "None" messages when no data available
- [ ] DateTime values display correctly (not corrupted)
- [ ] Refresh works when switching between tabs
- [ ] No errors in server/client logs

## Implementation Files Modified

### Guild Mod
- `src/network/GuildPackets.cs` - Added network packet DTOs
- `src/network/GuildNetworkHandler.cs` - Added request/response handlers
- `src/gui/DialogGuildMain.cs` - Added callback registration and handler
- `src/gui/tabs/GuildNodeWarsTab.cs` - Already had SetNodeWarData() method

### PVP Mod  
- `src/nodewars/NodeWarDataProvider.cs` - Added ConvertToNetworkPacket()
- `PVPModSystem.cs` - Added event listener and handler

## Summary

The `SetNodeWarData()` method is now properly called through a complete network communication flow:

1. **DialogGuildMain** requests data when tab is opened
2. **GuildNetworkHandler** routes the request to PVP mod via custom event
3. **PVPModSystem** processes request using **NodeWarDataProvider**
4. **NodeWarDataProvider** gathers data and converts to network packets
5. **GuildNetworkHandler** sends response back to client
6. **DialogGuildMain** receives response and calls `SetNodeWarData()`
7. **GuildNodeWarsTab** displays the updated data

This implementation maintains loose coupling between mods while providing real-time node war data to the Guild UI.
