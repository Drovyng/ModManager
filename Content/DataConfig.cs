﻿using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ModManager.Content
{
    public class DataConfig : ModConfig
    {
        public static DataConfig Instance => ModContent.GetInstance<DataConfig>();
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public int[] RootSize = [840, 650];
        [DefaultValue(150)]
        public int CollectionsSize = 150;

        public int[] CategoriesSizes = [90, -96, -12, -118];

        public List<string> Folders = new List<string>();
        public List<string> ConfigCollections = new List<string>();
        public Dictionary<string, string> ModPaths = new Dictionary<string, string>();
        public Dictionary<string, string> ModNames = new Dictionary<string, string>();
        public Dictionary<string, List<string>> Collections = new Dictionary<string, List<string>>();

        [DefaultValue(1f)]
        public float Scale = 1;
        [DefaultValue(1f)]
        public float ScaleText = 1;
        [DefaultValue(3f)]
        public float ScaleThreshold = 3;

        public void Save()
        {
            ConfigManager.Save(this);
        }
    }
}
