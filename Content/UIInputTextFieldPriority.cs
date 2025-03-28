using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content
{
    public static class UIInputTextFieldPriority
    {
        public static int MaxPriority;
    }
    public class UIInputTextFieldPriority<T> : UIInputTextField
    {
        public int Priority;
        public T TextHint;
        public UIInputTextFieldPriority(T textHint, int priority) : base("")
        {
            TextHint = textHint;
            Priority = priority;
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            string text = _currentString;
            if (UIInputTextFieldPriority.MaxPriority == Priority)
            {
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                Text = Main.GetInputText(_currentString);
                if (++_textBlinkerCount / 20 % 2 == 0)
                {
                    text += "|";
                }
            }
            CalculatedStyle dimensions = GetDimensions();
            if (_currentString.Length == 0)
            {
                Utils.DrawBorderString(spriteBatch, TextHint.ToString(), new Vector2(dimensions.X, dimensions.Y), Color.Gray);
            }
            else
            {
                Utils.DrawBorderString(spriteBatch, text, new Vector2(dimensions.X, dimensions.Y), Color.White);
            }
        }
    }
}
