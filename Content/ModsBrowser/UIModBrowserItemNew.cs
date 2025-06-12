using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;
using Terraria.Localization;

namespace ModManager.Content.ModsBrowser
{
    public class UIModBrowserItemNew : UIPanelStyled
    {
        public ModDownloadItem mod;

        public UIImage icon;
        public UITextDots<string> textName;
        public UITextDots<string> textAuthor;
        public UITextDots<string> textTime;
        public UIElement flagsMarkers;

        public UIImage divider1;
        public UIImage divider2;
        public UIImage divider3;

        public string Name;
        public bool loadedIcon;

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

            flagsMarkers = new()
            {
                Width = { Pixels = 200f },
                Height = { Precent = 1 },
                HAlign = 1
            };
            Append(flagsMarkers);

            textAuthor = new()
            {
                text = mod.Author,
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textAuthor);
            textTime = new()
            {
                text = TimeHelper.HumanTimeSpanString(mod.TimeStamp),
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textTime);
            divider1 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = UIColors.ColorBorderStatic * 0.75f  ,
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider1);
            divider2 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = UIColors.ColorBorderStatic * 0.75f,
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider2);

            divider3 = new(TextureAssets.MagicPixel)
            {
                OverrideSamplerState = SamplerState.PointClamp,
                Color = UIColors.ColorBorderStatic * 0.75f,
                Height = { Precent = 1 },
                Width = { Pixels = 4 },
                ScaleToFit = true
            };
            Append(divider3);

            flagsMarkers = new()
            {
                Width = { Pixels = 80 },
                Height = { Pixels = 32 },
                HAlign = 1
            };

            float off = -24;

            if (!mod.IsInstalled || mod.NeedUpdate)
            {
                var img1 = new UIImage(string.IsNullOrEmpty(mod.ModReferencesBySlug) ? UICommon.ButtonDownloadTexture : UICommon.ButtonDownloadMultipleTexture)
                {
                    HAlign = 1,
                    VAlign = 0.5f,
                    Left = { Pixels = off },
                    ScaleToFit = true,
                    Width = { Pixels = 28 },
                    Height = { Pixels = 28 },
                    OverrideSamplerState = SamplerState.PointClamp
                };
                img1.OnLeftClick += DownloadWithDeps;
                flagsMarkers.Append(img1);
                off -= 30;
            }
            var img = new UIImage(UICommon.ButtonModInfoTexture)
            {
                HAlign = 1,
                VAlign = 0.5f,
                Left = { Pixels = off },
                ScaleToFit = true,
                Width = { Pixels = 28 },
                Height = { Pixels = 28 },
                OverrideSamplerState = SamplerState.PointClamp
            };
            img.OnLeftClick += ViewModInfo;
            flagsMarkers.Append(img);
            off -= 30;

            flagsMarkers.Width.Pixels = -off;

            Redesign();

            OnLeftDoubleClick += DownloadWithDeps;
        }
        public async void DownloadWithDeps(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(in SoundID.MenuTick);
            if (await Interface.modBrowser.DownloadMods([mod]))
            {
                Main.QueueMainThreadAction(delegate { Main.menuMode = 10007; });
            }
        }
        public void ViewModInfo(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(in SoundID.MenuOpen);
            Utils.OpenToURL(Interface.modBrowser.SocialBackend.GetModWebPage(mod.PublishId));
        }
        public void Redesign()
        {
            float scale = UIModBrowserNew.Instance.scale;
            float scaleText = UIModBrowserNew.Instance.scaleText;
            bool grid = UIModBrowserNew.Instance.scaleGrid;
            var e = UIModBrowserNew.Instance.categoriesHorizontal.Elements;

            icon.Height = icon.Width = new(32 * scale - 6, 0);
            icon.Left.Pixels = 10;
            textName.Left.Pixels = grid ? 4 : 10 + 32 * scale;
            textName.Top.Pixels = grid ? 32 * scale + 9 + 5 * scaleText : 4;
            textName.Width = grid ? new(-8, 1) : new(e[0].GetOuterDimensions().Width - textName.Left.Pixels, 0);
            textName.Height = grid ? new(14 * scaleText, 0) : new(0, 1);
            textName.align = grid ? 0.5f : 0;
            textName.scale = scaleText;

            textName.text = Name;

            flagsMarkers.VAlign = grid ? 0 : 0.5f;

            var s = grid ? scale * 0.275f : scale * 0.8f;
            int u = (grid ? 0 : -10);
            foreach (var item in flagsMarkers.Elements)
            {
                item.Width.Pixels = 28 * s;
                item.Height.Pixels = 28 * s;
                item.Left.Pixels = u * s;
                u -= grid ? 28 : 30;
            }
            flagsMarkers.Height.Pixels = 32 * s;
            flagsMarkers.Width.Pixels = (-u + 12 + (grid ? 12 : 2)) * s;

            flagsMarkers.Remove();
            if (grid)
            {
                textAuthor.Remove();
                textTime.Remove();
                divider1.Remove();
                divider2.Remove();
                divider3.Remove();
            }
            else
            {
                Append(textAuthor);
                Append(textTime);
                Append(divider1);
                Append(divider2);
                Append(divider3);
            }
            Append(flagsMarkers);

            textAuthor.Left.Pixels = textName.Left.Pixels + textName.Width.Pixels + 6;
            textAuthor.Width.Pixels = e[1].GetOuterDimensions().Width - 6;
            textAuthor.scale = scaleText;
            divider1.Left.Pixels = textAuthor.Left.Pixels - 8;

            textTime.Left.Pixels = textAuthor.Left.Pixels + textAuthor.Width.Pixels + 6;
            textTime.Width.Pixels = e[2].GetOuterDimensions().Width - 6;
            textTime.scale = scaleText;
            divider2.Left.Pixels = textTime.Left.Pixels - 8;
            divider3.Left.Pixels = divider2.Left.Pixels + textTime.Width.Pixels + 8;

            icon.Top.Pixels = grid ? 10 : 3;

            Height.Pixels = 32 * scale + (grid ? 16 + 20 * scaleText : 0);

            Width = grid ? new(32 * scale + 16, 0) : new(0, 1);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;

            if (mod.NeedUpdate)
            {
                BackgroundColor.R = (byte)(MathHelper.Lerp(BackgroundColor.R, UIColors.ColorNeedUpdate.R, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.G = (byte)(MathHelper.Lerp(BackgroundColor.G, UIColors.ColorNeedUpdate.G, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.B = (byte)(MathHelper.Lerp(BackgroundColor.B, UIColors.ColorNeedUpdate.B, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.A = (byte)(MathHelper.Lerp(BackgroundColor.A, UIColors.ColorNeedUpdate.A, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.A * UIColors.ColorNeedUpdate.A / 255f);
            }

            if (IsMouseHovering && mod.NeedUpdate) UIModBrowserNew.Instance.Tooltip = Language.GetTextValue("Mods.ModManager.NeedUpdate");

            if (!loadedIcon)
            {
                loadedIcon = true;
                if (!string.IsNullOrEmpty(mod.ModIconUrl)) RequestModIcon();
            }
        }

        public async void RequestModIcon()
        {
            if (UIModDownloadItem.ModIconDownloadFailCount < UIModDownloadItem.MaxFails)
            {
                Texture2D? texture2D = await UIModDownloadItem.GetOrDownloadTextureAsync(mod.ModIconUrl);
                if (texture2D != null)
                {
                    icon._nonReloadingTexture = texture2D;
                    icon._texture = null;
                }
            }
        }
    }
}
