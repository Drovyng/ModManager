using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ModManager.Content.ModsList;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;
using static ModManager.Content.ModsList.UIModsNew;

namespace ModManager.Content.ModsBrowser
{
    public class UIModBrowserNew : UIModBrowser
    {
        public static UIModBrowserNew Instance;

        public UIModBrowserItemNew Selected;

        public float scale = DataConfigBrowser.Instance.Scale;
        public float scaleText = DataConfigBrowser.Instance.ScaleText;
        public float scaleThreshold = DataConfigBrowser.Instance.ScaleThreshold;

        public UIPanelSizeable root;

        public UIList mainList;
        public UIElement mainListIn;
        public UIScrollbar mainScrollbar;

        public UIPanel categoriesHorizontalOut;
        public UIContentList categoriesHorizontal;

        public static readonly IReadOnlyList<string> Categories = new List<string>()
        {
            "Name", "Author", "Date"
        };
        
        public UIModBrowserNew(SocialBrowserModule socialBackend) : base(socialBackend)
        {
            Instance = this;
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            Elements.Clear();

            root = new()
            {
                Width = { Pixels = DataConfig.Instance.RootSize[0] },
                Height = { Pixels = DataConfig.Instance.RootSize[1] },
                HAlign = 0.5f,
                VAlign = 0.5f,
                MinHorizontal = 400,
                MinVertical = 350
            };
            root.OnResizing += Redesign;
            Append(root);

            categoriesHorizontalOut = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Pixels = 32 },
            };
            categoriesHorizontalOut.SetPadding(0);
            Append(categoriesHorizontalOut);

            categoriesHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            categoriesHorizontalOut.Append(categoriesHorizontal);

            mainScrollbar = new UIScrollbar()
            {
                HAlign = 1,
                Height = { Precent = 1 },
                MarginRight = 12
            }.WithView(100, 1000);
            mainList = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Precent = 1, Pixels = -32 },
                Top = { Pixels = 32 },
                OverflowHidden = true
            };
            mainListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            mainList.Append(mainListIn);
            mainList.OnScrollWheel += (e, l) => { mainScrollbar.ViewPosition -= e.ScrollWheelValue; };
            mainListIn.OnUpdate += (e) => { mainListIn.Top.Pixels = -mainScrollbar.ViewPosition; };
            mainList.Append(mainListIn);

            Append(mainScrollbar);
            Append(mainList);

            //AddCategories();
            Redesign();
        }
        public override void Update(GameTime gameTime)
        {
            foreach (UIElement element in Elements)
            {
                element.Update(gameTime);
            }

            var res = (categoriesHorizontal._innerDimensions.Width - categoriesHorizontal.TotalSize) / 3f;
            if (res != 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    categoriesHorizontal.Elements[i].Width.Pixels += res;
                }
                categoriesHorizontal.Recalculate();
            }
        }
        public void Redesign()
        {
            var pos = Vector2.UnitY * 500;


            mainListIn.Height.Pixels = pos.Y;
            mainList.Recalculate();
            mainScrollbar.SetView(mainList.GetInnerDimensions().Height, pos.Y);
        }
        /*
        public void AddCategories()
        {
            categoriesHorizontal.Elements.Clear();
            var k = 0;
            foreach (var item in Categories)
            {
                k++;
                var j = k;
                var i = new UIPanelSizeable()
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 1 },
                    UseLeft = j > 1,
                    UseRight = j < 3
                };
                var c = item;
                i.Append(new UITextDots<string>()
                {
                    Width = { Precent = 1f },
                    Height = { Precent = 1f },
                    Left = { Pixels = 6 },
                    Top = { Pixels = 4 },
                    align = 0.5f,
                    text = c,
                });
                i.Width.Pixels = DataConfig.Instance.CategoriesSizes[j - 1];
                i.OnResizing = Redesign;
                categoriesHorizontal.Append(i);
            }
            for (int i = 0; i < Categories.Count; i++)
            {
                var item = categoriesHorizontal.Elements[i] as UIModsTableCategory;
                for (int j = 0; j < i; j++)
                {
                    item.LeftElem.Add(categoriesHorizontal.Elements[j]);
                }
                for (int j = i + 1; j < Categories.Count - 1; j++)
                {
                    item.RightElem.Add(categoriesHorizontal.Elements[j-1]);
                }
            }
            if (scale < scaleThreshold)
            {
                categoriesHorizontal.Append(new UIPanelSizeable()
                {
                    Width = { Pixels = 100 },
                    Height = { Precent = 1 },
                });
            }
            categoriesHorizontal.Activate();
        }
        */
    }
}
