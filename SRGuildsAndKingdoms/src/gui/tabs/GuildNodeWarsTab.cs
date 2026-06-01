using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.guilds;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.tabs
{
	// Token: 0x02000087 RID: 135
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildNodeWarsTab : GuildTabContent
	{
		// Token: 0x060005E6 RID: 1510 RVA: 0x0002B2DB File Offset: 0x000294DB
		public GuildNodeWarsTab(ICoreClientAPI capi, SRGuildsAndKingdomsModSystem modSystem, [Nullable(2)] GuildSummary currentGuild, ActionConsumable onSignupForWar, ActionConsumable onCancelSignup, ActionConsumable onJoinWar, ActionConsumable onViewWarDetails) : base(capi, modSystem, currentGuild)
		{
			this.onSignupForWar = onSignupForWar;
			this.onCancelSignup = onCancelSignup;
			this.onJoinWar = onJoinWar;
			this.onViewWarDetails = onViewWarDetails;
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x0002B306 File Offset: 0x00029506
		public void SetNodeWarData(NodeWarTabData data)
		{
			this.nodeWarData = data;
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x0002B310 File Offset: 0x00029510
		public override double AddContent(GuiComposer composer, double startTop)
		{
			if (this.currentGuild == null)
			{
				return startTop;
			}
			double elementHeight = 25.0;
			double spacing = 10.0;
			double top;
			if (this.nodeWarData == null)
			{
				GuiComposerHelpers.AddStaticText(composer, "Node Wars", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, 400.0, elementHeight), null);
				top = startTop + (elementHeight + spacing);
				GuiComposerHelpers.AddStaticText(composer, "No data", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.5,
					0.5,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight * 2.0), null);
				return top + (elementHeight * 2.0 + spacing);
			}
			GuiComposerHelpers.AddStaticText(composer, "Node Wars", CairoFont.WhiteMediumText(), ElementBounds.Fixed(0.0, startTop, 400.0, elementHeight), null);
			top = startTop + (elementHeight + spacing);
			top = this.AddControlledNodesSection(composer, top, elementHeight, spacing);
			top = this.AddActiveWarsSection(composer, top, elementHeight, spacing);
			return this.AddAvailableWarsSection(composer, top, elementHeight, spacing);
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x0002B42C File Offset: 0x0002962C
		private double AddControlledNodesSection(GuiComposer composer, double top, double elementHeight, double spacing)
		{
			if (this.nodeWarData == null || this.nodeWarData.ControlledNodes.Count == 0)
			{
				GuiComposerHelpers.AddStaticText(composer, "Controlled Nodes: None", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				return top;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Controlled Nodes (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.nodeWarData.ControlledNodes.Count);
			defaultInterpolatedStringHandler.AppendLiteral("):");
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
			top += elementHeight + 5.0;
			foreach (ControlledNodeInfo node in this.nodeWarData.ControlledNodes.Take(5))
			{
				string captureTime = (node.CapturedAt != null) ? (" (captured " + this.GetRelativeTime(node.CapturedAt.Value) + ")") : "";
				int? warStatus = node.WarStatus;
				if (warStatus == null)
				{
					goto IL_183;
				}
				string text;
				switch (warStatus.GetValueOrDefault())
				{
				case 1:
					text = " \ud83d\udcc5";
					break;
				case 2:
					text = " ⚔";
					break;
				case 3:
					text = " ✓";
					break;
				case 4:
					text = " ✗";
					break;
				default:
					goto IL_183;
				}
				IL_18A:
				string warStatusIcon = text;
				GuiComposerHelpers.AddStaticText(composer, "• " + node.NodeName + warStatusIcon + captureTime, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					1.0,
					0.7,
					1.0
				}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
				if (node.InfluencePerDay > 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("  +");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(node.InfluencePerDay);
					defaultInterpolatedStringHandler2.AppendLiteral(" influence/day");
					GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
					{
						1.0,
						1.0,
						0.7,
						1.0
					}), ElementBounds.Fixed(20.0, top, 380.0, elementHeight - 5.0), null);
					top += elementHeight - 5.0;
				}
				if (node.WarStatus != null && node.WarStatus.GetValueOrDefault() != 3 && node.WarStatus.GetValueOrDefault() != 4)
				{
					warStatus = node.WarStatus;
					if (warStatus == null)
					{
						goto IL_321;
					}
					int valueOrDefault = warStatus.GetValueOrDefault();
					if (valueOrDefault != 1)
					{
						if (valueOrDefault != 2)
						{
							goto IL_321;
						}
						text = "  War in progress!";
					}
					else
					{
						text = "  War scheduled: " + ((node.WarScheduledStartTime != null) ? this.GetRelativeTime(node.WarScheduledStartTime.Value) : "soon");
					}
					IL_328:
					string warStatusText = text;
					if (!string.IsNullOrEmpty(warStatusText))
					{
						GuiComposerHelpers.AddStaticText(composer, warStatusText, CairoFont.WhiteSmallText().WithColor(new double[]
						{
							1.0,
							0.8,
							0.5,
							1.0
						}), ElementBounds.Fixed(20.0, top, 380.0, elementHeight - 5.0), null);
						top += elementHeight - 5.0;
						continue;
					}
					continue;
					IL_321:
					text = "";
					goto IL_328;
				}
				continue;
				IL_183:
				text = "";
				goto IL_18A;
			}
			if (this.nodeWarData.ControlledNodes.Count > 5)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("...and ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.nodeWarData.ControlledNodes.Count - 5);
				defaultInterpolatedStringHandler3.AppendLiteral(" more");
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler3.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
			}
			top += spacing;
			return top;
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x0002B898 File Offset: 0x00029A98
		private double AddActiveWarsSection(GuiComposer composer, double top, double elementHeight, double spacing)
		{
			if (this.nodeWarData == null || this.nodeWarData.CurrentWar == null)
			{
				GuiComposerHelpers.AddStaticText(composer, "Active Wars: None", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				return top;
			}
			CurrentWarInfo war = this.nodeWarData.CurrentWar;
			GuiComposerHelpers.AddStaticText(composer, "Current War:", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
			top += elementHeight + 5.0;
			GuiComposerHelpers.AddStaticText(composer, "\ud83d\udccd " + war.NodeName, CairoFont.WhiteSmallText().WithColor(new double[]
			{
				1.0,
				0.8,
				0.5,
				1.0
			}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
			top += elementHeight;
			GuiComposerHelpers.AddStaticText(composer, "Status: " + war.Status, CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 200.0, elementHeight), null);
			top += elementHeight;
			if (war.YourGuildProgress != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Your Progress: ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(war.YourGuildProgress.CapturePoints, "F0");
				defaultInterpolatedStringHandler.AppendLiteral("/");
				defaultInterpolatedStringHandler.AppendFormatted<double>(war.PointsNeeded, "F0");
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					1.0,
					1.0,
					1.0
				}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Players in Zone: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(war.YourGuildProgress.PlayersInZone);
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 200.0, elementHeight), null);
				top += elementHeight;
			}
			if (war.Status == "Active")
			{
				GuiComposerHelpers.AddSmallButton(composer, "Join War", this.onJoinWar, ElementBounds.Fixed(0.0, top, 100.0, elementHeight), 2, null);
			}
			GuiComposerHelpers.AddSmallButton(composer, "View Details", this.onViewWarDetails, ElementBounds.Fixed(110.0, top, 100.0, elementHeight), 2, null);
			top += elementHeight + spacing;
			return top;
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x0002BB34 File Offset: 0x00029D34
		private double AddAvailableWarsSection(GuiComposer composer, double top, double elementHeight, double spacing)
		{
			if (this.nodeWarData == null)
			{
				return top;
			}
			bool hasSignup = this.nodeWarData.CurrentSignup != null;
			bool hasActiveWar = this.nodeWarData.CurrentWar != null;
			bool isEngaged = hasSignup || hasActiveWar;
			if (hasSignup)
			{
				CurrentSignupInfo signup = this.nodeWarData.CurrentSignup;
				GuiComposerHelpers.AddStaticText(composer, "Signed Up For:", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + 5.0;
				GuiComposerHelpers.AddStaticText(composer, "\ud83d\udccd " + signup.NodeName, CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					1.0,
					0.7,
					1.0
				}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
				GuiComposerHelpers.AddStaticText(composer, "Starts: " + this.GetRelativeTime(signup.WarStartTime), CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
				GuiComposerHelpers.AddStaticText(composer, "Signed up: " + this.GetRelativeTime(signup.SignupTime), CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
				if (base.IsLeader())
				{
					GuiComposerHelpers.AddSmallButton(composer, "Cancel Signup", this.onCancelSignup, ElementBounds.Fixed(0.0, top, 120.0, elementHeight), 2, null);
				}
				top += elementHeight + spacing;
			}
			if (this.nodeWarData.AvailableWars.Count == 0)
			{
				GuiComposerHelpers.AddStaticText(composer, "Available Wars: None", CairoFont.WhiteSmallText(), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight + spacing;
				return top;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Available Wars (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.nodeWarData.AvailableWars.Count);
			defaultInterpolatedStringHandler.AppendLiteral("):");
			GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
			top += elementHeight + 5.0;
			foreach (AvailableWarInfo war in this.nodeWarData.AvailableWars.Take(3))
			{
				GuiComposerHelpers.AddStaticText(composer, "\ud83d\udccd " + war.NodeName, CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 200.0, elementHeight), null);
				if (!isEngaged && base.IsLeader())
				{
					string capturedNodeId = war.NodeId;
					GuiComposerHelpers.AddSmallButton(composer, "Sign Up", delegate()
					{
						if (this.nodeWarData != null)
						{
							this.nodeWarData.SelectedWarForSignup = capturedNodeId;
						}
						return this.onSignupForWar.Invoke();
					}, ElementBounds.Fixed(220.0, top, 80.0, elementHeight), 2, null);
				}
				top += elementHeight;
				GuiComposerHelpers.AddStaticText(composer, "  Starts: " + this.GetRelativeTime(war.WarStartTime), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.8,
					0.8,
					0.8,
					1.0
				}), ElementBounds.Fixed(20.0, top, 380.0, elementHeight - 5.0), null);
				top += elementHeight - 5.0;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("  Signups: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(war.CurrentSignups);
				defaultInterpolatedStringHandler2.AppendLiteral("/");
				defaultInterpolatedStringHandler2.AppendFormatted((war.MaxGuilds == 0) ? "∞" : war.MaxGuilds.ToString());
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.8,
					0.8,
					0.8,
					1.0
				}), ElementBounds.Fixed(20.0, top, 380.0, elementHeight - 5.0), null);
				top += elementHeight;
			}
			if (this.nodeWarData.AvailableWars.Count > 3)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("...and ");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.nodeWarData.AvailableWars.Count - 3);
				defaultInterpolatedStringHandler3.AppendLiteral(" more (use /nodewar list)");
				GuiComposerHelpers.AddStaticText(composer, defaultInterpolatedStringHandler3.ToStringAndClear(), CairoFont.WhiteSmallText().WithColor(new double[]
				{
					0.7,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(10.0, top, 390.0, elementHeight), null);
				top += elementHeight;
			}
			if (!base.IsLeader())
			{
				GuiComposerHelpers.AddStaticText(composer, "Only guild leaders can sign up for wars", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight;
			}
			else if (hasActiveWar)
			{
				GuiComposerHelpers.AddStaticText(composer, "You are already in an active war", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight;
			}
			else if (hasSignup)
			{
				GuiComposerHelpers.AddStaticText(composer, "Cancel your current signup to sign up for another war", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.7,
					0.7,
					1.0
				}), ElementBounds.Fixed(0.0, top, 400.0, elementHeight), null);
				top += elementHeight;
			}
			top += spacing;
			return top;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x0002C124 File Offset: 0x0002A324
		private string GetRelativeTime(DateTime time)
		{
			TimeSpan diff = time - DateTime.UtcNow;
			if (diff.TotalDays > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler.AppendLiteral("in ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(diff.TotalDays, "F0");
				defaultInterpolatedStringHandler.AppendLiteral(" days");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			if (diff.TotalHours > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("in ");
				defaultInterpolatedStringHandler2.AppendFormatted<double>(diff.TotalHours, "F0");
				defaultInterpolatedStringHandler2.AppendLiteral(" hours");
				return defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			if (diff.TotalMinutes > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("in ");
				defaultInterpolatedStringHandler3.AppendFormatted<double>(diff.TotalMinutes, "F0");
				defaultInterpolatedStringHandler3.AppendLiteral(" minutes");
				return defaultInterpolatedStringHandler3.ToStringAndClear();
			}
			if (diff.TotalSeconds > 0.0)
			{
				return "soon";
			}
			diff = diff.Duration();
			if (diff.TotalDays > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(9, 1);
				defaultInterpolatedStringHandler4.AppendFormatted<double>(diff.TotalDays, "F0");
				defaultInterpolatedStringHandler4.AppendLiteral(" days ago");
				return defaultInterpolatedStringHandler4.ToStringAndClear();
			}
			if (diff.TotalHours > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler5.AppendFormatted<double>(diff.TotalHours, "F0");
				defaultInterpolatedStringHandler5.AppendLiteral(" hours ago");
				return defaultInterpolatedStringHandler5.ToStringAndClear();
			}
			if (diff.TotalMinutes > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler6.AppendFormatted<double>(diff.TotalMinutes, "F0");
				defaultInterpolatedStringHandler6.AppendLiteral(" minutes ago");
				return defaultInterpolatedStringHandler6.ToStringAndClear();
			}
			return "just now";
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x0002C317 File Offset: 0x0002A517
		[NullableContext(2)]
		public string GetSelectedWarForSignup()
		{
			NodeWarTabData nodeWarTabData = this.nodeWarData;
			if (nodeWarTabData == null)
			{
				return null;
			}
			return nodeWarTabData.SelectedWarForSignup;
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x0002C32A File Offset: 0x0002A52A
		[NullableContext(2)]
		public CurrentSignupInfo GetCurrentSignup()
		{
			NodeWarTabData nodeWarTabData = this.nodeWarData;
			if (nodeWarTabData == null)
			{
				return null;
			}
			return nodeWarTabData.CurrentSignup;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x0002C33D File Offset: 0x0002A53D
		[NullableContext(2)]
		public CurrentWarInfo GetCurrentWar()
		{
			NodeWarTabData nodeWarTabData = this.nodeWarData;
			if (nodeWarTabData == null)
			{
				return null;
			}
			return nodeWarTabData.CurrentWar;
		}

		// Token: 0x04000260 RID: 608
		private readonly ActionConsumable onSignupForWar;

		// Token: 0x04000261 RID: 609
		private readonly ActionConsumable onCancelSignup;

		// Token: 0x04000262 RID: 610
		private readonly ActionConsumable onJoinWar;

		// Token: 0x04000263 RID: 611
		private readonly ActionConsumable onViewWarDetails;

		// Token: 0x04000264 RID: 612
		[Nullable(2)]
		private NodeWarTabData nodeWarData;
	}
}
