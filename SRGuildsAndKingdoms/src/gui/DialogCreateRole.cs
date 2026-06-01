using System;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000078 RID: 120
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogCreateRole : GuiDialog
	{
		// Token: 0x060004FD RID: 1277 RVA: 0x0001EC3F File Offset: 0x0001CE3F
		public DialogCreateRole(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, Action<string, GuildPermission, int> onRoleCreated) : base(capi)
		{
			this.modSystem = modSystem;
			this.onRoleCreated = onRoleCreated;
			this.SetupDialog();
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060004FE RID: 1278 RVA: 0x0001EC64 File Offset: 0x0001CE64
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildcreaterole";
			}
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x0001EC6C File Offset: 0x0001CE6C
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildcreaterole", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:create-new-role-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 15.0;
			double elementHeight = 35.0;
			double width = 350.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:create-role-instructions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:role-name", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(110.0, top, 200.0, elementHeight), null, CairoFont.TextInput(), "rolename");
			top += elementHeight + spacing * 1.5;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:hierarchy", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 100.0, elementHeight), null);
			GuiComposerHelpers.AddSmallButton(composer, "▲", delegate()
			{
				this.selectedHierarchy = Math.Max(1, this.selectedHierarchy - 1);
				this.SetupDialog();
				return true;
			}, ElementBounds.Fixed(110.0, top, 30.0, elementHeight), 2, "hierarchy_up");
			GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(145.0, top, 50.0, elementHeight), delegate(string text)
			{
				this.OnHierarchyTextChanged(text);
			}, CairoFont.TextInput(), "hierarchy_input");
			GuiComposerHelpers.GetTextInput(composer, "hierarchy_input").SetValue(this.selectedHierarchy.ToString(), true);
			GuiComposerHelpers.AddSmallButton(composer, "▼", delegate()
			{
				this.selectedHierarchy = Math.Min(998, this.selectedHierarchy + 1);
				this.SetupDialog();
				return true;
			}, ElementBounds.Fixed(200.0, top, 30.0, elementHeight), 2, "hierarchy_down");
			GuiComposerHelpers.AddStaticText(composer, "(1-998)", CairoFont.WhiteSmallText().WithColor(new double[]
			{
				0.7,
				0.7,
				0.7,
				1.0
			}), ElementBounds.Fixed(235.0, top, 115.0, elementHeight), null);
			top += elementHeight + spacing * 1.5;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:permissions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, width, elementHeight), null);
			top += elementHeight + spacing;
			this.AddPermissionCheckbox(composer, ref top, "Invite", GuildPermission.Invite);
			this.AddPermissionCheckbox(composer, ref top, "Promote", GuildPermission.Promote);
			this.AddPermissionCheckbox(composer, ref top, "Kick", GuildPermission.Kick);
			this.AddPermissionCheckbox(composer, ref top, "ManageRoles", GuildPermission.ManageRoles);
			this.AddPermissionCheckbox(composer, ref top, "BreakAndPlaceBlocks", GuildPermission.BreakAndPlaceBlocks);
			this.AddPermissionCheckbox(composer, ref top, "InteractBlocks", GuildPermission.InteractBlocks);
			top += spacing;
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:create", Array.Empty<object>()), new ActionConsumable(this.OnCreateClick), ElementBounds.Fixed(0.0, top, 80.0, elementHeight), 2, null);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(90.0, top, 80.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001F038 File Offset: 0x0001D238
		private void AddPermissionCheckbox(GuiComposer composer, ref double top, string permissionName, GuildPermission permission)
		{
			double elementHeight = 25.0;
			double spacing = 5.0;
			bool isChecked = (this.selectedPermissions & permission) == permission;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:permission-" + permissionName.ToLower(), Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(30.0, top, 300.0, elementHeight), null);
			GuiComposerHelpers.AddSwitch(composer, delegate(bool on)
			{
				this.OnPermissionToggled(permission, on);
			}, ElementBounds.Fixed(0.0, top, 20.0, elementHeight), "perm_switch_" + permissionName, 20.0, 4.0);
			GuiComposerHelpers.GetSwitch(composer, "perm_switch_" + permissionName).SetValue(isChecked);
			top += elementHeight + spacing;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0001F12E File Offset: 0x0001D32E
		private void OnPermissionToggled(GuildPermission permission, bool enabled)
		{
			if (enabled)
			{
				this.selectedPermissions |= permission;
				return;
			}
			this.selectedPermissions &= ~permission;
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x0001F154 File Offset: 0x0001D354
		private void OnHierarchyTextChanged(string text)
		{
			int newHierarchy;
			if (int.TryParse(text, out newHierarchy) && newHierarchy >= 1 && newHierarchy <= 998)
			{
				this.selectedHierarchy = newHierarchy;
			}
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0001F180 File Offset: 0x0001D380
		private bool OnCreateClick()
		{
			GuiElementTextInput textInput = GuiComposerHelpers.GetTextInput(base.SingleComposer, "rolename");
			string text;
			if (textInput == null)
			{
				text = null;
			}
			else
			{
				string text2 = textInput.GetText();
				text = ((text2 != null) ? text2.Trim() : null);
			}
			string roleName = text;
			if (string.IsNullOrEmpty(roleName))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:please-enter-role-name", Array.Empty<object>()));
				return false;
			}
			if (roleName.Length < 2)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-name-too-short", Array.Empty<object>()));
				return false;
			}
			if (roleName.Length > 20)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-name-too-long", Array.Empty<object>()));
				return false;
			}
			if (roleName.Equals("Leader", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Member", StringComparison.OrdinalIgnoreCase))
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-name-reserved", Array.Empty<object>()));
				return false;
			}
			Action<string, GuildPermission, int> action = this.onRoleCreated;
			if (action != null)
			{
				action(roleName, this.selectedPermissions, this.selectedHierarchy);
			}
			this.TryClose();
			return true;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0001F283 File Offset: 0x0001D483
		private bool OnCancelClick()
		{
			this.TryClose();
			return true;
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0001F28D File Offset: 0x0001D48D
		private void OnTitleBarCloseClicked()
		{
			this.TryClose();
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000506 RID: 1286 RVA: 0x0001F296 File Offset: 0x0001D496
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040001E0 RID: 480
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001E1 RID: 481
		private Action<string, GuildPermission, int> onRoleCreated;

		// Token: 0x040001E2 RID: 482
		private GuildPermission selectedPermissions;

		// Token: 0x040001E3 RID: 483
		private int selectedHierarchy = 50;
	}
}
