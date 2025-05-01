using System;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.UI;

namespace ModManager.Content.ModsList
{
    public class UIModsTableCategory : UIPanelSizeable
    {
        public UITextDots<LocalizedText> Text;
        public UITextDots<string> Filter;
        public int id;
        public bool started;
        public Action OnClick;
        public UIModsTableCategory(string text, int i)
        {
            id = i;
            Text = new()
            {
                Width = { Precent = 1f, Pixels = -32 },
                Height = { Precent = 1f },
                Left = { Pixels = 6 },
                Top = { Pixels = 4 },
                align = 0.5f,
                text = string.IsNullOrEmpty(text) ? LocalizedText.Empty : ModManager.Get("Cat_" + text),
            };
            Filter = new()
            {
                Width = { Precent = 1f, Pixels = -8 },
                Height = { Precent = 1f },
                Top = { Pixels = 4 },
                color = UIColors.ColorBorderStatic,
                align = 1f,
                scale = 1f,
            };
            if (i == -1)
            {
                Filter.align = 0.5f;
                Filter.Width.Pixels = 0;
            }
            Width = new(0, i == 3 ? 0 : 1f / (UIModsNew.Categories.Count - 2));
        }
        public override void OnInitialize()
        {
            Append(Text);
            Append(Filter);
            Height.Set(0, 1);
            SetPadding(0);
            UseLeft = id > 0 && id != 3;
            UseRight = id != -1 && id < 2;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (UIModsNew.Instance.GrabbedItem)
            {
                BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundSelected : UIColors.ColorBackgroundHovered;
                BorderColor = IsMouseHovering ? UIColors.ColorBorderAllowDropHovered : UIColors.ColorBorderAllowDrop;
            }
            else
            {
                BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
                BorderColor = IsMouseHovering ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
            }
            if (resizing)
            {
                started = false;
            }
            if (!IsMouseHovering) started = false;
            var w = GetOuterDimensions().Width;
            var add = 75 - w;
            var flag = add > 0;
            if (id == -1)
            {
                Width.Pixels += 32 - w;
                add = (UIModsNew.Instance.categoriesHorizontal.TotalSize - UIModsNew.Instance.categoriesHorizontal.GetInnerDimensions().Width) / 3;
                flag = true;
            }
            if (id == 3)
            {
                Width.Pixels += 118 - w;
                add = (UIModsNew.Instance.categoriesHorizontal.TotalSize - UIModsNew.Instance.categoriesHorizontal.GetInnerDimensions().Width) / 3;
                flag = true;
            }
            if (flag && add != 0)
            {
                foreach (var item in LeftElem)
                {
                    if (item.GetOuterDimensions().Width >= 75 + add || add < 0)
                    {
                        item.Width.Pixels -= add; if (id != 3 && id != -1) Width.Pixels += add; item.Recalculate(); Recalculate(); return;
                    }
                }
                foreach (var item in RightElem)
                {
                    if (item.GetOuterDimensions().Width >= 75 + add || add < 0)
                    {
                        item.Width.Pixels -= add; if (id != 3 && id != -1) Width.Pixels += add; item.Recalculate(); Recalculate(); return;
                    }
                }
                UIModsNew.Instance.RedesignUIMods();
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt); started = true;
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt); if (started) OnClick?.Invoke();
        }
    }
}
