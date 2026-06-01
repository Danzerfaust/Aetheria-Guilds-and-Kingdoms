using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x02000077 RID: 119
	[NullableContext(1)]
	[Nullable(0)]
	internal class DialogGuildManageRoles : GuiDialog
	{
		// Token: 0x060004E2 RID: 1250 RVA: 0x0001D79C File Offset: 0x0001B99C
		public DialogGuildManageRoles(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem) : base(capi)
		{
			this.modSystem = modSystem;
			this.currentGuild = modSystem.GetCurrentPlayerGuildSummary();
			modSystem.OnClientGuildDataUpdated += this.OnGuildDataUpdated;
			this.LoadRolesFromGuild();
			this.SetupDialog();
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x0001D7F7 File Offset: 0x0001B9F7
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return "guildmanageroles";
			}
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0001D7FE File Offset: 0x0001B9FE
		private void OnGuildDataUpdated(List<GuildSummary> summaries)
		{
			this.currentGuild = this.modSystem.GetCurrentPlayerGuildSummary();
			this.LoadRolesFromGuild();
			if (this.IsOpened())
			{
				this.SetupDialog();
			}
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x0001D825 File Offset: 0x0001BA25
		public override void OnGuiClosed()
		{
			base.OnGuiClosed();
			this.modSystem.OnClientGuildDataUpdated -= this.OnGuildDataUpdated;
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0001D844 File Offset: 0x0001BA44
		private void LoadRolesFromGuild()
		{
			this.roles.Clear();
			if (this.currentGuild != null)
			{
				foreach (KeyValuePair<string, GuildRole> roleEntry in this.currentGuild.Roles)
				{
					this.roles[roleEntry.Key] = new GuildRole
					{
						Description = roleEntry.Value.Description,
						Permissions = roleEntry.Value.Permissions,
						Hierarchy = roleEntry.Value.Hierarchy
					};
				}
				if (this.roles.Count > 0)
				{
					ILogger logger = this.capi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[GuildManageRoles] Loaded ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.roles.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" roles from guild '");
					defaultInterpolatedStringHandler.AppendFormatted(this.currentGuild.Name);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					using (Dictionary<string, GuildRole>.Enumerator enumerator = this.roles.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, GuildRole> role = enumerator.Current;
							ILogger logger2 = this.capi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("  - Role '");
							defaultInterpolatedStringHandler2.AppendFormatted(role.Key);
							defaultInterpolatedStringHandler2.AppendLiteral("': ");
							defaultInterpolatedStringHandler2.AppendFormatted<GuildPermission>(role.Value.Permissions);
							logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
						}
						return;
					}
				}
				this.capi.Logger.Warning("[GuildManageRoles] Guild '" + this.currentGuild.Name + "' has no roles defined!");
				return;
			}
			this.capi.Logger.Warning("[GuildManageRoles] No current guild found, using fallback roles");
			this.roles["Leader"] = new GuildRole
			{
				Description = "Guild Leader",
				Permissions = (GuildPermission.Invite | GuildPermission.Promote | GuildPermission.ManageRoles),
				Hierarchy = 0
			};
			this.roles["Member"] = new GuildRole
			{
				Description = "Guild Member",
				Permissions = GuildPermission.None,
				Hierarchy = 100
			};
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0001DAA8 File Offset: 0x0001BCA8
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			GuiComposer composer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("guildmanageroles", dialogBounds), bgBounds, true, 5.0, 0.75f), Lang.Get("srguildsandkingdoms:manage-roles-title", Array.Empty<object>()), new Action(this.OnTitleBarCloseClicked), null, null, null).BeginChildElements(bgBounds);
			double top = 20.0;
			double spacing = 8.0;
			double elementHeight = 25.0;
			double leftColumnWidth = 180.0;
			double rightColumnX = leftColumnWidth + 20.0;
			double rightColumnWidth = 350.0;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:manage-roles-instructions", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, leftColumnWidth + rightColumnWidth + 20.0, elementHeight * 2.0), null);
			top += elementHeight * 2.0 + spacing;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:existing-roles", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(0.0, top, leftColumnWidth, elementHeight), null);
			double listTop = top + elementHeight + spacing;
			double roleListHeight = 200.0;
			string[] array = (from name in this.roles.Keys
			orderby this.roles[name].Hierarchy
			select name).ToArray<string>();
			ElementBounds.Fixed(0.0, listTop, leftColumnWidth, roleListHeight);
			double currentListTop = listTop;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string roleName = array2[i];
				bool flag = roleName == "Leader" || roleName == "Member";
				EnumButtonStyle buttonStyle = (this.selectedRoleName == roleName) ? 1 : 2;
				int hierarchy = this.roles[roleName].Hierarchy;
				string text2;
				if (!flag)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler.AppendFormatted(roleName);
					defaultInterpolatedStringHandler.AppendLiteral(" [");
					defaultInterpolatedStringHandler.AppendFormatted<int>(hierarchy);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					text2 = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler2.AppendFormatted(roleName);
					defaultInterpolatedStringHandler2.AppendLiteral(" [");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(hierarchy);
					defaultInterpolatedStringHandler2.AppendLiteral("]");
					text2 = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				string roleDisplayName = text2;
				GuiComposerHelpers.AddSmallButton(composer, roleDisplayName, () => this.OnRoleSelected(roleName), ElementBounds.Fixed(0.0, currentListTop, leftColumnWidth, elementHeight), buttonStyle, "role_btn_" + roleName);
				currentListTop += elementHeight + 2.0;
			}
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:create-new-role", Array.Empty<object>()), new ActionConsumable(this.OnCreateNewRole), ElementBounds.Fixed(0.0, listTop + roleListHeight + spacing, leftColumnWidth, elementHeight), 2, null);
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:role-editor", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(rightColumnX, top, rightColumnWidth, elementHeight), null);
			if (this.selectedRoleName != null)
			{
				double editorTop = top + elementHeight + spacing;
				GuildRole currentRole = this.roles[this.selectedRoleName];
				GuildPermission currentPerms = this.hasUnsavedChanges ? this.pendingPermissions : currentRole.Permissions;
				bool isDefaultRole = this.selectedRoleName == "Leader" || this.selectedRoleName == "Member";
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:role-name", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100.0, elementHeight), null);
				GuiComposerHelpers.AddStaticText(composer, this.selectedRoleName, CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(rightColumnX + 110.0, editorTop, rightColumnWidth - 110.0, elementHeight), null);
				editorTop += elementHeight + spacing;
				if (!isDefaultRole)
				{
					GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:hierarchy", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100.0, elementHeight), null);
					int currentHierarchy = this.hasUnsavedChanges ? this.pendingHierarchy : currentRole.Hierarchy;
					GuiComposerHelpers.AddSmallButton(composer, "▲", () => this.OnHierarchyUp(), ElementBounds.Fixed(rightColumnX + 110.0, editorTop, 30.0, elementHeight), 2, "hierarchy_up");
					GuiComposerHelpers.AddTextInput(composer, ElementBounds.Fixed(rightColumnX + 145.0, editorTop, 50.0, elementHeight), delegate(string text)
					{
						this.OnHierarchyTextChanged(text);
					}, CairoFont.TextInput(), "hierarchy_input");
					GuiComposerHelpers.GetTextInput(composer, "hierarchy_input").SetValue(currentHierarchy.ToString(), true);
					GuiComposerHelpers.AddSmallButton(composer, "▼", () => this.OnHierarchyDown(), ElementBounds.Fixed(rightColumnX + 200.0, editorTop, 30.0, elementHeight), 2, "hierarchy_down");
					GuiComposerHelpers.AddStaticText(composer, "(1-998)", CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.7,
						0.7,
						0.7,
						1.0
					}), ElementBounds.Fixed(rightColumnX + 235.0, editorTop, 50.0, elementHeight), null);
					editorTop += elementHeight + spacing;
				}
				else
				{
					GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:hierarchy", Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100.0, elementHeight), null);
					GuiComposer guiComposer = composer;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(8, 1);
					defaultInterpolatedStringHandler3.AppendFormatted<int>(currentRole.Hierarchy);
					defaultInterpolatedStringHandler3.AppendLiteral(" (Fixed)");
					GuiComposerHelpers.AddStaticText(guiComposer, defaultInterpolatedStringHandler3.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.7,
						0.7,
						0.7,
						1.0
					}), ElementBounds.Fixed(rightColumnX + 110.0, editorTop, rightColumnWidth - 110.0, elementHeight), null);
					editorTop += elementHeight + spacing;
				}
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:permissions", Array.Empty<object>()), CairoFont.WhiteDetailText(), ElementBounds.Fixed(rightColumnX, editorTop, rightColumnWidth, elementHeight), null);
				editorTop += elementHeight + spacing;
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "Invite", GuildPermission.Invite, currentPerms, isDefaultRole);
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "Promote", GuildPermission.Promote, currentPerms, isDefaultRole);
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "Kick", GuildPermission.Kick, currentPerms, isDefaultRole);
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "ManageRoles", GuildPermission.ManageRoles, currentPerms, isDefaultRole);
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "BreakAndPlaceBlocks", GuildPermission.BreakAndPlaceBlocks, currentPerms, isDefaultRole);
				this.AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth, "InteractBlocks", GuildPermission.InteractBlocks, currentPerms, isDefaultRole);
				editorTop += spacing;
				if (!isDefaultRole)
				{
					if (this.hasUnsavedChanges)
					{
						GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:save-role", Array.Empty<object>()), new ActionConsumable(this.OnSaveRole), ElementBounds.Fixed(rightColumnX, editorTop, 100.0, elementHeight), 2, null);
						GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:cancel", Array.Empty<object>()), new ActionConsumable(this.OnCancelChanges), ElementBounds.Fixed(rightColumnX + 110.0, editorTop, 80.0, elementHeight), 2, null);
						editorTop += elementHeight + spacing;
					}
					GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:delete-role", Array.Empty<object>()), new ActionConsumable(this.OnDeleteRole), ElementBounds.Fixed(rightColumnX, editorTop, 100.0, elementHeight), 1, null);
				}
				else
				{
					GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:default-role-notice", Array.Empty<object>()), CairoFont.WhiteSmallText().WithColor(new double[]
					{
						1.0,
						0.8,
						0.2,
						1.0
					}), ElementBounds.Fixed(rightColumnX, editorTop, rightColumnWidth, elementHeight * 2.0), null);
				}
			}
			else
			{
				double editorTop2 = top + elementHeight + spacing;
				GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:select-role-to-edit", Array.Empty<object>()), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(rightColumnX, editorTop2, rightColumnWidth, elementHeight * 2.0), null);
			}
			double bottomY = Math.Max(listTop + roleListHeight + elementHeight + spacing * 3.0, top + elementHeight * 10.0);
			GuiComposerHelpers.AddSmallButton(composer, Lang.Get("srguildsandkingdoms:close", Array.Empty<object>()), new ActionConsumable(this.OnClose), ElementBounds.Fixed(0.0, bottomY, 100.0, elementHeight), 2, null);
			base.SingleComposer = composer.Compose(true);
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0001E3B0 File Offset: 0x0001C5B0
		private void AddPermissionCheckbox(GuiComposer composer, ref double top, double left, double width, string permissionName, GuildPermission permission, GuildPermission currentPerms, bool isReadOnly)
		{
			double elementHeight = 25.0;
			double spacing = 5.0;
			bool isChecked = (currentPerms & permission) == permission;
			GuiComposerHelpers.AddStaticText(composer, Lang.Get("srguildsandkingdoms:permission-" + permissionName.ToLower(), Array.Empty<object>()), CairoFont.WhiteSmallText(), ElementBounds.Fixed(left + 30.0, top, width - 30.0, elementHeight), null);
			GuiComposerHelpers.AddSwitch(composer, delegate(bool on)
			{
				this.OnPermissionToggled(permission, on);
			}, ElementBounds.Fixed(left, top, 20.0, elementHeight), "perm_switch_" + permissionName, 20.0, 4.0);
			GuiComposerHelpers.GetSwitch(composer, "perm_switch_" + permissionName).SetValue(isChecked);
			GuiComposerHelpers.GetSwitch(composer, "perm_switch_" + permissionName).Enabled = !isReadOnly;
			top += elementHeight + spacing;
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0001E4BE File Offset: 0x0001C6BE
		private void OnPermissionToggled(GuildPermission permission, bool enabled)
		{
			if (this.selectedRoleName == null)
			{
				return;
			}
			if (enabled)
			{
				this.pendingPermissions |= permission;
			}
			else
			{
				this.pendingPermissions &= ~permission;
			}
			this.hasUnsavedChanges = true;
			this.SetupDialog();
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0001E4F8 File Offset: 0x0001C6F8
		private bool OnRoleSelected(string roleName)
		{
			this.selectedRoleName = roleName;
			if (this.roles.ContainsKey(roleName))
			{
				this.pendingPermissions = this.roles[roleName].Permissions;
				this.pendingHierarchy = this.roles[roleName].Hierarchy;
				this.hasUnsavedChanges = false;
			}
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0001E558 File Offset: 0x0001C758
		private bool OnHierarchyUp()
		{
			if (this.selectedRoleName == null)
			{
				return false;
			}
			List<KeyValuePair<string, GuildRole>> sortedRoles = (from r in this.roles
			orderby r.Value.Hierarchy
			select r).ToList<KeyValuePair<string, GuildRole>>();
			int currentIndex = sortedRoles.FindIndex((KeyValuePair<string, GuildRole> r) => r.Key == this.selectedRoleName);
			if (currentIndex <= 0)
			{
				return false;
			}
			KeyValuePair<string, GuildRole> roleAbove = sortedRoles[currentIndex - 1];
			if (roleAbove.Value.Hierarchy == 0)
			{
				return false;
			}
			int tempHierarchy = this.roles[this.selectedRoleName].Hierarchy;
			this.roles[this.selectedRoleName].Hierarchy = roleAbove.Value.Hierarchy;
			this.roles[roleAbove.Key].Hierarchy = tempHierarchy;
			this.pendingHierarchy = this.roles[this.selectedRoleName].Hierarchy;
			this.hasUnsavedChanges = true;
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x0001E650 File Offset: 0x0001C850
		private bool OnHierarchyDown()
		{
			if (this.selectedRoleName == null)
			{
				return false;
			}
			List<KeyValuePair<string, GuildRole>> sortedRoles = (from r in this.roles
			orderby r.Value.Hierarchy
			select r).ToList<KeyValuePair<string, GuildRole>>();
			int currentIndex = sortedRoles.FindIndex((KeyValuePair<string, GuildRole> r) => r.Key == this.selectedRoleName);
			if (currentIndex < 0 || currentIndex >= sortedRoles.Count - 1)
			{
				return false;
			}
			KeyValuePair<string, GuildRole> roleBelow = sortedRoles[currentIndex + 1];
			int tempHierarchy = this.roles[this.selectedRoleName].Hierarchy;
			this.roles[this.selectedRoleName].Hierarchy = roleBelow.Value.Hierarchy;
			this.roles[roleBelow.Key].Hierarchy = tempHierarchy;
			this.pendingHierarchy = this.roles[this.selectedRoleName].Hierarchy;
			this.hasUnsavedChanges = true;
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0001E740 File Offset: 0x0001C940
		private void OnHierarchyTextChanged(string text)
		{
			if (this.selectedRoleName == null)
			{
				return;
			}
			int newHierarchy;
			if (int.TryParse(text, out newHierarchy) && newHierarchy >= 1 && newHierarchy <= 998)
			{
				this.roles[this.selectedRoleName].Hierarchy = newHierarchy;
				this.pendingHierarchy = newHierarchy;
				this.hasUnsavedChanges = true;
			}
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0001E791 File Offset: 0x0001C991
		private bool OnCreateNewRole()
		{
			new DialogCreateRole(this.capi, this.modSystem, new Action<string, GuildPermission, int>(this.OnRoleCreated)).TryOpen();
			return true;
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0001E7B8 File Offset: 0x0001C9B8
		private void OnRoleCreated(string roleName, GuildPermission permissions, int hierarchy)
		{
			this.roles[roleName] = new GuildRole
			{
				Description = roleName,
				Permissions = permissions,
				Hierarchy = hierarchy
			};
			string permString = DialogGuildManageRoles.PermissionToString(permissions);
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildRoleManagementRequest("create", roleName, null, permString, hierarchy);
			}
			this.selectedRoleName = roleName;
			this.pendingPermissions = permissions;
			this.pendingHierarchy = hierarchy;
			this.hasUnsavedChanges = false;
			this.SetupDialog();
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0001E834 File Offset: 0x0001CA34
		private bool OnSaveRole()
		{
			if (this.selectedRoleName == null || !this.hasUnsavedChanges)
			{
				return false;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			foreach (KeyValuePair<string, GuildRole> roleEntry in this.roles)
			{
				string roleName = roleEntry.Key;
				GuildRole role = roleEntry.Value;
				GuildRole originalRole;
				if (this.currentGuild != null && this.currentGuild.Roles.TryGetValue(roleName, out originalRole))
				{
					bool hierarchyChanged = role.Hierarchy != originalRole.Hierarchy;
					bool permissionsChanged = role.Permissions != originalRole.Permissions;
					if (hierarchyChanged || (roleName == this.selectedRoleName && permissionsChanged))
					{
						string permString = DialogGuildManageRoles.PermissionToString((roleName == this.selectedRoleName) ? this.pendingPermissions : role.Permissions);
						if (hierarchyChanged)
						{
							if (networkHandler != null)
							{
								networkHandler.SendGuildRoleManagementRequest("update", roleName, null, permString, role.Hierarchy);
							}
						}
						else if (permissionsChanged && networkHandler != null)
						{
							networkHandler.SendGuildRoleManagementRequest("update", roleName, null, permString, 999);
						}
					}
				}
			}
			if (this.roles.ContainsKey(this.selectedRoleName))
			{
				this.roles[this.selectedRoleName].Permissions = this.pendingPermissions;
				this.roles[this.selectedRoleName].Hierarchy = this.pendingHierarchy;
			}
			this.hasUnsavedChanges = false;
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-saved", new object[]
			{
				this.selectedRoleName
			}));
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0001E9F4 File Offset: 0x0001CBF4
		private bool OnCancelChanges()
		{
			if (this.selectedRoleName == null)
			{
				return false;
			}
			if (this.roles.ContainsKey(this.selectedRoleName))
			{
				this.pendingPermissions = this.roles[this.selectedRoleName].Permissions;
				this.pendingHierarchy = this.roles[this.selectedRoleName].Hierarchy;
			}
			this.hasUnsavedChanges = false;
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0001EA64 File Offset: 0x0001CC64
		private bool OnDeleteRole()
		{
			if (this.selectedRoleName == null)
			{
				return false;
			}
			if (this.selectedRoleName == "Leader" || this.selectedRoleName == "Member")
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:cannot-delete-default-role", Array.Empty<object>()));
				return false;
			}
			GuildNetworkHandler networkHandler = this.modSystem.GetNetworkHandler();
			if (networkHandler != null)
			{
				networkHandler.SendGuildRoleManagementRequest("remove", this.selectedRoleName, null, null, 999);
			}
			this.roles.Remove(this.selectedRoleName);
			this.selectedRoleName = null;
			this.hasUnsavedChanges = false;
			this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:role-deleted", Array.Empty<object>()));
			this.SetupDialog();
			return true;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0001EB24 File Offset: 0x0001CD24
		private bool OnClose()
		{
			if (this.hasUnsavedChanges)
			{
				this.capi.ShowChatMessage(Lang.Get("srguildsandkingdoms:unsaved-changes-warning", Array.Empty<object>()));
			}
			this.TryClose();
			return true;
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0001EB50 File Offset: 0x0001CD50
		private void OnTitleBarCloseClicked()
		{
			this.OnClose();
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060004F5 RID: 1269 RVA: 0x0001EB59 File Offset: 0x0001CD59
		public override bool PrefersUngrabbedMouse
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0001EB5C File Offset: 0x0001CD5C
		private static string PermissionToString(GuildPermission perms)
		{
			List<string> parts = new List<string>();
			if ((perms & GuildPermission.Invite) != GuildPermission.None)
			{
				parts.Add("invite");
			}
			if ((perms & GuildPermission.Promote) != GuildPermission.None)
			{
				parts.Add("promote");
			}
			if ((perms & GuildPermission.Kick) != GuildPermission.None)
			{
				parts.Add("kick");
			}
			if ((perms & GuildPermission.ManageRoles) != GuildPermission.None)
			{
				parts.Add("manageroles");
			}
			if ((perms & GuildPermission.BreakAndPlaceBlocks) != GuildPermission.None)
			{
				parts.Add("breakplaceblocks");
			}
			if ((perms & GuildPermission.InteractBlocks) != GuildPermission.None)
			{
				parts.Add("interactblocks");
			}
			if (parts.Count <= 0)
			{
				return "none";
			}
			return string.Join(",", parts);
		}

		// Token: 0x040001D9 RID: 473
		private SRGuildsAndKingdomsModSystem modSystem;

		// Token: 0x040001DA RID: 474
		[Nullable(2)]
		private GuildSummary currentGuild;

		// Token: 0x040001DB RID: 475
		private Dictionary<string, GuildRole> roles = new Dictionary<string, GuildRole>();

		// Token: 0x040001DC RID: 476
		[Nullable(2)]
		private string selectedRoleName;

		// Token: 0x040001DD RID: 477
		private GuildPermission pendingPermissions;

		// Token: 0x040001DE RID: 478
		private int pendingHierarchy = 999;

		// Token: 0x040001DF RID: 479
		private bool hasUnsavedChanges;
	}
}
