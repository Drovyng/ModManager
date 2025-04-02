using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;

namespace ModManager.Content
{
    public static class UIUtils
    {

        public static T FadedMouseOver<T>(this T elem, Color overColor = default(Color), Color outColor = default(Color), Color overBorderColor = default(Color), Color outBorderColor = default(Color)) where T : UIPanel
        {
            if (overColor == default(Color))
            {
                overColor = UIColors.ColorBackgroundHovered;
            }

            if (outColor == default(Color))
            {
                outColor = UIColors.ColorBackgroundStatic;
            }

            if (overBorderColor == default(Color))
            {
                overBorderColor = UIColors.ColorBorderHovered;
            }

            if (outBorderColor == default(Color))
            {
                outBorderColor = UIColors.ColorBorderStatic;
            }

            elem.OnMouseOver += delegate
            {
                SoundEngine.PlaySound(in SoundID.MenuTick);
                elem.BackgroundColor = overColor;
                elem.BorderColor = overBorderColor;
            };
            elem.OnMouseOut += delegate
            {
                elem.BackgroundColor = outColor;
                elem.BorderColor = outBorderColor;
            };
            return elem;
        }
    }
}
