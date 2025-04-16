using System;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ModManager.Content.ResourcePackSelection
{
    public class UIResourcePackCategory : UIPanelSizeable
    {
        public Action clicked;
        public bool pressed;
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            pressed = true;
        }
        public override void Update(GameTime gameTime)
        {
            if (resizing)
            {
                pressed = false;
            }
            base.Update(gameTime);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (pressed)
            {
                clicked?.Invoke();
            }
        }
    }
}
