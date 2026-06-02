using SOAGuildsAndKingdoms.src.guilds;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdoms.src.gui
{
    /// <summary>
    /// Dialog for managing guild roles - create, edit, and delete roles
    /// </summary>
    internal class DialogGuildManageRoles : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private GuildSummary? currentGuild;
        private Dictionary<string, GuildRole> roles = new();
        private string? selectedRoleName = null;

        // Pending changes for the selected role
        private GuildPermission pendingPermissions = GuildPermission.None;
        private int pendingHierarchy = 999;
        private bool hasUnsavedChanges = false;

        public DialogGuildManageRoles(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem) : base(capi)
        {
            this.modSystem = modSystem;
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            // Subscribe to guild data updates
            modSystem.OnClientGuildDataUpdated += OnGuildDataUpdated;

            LoadRolesFromGuild();
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildmanageroles";

        private void OnGuildDataUpdated(List<GuildSummary> summaries)
        {
            // Refresh the current guild data
            currentGuild = modSystem.GetCurrentPlayerGuildSummary();

            // Reload roles from the updated guild data
            LoadRolesFromGuild();

            // Refresh the dialog if it's open
            if (IsOpened())
            {
                SetupDialog();
            }
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            // Unsubscribe from guild data updates
            modSystem.OnClientGuildDataUpdated -= OnGuildDataUpdated;
        }

        private void LoadRolesFromGuild()
        {
            // Clear existing roles
            roles.Clear();

            // Load roles from current guild if available
            if (currentGuild != null)
            {
                // Get all roles from the guild summary
                foreach (var roleEntry in currentGuild.Roles)
                {
                    roles[roleEntry.Key] = new GuildRole
                    {
                        Description = roleEntry.Value.Description,
                        Permissions = roleEntry.Value.Permissions,
                        Hierarchy = roleEntry.Value.Hierarchy
                    };
                }

                // Log for debugging
                if (roles.Count > 0)
                {
                    capi.Logger.Debug($"[GuildManageRoles] Loaded {roles.Count} roles from guild '{currentGuild.Name}'");
                    foreach (var role in roles)
                    {
                        capi.Logger.Debug($"  - Role '{role.Key}': {role.Value.Permissions}");
                    }
                }
                else
                {
                    capi.Logger.Warning($"[GuildManageRoles] Guild '{currentGuild.Name}' has no roles defined!");
                }
            }
            else
            {
                capi.Logger.Warning("[GuildManageRoles] No current guild found, using fallback roles");

                // Fallback: Add default roles if no guild is available
                roles["Leader"] = new GuildRole
                {
                    Description = "Guild Leader",
                    Permissions = GuildPermission.Invite | GuildPermission.Promote | GuildPermission.ManageRoles,
                    Hierarchy = 0
                };

                roles["Member"] = new GuildRole
                {
                    Description = "Guild Member",
                    Permissions = GuildPermission.None,
                    Hierarchy = 100
                };
            }
        }

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildmanageroles", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:manage-roles-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 8.0;
            var elementHeight = 25.0;
            var leftColumnWidth = 180.0;
            var rightColumnX = leftColumnWidth + 20.0;
            var rightColumnWidth = 350.0;

            // Instructions
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:manage-roles-instructions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, leftColumnWidth + rightColumnWidth + 20, elementHeight * 2));
            top += elementHeight * 2 + spacing;

            // Two-column layout: Left = Role list, Right = Role editor

            // === LEFT COLUMN: Role List ===
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:existing-roles"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, top, leftColumnWidth, elementHeight));

            var listTop = top + elementHeight + spacing;
            var roleListHeight = 200.0;

            // Create scrollable list of roles
            var roleNames = roles.Keys.OrderBy(name => roles[name].Hierarchy).ToArray();
            var roleListBounds = ElementBounds.Fixed(0, listTop, leftColumnWidth, roleListHeight);

            // Use a simple approach with buttons for each role
            var currentListTop = listTop;
            foreach (var roleName in roleNames)
            {
                bool isDefaultRole = roleName == "Leader" || roleName == "Member";
                var buttonStyle = (selectedRoleName == roleName) ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal;

                var hierarchy = roles[roleName].Hierarchy;
                var roleDisplayName = isDefaultRole
                    ? $"{roleName} [{hierarchy}]"
                    : $"{roleName} [{hierarchy}]";

                composer.AddSmallButton(roleDisplayName, () => OnRoleSelected(roleName),
                    ElementBounds.Fixed(0, currentListTop, leftColumnWidth, elementHeight),
                    buttonStyle, $"role_btn_{roleName}");
                currentListTop += elementHeight + 2;
            }

            // Add new role button
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:create-new-role"), OnCreateNewRole,
                ElementBounds.Fixed(0, listTop + roleListHeight + spacing, leftColumnWidth, elementHeight),
                EnumButtonStyle.Normal);

            // === RIGHT COLUMN: Role Editor ===
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:role-editor"),
                CairoFont.WhiteDetailText(), ElementBounds.Fixed(rightColumnX, top, rightColumnWidth, elementHeight));

            if (selectedRoleName != null)
            {
                var editorTop = top + elementHeight + spacing;

                // Get current role and check if it's a default role
                var currentRole = roles[selectedRoleName];
                var currentPerms = hasUnsavedChanges ? pendingPermissions : currentRole.Permissions;
                bool isDefaultRole = selectedRoleName == "Leader" || selectedRoleName == "Member";

                // Role name (read-only for existing roles)
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:role-name"),
                    CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100, elementHeight));
                composer.AddStaticText(selectedRoleName,
                    CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold),
                    ElementBounds.Fixed(rightColumnX + 110, editorTop, rightColumnWidth - 110, elementHeight));
                editorTop += elementHeight + spacing;

                // Hierarchy editor (if not default role)
                if (!isDefaultRole)
                {
                    composer.AddStaticText(Lang.Get("soaguildsandkingdoms:hierarchy"),
                        CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100, elementHeight));

                    var currentHierarchy = hasUnsavedChanges ? pendingHierarchy : currentRole.Hierarchy;

                    // Up button (move role up in list = decrease hierarchy number = higher authority)
                    composer.AddSmallButton("▲", () => OnHierarchyUp(),
                        ElementBounds.Fixed(rightColumnX + 110, editorTop, 30, elementHeight),
                        EnumButtonStyle.Normal, "hierarchy_up");

                    // Text input for direct hierarchy value entry
                    composer.AddTextInput(ElementBounds.Fixed(rightColumnX + 145, editorTop, 50, elementHeight),
                        (text) => OnHierarchyTextChanged(text),
                        CairoFont.TextInput(), "hierarchy_input");
                    composer.GetTextInput("hierarchy_input").SetValue(currentHierarchy.ToString());

                    // Down button (move role down in list = increase hierarchy number = lower authority)
                    composer.AddSmallButton("▼", () => OnHierarchyDown(),
                        ElementBounds.Fixed(rightColumnX + 200, editorTop, 30, elementHeight),
                        EnumButtonStyle.Normal, "hierarchy_down");

                    // Hierarchy explanation
                    composer.AddStaticText("(1-998)",
                        CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                        ElementBounds.Fixed(rightColumnX + 235, editorTop, 50, elementHeight));

                    editorTop += elementHeight + spacing;
                }
                else
                {
                    // Show hierarchy for default roles but read-only
                    composer.AddStaticText(Lang.Get("soaguildsandkingdoms:hierarchy"),
                        CairoFont.WhiteSmallText(), ElementBounds.Fixed(rightColumnX, editorTop, 100, elementHeight));
                    composer.AddStaticText($"{currentRole.Hierarchy} (Fixed)",
                        CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                        ElementBounds.Fixed(rightColumnX + 110, editorTop, rightColumnWidth - 110, elementHeight));
                    editorTop += elementHeight + spacing;
                }

                // Permissions section
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:permissions"),
                    CairoFont.WhiteDetailText(), ElementBounds.Fixed(rightColumnX, editorTop, rightColumnWidth, elementHeight));
                editorTop += elementHeight + spacing;

                // Permission checkboxes
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "Invite", GuildPermission.Invite, currentPerms, isDefaultRole);
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "Promote", GuildPermission.Promote, currentPerms, isDefaultRole);
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "Kick", GuildPermission.Kick, currentPerms, isDefaultRole);
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "ManageRoles", GuildPermission.ManageRoles, currentPerms, isDefaultRole);
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "BreakAndPlaceBlocks", GuildPermission.BreakAndPlaceBlocks, currentPerms, isDefaultRole);
                AddPermissionCheckbox(composer, ref editorTop, rightColumnX, rightColumnWidth,
                    "InteractBlocks", GuildPermission.InteractBlocks, currentPerms, isDefaultRole);

                editorTop += spacing;

                // Action buttons
                if (!isDefaultRole)
                {
                    // Save button
                    if (hasUnsavedChanges)
                    {
                        composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:save-role"), OnSaveRole,
                            ElementBounds.Fixed(rightColumnX, editorTop, 100, elementHeight),
                            EnumButtonStyle.Normal);

                        composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelChanges,
                            ElementBounds.Fixed(rightColumnX + 110, editorTop, 80, elementHeight),
                            EnumButtonStyle.Normal);
                        editorTop += elementHeight + spacing;
                    }

                    // Delete role button
                    composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:delete-role"), OnDeleteRole,
                        ElementBounds.Fixed(rightColumnX, editorTop, 100, elementHeight),
                        EnumButtonStyle.MainMenu);
                }
                else
                {
                    // Default roles can't be modified
                    composer.AddStaticText(Lang.Get("soaguildsandkingdoms:default-role-notice"),
                        CairoFont.WhiteSmallText().WithColor(new double[] { 1.0, 0.8, 0.2, 1.0 }),
                        ElementBounds.Fixed(rightColumnX, editorTop, rightColumnWidth, elementHeight * 2));
                }
            }
            else
            {
                // No role selected
                var editorTop = top + elementHeight + spacing;
                composer.AddStaticText(Lang.Get("soaguildsandkingdoms:select-role-to-edit"),
                    CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                    ElementBounds.Fixed(rightColumnX, editorTop, rightColumnWidth, elementHeight * 2));
            }

            // Close button at bottom
            var bottomY = Math.Max(listTop + roleListHeight + elementHeight + spacing * 3,
                                    top + elementHeight * 10);
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:close"), OnClose,
                ElementBounds.Fixed(0, bottomY, 100, elementHeight),
                EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private void AddPermissionCheckbox(GuiComposer composer, ref double top, double left, double width,
            string permissionName, GuildPermission permission, GuildPermission currentPerms, bool isReadOnly)
        {
            var elementHeight = 25.0;
            var spacing = 5.0;

            bool isChecked = (currentPerms & permission) == permission;

            composer.AddStaticText(Lang.Get($"soaguildsandkingdoms:permission-{permissionName.ToLower()}"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(left + 30, top, width - 30, elementHeight));

            composer.AddSwitch(
                    (on) => OnPermissionToggled(permission, on),
                    ElementBounds.Fixed(left, top, 20, elementHeight),
                    $"perm_switch_{permissionName}",
                    20);
            composer.GetSwitch($"perm_switch_{permissionName}").SetValue(isChecked);
            composer.GetSwitch($"perm_switch_{permissionName}").Enabled = !isReadOnly;

            top += elementHeight + spacing;
        }

        private void OnPermissionToggled(GuildPermission permission, bool enabled)
        {
            if (selectedRoleName == null) return;

            // Update pending permissions
            if (enabled)
            {
                pendingPermissions |= permission;
            }
            else
            {
                pendingPermissions &= ~permission;
            }

            hasUnsavedChanges = true;
            SetupDialog(); // Refresh to show save button
        }

        private bool OnRoleSelected(string roleName)
        {
            selectedRoleName = roleName;

            // Reset pending changes
            if (roles.ContainsKey(roleName))
            {
                pendingPermissions = roles[roleName].Permissions;
                pendingHierarchy = roles[roleName].Hierarchy;
                hasUnsavedChanges = false;
            }

            SetupDialog(); // Refresh dialog
            return true;
        }

        private bool OnHierarchyUp()
        {
            if (selectedRoleName == null) return false;

            // Get sorted list of roles (by hierarchy, ascending)
            var sortedRoles = roles.OrderBy(r => r.Value.Hierarchy).ToList();

            // Find current role index
            var currentIndex = sortedRoles.FindIndex(r => r.Key == selectedRoleName);
            if (currentIndex <= 0) return false; // Already at the top or not found

            // Get the role above (lower hierarchy value = higher authority)
            var roleAbove = sortedRoles[currentIndex - 1];

            // Don't allow moving above Leader (hierarchy 0)
            if (roleAbove.Value.Hierarchy == 0) return false;

            // Swap hierarchy values
            var tempHierarchy = roles[selectedRoleName].Hierarchy;
            roles[selectedRoleName].Hierarchy = roleAbove.Value.Hierarchy;
            roles[roleAbove.Key].Hierarchy = tempHierarchy;

            // Update pending hierarchy to match
            pendingHierarchy = roles[selectedRoleName].Hierarchy;

            hasUnsavedChanges = true;
            SetupDialog(); // Refresh to show new order

            return true;
        }

        private bool OnHierarchyDown()
        {
            if (selectedRoleName == null) return false;

            // Get sorted list of roles (by hierarchy, ascending)
            var sortedRoles = roles.OrderBy(r => r.Value.Hierarchy).ToList();

            // Find current role index
            var currentIndex = sortedRoles.FindIndex(r => r.Key == selectedRoleName);
            if (currentIndex < 0 || currentIndex >= sortedRoles.Count - 1) return false; // Already at bottom or not found

            // Get the role below (higher hierarchy value = lower authority)
            var roleBelow = sortedRoles[currentIndex + 1];

            // Swap hierarchy values
            var tempHierarchy = roles[selectedRoleName].Hierarchy;
            roles[selectedRoleName].Hierarchy = roleBelow.Value.Hierarchy;
            roles[roleBelow.Key].Hierarchy = tempHierarchy;

            // Update pending hierarchy to match
            pendingHierarchy = roles[selectedRoleName].Hierarchy;

            hasUnsavedChanges = true;
            SetupDialog(); // Refresh to show new order

            return true;
        }

        private void OnHierarchyTextChanged(string text)
        {
            if (selectedRoleName == null) return;

            // Try to parse the input
            if (int.TryParse(text, out int newHierarchy))
            {
                // Validate range (1-998, can't use 0 which is reserved for Leader)
                if (newHierarchy >= 1 && newHierarchy <= 998)
                {
                    // Update local role hierarchy directly
                    roles[selectedRoleName].Hierarchy = newHierarchy;
                    pendingHierarchy = newHierarchy;
                    hasUnsavedChanges = true;

                    // Don't refresh dialog here to avoid interrupting typing
                    // The dialog will refresh when they click save or change selection
                }
            }
        }

        private bool OnCreateNewRole()
        {
            // Open a sub-dialog for creating a new role
            var createRoleDialog = new DialogCreateRole(capi, modSystem, OnRoleCreated);
            createRoleDialog.TryOpen();
            return true;
        }

        private void OnRoleCreated(string roleName, GuildPermission permissions, int hierarchy)
        {
            // Add the new role to our local cache
            roles[roleName] = new GuildRole
            {
                Description = roleName,
                Permissions = permissions,
                Hierarchy = hierarchy
            };

            // Send network packet to server to create the role
            var permString = PermissionToString(permissions);
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler?.SendGuildRoleManagementRequest("create", roleName, null, permString, hierarchy);

            // Select the newly created role
            selectedRoleName = roleName;
            pendingPermissions = permissions;
            pendingHierarchy = hierarchy;
            hasUnsavedChanges = false;

            SetupDialog(); // Refresh dialog
        }

        private bool OnSaveRole()
        {
            if (selectedRoleName == null || !hasUnsavedChanges) return false;

            // When moving roles up/down, we may have changed multiple role hierarchies
            // We need to save all the roles that have been modified
            var networkHandler = modSystem.GetNetworkHandler();

            // For each role that has changed, send an update
            foreach (var roleEntry in roles)
            {
                var roleName = roleEntry.Key;
                var role = roleEntry.Value;

                // Check if this role's hierarchy or permissions differ from the guild's version
                if (currentGuild != null && currentGuild.Roles.TryGetValue(roleName, out var originalRole))
                {
                    bool hierarchyChanged = role.Hierarchy != originalRole.Hierarchy;
                    bool permissionsChanged = role.Permissions != originalRole.Permissions;

                    if (hierarchyChanged || (roleName == selectedRoleName && permissionsChanged))
                    {
                        var permString = PermissionToString(roleName == selectedRoleName ? pendingPermissions : role.Permissions);

                        if (hierarchyChanged)
                        {
                            networkHandler?.SendGuildRoleManagementRequest("update", roleName, null, permString, role.Hierarchy);
                        }
                        else if (permissionsChanged)
                        {
                            networkHandler?.SendGuildRoleManagementRequest("update", roleName, null, permString);
                        }
                    }
                }
            }

            // Update local cache for the selected role
            if (roles.ContainsKey(selectedRoleName))
            {
                roles[selectedRoleName].Permissions = pendingPermissions;
                roles[selectedRoleName].Hierarchy = pendingHierarchy;
            }

            hasUnsavedChanges = false;
            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-saved", selectedRoleName));

            SetupDialog(); // Refresh dialog
            return true;
        }

        private bool OnCancelChanges()
        {
            if (selectedRoleName == null) return false;

            // Reset pending permissions and hierarchy to current role values
            if (roles.ContainsKey(selectedRoleName))
            {
                pendingPermissions = roles[selectedRoleName].Permissions;
                pendingHierarchy = roles[selectedRoleName].Hierarchy;
            }

            hasUnsavedChanges = false;
            SetupDialog(); // Refresh dialog
            return true;
        }

        private bool OnDeleteRole()
        {
            if (selectedRoleName == null) return false;

            // Prevent deleting default roles
            if (selectedRoleName == "Leader" || selectedRoleName == "Member")
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:cannot-delete-default-role"));
                return false;
            }

            // Send network packet to server to delete the role
            var networkHandler = modSystem.GetNetworkHandler();
            networkHandler?.SendGuildRoleManagementRequest("remove", selectedRoleName);

            // Remove from local cache
            roles.Remove(selectedRoleName);
            selectedRoleName = null;
            hasUnsavedChanges = false;

            capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-deleted"));

            SetupDialog(); // Refresh dialog
            return true;
        }

        private bool OnClose()
        {
            if (hasUnsavedChanges)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:unsaved-changes-warning"));
                // In a production system, you'd want a confirmation dialog here
            }

            TryClose();
            return true;
        }

        private void OnTitleBarCloseClicked()
        {
            OnClose();
        }

        public override bool PrefersUngrabbedMouse => false;

        private static string PermissionToString(GuildPermission perms)
        {
            var parts = new List<string>();
            if ((perms & GuildPermission.Invite) != 0) parts.Add("invite");
            if ((perms & GuildPermission.Promote) != 0) parts.Add("promote");
            if ((perms & GuildPermission.Kick) != 0) parts.Add("kick");
            if ((perms & GuildPermission.ManageRoles) != 0) parts.Add("manageroles");
            if ((perms & GuildPermission.BreakAndPlaceBlocks) != 0) parts.Add("breakplaceblocks");
            if ((perms & GuildPermission.InteractBlocks) != 0) parts.Add("interactblocks");

            return parts.Count > 0 ? string.Join(",", parts) : "none";
        }
    }

    /// <summary>
    /// Sub-dialog for creating a new role
    /// </summary>
    internal class DialogCreateRole : GuiDialog
    {
        private SOAGuildsAndKingdomsModSystem modSystem;
        private Action<string, GuildPermission, int> onRoleCreated;
        private GuildPermission selectedPermissions = GuildPermission.None;
        private int selectedHierarchy = 50; // Default to mid-level hierarchy

        public DialogCreateRole(ICoreClientAPI capi, SOAGuildsAndKingdomsModSystem modSystem,
            Action<string, GuildPermission, int> onRoleCreated) : base(capi)
        {
            this.modSystem = modSystem;
            this.onRoleCreated = onRoleCreated;
            SetupDialog();
        }

        public override string ToggleKeyCombinationCode => "guildcreaterole";

        private void SetupDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("guildcreaterole", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Lang.Get("soaguildsandkingdoms:create-new-role-title"), OnTitleBarCloseClicked)
                .BeginChildElements(bgBounds);

            var top = 20.0;
            var spacing = 15.0;
            var elementHeight = 35.0;
            var width = 350.0;

            // Instructions
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:create-role-instructions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, width, elementHeight));
            top += elementHeight + spacing;

            // Role name input
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:role-name"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));
            composer.AddTextInput(ElementBounds.Fixed(110, top, 200, elementHeight), null,
                CairoFont.TextInput(), "rolename");
            top += elementHeight + spacing * 1.5;

            // Hierarchy input
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:hierarchy"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, 100, elementHeight));

            // Up button (decrease hierarchy number = higher authority)
            composer.AddSmallButton("▲", () => { selectedHierarchy = Math.Max(1, selectedHierarchy - 1); SetupDialog(); return true; },
                ElementBounds.Fixed(110, top, 30, elementHeight),
                EnumButtonStyle.Normal, "hierarchy_up");

            // Text input for direct hierarchy value entry
            composer.AddTextInput(ElementBounds.Fixed(145, top, 50, elementHeight),
                (text) => OnHierarchyTextChanged(text),
                CairoFont.TextInput(), "hierarchy_input");
            composer.GetTextInput("hierarchy_input").SetValue(selectedHierarchy.ToString());

            // Down button (increase hierarchy number = lower authority)
            composer.AddSmallButton("▼", () => { selectedHierarchy = Math.Min(998, selectedHierarchy + 1); SetupDialog(); return true; },
                ElementBounds.Fixed(200, top, 30, elementHeight),
                EnumButtonStyle.Normal, "hierarchy_down");

            // Hierarchy explanation
            composer.AddStaticText("(1-998)",
                CairoFont.WhiteSmallText().WithColor(new double[] { 0.7, 0.7, 0.7, 1.0 }),
                ElementBounds.Fixed(235, top, 115, elementHeight));

            top += elementHeight + spacing * 1.5;

            // Permissions section
            composer.AddStaticText(Lang.Get("soaguildsandkingdoms:permissions"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(0, top, width, elementHeight));
            top += elementHeight + spacing;

            // Permission checkboxes
            AddPermissionCheckbox(composer, ref top, "Invite", GuildPermission.Invite);
            AddPermissionCheckbox(composer, ref top, "Promote", GuildPermission.Promote);
            AddPermissionCheckbox(composer, ref top, "Kick", GuildPermission.Kick);
            AddPermissionCheckbox(composer, ref top, "ManageRoles", GuildPermission.ManageRoles);
            AddPermissionCheckbox(composer, ref top, "BreakAndPlaceBlocks", GuildPermission.BreakAndPlaceBlocks);
            AddPermissionCheckbox(composer, ref top, "InteractBlocks", GuildPermission.InteractBlocks);

            top += spacing;

            // Buttons
            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:create"), OnCreateClick,
                ElementBounds.Fixed(0, top, 80, elementHeight), EnumButtonStyle.Normal);

            composer.AddSmallButton(Lang.Get("soaguildsandkingdoms:cancel"), OnCancelClick,
                ElementBounds.Fixed(90, top, 80, elementHeight), EnumButtonStyle.Normal);

            SingleComposer = composer.Compose();
        }

        private void AddPermissionCheckbox(GuiComposer composer, ref double top, string permissionName, GuildPermission permission)
        {
            var elementHeight = 25.0;
            var spacing = 5.0;

            bool isChecked = (selectedPermissions & permission) == permission;

            composer.AddStaticText(Lang.Get($"soaguildsandkingdoms:permission-{permissionName.ToLower()}"),
                CairoFont.WhiteSmallText(), ElementBounds.Fixed(30, top, 300, elementHeight));

            composer.AddSwitch(
                (on) => OnPermissionToggled(permission, on),
                ElementBounds.Fixed(0, top, 20, elementHeight),
                $"perm_switch_{permissionName}",
                20);
            composer.GetSwitch($"perm_switch_{permissionName}").SetValue(isChecked);

            top += elementHeight + spacing;
        }

        private void OnPermissionToggled(GuildPermission permission, bool enabled)
        {
            if (enabled)
            {
                selectedPermissions |= permission;
            }
            else
            {
                selectedPermissions &= ~permission;
            }
        }

        private void OnHierarchyTextChanged(string text)
        {
            // Try to parse the input
            if (int.TryParse(text, out int newHierarchy))
            {
                // Validate range (1-998, can't use 0 which is reserved for Leader)
                if (newHierarchy >= 1 && newHierarchy <= 998)
                {
                    selectedHierarchy = newHierarchy;
                    // Don't refresh dialog here to avoid interrupting typing
                }
            }
        }

        private bool OnCreateClick()
        {
            var roleNameInput = SingleComposer.GetTextInput("rolename");
            var roleName = roleNameInput?.GetText()?.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:please-enter-role-name"));
                return false;
            }

            // Validate role name
            if (roleName.Length < 2)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-name-too-short"));
                return false;
            }

            if (roleName.Length > 20)
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-name-too-long"));
                return false;
            }

            // Prevent creating default role names
            if (roleName.Equals("Leader", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("Member", StringComparison.OrdinalIgnoreCase))
            {
                capi.ShowChatMessage(Lang.Get("soaguildsandkingdoms:role-name-reserved"));
                return false;
            }

            // Call the callback
            onRoleCreated?.Invoke(roleName, selectedPermissions, selectedHierarchy);

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
