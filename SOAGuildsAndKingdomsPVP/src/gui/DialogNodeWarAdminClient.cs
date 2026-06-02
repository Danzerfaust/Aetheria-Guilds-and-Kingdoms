using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using SOAGuildsAndKingdomsPVP.src.network;

namespace SOAGuildsAndKingdomsPVP.src.gui
{
    /// <summary>
    /// Client-side Node War Admin UI that works via network packets
    /// </summary>
    public class DialogNodeWarAdminClient : GuiDialog
    {
        private readonly PVPNetworkHandler networkHandler;
        
        private int currentTab = 0;
        private const int TAB_NODES = 0;
        private const int TAB_WARS = 1;
        private const int TAB_ZONES = 2;

        private string? selectedNodeId;
        private NodeWarAdminDataPacket? currentData;

        public override string ToggleKeyCombinationCode => "nodewaradmin";

        public DialogNodeWarAdminClient(ICoreClientAPI capi, PVPNetworkHandler handler) : base(capi)
        {
            networkHandler = handler;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            // Request data from server
            networkHandler.RequestNodeWarAdminData();
        }

        /// <summary>
        /// Called when data is received from server
        /// </summary>
        public void UpdateData(NodeWarAdminDataPacket data)
        {
            currentData = data;
            SetupDialog();
        }

