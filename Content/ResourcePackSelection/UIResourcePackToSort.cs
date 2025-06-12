using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.IO;
using Terraria.UI;

namespace ModManager.Content.ResourcePackSelection
{
    public class UIResourcePackToSort : UIPanelStyled
    {
        public ResourcePack pack;
        public bool loaded;

        public UIImage icon;
        public UITextDots<string> textName;

        public UIPanelStyled moveUp;
        public UIPanelStyled moveDown;

        public bool enableDraw;

        public string Name => pack.Name;

        public UIResourcePackToSort(ResourcePack _pack)
        {
            pack = _pack;
            loaded = pack.IsEnabled;
        }
        public override void OnInitialize()
        {
            SetPadding(0);

            Width.Precent = 1;

            var bgp = new UIElement()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IgnoresMouseInteraction = false,
            };
            bgp.OnLeftMouseDown += delegate { 
                UIResourcePackSelectionMenuNew.Instance.grabbed = this; 
                UIResourcePackSelectionMenuNew.Instance.grabbedPos = new Vector2(Main.mouseX, Main.mouseY); 
            };
            Append(bgp);

            icon = new UIImage(pack.Icon)
            {
                ScaleToFit = true,
                IgnoresMouseInteraction = true,
                Top = { Pixels = 3 },
                Left = { Pixels = 10 },
                OverrideSamplerState = SamplerState.PointClamp
            };
            Append(icon);
            textName = new()
            {
                text = Name,
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textName);

            var text = Main.Assets.Request<Texture2D>("Images/UI/TexturePackButtons", AssetRequestMode.ImmediateLoad);

            moveUp = new()
            {
                Height = { Pixels = 28 },
                Width = { Pixels = 30 },
                VAlign = 0.5f,
                HAlign = 1
            }; moveUp.FadedMouseOver(); moveUp.SetPadding(0);
            moveUp.Append(new UIImageFramed(text, new Rectangle(0, 4, 30, 20))
            {
                Top = { Pixels = 4 },
                OverrideSamplerState = SamplerState.PointClamp,
                IgnoresMouseInteraction = true
            });
            moveUp.OnLeftClick += delegate
            {
                pack.SortingOrder -= 3;
                UIResourcePackSelectionMenuNew.Instance.UpdateDisplayerToSort();
            };
            Append(moveUp);
            moveDown = new()
            {
                Height = { Pixels = 28 },
                Width = { Pixels = 30 },
                VAlign = 0.5f,
                HAlign = 1,
                Left = { Pixels = -30 }
            }; moveDown.FadedMouseOver(); moveDown.SetPadding(0);
            moveDown.Append(new UIImageFramed(text, new Rectangle(32, 6, 30, 20))
            {
                Top = { Pixels = 4 },
                OverrideSamplerState = SamplerState.PointClamp,
                IgnoresMouseInteraction = true
            });
            moveDown.OnLeftClick += delegate
            {
                pack.SortingOrder += 3;
                UIResourcePackSelectionMenuNew.Instance.UpdateDisplayerToSort();
            };
            Append(moveDown);

            Redesign();
        }
        public void Redesign()
        {
            float scale = UIResourcePackSelectionMenuNew.Instance.scaleText;

            icon.Height = icon.Width = new(32 * scale - 6, 0);
            icon.Left.Pixels = 4;
            textName.Left.Pixels = 4 + 32 * scale;
            textName.Top.Pixels = 4;
            textName.Width = new(-textName.Left.Pixels - 80, 1);
            textName.Height = new(0, 1);
            textName.align = 0;
            textName.scale = scale;

            icon.Top.Pixels = 3;

            Height.Pixels = 32 * scale;

            Width = new(0, 1);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
        }
        public int Sort(UIResourcePackToSort other)
        {
            return pack.SortingOrder.CompareTo(other.pack.SortingOrder);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (UIResourcePackSelectionMenuNew.Instance.grabbed == this)
            {
                if (enableDraw)
                {
                    base.Draw(spriteBatch);
                    enableDraw = false;
                }
            }
            else base.Draw(spriteBatch);
        }
    }
}
