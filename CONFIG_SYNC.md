# Config Synchronization - Implementation Complete

## Overview
The protected zones and territorial restrictions config data now properly synchronizes from server to clients, allowing the map visualization to work correctly.

## Changes Made

### 1. GuildPackets.cs
**Added new packet types**:
- `GuildConfigPacket`: Contains all config data (territorial restrictions + protected zones)
- `ProtectedZoneData`: DTO for individual protected zone information

```csharp
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GuildConfigPacket : GuildPacketBase
{
    public bool TerritorialRestrictionsEnabled { get; set; }
    public int? TerritorialCenterX { get; set; }
    public int? TerritorialCenterZ { get; set; }
    public int TerritorialRadius { get; set; }
    public bool ProtectedZonesEnabled { get; set; }
    public List<ProtectedZoneData> ProtectedZones { get; set; }
}
```

### 2. GuildNetworkHandler.cs
**Server-side additions**:
- Registered `GuildConfigPacket` and `ProtectedZoneData` message types
- Added `SendGuildConfig(IServerPlayer)` method - sends config to specific player
- Added `BroadcastGuildConfigToAll()` method - sends config to all online players
- Updated `OnPlayerJoin()` to send config when players connect

**Client-side additions**:
- Registered config packet handler
- Added `OnGuildConfigReceived(GuildConfigPacket)` callback handler
- Added `RegisterConfigCallback(Action<GuildConfigPacket>)` method

### 3. PlotMapLayer.cs
**Added method**:
```csharp
public void UpdateConfigFromServer(GuildConfigPacket config)
```

This method:
- Updates `territorialRestrictionsEnabled` and related fields
- Updates `protectedZonesEnabled` and `protectedZones` list
- Converts DTO data to cached tuple format
- Logs the update for debugging

### 4. SRGuildsAndKingdomsModSystem.cs
**Server-side changes**:
- Updated `OnSaveGameLoaded()` to call `BroadcastGuildConfigToAll()` on server start

**Client-side changes**:
- Added `OnGuildConfigReceived(GuildConfigPacket)` callback method
- Registered config callback in `StartClientSide()`
- Forwards received config to PlotMapLayer

## Data Flow

```
Server Start:
  ??> GuildConfigManager.LoadConfig()
  ??> BroadcastGuildConfigToAll()
  ??> Each online player receives GuildConfigPacket

Player Joins:
  ??> OnPlayerJoin event fires
  ??> Send GuildSyncPacket (guild data)
  ??> SendGuildConfig (config data)

Client Receives Config:
  ??> GuildNetworkHandler.OnGuildConfigReceived()
  ??> Callback registered in ModSystem
  ??> ModSystem.OnGuildConfigReceived()
  ??> Find PlotMapLayer instance
  ??> PlotMapLayer.UpdateConfigFromServer()
```

## Key Features

? **Automatic Sync**: Config sent on server start and player join
? **Efficient**: Only sends config when needed (no polling)
? **Type-Safe**: Uses ProtoContract serialization  
? **Debuggable**: Logging at each step for troubleshooting
? **Backward Compatible**: Nullable values for optional config

## Testing Checklist

### Server-Side
- [x] Config loads from file on server start
- [x] Config broadcasts to all players on load
- [x] Config sends to new players on join
- [x] Protected zones validated and logged

### Client-Side  
- [x] Client receives config packet
- [x] PlotMapLayer updates internal cache
- [x] Map visualization shows protected zones (purple)
- [x] Map visualization shows territorial restrictions (red)
- [x] Hover tooltips work correctly
- [x] Claiming prevention works in protected zones

### Network
- [x] Packet types registered on both sides
- [x] Serialization/deserialization works
- [x] No network errors in logs
- [x] Build successful

## Debug Commands

To verify config sync is working:

1. **Check server logs** on startup:
   ```
   [Info] Guild configuration loaded successfully
   [Info] Protected zones enabled: 2 zone(s) defined
   [Debug] Sending guild config to player: [PlayerName]
   ```

2. **Check client logs** when joining:
   ```
   [Debug] Received guild config from server: Territorial=False, Protected Zones=True (2 zones)
   [Debug] PlotMapLayer config updated: Territorial=False, Protected Zones=True (2 zones)
   ```

3. **Test in-game**:
   - Open world map (M key)
   - Look for purple overlays on protected zones
   - Hover over protected zones - should show zone info
   - Try to claim in protected zone - should be prevented

## Troubleshooting

### Protected zones not visible on map
- Check client debug logs for "PlotMapLayer config updated"
- Verify zones are within visible map area
- Zoom in closer (zones only render when large enough)

### Config not syncing
- Check that packet types are registered on both client and server
- Verify `BroadcastGuildConfigToAll()` is called in `OnSaveGameLoaded()`
- Check for serialization errors in logs

### Performance Issues
- Config sync happens once per player join (very lightweight)
- No continuous polling or updates
- Caching on client prevents repeated calculations

## Future Enhancements

Potential improvements:
- [ ] Admin command to reload config without restart
- [ ] In-game command to add/remove protected zones
- [ ] Config change detection and re-broadcast
- [ ] Client-side config caching to disk
- [ ] Protected zone visualization in 3D world

## Related Files

- `GuildPackets.cs` - Network packet definitions
- `GuildNetworkHandler.cs` - Network communication
- `PlotMapLayer.cs` - Client-side map rendering
- `SRGuildsAndKingdomsModSystem.cs` - Mod initialization
- `GuildConfig.cs` - Server-side config logic

## Documentation

See also:
- `PROTECTED_ZONES.md` - User guide for protected zones
- `INTEGRATION_SUMMARY.md` - Technical implementation details
- `QUICK_REFERENCE.md` - Quick setup guide

---

**Status**: ? Complete and tested
**Build**: ? Successful
**Ready for**: Production use
