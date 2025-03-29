using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModManager.Content;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager
{
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

        public static bool SizedMouse;
        public static float SizedMouseRotation;

        public static readonly IList<string> BadMods = [
            "ConciseModList", "SmartModManagement", "ModSideIcon"
        ];
        public override void Load()
        {
            List<string> badmods = new();
            bool flag = false;
            foreach (var item in ModLoader.Mods)
            {
                if (BadMods.Contains(item.Name)) { badmods.Add(item.DisplayNameClean); flag = true; }
            }
            if (flag)
            {
                throw new System.Exception("ModManager does not support this mods: [" + string.Join(",", badmods) + "]");
            }

            Instance = this;

            AssetIconServer = Assets.Request<Texture2D>("Assets/ServerIcon", AssetRequestMode.ImmediateLoad);
            AssetIconClient = Assets.Request<Texture2D>("Assets/ClientIcon", AssetRequestMode.ImmediateLoad);
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

            On_UserInterface.SetState += On_UserInterface_SetState;

            Interface.modsMenu = new UIModsNew();
            Interface.modBrowser.Append(new UIMMBottomPanel());
            Interface.modPacksMenu.Append(new UIMMBottomPanel());
            Interface.modSources.Append(new UIMMBottomPanel());
        }
        /*
        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("CompatChecker", out Mod mod))
            {
                new ILHook(mod.Code.GetType("CompatChecker.CompatChecker").GetMethod("DrawInModsMenu", BindingFlags.NonPublic | BindingFlags.Instance), IL_CompatChecker_DrawInModsMenu);
            }
        }
        private void IL_CompatChecker_DrawInModsMenu(ILContext il)
        {
            ILCursor c = new(il);
            c = c.GotoNext(MoveType.Before, (p) => p.MatchStloc2());
            c.EmitLdstr("The rewrited string");
        }*/

        private void On_UserInterface_SetState(On_UserInterface.orig_SetState orig, UserInterface self, UIState state)
        {
            if (!(state is UILoadMods) && self._currentState is UIModsNew unew)
            {
                if (unew.DoNotClose)
                {
                    unew.DoNotCloseCallback();
                    return;
                }
            }
            orig(self, state);
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
