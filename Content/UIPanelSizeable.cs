using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ModManager.Content
{
    public class UIPanelSizeable : UIPanelStyled
    {
        public Action OnResizing;

        public bool centered;
        public bool resizing;
        public int side;
        public Vector2 pos;
        public Vector2 off;

        public bool UseLeft;
        public bool UseUp;
        public bool UseRight;
        public bool UseDown;

        public List<UIElement> LeftElem = new();
        public List<UIElement> RightElem = new();

        public bool mouse;
        public float mouseRot;

        public float MinHorizontal = 50;
        public float MinVertical = 50;

        public float MaxHorizontal = float.MaxValue;
        public float MaxVertical = float.MaxValue;

        public float border = 12;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            var i = IsIn();
            if (i.HasValue)
            {
                resizing = true;
                side = i.Value;
                pos = new Vector2(Main.mouseX, Main.mouseY);
            }
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (mouse)
            {
                ModManager.SizedMouse = true;
                ModManager.SizedMouseRotation = mouseRot;
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            resizing = false;
        }
        public override void Update(GameTime gameTime)
        {
            mouse = false;
            if (resizing)
            {
                var p = (new Vector2(Main.mouseX, Main.mouseY) - pos) * (centered ? 2 : 1);

                off = new Vector2(Width.Pixels, Height.Pixels);
                mouse = true;

                var c = Parent != null ? Parent.GetInnerDimensions() : new CalculatedStyle();

                if (side == 0 || side == 2)
                {
                    var change = p.X * (side == 0 ? -1 : 1);

                    change -= Width.Pixels + Width.Precent * c.Width + change - Math.Clamp(Width.Pixels + Width.Precent * c.Width + change, MinHorizontal, MaxHorizontal);

                    var l = side == 0 ? LeftElem : RightElem;
                    float back = 0;
                    foreach (var item in l)
                    {
                        var m = change / l.Count;
                        var b = item.GetOuterDimensions().Width - m - Math.Max(item.GetOuterDimensions().Width - m, 75f);
                        item.Width.Pixels -= m + b;
                        back += b;
                    }
                    Width.Pixels += change + back;
                    mouseRot = -MathHelper.PiOver4;
                }
                else
                {
                    var change = p.Y * (side == 1 ? -1 : 1);
                    change -= Height.Pixels + Height.Precent * c.Height + change - Math.Clamp(Height.Pixels + Height.Precent * c.Height + change, MinVertical, MaxVertical);
                    Height.Pixels += change;
                    mouseRot = MathHelper.PiOver4;
                }
                off = new Vector2(Width.Pixels, Height.Pixels) - off;
                off *= centered ? 0.5f : 1;
                if (side == 0 || side == 1) off *= -1;
                pos += off;
                Recalculate();
                OnResizing?.Invoke();
            }
            else
            {
                var i = IsIn();
                if (i.HasValue)
                {
                    var s = i.Value;
                    mouse = true;
                    if (s == 0 || s == 2) mouseRot = -MathHelper.PiOver4;
                    else mouseRot = MathHelper.PiOver4;
                }
            }
            base.Update(gameTime);
        }
        /// <summary>
        /// 0 <= Left
        /// 1 <= Up
        /// 2 <= Right
        /// 3 <= Down
        /// </summary>
        public int? IsIn()
        {
            if (!IsMouseHovering) return null;
            var c = GetOuterDimensions();
            var x = Main.mouseX;
            var y = Main.mouseY;
            if (c.X <= x && x <= c.X + border && UseLeft) return 0;
            if (c.X + c.Width - border <= x && x <= c.X + c.Width && UseRight) return 2;
            if (c.Y <= y && y <= c.Y + border && UseUp) return 1;
            if (c.Y + c.Height - border <= y && y <= c.Y + c.Height && UseDown) return 3;
            return null;
        }
    }
}
