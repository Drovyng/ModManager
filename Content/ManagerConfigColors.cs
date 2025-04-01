using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ModManager.Content
{
    public class ManagerConfigColors : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static ManagerConfigColors Instance => ModContent.GetInstance<ManagerConfigColors>();

        [DefaultValue(typeof(Color), "0, 0, 0, 255")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBorderStatic;
        [DefaultValue(typeof(Color), "255, 215, 0, 255")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBorderHovered;

        [DefaultValue(typeof(Color), "0, 255, 0, 255")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBorderAllowDrop;
        [DefaultValue(typeof(Color), "255, 255, 224, 255")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBorderAllowDropHovered;

        [DefaultValue(typeof(Color), "100, 50, 100, 178")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBackgroundDisabled;
        [DefaultValue(typeof(Color), "44, 57, 105, 178")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBackgroundStatic;
        [DefaultValue(typeof(Color), "65, 71, 119, 178")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBackgroundHovered;
        [DefaultValue(typeof(Color), "103, 112, 201, 255")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorBackgroundSelected;

        [DefaultValue(typeof(Color), "50, 150, 150, 50")]
        [ColorHSLSlider]
        [ReloadRequired]
        public Color ColorNeedUpdate;
    }
}
