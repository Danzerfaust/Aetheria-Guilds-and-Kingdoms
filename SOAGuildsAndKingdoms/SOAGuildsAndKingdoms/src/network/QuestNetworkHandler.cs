using SOAGuildsAndKingdoms.src.database;
using SOAGuildsAndKingdoms.src.guilds;
using SOAGuildsAndKingdoms.src.quests;
using SOAGuildsAndKingdoms.src.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace SOAGuildsAndKingdoms.src.network
{
    /// <summary>
    /// Handles quest-related network communication between client and server
    /// </summary>
    public class QuestNetworkHandler
    {
        private const string ChannelName = "soaguildsandkingdoms:quest";

        private ICoreServerAPI? serverApi;
        private ICoreClientAPI? clientApi;
        private QuestRepository? questRepository;
        private GuildRepository? guildRepository;
        private GuildManager? guildManager;
        private GuildWeeklyPointsRepository? weeklyPointsRepository;

        // NBT attributes to ignore when comparing items for quest objectives
        public static readonly HashSet<string> NbtAttributesToIgnore = new()
        {
            "transitionstate",
            "transitionState",
            "transitionedHours",
            "transitionHoursLeft"
        };

        // Client-side callbacks (public properties that can be set by tabs)
        public Action<List<QuestDto>>? OnQuestListReceived { get; set; }
        public Action<List<PlayerQuestProgressDto>, List<string>, List<CompletedQuestInfo>>? OnProgressReceived { get; set; }
        public Action<QuestStartResponsePacket>? OnQuestStartResponse { get; set; }
        public Action<QuestAbandonResponsePacket>? OnQuestAbandonResponse { get; set; }
        public Action<QuestSubmitPreviewResponsePacket>? OnSubmitPreviewReceived { get; set; }
        public Action<QuestSubmitConfirmResponsePacket>? OnSubmitConfirmReceived { get; set; }
        public Action<QuestCompleteResponsePacket>? OnQuestCompleteReceived { get; set; }
        public Action<QuestManagerListResponsePacket>? OnQuestManagerListReceived { get; set; }
        public Action? OnOpenQuestManager { get; set; }
        public Action<QuestSaveResponsePacket>? OnQuestSaveResponse { get; set; }
        public Action<QuestDeleteResponsePacket>? OnQuestDeleteResponse { get; set; }

        // Server-side callback for broadcasting guild updates after GRS points awarded
        public Action<IServerPlayer>? OnGuildPointsAwarded { get; set; }

        #region Server-Side Initialization

        /// <summary>
        /// Initialize server-side quest networking
        /// </summary>
        public void InitializeServer(ICoreServerAPI api, QuestRepository questRepo, GuildRepository guildRepo, GuildManager? guildMgr = null, GuildWeeklyPointsRepository? weeklyPointsRepo = null)
        {
            serverApi = api;
            questRepository = questRepo;
            guildRepository = guildRepo;
            guildManager = guildMgr;
            weeklyPointsRepository = weeklyPointsRepo;

            // Register all packet types and handlers
            serverApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<QuestListRequestPacket>()
                .RegisterMessageType<QuestProgressRequestPacket>()
                .RegisterMessageType<QuestStartRequestPacket>()
                .RegisterMessageType<QuestAbandonRequestPacket>()
                .RegisterMessageType<QuestSubmitPreviewRequestPacket>()
                .RegisterMessageType<QuestSubmitConfirmPacket>()
                .RegisterMessageType<QuestCompleteRequestPacket>()
                .RegisterMessageType<QuestListResponsePacket>()
                .RegisterMessageType<QuestProgressResponsePacket>()
                .RegisterMessageType<QuestStartResponsePacket>()
                .RegisterMessageType<QuestAbandonResponsePacket>()
                .RegisterMessageType<QuestSubmitPreviewResponsePacket>()
                .RegisterMessageType<QuestSubmitConfirmResponsePacket>()
                .RegisterMessageType<QuestCompleteResponsePacket>()
                .RegisterMessageType<QuestManagerListRequestPacket>()
                .RegisterMessageType<QuestManagerListResponsePacket>()
                .RegisterMessageType<OpenQuestManagerPacket>()
                .RegisterMessageType<QuestSaveRequestPacket>()
                .RegisterMessageType<QuestSaveResponsePacket>()
                .RegisterMessageType<QuestDeleteRequestPacket>()
                .RegisterMessageType<QuestDeleteResponsePacket>()
                .RegisterMessageType<QuestDto>()
                .RegisterMessageType<QuestSaveDto>()
                .RegisterMessageType<QuestObjectiveDto>()
                .RegisterMessageType<QuestRewardDto>()
                .RegisterMessageType<QuestAcceptedItemDto>()
                .RegisterMessageType<PlayerQuestProgressDto>()
                .RegisterMessageType<QuestSubmittableItem>()
                .RegisterMessageType<CurrencyDefinitionDto>()
                .SetMessageHandler<QuestListRequestPacket>(OnQuestListRequest)
                .SetMessageHandler<QuestProgressRequestPacket>(OnProgressRequest)
                .SetMessageHandler<QuestStartRequestPacket>(OnQuestStartRequest)
                .SetMessageHandler<QuestAbandonRequestPacket>(OnQuestAbandonRequest)
                .SetMessageHandler<QuestSubmitPreviewRequestPacket>(OnSubmitPreviewRequest)
                .SetMessageHandler<QuestSubmitConfirmPacket>(OnSubmitConfirm)
                .SetMessageHandler<QuestCompleteRequestPacket>(OnQuestCompleteRequest)
                .SetMessageHandler<QuestManagerListRequestPacket>(OnQuestManagerListRequest)
                .SetMessageHandler<QuestSaveRequestPacket>(OnQuestSaveRequest)
                .SetMessageHandler<QuestDeleteRequestPacket>(OnQuestDeleteRequest);

            serverApi.Logger.Notification("[QuestNetworkHandler] Server-side quest networking initialized");
        }

        #endregion

        #region Client-Side Initialization

        /// <summary>
        /// Initialize client-side quest networking (registers channel only)
        /// Callbacks should be set via public properties by the quest UI
        /// </summary>
        public void InitializeClient(ICoreClientAPI api)
        {
            clientApi = api;

            // Register all packet types and handlers
            clientApi.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<QuestListRequestPacket>()
                .RegisterMessageType<QuestProgressRequestPacket>()
                .RegisterMessageType<QuestStartRequestPacket>()
                .RegisterMessageType<QuestAbandonRequestPacket>()
                .RegisterMessageType<QuestSubmitPreviewRequestPacket>()
                .RegisterMessageType<QuestSubmitConfirmPacket>()
                .RegisterMessageType<QuestCompleteRequestPacket>()
                .RegisterMessageType<QuestListResponsePacket>()
                .RegisterMessageType<QuestProgressResponsePacket>()
                .RegisterMessageType<QuestStartResponsePacket>()
                .RegisterMessageType<QuestAbandonResponsePacket>()
                .RegisterMessageType<QuestSubmitPreviewResponsePacket>()
                .RegisterMessageType<QuestSubmitConfirmResponsePacket>()
                .RegisterMessageType<QuestCompleteResponsePacket>()
                .RegisterMessageType<QuestManagerListRequestPacket>()
                .RegisterMessageType<QuestManagerListResponsePacket>()
                .RegisterMessageType<OpenQuestManagerPacket>()
                .RegisterMessageType<QuestSaveRequestPacket>()
                .RegisterMessageType<QuestSaveResponsePacket>()
                .RegisterMessageType<QuestDeleteRequestPacket>()
                .RegisterMessageType<QuestDeleteResponsePacket>()
                .RegisterMessageType<QuestDto>()
                .RegisterMessageType<QuestSaveDto>()
                .RegisterMessageType<QuestObjectiveDto>()
                .RegisterMessageType<QuestRewardDto>()
                .RegisterMessageType<QuestAcceptedItemDto>()
                .RegisterMessageType<PlayerQuestProgressDto>()
                .RegisterMessageType<QuestSubmittableItem>()
                .RegisterMessageType<CompletedQuestInfo>()
                .RegisterMessageType<CurrencyDefinitionDto>()
                .SetMessageHandler<QuestListResponsePacket>(OnQuestListReceivedHandler)
                .SetMessageHandler<QuestProgressResponsePacket>(OnProgressReceivedHandler)
                .SetMessageHandler<QuestStartResponsePacket>(OnQuestStartResponseReceived)
                .SetMessageHandler<QuestAbandonResponsePacket>(OnQuestAbandonResponseReceived)
                .SetMessageHandler<QuestSubmitPreviewResponsePacket>(OnSubmitPreviewReceivedHandler)
                .SetMessageHandler<QuestSubmitConfirmResponsePacket>(OnSubmitConfirmReceivedHandler)
                .SetMessageHandler<QuestCompleteResponsePacket>(OnQuestCompleteReceivedHandler)
                .SetMessageHandler<QuestManagerListResponsePacket>(OnQuestManagerListReceivedHandler)
                .SetMessageHandler<OpenQuestManagerPacket>(OnOpenQuestManagerReceived)
                .SetMessageHandler<QuestSaveResponsePacket>(OnQuestSaveResponseReceived)
                .SetMessageHandler<QuestDeleteResponsePacket>(OnQuestDeleteResponseReceived);

            clientApi.Logger.Notification("[QuestNetworkHandler] Client-side quest networking initialized");
        }

        #endregion

        #region Client ? Server Request Methods

        /// <summary>
        /// Request list of available quests
        /// </summary>
        public void RequestQuestList(string playerUid)
        {
            if (clientApi == null) return;

            var packet = new QuestListRequestPacket { PlayerUid = playerUid };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request player's quest progress
        /// </summary>
        public void RequestQuestProgress(string playerUid)
        {
            if (clientApi == null) return;

            var packet = new QuestProgressRequestPacket { PlayerUid = playerUid };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request to start a quest
        /// </summary>
        public void RequestStartQuest(string playerUid, int questId)
        {
            if (clientApi == null) return;

            var packet = new QuestStartRequestPacket { PlayerUid = playerUid, QuestId = questId };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request to abandon a quest
        /// </summary>
        public void RequestAbandonQuest(string playerUid, int questId)
        {
            if (clientApi == null) return;

            var packet = new QuestAbandonRequestPacket { PlayerUid = playerUid, QuestId = questId };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request preview of submittable items
        /// </summary>
        public void RequestSubmitPreview(string playerUid, int questId)
        {
            if (clientApi == null) return;

            var packet = new QuestSubmitPreviewRequestPacket
            {
                PlayerUid = playerUid,
                QuestId = questId,
            };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Confirm item submission
        /// </summary>
        public void ConfirmSubmit(string playerUid, int questId, List<QuestSubmittableItem> items)
        {
            if (clientApi == null) return;

            var packet = new QuestSubmitConfirmPacket
            {
                PlayerUid = playerUid,
                QuestId = questId,
                Items = items
            };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request to complete a quest
        /// </summary>
        public void RequestCompleteQuest(string playerUid, int questId)
        {
            if (clientApi == null) return;

            var packet = new QuestCompleteRequestPacket { PlayerUid = playerUid, QuestId = questId };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Request all quests for admin management
        /// </summary>
        public void RequestQuestManagerList(string playerUid)
        {
            if (clientApi == null) return;

            var packet = new QuestManagerListRequestPacket { PlayerUid = playerUid };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Save (create or update) a quest
        /// </summary>
        public void SaveQuest(string playerUid, QuestSaveDto quest)
        {
            if (clientApi == null) return;

            var packet = new QuestSaveRequestPacket { PlayerUid = playerUid, Quest = quest };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        /// <summary>
        /// Delete a quest
        /// </summary>
        public void DeleteQuest(string playerUid, int questId)
        {
            if (clientApi == null) return;

            var packet = new QuestDeleteRequestPacket { PlayerUid = playerUid, QuestId = questId };
            clientApi.Network.GetChannel(ChannelName).SendPacket(packet);
        }

        #endregion

        #region Server-Side Request Handlers

        private void OnQuestListRequest(IServerPlayer player, QuestListRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                // Get player's guild
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendQuestListResponse(player, []);
                    return;
                }

                // Check if guild has a database ID
                if (!guild.DatabaseId.HasValue)
                {
                    SendQuestListResponse(player, []);
                    return;
                }

                var calendar = serverApi.World.Calendar;
                var ingameDate = new GameDate(
                    calendar.Year + 1,
                    calendar.Month,
                    (calendar.DayOfYear % calendar.DaysPerMonth) + 1
                );

                var config = guildManager?.GetConfigManager()?.GetConfig();
                string guildRank = "D";
                if (config != null)
                {
                    guildRank = config.GetGuildRankClass(guild.Points);
                }

                // Get available quests for this player
                var quests = questRepository.GetAvailableQuestsForPlayer(packet.PlayerUid, ingameDate);

                // Only show quests at or below guild rank)
                var filteredQuests = quests.Where(q => CanAccessQuestRank(guildRank, q.Rank)).ToList();

                var dtos = filteredQuests.Select(q => MapQuestToDtoWithHistory(q, packet.PlayerUid)).ToList();

                SendQuestListResponse(player, dtos);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to handle quest list request: {ex.Message}");
                serverApi.Logger.Error($"[QuestNetworkHandler] Stack trace: {ex.StackTrace}");
                SendQuestListResponse(player, []);
            }
        }

        private void OnProgressRequest(IServerPlayer player, QuestProgressRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendProgressResponse(player, [], [], []);
                    return;
                }

                // Check if guild has a database ID
                if (!guild.DatabaseId.HasValue)
                {
                    SendProgressResponse(player, [], [], []);
                    return;
                }

                // Get current in-game date for filtering IGT quests
                var calendar = serverApi.World.Calendar;
                var ingameDate = new GameDate(
                    calendar.Year + 1,
                    calendar.Month,
                    (calendar.DayOfYear % calendar.DaysPerMonth) + 1
                );

                // Get active quest progress (excludes expired quests)
                var progress = questRepository.GetPlayerActiveQuests(packet.PlayerUid, ingameDate);

                // Map progress to DTOs with quest details
                var progressDtos = new List<PlayerQuestProgressDto>();
                foreach (var prog in progress)
                {
                    var quest = questRepository.GetQuest(prog.QuestId);
                    if (quest != null)
                    {
                        progressDtos.Add(MapProgressToDto(prog, quest));
                    }
                }

                // Get completed period keys for all recurrence types
                var completedKeys = new List<string>();
                foreach (QuestRecurrenceType type in Enum.GetValues<QuestRecurrenceType>())
                {
                    var keys = questRepository.GetPlayerCompletedPeriodKeys(packet.PlayerUid, type);
                    completedKeys.AddRange(keys);
                }

                // Get completed quests with their period keys (for weekly quest filtering)
                var completedQuestsData = questRepository.GetPlayerCompletedQuestsByPeriod(packet.PlayerUid);
                var completedQuests = completedQuestsData
                    .Select(cq => new CompletedQuestInfo { QuestId = cq.questId, PeriodKey = cq.periodKey })
                    .ToList();

                SendProgressResponse(player, progressDtos, completedKeys, completedQuests);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to handle progress request: {ex.Message}");
                SendProgressResponse(player, [], [], []);
            }
        }

        private void OnQuestStartRequest(IServerPlayer player, QuestStartRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendQuestStartResponse(player, false, "You are not in a guild", packet.QuestId);
                    return;
                }

                bool success = questRepository.StartQuest(packet.PlayerUid, packet.QuestId);
                string message = success ? "Quest started successfully" : "Failed to start quest (already started or period-locked)";

                SendQuestStartResponse(player, success, message, packet.QuestId);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to start quest: {ex.Message}");
                SendQuestStartResponse(player, false, $"Error: {ex.Message}", packet.QuestId);
            }
        }

        private void OnQuestAbandonRequest(IServerPlayer player, QuestAbandonRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendQuestAbandonResponse(player, false, "You are not in a guild", packet.QuestId);
                    return;
                }

                bool success = questRepository.AbandonQuest(packet.PlayerUid, packet.QuestId);
                string message = success ? "Quest abandoned" : "Failed to abandon quest";

                SendQuestAbandonResponse(player, success, message, packet.QuestId);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to abandon quest: {ex.Message}");
                SendQuestAbandonResponse(player, false, $"Error: {ex.Message}", packet.QuestId);
            }
        }

        private void OnSubmitPreviewRequest(IServerPlayer player, QuestSubmitPreviewRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendSubmitPreviewResponse(player, false, "You are not in a guild", packet.QuestId, []);
                    return;
                }

                var quest = questRepository.GetQuest(packet.QuestId);
                if (quest == null)
                {
                    SendSubmitPreviewResponse(player, false, "Quest not found", packet.QuestId, []);
                    return;
                }

                var questPeriod = quest.GeneratePeriodKey();

                var progress = questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
                if (progress == null)
                {
                    SendSubmitPreviewResponse(player, false, "Quest is not active", packet.QuestId, []);
                    return;
                }

                // Find all turn_in objectives and check inventory for matching items
                var items = new List<QuestSubmittableItem>();
                foreach (var objective in quest.Objectives)
                {
                    if (!objective.Type.Equals("turn_in", StringComparison.OrdinalIgnoreCase)) continue;
                    if (objective.AcceptedItems == null || objective.AcceptedItems.Count == 0) continue;

                    int currentProgress = progress.GetObjectiveProgress(objective.Id);
                    int needed = objective.Count - currentProgress;
                    if (needed <= 0) continue;

                    int totalFound = 0;
                    foreach (var acceptedItem in objective.AcceptedItems)
                    {
                        if (totalFound >= needed) break;

                        var assetLocation = new AssetLocation(acceptedItem.Code);
                        int available = GetTotalItemCount(player, assetLocation, acceptedItem.Nbt);
                        if (available <= 0) continue;

                        int toTake = Math.Min(available, needed - totalFound);
                        string displayName = GetItemDisplayName(assetLocation);

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

                if (items.Count == 0)
                {
                    SendSubmitPreviewResponse(player, false, "No matching items found in your inventory", packet.QuestId, []);
                    return;
                }

                SendSubmitPreviewResponse(player, true, "", packet.QuestId, items);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to handle submit preview: {ex.Message}");
                SendSubmitPreviewResponse(player, false, $"Error: {ex.Message}", packet.QuestId, []);
            }
        }

        private void OnSubmitConfirm(IServerPlayer player, QuestSubmitConfirmPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            if (player.PlayerUID != packet.PlayerUid) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendSubmitConfirmResponse(player, false, "You are not in a guild", packet.QuestId, 0);
                    return;
                }

                var quest = questRepository.GetQuest(packet.QuestId);
                if (quest == null)
                {
                    SendSubmitConfirmResponse(player, false, "Quest not found", packet.QuestId, 0);
                    return;
                }

                var questPeriod = quest.GeneratePeriodKey();

                var progress = questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
                if (progress == null)
                {
                    SendSubmitConfirmResponse(player, false, "Quest is not active", packet.QuestId, 0);
                    return;
                }

                // Re-validate that the player still has all the items
                // First, build a map of item codes to their NBT from the quest objectives
                var itemCodeToNbt = new Dictionary<string, string?>();
                foreach (var objective in quest.Objectives)
                {
                    if (objective.AcceptedItems != null)
                    {
                        foreach (var acceptedItem in objective.AcceptedItems)
                        {
                            if (!itemCodeToNbt.ContainsKey(acceptedItem.Code))
                            {
                                itemCodeToNbt[acceptedItem.Code] = acceptedItem.Nbt;
                            }
                        }
                    }
                }

                foreach (var item in packet.Items)
                {
                    var assetLocation = new AssetLocation(item.ItemCode);
                    string? nbtFilter = itemCodeToNbt.GetValueOrDefault(item.ItemCode);
                    int available = GetTotalItemCount(player, assetLocation, nbtFilter);
                    if (available < item.Quantity)
                    {
                        SendSubmitConfirmResponse(player, false,
                            $"Not enough {item.DisplayName} in inventory (need {item.Quantity}, have {available})",
                            packet.QuestId, 0);
                        return;
                    }
                }

                // All items validated - consume items and update progress
                int totalConsumed = 0;
                var itemsByObjective = packet.Items.GroupBy(i => i.ObjectiveId);
                foreach (var group in itemsByObjective)
                {
                    var objective = quest.Objectives.Find(o => o.Id == group.Key);
                    if (objective == null) continue;

                    int objectiveItemCount = 0;
                    foreach (var item in group)
                    {
                        var assetLocation = new AssetLocation(item.ItemCode);
                        string? nbtFilter = itemCodeToNbt.GetValueOrDefault(item.ItemCode);
                        ConsumeItems(player, assetLocation, item.Quantity, nbtFilter);
                        objectiveItemCount += item.Quantity;
                        totalConsumed += item.Quantity;
                    }

                    progress.AddObjectiveProgress(group.Key, objectiveItemCount, objective.Count);
                }

                // Save updated progress to the database
                questRepository.UpdatePlayerQuestProgress(progress);

                SendSubmitConfirmResponse(player, true, "Items submitted successfully", packet.QuestId, totalConsumed);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to confirm submit: {ex.Message}");
                SendSubmitConfirmResponse(player, false, $"Error: {ex.Message}", packet.QuestId, 0);
            }
        }

        private void OnQuestCompleteRequest(IServerPlayer player, QuestCompleteRequestPacket packet)
        {
            if (serverApi == null || questRepository == null || guildRepository == null) return;

            try
            {
                var guild = guildRepository.GetGuildByMember(packet.PlayerUid);
                if (guild == null)
                {
                    SendQuestCompleteResponse(player, false, "You are not in a guild", packet.QuestId, [], "");
                    return;
                }

                var quest = questRepository.GetQuest(packet.QuestId);
                if (quest == null)
                {
                    SendQuestCompleteResponse(player, false, "Quest not found", packet.QuestId, [], "");
                    return;
                }

                var questPeriod = quest.GeneratePeriodKey();

                var progress = questRepository.GetPlayerQuestProgress(packet.PlayerUid, packet.QuestId, questPeriod);
                if (progress == null)
                {
                    SendQuestCompleteResponse(player, false, "Quest is not active", packet.QuestId, [], "");
                    return;
                }

                // Verify all objectives are complete
                if (!progress.AreAllObjectivesComplete(quest.Objectives))
                {
                    SendQuestCompleteResponse(player, false, "Not all objectives are complete", packet.QuestId, [], "");
                    return;
                }

                // Separate item rewards from special rewards (GRS points)
                var itemRewards = new List<ItemStack>();
                int grsPointsReward = 0;

                foreach (var reward in quest.Rewards)
                {
                    if (reward.Code == QuestRewardCodes.GrsPoints)
                    {
                        grsPointsReward += reward.Amount;
                        continue;
                    }

                    var itemStack = BuildRewardItemStack(reward);
                    if (itemStack != null)
                    {
                        itemRewards.Add(itemStack);
                    }
                }

                // Check inventory space for item rewards
                if (itemRewards.Count > 0 && !CanFitAll(player, [.. itemRewards]))
                {
                    SendQuestCompleteResponse(player, false,
                        "Not enough inventory space for rewards. Please make room and try again.",
                        packet.QuestId, [], "");
                    return;
                }

                // Give item rewards
                foreach (var item in itemRewards)
                {
                    player.InventoryManager.TryGiveItemstack(item.Clone());
                }

                // Award GRS points
                if (grsPointsReward > 0)
                {
                    bool grsAwarded = false;
                    int actualGrsAwarded = 0;
                    var config = guildManager?.GetConfigManager()?.GetConfig();
                    int weeklyLimit = config?.MaxWeeklyGrsPoints ?? 0;

                    // S-rank guilds are exempt from weekly caps
                    string guildRank = config?.GetGuildRankClass(guild.Points) ?? "D";

                    if (weeklyLimit > 0 && weeklyPointsRepository != null && guildRank != "S")
                    {
                        var weekKey = WeekKeyHelper.GenerateWeekKey(DateTime.Now);
                        var weekStartUnix = WeekKeyHelper.GetWeekStartUnix(DateTime.Now);

                        var guildId = guildRepository.GetGuildIdByName(guild.Name);

                        var currentWeeklyPoints = weeklyPointsRepository.GetWeeklyPointsEarned(guildId, weekKey);

                        var remainingCapacity = weeklyLimit - currentWeeklyPoints;

                        if (remainingCapacity > 0)
                        {
                            actualGrsAwarded = Math.Min(grsPointsReward, remainingCapacity);
                            guild.Points += actualGrsAwarded;
                            weeklyPointsRepository.AddWeeklyPoints(guildId, weekKey, actualGrsAwarded, weekStartUnix);
                            grsAwarded = true;

                            if (actualGrsAwarded < grsPointsReward)
                            {
                                player.SendMessage(GlobalConstants.GeneralChatGroup,
                                    $"Your guild has hit the weekly limit for GRS points. Awarded {actualGrsAwarded} GRS points (limit reached).",
                                    EnumChatType.Notification);
                            }
                        }
                        else
                        {
                            player.SendMessage(GlobalConstants.GeneralChatGroup,
                                "Your guild has hit the weekly limit for GRS points, no GRS points awarded for quest submission.",
                                EnumChatType.Notification);
                        }
                    }
                    else
                    {
                        actualGrsAwarded = grsPointsReward;
                        guild.Points += grsPointsReward;
                        grsAwarded = true;
                    }

                    if (grsAwarded && actualGrsAwarded > 0 && guild.Members.TryGetValue(packet.PlayerUid, out var member))
                    {
                        if (config != null)
                        {
                            string rankBefore = config.GetMemberRank(member.PointsContribution);

                            member.PointsContribution += actualGrsAwarded;

                            string rankAfter = config.GetMemberRank(member.PointsContribution);

                            if (rankBefore != rankAfter)
                            {
                                player.SendMessage(GlobalConstants.GeneralChatGroup,
                                    $"You ranked up to {rankAfter}!",
                                    EnumChatType.Notification);
                            }
                        }
                        else
                        {
                            member.PointsContribution += actualGrsAwarded;
                        }
                    }

                    if (grsAwarded && actualGrsAwarded > 0)
                    {
                        guildRepository.MarkDirty(guild.Name);

                        // Sync traits for all guild members to ensure any trait changes from crossing thresholds are applied immediately
                        guildManager?.SyncGuildMemberTraits(guild);

                        OnGuildPointsAwarded?.Invoke(player);
                    }
                }

                // Mark quest as completed in database
                questRepository.CompleteQuest(packet.PlayerUid, packet.QuestId);

                var rewardDtos = quest.Rewards.Select(MapRewardToDto).ToList();
                var periodKey = quest.GeneratePeriodKey();

                serverApi.Logger.Notification($"[QuestNetworkHandler] Player {packet.PlayerUid} completed quest {packet.QuestId} (period: {periodKey})");
                SendQuestCompleteResponse(player, true, "Quest completed!", packet.QuestId, rewardDtos, periodKey);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to complete quest: {ex.Message}");
                SendQuestCompleteResponse(player, false, $"Error: {ex.Message}", packet.QuestId, [], "");
            }
        }

        private void OnQuestManagerListRequest(IServerPlayer player, QuestManagerListRequestPacket packet)
        {
            if (serverApi == null || questRepository == null) return;

            try
            {
                // Privilege check - require controlserver privilege
                if (!player.HasPrivilege("soaguildsandkingdoms:questmanager"))
                {
                    SendQuestManagerListResponse(player, false, "You do not have permission to manage quests", [], null, null, 0, 0);
                    return;
                }

                // Get ALL quests from the database
                var quests = questRepository.GetAllQuests();
                var dtos = quests.Select(MapQuestToDto).ToList();

                // Get currency definitions from config
                var config = guildManager?.GetConfigManager()?.GetConfig();
                CurrencyDefinitionDto? tailsDto = null;
                CurrencyDefinitionDto? crownsDto = null;

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

                serverApi.Logger.Debug($"[QuestNetworkHandler] Sending {dtos.Count} quests to quest manager for player {player.PlayerName}");

                // Get Eastern Time and timezone offset (quests use ET for consistency)
                var serverLocalTime = QuestTimeHelper.NowEasternOffset.ToUnixTimeSeconds();
                var serverTimezoneOffset = QuestTimeHelper.EasternTimezoneOffsetHours;

                SendQuestManagerListResponse(player, true, "", dtos, tailsDto, crownsDto, serverLocalTime, serverTimezoneOffset);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to handle quest manager list request: {ex.Message}");
                SendQuestManagerListResponse(player, false, $"Error: {ex.Message}", [], null, null, 0, 0);
            }
        }

        private void OnQuestSaveRequest(IServerPlayer player, QuestSaveRequestPacket packet)
        {
            if (serverApi == null || questRepository == null) return;

            try
            {
                // Privilege check 
                if (!player.HasPrivilege("soaguildsandkingdoms:questmanager"))
                {
                    SendQuestSaveResponse(player, false, "You do not have permission to manage quests", []);
                    return;
                }

                var dto = packet.Quest;

                serverApi.Logger.Debug($"[QuestNetworkHandler] Received quest save request - RecurrenceType: '{dto.RecurrenceType}', Title: '{dto.Title}'");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    SendQuestSaveResponse(player, false, "Quest title is required", []);
                    return;
                }

                if (!Enum.TryParse<QuestRecurrenceType>(dto.RecurrenceType, true, out var recurrenceType))
                {
                    serverApi.Logger.Warning($"[QuestNetworkHandler] Failed to parse recurrence type: '{dto.RecurrenceType}'");
                    SendQuestSaveResponse(player, false, "Invalid recurrence type", []);
                    return;
                }

                serverApi.Logger.Debug($"[QuestNetworkHandler] Parsed recurrence type: {recurrenceType}");

                // Map DTO to Quest entity
                var quest = new Quest
                {
                    RecurrenceType = recurrenceType,
                    Title = dto.Title,
                    Description = dto.Description,
                    Objectives = dto.Objectives.Select(MapDtoToObjective).ToList(),
                    Rewards = dto.Rewards.Select(MapDtoToReward).ToList(),
                    StartsAt = dto.StartsAt,
                    ExpiresAt = dto.ExpiresAt,
                    UsesIngameTime = dto.UsesIngameTime,
                    Repeat = dto.Repeat,
                    Rank = dto.Rank
                };

                // Save to database (create or update)
                if (dto.Id.HasValue)
                {
                    // Update existing quest
                    quest.Id = dto.Id.Value;
                    bool updated = questRepository.UpdateQuest(quest);
                    if (!updated)
                    {
                        SendQuestSaveResponse(player, false, "Failed to update quest", []);
                        return;
                    }
                    serverApi.Logger.Notification($"[QuestNetworkHandler] Updated quest {quest.Id} '{quest.Title}' (type: {recurrenceType}) by {player.PlayerName}");
                }
                else
                {
                    // Create new quest
                    int newId = questRepository.CreateQuest(quest);
                    quest.Id = newId;
                    serverApi.Logger.Notification($"[QuestNetworkHandler] Created quest {quest.Id} '{quest.Title}' (type: {recurrenceType}) by {player.PlayerName}");
                }

                // Reload all quests from database for cache refresh
                var allQuests = questRepository.GetAllQuests();
                var allDtos = allQuests.Select(MapQuestToDto).ToList();

                SendQuestSaveResponse(player, true, "Quest saved successfully", allDtos);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to save quest: {ex.Message}");
                serverApi.Logger.Error($"[QuestNetworkHandler] Stack trace: {ex.StackTrace}");
                SendQuestSaveResponse(player, false, $"Error: {ex.Message}", []);
            }
        }

        private void OnQuestDeleteRequest(IServerPlayer player, QuestDeleteRequestPacket packet)
        {
            if (serverApi == null || questRepository == null) return;

            try
            {
                // Privilege check
                if (!player.HasPrivilege("soaguildsandkingdoms:questmanager"))
                {
                    SendQuestDeleteResponse(player, false, "You do not have permission to manage quests", []);
                    return;
                }

                // Delete the quest
                bool deleted = questRepository.DeleteQuest(packet.QuestId);
                if (!deleted)
                {
                    SendQuestDeleteResponse(player, false, "Failed to delete quest (quest not found)", []);
                    return;
                }

                serverApi.Logger.Notification($"[QuestNetworkHandler] Deleted quest {packet.QuestId} by {player.PlayerName}");

                // Reload all quests from database for cache refresh
                var allQuests = questRepository.GetAllQuests();
                var allDtos = allQuests.Select(MapQuestToDto).ToList();

                SendQuestDeleteResponse(player, true, "Quest deleted successfully", allDtos);
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to delete quest: {ex.Message}");
                serverApi.Logger.Error($"[QuestNetworkHandler] Stack trace: {ex.StackTrace}");
                SendQuestDeleteResponse(player, false, $"Error: {ex.Message}", []);
            }
        }

        #endregion

        #region Server ? Client Response Methods

        private void SendQuestListResponse(IServerPlayer player, List<QuestDto> quests)
        {
            if (serverApi == null) return;

            var response = new QuestListResponsePacket { Quests = quests };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendProgressResponse(IServerPlayer player, List<PlayerQuestProgressDto> progress, List<string> completedKeys, List<CompletedQuestInfo> completedQuests)
        {
            if (serverApi == null) return;

            var response = new QuestProgressResponsePacket
            {
                Progress = progress,
                CompletedPeriodKeys = completedKeys,
                CompletedQuests = completedQuests
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestStartResponse(IServerPlayer player, bool success, string message, int questId)
        {
            if (serverApi == null) return;

            var response = new QuestStartResponsePacket
            {
                Success = success,
                Message = message,
                QuestId = questId
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestAbandonResponse(IServerPlayer player, bool success, string message, int questId)
        {
            if (serverApi == null) return;

            var response = new QuestAbandonResponsePacket
            {
                Success = success,
                Message = message,
                QuestId = questId
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendSubmitPreviewResponse(IServerPlayer player, bool success, string message, int questId, List<QuestSubmittableItem> items)
        {
            if (serverApi == null) return;

            var response = new QuestSubmitPreviewResponsePacket
            {
                Success = success,
                Message = message,
                QuestId = questId,
                Items = items
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendSubmitConfirmResponse(IServerPlayer player, bool success, string message, int questId, int itemsConsumed)
        {
            if (serverApi == null) return;

            var response = new QuestSubmitConfirmResponsePacket
            {
                Success = success,
                Message = message,
                QuestId = questId,
                ItemsConsumed = itemsConsumed
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestCompleteResponse(IServerPlayer player, bool success, string message, int questId, List<QuestRewardDto> rewardsGranted, string periodKey)
        {
            if (serverApi == null) return;

            var response = new QuestCompleteResponsePacket
            {
                Success = success,
                Message = message,
                QuestId = questId,
                RewardsGranted = rewardsGranted,
                PeriodKey = periodKey
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestManagerListResponse(IServerPlayer player, bool success, string message, List<QuestDto> quests, CurrencyDefinitionDto? tailsDefinition, CurrencyDefinitionDto? crownsDefinition, long serverLocalTime, double serverTimezoneOffset)
        {
            if (serverApi == null) return;

            var response = new QuestManagerListResponsePacket
            {
                Success = success,
                Message = message,
                Quests = quests,
                TailsDefinition = tailsDefinition,
                CrownsDefinition = crownsDefinition,
                ServerLocalTime = serverLocalTime,
                ServerTimezoneOffset = serverTimezoneOffset
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestSaveResponse(IServerPlayer player, bool success, string message, List<QuestDto> allQuests)
        {
            if (serverApi == null) return;

            var response = new QuestSaveResponsePacket
            {
                Success = success,
                Message = message,
                AllQuests = allQuests
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        private void SendQuestDeleteResponse(IServerPlayer player, bool success, string message, List<QuestDto> allQuests)
        {
            if (serverApi == null) return;

            var response = new QuestDeleteResponsePacket
            {
                Success = success,
                Message = message,
                AllQuests = allQuests
            };
            serverApi.Network.GetChannel(ChannelName).SendPacket(response, player);
        }

        #endregion

        #region Inventory Helpers

        /// <summary>
        /// Gets the total count of a specific item across the player's carried inventories
        /// </summary>
        /// <param name="player">The player whose inventory to search</param>
        /// <param name="itemCode">The item code to match</param>
        /// <param name="nbtBase64">Optional Base64-encoded NBT data to match (null = any NBT)</param>
        private int GetTotalItemCount(IServerPlayer player, AssetLocation itemCode, string? nbtBase64 = null)
        {
            int total = 0;
            foreach (var inventory in player.InventoryManager.Inventories.Values)
            {
                if (inventory.ClassName != GlobalConstants.backpackInvClassName &&
                    inventory.ClassName != GlobalConstants.hotBarInvClassName) continue;

                foreach (var slot in inventory)
                {
                    if (slot.Itemstack != null && slot.Itemstack.Collectible.Code.Equals(itemCode))
                    {
                        // If NBT filter is specified, check if the item's NBT matches
                        if (!string.IsNullOrEmpty(nbtBase64))
                        {
                            if (NbtAttributesMatch(slot.Itemstack, nbtBase64))
                            {
                                total += slot.Itemstack.StackSize;
                            }
                        }
                        else
                        {
                            // No NBT filter - count all items with matching code
                            total += slot.Itemstack.StackSize;
                        }
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// Consumes a specific quantity of an item from the player's carried inventories
        /// </summary>
        /// <param name="player">The player whose inventory to consume from</param>
        /// <param name="itemCode">The item code to match</param>
        /// <param name="quantityToRemove">The quantity to remove</param>
        /// <param name="nbtBase64">Optional Base64-encoded NBT data to match (null = any NBT)</param>
        private void ConsumeItems(IServerPlayer player, AssetLocation itemCode, int quantityToRemove, string? nbtBase64 = null)
        {
            int remaining = quantityToRemove;

            foreach (var inventory in player.InventoryManager.Inventories.Values)
            {
                if (inventory.ClassName != GlobalConstants.backpackInvClassName &&
                    inventory.ClassName != GlobalConstants.hotBarInvClassName) continue;

                foreach (var slot in inventory)
                {
                    if (remaining <= 0) break;

                    if (slot.Itemstack != null && slot.Itemstack.Collectible.Code.Equals(itemCode))
                    {
                        // If NBT filter is specified, check if the item's NBT matches
                        bool matches = string.IsNullOrEmpty(nbtBase64) || NbtAttributesMatch(slot.Itemstack, nbtBase64);

                        if (matches)
                        {
                            int take = Math.Min(slot.Itemstack.StackSize, remaining);
                            slot.TakeOut(take);
                            slot.MarkDirty();
                            remaining -= take;
                        }
                    }
                }

                if (remaining <= 0) break;
            }
        }

        /// <summary>
        /// Compares an ItemStack's NBT attributes with a Base64-encoded NBT string,
        /// ignoring attributes specified in NbtAttributesToIgnore.
        /// </summary>
        /// <param name="itemStack">The ItemStack to compare</param>
        /// <param name="nbtBase64">Base64-encoded NBT data to compare against</param>
        /// <returns>True if the NBT attributes match (ignoring filtered attributes)</returns>
        private bool NbtAttributesMatch(ItemStack itemStack, string nbtBase64)
        {
            try
            {
                // Decode the target NBT from Base64
                var targetNbt = DecodeNbtFromBase64(nbtBase64);
                if (targetNbt == null) return false;

                // Get the item's current NBT (or empty if none)
                var itemNbt = itemStack.Attributes ?? new Vintagestory.API.Datastructures.TreeAttribute();

                // Compare all attributes in target NBT, ignoring filtered ones
                foreach (var key in targetNbt)
                {
                    // Skip ignored attributes
                    if (NbtAttributesToIgnore.Contains(key.Key)) continue;

                    // Check if the item has this attribute and if it matches
                    if (!itemNbt.HasAttribute(key.Key))
                    {
                        // Target has an attribute that item doesn't have
                        return false;
                    }

                    var targetValue = targetNbt[key.Key];
                    var itemValue = itemNbt[key.Key];

                    // Compare the attribute values
                    if (!AttributeValuesEqual(targetValue, itemValue))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                serverApi?.Logger.Warning($"[QuestNetworkHandler] Failed to compare NBT attributes: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decodes Base64-encoded NBT data into a TreeAttribute
        /// </summary>
        private static Vintagestory.API.Datastructures.ITreeAttribute? DecodeNbtFromBase64(string nbtBase64)
        {
            try
            {
                var nbtBytes = Convert.FromBase64String(nbtBase64);
                using var ms = new System.IO.MemoryStream(nbtBytes);
                using var reader = new System.IO.BinaryReader(ms);
                var attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                attributes.FromBytes(reader);
                return attributes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Compares two attribute values for equality
        /// </summary>
        private static bool AttributeValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Handle TreeAttribute (nested attributes)
            if (value1 is Vintagestory.API.Datastructures.ITreeAttribute tree1 &&
                value2 is Vintagestory.API.Datastructures.ITreeAttribute tree2)
            {
                // Compare all keys in tree1 with tree2, ignoring filtered attributes
                foreach (var key in tree1)
                {
                    if (NbtAttributesToIgnore.Contains(key.Key)) continue;

                    if (!tree2.HasAttribute(key.Key))
                        return false;

                    if (!AttributeValuesEqual(tree1[key.Key], tree2[key.Key]))
                        return false;
                }

                // Check that tree2 doesn't have extra keys (excluding filtered ones)
                foreach (var key in tree2)
                {
                    if (NbtAttributesToIgnore.Contains(key.Key)) continue;

                    if (!tree1.HasAttribute(key.Key))
                        return false;
                }

                return true;
            }

            // For other types, use default equality
            return value1.ToString() == value2.ToString() || value1.Equals(value2);
        }

        /// <summary>
        /// Gets the localized display name for an item or block code
        /// </summary>
        private string GetItemDisplayName(AssetLocation assetLocation)
        {
            if (serverApi == null) return assetLocation.ToString();

            var item = serverApi.World.GetItem(assetLocation);
            if (item != null)
            {
                return item.GetHeldItemName(new ItemStack(item, 1));
            }

            var block = serverApi.World.GetBlock(assetLocation);
            if (block != null)
            {
                return block.GetHeldItemName(new ItemStack(block, 1));
            }

            return assetLocation.ToString();
        }

        /// <summary>
        /// Checks whether the player's carried inventories have enough space for all given items
        /// without actually modifying any slots (dry-run simulation).
        /// </summary>
        private bool CanFitAll(IServerPlayer player, ItemStack[] itemsToGive)
        {
            // Clone so we can decrement StackSize during simulation
            List<ItemStack> virtualItems = itemsToGive.Select(s => s.Clone()).ToList();

            foreach (var inventory in player.InventoryManager.Inventories.Values)
            {
                if (inventory.ClassName != GlobalConstants.backpackInvClassName &&
                    inventory.ClassName != GlobalConstants.hotBarInvClassName) continue;

                foreach (var slot in inventory)
                {
                    if (virtualItems.Count == 0) return true;

                    if (slot.Empty)
                    {
                        // An empty slot can accept one item type � pick the first that still needs space
                        for (int i = virtualItems.Count - 1; i >= 0; i--)
                        {
                            var target = virtualItems[i];
                            if (target.StackSize <= 0) continue;

                            int canTake = Math.Min(target.Collectible.MaxStackSize, target.StackSize);
                            target.StackSize -= canTake;

                            if (target.StackSize <= 0) virtualItems.RemoveAt(i);
                            break; // slot is now "occupied" in our simulation
                        }
                    }
                    else
                    {
                        // Occupied slot � only the matching item type can stack into remaining space
                        for (int i = virtualItems.Count - 1; i >= 0; i--)
                        {
                            var target = virtualItems[i];
                            if (target.StackSize <= 0) continue;

                            if (slot.Itemstack.Satisfies(target))
                            {
                                int remainingSpace = slot.Itemstack.Collectible.MaxStackSize - slot.Itemstack.StackSize;
                                int canTake = Math.Min(remainingSpace, target.StackSize);
                                target.StackSize -= canTake;

                                if (target.StackSize <= 0) virtualItems.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            return virtualItems.Count == 0;
        }

        /// <summary>
        /// Creates an ItemStack from a quest reward definition, applying optional NBT data.
        /// Returns null for unresolvable codes.
        /// </summary>
        private ItemStack? BuildRewardItemStack(QuestReward reward)
        {
            if (serverApi == null) return null;

            try
            {
                var assetLocation = new AssetLocation(reward.Code);

                var item = serverApi.World.GetItem(assetLocation);
                if (item != null)
                {
                    var stack = new ItemStack(item, reward.Amount);
                    ApplyRewardNbt(stack, reward.Nbt);
                    return stack;
                }

                var block = serverApi.World.GetBlock(assetLocation);
                if (block != null)
                {
                    var stack = new ItemStack(block, reward.Amount);
                    ApplyRewardNbt(stack, reward.Nbt);
                    return stack;
                }

                serverApi.Logger.Warning($"[QuestNetworkHandler] Could not resolve reward item code: {reward.Code}");
                return null;
            }
            catch (Exception ex)
            {
                serverApi.Logger.Error($"[QuestNetworkHandler] Failed to build reward itemstack for {reward.Code}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Applies Base64-encoded NBT data to an ItemStack if present.
        /// </summary>
        private static void ApplyRewardNbt(ItemStack stack, string? nbtBase64)
        {
            if (string.IsNullOrEmpty(nbtBase64)) return;

            var nbtBytes = Convert.FromBase64String(nbtBase64);
            using var ms = new System.IO.MemoryStream(nbtBytes);
            using var reader = new System.IO.BinaryReader(ms);
            stack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
            stack.Attributes.FromBytes(reader);
        }

        #endregion

        #region Client-Side Response Handlers

        private void OnQuestListReceivedHandler(QuestListResponsePacket packet)
        {
            OnQuestListReceived?.Invoke(packet.Quests);
        }

        private void OnProgressReceivedHandler(QuestProgressResponsePacket packet)
        {
            OnProgressReceived?.Invoke(packet.Progress, packet.CompletedPeriodKeys, packet.CompletedQuests);
        }

        private void OnQuestStartResponseReceived(QuestStartResponsePacket packet)
        {
            OnQuestStartResponse?.Invoke(packet);
        }

        private void OnQuestAbandonResponseReceived(QuestAbandonResponsePacket packet)
        {
            OnQuestAbandonResponse?.Invoke(packet);
        }

        private void OnSubmitPreviewReceivedHandler(QuestSubmitPreviewResponsePacket packet)
        {
            OnSubmitPreviewReceived?.Invoke(packet);
        }

        private void OnSubmitConfirmReceivedHandler(QuestSubmitConfirmResponsePacket packet)
        {
            OnSubmitConfirmReceived?.Invoke(packet);
        }

        private void OnQuestCompleteReceivedHandler(QuestCompleteResponsePacket packet)
        {
            OnQuestCompleteReceived?.Invoke(packet);
        }

        private void OnQuestManagerListReceivedHandler(QuestManagerListResponsePacket packet)
        {
            OnQuestManagerListReceived?.Invoke(packet);
        }

        private void OnOpenQuestManagerReceived(OpenQuestManagerPacket packet)
        {
            OnOpenQuestManager?.Invoke();
        }

        private void OnQuestSaveResponseReceived(QuestSaveResponsePacket packet)
        {
            OnQuestSaveResponse?.Invoke(packet);
        }

        private void OnQuestDeleteResponseReceived(QuestDeleteResponsePacket packet)
        {
            OnQuestDeleteResponse?.Invoke(packet);
        }

        #endregion

        #region Rank Helpers

        /// <summary>
        /// Gets the numeric value for a rank
        /// </summary>
        private static int GetRankValue(string rank)
        {
            return rank.ToUpperInvariant() switch
            {
                "D" => 0,
                "C" => 1,
                "B" => 2,
                "A" => 3,
                "S" => 4,
                _ => 0 // Default to D if unknown
            };
        }

        /// <summary>
        /// Checks if questRank is less than or equal to playerRank
        /// </summary>
        private static bool CanAccessQuestRank(string playerRank, string questRank)
        {
            return GetRankValue(questRank) <= GetRankValue(playerRank);
        }

        #endregion

        #region Mapping Helpers

        private static QuestDto MapQuestToDto(Quest quest)
        {
            return new QuestDto
            {
                Id = quest.Id,
                RecurrenceType = quest.RecurrenceType.ToString().ToLowerInvariant(),
                Title = quest.Title,
                Description = quest.Description,
                Objectives = quest.Objectives.Select(MapObjectiveToDto).ToList(),
                Rewards = quest.Rewards.Select(MapRewardToDto).ToList(),
                StartsAt = quest.StartsAt,
                ExpiresAt = quest.ExpiresAt,
                UsesIngameTime = quest.UsesIngameTime,
                Repeat = quest.Repeat,
                PeriodKey = quest.GeneratePeriodKey(),
                AlreadyCompletedLastWeek = false,
                Rank = quest.Rank
            };
        }

        /// <summary>
        /// Maps a Quest to QuestDto
        /// </summary>
        private QuestDto MapQuestToDtoWithHistory(Quest quest, string playerUid)
        {
            var dto = MapQuestToDto(quest);

            return dto;
        }

        private static QuestObjectiveDto MapObjectiveToDto(QuestObjective objective)
        {
            return new QuestObjectiveDto
            {
                Id = objective.Id,
                Type = objective.Type,
                Count = objective.Count,
                AcceptedTargets = objective.AcceptedTargets ?? [],
                AcceptedItems = objective.AcceptedItems ?? []
            };
        }

        private static QuestRewardDto MapRewardToDto(QuestReward reward)
        {
            return new QuestRewardDto
            {
                Code = reward.Code,
                Nbt = reward.Nbt,
                Amount = reward.Amount
            };
        }

        private static PlayerQuestProgressDto MapProgressToDto(PlayerQuestProgress progress, Quest quest)
        {
            return new PlayerQuestProgressDto
            {
                QuestId = progress.QuestId,
                Status = progress.Status.ToString().ToLowerInvariant(),
                ObjectiveProgress = progress.ObjectiveProgress.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Current
                ),
                StartedAt = progress.StartedAt,
                CompletedAt = progress.CompletedAt,
                PeriodKey = progress.PeriodKey ?? string.Empty,
                QuestTitle = quest.Title,
                QuestDescription = quest.Description,
                RecurrenceType = quest.RecurrenceType.ToString().ToLowerInvariant(),
                ExpiresAt = quest.ExpiresAt,
                UsesIngameTime = quest.UsesIngameTime,
                Objectives = quest.Objectives.Select(MapObjectiveToDto).ToList(),
                Rewards = quest.Rewards.Select(MapRewardToDto).ToList(),
                Rank = quest.Rank
            };
        }

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

        private static QuestReward MapDtoToReward(QuestRewardDto dto)
        {
            return new QuestReward
            {
                Code = dto.Code,
                Nbt = dto.Nbt,
                Amount = dto.Amount
            };
        }

        #endregion
    }
}
