using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace ModManager.Content
{
    public class UITextLines : UIElement
    {
        public string text;
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
            if (text == "") return;
            string t = "" + text[0];
            float p = 0;
            for (int i = 1; i < text.Length; i++)
            {
                var s = f.MeasureString(text.Substring(0, i)).X * scale;
                if (f.MeasureString(text.Substring(0, i)).X * scale > c.Width + p)
                {
                    p = s;
                    t += "\n";
                }
                t += text[i];
            }
            if (!big) Utils.DrawBorderString(spriteBatch, t, new Vector2(c.X + c.Width * align, c.Y + c.Height * 0.5f), color, scale, align, 0.5f);
            else Utils.DrawBorderStringBig(spriteBatch, t, new Vector2(c.X + c.Width * align, c.Y + c.Height * 0.5f), color, scale, align, 0.5f);
        }
    }
}
