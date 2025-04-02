using System.IO;
using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using System.Linq;
using System.Collections.Generic;
using Terraria.ModLoader.UI;
using Terraria;

namespace ModManager.Content.ModsList
{
    public class UIModsConfigCollection : UIElement
    {
        public UITextDots<string> Text;
        public UIPanelStyled Panel;
        public UIImage Toggle;
        public bool isAll;
        public UIModsConfigCollection(string name)
        {
            Text = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                Top = { Pixels = 8 },
                text = name,
            };
            Panel = new()
            {
                Height = { Precent = 1 },
                PaddingTop = PaddingBottom = 0,
                PaddingLeft = PaddingRight = 6,
            };
        }
        public static void Save(ModConfig config, string text)
        {
            Directory.CreateDirectory(ConfigManager.ModConfigPath);
            Directory.CreateDirectory(Path.Combine(ConfigManager.ModConfigPath, "Collection_" + text));
            string path = config.Mod.Name + "_" + config.Name + ".json";
            string path2 = Path.Combine(ConfigManager.ModConfigPath, "Collection_" + text, path);
            string contents = JsonConvert.SerializeObject(config, ConfigManager.serializerSettings);
            File.WriteAllText(path2, contents);
        }
        public static void Rename(string from, string to)
        {
            string path = Path.Combine(ConfigManager.ModConfigPath, "Collection_" + from);
            string path2 = Path.Combine(ConfigManager.ModConfigPath, "Collection_" + to);
            Directory.CreateDirectory(path2);
            foreach (var item in Directory.EnumerateFiles(path))
            {
                File.Move(item, path2 + item.Substring(path.Length));
            }
            Directory.Delete(path);
        }
        public static void Delete(string text)
        {
            string path = Path.Combine(ConfigManager.ModConfigPath, "Collection_" + text);
            Directory.Delete(path, true);
        }
        public static void SaveAll(string text)
        {
            foreach (KeyValuePair<Mod, List<ModConfig>> config in ConfigManager.Configs)
            {
                foreach (ModConfig item in config.Value)
                {
                    Save(item, text);
                }
            }
        }
        public void LoadConfig(ModConfig config)
        {
            ModConfig config2 = config;
            string path = config2.Mod.Name + "_" + config2.Name + ".json";
            string text = Path.Combine(ConfigManager.ModConfigPath, "Collection_" + Text.text, path);
            if (config2.Mode == ConfigScope.ServerSide && ModNet.NetReloadActive)
            {
                JsonConvert.PopulateObject(ModNet.pendingConfigs.Single((x) => x.modname == config2.Mod.Name && x.configname == config2.Name).json, config2, ConfigManager.serializerSettingsCompact);
                return;
            }

            bool flag = File.Exists(text);
            string value = flag ? File.ReadAllText(text) : "{}";
            try
            {
                JsonConvert.PopulateObject(value, config2, ConfigManager.serializerSettings);
            }
            catch (Exception ex) when (flag && (ex is JsonReaderException || ex is JsonSerializationException))
            {
                Logging.tML.Warn($"Then config file {config2.Name} from the mod {config2.Mod.Name} located at {text} failed to load. The file was likely corrupted somehow, so the defaults will be loaded and the file deleted.");
                File.Delete(text);
                JsonConvert.PopulateObject("{}", config2, ConfigManager.serializerSettings);
            }
        }
        public void LoadAll()
        {
            foreach (KeyValuePair<Mod, List<ModConfig>> config in ConfigManager.Configs)
            {
                if (config.Key.Name != "ModManager")
                {
                    foreach (ModConfig item in config.Value)
                    {
                        LoadConfig(item);
                    }
                }
            }
        }
        public override void OnInitialize()
        {
            SetPadding(0);
            if (isAll)
            {
                Panel.Width.Set(-2, 1);
                Panel.Left.Set(2, 0);
            }
            else
            {
                Panel.Width.Set(-25, 1);
                Panel.Left.Set(24, 0);
            }
            Width.Set(0, 1);
            Height.Set(32, 0);

            Append(Panel);
            Panel.Append(Text);

            if (!isAll)
            {
                Toggle = new UIImage(ModManager.AssetSettingsToggle)
                {
                    VAlign = 0.5f,
                    NormalizedOrigin = Vector2.One * 0.5f,
                    Left = { Pixels = 2 }
                };
                Append(Toggle);
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (Toggle != null && Toggle.IsMouseHovering)
            {
                UIModsNew.Instance.root.UseLeft = false;
            }
            Panel.BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            if (Toggle != null && Toggle.IsMouseHovering)
            {
                LoadAll();
                return;
            }
            if (isAll)
            {
                UIModsNew.Instance.OnDeactivate();
                Main.menuMode = Interface.modConfigListID;
                Interface.modConfig.modderOnClose = () =>
                {
                    Main.menuMode = Interface.modsMenuID;
                };
                Interface.modConfig.openedFromModder = true;
            }
        }
    }

}
