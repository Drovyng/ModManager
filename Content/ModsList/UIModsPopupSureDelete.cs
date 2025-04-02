using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace ModManager.Content.ModsList
{
    public class UIModsPopupSureDelete : UIPanelSizeable
    {
        public UITextDots<LocalizedText> Title;
        public UITextDots<LocalizedText> Description;
        public UIPanelStyled ConfirmBG;
        public UITextDots<LocalizedText> Confirm;
        public UIPanelStyled CancelBG;
        public UITextDots<LocalizedText> Cancel;
        public Action OnApply;
        public override void OnInitialize()
        {
            UIModsNew.Instance.ChangeSelection += () =>
            {
                Top.Pixels = -100000;
                resizing = false;
                Recalculate();
            };

            SetPadding(8);
            MinHorizontal = 300;
            Width.Set(320, 0);
            Height.Set(120, 0);
            HAlign = VAlign = 0.5f;
            centered = true;
            UseLeft = UseRight = true;

            BackgroundColor = UICommon.DefaultUIBlue * 0.825f;

            Title = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                align = 0.5f,
                text = ModManager.Get("DeleteTitle"),
                scale = 1.25f
            };
            Append(Title);
            Description = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                Top = { Pixels = 32 },
                align = 0.5f,
                text = ModManager.Get("Delete")
            };
            Append(Description);
            ConfirmBG = new()
            {
                Width = { Precent = 0.4f },
                Left = { Precent = 0.55f },
                Height = { Pixels = 32 },
                VAlign = 1,
                Top = { Pixels = -8 }
            };
            Confirm = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                Top = { Pixels = 4 },
                align = 0.5f,
                text = ModManager.Get("Confirm")
            };
            ConfirmBG = ConfirmBG.WithFadedMouseOver();
            ConfirmBG.Append(Confirm);
            Append(ConfirmBG);

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
            CancelBG = CancelBG.WithFadedMouseOver();
            CancelBG.Append(Cancel);
            Append(CancelBG);

            CancelBG.OnLeftClick += (e, l) =>
            {
                Top.Pixels = -100000;
                resizing = false;
                Recalculate();
            };

            ConfirmBG.OnLeftClick += (e, l) =>
            {
                Top.Pixels = -100000;
                resizing = false;
                OnApply();
                Recalculate();
            };

            Top.Pixels = -100000;
            resizing = false;
            Recalculate();

            UIModsNew.Instance.root.OnLeftMouseDown += (m, l) =>
            {
                Top.Pixels = -100000;
                resizing = false;
                Recalculate();
            };
        }
        public void Popup()
        {
            Description.text = ModManager.Get("DeleteDesc" + (UIModsNew.Instance.SelectedItem.mod == null ? "Folder" : "Mod"));
            Top.Pixels = 0;
            resizing = false;
            Recalculate();
        }
    }
}
