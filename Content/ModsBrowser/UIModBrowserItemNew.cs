using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModManager.Content.ModsBrowser;
using ModManager.Content.ModsList;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace ModManager.Content.ModsBrowser
{
    public class UIModBrowserItemNew : UIPanel
    {
        public ModDownloadItem mod;
        public bool loaded;

        public UIImage icon;
        public UITextDots<string> textName;
        public UITextDots<string> textAuthor;
        public UITextDots<string> textTime;
        //public UIElement flagsMarkers;

        public UIImage divider1;
        public UIImage divider2;
        //public UIImage divider3;

        public bool Active;
        public string Name;

        public UIModBrowserItemNew(ModDownloadItem _mod)
        {
            mod = _mod;
            Name = mod.DisplayNameClean;
        }
        public override void OnInitialize()
        {
            SetPadding(0);

            Width.Precent = 1;

            icon = new UIImage(ModManager.AssetModIcon)
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
            /*
            flagsMarkers = new()
            {
                Width = { Pixels = 200f },
                Height = { Precent = 1 },
                HAlign = 1
            };
            Append(flagsMarkers);
            */
            textAuthor = new()
            {
                text = mod.Author,
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textAuthor);
            textTime = new()
            {
                text = "v" + TimeHelper.HumanTimeSpanString(mod.TimeStamp),
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textTime);
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
            /*
            divider3 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = new Color(0, 0, 0, 0.5f),
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider3);

            var tags = new List<(Texture2D, string)>();

            //if (mod)

            float offset = 8;
            foreach (var item in tags)
            {
                var img = new UIImage(item.Item1)
                {
                    NormalizedOrigin = Vector2.One * 0.5f,
                    VAlign = 0.5f,
                    HAlign = 1f,
                    Left = { Pixels = -offset },
                    Width = { Pixels = 22 },
                    Height = { Pixels = 22 },
                };
                flagsMarkers.Append(img);
                offset += 24;
            }
            */
            Redesign();
        }
        public void Redesign()
        {
            float scale = UIModBrowserNew.Instance.scale;
            float scaleText = UIModBrowserNew.Instance.scaleText;
            bool grid = scale >= UIModBrowserNew.Instance.scaleThreshold;
            var e = UIModsNew.Instance.categoriesHorizontal.Elements;

            icon.Height = icon.Width = new(32 * scale - 6, 0);
            icon.Left.Pixels = grid ? 10 : 20;
            textName.Left.Pixels = grid ? 4 : 20 + 32 * scale;
            textName.Top.Pixels = grid ? 32 * scale + 9 + 5 * scaleText : 4;
            textName.Width = grid ? new(-8, 1) : new(e[0].GetOuterDimensions().Width - textName.Left.Pixels, 0);
            textName.Height = grid ? new(14 * scaleText, 0) : new(0, 1);
            textName.align = grid ? 0.5f : 0;
            textName.scale = scaleText;

            textName.text = Name;

            if (grid)
            {
                textAuthor.Remove();
                textTime.Remove();
                //flagsMarkers.Remove();
                divider1.Remove();
                divider2.Remove();
                //divider3.Remove();
            }
            else
            {
                Append(textAuthor);
                Append(textTime);
                //Append(flagsMarkers);
                Append(divider1);
                Append(divider2);
                //Append(divider3);
            }

            textAuthor.Left.Pixels = textName.Left.Pixels + textName.Width.Pixels + 6;
            textAuthor.Width.Pixels = e[1].GetOuterDimensions().Width - 6;
            textAuthor.scale = scaleText;
            divider1.Left.Pixels = textAuthor.Left.Pixels - 8;

            textTime.Left.Pixels = textAuthor.Left.Pixels + textAuthor.Width.Pixels + 6;
            textTime.Width.Pixels = e[2].GetOuterDimensions().Width - 6;
            textTime.scale = scaleText;
            divider2.Left.Pixels = textTime.Left.Pixels - 8;
            //divider3.Left.Pixels = divider2.Left.Pixels + textTime.Width.Pixels + 8;

            icon.Top.Pixels = grid ? 10 : 3;

            Height.Pixels = 32 * scale + (grid ? 16 + 20 * scaleText : 0);

            Width = grid ? new(32 * scale + 16, 0) : new(0, 1);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            BackgroundColor = IsMouseHovering ? new Color(93, 102, 171) * 0.7f : new Color(63, 82, 151) * 0.7f;
            BorderColor = UIModBrowserNew.Instance.Selected == this ? Color.Gold : Color.Black;
        }

        public async void RequestModIcon()
        {
            if (UIModDownloadItem.ModIconDownloadFailCount < UIModDownloadItem.MaxFails)
            {
                ModIconStatus = ModIconStatus.REQUESTED;
                Texture2D texture2D = await UIModDownloadItem.GetOrDownloadTextureAsync(mod.ModIconUrl);
                if (texture2D != null)
                {
                    icon._nonReloadingTexture = texture2D;
                    icon._texture = null;
                }
            }

            ModIconStatus = ModIconStatus.READY;
        }
        public ModIconStatus ModIconStatus;
        /*public int Sort(UIModItemNew other, int FilterCategory, int FilterTypo)
        {
            var mul = FilterTypo == 1 ? -1 : 1;
            if (mod == null && other.mod == null)
            {
                return Name.CompareTo(other.Name) * (FilterCategory > 1 ? mul : 1);
            }
            if (other.mod == null) return 1;
            if (mod == null) return -1;
            if (FilterCategory == 1)
            {
                return Name.CompareTo(other.Name) * mul;
            }
            if (FilterCategory == 2)
            {
                return mod.properties.author.CompareTo(other.mod.properties.author) * mul;
            }
            if (FilterCategory == 3)
            {
                return mod.properties.version.CompareTo(other.mod.properties.version) * mul;
            }
            if (FilterCategory == 4)
            {
                return mod.properties.side.ToFriendlyString().CompareTo(other.mod.properties.side.ToFriendlyString()) * mul;
            }
            return 0;
        }*/
    }
}
