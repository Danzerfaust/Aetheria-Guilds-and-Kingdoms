using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using SRGuildsAndKingdoms;
using SRGuildsAndKingdoms.src.guilds;
using SRGuildsAndKingdomsPVP.src.nodewars;

namespace SRGuildsAndKingdomsPVP.src.pvp
{
    /// <summary>
    /// Manages PVP state for all players based on guild membership
    /// Requires the SRGuildsAndKingdoms mod to be installed
    /// </summary>
    public class PVPManager
    {
        private readonly ICoreServerAPI api;
        private readonly Dictionary<string, PVPPlayerData> playerPVPData;
        private const string STORAGE_KEY = "srguildsandkingdomspvp_playerdata";
        private SRGuildsAndKingdomsModSystem? guildModSystem;
        private DuelManager? duelManager;
        private NodeWarManager? nodeWarManager;

        public DuelManager? DuelManager => duelManager;
        public NodeWarManager? NodeWarManager => nodeWarManager;

        public PVPManager(ICoreServerAPI api)
        {
            this.api = api;
            this.playerPVPData = new Dictionary<string, PVPPlayerData>();
            this.duelManager = new DuelManager(api);
            this.nodeWarManager = new NodeWarManager(api);

            // Initialize the node war manager and its capture zone system
            this.nodeWarManager.Initialize();

            // Get the guild mod system - this is REQUIRED
            api.Event.SaveGameLoaded += () =>
            {
                guildModSystem = api.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>();
                if (guildModSystem == null)
                {
                    api.Logger.Error("[PVP] CRITICAL: Could not find SRGuildsAndKingdoms mod system!");
                    api.Logger.Error("[PVP] This mod requires the SRGuildsAndKingdoms mod to be installed and loaded.");
                    api.Logger.Error("[PVP] PVP functionality will be disabled.");
                }
                else
                {
                    api.Logger.Notification("[PVP] Successfully integrated with SRGuildsAndKingdoms mod");
                    api.Logger.Notification("[PVP] Guild-based PVP is now active: players in different guilds can attack each other");

                    // Set the NodeManager on the NodeWarManager for database persistence
                    var nodeManager = guildModSystem.GetNodeManager();
                    if (nodeManager != null && nodeWarManager != null)
                    {
                        nodeWarManager.SetNodeManager(nodeManager);
                        api.Logger.Notification("[PVP] Node database persistence enabled");
                    }
                    else
                    {
                        api.Logger.Warning("[PVP] NodeManager not available - nodes will not be persisted to database");
                    }
                }
            };
        }

        /// <summary>
        /// Shutdown the PVP manager and cleanup resources
        /// </summary>
        public void Dispose()
        {
            nodeWarManager?.Shutdown();
            api.Logger.Notification("[PVP] PVP Manager disposed");
        }

        /// <summary>
        /// Load all PVP data from world storage
        /// </summary>
        public void LoadAllPlayerData()
        {
            var dataBytes = api.WorldManager.SaveGame.GetData(STORAGE_KEY);
            if (dataBytes == null) return;

            try
            {
                var dataString = System.Text.Encoding.UTF8.GetString(dataBytes);
                var allData = JsonConvert.DeserializeObject<Dictionary<string, PVPPlayerData>>(dataString);

                if (allData != null)
                {
                    foreach (var kvp in allData)
                    {
                        playerPVPData[kvp.Key] = kvp.Value;
                    }
                    api.Logger.Debug($"[PVP] Loaded data for {allData.Count} players");
                }
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[PVP] Failed to load player data: {ex.Message}");
            }
        }

