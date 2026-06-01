using System;
using ProtoBuf;

namespace SRGuildsAndKingdoms.src.network
{
	// Token: 0x02000052 RID: 82
	[ProtoContract]
	public enum NotificationType
	{
		// Token: 0x04000133 RID: 307
		[ProtoEnum]
		Info,
		// Token: 0x04000134 RID: 308
		[ProtoEnum]
		Success,
		// Token: 0x04000135 RID: 309
		[ProtoEnum]
		Warning,
		// Token: 0x04000136 RID: 310
		[ProtoEnum]
		Error
	}
}
