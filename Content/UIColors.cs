﻿using Microsoft.Xna.Framework;

namespace ModManager.Content
{
    public static class UIColors
    {
        public static Color ColorBorderStatic;
        public static Color ColorBorderHovered;
               
        public static Color ColorBorderAllowDrop;
        public static Color ColorBorderAllowDropHovered;
               
        public static Color ColorBackgroundDisabled;
        public static Color ColorBackgroundStatic;
        public static Color ColorBackgroundHovered;
        public static Color ColorBackgroundSelected;
               
        public static Color ColorNeedUpdate;

        public static Color ColorInvert;
        public static Color ColorSliders;

        public static void SetBlue()
        {
            ColorBorderStatic = new Color(0, 0, 0, 255);
            ColorBorderHovered = new Color(255, 215, 0, 255);

            ColorBorderAllowDrop = new Color(0, 255, 0, 255);
            ColorBorderAllowDropHovered = new Color(255, 255, 224, 255);

            ColorBackgroundDisabled = new Color(100, 50, 100, 178);
            ColorBackgroundStatic = new Color(44, 57, 105, 178);
            ColorBackgroundHovered = new Color(65, 71, 119, 178);
            ColorBackgroundSelected = new Color(103, 112, 201, 255);

            ColorNeedUpdate = new Color(50, 150, 150, 50);

            ColorInvert = Color.Black;
            ColorSliders = Color.White;
        }
        public static void SetLight()
        {
            ColorBorderStatic = new Color(0, 0, 0) * 0.75f;
            ColorBorderHovered = new Color(255, 215, 0);

            ColorBorderAllowDrop = new Color(0, 255, 0) * 0.75f;
            ColorBorderAllowDropHovered = new Color(255, 255, 220) * 0.75f;

            ColorBackgroundDisabled = new Color(170, 64, 170) * 0.35f;
            ColorBackgroundStatic = new Color(128, 128, 128) * 0.35f;
            ColorBackgroundHovered = new Color(192, 192, 192) * 0.35f;
            ColorBackgroundSelected = new Color(255, 255, 255) * 0.5f;

            ColorNeedUpdate = new Color(64, 192, 192) * 0.55f;

            ColorInvert = Color.Gray;
            ColorSliders = Color.White;
        }
        public static void SetDark()
        {
            ColorBorderStatic = new Color(150, 150, 150);
            ColorBorderHovered = new Color(255, 215, 0);

            ColorBorderAllowDrop = new Color(0, 255, 0);
            ColorBorderAllowDropHovered = new Color(255, 255, 220);

            ColorBackgroundDisabled = new Color(128, 8, 128) * 0.6f;
            ColorBackgroundStatic = new Color(8, 8, 8) * 0.6f;
            ColorBackgroundHovered = new Color(24, 24, 24) * 0.6f;
            ColorBackgroundSelected = new Color(48, 48, 48) * 0.8f;

            ColorNeedUpdate = new Color(0, 128, 128) * 0.75f;

            ColorInvert = Color.Gray;
            ColorSliders = Color.White;
        }
        public static void SetConfig()
        {
            switch (ManagerConfig.Instance.Theme)
            {
                case ManagerConfigTheme.Light:
                    SetLight();
                    return;
                case ManagerConfigTheme.Dark:
                    SetDark();
                    return;
                default:
                    SetBlue();
                    return;
            }
        }
    }
}