        /// <summary>
        /// Save all PVP data to world storage
        /// </summary>
        public void SaveAllPlayerData()
        {
            try
            {
                var dataString = JsonConvert.SerializeObject(playerPVPData);
                var dataBytes = System.Text.Encoding.UTF8.GetBytes(dataString);
                api.WorldManager.SaveGame.StoreData(STORAGE_KEY, dataBytes);
                api.Logger.Debug($"[PVP] Saved data for {playerPVPData.Count} players");
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[PVP] Failed to save player data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load PVP data for a player from memory (loads all data if not already loaded)
        /// </summary>
        public void LoadPlayerData(string playerUid)
        {
            // If player data already exists in memory, we're done
            if (playerPVPData.ContainsKey(playerUid))
                return;

            // If player data already exists in memory, we're done
            if (playerPVPData.ContainsKey(playerUid))
                return;

            // Create default data for new player
            playerPVPData[playerUid] = new PVPPlayerData
            {
                PlayerUid = playerUid,
                PVPEnabled = false
            };
        }

        /// <summary>
        /// Save PVP data for a player (saves all data to world storage)
        /// </summary>
        public void SavePlayerData(string playerUid)
        {
            // We save all data at once, so just call SaveAllPlayerData
            SaveAllPlayerData();
        }

        /// <summary>
        /// Get PVP data for a player
        /// </summary>
        public PVPPlayerData? GetPlayerData(string playerUid)
        {
            if (!playerPVPData.TryGetValue(playerUid, out var data))
            {
                LoadPlayerData(playerUid);
                playerPVPData.TryGetValue(playerUid, out data);
            }
            return data;
        }

        /// <summary>
        /// Check if a player has PVP enabled (legacy - kept for backwards compatibility)
        /// </summary>
        public bool IsPVPEnabled(string playerUid)
        {
            var data = GetPlayerData(playerUid);
            return data?.PVPEnabled ?? false;
        }

        /// <summary>
        /// Check if a player is in a guild
        /// </summary>
        public bool IsPlayerInGuild(string playerUid)
        {
            if (guildModSystem?.GuildManager == null)
                return false;

            var guild = guildModSystem.GuildManager.GetGuildByMember(playerUid);
            return guild != null;
        }

        /// <summary>
        /// Get the guild name for a player, or null if not in a guild
        /// </summary>
        public string? GetPlayerGuildName(string playerUid)
        {
            if (guildModSystem?.GuildManager == null)
                return null;

            var guild = guildModSystem.GuildManager.GetGuildByMember(playerUid);
            return guild?.Name;
        }

        /// <summary>
        /// Check if two players can fight each other based on guild membership and PVP opt-in
        /// Rules:
        /// 1. If players are in an active duel, they can always attack each other
        /// 2. Guild mod must be loaded
        /// 3. Both players must be in a guild
        /// 4. Both players must have PVP enabled (opt-in)
        /// 5. Players in the same guild cannot attack each other
        /// 6. Players in different guilds can attack each other if both have PVP enabled
        /// </summary>
        public bool CanPlayersAttackEachOther(string attackerUid, string victimUid)
        {
            // Players cannot attack themselves
            if (attackerUid == victimUid)
                return false;

            // If players are in an active duel, they can attack each other
            if (duelManager != null && duelManager.ArePlayersDueling(attackerUid, victimUid))
                return true;

            // Guild system must be available
            if (guildModSystem?.GuildManager == null)
            {
                api.Logger.Debug("[PVP] Guild system not available - PVP disabled");
                return false;
            }

            var guildManager = guildModSystem.GuildManager;

            // Get guilds for both players
            var attackerGuild = guildManager.GetGuildByMember(attackerUid);
            var victimGuild = guildManager.GetGuildByMember(victimUid);

            // Both players must be in a guild to engage in PVP
            if (attackerGuild == null || victimGuild == null)
            {
                return false;
            }

            // Players in the same guild cannot attack each other
            if (attackerGuild.Name == victimGuild.Name)
            {
                return false;
            }

            // Both players must have PVP enabled (opt-in)
            if (!IsPVPEnabled(attackerUid) || !IsPVPEnabled(victimUid))
            {
                return false;
            }

            // Players in different guilds can attack each other if both have PVP enabled
            return true;
        }

        /// <summary>
        /// Toggle PVP for a player (opt-in system with cooldown)
        /// </summary>
        public bool TogglePVP(string playerUid, out string message)
        {
            // Check if guild system is available
            if (guildModSystem?.GuildManager == null)
            {
                message = "PVP system requires the SRGuildsAndKingdoms mod to be installed.";
                return false;
            }

            // Player must be in a guild to enable PVP
            var guildName = GetPlayerGuildName(playerUid);
            if (guildName == null)
            {
                message = "You must join a guild before you can enable PVP! Join a guild to participate in guild-based PVP combat.";
                return false;
            }

            // Get player data
            var data = GetPlayerData(playerUid);
            if (data == null)
            {
                message = "Failed to load player data.";
                return false;
            }

            // Check cooldown
            if (!data.CanTogglePVP())
            {
                var remainingSeconds = data.GetRemainingCooldown();
                message = $"You must wait {remainingSeconds} more seconds before toggling PVP again.";
                return false;
            }

            // Toggle PVP
            if (!data.TogglePVP())
            {
                message = "Failed to toggle PVP. Please try again.";
                return false;
            }

            // Save the change
            SavePlayerData(playerUid);

            // Build success message
            if (data.PVPEnabled)
            {
                message = $"PVP enabled! You are in guild '{guildName}' and can now attack/be attacked by players in other guilds who also have PVP enabled.";
            }
            else
            {
                message = $"PVP disabled. You are safe from PVP combat. You remain in guild '{guildName}'.";
            }

            return true;
        }

        /// <summary>
        /// Set PVP state for a player (admin command)
        /// </summary>
        public void SetPVPState(string playerUid, bool enabled)
        {
            var data = GetPlayerData(playerUid);
            if (data != null)
            {
                data.SetPVPState(enabled);
                SavePlayerData(playerUid);
            }
        }

        /// <summary>
        /// Get all players with PVP enabled (in a guild AND have PVP flag enabled)
        /// </summary>
        public List<string> GetPVPPlayers()
        {
            if (guildModSystem?.GetGuildRepository() == null)
                return new List<string>();

            // Get all players who are in guilds
            var allGuilds = guildModSystem.GetGuildRepository()?.GetAllGuilds();
            if (allGuilds == null)
                return new List<string>();

            var allGuildMembers = allGuilds
                .SelectMany(guild => guild.Members.Keys)
                .Distinct()
                .ToList();

            // Filter to only those who have PVP enabled
            return allGuildMembers
                .Where(playerUid => IsPVPEnabled(playerUid))
                .ToList();
        }
    }
}
