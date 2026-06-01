using SRGuildsAndKingdoms.src.gui.components;
using SRGuildsAndKingdoms.src.network;
using SRGuildsAndKingdoms.src.party;
using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
    public class DialogPartyManager(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : GuiDialog(capi)
    {
        private Party? currentParty;
        private GuiElementListMenu? listMenu;

        private string? playerToAction;

        private GuiDialogConfirm? kickConfirmDialog;
        private GuiDialogConfirm? promoteConfirmDialog;
        private GuiDialogConfirm? leaveConfirmDialog;
        private GuiDialogConfirm? disbandConfirmDialog;

        private readonly CairoFont Font = CairoFont.WhiteSmallishText().WithFontSize(12);

        public override string ToggleKeyCombinationCode => "partymanagerdialog";

        private void PromotePlayerConfirm(bool choice)
        {
            if (choice && playerToAction != null)
            {
                OnPromotePlayer(playerToAction);
            }
        }

        private void KickPlayerConfirm(bool choice)
        {
            if (choice && playerToAction != null)
            {
                OnKickPlayer(playerToAction);
            }
        }

        private void LeaveGuildConfirm(bool choice)
        {
            if (choice)
            {
                OnLeaveParty();
            }
        }

        private void DisbandGuildConfirm(bool choice)
        {
            if (choice)
            {
                OnDisbandParty();
            }
        }

        public void UpdateParty(Party? party)
        {
            currentParty = party;

            if (IsOpened())
            {
                ComposeDialog();
            }
        }

        private void ComposeDialog()
        {
            ClearComposers();

            if (currentParty == null)
            {
                TryClose();
                return;
            }

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            bool isLeader = currentParty.IsLeader(capi.World.Player.PlayerUID);

            int dialogWidth = 380;
            int scrollAreaHeight = 200;
            int scrollbarWidth = 20;

            var scrollInsetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, dialogWidth, scrollAreaHeight);
            var scrollbarBounds = scrollInsetBounds.RightCopy().WithFixedWidth(scrollbarWidth);

            var clipBounds = scrollInsetBounds.ForkContainingChild();
            var containerBounds = scrollInsetBounds.ForkContainingChild();

            var bgBounds = ElementBounds.Fill
                .WithFixedPadding(GuiStyle.ElementToDialogPadding)
                .WithSizing(ElementSizing.FitToChildren)
                .WithChildren(scrollInsetBounds, scrollbarBounds);

            var composer = capi.Gui.CreateCompo("party-dialog", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar($"Party: {currentParty.Name}", OnTitleBarClose, Font.Clone().WithFontSize(14))
                .BeginChildElements(bgBounds);

            composer
                .AddInset(scrollInsetBounds)
                .BeginClip(clipBounds)
                .AddContainer(containerBounds, "scroll-content")
                .EndClip()
                .AddInteractiveElement(new GuiElementQuestScrollbar(capi, OnNewScrollbarValue, scrollbarBounds), "scrollbar");

            GuiElementContainer scrollArea = composer.GetContainer("scroll-content");
            double contentHeight = AddMemberList(scrollArea, currentParty);

            float scrollVisibleHeight = (float)clipBounds.fixedHeight;

            if (contentHeight < scrollVisibleHeight)
            {
                scrollInsetBounds.fixedHeight = contentHeight;
                clipBounds.fixedHeight = contentHeight;
                scrollbarBounds.fixedHeight = contentHeight;

                bgBounds.CalcWorldBounds();

                scrollVisibleHeight = (float)contentHeight;
            }


            AddActionButtons(composer, scrollVisibleHeight);

            composer.EndChildElements();

            SingleComposer = composer.Compose();

            float scrollTotalHeight = (float)Math.Max(contentHeight, scrollVisibleHeight);
            SingleComposer.GetScrollbar("scrollbar").SetHeights(scrollVisibleHeight, scrollTotalHeight);

            Composers["partymanager"] = composer;
        }

        private void AddActionButtons(GuiComposer composer, double yPos)
        {
            yPos += 50;

            if (currentParty?.IsLeader(capi.World.Player.PlayerUID) == true)
            {
                var disbandButtonBounds = ElementBounds.Fixed(0, yPos, 80, 25);
                composer.AddButton("Disband", HandleClickDisband, disbandButtonBounds, Font.Clone().WithOrientation(EnumTextOrientation.Center).WithWeight(Cairo.FontWeight.Bold), EnumButtonStyle.Normal, "disband-button");

                var onlinePlayers = capi.World.AllOnlinePlayers
                    .Where(p => !currentParty.Members.Any(m => m.PlayerUid == p.PlayerUID))
                    .ToArray();
                string?[] uidArray = new string?[onlinePlayers.Length + 1];
                string[] nameArray = new string[onlinePlayers.Length + 1];
                uidArray[0] = null;
                nameArray[0] = "Invite...";
                for (int i = 0; i < onlinePlayers.Length; i++)
                {
                    uidArray[i + 1] = onlinePlayers[i].PlayerUID;
                    nameArray[i + 1] = onlinePlayers[i].PlayerName;
                }

                var inviteDropdownBounds = ElementBounds.Fixed(300, yPos, 80, 25);
                composer.AddDropDown(uidArray, nameArray, 0, (code, selected) => OnInvitePlayer(code), inviteDropdownBounds, Font, "invite-player");
            }
            else
            {
                var leaveButtonBounds = ElementBounds.Fixed(0, yPos, 60, 25);
                composer.AddButton("Leave", HandleClickLeave, leaveButtonBounds, Font.Clone().WithOrientation(EnumTextOrientation.Center).WithWeight(Cairo.FontWeight.Bold), EnumButtonStyle.Normal, "leave-button");
            }
        }

        private bool HandleClickDisband()
        {
            disbandConfirmDialog?.TryOpen();

            return true;
        }

        private bool HandleClickLeave()
        {
            leaveConfirmDialog?.TryOpen();

            return true;
        }

        private double AddMemberList(GuiElementContainer scrollArea, Party party)
        {
            double yPos = 0;
            bool isLeader = party.IsLeader(capi.World.Player.PlayerUID);

            for (int i = 0; i < party.Members.Count; i++)
            {
                var member = party.Members[i];
                bool isThisMemberLeader = i == 0;

                var insetBounds = ElementBounds.Fixed(0, yPos, 380, 30);
                var inset = new components.GuiElementPartyMemberInset(capi, insetBounds, 0, 1f, 0.85f);
                scrollArea.Add(inset);

                string displayName = member.PlayerName;
                string icon = "wpCircle";

                if (isThisMemberLeader)
                {
                    icon = "wpStar2";
                }

                var iconBounds = ElementBounds.Fixed(8, yPos + (icon == "wpCircle" ? 9 : 10), 15, 30);
                var nameBounds = ElementBounds.Fixed(25, yPos + 8, 225, 30);

                var iconComponents = VtmlUtil.Richtextify(
                    capi,
                    $"<font size=\"12\" color=\"#{(member.IsOnline ? "00FF00" : "808080")}\"><icon name={icon}></icon></font>",
                    Font
                );

                var nameComponents = VtmlUtil.Richtextify(
                    capi,
                    $"{displayName}",
                    Font
                );

                scrollArea.Add(new GuiElementRichtext(capi, iconComponents, iconBounds));
                scrollArea.Add(new GuiElementRichtext(capi, nameComponents, nameBounds));

                if (isLeader && member.PlayerUid != capi.World.Player.PlayerUID)
                {
                    var dropdownTriggerBounds = ElementBounds.Fixed(nameBounds.fixedX + nameBounds.fixedWidth + 105, yPos + 4, 20, 20);
                    var dropdownBounds = ElementBounds.Fixed(nameBounds.fixedX + nameBounds.fixedWidth + 155, yPos + 4, 20, 20);
                    var menu = new GuiElementListMenu(capi, [null, "kick", "promote"], ["...", "Kick", "Promote"], 0, (code, selected) => OnMemberAction(code, selected, member.PlayerUid), dropdownTriggerBounds, Font, false);

                    scrollArea.Add(menu);
                    scrollArea.Add(new GuiElementIconButton(capi, "wpGear", "", Font, obj =>
                    {
                        if (listMenu != null)
                        {
                            listMenu.OnFocusLost();
                            listMenu.SelectedIndex = -1;
                        }

                        listMenu = menu;
                        listMenu.SelectedIndex = -1;
                        listMenu.Open();
                    }, dropdownTriggerBounds, false));
                }

                yPos += 30;
            }

            return yPos;
        }

        private void OnMemberAction(string code, bool selected, string uid)
        {
            if (code == "kick")
            {
                playerToAction = uid;
                kickConfirmDialog?.TryOpen();
            }
            else if (code == "promote")
            {
                playerToAction = uid;
                promoteConfirmDialog?.TryOpen();
            }
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();

            promoteConfirmDialog ??= new(capi, "Are you sure you want to promote this player to party leader?", PromotePlayerConfirm);
            leaveConfirmDialog ??= new(capi, "Are you sure you want to leave this party?", LeaveGuildConfirm);
            kickConfirmDialog ??= new(capi, "Are you sure you want to kick this player from the party?", KickPlayerConfirm);
            disbandConfirmDialog ??= new(capi, "Are you sure you want to disband the party?", DisbandGuildConfirm);

            modSystem.PartyNetworkHandler?.SendRequestPartyData();
        }

        private void OnNewScrollbarValue(float value)
        {
            ElementBounds bounds = SingleComposer.GetContainer("scroll-content").Bounds;
            bounds.fixedY = 0 - value;
            bounds.CalcWorldBounds();
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private bool OnInvitePlayer(string? playerUid)
        {
            if (playerUid == null)
            {
                return false;
            }

            modSystem.PartyNetworkHandler?.SendInvitePlayer(playerUid);

            var dropdown = SingleComposer.GetDropDown("invite-player");

            if (dropdown != null)
            {
                dropdown.SetSelectedIndex(0);
            }
            return true;
        }

        private bool OnKickPlayer(string playerUid)
        {
            if (currentParty == null) return true;

            capi.Logger.Notification($"[DialogPartyManager] Kicking player {playerUid}");
            modSystem.PartyNetworkHandler?.SendKickPlayer(playerUid);

            return true;
        }

        private bool OnPromotePlayer(string playerUid)
        {
            if (currentParty == null) return true;

            capi.Logger.Notification($"[DialogPartyManager] Promoting player {playerUid} to leader");
            modSystem.PartyNetworkHandler?.SendPromotePlayer(playerUid);

            return true;
        }

        private bool OnLeaveParty()
        {
            capi.Logger.Notification("[DialogPartyManager] Leaving party");
            modSystem.PartyNetworkHandler?.SendLeaveParty();

            TryClose();
            return true;
        }

        private bool OnDisbandParty()
        {
            if (currentParty == null) return true;

            capi.Logger.Notification($"[DialogPartyManager] Disbanding party '{currentParty.Name}'");
            modSystem.PartyNetworkHandler?.SendDisbandParty();

            TryClose();
            return true;
        }
    }
}
