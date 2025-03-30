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
            if (UIModsNew.Instance.GrabbedItem)
            {
                BackgroundColor = IsMouseHovering ? new Color(63, 82, 151) : new Color(63, 82, 151) * 0.75f;
                BorderColor = IsMouseHovering ? Color.LightYellow : Color.Lime;
                if (IsMouseHovering)
                {
                    UIModsNew.Instance.GrabbedFolder = path;
                }
                return;
            }
            BackgroundColor = IsMouseHovering ? new Color(63, 82, 151) * 0.75f : new Color(63, 82, 151) * 0.25f;
            BorderColor = IsMouseHovering ? Color.Gold : Color.Black;
        }
    }

}
