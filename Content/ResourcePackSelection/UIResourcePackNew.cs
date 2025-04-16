using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ModManager.Content.ModsList;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader;
using Terraria.Social.Base;
using Terraria.UI;
using Terraria;
using Terraria.IO;

namespace ModManager.Content.ResourcePackSelection
{
    public class UIResourcePackNew : UIPanelStyled
    {
        public ResourcePack pack;
        public bool loaded;

        public UIImage icon;
        public UITextDots<string> textName;
        public UITextDots<string> textAuthor;
        public UITextDots<string> textVersion;

        public UIImage divider1;
        public UIImage divider2;

        public UIImage toggle;

        public string Name => pack.Name;

        public UIResourcePackNew(ResourcePack _pack)
        {
            pack = _pack;
            loaded = pack.IsEnabled;
        }
        public override void OnInitialize()
        {
            SetPadding(0);

            Width.Precent = 1;

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

            textAuthor = new()
            {
                text = pack.Author,
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textAuthor);
            textVersion = new()
            {
                text = "v" + pack.Version.GetFormattedVersion(),
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textVersion);
            divider1 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = new Color(0, 0, 0, 0.5f),
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider1);
            divider2 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = new Color(0, 0, 0, 0.5f),
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider2);


            toggle = new(pack.IsEnabled ? ModManager.AssetToggleOn : ModManager.AssetToggleOff)
            {
                ImageScale = 0.75f,
                NormalizedOrigin = Vector2.One * 0.5f,
                Color = pack.IsEnabled ? new Color(0.75f, 1f, 0.75f) : Color.White
            };
            Append(toggle);

            toggle.OnLeftClick += (e, l) =>
            {
                var prev = pack.IsEnabled;
                Set(null);
                UIResourcePackSelectionMenuNew.Instance.ToRedo.Clear();
                UIResourcePackSelectionMenuNew.Instance.ToUndo.Add((this, prev));
            };

            Redesign();
        }
        public void Set(bool? enabled = null)
        {
            if (pack == null) return;

            enabled ??= !pack.IsEnabled;

            pack.IsEnabled = enabled.Value;

            toggle._texture = pack.IsEnabled ? ModManager.AssetToggleOn : ModManager.AssetToggleOff;
            toggle.Color = pack.IsEnabled != loaded ? Color.Gold : pack.IsEnabled ? new Color(0.75f, 1f, 0.75f) : Color.White;
        }
        public void Redesign()
        {
            float scale = UIResourcePackSelectionMenuNew.Instance.scale;
            float scaleText = UIResourcePackSelectionMenuNew.Instance.scaleText;
            bool grid = scale >= UIResourcePackSelectionMenuNew.Instance.scaleThreshold;
            var e = UIResourcePackSelectionMenuNew.Instance.categoriesHorizontal.Elements;

            icon.Height = icon.Width = new(32 * scale - 6, 0);
            icon.Left.Pixels = grid ? 10 : 20;
            textName.Left.Pixels = grid ? 4 : 20 + 32 * scale;
            textName.Top.Pixels = grid ? 32 * scale + 9 + 5 * scaleText : 4;
            textName.Width = grid ? new(-8, 1) : new(e[0].GetOuterDimensions().Width + e[1].GetOuterDimensions().Width - textName.Left.Pixels, 0);
            textName.Height = grid ? new(14 * scaleText, 0) : new(0, 1);
            textName.align = grid ? 0.5f : 0;
            textName.scale = scaleText;

            if (grid)
            {
                textAuthor.Remove();
                textVersion.Remove();
                divider1.Remove();
                divider2.Remove();
            }
            else
            {
                Append(textAuthor);
                Append(textVersion);
                Append(divider1);
                Append(divider2);
            }

            toggle.Top = grid ? new(2, 0) : new(-12, 0.5f);
            toggle.Left.Pixels = grid ? 2 : -2;
            toggle.ImageScale = grid ? 1f : 0.75f;

            textAuthor.Left.Pixels = textName.Left.Pixels + textName.Width.Pixels + 6;
            textAuthor.Width.Pixels = e[2].GetOuterDimensions().Width - 6;
            textAuthor.scale = scaleText;
            divider1.Left.Pixels = textAuthor.Left.Pixels - 8;

            textVersion.Left.Pixels = textAuthor.Left.Pixels + textAuthor.Width.Pixels + 6;
            textVersion.Width.Pixels = e[3].GetOuterDimensions().Width - 6;
            textVersion.scale = scaleText;
            divider2.Left.Pixels = textVersion.Left.Pixels - 8;
            
            icon.Top.Pixels = grid ? 10 : 3;

            Height.Pixels = 32 * scale + (grid ? 16 + 20 * scaleText : 0);

            Width = grid ? new(32 * scale + 16, 0) : new(0, 1);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
        }
        public int Sort(UIResourcePackNew other, int FilterCategory, int FilterTypo)
        {
            var mul = FilterTypo == 1 ? -1 : 1;
            if (FilterCategory == 0)
            {
                return pack.IsEnabled.CompareTo(other.pack.IsEnabled) * -mul;
            }
            if (FilterCategory == 1)
            {
                return Name.CompareTo(other.Name) * mul;
            }
            if (FilterCategory == 2)
            {
                return pack.Author.CompareTo(other.pack.Author) * mul;
            }
            if (FilterCategory == 3)
            {
                return pack.Version.CompareTo(other.pack.Version) * mul;
            }
            return 0;
        }
    }
}
