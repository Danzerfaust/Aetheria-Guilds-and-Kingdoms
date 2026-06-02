using SOAGuildsAndKingdoms.src.gui.components;
using SOAGuildsAndKingdoms.src.network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace SOAGuildsAndKingdoms.src.gui
{
    public class DialogEventEditor : GuiDialog
    {
        public override string? ToggleKeyCombinationCode => null;

        public DialogEventEditor(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem, EventDto eventDto) : base(capi)
        {
            SetupDialog();
        }

        #region Defaults

        private const string DialogTitle = "Event Editor";
        private const string DialogId = "event-editor-dialog";

        private readonly int DialogWidth = 800;
        private readonly int ScrollAreaHeight = 400;
        private readonly int ScrollbarWidth = 20;
        private readonly int FieldHeight = 30;
        private readonly int LabelHeight = 10;
        private readonly int Padding = 5;

        private readonly string NameLabel = "Name";
        private readonly int NameWidth = 350;
        private readonly string DescriptionLabel = "Description";
        private readonly string MaxPlayersLabel = "Max Players";
        private readonly string PositionLabel = "Event Position: {0}";
        private readonly int PositionWidth = 150;

        private readonly CairoFont Font = CairoFont.WhiteSmallishText().WithFontSize(12);

        #endregion

        #region State

        // Preserve form state across compositions
        private string eventName = "My New Event";
        private string eventDescription = "This is an example description.";
        private int eventMaxPlayers = 50;
        private Vec3i? eventPosition = null;

        private int eventStartYear = DateTime.Now.Year;
        private int eventStartMonth = DateTime.Now.Month;
        private int eventStartDay = DateTime.Now.Day;
        private int eventStartHour = DateTime.Now.Hour;
        private int eventStartMinute = DateTime.Now.Minute;

        private int eventEndYear = DateTime.Now.Year;
        private int eventEndMonth = DateTime.Now.Month;
        private int eventEndDay = DateTime.Now.Day;
        private int eventEndHour = DateTime.Now.Hour;
        private int eventEndMinute = DateTime.Now.Minute;

        #endregion

        #region Dialog composition

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            var scrollInsetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, DialogWidth, ScrollAreaHeight);
            var scrollbarBounds = scrollInsetBounds.RightCopy().WithFixedWidth(ScrollbarWidth);

            var bgBounds = ElementBounds.Fill
                .WithFixedPadding(GuiStyle.ElementToDialogPadding)
                .WithSizing(ElementSizing.FitToChildren)
                .WithChildren(scrollInsetBounds, scrollbarBounds);

            var composer = capi.Gui.CreateCompo(DialogId, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose, Font.Clone().WithFontSize(14))
                .BeginChildElements(bgBounds)
                .AddInset(scrollInsetBounds);

            AddEventList(composer);

            composer.EndChildElements();

            SingleComposer = composer.Compose();

            composer.GetTextInput(NameLabel).SetValue(eventName);
            composer.GetTextArea(DescriptionLabel).SetValue(eventDescription);

            SingleComposer.UnfocusOwnElements();
        }

        private double AddEventList(GuiComposer composer)
        {
            double yPos = GuiStyle.TitleBarHeight + Padding;

            // Name label
            composer.AddStaticText(NameLabel, Font, ElementBounds.Fixed(Padding, yPos, NameWidth, LabelHeight));
            yPos += LabelHeight + Padding;
            // Name field
            composer.AddTextInput(ElementBounds.Fixed(Padding, yPos, NameWidth, FieldHeight), null, Font, NameLabel);
            yPos += FieldHeight + Padding;

            // Description label
            composer.AddStaticText(DescriptionLabel, Font, ElementBounds.Fixed(Padding, yPos, NameWidth, LabelHeight));
            yPos += LabelHeight + Padding;
            // Description field
            composer.AddTextArea(ElementBounds.Fixed(Padding, yPos, NameWidth, FieldHeight * 6), null, Font, DescriptionLabel);
            var descriptionBox = composer.GetTextArea(DescriptionLabel);
            descriptionBox.Autoheight = false;
            descriptionBox.SetMaxLines(12);
            yPos += (FieldHeight * 6) + Padding;

            // Max players label
            composer.AddStaticText(MaxPlayersLabel, Font, ElementBounds.Fixed(Padding, yPos, NameWidth, LabelHeight));
            yPos += LabelHeight + Padding;
            // Max players field
            string[] maxPlayersValues = [.. Enumerable.Range(1, 100).Select(i => i.ToString())];
            composer.AddDropDown(maxPlayersValues, maxPlayersValues, eventMaxPlayers - 1, null, ElementBounds.Fixed(Padding, yPos, NameWidth, FieldHeight), MaxPlayersLabel);
            yPos += FieldHeight + Padding;

            // Position label
            composer.AddStaticText(string.Format(PositionLabel, eventPosition != null ? $"{eventPosition.X}, {eventPosition.Z}" : "unknown"), Font, ElementBounds.Fixed(Padding, yPos, NameWidth, LabelHeight));
            yPos += LabelHeight + Padding;
            // Position button
            composer.AddButton(eventPosition != null ? $"Clear Position" : "Set Position to Here", OnPositionClick, ElementBounds.Fixed(Padding, yPos, PositionWidth, FieldHeight), Font.Clone().WithOrientation(EnumTextOrientation.Center), EnumButtonStyle.Normal);
            yPos += FieldHeight + Padding;

            return yPos + Padding;
        }

        private bool OnPositionClick()
        {
            if (eventPosition != null)
            {
                eventPosition = null;
            }
            else
            {
                eventPosition = capi.World.Player.Entity.Pos.AsBlockPos.ToLocalPosition(capi);
            }

            return true;
        }

        #endregion

        #region Behaviors

        private void OnTitleBarClose()
        {
            TryClose();
        }

        #endregion
    }
}