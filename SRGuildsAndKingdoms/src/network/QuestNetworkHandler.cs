using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.config;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdoms.src.quests;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000053 RID: 83
	[NullableContext(1)]
	[Nullable(0)]
	public class QuestNetworkHandler
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600031B RID: 795 RVA: 0x0001571B File Offset: 0x0001391B
		// (set) Token: 0x0600031C RID: 796 RVA: 0x00015723 File Offset: 0x00013923
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		public Action<List<QuestDto>> OnQuestListReceived { [return: Nullable(new byte[]
		{
			2,
			1,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1,
			1
		})] set; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x0600031D RID: 797 RVA: 0x0001572C File Offset: 0x0001392C
		// (set) Token: 0x0600031E RID: 798 RVA: 0x00015734 File Offset: 0x00013934
		[Nullable(new byte[]
		{
			2,
			1,
			1,
			1,
			1
		})]
		public Action<List<PlayerQuestProgressDto>, List<string>> OnProgressReceived { [return: Nullable(new byte[]
		{
			2,
			1,
			1,
			1,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1,
			1,
			1,
			1
		})] set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x0600031F RID: 799 RVA: 0x0001573D File Offset: 0x0001393D
		// (set) Token: 0x06000320 RID: 800 RVA: 0x00015745 File Offset: 0x00013945
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestStartResponsePacket> OnQuestStartResponse { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0001574E File Offset: 0x0001394E
		// (set) Token: 0x06000322 RID: 802 RVA: 0x00015756 File Offset: 0x00013956
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestAbandonResponsePacket> OnQuestAbandonResponse { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000323 RID: 803 RVA: 0x0001575F File Offset: 0x0001395F
		// (set) Token: 0x06000324 RID: 804 RVA: 0x00015767 File Offset: 0x00013967
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestSubmitPreviewResponsePacket> OnSubmitPreviewReceived { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000325 RID: 805 RVA: 0x00015770 File Offset: 0x00013970
		// (set) Token: 0x06000326 RID: 806 RVA: 0x00015778 File Offset: 0x00013978
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestSubmitConfirmResponsePacket> OnSubmitConfirmReceived { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000327 RID: 807 RVA: 0x00015781 File Offset: 0x00013981
		// (set) Token: 0x06000328 RID: 808 RVA: 0x00015789 File Offset: 0x00013989
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestCompleteResponsePacket> OnQuestCompleteReceived { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000329 RID: 809 RVA: 0x00015792 File Offset: 0x00013992
		// (set) Token: 0x0600032A RID: 810 RVA: 0x0001579A File Offset: 0x0001399A
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestManagerListResponsePacket> OnQuestManagerListReceived { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x0600032B RID: 811 RVA: 0x000157A3 File Offset: 0x000139A3
		// (set) Token: 0x0600032C RID: 812 RVA: 0x000157AB File Offset: 0x000139AB
		[Nullable(2)]
		public Action OnOpenQuestManager { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600032D RID: 813 RVA: 0x000157B4 File Offset: 0x000139B4
		// (set) Token: 0x0600032E RID: 814 RVA: 0x000157BC File Offset: 0x000139BC
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestSaveResponsePacket> OnQuestSaveResponse { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x0600032F RID: 815 RVA: 0x000157C5 File Offset: 0x000139C5
		// (set) Token: 0x06000330 RID: 816 RVA: 0x000157CD File Offset: 0x000139CD
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<QuestDeleteResponsePacket> OnQuestDeleteResponse { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000331 RID: 817 RVA: 0x000157D6 File Offset: 0x000139D6
		// (set) Token: 0x06000332 RID: 818 RVA: 0x000157DE File Offset: 0x000139DE
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Action<IServerPlayer> OnGuildPointsAwarded { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x06000333 RID: 819 RVA: 0x000157E8 File Offset: 0x000139E8
		public void InitializeServer(ICoreServerAPI api, QuestRepository questRepo, GuildRepository guildRepo, [Nullable(2)] GuildManager guildMgr = null)
		{
			this.serverApi = api;
			this.questRepository = questRepo;
			this.guildRepository = guildRepo;
			this.guildManager = guildMgr;
			this.serverApi.Network.RegisterChannel("srguildsandkingdoms:quest").RegisterMessageType<QuestListRequestPacket>().RegisterMessageType<QuestProgressRequestPacket>().RegisterMessageType<QuestStartRequestPacket>().RegisterMessageType<QuestAbandonRequestPacket>().RegisterMessageType<QuestSubmitPreviewRequestPacket>().RegisterMessageType<QuestSubmitConfirmPacket>().RegisterMessageType<QuestCompleteRequestPacket>().RegisterMessageType<QuestListResponsePacket>().RegisterMessageType<QuestProgressResponsePacket>().RegisterMessageType<QuestStartResponsePacket>().RegisterMessageType<QuestAbandonResponsePacket>().RegisterMessageType<QuestSubmitPreviewResponsePacket>().RegisterMessageType<QuestSubmitConfirmResponsePacket>().RegisterMessageType<QuestCompleteResponsePacket>().RegisterMessageType<QuestManagerListRequestPacket>().RegisterMessageType<QuestManagerListResponsePacket>().RegisterMessageType<OpenQuestManagerPacket>().RegisterMessageType<QuestSaveRequestPacket>().RegisterMessageType<QuestSaveResponsePacket>().RegisterMessageType<QuestDeleteRequestPacket>().RegisterMessageType<QuestDeleteResponsePacket>().RegisterMessageType<QuestDto>().RegisterMessageType<QuestSaveDto>().RegisterMessageType<QuestObjectiveDto>().RegisterMessageType<QuestRewardDto>().RegisterMessageType<QuestAcceptedItemDto>().RegisterMessageType<PlayerQuestProgressDto>().RegisterMessageType<QuestSubmittableItem>().RegisterMessageType<CurrencyDefinitionDto>().SetMessageHandler<QuestListRequestPacket>(new NetworkClientMessageHandler<QuestListRequestPacket>(this.OnQuestListRequest)).SetMessageHandler<QuestProgressRequestPacket>(new NetworkClientMessageHandler<QuestProgressRequestPacket>(this.OnProgressRequest)).SetMessageHandler<QuestStartRequestPacket>(new NetworkClientMessageHandler<QuestStartRequestPacket>(this.OnQuestStartRequest)).SetMessageHandler<QuestAbandonRequestPacket>(new NetworkClientMessageHandler<QuestAbandonRequestPacket>(this.OnQuestAbandonRequest)).SetMessageHandler<QuestSubmitPreviewRequestPacket>(new NetworkClientMessageHandler<QuestSubmitPreviewRequestPacket>(this.OnSubmitPreviewRequest)).SetMessageHandler<QuestSubmitConfirmPacket>(new NetworkClientMessageHandler<QuestSubmitConfirmPacket>(this.OnSubmitConfirm)).SetMessageHandler<QuestCompleteRequestPacket>(new NetworkClientMessageHandler<QuestCompleteRequestPacket>(this.OnQuestCompleteRequest)).SetMessageHandler<QuestManagerListRequestPacket>(new NetworkClientMessageHandler<QuestManagerListRequestPacket>(this.OnQuestManagerListRequest)).SetMessageHandler<QuestSaveRequestPacket>(new NetworkClientMessageHandler<QuestSaveRequestPacket>(this.OnQuestSaveRequest)).SetMessageHandler<QuestDeleteRequestPacket>(new NetworkClientMessageHandler<QuestDeleteRequestPacket>(this.OnQuestDeleteRequest));
			this.serverApi.Logger.Notification("[QuestNetworkHandler] Server-side quest networking initialized");
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00015978 File Offset: 0x00013B78
		public void InitializeClient(ICoreClientAPI api)
		{
			this.clientApi = api;
			this.clientApi.Network.RegisterChannel("srguildsandkingdoms:quest").RegisterMessageType<QuestListRequestPacket>().RegisterMessageType<QuestProgressRequestPacket>().RegisterMessageType<QuestStartRequestPacket>().RegisterMessageType<QuestAbandonRequestPacket>().RegisterMessageType<QuestSubmitPreviewRequestPacket>().RegisterMessageType<QuestSubmitConfirmPacket>().RegisterMessageType<QuestCompleteRequestPacket>().RegisterMessageType<QuestListResponsePacket>().RegisterMessageType<QuestProgressResponsePacket>().RegisterMessageType<QuestStartResponsePacket>().RegisterMessageType<QuestAbandonResponsePacket>().RegisterMessageType<QuestSubmitPreviewResponsePacket>().RegisterMessageType<QuestSubmitConfirmResponsePacket>().RegisterMessageType<QuestCompleteResponsePacket>().RegisterMessageType<QuestManagerListRequestPacket>().RegisterMessageType<QuestManagerListResponsePacket>().RegisterMessageType<OpenQuestManagerPacket>().RegisterMessageType<QuestSaveRequestPacket>().RegisterMessageType<QuestSaveResponsePacket>().RegisterMessageType<QuestDeleteRequestPacket>().RegisterMessageType<QuestDeleteResponsePacket>().RegisterMessageType<QuestDto>().RegisterMessageType<QuestSaveDto>().RegisterMessageType<QuestObjectiveDto>().RegisterMessageType<QuestRewardDto>().RegisterMessageType<QuestAcceptedItemDto>().RegisterMessageType<PlayerQuestProgressDto>().RegisterMessageType<QuestSubmittableItem>().RegisterMessageType<CurrencyDefinitionDto>().SetMessageHandler<QuestListResponsePacket>(new NetworkServerMessageHandler<QuestListResponsePacket>(this.OnQuestListReceivedHandler)).SetMessageHandler<QuestProgressResponsePacket>(new NetworkServerMessageHandler<QuestProgressResponsePacket>(this.OnProgressReceivedHandler)).SetMessageHandler<QuestStartResponsePacket>(new NetworkServerMessageHandler<QuestStartResponsePacket>(this.OnQuestStartResponseReceived)).SetMessageHandler<QuestAbandonResponsePacket>(new NetworkServerMessageHandler<QuestAbandonResponsePacket>(this.OnQuestAbandonResponseReceived)).SetMessageHandler<QuestSubmitPreviewResponsePacket>(new NetworkServerMessageHandler<QuestSubmitPreviewResponsePacket>(this.OnSubmitPreviewReceivedHandler)).SetMessageHandler<QuestSubmitConfirmResponsePacket>(new NetworkServerMessageHandler<QuestSubmitConfirmResponsePacket>(this.OnSubmitConfirmReceivedHandler)).SetMessageHandler<QuestCompleteResponsePacket>(new NetworkServerMessageHandler<QuestCompleteResponsePacket>(this.OnQuestCompleteReceivedHandler)).SetMessageHandler<QuestManagerListResponsePacket>(new NetworkServerMessageHandler<QuestManagerListResponsePacket>(this.OnQuestManagerListReceivedHandler)).SetMessageHandler<OpenQuestManagerPacket>(new NetworkServerMessageHandler<OpenQuestManagerPacket>(this.OnOpenQuestManagerReceived)).SetMessageHandler<QuestSaveResponsePacket>(new NetworkServerMessageHandler<QuestSaveResponsePacket>(this.OnQuestSaveResponseReceived)).SetMessageHandler<QuestDeleteResponsePacket>(new NetworkServerMessageHandler<QuestDeleteResponsePacket>(this.OnQuestDeleteResponseReceived));
			this.clientApi.Logger.Notification("[QuestNetworkHandler] Client-side quest networking initialized");
		}

		// Token: 0x06000335 RID: 821 RVA: 0x00015B04 File Offset: 0x00013D04
		public void RequestQuestList(string playerUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestListRequestPacket packet = new QuestListRequestPacket
			{
				PlayerUid = playerUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestListRequestPacket>(packet);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00015B44 File Offset: 0x00013D44
		public void RequestQuestProgress(string playerUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestProgressRequestPacket packet = new QuestProgressRequestPacket
			{
				PlayerUid = playerUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestProgressRequestPacket>(packet);
		}

		// Token: 0x06000337 RID: 823 RVA: 0x00015B84 File Offset: 0x00013D84
		public void RequestStartQuest(string playerUid, int questId)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestStartRequestPacket packet = new QuestStartRequestPacket
			{
				PlayerUid = playerUid,
				QuestId = questId
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestStartRequestPacket>(packet);
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00015BCC File Offset: 0x00013DCC
		public void RequestAbandonQuest(string playerUid, int questId)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestAbandonRequestPacket packet = new QuestAbandonRequestPacket
			{
				PlayerUid = playerUid,
				QuestId = questId
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestAbandonRequestPacket>(packet);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x00015C14 File Offset: 0x00013E14
		public void RequestSubmitPreview(string playerUid, int questId)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestSubmitPreviewRequestPacket packet = new QuestSubmitPreviewRequestPacket
			{
				PlayerUid = playerUid,
				QuestId = questId
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSubmitPreviewRequestPacket>(packet);
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00015C5C File Offset: 0x00013E5C
		public void ConfirmSubmit(string playerUid, int questId, List<QuestSubmittableItem> items)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestSubmitConfirmPacket packet = new QuestSubmitConfirmPacket
			{
				PlayerUid = playerUid,
				QuestId = questId,
				Items = items
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSubmitConfirmPacket>(packet);
		}

		// Token: 0x0600033B RID: 827 RVA: 0x00015CA8 File Offset: 0x00013EA8
		public void RequestCompleteQuest(string playerUid, int questId)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestCompleteRequestPacket packet = new QuestCompleteRequestPacket
			{
				PlayerUid = playerUid,
				QuestId = questId
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestCompleteRequestPacket>(packet);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00015CF0 File Offset: 0x00013EF0
		public void RequestQuestManagerList(string playerUid)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestManagerListRequestPacket packet = new QuestManagerListRequestPacket
			{
				PlayerUid = playerUid
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestManagerListRequestPacket>(packet);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00015D30 File Offset: 0x00013F30
		public void SaveQuest(string playerUid, QuestSaveDto quest)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestSaveRequestPacket packet = new QuestSaveRequestPacket
			{
				PlayerUid = playerUid,
				Quest = quest
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSaveRequestPacket>(packet);
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00015D78 File Offset: 0x00013F78
		public void DeleteQuest(string playerUid, int questId)
		{
			if (this.clientApi == null)
			{
				return;
			}
			QuestDeleteRequestPacket packet = new QuestDeleteRequestPacket
			{
				PlayerUid = playerUid,
				QuestId = questId
			};
			this.clientApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestDeleteRequestPacket>(packet);
		}

		// Token: 0x0600033F RID: 831 RVA: 0x00015DC0 File Offset: 0x00013FC0
		private void OnQuestListRequest(IServerPlayer player, QuestListRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				Guild guild = this.guildRepository.GetGuildByMember(packet.PlayerUid);
				if (guild == null)
				{
					this.SendQuestListResponse(player, new List<QuestDto>());
				}
				else if (guild.DatabaseId == null)
				{
					this.SendQuestListResponse(player, new List<QuestDto>());
				}
				else
				{
					IGameCalendar calendar = this.serverApi.World.Calendar;
					GameDate ingameDate = new GameDate(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1);
					List<QuestDto> dtos = (from q in this.questRepository.GetAvailableQuestsForPlayer(packet.PlayerUid, new GameDate?(ingameDate))
					select this.MapQuestToDtoWithHistory(q, packet.PlayerUid)).ToList<QuestDto>();
					this.SendQuestListResponse(player, dtos);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to handle quest list request: " + ex.Message);
				this.serverApi.Logger.Error("[QuestNetworkHandler] Stack trace: " + ex.StackTrace);
				this.SendQuestListResponse(player, new List<QuestDto>());
			}
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00015F20 File Offset: 0x00014120
		private void OnProgressRequest(IServerPlayer player, QuestProgressRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				Guild guild = this.guildRepository.GetGuildByMember(packet.PlayerUid);
				if (guild == null)
				{
					this.SendProgressResponse(player, new List<PlayerQuestProgressDto>(), new List<string>());
				}
				else if (guild.DatabaseId == null)
				{
					this.SendProgressResponse(player, new List<PlayerQuestProgressDto>(), new List<string>());
				}
				else
				{
					IGameCalendar calendar = this.serverApi.World.Calendar;
					GameDate ingameDate = new GameDate(calendar.Year + 1, calendar.Month, calendar.DayOfYear % calendar.DaysPerMonth + 1);
					List<PlayerQuestProgress> playerActiveQuests = this.questRepository.GetPlayerActiveQuests(packet.PlayerUid, new GameDate?(ingameDate));
					List<PlayerQuestProgressDto> progressDtos = new List<PlayerQuestProgressDto>();
					foreach (PlayerQuestProgress prog in playerActiveQuests)
					{
						Quest quest = this.questRepository.GetQuest(prog.QuestId);
						if (quest != null)
						{
							progressDtos.Add(QuestNetworkHandler.MapProgressToDto(prog, quest));
						}
					}
					List<string> completedKeys = new List<string>();
					foreach (QuestRecurrenceType type in Enum.GetValues<QuestRecurrenceType>())
					{
						List<string> keys = this.questRepository.GetPlayerCompletedPeriodKeys(packet.PlayerUid, type);
						completedKeys.AddRange(keys);
					}
					this.SendProgressResponse(player, progressDtos, completedKeys);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to handle progress request: " + ex.Message);
				this.SendProgressResponse(player, new List<PlayerQuestProgressDto>(), new List<string>());
			}
		}

		// Token: 0x06000341 RID: 833 RVA: 0x000160FC File Offset: 0x000142FC
		private void OnQuestStartRequest(IServerPlayer player, QuestStartRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				if (this.guildRepository.GetGuildByMember(packet.PlayerUid) == null)
				{
					this.SendQuestStartResponse(player, false, "You are not in a guild", packet.QuestId);
				}
				else
				{
					bool success = this.questRepository.StartQuest(packet.PlayerUid, packet.QuestId);
					string message = success ? "Quest started successfully" : "Failed to start quest (already started or period-locked)";
					this.SendQuestStartResponse(player, success, message, packet.QuestId);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to start quest: " + ex.Message);
				this.SendQuestStartResponse(player, false, "Error: " + ex.Message, packet.QuestId);
			}
		}

		// Token: 0x06000342 RID: 834 RVA: 0x000161D8 File Offset: 0x000143D8
		private void OnQuestAbandonRequest(IServerPlayer player, QuestAbandonRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				if (this.guildRepository.GetGuildByMember(packet.PlayerUid) == null)
				{
					this.SendQuestAbandonResponse(player, false, "You are not in a guild", packet.QuestId);
				}
				else
				{
					bool success = this.questRepository.AbandonQuest(packet.PlayerUid, packet.QuestId);
					string message = success ? "Quest abandoned" : "Failed to abandon quest";
					this.SendQuestAbandonResponse(player, success, message, packet.QuestId);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to abandon quest: " + ex.Message);
				this.SendQuestAbandonResponse(player, false, "Error: " + ex.Message, packet.QuestId);
			}
		}

		// Token: 0x06000343 RID: 835 RVA: 0x000162B4 File Offset: 0x000144B4
		private void OnSubmitPreviewRequest(IServerPlayer player, QuestSubmitPreviewRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				if (this.guildRepository.GetGuildByMember(packet.PlayerUid) == null)
				{
					this.SendSubmitPreviewResponse(player, false, "You are not in a guild", packet.QuestId, new List<QuestSubmittableItem>());
				}
				else
				{
					Quest quest = this.questRepository.GetQuest(packet.QuestId);
					if (quest == null)
					{
						this.SendSubmitPreviewResponse(player, false, "Quest not found", packet.QuestId, new List<QuestSubmittableItem>());
					}
					else
					{
						string questPeriod = quest.GeneratePeriodKey();
						PlayerQuestProgress progress = this.questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
						if (progress == null)
						{
							this.SendSubmitPreviewResponse(player, false, "Quest is not active", packet.QuestId, new List<QuestSubmittableItem>());
						}
						else
						{
							List<QuestSubmittableItem> items = new List<QuestSubmittableItem>();
							foreach (QuestObjective objective in quest.Objectives)
							{
								if (objective.Type.Equals("turn_in", StringComparison.OrdinalIgnoreCase) && objective.AcceptedItems != null && objective.AcceptedItems.Count != 0)
								{
									int currentProgress = progress.GetObjectiveProgress(objective.Id);
									int needed = objective.Count - currentProgress;
									if (needed > 0)
									{
										int totalFound = 0;
										foreach (QuestAcceptedItemDto acceptedItem in objective.AcceptedItems)
										{
											if (totalFound >= needed)
											{
												break;
											}
											AssetLocation assetLocation = new AssetLocation(acceptedItem.Code);
											int available = this.GetTotalItemCount(player, assetLocation, acceptedItem.Nbt);
											if (available > 0)
											{
												int toTake = Math.Min(available, needed - totalFound);
												string displayName = this.GetItemDisplayName(assetLocation);
												items.Add(new QuestSubmittableItem
												{
													ObjectiveId = objective.Id,
													ItemCode = acceptedItem.Code,
													DisplayName = displayName,
													Quantity = toTake
												});
												totalFound += toTake;
											}
										}
									}
								}
							}
							if (items.Count == 0)
							{
								this.SendSubmitPreviewResponse(player, false, "No matching items found in your inventory", packet.QuestId, new List<QuestSubmittableItem>());
							}
							else
							{
								this.SendSubmitPreviewResponse(player, true, "", packet.QuestId, items);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to handle submit preview: " + ex.Message);
				this.SendSubmitPreviewResponse(player, false, "Error: " + ex.Message, packet.QuestId, new List<QuestSubmittableItem>());
			}
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0001659C File Offset: 0x0001479C
		private void OnSubmitConfirm(IServerPlayer player, QuestSubmitConfirmPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			if (player.PlayerUID != packet.PlayerUid)
			{
				return;
			}
			try
			{
				if (this.guildRepository.GetGuildByMember(packet.PlayerUid) == null)
				{
					this.SendSubmitConfirmResponse(player, false, "You are not in a guild", packet.QuestId, 0);
				}
				else
				{
					Quest quest = this.questRepository.GetQuest(packet.QuestId);
					if (quest == null)
					{
						this.SendSubmitConfirmResponse(player, false, "Quest not found", packet.QuestId, 0);
					}
					else
					{
						string questPeriod = quest.GeneratePeriodKey();
						PlayerQuestProgress progress = this.questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
						if (progress == null)
						{
							this.SendSubmitConfirmResponse(player, false, "Quest is not active", packet.QuestId, 0);
						}
						else
						{
							Dictionary<string, string> itemCodeToNbt = new Dictionary<string, string>();
							foreach (QuestObjective objective in quest.Objectives)
							{
								if (objective.AcceptedItems != null)
								{
									foreach (QuestAcceptedItemDto acceptedItem in objective.AcceptedItems)
									{
										if (!itemCodeToNbt.ContainsKey(acceptedItem.Code))
										{
											itemCodeToNbt[acceptedItem.Code] = acceptedItem.Nbt;
										}
									}
								}
							}
							foreach (QuestSubmittableItem item in packet.Items)
							{
								AssetLocation assetLocation = new AssetLocation(item.ItemCode);
								string nbtFilter = itemCodeToNbt.GetValueOrDefault(item.ItemCode);
								int available = this.GetTotalItemCount(player, assetLocation, nbtFilter);
								if (available < item.Quantity)
								{
									bool success = false;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 3);
									defaultInterpolatedStringHandler.AppendLiteral("Not enough ");
									defaultInterpolatedStringHandler.AppendFormatted(item.DisplayName);
									defaultInterpolatedStringHandler.AppendLiteral(" in inventory (need ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(item.Quantity);
									defaultInterpolatedStringHandler.AppendLiteral(", have ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(available);
									defaultInterpolatedStringHandler.AppendLiteral(")");
									this.SendSubmitConfirmResponse(player, success, defaultInterpolatedStringHandler.ToStringAndClear(), packet.QuestId, 0);
									return;
								}
							}
							int totalConsumed = 0;
							using (IEnumerator<IGrouping<int, QuestSubmittableItem>> enumerator4 = (from i in packet.Items
							group i by i.ObjectiveId).GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									IGrouping<int, QuestSubmittableItem> group = enumerator4.Current;
									QuestObjective objective2 = quest.Objectives.Find((QuestObjective o) => o.Id == group.Key);
									if (objective2 != null)
									{
										int objectiveItemCount = 0;
										foreach (QuestSubmittableItem item2 in group)
										{
											AssetLocation assetLocation2 = new AssetLocation(item2.ItemCode);
											string nbtFilter2 = itemCodeToNbt.GetValueOrDefault(item2.ItemCode);
											this.ConsumeItems(player, assetLocation2, item2.Quantity, nbtFilter2);
											objectiveItemCount += item2.Quantity;
											totalConsumed += item2.Quantity;
										}
										progress.AddObjectiveProgress(group.Key, objectiveItemCount, objective2.Count);
									}
								}
							}
							this.questRepository.UpdatePlayerQuestProgress(progress);
							this.SendSubmitConfirmResponse(player, true, "Items submitted successfully", packet.QuestId, totalConsumed);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to confirm submit: " + ex.Message);
				this.SendSubmitConfirmResponse(player, false, "Error: " + ex.Message, packet.QuestId, 0);
			}
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00016A04 File Offset: 0x00014C04
		private void OnQuestCompleteRequest(IServerPlayer player, QuestCompleteRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null || this.guildRepository == null)
			{
				return;
			}
			try
			{
				Guild guild = this.guildRepository.GetGuildByMember(packet.PlayerUid);
				if (guild == null)
				{
					this.SendQuestCompleteResponse(player, false, "You are not in a guild", packet.QuestId, new List<QuestRewardDto>(), "");
				}
				else
				{
					Quest quest = this.questRepository.GetQuest(packet.QuestId);
					if (quest == null)
					{
						this.SendQuestCompleteResponse(player, false, "Quest not found", packet.QuestId, new List<QuestRewardDto>(), "");
					}
					else
					{
						string questPeriod = quest.GeneratePeriodKey();
						PlayerQuestProgress progress = this.questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
						if (progress == null)
						{
							this.SendQuestCompleteResponse(player, false, "Quest is not active", packet.QuestId, new List<QuestRewardDto>(), "");
						}
						else if (!progress.AreAllObjectivesComplete(quest.Objectives))
						{
							this.SendQuestCompleteResponse(player, false, "Not all objectives are complete", packet.QuestId, new List<QuestRewardDto>(), "");
						}
						else
						{
							List<ItemStack> itemRewards = new List<ItemStack>();
							int grsPointsReward = 0;
							foreach (QuestReward reward in quest.Rewards)
							{
								if (reward.Code == "game:grspoints")
								{
									grsPointsReward += reward.Amount;
								}
								else
								{
									ItemStack itemStack = this.BuildRewardItemStack(reward);
									if (itemStack != null)
									{
										itemRewards.Add(itemStack);
									}
								}
							}
							if (itemRewards.Count > 0 && !this.CanFitAll(player, itemRewards.ToArray()))
							{
								this.SendQuestCompleteResponse(player, false, "Not enough inventory space for rewards. Please make room and try again.", packet.QuestId, new List<QuestRewardDto>(), "");
							}
							else
							{
								foreach (ItemStack item in itemRewards)
								{
									player.InventoryManager.TryGiveItemstack(item.Clone(), false);
								}
								if (grsPointsReward > 0)
								{
									guild.Points += grsPointsReward;
									GuildMember member;
									if (guild.Members.TryGetValue(packet.PlayerUid, out member))
									{
										GuildManager guildManager = this.guildManager;
										GuildConfig guildConfig;
										if (guildManager == null)
										{
											guildConfig = null;
										}
										else
										{
											GuildConfigManager configManager = guildManager.GetConfigManager();
											guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
										}
										GuildConfig config = guildConfig;
										if (config != null)
										{
											string memberRank = config.GetMemberRank(member.PointsContribution);
											member.PointsContribution += grsPointsReward;
											string rankAfter = config.GetMemberRank(member.PointsContribution);
											if (memberRank != rankAfter)
											{
												player.SendMessage(GlobalConstants.GeneralChatGroup, "You ranked up to " + rankAfter + "!", 4, null);
											}
										}
										else
										{
											member.PointsContribution += grsPointsReward;
										}
									}
									this.guildRepository.MarkDirty(guild.Name);
									GuildManager guildManager2 = this.guildManager;
									if (guildManager2 != null)
									{
										guildManager2.SyncGuildMemberTraits(guild);
									}
									Action<IServerPlayer> onGuildPointsAwarded = this.OnGuildPointsAwarded;
									if (onGuildPointsAwarded != null)
									{
										onGuildPointsAwarded(player);
									}
								}
								this.questRepository.CompleteQuest(packet.PlayerUid, packet.QuestId);
								IEnumerable<QuestReward> rewards = quest.Rewards;
								Func<QuestReward, QuestRewardDto> selector;
								if ((selector = QuestNetworkHandler.<>O.<0>__MapRewardToDto) == null)
								{
									selector = (QuestNetworkHandler.<>O.<0>__MapRewardToDto = new Func<QuestReward, QuestRewardDto>(QuestNetworkHandler.MapRewardToDto));
								}
								List<QuestRewardDto> rewardDtos = rewards.Select(selector).ToList<QuestRewardDto>();
								string periodKey = quest.GeneratePeriodKey();
								ILogger logger = this.serverApi.Logger;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 3);
								defaultInterpolatedStringHandler.AppendLiteral("[QuestNetworkHandler] Player ");
								defaultInterpolatedStringHandler.AppendFormatted(packet.PlayerUid);
								defaultInterpolatedStringHandler.AppendLiteral(" completed quest ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(packet.QuestId);
								defaultInterpolatedStringHandler.AppendLiteral(" (period: ");
								defaultInterpolatedStringHandler.AppendFormatted(periodKey);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
								this.SendQuestCompleteResponse(player, true, "Quest completed!", packet.QuestId, rewardDtos, periodKey);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to complete quest: " + ex.Message);
				this.SendQuestCompleteResponse(player, false, "Error: " + ex.Message, packet.QuestId, new List<QuestRewardDto>(), "");
			}
		}

		// Token: 0x06000346 RID: 838 RVA: 0x00016E64 File Offset: 0x00015064
		private void OnQuestManagerListRequest(IServerPlayer player, QuestManagerListRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null)
			{
				return;
			}
			try
			{
				if (!player.HasPrivilege(Privilege.controlserver))
				{
					this.SendQuestManagerListResponse(player, false, "You do not have permission to manage quests", new List<QuestDto>(), null, null, 0L, 0.0);
				}
				else
				{
					IEnumerable<Quest> allQuests = this.questRepository.GetAllQuests();
					Func<Quest, QuestDto> selector;
					if ((selector = QuestNetworkHandler.<>O.<1>__MapQuestToDto) == null)
					{
						selector = (QuestNetworkHandler.<>O.<1>__MapQuestToDto = new Func<Quest, QuestDto>(QuestNetworkHandler.MapQuestToDto));
					}
					List<QuestDto> dtos = allQuests.Select(selector).ToList<QuestDto>();
					GuildManager guildManager = this.guildManager;
					GuildConfig guildConfig;
					if (guildManager == null)
					{
						guildConfig = null;
					}
					else
					{
						GuildConfigManager configManager = guildManager.GetConfigManager();
						guildConfig = ((configManager != null) ? configManager.GetConfig() : null);
					}
					GuildConfig config = guildConfig;
					CurrencyDefinitionDto tailsDto = null;
					CurrencyDefinitionDto crownsDto = null;
					if (config != null)
					{
						if (config.QuestTailsDefinition != null)
						{
							tailsDto = new CurrencyDefinitionDto
							{
								Code = config.QuestTailsDefinition.Code,
								Nbt = config.QuestTailsDefinition.Nbt
							};
						}
						if (config.QuestCrownsDefinition != null)
						{
							crownsDto = new CurrencyDefinitionDto
							{
								Code = config.QuestCrownsDefinition.Code,
								Nbt = config.QuestCrownsDefinition.Nbt
							};
						}
					}
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestNetworkHandler] Sending ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(dtos.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" quests to quest manager for player ");
					defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					long serverLocalTime = QuestTimeHelper.NowEasternOffset.ToUnixTimeSeconds();
					double serverTimezoneOffset = QuestTimeHelper.EasternTimezoneOffsetHours;
					this.SendQuestManagerListResponse(player, true, "", dtos, tailsDto, crownsDto, serverLocalTime, serverTimezoneOffset);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to handle quest manager list request: " + ex.Message);
				this.SendQuestManagerListResponse(player, false, "Error: " + ex.Message, new List<QuestDto>(), null, null, 0L, 0.0);
			}
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00017058 File Offset: 0x00015258
		private void OnQuestSaveRequest(IServerPlayer player, QuestSaveRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null)
			{
				return;
			}
			try
			{
				if (!player.HasPrivilege("srguildsandkingdoms:questmanager"))
				{
					this.SendQuestSaveResponse(player, false, "You do not have permission to manage quests", new List<QuestDto>());
				}
				else
				{
					QuestSaveDto dto = packet.Quest;
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestNetworkHandler] Received quest save request - RecurrenceType: '");
					defaultInterpolatedStringHandler.AppendFormatted(dto.RecurrenceType);
					defaultInterpolatedStringHandler.AppendLiteral("', Title: '");
					defaultInterpolatedStringHandler.AppendFormatted(dto.Title);
					defaultInterpolatedStringHandler.AppendLiteral("'");
					logger.Debug(defaultInterpolatedStringHandler.ToStringAndClear());
					QuestRecurrenceType recurrenceType;
					if (string.IsNullOrWhiteSpace(dto.Title))
					{
						this.SendQuestSaveResponse(player, false, "Quest title is required", new List<QuestDto>());
					}
					else if (!Enum.TryParse<QuestRecurrenceType>(dto.RecurrenceType, true, out recurrenceType))
					{
						this.serverApi.Logger.Warning("[QuestNetworkHandler] Failed to parse recurrence type: '" + dto.RecurrenceType + "'");
						this.SendQuestSaveResponse(player, false, "Invalid recurrence type", new List<QuestDto>());
					}
					else
					{
						ILogger logger2 = this.serverApi.Logger;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(46, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("[QuestNetworkHandler] Parsed recurrence type: ");
						defaultInterpolatedStringHandler2.AppendFormatted<QuestRecurrenceType>(recurrenceType);
						logger2.Debug(defaultInterpolatedStringHandler2.ToStringAndClear());
						Quest quest2 = new Quest();
						quest2.RecurrenceType = recurrenceType;
						quest2.Title = dto.Title;
						quest2.Description = dto.Description;
						IEnumerable<QuestObjectiveDto> objectives = dto.Objectives;
						Func<QuestObjectiveDto, QuestObjective> selector;
						if ((selector = QuestNetworkHandler.<>O.<2>__MapDtoToObjective) == null)
						{
							selector = (QuestNetworkHandler.<>O.<2>__MapDtoToObjective = new Func<QuestObjectiveDto, QuestObjective>(QuestNetworkHandler.MapDtoToObjective));
						}
						quest2.Objectives = objectives.Select(selector).ToList<QuestObjective>();
						IEnumerable<QuestRewardDto> rewards = dto.Rewards;
						Func<QuestRewardDto, QuestReward> selector2;
						if ((selector2 = QuestNetworkHandler.<>O.<3>__MapDtoToReward) == null)
						{
							selector2 = (QuestNetworkHandler.<>O.<3>__MapDtoToReward = new Func<QuestRewardDto, QuestReward>(QuestNetworkHandler.MapDtoToReward));
						}
						quest2.Rewards = rewards.Select(selector2).ToList<QuestReward>();
						quest2.StartsAt = dto.StartsAt;
						quest2.ExpiresAt = dto.ExpiresAt;
						quest2.UsesIngameTime = dto.UsesIngameTime;
						quest2.Repeat = dto.Repeat;
						Quest quest = quest2;
						if (dto.Id != null)
						{
							quest.Id = dto.Id.Value;
							if (!this.questRepository.UpdateQuest(quest))
							{
								this.SendQuestSaveResponse(player, false, "Failed to update quest", new List<QuestDto>());
								return;
							}
							ILogger logger3 = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(52, 4);
							defaultInterpolatedStringHandler3.AppendLiteral("[QuestNetworkHandler] Updated quest ");
							defaultInterpolatedStringHandler3.AppendFormatted<int>(quest.Id);
							defaultInterpolatedStringHandler3.AppendLiteral(" '");
							defaultInterpolatedStringHandler3.AppendFormatted(quest.Title);
							defaultInterpolatedStringHandler3.AppendLiteral("' (type: ");
							defaultInterpolatedStringHandler3.AppendFormatted<QuestRecurrenceType>(recurrenceType);
							defaultInterpolatedStringHandler3.AppendLiteral(") by ");
							defaultInterpolatedStringHandler3.AppendFormatted(player.PlayerName);
							logger3.Notification(defaultInterpolatedStringHandler3.ToStringAndClear());
						}
						else
						{
							int newId = this.questRepository.CreateQuest(quest);
							quest.Id = newId;
							ILogger logger4 = this.serverApi.Logger;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(52, 4);
							defaultInterpolatedStringHandler4.AppendLiteral("[QuestNetworkHandler] Created quest ");
							defaultInterpolatedStringHandler4.AppendFormatted<int>(quest.Id);
							defaultInterpolatedStringHandler4.AppendLiteral(" '");
							defaultInterpolatedStringHandler4.AppendFormatted(quest.Title);
							defaultInterpolatedStringHandler4.AppendLiteral("' (type: ");
							defaultInterpolatedStringHandler4.AppendFormatted<QuestRecurrenceType>(recurrenceType);
							defaultInterpolatedStringHandler4.AppendLiteral(") by ");
							defaultInterpolatedStringHandler4.AppendFormatted(player.PlayerName);
							logger4.Notification(defaultInterpolatedStringHandler4.ToStringAndClear());
						}
						IEnumerable<Quest> allQuests = this.questRepository.GetAllQuests();
						Func<Quest, QuestDto> selector3;
						if ((selector3 = QuestNetworkHandler.<>O.<1>__MapQuestToDto) == null)
						{
							selector3 = (QuestNetworkHandler.<>O.<1>__MapQuestToDto = new Func<Quest, QuestDto>(QuestNetworkHandler.MapQuestToDto));
						}
						List<QuestDto> allDtos = allQuests.Select(selector3).ToList<QuestDto>();
						this.SendQuestSaveResponse(player, true, "Quest saved successfully", allDtos);
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to save quest: " + ex.Message);
				this.serverApi.Logger.Error("[QuestNetworkHandler] Stack trace: " + ex.StackTrace);
				this.SendQuestSaveResponse(player, false, "Error: " + ex.Message, new List<QuestDto>());
			}
		}

		// Token: 0x06000348 RID: 840 RVA: 0x00017488 File Offset: 0x00015688
		private void OnQuestDeleteRequest(IServerPlayer player, QuestDeleteRequestPacket packet)
		{
			if (this.serverApi == null || this.questRepository == null)
			{
				return;
			}
			try
			{
				if (!player.HasPrivilege("srguildsandkingdoms:questmanager"))
				{
					this.SendQuestDeleteResponse(player, false, "You do not have permission to manage quests", new List<QuestDto>());
				}
				else if (!this.questRepository.DeleteQuest(packet.QuestId))
				{
					this.SendQuestDeleteResponse(player, false, "Failed to delete quest (quest not found)", new List<QuestDto>());
				}
				else
				{
					ILogger logger = this.serverApi.Logger;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[QuestNetworkHandler] Deleted quest ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(packet.QuestId);
					defaultInterpolatedStringHandler.AppendLiteral(" by ");
					defaultInterpolatedStringHandler.AppendFormatted(player.PlayerName);
					logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
					IEnumerable<Quest> allQuests = this.questRepository.GetAllQuests();
					Func<Quest, QuestDto> selector;
					if ((selector = QuestNetworkHandler.<>O.<1>__MapQuestToDto) == null)
					{
						selector = (QuestNetworkHandler.<>O.<1>__MapQuestToDto = new Func<Quest, QuestDto>(QuestNetworkHandler.MapQuestToDto));
					}
					List<QuestDto> allDtos = allQuests.Select(selector).ToList<QuestDto>();
					this.SendQuestDeleteResponse(player, true, "Quest deleted successfully", allDtos);
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to delete quest: " + ex.Message);
				this.serverApi.Logger.Error("[QuestNetworkHandler] Stack trace: " + ex.StackTrace);
				this.SendQuestDeleteResponse(player, false, "Error: " + ex.Message, new List<QuestDto>());
			}
		}

		// Token: 0x06000349 RID: 841 RVA: 0x000175F8 File Offset: 0x000157F8
		private void SendQuestListResponse(IServerPlayer player, List<QuestDto> quests)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestListResponsePacket response = new QuestListResponsePacket
			{
				Quests = quests
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestListResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00017640 File Offset: 0x00015840
		private void SendProgressResponse(IServerPlayer player, List<PlayerQuestProgressDto> progress, List<string> completedKeys)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestProgressResponsePacket response = new QuestProgressResponsePacket
			{
				Progress = progress,
				CompletedPeriodKeys = completedKeys
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestProgressResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00017690 File Offset: 0x00015890
		private void SendQuestStartResponse(IServerPlayer player, bool success, string message, int questId)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestStartResponsePacket response = new QuestStartResponsePacket
			{
				Success = success,
				Message = message,
				QuestId = questId
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestStartResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034C RID: 844 RVA: 0x000176E8 File Offset: 0x000158E8
		private void SendQuestAbandonResponse(IServerPlayer player, bool success, string message, int questId)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestAbandonResponsePacket response = new QuestAbandonResponsePacket
			{
				Success = success,
				Message = message,
				QuestId = questId
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestAbandonResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034D RID: 845 RVA: 0x00017740 File Offset: 0x00015940
		private void SendSubmitPreviewResponse(IServerPlayer player, bool success, string message, int questId, List<QuestSubmittableItem> items)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestSubmitPreviewResponsePacket response = new QuestSubmitPreviewResponsePacket
			{
				Success = success,
				Message = message,
				QuestId = questId,
				Items = items
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSubmitPreviewResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034E RID: 846 RVA: 0x000177A0 File Offset: 0x000159A0
		private void SendSubmitConfirmResponse(IServerPlayer player, bool success, string message, int questId, int itemsConsumed)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestSubmitConfirmResponsePacket response = new QuestSubmitConfirmResponsePacket
			{
				Success = success,
				Message = message,
				QuestId = questId,
				ItemsConsumed = itemsConsumed
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSubmitConfirmResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x0600034F RID: 847 RVA: 0x00017800 File Offset: 0x00015A00
		private void SendQuestCompleteResponse(IServerPlayer player, bool success, string message, int questId, List<QuestRewardDto> rewardsGranted, string periodKey)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestCompleteResponsePacket response = new QuestCompleteResponsePacket
			{
				Success = success,
				Message = message,
				QuestId = questId,
				RewardsGranted = rewardsGranted,
				PeriodKey = periodKey
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestCompleteResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00017868 File Offset: 0x00015A68
		private void SendQuestManagerListResponse(IServerPlayer player, bool success, string message, List<QuestDto> quests, [Nullable(2)] CurrencyDefinitionDto tailsDefinition, [Nullable(2)] CurrencyDefinitionDto crownsDefinition, long serverLocalTime, double serverTimezoneOffset)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestManagerListResponsePacket response = new QuestManagerListResponsePacket
			{
				Success = success,
				Message = message,
				Quests = quests,
				TailsDefinition = tailsDefinition,
				CrownsDefinition = crownsDefinition,
				ServerLocalTime = serverLocalTime,
				ServerTimezoneOffset = serverTimezoneOffset
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestManagerListResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x06000351 RID: 849 RVA: 0x000178E0 File Offset: 0x00015AE0
		private void SendQuestSaveResponse(IServerPlayer player, bool success, string message, List<QuestDto> allQuests)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestSaveResponsePacket response = new QuestSaveResponsePacket
			{
				Success = success,
				Message = message,
				AllQuests = allQuests
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestSaveResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00017938 File Offset: 0x00015B38
		private void SendQuestDeleteResponse(IServerPlayer player, bool success, string message, List<QuestDto> allQuests)
		{
			if (this.serverApi == null)
			{
				return;
			}
			QuestDeleteResponsePacket response = new QuestDeleteResponsePacket
			{
				Success = success,
				Message = message,
				AllQuests = allQuests
			};
			this.serverApi.Network.GetChannel("srguildsandkingdoms:quest").SendPacket<QuestDeleteResponsePacket>(response, new IServerPlayer[]
			{
				player
			});
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00017990 File Offset: 0x00015B90
		private int GetTotalItemCount(IServerPlayer player, AssetLocation itemCode, [Nullable(2)] string nbtBase64 = null)
		{
			int total = 0;
			foreach (IInventory inventory in player.InventoryManager.Inventories.Values)
			{
				if (!(inventory.ClassName != "backpack") || !(inventory.ClassName != "hotbar"))
				{
					foreach (ItemSlot slot in inventory)
					{
						if (slot.Itemstack != null && slot.Itemstack.Collectible.Code.Equals(itemCode))
						{
							if (!string.IsNullOrEmpty(nbtBase64))
							{
								if (this.NbtAttributesMatch(slot.Itemstack, nbtBase64))
								{
									total += slot.Itemstack.StackSize;
								}
							}
							else
							{
								total += slot.Itemstack.StackSize;
							}
						}
					}
				}
			}
			return total;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00017AA0 File Offset: 0x00015CA0
		private void ConsumeItems(IServerPlayer player, AssetLocation itemCode, int quantityToRemove, [Nullable(2)] string nbtBase64 = null)
		{
			int remaining = quantityToRemove;
			foreach (IInventory inventory in player.InventoryManager.Inventories.Values)
			{
				if (!(inventory.ClassName != "backpack") || !(inventory.ClassName != "hotbar"))
				{
					foreach (ItemSlot slot in inventory)
					{
						if (remaining <= 0)
						{
							break;
						}
						if (slot.Itemstack != null && slot.Itemstack.Collectible.Code.Equals(itemCode) && (string.IsNullOrEmpty(nbtBase64) || this.NbtAttributesMatch(slot.Itemstack, nbtBase64)))
						{
							int take = Math.Min(slot.Itemstack.StackSize, remaining);
							slot.TakeOut(take);
							slot.MarkDirty();
							remaining -= take;
						}
					}
					if (remaining <= 0)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00017BD4 File Offset: 0x00015DD4
		private bool NbtAttributesMatch(ItemStack itemStack, string nbtBase64)
		{
			bool result;
			try
			{
				ITreeAttribute targetNbt = QuestNetworkHandler.DecodeNbtFromBase64(nbtBase64);
				if (targetNbt == null)
				{
					result = false;
				}
				else
				{
					ITreeAttribute itemNbt = itemStack.Attributes ?? new TreeAttribute();
					foreach (KeyValuePair<string, IAttribute> key in targetNbt)
					{
						if (!QuestNetworkHandler.NbtAttributesToIgnore.Contains(key.Key))
						{
							if (!itemNbt.HasAttribute(key.Key))
							{
								return false;
							}
							object value = targetNbt[key.Key];
							IAttribute itemValue = itemNbt[key.Key];
							if (!QuestNetworkHandler.AttributeValuesEqual(value, itemValue))
							{
								return false;
							}
						}
					}
					result = true;
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI != null)
				{
					coreServerAPI.Logger.Warning("[QuestNetworkHandler] Failed to compare NBT attributes: " + ex.Message);
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00017CC4 File Offset: 0x00015EC4
		[return: Nullable(2)]
		private static ITreeAttribute DecodeNbtFromBase64(string nbtBase64)
		{
			ITreeAttribute result;
			try
			{
				using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(nbtBase64)))
				{
					using (BinaryReader reader = new BinaryReader(ms))
					{
						TreeAttribute treeAttribute = new TreeAttribute();
						treeAttribute.FromBytes(reader);
						result = treeAttribute;
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00017D38 File Offset: 0x00015F38
		[NullableContext(2)]
		private static bool AttributeValuesEqual(object value1, object value2)
		{
			if (value1 == null && value2 == null)
			{
				return true;
			}
			if (value1 == null || value2 == null)
			{
				return false;
			}
			ITreeAttribute tree = value1 as ITreeAttribute;
			if (tree != null)
			{
				ITreeAttribute tree2 = value2 as ITreeAttribute;
				if (tree2 != null)
				{
					foreach (KeyValuePair<string, IAttribute> key in tree)
					{
						if (!QuestNetworkHandler.NbtAttributesToIgnore.Contains(key.Key))
						{
							if (!tree2.HasAttribute(key.Key))
							{
								return false;
							}
							if (!QuestNetworkHandler.AttributeValuesEqual(tree[key.Key], tree2[key.Key]))
							{
								return false;
							}
						}
					}
					foreach (KeyValuePair<string, IAttribute> key2 in tree2)
					{
						if (!QuestNetworkHandler.NbtAttributesToIgnore.Contains(key2.Key) && !tree.HasAttribute(key2.Key))
						{
							return false;
						}
					}
					return true;
				}
			}
			return value1.ToString() == value2.ToString() || value1.Equals(value2);
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00017E6C File Offset: 0x0001606C
		private string GetItemDisplayName(AssetLocation assetLocation)
		{
			if (this.serverApi == null)
			{
				return assetLocation.ToString();
			}
			Item item = this.serverApi.World.GetItem(assetLocation);
			if (item != null)
			{
				return item.GetHeldItemName(new ItemStack(item, 1));
			}
			Block block = this.serverApi.World.GetBlock(assetLocation);
			if (block != null)
			{
				return block.GetHeldItemName(new ItemStack(block, 1));
			}
			return assetLocation.ToString();
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00017ED4 File Offset: 0x000160D4
		private bool CanFitAll(IServerPlayer player, ItemStack[] itemsToGive)
		{
			List<ItemStack> virtualItems = (from s in itemsToGive
			select s.Clone()).ToList<ItemStack>();
			foreach (IInventory inventory in player.InventoryManager.Inventories.Values)
			{
				if (!(inventory.ClassName != "backpack") || !(inventory.ClassName != "hotbar"))
				{
					foreach (ItemSlot slot in inventory)
					{
						if (virtualItems.Count == 0)
						{
							return true;
						}
						if (slot.Empty)
						{
							int i = virtualItems.Count - 1;
							while (i >= 0)
							{
								ItemStack target = virtualItems[i];
								if (target.StackSize > 0)
								{
									int canTake = Math.Min(target.Collectible.MaxStackSize, target.StackSize);
									target.StackSize -= canTake;
									if (target.StackSize <= 0)
									{
										virtualItems.RemoveAt(i);
										break;
									}
									break;
								}
								else
								{
									i--;
								}
							}
						}
						else
						{
							for (int j = virtualItems.Count - 1; j >= 0; j--)
							{
								ItemStack target2 = virtualItems[j];
								if (target2.StackSize > 0 && slot.Itemstack.Satisfies(target2))
								{
									int canTake2 = Math.Min(slot.Itemstack.Collectible.MaxStackSize - slot.Itemstack.StackSize, target2.StackSize);
									target2.StackSize -= canTake2;
									if (target2.StackSize <= 0)
									{
										virtualItems.RemoveAt(j);
									}
								}
							}
						}
					}
				}
			}
			return virtualItems.Count == 0;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x000180F4 File Offset: 0x000162F4
		[return: Nullable(2)]
		private ItemStack BuildRewardItemStack(QuestReward reward)
		{
			if (this.serverApi == null)
			{
				return null;
			}
			ItemStack result;
			try
			{
				AssetLocation assetLocation = new AssetLocation(reward.Code);
				Item item = this.serverApi.World.GetItem(assetLocation);
				if (item != null)
				{
					ItemStack itemStack = new ItemStack(item, reward.Amount);
					QuestNetworkHandler.ApplyRewardNbt(itemStack, reward.Nbt);
					result = itemStack;
				}
				else
				{
					Block block = this.serverApi.World.GetBlock(assetLocation);
					if (block != null)
					{
						ItemStack itemStack2 = new ItemStack(block, reward.Amount);
						QuestNetworkHandler.ApplyRewardNbt(itemStack2, reward.Nbt);
						result = itemStack2;
					}
					else
					{
						this.serverApi.Logger.Warning("[QuestNetworkHandler] Could not resolve reward item code: " + reward.Code);
						result = null;
					}
				}
			}
			catch (Exception ex)
			{
				this.serverApi.Logger.Error("[QuestNetworkHandler] Failed to build reward itemstack for " + reward.Code + ": " + ex.Message);
				result = null;
			}
			return result;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x000181E4 File Offset: 0x000163E4
		private static void ApplyRewardNbt(ItemStack stack, [Nullable(2)] string nbtBase64)
		{
			if (string.IsNullOrEmpty(nbtBase64))
			{
				return;
			}
			using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(nbtBase64)))
			{
				using (BinaryReader reader = new BinaryReader(ms))
				{
					stack.Attributes = new TreeAttribute();
					stack.Attributes.FromBytes(reader);
				}
			}
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00018258 File Offset: 0x00016458
		private void OnQuestListReceivedHandler(QuestListResponsePacket packet)
		{
			Action<List<QuestDto>> onQuestListReceived = this.OnQuestListReceived;
			if (onQuestListReceived == null)
			{
				return;
			}
			onQuestListReceived(packet.Quests);
		}

		// Token: 0x0600035D RID: 861 RVA: 0x00018270 File Offset: 0x00016470
		private void OnProgressReceivedHandler(QuestProgressResponsePacket packet)
		{
			Action<List<PlayerQuestProgressDto>, List<string>> onProgressReceived = this.OnProgressReceived;
			if (onProgressReceived == null)
			{
				return;
			}
			onProgressReceived(packet.Progress, packet.CompletedPeriodKeys);
		}

		// Token: 0x0600035E RID: 862 RVA: 0x0001828E File Offset: 0x0001648E
		private void OnQuestStartResponseReceived(QuestStartResponsePacket packet)
		{
			Action<QuestStartResponsePacket> onQuestStartResponse = this.OnQuestStartResponse;
			if (onQuestStartResponse == null)
			{
				return;
			}
			onQuestStartResponse(packet);
		}

		// Token: 0x0600035F RID: 863 RVA: 0x000182A1 File Offset: 0x000164A1
		private void OnQuestAbandonResponseReceived(QuestAbandonResponsePacket packet)
		{
			Action<QuestAbandonResponsePacket> onQuestAbandonResponse = this.OnQuestAbandonResponse;
			if (onQuestAbandonResponse == null)
			{
				return;
			}
			onQuestAbandonResponse(packet);
		}

		// Token: 0x06000360 RID: 864 RVA: 0x000182B4 File Offset: 0x000164B4
		private void OnSubmitPreviewReceivedHandler(QuestSubmitPreviewResponsePacket packet)
		{
			Action<QuestSubmitPreviewResponsePacket> onSubmitPreviewReceived = this.OnSubmitPreviewReceived;
			if (onSubmitPreviewReceived == null)
			{
				return;
			}
			onSubmitPreviewReceived(packet);
		}

		// Token: 0x06000361 RID: 865 RVA: 0x000182C7 File Offset: 0x000164C7
		private void OnSubmitConfirmReceivedHandler(QuestSubmitConfirmResponsePacket packet)
		{
			Action<QuestSubmitConfirmResponsePacket> onSubmitConfirmReceived = this.OnSubmitConfirmReceived;
			if (onSubmitConfirmReceived == null)
			{
				return;
			}
			onSubmitConfirmReceived(packet);
		}

		// Token: 0x06000362 RID: 866 RVA: 0x000182DA File Offset: 0x000164DA
		private void OnQuestCompleteReceivedHandler(QuestCompleteResponsePacket packet)
		{
			Action<QuestCompleteResponsePacket> onQuestCompleteReceived = this.OnQuestCompleteReceived;
			if (onQuestCompleteReceived == null)
			{
				return;
			}
			onQuestCompleteReceived(packet);
		}

		// Token: 0x06000363 RID: 867 RVA: 0x000182ED File Offset: 0x000164ED
		private void OnQuestManagerListReceivedHandler(QuestManagerListResponsePacket packet)
		{
			Action<QuestManagerListResponsePacket> onQuestManagerListReceived = this.OnQuestManagerListReceived;
			if (onQuestManagerListReceived == null)
			{
				return;
			}
			onQuestManagerListReceived(packet);
		}

		// Token: 0x06000364 RID: 868 RVA: 0x00018300 File Offset: 0x00016500
		private void OnOpenQuestManagerReceived(OpenQuestManagerPacket packet)
		{
			Action onOpenQuestManager = this.OnOpenQuestManager;
			if (onOpenQuestManager == null)
			{
				return;
			}
			onOpenQuestManager();
		}

		// Token: 0x06000365 RID: 869 RVA: 0x00018312 File Offset: 0x00016512
		private void OnQuestSaveResponseReceived(QuestSaveResponsePacket packet)
		{
			Action<QuestSaveResponsePacket> onQuestSaveResponse = this.OnQuestSaveResponse;
			if (onQuestSaveResponse == null)
			{
				return;
			}
			onQuestSaveResponse(packet);
		}

		// Token: 0x06000366 RID: 870 RVA: 0x00018325 File Offset: 0x00016525
		private void OnQuestDeleteResponseReceived(QuestDeleteResponsePacket packet)
		{
			Action<QuestDeleteResponsePacket> onQuestDeleteResponse = this.OnQuestDeleteResponse;
			if (onQuestDeleteResponse == null)
			{
				return;
			}
			onQuestDeleteResponse(packet);
		}

		// Token: 0x06000367 RID: 871 RVA: 0x00018338 File Offset: 0x00016538
		private static QuestDto MapQuestToDto(Quest quest)
		{
			QuestDto questDto = new QuestDto();
			questDto.Id = quest.Id;
			questDto.RecurrenceType = quest.RecurrenceType.ToString().ToLowerInvariant();
			questDto.Title = quest.Title;
			questDto.Description = quest.Description;
			IEnumerable<QuestObjective> objectives = quest.Objectives;
			Func<QuestObjective, QuestObjectiveDto> selector;
			if ((selector = QuestNetworkHandler.<>O.<4>__MapObjectiveToDto) == null)
			{
				selector = (QuestNetworkHandler.<>O.<4>__MapObjectiveToDto = new Func<QuestObjective, QuestObjectiveDto>(QuestNetworkHandler.MapObjectiveToDto));
			}
			questDto.Objectives = objectives.Select(selector).ToList<QuestObjectiveDto>();
			IEnumerable<QuestReward> rewards = quest.Rewards;
			Func<QuestReward, QuestRewardDto> selector2;
			if ((selector2 = QuestNetworkHandler.<>O.<0>__MapRewardToDto) == null)
			{
				selector2 = (QuestNetworkHandler.<>O.<0>__MapRewardToDto = new Func<QuestReward, QuestRewardDto>(QuestNetworkHandler.MapRewardToDto));
			}
			questDto.Rewards = rewards.Select(selector2).ToList<QuestRewardDto>();
			questDto.StartsAt = quest.StartsAt;
			questDto.ExpiresAt = quest.ExpiresAt;
			questDto.UsesIngameTime = quest.UsesIngameTime;
			questDto.Repeat = quest.Repeat;
			questDto.PeriodKey = quest.GeneratePeriodKey();
			questDto.AlreadyCompletedLastWeek = false;
			return questDto;
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00018434 File Offset: 0x00016634
		private QuestDto MapQuestToDtoWithHistory(Quest quest, string playerUid)
		{
			QuestDto dto = QuestNetworkHandler.MapQuestToDto(quest);
			if (quest.Repeat && quest.RecurrenceType == QuestRecurrenceType.Daily)
			{
				dto.AlreadyCompletedLastWeek = this.WasQuestCompletedLastWeek(quest, playerUid);
			}
			return dto;
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00018468 File Offset: 0x00016668
		private bool WasQuestCompletedLastWeek(Quest quest, string playerUid)
		{
			if (this.questRepository == null)
			{
				return false;
			}
			bool result;
			try
			{
				DateTime startsAt;
				if (!DateTime.TryParse(quest.StartsAt, out startsAt))
				{
					result = false;
				}
				else
				{
					string sevenDaysAgoStr = QuestPeriodKeyGenerator.FormatDate(startsAt.AddDays(-7.0));
					string lastWeekPeriodKey = QuestPeriodKeyGenerator.GeneratePeriodKey(quest.RecurrenceType, sevenDaysAgoStr, sevenDaysAgoStr, false);
					result = this.questRepository.WasQuestCompletedInPeriod(playerUid, quest.Id, lastWeekPeriodKey);
				}
			}
			catch (Exception ex)
			{
				ICoreServerAPI coreServerAPI = this.serverApi;
				if (coreServerAPI != null)
				{
					coreServerAPI.Logger.Error("[QuestNetworkHandler] Failed to check last week completion: " + ex.Message);
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0001850C File Offset: 0x0001670C
		private static QuestObjectiveDto MapObjectiveToDto(QuestObjective objective)
		{
			return new QuestObjectiveDto
			{
				Id = objective.Id,
				Type = objective.Type,
				Count = objective.Count,
				AcceptedTargets = (objective.AcceptedTargets ?? new List<string>()),
				AcceptedItems = (objective.AcceptedItems ?? new List<QuestAcceptedItemDto>())
			};
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0001856C File Offset: 0x0001676C
		private static QuestRewardDto MapRewardToDto(QuestReward reward)
		{
			return new QuestRewardDto
			{
				Code = reward.Code,
				Nbt = reward.Nbt,
				Amount = reward.Amount
			};
		}

		// Token: 0x0600036C RID: 876 RVA: 0x00018598 File Offset: 0x00016798
		private static PlayerQuestProgressDto MapProgressToDto(PlayerQuestProgress progress, Quest quest)
		{
			PlayerQuestProgressDto playerQuestProgressDto = new PlayerQuestProgressDto();
			playerQuestProgressDto.QuestId = progress.QuestId;
			playerQuestProgressDto.Status = progress.Status.ToString().ToLowerInvariant();
			playerQuestProgressDto.ObjectiveProgress = progress.ObjectiveProgress.ToDictionary((KeyValuePair<int, ObjectiveProgress> kvp) => kvp.Key, (KeyValuePair<int, ObjectiveProgress> kvp) => kvp.Value.Current);
			playerQuestProgressDto.StartedAt = progress.StartedAt;
			playerQuestProgressDto.CompletedAt = progress.CompletedAt;
			playerQuestProgressDto.PeriodKey = (progress.PeriodKey ?? string.Empty);
			playerQuestProgressDto.QuestTitle = quest.Title;
			playerQuestProgressDto.QuestDescription = quest.Description;
			playerQuestProgressDto.RecurrenceType = quest.RecurrenceType.ToString().ToLowerInvariant();
			playerQuestProgressDto.ExpiresAt = quest.ExpiresAt;
			playerQuestProgressDto.UsesIngameTime = quest.UsesIngameTime;
			IEnumerable<QuestObjective> objectives = quest.Objectives;
			Func<QuestObjective, QuestObjectiveDto> selector;
			if ((selector = QuestNetworkHandler.<>O.<4>__MapObjectiveToDto) == null)
			{
				selector = (QuestNetworkHandler.<>O.<4>__MapObjectiveToDto = new Func<QuestObjective, QuestObjectiveDto>(QuestNetworkHandler.MapObjectiveToDto));
			}
			playerQuestProgressDto.Objectives = objectives.Select(selector).ToList<QuestObjectiveDto>();
			IEnumerable<QuestReward> rewards = quest.Rewards;
			Func<QuestReward, QuestRewardDto> selector2;
			if ((selector2 = QuestNetworkHandler.<>O.<0>__MapRewardToDto) == null)
			{
				selector2 = (QuestNetworkHandler.<>O.<0>__MapRewardToDto = new Func<QuestReward, QuestRewardDto>(QuestNetworkHandler.MapRewardToDto));
			}
			playerQuestProgressDto.Rewards = rewards.Select(selector2).ToList<QuestRewardDto>();
			return playerQuestProgressDto;
		}

		// Token: 0x0600036D RID: 877 RVA: 0x00018704 File Offset: 0x00016904
		private static QuestObjective MapDtoToObjective(QuestObjectiveDto dto)
		{
			return new QuestObjective
			{
				Id = dto.Id,
				Type = dto.Type,
				Count = dto.Count,
				AcceptedTargets = dto.AcceptedTargets,
				AcceptedItems = dto.AcceptedItems
			};
		}

		// Token: 0x0600036E RID: 878 RVA: 0x00018752 File Offset: 0x00016952
		private static QuestReward MapDtoToReward(QuestRewardDto dto)
		{
			return new QuestReward
			{
				Code = dto.Code,
				Nbt = dto.Nbt,
				Amount = dto.Amount
			};
		}

		// Token: 0x04000137 RID: 311
		private const string ChannelName = "srguildsandkingdoms:quest";

		// Token: 0x04000138 RID: 312
		[Nullable(2)]
		private ICoreServerAPI serverApi;

		// Token: 0x04000139 RID: 313
		[Nullable(2)]
		private ICoreClientAPI clientApi;

		// Token: 0x0400013A RID: 314
		[Nullable(2)]
		private QuestRepository questRepository;

		// Token: 0x0400013B RID: 315
		[Nullable(2)]
		private GuildRepository guildRepository;

		// Token: 0x0400013C RID: 316
		[Nullable(2)]
		private GuildManager guildManager;

		// Token: 0x0400013D RID: 317
		public static readonly HashSet<string> NbtAttributesToIgnore = new HashSet<string>
		{
			"transitionstate",
			"transitionState",
			"transitionedHours",
			"transitionHoursLeft"
		};

		// Token: 0x02000100 RID: 256
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040004A2 RID: 1186
			[Nullable(0)]
			public static Func<QuestReward, QuestRewardDto> <0>__MapRewardToDto;

			// Token: 0x040004A3 RID: 1187
			[Nullable(0)]
			public static Func<Quest, QuestDto> <1>__MapQuestToDto;

			// Token: 0x040004A4 RID: 1188
			[Nullable(0)]
			public static Func<QuestObjectiveDto, QuestObjective> <2>__MapDtoToObjective;

			// Token: 0x040004A5 RID: 1189
			[Nullable(0)]
			public static Func<QuestRewardDto, QuestReward> <3>__MapDtoToReward;

			// Token: 0x040004A6 RID: 1190
			[Nullable(0)]
			public static Func<QuestObjective, QuestObjectiveDto> <4>__MapObjectiveToDto;
		}
	}
}
