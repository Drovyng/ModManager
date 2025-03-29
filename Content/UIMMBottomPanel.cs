using Microsoft.Xna.Framework;
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
    public class UIMMBottomPanel : UIPanel
    {
        public UIPanel buttonMods;
        public UIPanel buttonModBrowser;
        public UIPanel buttonModPacks;
        public UIPanel buttonModDevelop;
        public override void OnInitialize()
        {
            SetPadding(0);
            Height.Set(40, 0);

            buttonMods = new UIPanel().WithFadedMouseOver();
            buttonMods.Height.Precent = 1;
            buttonMods.Append(new UIText(Language.GetText("tModLoader.MenuManageMods"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonMods.SetPadding(0);
            buttonMods.OnLeftClick += (_, _) => { Click(Interface.modsMenu); };
            buttonMods.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonMods);

            buttonModBrowser = new UIPanel().WithFadedMouseOver();
            buttonModBrowser.Height.Precent = 1;
            buttonModBrowser.Append(new UIText(Language.GetText("tModLoader.MenuModBrowser"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonModBrowser.SetPadding(0);
            buttonModBrowser.OnLeftClick += (_, _) => { Click(Interface.modBrowser); };
            buttonModBrowser.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonModBrowser);

            buttonModPacks = new UIPanel().WithFadedMouseOver();
            buttonModPacks.Height.Precent = 1;
            buttonModPacks.Append(new UIText(Language.GetText("tModLoader.ModsModPacks"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonModPacks.SetPadding(0);
            buttonModPacks.OnLeftClick += (_, _) => { Click(Interface.modPacksMenu); };
            buttonModPacks.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonModPacks);

            buttonModDevelop = new UIPanel().WithFadedMouseOver();
            buttonModDevelop.Height.Precent = 1;
            buttonModDevelop.Append(new UIText(Language.GetText("tModLoader.MenuDevelopMods"), 0.6f, true)
            {
                Width = { Precent = 1 }, Height = { Precent = 1 }, TextOriginX = 0.5f, TextOriginY = 0.5f
            }); buttonModDevelop.SetPadding(0);
            buttonModDevelop.OnLeftClick += (_, _) => { Click(Interface.modSources); };
            buttonModDevelop.OnMouseOver += (_, _) => { SoundEngine.PlaySound(SoundID.MenuTick); };
            Append(buttonModDevelop);
        }

        public override void OnActivate()
        {
            var f = FontAssets.DeathText.Value;
            buttonMods.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.MenuManageMods")).X * 0.6f;
            buttonModBrowser.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.MenuModBrowser")).X * 0.6f;
            buttonModPacks.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.ModsModPacks")).X * 0.6f;
            buttonModDevelop.Width.Pixels = 16 + f.MeasureString(Language.GetTextValue("tModLoader.MenuDevelopMods")).X * 0.6f;

            Width.Pixels = buttonMods.Width.Pixels;

            buttonModBrowser.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonModBrowser.Width.Pixels;

            buttonModPacks.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonModPacks.Width.Pixels;

            buttonModDevelop.Left.Pixels = Width.Pixels;
            Width.Pixels += buttonModDevelop.Width.Pixels;

            Left.Set(Width.Pixels * -0.5f, 0.5f);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            buttonMods.BorderColor = Main.MenuUI._currentState == Interface.modsMenu ? Color.Gold : Color.Black;
            buttonModBrowser.BorderColor = Main.MenuUI._currentState == Interface.modBrowser ? Color.Gold : Color.Black;
            buttonModPacks.BorderColor = Main.MenuUI._currentState == Interface.modPacksMenu ? Color.Gold : Color.Black;
            buttonModDevelop.BorderColor = Main.MenuUI._currentState == Interface.modSources ? Color.Gold : Color.Black;

            buttonMods.BackgroundColor = Main.MenuUI._currentState == Interface.modsMenu ? new Color(93, 102, 171) * 0.7f : (buttonMods.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);
            buttonModBrowser.BackgroundColor = Main.MenuUI._currentState == Interface.modBrowser ? new Color(93, 102, 171) * 0.7f : (buttonModBrowser.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);
            buttonModPacks.BackgroundColor = Main.MenuUI._currentState == Interface.modPacksMenu ? new Color(93, 102, 171) * 0.7f : (buttonModPacks.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);
            buttonModDevelop.BackgroundColor = Main.MenuUI._currentState == Interface.modSources ? new Color(93, 102, 171) * 0.7f : (buttonModDevelop.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);

            base.Update(gameTime);
        }
        public void Click(UIState need)
        {
            if (Main.MenuUI._currentState == need) return;
            Main.MenuUI.SetState(need);
            Main.MenuUI._history.RemoveAt(Main.MenuUI._history.Count - 1);
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
    }
}
