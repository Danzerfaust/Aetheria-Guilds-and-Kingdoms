# Testing Guild Invite Popup System

This guide will help you test the guild invite popup feature in singleplayer mode using debug commands.

## Prerequisites

- Start Vintage Story in **Creative Mode** (for controlserver privilege)
- Load a world with the mod installed

## Debug Command

The mod includes a `/debuginvite` command that simulates receiving guild invites without needing multiple players.

### Syntax

```
/debuginvite <guildName> [inviterName] [expirySeconds]
```

**Parameters:**
- `guildName` (required): Name of the guild for the invite
- `inviterName` (optional): Name of player sending invite (default: "TestPlayer")
- `expirySeconds` (optional): Seconds until invite expires (default: 300 = 5 minutes)

## Testing Scenarios

### 1. Basic Single Invite Test

Test the popup appearance and basic functionality:

```
/debuginvite "Warriors Guild" Bob
```

**What to check:**
- ✅ Popup appears in bottom-right corner
- ✅ Shows guild name "Warriors Guild"
- ✅ Shows inviter name "Bob"
- ✅ Countdown timer shows "Expires in 5 minutes"
- ✅ Accept and Decline buttons are visible and clickable

### 2. Quick Expiry Test

Test the auto-expiry functionality with a 10-second timer:

```
/debuginvite "Test Guild" Alice 10
```

**What to check:**
- ✅ Timer shows "Expires in 10 seconds" initially
- ✅ Timer counts down each second
- ✅ When timer reaches 0, invite disappears from popup
- ✅ If last invite, popup auto-closes

### 3. Multiple Invites Test

Test navigation between multiple invites:

```
/debuginvite "Guild Alpha" Alice
/debuginvite "Guild Beta" Bob
/debuginvite "Guild Gamma" Charlie
/debuginvite "Guild Delta" Dave
```

**What to check:**
- ✅ Popup shows "Invite 1 of 4"
- ✅ < and > navigation buttons appear
- ✅ Can navigate forward and backward through invites
- ✅ Each invite shows correct guild name and inviter
- ✅ Navigation wraps around (from last to first, first to last)

### 4. Accept Invite Test

```
/debuginvite "TestGuild" TestPlayer 60
```

Click **Accept** button

**What to check:**
- ✅ Invite is removed from the list
- ✅ If multiple invites exist, switches to next invite
- ✅ If last invite, popup closes
- ✅ Chat message confirms "No pending guild invite found or invite has expired" (since it's a fake invite)

### 5. Decline Invite Test

```
/debuginvite "TestGuild" TestPlayer 60
```

Click **Decline** button

**What to check:**
- ✅ Invite is removed from the list
- ✅ If multiple invites exist, switches to next invite
- ✅ If last invite, popup closes

### 6. Mixed Expiry Times Test

Test with different expiry times to see staggered expiration:

```
/debuginvite "Guild1" Player1 15
/debuginvite "Guild2" Player2 30
/debuginvite "Guild3" Player3 45
```

**What to check:**
- ✅ First invite expires after 15 seconds
- ✅ Invite count updates (e.g., "Invite 1 of 2" after first expires)
- ✅ Second invite expires after 30 seconds
- ✅ Third invite expires after 45 seconds
- ✅ Popup auto-closes when last invite expires

### 7. Re-opening Closed Popup

```
/debuginvite "TestGuild1" Player1
/debuginvite "TestGuild2" Player2
```

Close the popup (X button), then use:

```
/guild invites
```

**What to check:**
- ✅ Popup reopens with all pending invites
- ✅ Shows same invites that were there before
- ✅ Timers continue counting down correctly

### 8. Rapid Fire Invites Test

Send many invites quickly:

```
/debuginvite "Guild01" P1
/debuginvite "Guild02" P2
/debuginvite "Guild03" P3
/debuginvite "Guild04" P4
/debuginvite "Guild05" P5
```

**What to check:**
- ✅ All invites are captured
- ✅ Navigation works smoothly
- ✅ No visual glitches or overlap
- ✅ Performance is smooth

## Edge Cases to Test

### Close and Reopen During Countdown

1. Send an invite with 60 second expiry
2. Wait 30 seconds
3. Close popup
4. Use `/guild invites` to reopen
5. Verify timer shows ~30 seconds remaining

### Accept Middle Invite in List

1. Send 5 invites
2. Navigate to 3rd invite
3. Click Accept
4. Verify it removes the 3rd invite and shows the 4th

### Multiple Actions Quickly

1. Send 3 invites
2. Quickly decline, navigate, accept, navigate, decline
3. Verify no crashes or stuck states

## Command Reference

```bash
# Single invite (5 min expiry)
/debuginvite "MyGuild"

# With custom inviter
/debuginvite "MyGuild" "JohnDoe"

# With quick expiry (30 seconds)
/debuginvite "MyGuild" "JohnDoe" 30

# List current invites
/guild invites

# Accept first pending invite (if using real guilds)
/guild accept
```

## Tips for Testing

1. **Test in windowed mode** - easier to see bottom-right corner
2. **Adjust UI scale** - test at different scales to ensure popup is visible
3. **Test with other dialogs open** - ensure no conflicts
4. **Use F3 debug info** - verify no errors in console
5. **Test both day and night** - ensure text is readable in all lighting

## Known Behaviors

- Debug invites won't actually add you to a guild (they're fake)
- Accept/Decline will show "No pending invite found" for debug invites
- Expiry cleanup happens every time you interact with invites
- Popup auto-closes when no invites remain

## Troubleshooting

**Popup doesn't appear:**
- Check you have controlserver privilege (Creative Mode)
- Verify mod is loaded (`/modinfo SRGuildsAndKingdoms`)
- Check log for errors

**Timer not updating:**
- This is a bug - popup should update every second
- Try closing and reopening with `/guild invites`

**Buttons not working:**
- Ensure popup has focus
- Try clicking slightly inside the button boundaries
- Check log for click handler errors

## Real World Testing

After debug testing, test with real guilds:

1. Create a guild: `/guild create TestGuild`
2. Have another player join
3. Invite them through the GUI
4. They should see the real popup
5. They can accept through popup or `/guild accept`

---

Happy testing! Report any issues or unexpected behaviors.
