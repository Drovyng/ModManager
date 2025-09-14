using Microsoft.Xna.Framework;
using ModManager.Content.ResourcePackSelection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content
{
    public class UIMMTopPanel : UIPanel
    {
        public UIPanelStyled buttonBack;
        public UIPanelStyled buttonMods;
        public UIPanelStyled buttonModBrowser;
        public UIPanelStyled buttonModPacks;
        public UIPanelStyled buttonResourcePacks;
        public override void OnInitialize()
        {
            SetPadding(0);
            Height.Set(40, 0);

            buttonBack = new UIPanelStyled().FadedMouseOver();
            buttonBack.Height.Precent = 1;
            buttonBack.Append(new UIText(Language.GetText("UI.Back"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonBack.SetPadding(0);
            buttonBack.OnLeftClick += (_, _) => { Click(null); };
            buttonBack.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonBack);

            buttonMods = new UIPanelStyled().FadedMouseOver();
            buttonMods.Height.Precent = 1;
            buttonMods.Append(new UIText(Language.GetText("tModLoader.MenuManageMods"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonMods.SetPadding(0);
            buttonMods.OnLeftClick += (_, _) => { Click(Interface.modsMenu); };
            buttonMods.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonMods);

            buttonModBrowser = new UIPanelStyled().FadedMouseOver();
            buttonModBrowser.Height.Precent = 1;
            buttonModBrowser.Append(new UIText(Language.GetText("tModLoader.MenuModBrowser"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonModBrowser.SetPadding(0);
            buttonModBrowser.OnLeftClick += (_, _) => { Click(Interface.modBrowser); };
            buttonModBrowser.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonModBrowser);

            buttonModPacks = new UIPanelStyled().FadedMouseOver();
            buttonModPacks.Height.Precent = 1;
            buttonModPacks.Append(new UIText(Language.GetText("tModLoader.ModsModPacks"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonModPacks.SetPadding(0);
            buttonModPacks.OnLeftClick += (_, _) => { Click(Interface.modPacksMenu); };
            buttonModPacks.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonModPacks);

            buttonResourcePacks = new UIPanelStyled().FadedMouseOver();
            buttonResourcePacks.Height.Precent = 1;
            buttonResourcePacks.Append(new UIText(Language.GetText("UI.ResourcePacks"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonResourcePacks.SetPadding(0);
            buttonResourcePacks.OnLeftClick += (_, _) => { Main.OpenResourcePacksMenu(Main.MenuUI._currentState); };
            buttonResourcePacks.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonResourcePacks);
            
        }

        public override void OnActivate()
        {
            var f = FontAssets.DeathText.Value;
            buttonBack.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("UI.Back")).X * 0.6f;
            buttonMods.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.MenuManageMods")).X * 0.6f;
            buttonModBrowser.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.MenuModBrowser")).X * 0.6f;
            buttonModPacks.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.ModsModPacks")).X * 0.6f;
            buttonResourcePacks.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("UI.ResourcePacks")).X * 0.6f;

            Width.Pixels = buttonBack.Width.Pixels;
            buttonMods.Left.Pixels = Width.Pixels;
            Width.Pixels = buttonMods.Width.Pixels + buttonBack.Width.Pixels;

            buttonModBrowser.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonModBrowser.Width.Pixels;

            buttonModPacks.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonModPacks.Width.Pixels;

            buttonResourcePacks.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonResourcePacks.Width.Pixels;

            Left.Set(Width.Pixels * -0.5f, 0.5f);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            buttonMods.BorderColor = Main.MenuUI._currentState == Interface.modsMenu ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
            buttonModBrowser.BorderColor = Main.MenuUI._currentState == Interface.modBrowser ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
            buttonModPacks.BorderColor = Main.MenuUI._currentState == Interface.modPacksMenu ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
            buttonResourcePacks.BorderColor = Main.MenuUI._currentState == Interface.modSources ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;

            buttonBack.BackgroundColor = buttonBack.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
            buttonMods.BackgroundColor = Main.MenuUI._currentState == Interface.modsMenu ? UIColors.ColorBackgroundSelected : (buttonMods.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
            buttonModBrowser.BackgroundColor = Main.MenuUI._currentState == Interface.modBrowser ? UIColors.ColorBackgroundSelected : (buttonModBrowser.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
            buttonModPacks.BackgroundColor = Main.MenuUI._currentState == Interface.modPacksMenu ? UIColors.ColorBackgroundSelected : (buttonModPacks.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
            buttonResourcePacks.BackgroundColor = (Main.MenuUI._currentState is UIResourcePackSelectionMenuNew) ? UIColors.ColorBackgroundSelected : (buttonResourcePacks.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
            
            base.Update(gameTime);
        }
        public void Click(UIState need)
        {
            if (Main.MenuUI._currentState == need) return;
            Main.MenuUI.SetState(need);
            Main.MenuUI._history.RemoveAt(Main.MenuUI._history.Count - 1);
            if (need == null)
            {
                Main.MenuUI._currentState = null;
                Main.menuMode = 0;
                Main.MenuUI._history.Clear();
            }
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
    }
}
