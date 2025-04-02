using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ModManager.Content.ModsList
{
    public class UIAlphaFixer : UIElement
    {
        public override void Draw(SpriteBatch spriteBatch)
        {
            return;
            bool overflowHidden = Parent.OverflowHidden;
            bool useImmediateMode = Parent.UseImmediateMode;
            RasterizerState rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
            Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
            SamplerState anisotropicClamp = SamplerState.AnisotropicClamp;
            if (useImmediateMode || Parent.OverrideSamplerState != null)
            {
                spriteBatch.End();
                spriteBatch.Begin(useImmediateMode ? SpriteSortMode.Immediate : SpriteSortMode.Deferred, BlendState.NonPremultiplied, (Parent.OverrideSamplerState != null) ? Parent.OverrideSamplerState : anisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
                DrawSelf(spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, anisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
            }
        }
    }
}
