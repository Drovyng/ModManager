using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace ModManager.Content
{
    public class UITextDots<T> : UIElement
    {
        public T text;
        public float scale = 1;
        public Color color = Color.White;
        public const string Dots = "…";
        public float align;
        public bool big;
        public override void OnInitialize()
        {
            IgnoresMouseInteraction = true;
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var c = GetInnerDimensions();
            var f = big ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
            if (text == null) return;
            var t = text.ToString();
            if (t == "") return;
            for (int i = 1; i < t.Length; i++)
            {
                if (f.MeasureString(t.Substring(0, i) + Dots).X * scale > c.Width)
                {
                    t = t.Substring(0, i - 1) + Dots;
                    break;
                }
            }
            if (!big) Utils.DrawBorderString(spriteBatch, t, new Vector2(c.X + c.Width * align, c.Y + c.Height * 0.5f), color, scale, align, 0.5f);
            else Utils.DrawBorderStringBig(spriteBatch, t, new Vector2(c.X + c.Width * align, c.Y + c.Height * 0.5f), color, scale, align, 0.5f);
        }
    }
}
