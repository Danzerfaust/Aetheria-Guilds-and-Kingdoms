using System;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace SRGuildsAndKingdoms.src.gui.components
{
	// Token: 0x0200009E RID: 158
	[NullableContext(1)]
	[Nullable(0)]
	public class ItemSlotDisplayOnly : ItemSlot
	{
		// Token: 0x060006D6 RID: 1750 RVA: 0x00033BF8 File Offset: 0x00031DF8
		public ItemSlotDisplayOnly(InventoryBase inventory) : base(inventory)
		{
			this.MaxSlotStackSize = 1;
		}

		// Token: 0x060006D7 RID: 1751 RVA: 0x00033C08 File Offset: 0x00031E08
		[NullableContext(2)]
		public void SetItemstack(ItemStack stack, string originalCode = null)
		{
			if (stack == null)
			{
				base.Itemstack = null;
				return;
			}
			if ((originalCode ?? stack.Collectible.Code.ToString()) == "game:grspoints")
			{
				Item parchmentItem = this.inventory.Api.World.GetItem(new AssetLocation("game:paper-parchment"));
				if (parchmentItem != null)
				{
					ItemStack visualStack = new ItemStack(parchmentItem, 1);
					visualStack.Attributes = new TreeAttribute();
					visualStack.Attributes.SetString("title", "GRS Points");
					visualStack.Attributes.SetString("text", "");
					visualStack.Attributes.SetString("signedby", "The Shadow Realm");
					visualStack.Attributes.SetString("_originalCode", "game:grspoints");
					base.Itemstack = visualStack;
					return;
				}
			}
			base.Itemstack = stack;
		}

		// Token: 0x060006D8 RID: 1752 RVA: 0x00033CE0 File Offset: 0x00031EE0
		[NullableContext(2)]
		public string GetActualItemCode()
		{
			if (base.Itemstack == null)
			{
				return null;
			}
			ITreeAttribute attributes = base.Itemstack.Attributes;
			string originalCode = (attributes != null) ? attributes.GetString("_originalCode", null) : null;
			if (!string.IsNullOrEmpty(originalCode))
			{
				return originalCode;
			}
			return base.Itemstack.Collectible.Code.ToString();
		}

		// Token: 0x060006D9 RID: 1753 RVA: 0x00033D34 File Offset: 0x00031F34
		public override int TryPutInto(ItemSlot sinkSlot, ref ItemStackMoveOperation op)
		{
			return base.TryPutInto(sinkSlot, ref op);
		}

		// Token: 0x060006DA RID: 1754 RVA: 0x00033D40 File Offset: 0x00031F40
		protected override void ActivateSlotLeftClick(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
		{
			if (((sourceSlot != null) ? sourceSlot.Itemstack : null) != null && this.Empty)
			{
				ItemStack clonedStack = sourceSlot.Itemstack.Clone();
				clonedStack.StackSize = 1;
				this.SetItemstack(clonedStack, null);
				this.MarkDirty();
				op.MovedQuantity = 0;
				op.MovableQuantity = 0;
				sourceSlot.MarkDirty();
				return;
			}
			if (sourceSlot != null && sourceSlot.Empty && !this.Empty)
			{
				base.Itemstack = null;
				this.MarkDirty();
				op.MovedQuantity = 0;
				op.MovableQuantity = 0;
			}
		}

		// Token: 0x060006DB RID: 1755 RVA: 0x00033DCB File Offset: 0x00031FCB
		protected override void ActivateSlotRightClick(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
		{
			this.ActivateSlotLeftClick(sourceSlot, ref op);
		}

		// Token: 0x040002E7 RID: 743
		private const string GRS_POINTS_CODE = "game:grspoints";

		// Token: 0x040002E8 RID: 744
		private const string PARCHMENT_CODE = "game:paper-parchment";
	}
}
