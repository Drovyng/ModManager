using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace ModManager.Content
{
    public class UIPanelStyled : UIPanel
    {
        public UIPanelStyled() : base(ModManager.AssetStyledBackground, ModManager.AssetStyledBorder, 12, 4)
        {
            BackgroundColor = UIColors.ColorBackgroundStatic;
            BorderColor = UIColors.ColorBorderStatic;
        }
    }
}
