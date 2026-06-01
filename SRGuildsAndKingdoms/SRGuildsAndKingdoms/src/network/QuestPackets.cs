using ProtoBuf;
using System.Collections.Generic;

namespace SRGuildsAndKingdoms.src.network
{
    // ============================================================================
    // CLIENT > SERVER PACKETS
    // ============================================================================

    /// <summary>
    /// Request list of available quests for the player
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestListRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request the player's current progress on all active quests
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestProgressRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to start a quest
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestStartRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Request to abandon an active quest
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestAbandonRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Request preview of items that can be submitted for a turn_in objective
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSubmitPreviewRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Confirm submission of items for turn_in objectives
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSubmitConfirmPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
        /// <summary>
        /// Items the client is confirming to submit (for server re-validation)
        /// </summary>
        public List<QuestSubmittableItem> Items { get; set; } = [];
    }

    /// <summary>
    /// Request to complete a quest and claim rewards
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestCompleteRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    // ============================================================================
    // SERVER > CLIENT PACKETS
    // ============================================================================

    /// <summary>
    /// Response with list of available quests
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestListResponsePacket
    {
        public List<QuestDto> Quests { get; set; } = [];
    }

    /// <summary>
    /// Represents a completed quest for a specific period
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CompletedQuestInfo
    {
        public int QuestId { get; set; }
        public string PeriodKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response with player's quest progress
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestProgressResponsePacket
    {
        public List<PlayerQuestProgressDto> Progress { get; set; } = [];
        /// <summary>
        /// Period keys the player has already completed (for UI locking)
        /// </summary>
        public List<string> CompletedPeriodKeys { get; set; } = [];
        /// <summary>
        /// List of completed quests with their period keys (for weekly quest filtering)
        /// </summary>
        public List<CompletedQuestInfo> CompletedQuests { get; set; } = [];
    }

    /// <summary>
    /// Response to quest start request
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestStartResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Response to quest abandon request
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestAbandonResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Response with preview of items that can be submitted across all turn_in objectives
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSubmitPreviewResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int QuestId { get; set; }
        /// <summary>
        /// Items available to submit from player's inventory (each carries its ObjectiveId)
        /// </summary>
        public List<QuestSubmittableItem> Items { get; set; } = [];
    }

    /// <summary>
    /// Response after confirming item submission
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSubmitConfirmResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int QuestId { get; set; }
        public int ItemsConsumed { get; set; }
    }

    /// <summary>
    /// Response to quest completion request
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestCompleteResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int QuestId { get; set; }
        /// <summary>
        /// Rewards that were granted (for display purposes)
        /// </summary>
        public List<QuestRewardDto> RewardsGranted { get; set; } = [];
        /// <summary>
        /// Period key that was locked (for UI update)
        /// </summary>
        public string PeriodKey { get; set; } = string.Empty;
    }

    // ============================================================================
    // DATA TRANSFER OBJECTS (DTOs)
    // ============================================================================

    /// <summary>
    /// Lightweight quest definition for client
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestDto
    {
        public int Id { get; set; }
        public string RecurrenceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<QuestObjectiveDto> Objectives { get; set; } = [];
        public List<QuestRewardDto> Rewards { get; set; } = [];
        public string StartsAt { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public bool UsesIngameTime { get; set; }
        public bool Repeat { get; set; }
        public string PeriodKey { get; set; } = string.Empty;
        public bool AlreadyCompletedLastWeek { get; set; }
        public string Rank { get; set; } = "D";
    }

    /// <summary>
    /// Quest objective DTO
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestObjectiveDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<string> AcceptedTargets { get; set; } = [];
        public List<QuestAcceptedItemDto> AcceptedItems { get; set; } = [];
    }

    /// <summary>
    /// Quest reward DTO
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestRewardDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Nbt { get; set; } // Base64-encoded NBT data
        public int Amount { get; set; }
    }

    /// <summary>
    /// Accepted item DTO (for quest objectives)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestAcceptedItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Nbt { get; set; } // Base64-encoded NBT data
    }

    /// <summary>
    /// Player quest progress DTO
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PlayerQuestProgressDto
    {
        public int QuestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<int, int> ObjectiveProgress { get; set; } = [];
        public long StartedAt { get; set; }
        public long? CompletedAt { get; set; }
        public string PeriodKey { get; set; } = string.Empty;

        // Quest details (so we don't need to look them up separately)
        public string QuestTitle { get; set; } = string.Empty;
        public string QuestDescription { get; set; } = string.Empty;
        public string RecurrenceType { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public bool UsesIngameTime { get; set; }
        public List<QuestObjectiveDto> Objectives { get; set; } = [];
        public List<QuestRewardDto> Rewards { get; set; } = [];
        public string Rank { get; set; } = "D";
    }

    /// <summary>
    /// Represents an item that can be submitted for a quest objective
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSubmittableItem
    {
        public int ObjectiveId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    // ============================================================================
    // QUEST MANAGER PACKETS (Admin)
    // ============================================================================

    /// <summary>
    /// Request all quests for admin management (client -> server)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestManagerListRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response with all quests for admin management (server -> client)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestManagerListResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<QuestDto> Quests { get; set; } = [];
        /// <summary>
        /// Currency definition for "Tails" (lower value currency)
        /// </summary>
        public CurrencyDefinitionDto? TailsDefinition { get; set; }
        /// <summary>
        /// Currency definition for "Crowns" (higher value currency)
        /// </summary>
        public CurrencyDefinitionDto? CrownsDefinition { get; set; }
        /// <summary>
        /// Server's current local time (for quest editor display)
        /// </summary>
        public long ServerLocalTime { get; set; }
        /// <summary>
        /// Server's timezone offset in hours from UTC (e.g., -5 for EST, +1 for CET)
        /// </summary>
        public double ServerTimezoneOffset { get; set; }
    }

    /// <summary>
    /// Server tells client to open the quest manager dialog (server -> client)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OpenQuestManagerPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to save (create or update) a quest (client -> server)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSaveRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public QuestSaveDto Quest { get; set; } = new();
    }

    /// <summary>
    /// Response after saving a quest (server -> client)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSaveResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// All quests after save (full cache refresh)
        /// </summary>
        public List<QuestDto> AllQuests { get; set; } = [];
    }

    /// <summary>
    /// Request to delete a quest (client -> server)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestDeleteRequestPacket
    {
        public string PlayerUid { get; set; } = string.Empty;
        public int QuestId { get; set; }
    }

    /// <summary>
    /// Response after deleting a quest (server -> client)
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestDeleteResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// All quests after deletion (full cache refresh)
        /// </summary>
        public List<QuestDto> AllQuests { get; set; } = [];
    }

    /// <summary>
    /// Currency definition DTO for quest rewards
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CurrencyDefinitionDto
    {
        public string Code { get; set; } = string.Empty;
        public string? Nbt { get; set; } = null;
    }

    /// <summary>
    /// Quest save DTO (supports both create and update)
    /// Id is null for new quests, present for updates
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class QuestSaveDto
    {
        public int? Id { get; set; }
        public string RecurrenceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<QuestObjectiveDto> Objectives { get; set; } = [];
        public List<QuestRewardDto> Rewards { get; set; } = [];
        public string StartsAt { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public bool UsesIngameTime { get; set; }
        public bool Repeat { get; set; }
        public string Rank { get; set; } = "D";
    }
}
