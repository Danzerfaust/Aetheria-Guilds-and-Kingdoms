using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui
{
	// Token: 0x0200007C RID: 124
	[NullableContext(1)]
	[Nullable(0)]
	public class DialogNodeWarSignup : GuiDialog
	{
		// Token: 0x17000171 RID: 369
		// (get) Token: 0x0600052C RID: 1324 RVA: 0x00020866 File Offset: 0x0001EA66
		public override string ToggleKeyCombinationCode
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00020869 File Offset: 0x0001EA69
		public DialogNodeWarSignup(ICoreClientAPI capi, NodeWarSignupData warData, Action<string> onConfirmSignup, Action onCancel) : base(capi)
		{
			this.warData = warData;
			this.onConfirmSignup = onConfirmSignup;
			this.onCancel = onCancel;
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00020888 File Offset: 0x0001EA88
		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.SetupDialog();
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00020898 File Offset: 0x0001EA98
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(6);
			ElementBounds insetBounds = ElementBounds.Fixed(0.0, 0.0, 450.0, 350.0);
			ElementBounds contentBounds = ElementBounds.Fixed(0.0, 30.0, 450.0, 320.0);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = 2;
			bgBounds.WithChildren(new ElementBounds[]
			{
				insetBounds,
				contentBounds
			});
			base.SingleComposer = GuiComposerHelpers.AddDialogTitleBar(GuiComposerHelpers.AddShadedDialogBG(this.capi.Gui.CreateCompo("nodewar-signup", dialogBounds), bgBounds, true, 5.0, 0.75f), "Sign Up for Node War", new Action(this.OnTitleBarClose), null, null, null).BeginChildElements(bgBounds);
			double top = 30.0;
			double elementHeight = 25.0;
			double spacing = 10.0;
			GuiComposerHelpers.AddStaticText(base.SingleComposer, "Node: " + this.warData.NodeName, CairoFont.WhiteMediumText(), ElementBounds.Fixed(10.0, top, 430.0, elementHeight), null);
			top += elementHeight + 5.0;
			if (!string.IsNullOrEmpty(this.warData.NodeDescription))
			{
				GuiComposerHelpers.AddStaticText(base.SingleComposer, this.warData.NodeDescription, CairoFont.WhiteSmallText(), ElementBounds.Fixed(10.0, top, 430.0, elementHeight * 2.0), null);
				top += elementHeight * 2.0 + spacing;
			}
			GuiComposerHelpers.AddStaticText(base.SingleComposer, "War Details:", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(10.0, top, 430.0, elementHeight), null);
			top += elementHeight + 5.0;
			GuiComposerHelpers.AddStaticText(base.SingleComposer, "Starts: " + this.GetTimeString(this.warData.StartTime), CairoFont.WhiteSmallText(), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
			top += elementHeight;
			GuiComposer singleComposer = base.SingleComposer;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Duration: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.warData.DurationMinutes);
			defaultInterpolatedStringHandler.AppendLiteral(" minutes");
			GuiComposerHelpers.AddStaticText(singleComposer, defaultInterpolatedStringHandler.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
			top += elementHeight;
			GuiComposer singleComposer2 = base.SingleComposer;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Points to Win: ");
			defaultInterpolatedStringHandler2.AppendFormatted<double>(this.warData.PointsNeeded, "F0");
			GuiComposerHelpers.AddStaticText(singleComposer2, defaultInterpolatedStringHandler2.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
			top += elementHeight;
			GuiComposer singleComposer3 = base.SingleComposer;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(13, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("Min Players: ");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(this.warData.MinPlayersRequired);
			GuiComposerHelpers.AddStaticText(singleComposer3, defaultInterpolatedStringHandler3.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
			top += elementHeight + spacing;
			GuiComposerHelpers.AddStaticText(base.SingleComposer, "Guild Signups:", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(10.0, top, 430.0, elementHeight), null);
			top += elementHeight + 5.0;
			string maxGuilds = (this.warData.MaxGuilds == 0) ? "Unlimited" : this.warData.MaxGuilds.ToString();
			GuiComposer singleComposer4 = base.SingleComposer;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(27, 2);
			defaultInterpolatedStringHandler4.AppendFormatted<int>(this.warData.CurrentSignups);
			defaultInterpolatedStringHandler4.AppendLiteral(" guild(s) signed up (Max: ");
			defaultInterpolatedStringHandler4.AppendFormatted(maxGuilds);
			defaultInterpolatedStringHandler4.AppendLiteral(")");
			GuiComposerHelpers.AddStaticText(singleComposer4, defaultInterpolatedStringHandler4.ToStringAndClear(), CairoFont.WhiteSmallText(), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
			top += elementHeight;
			if (this.warData.SignedUpGuilds.Count > 0)
			{
				foreach (string guild in this.warData.SignedUpGuilds)
				{
					GuiComposerHelpers.AddStaticText(base.SingleComposer, "• " + guild, CairoFont.WhiteSmallText().WithColor(new double[]
					{
						0.8,
						0.8,
						1.0,
						1.0
					}), ElementBounds.Fixed(30.0, top, 400.0, elementHeight - 5.0), null);
					top += elementHeight - 5.0;
				}
			}
			top += spacing;
			GuiComposerHelpers.AddStaticText(base.SingleComposer, "Your Guild Requirements:", CairoFont.WhiteSmallText().WithWeight(1), ElementBounds.Fixed(10.0, top, 430.0, elementHeight), null);
			top += elementHeight + 5.0;
			foreach (ValueTuple<string, bool> valueTuple in this.CheckRequirements())
			{
				string requirement = valueTuple.Item1;
				bool met = valueTuple.Item2;
				double[] array;
				if (!met)
				{
					RuntimeHelpers.InitializeArray(array = new double[4], fieldof(<PrivateImplementationDetails>.163A1F3CC0E5FDC4A6CDB564DAFF6423E7FEBDE5288FC5094AC336058CC64EB8).FieldHandle);
				}
				else
				{
					RuntimeHelpers.InitializeArray(array = new double[4], fieldof(<PrivateImplementationDetails>.9D049590ECAD48DE72AC271F860A62FB1ACB7E6EB02CCE040F05169470769ECC).FieldHandle);
				}
				double[] color = array;
				string icon = met ? "✓" : "✗";
				GuiComposerHelpers.AddStaticText(base.SingleComposer, icon + " " + requirement, CairoFont.WhiteSmallText().WithColor(color), ElementBounds.Fixed(20.0, top, 410.0, elementHeight), null);
				top += elementHeight;
			}
			top += spacing;
			if (!this.AllRequirementsMet())
			{
				GuiComposerHelpers.AddStaticText(base.SingleComposer, "Your guild does not meet all requirements!", CairoFont.WhiteSmallText().WithColor(new double[]
				{
					1.0,
					0.5,
					0.5,
					1.0
				}), ElementBounds.Fixed(10.0, top, 430.0, elementHeight), null);
				top += elementHeight + spacing;
			}
			double buttonY = top;
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Sign Up", new ActionConsumable(this.OnConfirmClick), ElementBounds.Fixed(10.0, buttonY, 100.0, elementHeight), 2, "btnConfirm");
			if (!this.AllRequirementsMet())
			{
				GuiComposerHelpers.GetButton(base.SingleComposer, "btnConfirm").Enabled = false;
			}
			GuiComposerHelpers.AddSmallButton(base.SingleComposer, "Cancel", new ActionConsumable(this.OnCancelClick), ElementBounds.Fixed(120.0, buttonY, 100.0, elementHeight), 2, null);
			base.SingleComposer.EndChildElements().Compose(true);
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00021018 File Offset: 0x0001F218
		[return: TupleElementNames(new string[]
		{
			"requirement",
			"met"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		private List<ValueTuple<string, bool>> CheckRequirements()
		{
			List<ValueTuple<string, bool>> requirements = new List<ValueTuple<string, bool>>();
			List<ValueTuple<string, bool>> list = requirements;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
			defaultInterpolatedStringHandler.AppendLiteral("At least ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.warData.MinGuildMembers);
			defaultInterpolatedStringHandler.AppendLiteral(" total members (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.warData.GuildTotalMembers);
			defaultInterpolatedStringHandler.AppendLiteral(" current)");
			list.Add(new ValueTuple<string, bool>(defaultInterpolatedStringHandler.ToStringAndClear(), this.warData.GuildTotalMembers >= this.warData.MinGuildMembers));
			List<ValueTuple<string, bool>> list2 = requirements;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("At least ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(this.warData.MinOnlineMembers);
			defaultInterpolatedStringHandler2.AppendLiteral(" members online (");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(this.warData.GuildOnlineMembers);
			defaultInterpolatedStringHandler2.AppendLiteral(" current)");
			list2.Add(new ValueTuple<string, bool>(defaultInterpolatedStringHandler2.ToStringAndClear(), this.warData.GuildOnlineMembers >= this.warData.MinOnlineMembers));
			requirements.Add(new ValueTuple<string, bool>("Not signed up for another war", !this.warData.IsAlreadySignedUp));
			requirements.Add(new ValueTuple<string, bool>("You must be the guild leader", this.warData.IsPlayerLeader));
			requirements.Add(new ValueTuple<string, bool>("Signup period still open", !this.warData.IsSignupClosed));
			if (this.warData.MaxGuilds > 0)
			{
				List<ValueTuple<string, bool>> list3 = requirements;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Space available (");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.warData.CurrentSignups);
				defaultInterpolatedStringHandler3.AppendLiteral("/");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.warData.MaxGuilds);
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				list3.Add(new ValueTuple<string, bool>(defaultInterpolatedStringHandler3.ToStringAndClear(), this.warData.CurrentSignups < this.warData.MaxGuilds));
			}
			return requirements;
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x0002120C File Offset: 0x0001F40C
		private bool AllRequirementsMet()
		{
			using (List<ValueTuple<string, bool>>.Enumerator enumerator = this.CheckRequirements().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.Item2)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x00021268 File Offset: 0x0001F468
		private bool OnConfirmClick()
		{
			Action<string> action = this.onConfirmSignup;
			if (action != null)
			{
				action(this.warData.NodeId);
			}
			this.TryClose();
			return true;
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0002128E File Offset: 0x0001F48E
		private bool OnCancelClick()
		{
			Action action = this.onCancel;
			if (action != null)
			{
				action();
			}
			this.TryClose();
			return true;
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x000212A9 File Offset: 0x0001F4A9
		private void OnTitleBarClose()
		{
			this.TryClose();
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x000212B4 File Offset: 0x0001F4B4
		private string GetTimeString(DateTime time)
		{
			TimeSpan diff = time - DateTime.UtcNow;
			if (diff.TotalDays > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 2);
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(time, "yyyy-MM-dd HH:mm");
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<double>(diff.TotalDays, "F0");
				defaultInterpolatedStringHandler.AppendLiteral(" days from now)");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			if (diff.TotalHours > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<DateTime>(time, "HH:mm");
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted<double>(diff.TotalHours, "F0");
				defaultInterpolatedStringHandler2.AppendLiteral(" hours from now)");
				return defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			if (diff.TotalMinutes > 1.0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 2);
				defaultInterpolatedStringHandler3.AppendFormatted<DateTime>(time, "HH:mm");
				defaultInterpolatedStringHandler3.AppendLiteral(" (");
				defaultInterpolatedStringHandler3.AppendFormatted<double>(diff.TotalMinutes, "F0");
				defaultInterpolatedStringHandler3.AppendLiteral(" minutes from now)");
				return defaultInterpolatedStringHandler3.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler4.AppendFormatted<DateTime>(time, "HH:mm");
			defaultInterpolatedStringHandler4.AppendLiteral(" (starting soon)");
			return defaultInterpolatedStringHandler4.ToStringAndClear();
		}

		// Token: 0x040001EE RID: 494
		private readonly Action<string> onConfirmSignup;

		// Token: 0x040001EF RID: 495
		private readonly Action onCancel;

		// Token: 0x040001F0 RID: 496
		private readonly NodeWarSignupData warData;
	}
}
