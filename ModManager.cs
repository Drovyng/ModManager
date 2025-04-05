using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModManager.Content;
using ModManager.Content.ModsBrowser;
using ModManager.Content.ModsList;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;

namespace ModManager
{
    public class UnsupportedModsException : Exception
    {
        public override string StackTrace => "";
        public override string ToString() => base.Message;
        public UnsupportedModsException(string text) : base(text) { }
    }
    public class ModManager : Mod
	{
        public static ModManager Instance;
        public static Asset<Texture2D> AssetToggleOn;
        public static Asset<Texture2D> AssetToggleHalf;
        public static Asset<Texture2D> AssetToggleOff;
        public static Asset<Texture2D> AssetLoading;
        public static Asset<Texture2D> AssetSizeCursor;
        public static Asset<Texture2D> AssetSizeCursorBG;
        public static Asset<Texture2D> AssetModIcon;
        public static Asset<Texture2D> AssetSettingsToggle;
        public static Asset<Texture2D> AssetIconServer;
        public static Asset<Texture2D> AssetIconClient;

        public static Asset<Texture2D> AssetStyledBorder;
        public static Asset<Texture2D> AssetStyledBackground;

        public static bool SizedMouse;
        public static float SizedMouseRotation;

        public static readonly IList<string> BadMods = [
            "ConciseModList", "SmartModManagement", "ModSideIcon", "CompactMods", "ModFolder"
        ];
        public override void Load()
        {
            List<string> badmods = new();
            bool flag = false;
            foreach (var item in ModLoader.Mods)
            {
                if (BadMods.Contains(item.Name)) { badmods.Add("[c/FFFF88:" + item.DisplayNameClean + "]"); flag = true; }
            }
            if (flag)
            {
                throw new UnsupportedModsException("[c/FF8888:ModManager does not support this mods:]\n\n" + string.Join(",\n", badmods) + "\n\n[c/88FF88:Please turn them off!]");
            }
            Instance = this;

            AssetStyledBorder = Assets.Request<Texture2D>("Assets/StyledBorder", AssetRequestMode.ImmediateLoad);
            AssetStyledBackground = Assets.Request<Texture2D>("Assets/StyledBackground", AssetRequestMode.ImmediateLoad);


            AssetIconClient = Assets.Request<Texture2D>("Assets/ClientIcon", AssetRequestMode.ImmediateLoad);
            AssetIconServer = Assets.Request<Texture2D>("Assets/ServerIcon", AssetRequestMode.ImmediateLoad);
            AssetSettingsToggle = Assets.Request<Texture2D>("Assets/Settings_Toggle", AssetRequestMode.ImmediateLoad);
            AssetToggleOn = Assets.Request<Texture2D>("Assets/ToggleOn", AssetRequestMode.ImmediateLoad);
            AssetToggleHalf = Assets.Request<Texture2D>("Assets/ToggleHalf", AssetRequestMode.ImmediateLoad);
            AssetToggleOff = Assets.Request<Texture2D>("Assets/ToggleOff", AssetRequestMode.ImmediateLoad);
            AssetLoading = Assets.Request<Texture2D>("Assets/Loading", AssetRequestMode.ImmediateLoad);
            AssetSizeCursor = Assets.Request<Texture2D>("Assets/SizeCursor", AssetRequestMode.ImmediateLoad);
            AssetSizeCursorBG = Assets.Request<Texture2D>("Assets/SizeCursorBG", AssetRequestMode.ImmediateLoad);
            AssetModIcon = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);

            On_Main.DrawThickCursor += Main_DrawThickCursor;
            On_Main.DrawCursor += Main_DrawCursor;
            On_Main.DoDraw += On_Main_DoDraw;

            UIColors.SetConfig();

            Interface.modsMenu = new UIModsNew();
            Interface.modBrowser = new UIModBrowserNew(Interface.modBrowser.SocialBackend);
            Interface.modPacksMenu.Append(new UIMMTopPanel());
            Interface.modSources.Append(new UIMMTopPanel());

