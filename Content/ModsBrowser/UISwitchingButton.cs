using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace ModManager.Content.ModsBrowser
{
    public class UISwitchingButton<T> : UIPanelStyled where T : struct, Enum
    {
        public UITextDots<string> TextDisplay;
        public LocalizedText text;
        public UIBrowserFilterToggle<T> toggle;
        public Action OnChange;
        public UISwitchingButton(LocalizedText _text, UIBrowserFilterToggle<T> _toggle)
        {
            text = _text;
            toggle = _toggle;
        }
        public override void OnInitialize()
        {
            SetPadding(0);
            this.FadedMouseOver();

            TextDisplay = new()
            {
                Width = { Precent = 1, Pixels = -16 },
                Height = { Precent = 1 },
                Top = { Pixels = 4 },
                Left = { Pixels = 8 }
            };
            Append(TextDisplay);
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            toggle.UpdateToNext(null, null);
            OnChange?.Invoke();
        }
        public override void RightClick(UIMouseEvent evt)
        {
            toggle.UpdateToPrevious(null, null);
            OnChange?.Invoke();
        }
        public override void Update(GameTime gameTime)
        {
            var s = toggle.State;
            var t = "";

            if (s is ModBrowserSortMode s1)
            {
                t = s1.ToFriendlyString();
            }
            else if (s is ModBrowserTimePeriod s2)
            {
                t = s2.ToFriendlyString();
            }
            else if (s is UpdateFilter s3)
            {
                t = s3.ToFriendlyString();
            }
            else if (s is SearchFilter s4)
            {
                t = s4.ToFriendlyString();
            }
            else if (s is ModSideFilter s5)
            {
                t = s5.ToFriendlyString();
            }
            TextDisplay.text = text.ToString() + ": " + t;

            if (IsMouseHovering) UIModBrowserNew.Instance.Tooltip = t;

            IgnoresMouseInteraction = toggle.Disabled;
            BackgroundColor = toggle.Disabled ? UIColors.ColorBackgroundDisabled : (IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
        }
    }
}
