using Vintagestory.API.Common;

namespace SRGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// A slot that displays items without actually taking them from the player's inventory.
    /// Items placed here are cloned, and the original remains in the player's inventory.
    /// </summary>
    public class ItemSlotDisplayOnly : ItemSlot
    {
        private const string GRS_POINTS_CODE = "game:grspoints";
        private const string PARCHMENT_CODE = "game:paper-parchment";

        public ItemSlotDisplayOnly(InventoryBase inventory) : base(inventory)
        {
            MaxSlotStackSize = 1;
        }

        /// <summary>
        /// Sets an itemstack, transforming GRS points to parchment for display
        /// Use this instead of directly setting Itemstack when loading/creating items programmatically
        /// </summary>
        public void SetItemstack(ItemStack? stack, string? originalCode = null)
        {
            if (stack == null)
            {
                this.Itemstack = null;
                return;
            }

            // Use provided originalCode if available, otherwise use the collectible's code
            var itemCode = originalCode ?? stack.Collectible.Code.ToString();

            // If the item is GRS points, display it as parchment
            if (itemCode == GRS_POINTS_CODE)
            {
                var world = inventory.Api.World;
                var parchmentItem = world.GetItem(new AssetLocation(PARCHMENT_CODE));

                if (parchmentItem != null)
                {
                    // Create a visual parchment stack
                    var visualStack = new ItemStack(parchmentItem, 1);
                    visualStack.Attributes = new Vintagestory.API.Datastructures.TreeAttribute();
                    visualStack.Attributes.SetString("title", $"GRS Points");
                    visualStack.Attributes.SetString("text", "");
                    visualStack.Attributes.SetString("signedby", $"The Shadow Realm");
                    visualStack.Attributes.SetString("_originalCode", GRS_POINTS_CODE);

                    this.Itemstack = visualStack;
                    return;
                }
            }

            // Not GRS points, just set normally
            this.Itemstack = stack;
        }

        /// <summary>
        /// Gets the actual item code, checking for stored original codes
        /// Use this when saving to get the real code (e.g., "game:grspoints" instead of "game:papyrus")
        /// </summary>
        public string? GetActualItemCode()
        {
            if (Itemstack == null) return null;

            // Check if this is a display parchment representing something else
            var originalCode = Itemstack.Attributes?.GetString("_originalCode");
            if (!string.IsNullOrEmpty(originalCode))
            {
                return originalCode;
            }

            return Itemstack.Collectible.Code.ToString();
        }

        public override int TryPutInto(ItemSlot sinkSlot, ref ItemStackMoveOperation op)
        {
            // This is called when trying to put THIS slot's item into another slot
            // Use default behavior for taking out
            return base.TryPutInto(sinkSlot, ref op);
        }

        protected override void ActivateSlotLeftClick(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            // This is called when clicking this slot with another item
            if (sourceSlot?.Itemstack != null && this.Empty)
            {
                // Clone the item and use our custom setter for transformation
                var clonedStack = sourceSlot.Itemstack.Clone();
                clonedStack.StackSize = 1;

                SetItemstack(clonedStack);
                this.MarkDirty();

                // Set the operation to indicate nothing was moved from source
                op.MovedQuantity = 0;
                op.MovableQuantity = 0;

                // Force the source slot to refresh its visual state
                sourceSlot.MarkDirty();
            }
            else if (sourceSlot?.Empty == true && !this.Empty)
            {
                // Just delete the item from this slot instead of moving it
                this.Itemstack = null;
                this.MarkDirty();

                // Set the operation to indicate nothing was moved to the source
                op.MovedQuantity = 0;
                op.MovableQuantity = 0;
            }
        }

        protected override void ActivateSlotRightClick(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            // Same behavior for right-click
            ActivateSlotLeftClick(sourceSlot, ref op);
        }
    }
}