using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content
{
    public class UISliderNew : UIElement
    {
        public UIImage filler;
        public float minimum;
        public float maximum = 100;
        public float valueLast = 50;
        public float value = 50;
        public bool changing;
        public Action OnChange;
        public override void OnInitialize()
        {
            SetPadding(0);
            PaddingLeft = PaddingRight = 8;
            filler = new(Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner"))
            {
                IgnoresMouseInteraction = true,
                Color = new Color(0.9f, 0.9f, 0.9f),
                Rotation = MathHelper.PiOver2,
                NormalizedOrigin = Vector2.One * 0.5f,
                Left = { Pixels = -10, Precent = (value - minimum) / maximum },
                Top = { Pixels = -2 }
            };
            Append(filler);
            valueLast = value;
            Height.Set(12, 0);
        }
        public override void Update(GameTime gameTime)
        {
            if (!Main.mouseLeft) { changing = false; return; }
            if (!changing || !IsMouseHovering) return;
            var c = GetInnerDimensions();
            if (c.Width != 0)
            {
                var lerp = MathF.Round((Main.mouseX - c.X) / c.Width * 20) * 0.05f;
                if (lerp < 0) lerp = 0; if (lerp > 1) lerp = 1;
                value = minimum + maximum * lerp;
                if (value != valueLast)
                {
                    filler.Left.Precent = lerp;
                    Recalculate();
                    valueLast = value;
                    OnChange?.Invoke();
                }
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            changing = true;
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var c = GetOuterDimensions();
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)c.X, (int)(c.Y + c.Height * 0.5f - 3), (int)c.Width, 6), UICommon.MainPanelBackground);
        }
    }
}
