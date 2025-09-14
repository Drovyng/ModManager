using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace ModManager.Content.ModsList
{
    public class UIModsPopupRename : UIPanelSizeable
    {
        public UITextDots<LocalizedText> Title;
        public UIPanelStyled OrigBG;
        public UITextDots<string> Orig;
        public UIPanelStyled InputBG;
        public UIInputTextFieldPriority<string> Input;
        public UIPanelStyled ApplyBG;
        public UITextDots<LocalizedText> Apply;
        public UIPanelStyled CancelBG;
        public UITextDots<LocalizedText> Cancel;
        public Action<string> OnApply;
        public Func<string, bool> OnApplyCustom;
        public override void OnInitialize()
        {
            SetPadding(8);
            MinHorizontal = 300;
            Width.Set(300, 0);
            Height.Set(170, 0);
            HAlign = VAlign = 0.5f;
            centered = true;
            UseLeft = UseRight = true;

            UIModsNew.Instance.ChangeSelection += () =>
            {
                Top.Pixels = -100000;
                resizing = false;
                Recalculate();
            };

            Title = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                align = 0.5f,
                text = ModManager.Get("RenameTitle"),
                scale = 1.25f
            };
            Append(Title);
            OrigBG = new()
            {
                Width = { Precent = 0.9f },
                Left = { Precent = 0.05f },
                Top = { Pixels = 32 },
                Height = { Pixels = 32 },
            };
            Orig = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                Top = { Pixels = 4 },
                align = 0.5f,
            };
            OrigBG.Append(Orig);
            Append(OrigBG);

            InputBG = new()
            {
                Width = { Precent = 0.9f },
                Left = { Precent = 0.05f },
                Top = { Pixels = 70 },
                Height = { Pixels = 32 },
            };
            InputBG.SetPadding(8);
            Input = new("", 1)
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            InputBG.Append(Input);
            Append(InputBG);
            ApplyBG = new()
            {
                Width = { Precent = 0.4f },
                Left = { Precent = 0.55f },
                Height = { Pixels = 32 },
                VAlign = 1,
                Top = { Pixels = -8 }
            };
            Apply = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                Top = { Pixels = 4 },
                align = 0.5f,
                text = ModManager.Get("Apply")
            };
            ApplyBG = ApplyBG.FadedMouseOver();
            ApplyBG.Append(Apply);
            Append(ApplyBG);

            CancelBG = new()
            {
                Width = { Precent = 0.4f },
                Left = { Precent = 0.05f },
                Height = { Pixels = 32 },
                VAlign = 1,
                Top = { Pixels = -8 }
            };
            Cancel = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                Top = { Pixels = 4 },
                align = 0.5f,
                text = ModManager.Get("Cancel")
            };
            CancelBG = CancelBG.FadedMouseOver();
            CancelBG.Append(Cancel);
            Append(CancelBG);

            CancelBG.OnLeftClick += delegate
            {
                Top.Pixels = -100000;
                resizing = false;
                Recalculate();
                OnApplyCustom = null;
            };

            ApplyBG.OnLeftClick += delegate
            {
                Input._currentString = Input._currentString.Replace("¶", "");
                if (Input._currentString.Length == 0) return;
                Top.Pixels = -100000;
                resizing = false;
                if (OnApplyCustom != null)
                {
                    if (!OnApplyCustom(Input._currentString.Replace("/", "¶"))) return;
                }
                else OnApply(Input._currentString.Replace("/", "¶"));
                Recalculate();
                OnApplyCustom = null;
            };

            Top.Pixels = -100000;
            resizing = false;
            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Input._currentString = Input._currentString.Replace("¶", "");
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                ApplyBG.LeftClick(null);
        }
        public void Popup(string origName, string Inputted, Func<string, bool> onRename = null)
        {
            resizing = false;
            Top.Pixels = 0;
            Orig.text = origName.Replace("¶", "/");
            Input.TextHint = Inputted.Replace("¶", "/");
            Input._currentString = Inputted.Replace("¶", "/");
            OnApplyCustom = onRename;
            Recalculate();
        }
    }
}
