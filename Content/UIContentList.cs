using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ModManager.Content
{
    public class UIContentList : UIElement
    {
        public bool IsVertical;
        public float TotalSize;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var c = GetInnerDimensions();
            float off = 0;
            foreach (var item in Elements)
            {
                if (IsVertical)
                {
                    item.Top.Set(off, 0);
                    off += item.Height.Pixels + item.Height.Precent * c.Height;
                }
                else
                {
                    item.Left.Set(off, 0);
                    off += item.Width.Pixels + item.Width.Precent * c.Width;
                }
            }
            TotalSize = off;
            RecalculateChildren();
        }
    }
}
