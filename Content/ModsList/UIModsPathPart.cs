using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;

namespace ModManager.Content.ModsList
{
    public class UIModsPathPart : UIPanel
    {
        public UITextDots<string> Text;
        public string path;
        public UIModsPathPart(string text, string _path)
        {
            path = _path;
            var size = FontAssets.MouseText.Value.MeasureString(text + ".").X + 10;
            Text = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                text = text,
                Left = { Pixels = 6 },
                Top = { Pixels = 4 }
            };
            Width = new(size, 0);
        }
        public override void OnInitialize()
        {
            Append(Text);
            Height.Set(0, 1);
            SetPadding(0);
        }
        public override void Update(GameTime gameTime)
        {
            var colors = ManagerConfigColors.Instance;
            if (UIModsNew.Instance.GrabbedItem)
            {
                BackgroundColor = IsMouseHovering ? colors.ColorBackgroundSelected : colors.ColorBackgroundHovered;
                BorderColor = IsMouseHovering ? colors.ColorBorderAllowDropHovered : colors.ColorBorderAllowDrop;
                if (IsMouseHovering)
                {
                    UIModsNew.Instance.GrabbedFolder = path;
                }
                return;
            }
            BackgroundColor = IsMouseHovering ? colors.ColorBackgroundHovered : colors.ColorBackgroundStatic;
            BorderColor = IsMouseHovering ? colors.ColorBorderHovered : colors.ColorBorderStatic;
        }
    }

}
