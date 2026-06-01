using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRGuildsAndKingdoms.src.techblock
{
	// Token: 0x0200000B RID: 11
	[NullableContext(1)]
	[Nullable(0)]
	public class GuildTechData
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000098 RID: 152 RVA: 0x0000A2F4 File Offset: 0x000084F4
		// (set) Token: 0x06000099 RID: 153 RVA: 0x0000A2FC File Offset: 0x000084FC
		public string GuildId { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600009A RID: 154 RVA: 0x0000A305 File Offset: 0x00008505
		// (set) Token: 0x0600009B RID: 155 RVA: 0x0000A30D File Offset: 0x0000850D
		public Dictionary<int, GuildTechProgress> TechProgress { get; set; } = new Dictionary<int, GuildTechProgress>();

		// Token: 0x0600009C RID: 156 RVA: 0x0000A316 File Offset: 0x00008516
		public GuildTechProgress GetOrCreateProgress(int techBlockId)
		{
			if (!this.TechProgress.ContainsKey(techBlockId))
			{
				this.TechProgress[techBlockId] = new GuildTechProgress
				{
					TechBlockId = techBlockId
				};
			}
			return this.TechProgress[techBlockId];
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000A34C File Offset: 0x0000854C
		public bool IsTechUnlocked(int techBlockId)
		{
			GuildTechProgress progress;
			return this.TechProgress.TryGetValue(techBlockId, out progress) && progress.IsUnlocked;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000A371 File Offset: 0x00008571
		public string ToJson()
		{
			return JsonSerializer.Serialize<GuildTechData>(this, GuildTechData.SerializeOptions);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000A380 File Offset: 0x00008580
		[return: Nullable(2)]
		public static GuildTechData FromJson(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}
			GuildTechData result;
			try
			{
				result = JsonSerializer.Deserialize<GuildTechData>(json, GuildTechData.DeserializeOptions);
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException("Failed to deserialize GuildTechData: " + ex.Message, ex);
			}
			return result;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0000A3D0 File Offset: 0x000085D0
		public byte[] ToBytes()
		{
			string json = this.ToJson();
			return Encoding.UTF8.GetBytes(json);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000A3EF File Offset: 0x000085EF
		[return: Nullable(2)]
		public static GuildTechData FromBytes(byte[] data)
		{
			if (data == null || data.Length == 0)
			{
				return null;
			}
			return GuildTechData.FromJson(Encoding.UTF8.GetString(data));
		}

		// Token: 0x0400002F RID: 47
		private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never
		};

		// Token: 0x04000030 RID: 48
		private static readonly JsonSerializerOptions DeserializeOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
	}
}
