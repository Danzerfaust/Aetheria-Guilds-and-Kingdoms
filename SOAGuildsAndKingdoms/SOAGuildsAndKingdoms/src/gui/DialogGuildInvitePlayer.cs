using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    internal class DialogGuildInvitePlayer : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private List<string> availablePlayerNames = new();
        private List<IPlayer> availablePlayers = new();

        public DialogGuildInvitePlayer(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
            PopulateAvailablePlayers();
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildinvite";

        private void PopulateAvailablePlayers()
        {
            availablePlayers.Clear();
            availablePlayerNames.Clear();

            // Get all online players
            var allPlayers = capi.World.AllPlayers;
            var currentPlayerUid = capi.World.Player.PlayerUID;

            // Get all guild summaries to determine who's already in guilds or has pending invites
            var guildSummaries = modSystem.GetClientGuildSummaries();
            var playersInGuilds = new HashSet<string>();
            var playersWithPendingInvites = new HashSet<string>();

            // Collect UIDs of players who are already in guilds or have pending invites
            foreach (var guild in guildSummaries)
            {
                foreach (var memberUid in guild.MemberUids)
                {
                    playersInGuilds.Add(memberUid);
                }

                foreach (var pendingInviteUid in guild.PendingInviteUids)
                {
                    playersWithPendingInvites.Add(pendingInviteUid);
                }
            }

            // Filter players: exclude current player, those already in guilds, and those with pending invites
            foreach (var player in allPlayers)
            {
                if (player.PlayerUID != currentPlayerUid &&
                    !playersInGuilds.Contains(player.PlayerUID) &&
                    !playersWithPendingInvites.Contains(player.PlayerUID))
                {
                    availablePlayers.Add(player);
                }
            }

            // Sort alphabetically by player name for better UX
            availablePlayers = availablePlayers.OrderBy(p => p.PlayerName).ToList();

            // Build the player names list for the dropdown
            availablePlayerNames.Add("Select a player...");
            foreach (var player in availablePlayers)
            {
                availablePlayerNames.Add(player.PlayerName);
            }
        }

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildinvite", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:invite-player-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 10.0;
            var elementHeight = 25.0;

            // Instructions
            string instructionText = availablePlayerNames.Count > 1
                ? Lang.Get("soaguildsandkingdoms:invite-player-instructions")
                : "No players available to invite. All online players are either already in guilds or it's just you online.";

            composer.AddStaticText(instructionText,
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 350, elementHeight * 2));
            top += elementHeight * 2 + spacing;

            if (availablePlayerNames.Count > 1) // More than just the "Select player..." option
            {
                // Player selection dropdown
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:player-name"),
                    CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));

                composer.AddDropDown(availablePlayerNames.ToArray(), availablePlayerNames.ToArray(), 0,
                    OnPlayerSelected, ElementBounds.Fixed(110, top, 200, elementHeight), "playerdropdown");
                top += elementHeight + spacing * 1.5;

                // Buttons
                composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:invite"), OnInviteClick,
                    ElementBounds.Fixed(0, top, 80, elementHeight), EnumButtonStyle.Normal);

                composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelClick,
                    ElementBounds.Fixed(90, top, 80, elementHeight), EnumButtonStyle.Normal);
            }
            else
            {
                // Only show close button if no players available
                composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:close"), OnCancelClick,
                    ElementBounds.Fixed(0, top, 80, elementHeight), EnumButtonStyle.Normal);
            }

            SingleComposer = composer.Compose();
        }

        private void OnPlayerSelected(string code, bool selected)
        {
            // This callback is triggered when a player is selected from the dropdown
            // The 'code' parameter contains the selected player name
        }

        private bool OnInviteClick()
        {
            if (availablePlayers.Count == 0) return true; // No players available

            var dropdown = SingleComposer.GetDropDown("playerdropdown");
            var selectedIndex = dropdown.SelectedIndices[0];

            // Index 0 is "Select a player...", so we need index > 0
            if (selectedIndex <= 0)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:please-select-player"));
                return true;
            }

            // Adjust index to account for "Select a player..." at position 0
            var playerIndex = selectedIndex - 1;
            var selectedPlayer = availablePlayers[playerIndex];

            // Use network handler to send invite request directly with UID
            var networkHandler = modSystem.GetNetworkHandler();
            if (networkHandler != null)
            {
                networkHandler.SendGuildInviteRequest(selectedPlayer.PlayerUID);
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:invite-sent", selectedPlayer.PlayerName));
            }

            TryClose();
            return true;
        }

        private bool OnCancelClick()
        {
            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }

        public override bool PrefersUngrabbedMouse => false;
    }
}
