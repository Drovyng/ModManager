using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ModManager.Content
{
    public class UIModsCollection : UIElement
    {
        public UITextDots<string> Text;
        public UIPanel Panel;
        public UIImage Toggle;
        public bool isAll;
        public bool toggleEnable;
        public UIModsCollection(string name)
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
                int toggled = 0;
                int c = 0;
                var l = DataConfig.Instance.Collections[Text.text];
                if (l.Count != 0)
                {
                    foreach (var item in UIModsNew.Instance.uIMods)
                    {
                        if (item.mod != null && l.Contains(item.mod.Name) && item.mod.Enabled) c++;
                    }
                    if (c == l.Count) toggled = 1;
                    else if (c != 0) toggled = 2;
                }
                toggleEnable = toggled == 0;

                Toggle = new UIImage(toggled == 0 ? ModManager.AssetToggleOff : (toggled == 1 ? ModManager.AssetToggleOn : ModManager.AssetToggleHalf))
                {
                    VAlign = 0.5f,
                    Color = toggled == 0 ? Color.White : (toggled == 1 ? new Color(0.75f, 1f, 0.75f) : new Color(1f, 0.9f, 0.75f)),
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
            if (UIModsNew.Instance.GrabbedItem != null && !isAll)
            {
                Panel.BackgroundColor = IsMouseHovering ? new Color(63, 82, 151) : new Color(63, 82, 151) * 0.75f;
                Panel.BorderColor = IsMouseHovering ? Color.LightYellow : Color.Lime;
                if (IsMouseHovering)
                {
                    UIModsNew.Instance.GrabbedFolder = "collections/" + Text.text;
                }
                return;
            }
            Panel.BackgroundColor = IsMouseHovering ? new Color(93, 102, 171) * 0.7f : new Color(63, 82, 151) * 0.7f;
            Panel.BorderColor = UIModsNew.Instance.SelectedCollection == this ? Color.Gold : Color.Black;
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            if (Toggle != null && Toggle.IsMouseHovering)
            {
                var l = DataConfig.Instance.Collections[Text.text];
                if (l.Count != 0)
                {
                    foreach (var item in UIModsNew.Instance.uIMods)
                    {
                        if (item.mod != null && l.Contains(item.mod.Name))
                        {
                            item.Set(toggleEnable);
                        }
                    }
                }
                return;
            }
            UIModsNew.Instance.OpenedCollections = true;
            UIModsNew.Instance.OpenedPath.Clear();
            if (!isAll) UIModsNew.Instance.OpenedPath.Add(Text.text);
            UIModsNew.Instance.RecalculatePath();
        }
    }

}
