using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ModManager.Content
{
    public enum ManagerConfigTheme : byte
    {
        Blue,
        Light
    }
    public class ManagerConfig : ModConfig
    {
        public static ManagerConfig Instance => ModContent.GetInstance<ManagerConfig>();
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [ReloadRequired]
        [DefaultValue(ManagerConfigTheme.Light)]
        public ManagerConfigTheme Theme;

        [ReloadRequired]
        [DefaultValue(false)]
        public bool ModLoadingUpgrade;
    }
}
