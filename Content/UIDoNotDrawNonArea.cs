using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace ModManager.Content
{
    public class UIDoNotDrawNonArea : UIElement
    {
        public new event ElementEvent OnUpdate;
        public override void DrawChildren(SpriteBatch spriteBatch)
        {
            foreach (UIElement element in Elements)
            {
                if (element._outerDimensions.Y + element._outerDimensions.Height >= Parent._innerDimensions.Y - 10)
                    element.Draw(spriteBatch);
                if (element._outerDimensions.Y >= Parent._innerDimensions.Y + Parent._innerDimensions.Height + 10) break;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (OnUpdate != null)   // I forgot
            {
                OnUpdate(this);
            }
            foreach (UIElement element in Elements)
            {
                if (element._outerDimensions.Y + element._outerDimensions.Height >= Parent._innerDimensions.Y - 10)
                    element.Update(gameTime);
                if (element._outerDimensions.Y >= Parent._innerDimensions.Y + Parent._innerDimensions.Height + 10) break;
            }
        }
        public override void RecalculateChildren()
        {
            foreach (UIElement element in Elements)
            {
                element.Recalculate();
                //if (element._outerDimensions.Y + element._outerDimensions.Height >= Parent._innerDimensions.Y + Parent._innerDimensions.Height + 10) break;
            }
        }
    }
}