        private void SetupDialog()
        {
            if (currentData == null)
            {
                // Show loading message
                ShowLoadingDialog();
                return;
            }

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithAlignment(EnumDialogArea.CenterMiddle)
                .WithFixedAlignmentOffset(0, 0);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("nodewaradmin", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Node War Administration", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            var top = 30.0;
            var tabHeight = 30.0;
            var tabWidth = 150.0;

            // Add tab buttons
            composer.AddSmallButton("Manage Nodes", OnNodesTab,
                ElementBounds.Fixed(10, top, tabWidth, tabHeight),
                currentTab == TAB_NODES ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Manage Wars", OnWarsTab,
                ElementBounds.Fixed(tabWidth + 15, top, tabWidth, tabHeight),
                currentTab == TAB_WARS ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            composer.AddSmallButton("Capture Zones", OnZonesTab,
                ElementBounds.Fixed((tabWidth + 5) * 2 + 10, top, tabWidth, tabHeight),
                currentTab == TAB_ZONES ? EnumButtonStyle.MainMenu : EnumButtonStyle.Normal);

            top += tabHeight + 15;

            // Add tab content
            switch (currentTab)
            {
                case TAB_NODES:
                    AddNodesTabContent(composer, ref top);
                    break;
                case TAB_WARS:
                    AddWarsTabContent(composer, ref top);
                    break;
                case TAB_ZONES:
                    AddZonesTabContent(composer, ref top);
                    break;
            }

            SingleComposer = composer.Compose();
        }

        private void ShowLoadingDialog()
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithAlignment(EnumDialogArea.CenterMiddle);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            var composer = capi.Gui.CreateCompo("nodewaradmin", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Node War Administration", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            composer.AddStaticText("Loading data from server...", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(10, 40, 300, 50));

            SingleComposer = composer.Compose();
        }

        #region Nodes Tab

        private void AddNodesTabContent(GuiComposer composer, ref double top)
        {
            if (currentData == null) return;

            var leftCol = 10.0;
            var rightCol = 320.0;

            // Left side - Node List
            composer.AddStaticText("Registered Nodes:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(leftCol, top, 300, 25));

            composer.AddStaticText($"Total: {currentData.Nodes.Count}", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(leftCol + 200, top + 5, 150, 25));

            top += 30;

            if (currentData.Nodes.Count == 0)
            {
                composer.AddStaticText("No nodes registered.\nUse 'Register New Node' to create one.",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 50));
            }
            else
            {
                // Build node list with war status from node data
                var nodeList = new List<string>();
                foreach (var node in currentData.Nodes)
                {
                    string status = node.WarStatus switch
                    {
                        0 => "✓ Available",  // NodeWarStatus.None
                        1 => "📅 SCHEDULED", // NodeWarStatus.Scheduled
                        2 => "⚔ ACTIVE",    // NodeWarStatus.Active
                        3 => "✓ COMPLETED", // NodeWarStatus.Completed
                        4 => "✗ CANCELLED", // NodeWarStatus.Cancelled
                        _ => "✓ Available"
                    };
                    nodeList.Add($"{node.NodeName} ({node.NodeId}) - {status}");
                }

                composer.AddDropDown(currentData.Nodes.Select(n => n.NodeId).ToArray(),
                    nodeList.ToArray(),
                    selectedNodeId != null ? Array.FindIndex(currentData.Nodes.ToArray(), n => n.NodeId == selectedNodeId) : 0,
                    OnNodeSelected,
                    ElementBounds.Fixed(leftCol, top, 300, 25),
                    "nodeSelector");

                top += 35;

                // Selected node details
                if (selectedNodeId != null)
                {
                    var selectedNode = currentData.Nodes.FirstOrDefault(n => n.NodeId == selectedNodeId);
                    if (selectedNode != null)
                    {
                        composer.AddStaticText("Selected Node Details:", CairoFont.WhiteSmallText(),
                            ElementBounds.Fixed(leftCol, top, 300, 20));
                        top += 25;

                        var details = $"Name: {selectedNode.NodeName}\n" +
                                    $"ID: {selectedNode.NodeId}\n" +
                                    $"Location: X:{selectedNode.CenterX:F0}, Y:{selectedNode.CenterY:F0}, Z:{selectedNode.CenterZ:F0}\n" +
                                    $"Radius: {selectedNode.Radius} blocks\n" +
                                    $"Owner: {selectedNode.OwningGuildName ?? "None"}\n" +
                                    $"Active: {(selectedNode.IsActive ? "Yes" : "No")}\n" +
                                    $"Capture Zones: {selectedNode.CaptureZones.Count}";

                        // Add war status info if available
                        if (selectedNode.WarStatus.HasValue)
                        {
                            details += $"\n\nWar Status: {GetWarStatusName(selectedNode.WarStatus.Value)}";

                            if (selectedNode.WarStatus == 1) // Scheduled
                            {
                                details += $"\nSignups: {selectedNode.WarSignupCount ?? 0}/{(selectedNode.WarMaxGuilds > 0 ? selectedNode.WarMaxGuilds.ToString() : "∞")}";
                            }
                            else if (selectedNode.WarStatus == 3 && !string.IsNullOrEmpty(selectedNode.WarWinnerGuildName)) // Completed
                            {
                                details += $"\nWinner: {selectedNode.WarWinnerGuildName}";
                            }
                        }

                        composer.AddStaticText(details, CairoFont.WhiteSmallText(),
                            ElementBounds.Fixed(leftCol, top, 300, 200));
                        top += 210;
                    }
                }
            }

            // Right side - Node Actions
            top = 75; // Reset to top for right column
            
            composer.AddStaticText("Node Actions:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(rightCol, top, 280, 25));
            top += 35;

            // Register Node section
            composer.AddStaticText("Register New Node:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 280, 20));
            top += 25;

            composer.AddStaticText("Node ID:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "nodeId");
            top += 30;

            composer.AddStaticText("Name:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "nodeName");
            top += 30;

            composer.AddStaticText("Radius:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "nodeRadius");
            top += 30;

            composer.AddSmallButton("Register at Current Position", OnRegisterNode,
                ElementBounds.Fixed(rightCol, top, 280, 25));
            top += 35;

            // Update/Unregister selected node
            if (selectedNodeId != null)
            {
                composer.AddSmallButton("Update Selected Node Position", OnUpdateNode,
                    ElementBounds.Fixed(rightCol, top, 280, 25));
                top += 30;

                composer.AddSmallButton("Unregister Selected Node", OnUnregisterNode,
                    ElementBounds.Fixed(rightCol, top, 280, 25),
                    EnumButtonStyle.Normal, "unregisterBtn");
                top += 30;
            }
        }

        private void OnNodeSelected(string code, bool selected)
        {
            selectedNodeId = code;
            SetupDialog();
        }

        private bool OnRegisterNode()
        {
            var nodeId = SingleComposer.GetTextInput("nodeId")?.GetText();
            var nodeName = SingleComposer.GetTextInput("nodeName")?.GetText();
            var radiusText = SingleComposer.GetTextInput("nodeRadius")?.GetText();

            if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(nodeName))
            {
                capi.ShowChatMessage("Please enter both Node ID and Name");
                return true;
            }

            if (!int.TryParse(radiusText, out int radius) || radius <= 0)
            {
                capi.ShowChatMessage("Please enter a valid radius (positive integer)");
                return true;
            }

            var player = capi.World.Player;
            var pos = player.Entity.Pos.XYZ;

            networkHandler.RequestRegisterNode(nodeId, nodeName, pos, radius);

            // Clear inputs
            SingleComposer.GetTextInput("nodeId")?.SetValue("");
            SingleComposer.GetTextInput("nodeName")?.SetValue("");
            SingleComposer.GetTextInput("nodeRadius")?.SetValue("");

            return true;
        }

        private bool OnUpdateNode()
        {
            if (selectedNodeId == null) return true;

            var player = capi.World.Player;
			var absolutePos = player.Entity.Pos.XYZ;
			var spawnPos = capi.World.DefaultSpawnPosition.XYZ;
			var pos = absolutePos.Sub(spawnPos);

			networkHandler.RequestUpdateNode(selectedNodeId, pos);

            return true;
        }

        private bool OnUnregisterNode()
        {
            if (selectedNodeId == null) return true;

            networkHandler.RequestUnregisterNode(selectedNodeId);

            selectedNodeId = null;
            return true;
        }

        #endregion

        #region Wars Tab

        private void AddWarsTabContent(GuiComposer composer, ref double top)
        {
            if (currentData == null) return;

            var leftCol = 10.0;
            var rightCol = 320.0;

            if (currentData.Nodes.Count == 0)
            {
                composer.AddStaticText("No nodes registered.\nCreate nodes in the 'Manage Nodes' tab first.",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 600, 50));
                return;
            }

            // Left side - Select Node
            composer.AddStaticText("Select Node for War Management:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(leftCol, top, 400, 25));
            top += 30;

            var nodeList = currentData.Nodes.Select(n => $"{n.NodeName} ({n.NodeId})").ToArray();
            composer.AddDropDown(currentData.Nodes.Select(n => n.NodeId).ToArray(),
                nodeList,
                selectedNodeId != null ? Array.FindIndex(currentData.Nodes.ToArray(), n => n.NodeId == selectedNodeId) : 0,
                OnNodeSelected,
                ElementBounds.Fixed(leftCol, top, 300, 25),
                "warNodeSelector");
            top += 35;

            if (selectedNodeId == null)
            {
                composer.AddStaticText("Select a node to manage its wars",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 25));
                return;
            }

            // Get selected node
            var selectedNode = currentData.Nodes.FirstOrDefault(n => n.NodeId == selectedNodeId);
            if (selectedNode == null) return;

            // Show current war status
            composer.AddStaticText("Current Status:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(leftCol, top, 300, 20));
            top += 25;

            if (!selectedNode.WarStatus.HasValue || selectedNode.WarStatus == 0)
            {
                composer.AddStaticText("No active or scheduled war",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 20));
                top += 30;
            }
            else
            {
                var statusText = new StringBuilder();
                statusText.AppendLine($"Status: {GetWarStatusName(selectedNode.WarStatus.Value)}");

                if (selectedNode.WarStatus == 1) // Scheduled
                {
                    statusText.AppendLine($"Signups: {selectedNode.WarSignupCount ?? 0}/{(selectedNode.WarMaxGuilds > 0 ? selectedNode.WarMaxGuilds.ToString() : "∞")}");
                }
                else if (selectedNode.WarStatus == 3 && !string.IsNullOrEmpty(selectedNode.WarWinnerGuildName)) // Completed
                {
                    statusText.AppendLine($"Winner: {selectedNode.WarWinnerGuildName}");
                }

                composer.AddStaticText(statusText.ToString(),
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 150));
                top += 160;
            }

            // Right side - War Actions
            top = 75; // Reset to top for right column

            composer.AddStaticText("War Actions:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(rightCol, top, 280, 25));
            top += 35;

            if (!selectedNode.WarStatus.HasValue || selectedNode.WarStatus == 0 || selectedNode.WarStatus == 3 || selectedNode.WarStatus == 4) // None, Completed or Cancelled
            {
                // Schedule new war
                composer.AddStaticText("Schedule New War:", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(rightCol, top, 280, 20));
                top += 25;

                composer.AddStaticText("Hours from now:", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(rightCol, top, 120, 20));
                composer.AddTextInput(ElementBounds.Fixed(rightCol + 125, top, 155, 25),
                    null, CairoFont.WhiteSmallText(), "scheduleHours");
                top += 30;

                composer.AddSmallButton("Schedule War", OnScheduleWar,
                    ElementBounds.Fixed(rightCol, top, 280, 25));
                top += 35;
            }

            if (selectedNode.WarStatus.HasValue && selectedNode.WarStatus != 0)
            {
                if (selectedNode.WarStatus == 1) // Scheduled
                {
                    composer.AddSmallButton("Start War Now", OnStartWar,
                        ElementBounds.Fixed(rightCol, top, 280, 25));
                    top += 30;

                    composer.AddSmallButton("Cancel War", OnCancelWar,
                        ElementBounds.Fixed(rightCol, top, 280, 25),
                        EnumButtonStyle.Normal);
                    top += 35;
                }
                else if (selectedNode.WarStatus == 2) // Active
                {
                    composer.AddStaticText("Force End War:", CairoFont.WhiteSmallText(),
                        ElementBounds.Fixed(rightCol, top, 280, 20));
                    top += 25;

                    composer.AddStaticText("Winner Guild (optional):", CairoFont.WhiteSmallText(),
                        ElementBounds.Fixed(rightCol, top, 280, 20));
                    top += 22;

                    // Guild dropdown
                    if (currentData.AvailableGuilds.Count > 0)
                    {
                        var guilds = currentData.AvailableGuilds.ToList();
                        guilds.Insert(0, "None");

                        composer.AddDropDown(guilds.ToArray(), guilds.ToArray(), 0, null,
                            ElementBounds.Fixed(rightCol, top, 280, 25),
                            "winnerGuild");
                        top += 30;
                    }

                    composer.AddSmallButton("End War", OnEndWar,
                        ElementBounds.Fixed(rightCol, top, 280, 25),
                        EnumButtonStyle.Normal);
                    top += 30;
                }
            }
        }

        private string GetWarStatusName(int status)
        {
            return status switch
            {
                0 => "None",
                1 => "Scheduled",
                2 => "Active",
                3 => "Completed",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }

        private bool OnScheduleWar()
        {
            if (selectedNodeId == null) return true;

            var hoursText = SingleComposer.GetTextInput("scheduleHours")?.GetText();
            if (!double.TryParse(hoursText, out double hours) || hours < 0)
            {
                capi.ShowChatMessage("Please enter a valid number of hours");
                return true;
            }

            // Send hours from now (relative time), not absolute in-game hours
            networkHandler.RequestScheduleWar(selectedNodeId, hours);

            SingleComposer.GetTextInput("scheduleHours")?.SetValue("");
            return true;
        }

        private bool OnStartWar()
        {
            if (selectedNodeId == null) return true;

            networkHandler.RequestStartWar(selectedNodeId);
            return true;
        }

        private bool OnEndWar()
        {
            if (selectedNodeId == null || currentData == null) return true;

            string? winnerGuildUid = null;
            var winnerGuildName = SingleComposer.GetDropDown("winnerGuild")?.SelectedValue;
            
            if (!string.IsNullOrWhiteSpace(winnerGuildName) && winnerGuildName != "None")
            {
                // Guild UID is same as guild name in the current system
                winnerGuildUid = winnerGuildName;
            }

            networkHandler.RequestEndWar(selectedNodeId, winnerGuildUid);
            return true;
        }

        private bool OnCancelWar()
        {
            if (selectedNodeId == null) return true;

            networkHandler.RequestCancelWar(selectedNodeId);
            return true;
        }

        #endregion

        #region Zones Tab

        private void AddZonesTabContent(GuiComposer composer, ref double top)
        {
            if (currentData == null) return;

            var leftCol = 10.0;
            var rightCol = 320.0;

            if (currentData.Nodes.Count == 0)
            {
                composer.AddStaticText("No nodes registered.\nCreate nodes in the 'Manage Nodes' tab first.",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 600, 50));
                return;
            }

            // Left side - Select Node
            composer.AddStaticText("Select Node:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(leftCol, top, 300, 25));
            top += 30;

            var nodeList = currentData.Nodes.Select(n => $"{n.NodeName} ({n.NodeId})").ToArray();
            composer.AddDropDown(currentData.Nodes.Select(n => n.NodeId).ToArray(),
                nodeList,
                selectedNodeId != null ? Array.FindIndex(currentData.Nodes.ToArray(), n => n.NodeId == selectedNodeId) : 0,
                OnNodeSelected,
                ElementBounds.Fixed(leftCol, top, 300, 25),
                "zoneNodeSelector");
            top += 35;

            if (selectedNodeId == null)
            {
                composer.AddStaticText("Select a node to manage its capture zones",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 25));
                return;
            }

            var selectedNode = currentData.Nodes.FirstOrDefault(n => n.NodeId == selectedNodeId);
            if (selectedNode == null) return;

            // Show capture zones
            composer.AddStaticText($"Capture Zones: {selectedNode.CaptureZones.Count}",
                CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(leftCol, top, 300, 20));
            top += 25;

            if (selectedNode.CaptureZones.Count == 0)
            {
                composer.AddStaticText("No capture zones defined.\nThe entire node area will be used as default.",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(leftCol, top, 300, 40));
                top += 45;
            }
            else
            {
                foreach (var zone in selectedNode.CaptureZones.OrderBy(z => z.ZoneName))
                {
                    var zoneInfo = $"• {zone.ZoneName} ({zone.ZoneId})\n" +
                                  $"  Location: X:{zone.CenterX:F0}, Y:{zone.CenterY:F0}, Z:{zone.CenterZ:F0}\n" +
                                  $"  Radius: {zone.Radius} blocks | Active: {(zone.IsActive ? "Yes" : "No")}";
                    
                    composer.AddStaticText(zoneInfo, CairoFont.WhiteSmallText(),
                        ElementBounds.Fixed(leftCol, top, 300, 60));
                    top += 65;
                }
            }

            // Right side - Zone Actions
            top = 75; // Reset to top for right column

            composer.AddStaticText("Capture Zone Actions:", CairoFont.WhiteSmallishText(),
                ElementBounds.Fixed(rightCol, top, 280, 25));
            top += 35;

            composer.AddStaticText("Add New Capture Zone:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 280, 20));
            top += 25;

            composer.AddStaticText("Zone ID:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "zoneId");
            top += 30;

            composer.AddStaticText("Name:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "zoneName");
            top += 30;

            composer.AddStaticText("Radius:", CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(rightCol, top, 80, 20));
            composer.AddTextInput(ElementBounds.Fixed(rightCol + 85, top, 195, 25),
                null, CairoFont.WhiteSmallText(), "zoneRadius");
            top += 30;

            composer.AddSmallButton("Add Zone at Current Position", OnAddCaptureZone,
                ElementBounds.Fixed(rightCol, top, 280, 25));
            top += 35;

            // Remove zone section
            if (selectedNode.CaptureZones.Count > 0)
            {
                composer.AddStaticText("Remove Capture Zone:", CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(rightCol, top, 280, 20));
                top += 25;

                var zoneIds = selectedNode.CaptureZones.Select(z => z.ZoneId).ToArray();
                var zoneNames = selectedNode.CaptureZones.Select(z => z.ZoneName).ToArray();

                composer.AddDropDown(zoneIds, zoneNames, 0, null,
                    ElementBounds.Fixed(rightCol, top, 280, 25),
                    "zoneToRemove");
                top += 30;

                composer.AddSmallButton("Remove Selected Zone", OnRemoveCaptureZone,
                    ElementBounds.Fixed(rightCol, top, 280, 25),
                    EnumButtonStyle.Normal);
                top += 30;
            }
        }

        private bool OnAddCaptureZone()
        {
            if (selectedNodeId == null || currentData == null) return true;

            var zoneId = SingleComposer.GetTextInput("zoneId")?.GetText();
            var zoneName = SingleComposer.GetTextInput("zoneName")?.GetText();
            var radiusText = SingleComposer.GetTextInput("zoneRadius")?.GetText();

            if (string.IsNullOrWhiteSpace(zoneId) || string.IsNullOrWhiteSpace(zoneName))
            {
                capi.ShowChatMessage("Please enter both Zone ID and Name");
                return true;
            }

            if (!int.TryParse(radiusText, out int radius) || radius <= 0)
            {
                capi.ShowChatMessage("Please enter a valid radius");
                return true;
            }

            var player = capi.World.Player;
            var pos = player.Entity.Pos.XYZ;

            networkHandler.RequestAddCaptureZone(selectedNodeId, zoneId, zoneName, pos, radius);

            // Clear inputs
            SingleComposer.GetTextInput("zoneId")?.SetValue("");
            SingleComposer.GetTextInput("zoneName")?.SetValue("");
            SingleComposer.GetTextInput("zoneRadius")?.SetValue("");

            return true;
        }

        private bool OnRemoveCaptureZone()
        {
            if (selectedNodeId == null || currentData == null) return true;

            var zoneId = SingleComposer.GetDropDown("zoneToRemove")?.SelectedValue;
            if (string.IsNullOrEmpty(zoneId)) return true;

            networkHandler.RequestRemoveCaptureZone(selectedNodeId, zoneId);

            return true;
        }

        #endregion

        #region Tab Navigation

        private bool OnNodesTab()
        {
            currentTab = TAB_NODES;
            SetupDialog();
            return true;
        }

        private bool OnWarsTab()
        {
            currentTab = TAB_WARS;
            SetupDialog();
            return true;
        }

        private bool OnZonesTab()
        {
            currentTab = TAB_ZONES;
            SetupDialog();
            return true;
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        #endregion
    }
}
