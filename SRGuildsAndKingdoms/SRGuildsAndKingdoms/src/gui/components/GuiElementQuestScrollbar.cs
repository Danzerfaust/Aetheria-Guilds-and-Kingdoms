using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace SRGuildsAndKingdoms.src.gui.components
{
    /// <summary>
    /// Custom scrollbar with smooth, enhanced mouse wheel scrolling for quest dialogs
    /// </summary>
    public class GuiElementQuestScrollbar(ICoreClientAPI capi, Action<float> OnNewScrollbarValue, ElementBounds bounds) : GuiElementScrollbar(capi, OnNewScrollbarValue, bounds)
    {
        private float targetHandlePosition;
        private bool isSmoothScrolling;
        private const float SmoothingFactor = 0.3f; // Increased for snappier response

        public override void OnMouseWheel(ICoreClientAPI api, MouseWheelEventArgs args)
        {
            if (Bounds.InnerHeight <= currentHandleHeight + 0.001) return;

            float scrollAmount = (float)scaled(51) * args.deltaPrecise / ScrollConversionFactor;
            targetHandlePosition = currentHandlePosition - scrollAmount;

            double scrollbarMoveableHeight = Bounds.InnerHeight - currentHandleHeight;
            targetHandlePosition = (float)GameMath.Clamp(targetHandlePosition, 0, scrollbarMoveableHeight);

            isSmoothScrolling = true;
            args.SetHandled(true);
        }

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            // When manually dragging, disable smooth scrolling
            isSmoothScrolling = false;
            base.OnMouseDownOnElement(api, args);
            targetHandlePosition = currentHandlePosition;
        }

        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            base.OnMouseMove(api, args);
            // Sync target with current position when manually dragging
            if (!isSmoothScrolling)
            {
                targetHandlePosition = currentHandlePosition;
            }
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            // Only apply smooth scrolling when triggered by mouse wheel
            if (isSmoothScrolling)
            {
                float diff = targetHandlePosition - currentHandlePosition;

                // Use exponential smoothing
                if (Math.Abs(diff) > 0.5f)
                {
                    currentHandlePosition += diff * (SmoothingFactor - 0.2f);

                    // Clamp to valid range
                    double scrollbarMoveableHeight = Bounds.InnerHeight - currentHandleHeight;
                    currentHandlePosition = (float)GameMath.Clamp(currentHandlePosition, 0, scrollbarMoveableHeight);

                    TriggerChanged();
                }
                else
                {
                    // Snap to target when very close to prevent jitter
                    currentHandlePosition = targetHandlePosition;
                    isSmoothScrolling = false;
                    TriggerChanged();
                }
            }

            base.RenderInteractiveElements(deltaTime);
        }
    }
}
