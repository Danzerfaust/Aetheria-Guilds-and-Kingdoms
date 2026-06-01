using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SRGuildsAndKingdoms.src.database;
using SRGuildsAndKingdoms.src.network;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SRGuildsAndKingdoms.src.config
{
	// Token: 0x020000C9 RID: 201
	[NullableContext(1)]
	[Nullable(0)]
	public class ZoneWhitelistManager
	{
		// Token: 0x060009B2 RID: 2482 RVA: 0x00044ECB File Offset: 0x000430CB
		public ZoneWhitelistManager(ICoreServerAPI api, ZoneWhitelistRepository repository)
		{
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x00044EEC File Offset: 0x000430EC
		public bool AddPlayerToZone(int zoneId, string playerUid)
		{
			this.EnsureCacheLoaded();
			if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
			{
				return false;
			}
			if (!this.zoneWhitelists.ContainsKey(zoneId))
			{
				this.zoneWhitelists[zoneId] = new HashSet<string>();
			}
			bool flag = this.zoneWhitelists[zoneId].Add(playerUid);
			if (flag)
			{
				this.<repository>P.AddPlayerToZone(zoneId, playerUid);
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
				if (srguildsAndKingdomsModSystem == null)
				{
					return flag;
				}
				GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
				if (networkHandler == null)
				{
					return flag;
				}
				networkHandler.BroadcastGuildConfigToAll();
			}
			return flag;
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x00044F6C File Offset: 0x0004316C
		public bool RemovePlayerFromZone(int zoneId, string playerUid)
		{
			this.EnsureCacheLoaded();
			if (zoneId < 0 || string.IsNullOrWhiteSpace(playerUid))
			{
				return false;
			}
			if (!this.zoneWhitelists.ContainsKey(zoneId))
			{
				return false;
			}
			bool flag = this.zoneWhitelists[zoneId].Remove(playerUid);
			if (flag)
			{
				this.<repository>P.RemovePlayerFromZone(zoneId, playerUid);
				if (this.zoneWhitelists[zoneId].Count == 0)
				{
					this.zoneWhitelists.Remove(zoneId);
				}
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
				if (srguildsAndKingdomsModSystem == null)
				{
					return flag;
				}
				GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
				if (networkHandler == null)
				{
					return flag;
				}
				networkHandler.BroadcastGuildConfigToAll();
			}
			return flag;
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00044FFA File Offset: 0x000431FA
		public bool IsPlayerWhitelisted(int zoneId, string playerUid)
		{
			this.EnsureCacheLoaded();
			return zoneId >= 0 && !string.IsNullOrWhiteSpace(playerUid) && this.zoneWhitelists.ContainsKey(zoneId) && this.zoneWhitelists[zoneId].Contains(playerUid);
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x00045034 File Offset: 0x00043234
		public List<int> GetWhitelistedZones(string playerUid)
		{
			this.EnsureCacheLoaded();
			if (string.IsNullOrWhiteSpace(playerUid))
			{
				return new List<int>();
			}
			return (from kvp in this.zoneWhitelists
			where kvp.Value.Contains(playerUid)
			select kvp.Key).ToList<int>();
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x000450A7 File Offset: 0x000432A7
		public List<string> GetWhitelistedPlayers(int zoneId)
		{
			this.EnsureCacheLoaded();
			if (zoneId < 0)
			{
				return new List<string>();
			}
			if (!this.zoneWhitelists.ContainsKey(zoneId))
			{
				return new List<string>();
			}
			return this.zoneWhitelists[zoneId].ToList<string>();
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x000450DE File Offset: 0x000432DE
		public int GetWhitelistedPlayersCount(int zoneId)
		{
			this.EnsureCacheLoaded();
			if (zoneId < 0)
			{
				return 0;
			}
			if (!this.zoneWhitelists.ContainsKey(zoneId))
			{
				return 0;
			}
			return this.zoneWhitelists[zoneId].Count;
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x00045110 File Offset: 0x00043310
		public int ClearZone(int zoneId)
		{
			this.EnsureCacheLoaded();
			if (zoneId < 0)
			{
				return 0;
			}
			if (!this.zoneWhitelists.ContainsKey(zoneId))
			{
				return 0;
			}
			int count = this.zoneWhitelists[zoneId].Count;
			this.zoneWhitelists.Remove(zoneId);
			if (count > 0)
			{
				this.<repository>P.ClearZone(zoneId);
				SRGuildsAndKingdomsModSystem srguildsAndKingdomsModSystem = this.modSystem;
				if (srguildsAndKingdomsModSystem == null)
				{
					return count;
				}
				GuildNetworkHandler networkHandler = srguildsAndKingdomsModSystem.NetworkHandler;
				if (networkHandler == null)
				{
					return count;
				}
				networkHandler.BroadcastGuildConfigToAll();
			}
			return count;
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x00045182 File Offset: 0x00043382
		public List<int> GetAllZoneIds()
		{
			this.EnsureCacheLoaded();
			return this.zoneWhitelists.Keys.ToList<int>();
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0004519C File Offset: 0x0004339C
		public int GetTotalWhitelistedPlayersCount()
		{
			this.EnsureCacheLoaded();
			return this.zoneWhitelists.Values.SelectMany((HashSet<string> s) => s).Distinct<string>().Count<string>();
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x000451E8 File Offset: 0x000433E8
		public void Load()
		{
			try
			{
				this.<api>P.Logger.Notification("[ZoneWhitelist] Loading zone whitelists from database...");
				this.zoneWhitelists.Clear();
				foreach (KeyValuePair<int, List<string>> kvp in this.<repository>P.GetAllWhitelists())
				{
					this.zoneWhitelists[kvp.Key] = new HashSet<string>(kvp.Value);
				}
				this.cacheLoaded = true;
				ILogger logger = this.<api>P.Logger;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[ZoneWhitelist] Loaded ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.zoneWhitelists.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" zone whitelist(s) from database");
				logger.Notification(defaultInterpolatedStringHandler.ToStringAndClear());
				this.modSystem = this.<api>P.ModLoader.GetModSystem<SRGuildsAndKingdomsModSystem>(true);
			}
			catch (Exception ex)
			{
				this.<api>P.Logger.Error("[ZoneWhitelist] Failed to load whitelist data: " + ex.Message);
				this.zoneWhitelists.Clear();
				this.cacheLoaded = true;
			}
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x00045324 File Offset: 0x00043524
		private void EnsureCacheLoaded()
		{
			if (!this.cacheLoaded)
			{
				this.Load();
			}
		}

		// Token: 0x040003DA RID: 986
		[CompilerGenerated]
		private ICoreServerAPI <api>P = api;

		// Token: 0x040003DB RID: 987
		[CompilerGenerated]
		private ZoneWhitelistRepository <repository>P = repository;

		// Token: 0x040003DC RID: 988
		private readonly Dictionary<int, HashSet<string>> zoneWhitelists = new Dictionary<int, HashSet<string>>();

		// Token: 0x040003DD RID: 989
		private bool cacheLoaded;

		// Token: 0x040003DE RID: 990
		[Nullable(2)]
		private SRGuildsAndKingdomsModSystem modSystem;
	}
}