            if (ManagerConfig.Instance.ModLoadingUpgrade && !(Interface.loadMods.Elements[Interface.loadMods.Elements.Count - 1] is UIPanel))
            {
                Interface.loadMods.OnUpdate += LoadMods_OnUpdate;
            }
        }
        public void InitializeLoadModsMenu()
        {
            var pan = new UIPanel()
            {
                VAlign = 1f,
                HAlign = 1f,
                Width = { Precent = 0.345f, },
                Height = { Precent = 0.35f },
                OverflowHidden = true
            };
            pan.SetPadding(8);
            pan.OnUpdate += delegate
            {
                var text = Interface.loadMods.DisplayText;
                if (pan.Elements.Count == 0)
                {
                    pan.Append(new UIText(Interface.loadMods._cts.GetHashCode().ToString(), 1, false)
                    {
                        TextColor = Color.Transparent
                    });
                    return;
                }
                if ((pan.Elements[0] as UIText).Text != Interface.loadMods._cts.GetHashCode().ToString())
                {
                    pan.RemoveAllChildren();
                    return;
                }
                if ((pan.Elements.Count == 1 || (pan.Elements[pan.Elements.Count - 1] as UIText).Text != text) && text != null)
                {
                    int i = 1;
                    while (i < pan.Elements.Count)
                    {
                        pan.Elements[i].Top.Pixels += 24;
                        if (pan.Elements[i].Top.Pixels > Main.screenHeight * 0.4f)
                        {
                            pan.Elements[i].Remove();
                            continue;
                        }
                        i++;
                    }
                    pan.Append(new UIText(text ?? "", 1, false)
                    {
                        TextOriginX = 0.5f,
                        TextOriginY = 0.5f,
                        Width = { Precent = 1 },
                        Height = { Pixels = 24 }
                    });
                    pan.Recalculate();
                }
            };
            Interface.loadMods.Append(pan);
        }
        private void LoadMods_OnUpdate(Terraria.UI.UIElement affectedElement)
        {
            InitializeLoadModsMenu();
            Interface.loadMods.OnUpdate -= LoadMods_OnUpdate;
        }


        public static void OpenFolder(string path)
        {
            if (System.IO.Directory.Exists(path)) Utils.OpenFolder(path);
        }
        public static LocalizedText Get(string name)
        {
            return Language.GetText("Mods.ModManager." + name);
        }
        private void On_Main_DoDraw(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            SizedMouse = false;
            orig(self, gameTime);
        }

        public override void Unload()
        {
            Interface.modsMenu = new UIMods();
            Interface.modBrowser = new UIModBrowser(Interface.modBrowser.SocialBackend);
            Interface.modPacksMenu = new UIModPacks();
            Interface.modSources = new UIModSources();
        }

        private Vector2 Main_DrawThickCursor(On_Main.orig_DrawThickCursor orig, bool smart)
        {
            if (!SizedMouse || !Main.gameMenu)
            {
                return orig(smart);
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vector = Vector2.Zero;
                switch (i)
                {
                    case 0:
                        vector = new Vector2(0f, 1f);
                        break;
                    case 1:
                        vector = new Vector2(1f, 0f);
                        break;
                    case 2:
                        vector = new Vector2(0f, -1f);
                        break;
                    case 3:
                        vector = new Vector2(-1f, 0f);
                        break;
                }
                vector += Vector2.One * 2f;
                Vector2 origin = new Vector2(10f);
                Rectangle? sourceRectangle = null;
                float scale = Main.cursorScale * 1.1f;
                Main.spriteBatch.Draw(AssetSizeCursorBG.Value, new Vector2(Main.mouseX, Main.mouseY) + vector, sourceRectangle, Main.MouseBorderColor, SizedMouseRotation, origin, scale, SpriteEffects.None, 0f);
            }
            return new Vector2(2f);
        }
        private void Main_DrawCursor(On_Main.orig_DrawCursor orig, Vector2 bonus, bool smart)
        {
            if (!SizedMouse || !Main.gameMenu)
            {
                orig(bonus, smart); return;
            }
            Main.spriteBatch.Draw(AssetSizeCursor.Value, new Vector2(Main.mouseX, Main.mouseY) + bonus + Vector2.One, null, new Color((int)((float)(int)Main.cursorColor.R * 0.2f), (int)((float)(int)Main.cursorColor.G * 0.2f), (int)((float)(int)Main.cursorColor.B * 0.2f), (int)((float)(int)Main.cursorColor.A * 0.5f)), SizedMouseRotation, Vector2.One * 8, Main.cursorScale * 1.1f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(AssetSizeCursor.Value, new Vector2(Main.mouseX, Main.mouseY) + bonus, null, Main.cursorColor, SizedMouseRotation, Vector2.One * 8, Main.cursorScale, SpriteEffects.None, 0f);
        }
    }
}
