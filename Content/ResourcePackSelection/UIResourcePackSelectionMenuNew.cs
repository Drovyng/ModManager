﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModManager.Content.ModsList;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content.ResourcePackSelection
{
    public class UIResourcePackSelectionMenuNew : UIState
    {
        public static UIResourcePackSelectionMenuNew Instance;

        public float scale = DataConfigBrowser.Instance.Scale;
        public float scaleText = DataConfigBrowser.Instance.ScaleText;
        public bool scaleGrid = DataConfigBrowser.Instance.ScaleGrid;

        public UIResourcePackToSort grabbed;
        public Vector2 grabbedPos;

        private AssetSourceController _sourceController;
        private ResourcePackList _packsList;

        public UIPanelSizeable root;

        public UIList mainList;
        public UIDoNotDrawNonArea mainListIn;
        public UIScrollbar mainScrollbar;

        public UIList sortList;
        public UIDoNotDrawNonArea sortListIn;
        public UIScrollbar sortScrollbar;

        public UIPanelStyled filtersHorizontalOut;
        public UIContentList filtersHorizontal;
        public int currentFiltersSet;

        public UIPanelStyled categoriesHorizontalOut;
        public UIContentList categoriesHorizontal;

        public string Tooltip;
        public int FilterCategory = 1;
        public int FilterCategoryType = 0;

        public List<UIResourcePackNew> uIResourcePacks;
        public List<UIResourcePackToSort> uIResourcePacksSorted;

        public List<(UIResourcePackNew, bool)> ToUndo = new();
        public List<(UIResourcePackNew, bool)> ToRedo = new();

        public static readonly IReadOnlyList<string> Categories = new List<string>()
        {
            "", "Name", "Author", "Date"
        };
        public UIResourcePackSelectionMenuNew(UIState uiStateToGoBackTo, AssetSourceController sourceController, ResourcePackList currentResourcePackList)
        {
            Instance = this;
            _sourceController = sourceController;
            _packsList = currentResourcePackList;
        }
        public override void OnInitialize()
        {
            Elements.Clear();

            root = new()
            {
                Width = { Pixels = MathF.Max(DataConfigResourcePack.Instance.RootSize[0], 400) },
                Height = { Pixels = MathF.Max(DataConfigResourcePack.Instance.RootSize[1], 350) },
                HAlign = 0.5f,
                VAlign = 0.5f,
                MinHorizontal = 400,
                MinVertical = 350,
                UseDown = true,
                UseUp = true,
                UseLeft = true,
                UseRight = true,
                centered = true
            };
            root.SetPadding(0);
            root.OnResizing += Redesign;
            Append(root);

            categoriesHorizontalOut = new()
            {
                Width = { Precent = 0.7f },
                Height = { Pixels = 32 },
                Top = { Pixels = 100 },
                Left = { Precent = 0.3f }
            };
            categoriesHorizontalOut.SetPadding(0);
            root.Append(categoriesHorizontalOut);

            categoriesHorizontal = new()
            {
                Width = { Precent = 1f },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            categoriesHorizontalOut.Append(categoriesHorizontal);

            var main = new UIPanelStyled()
            {
                Width = { Precent = 0.7f },
                Height = { Precent = 1, Pixels = -132 },
                HAlign = 1,
                Top = { Pixels = 132 }
            }; main.SetPadding(0);
            mainScrollbar = new UIScrollbar()
            {
                HAlign = 1,
                Height = { Precent = 1, Pixels = -16 },
                MarginRight = 12,
                Top = { Pixels = 8 }
            }.WithView(1000, 1000);
            mainList = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            mainListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            mainList.OnScrollWheel += (e, _) => { mainScrollbar.ViewPosition -= e.ScrollWheelValue; if (e.ScrollWheelValue != 0) Redesign(); };
            mainListIn.OnUpdate += (_) => { mainListIn.Top.Pixels = -mainScrollbar.ViewPosition; mainList.Recalculate(); };
            mainList.Append(mainListIn);
            main.Append(mainScrollbar);
            main.Append(mainList);
            root.Append(main);

            var sort = new UIPanelStyled()
            {
                Width = { Precent = 0.3f },
                Height = { Precent = 1, Pixels = -100 },
                Top = { Pixels = 100 }
            }; sort.SetPadding(0);
            sortScrollbar = new UIScrollbar()
            {
                HAlign = 1,
                Height = { Precent = 1, Pixels = -16 },
                MarginRight = 12,
                Top = { Pixels = 8 }
            }.WithView(1000, 1000);
            sortList = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            sortListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            sortList.OnScrollWheel += (e, _) => { sortScrollbar.ViewPosition -= e.ScrollWheelValue; if (e.ScrollWheelValue != 0) Redesign(); };
            sortListIn.OnUpdate += (_) => { sortListIn.Top.Pixels = -sortScrollbar.ViewPosition; sortList.Recalculate(); };
            sortList.Append(sortListIn);
            sort.Append(sortScrollbar);
            sort.Append(sortList);
            root.Append(sort);

            var topPanel = new UIPanelStyled()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 100 }
            };
            topPanel.SetPadding(8);
            root.Append(topPanel);
            {
                var ontopSettings = new UIPanelStyled()
                {
                    Width = { Precent = 0.5f },
                    Height = { Precent = 1 }
                }; ontopSettings.SetPadding(8);
                var labelScale = new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 6 },
                    Height = { Pixels = 16 },
                    text = ModManager.Get("S_Scale")
                };
                var sliderScale = new UISliderNew()
                {
                    Width = { Precent = 0.55f },
                    Left = { Precent = 0.45f },
                    Top = { Pixels = 2 },
                    minimum = 1,
                    maximum = 6,
                    value = DataConfig.Instance.Scale
                };
                var labelTextScale = new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 30 },
                    Height = { Pixels = 16 },
                    text = ModManager.Get("S_TextScale")
                };
                var sliderTextScale = new UISliderNew()
                {
                    Width = { Precent = 0.55f },
                    Left = { Precent = 0.45f },
                    Top = { Pixels = 26 },
                    minimum = 0.25f,
                    maximum = 1.5f,
                    value = DataConfig.Instance.ScaleText
                };

                var labelGridScale = new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 54 },
                    Height = { Pixels = 16 },
                    text = ModManager.Get("ViewStyle")
                };
                var buttonListScale = new UIPanelStyled()
                {
                    Width = { Precent = 0.275f },
                    Left = { Precent = 0.45f },
                    Top = { Pixels = 50 },
                    Height = { Pixels = 20 },
                }.FadedMouseOver();
                buttonListScale.OnLeftClick += delegate { scaleGrid = false; Redesign(); };
                buttonListScale.OnUpdate += delegate { buttonListScale.BorderColor = scaleGrid ? UIColors.ColorBorderStatic : UIColors.ColorBorderHovered; };
                buttonListScale.Append(new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    align = 0.5f,
                    Top = { Pixels = 2 },
                    text = ModManager.Get("ViewList")
                });
                var buttonGridScale = new UIPanelStyled()
                {
                    Width = { Precent = 0.275f },
                    Left = { Precent = 0.725f },
                    Top = { Pixels = 50 },
                    Height = { Pixels = 20 },
                }.FadedMouseOver();
                buttonGridScale.OnLeftClick += delegate { scaleGrid = true; Redesign(); };
                buttonGridScale.OnUpdate += delegate { buttonGridScale.BorderColor = scaleGrid ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic; };
                buttonGridScale.Append(new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    align = 0.5f,
                    Top = { Pixels = 2 },
                    text = ModManager.Get("ViewGrid")
                });
                sliderScale.OnChange += () =>
                {
                    scale = sliderScale.value;
                    Redesign();
                };
                sliderTextScale.OnChange += () =>
                {
                    scaleText = sliderTextScale.value;
                    Redesign();
                };
                ontopSettings.Append(labelScale);
                ontopSettings.Append(sliderScale);
                ontopSettings.Append(labelTextScale);
                ontopSettings.Append(sliderTextScale);
                ontopSettings.Append(labelGridScale);
                ontopSettings.Append(buttonListScale);
                ontopSettings.Append(buttonGridScale);
                topPanel.Append(ontopSettings);
            }
            {
                var ontopButtons = new UIPanelStyled()
                {
                    Width = { Precent = 0.5f },
                    Left = { Precent = 0.5f },
                    Height = { Precent = 1 }
                }; ontopButtons.SetPadding(0);
                var buttonEnableAll = new UIPanelStyled()
                {
                    Width = { Precent = 0.5f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 2f / 3f }
                }.FadedMouseOver();
                buttonEnableAll.Append(new UITextDots<LocalizedText>()
                {
                    text = Language.GetText("tModLoader.ModsEnableAll"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonEnableAll.OnLeftClick += (e, l) =>
                {
                    foreach (var item in uIResourcePacks)
                    {
                        item.Set(true);
                    }
                    UpdateDisplayerToSort();
                };
                ontopButtons.Append(buttonEnableAll);
                var buttonDisableAll = new UIPanelStyled()
                {
                    Width = { Precent = 0.5f },
                    Left = { Precent = 0.5f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 2f / 3f }
                }.FadedMouseOver();
                buttonDisableAll.Append(new UITextDots<LocalizedText>()
                {
                    text = Language.GetText("tModLoader.ModsDisableAll"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonDisableAll.OnLeftClick += (e, l) =>
                {
                    foreach (var item in uIResourcePacks)
                    {
                        item.Set(false);
                    }
                    UpdateDisplayerToSort();
                };
                ontopButtons.Append(buttonDisableAll);
                var buttonApply = new UIPanelStyled()
                {
                    Width = { Precent = 1f },
                    Height = { Precent = 1f / 3f },
                }.FadedMouseOver();
                buttonApply.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("ButtonApplyChanges"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonApply.OnLeftClick += (e, l) =>
                {
                    SoundEngine.PlaySound(11);
                    _sourceController.UseResourcePacks(new ResourcePackList(uIResourcePacks.Where(p => p.pack.IsEnabled).Select(pack => pack.pack)));
                    Main.SaveSettings();
                    Populate();
                };
                ontopButtons.Append(buttonApply);
                var buttonReject = new UIPanelStyled()
                {
                    Width = { Precent = 1f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 1f / 3f },
                }.FadedMouseOver();
                buttonReject.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("ButtonRejectChanges"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonReject.OnLeftClick += (e, l) =>
                {
                    foreach (var item in uIResourcePacks)
                    {
                        item.Set(item.loaded);
                    }
                };
                ontopButtons.Append(buttonReject);
                topPanel.Append(ontopButtons);
            }
            AddCategories();
        }
        public override void OnActivate()
        {
            ToUndo = new();
            ToRedo = new();
            Populate();
        }
        public void Populate()
        {
            uIResourcePacks = new(_packsList._resourcePacks.Count);
            uIResourcePacksSorted = new(_packsList._resourcePacks.Count);
            foreach (var item in _packsList._resourcePacks)
            {
                uIResourcePacks.Add(new UIResourcePackNew(item));
                uIResourcePacksSorted.Add(new UIResourcePackToSort(item));
            }
            UpdateDisplayed();
            foreach (var item in uIResourcePacks)
            {
                if (!item.pack.IsEnabled) continue;
                item.loaded = true;
                item.Set(true);
            }
            ToRedo.Clear();
            ToUndo.Clear();
        }
        public void UpdateDisplayed()
        {
            mainListIn.Elements.Clear();
            var list = uIResourcePacks.ToList();
            list.Sort((left, right) => { return left.Sort(right, FilterCategory, FilterCategoryType); });
            foreach (var item in list)
            {
                mainListIn.Append(item);
                item.Activate();
            }
            mainListIn.Recalculate();
            Redesign();
            UpdateDisplayerToSort();
        }
        public void UpdateDisplayerToSort()
        {
            sortListIn.Elements.Clear();
            uIResourcePacksSorted.Sort((left, right) => { return left.Sort(right); });
            int num = 0;
            foreach (var item in uIResourcePacksSorted.Where(pack => pack.pack.IsEnabled))
            {
                item.pack.SortingOrder = num;
                num += 2;
                sortListIn.Append(item);
                item.Activate();
            }
            sortListIn.Recalculate();
            RedesingSorted();
        }
        public void RedesingSorted()
        {
            float pos = 0f;
            var c = sortList.GetInnerDimensions();
            foreach (var item in sortListIn.Elements)
            {
                var pack = item as UIResourcePackToSort;
                pack.Redesign();
                pack.Left.Precent = 0;
                pack.Left.Pixels = 0;
                pack.Top.Pixels = pos;
                pos += pack.GetOuterDimensions().Height;
            }
            pos = MathF.Max(pos, c.Height);
            sortListIn.Height.Pixels = pos;
            sortList.Recalculate();
            sortScrollbar.SetView(c.Height, pos);
        }
        public void Redesign()
        {
            var pos = Vector2.Zero;
            var c = mainList.GetInnerDimensions();
            float addGridHeight = 0;
            foreach (var item in mainListIn.Elements)
            {
                var pack = item as UIResourcePackNew;
                pack.Redesign();
                if (scaleGrid)
                {
                    float add = 1f / (int)(c.Width / pack.GetOuterDimensions().Width);

                    if (pos.X + pack.GetOuterDimensions().Width / c.Width > 1)
                    {
                        pos.Y += pack.GetOuterDimensions().Height;
                        pos.Y += pack.GetOuterDimensions().Height;
                        pack.Left.Precent = 0;
                        pos.X = add;
                        addGridHeight = 0;
                    }
                    else
                    {
                        pack.Left.Precent = pos.X;
                        pos.X += add;
                        addGridHeight = pack.GetOuterDimensions().Height;
                    }
                }
                else pack.Left.Precent = 0;
                pack.Left.Pixels = 0;
                pack.Top.Pixels = pos.Y;
                if (!scaleGrid) pos.Y += pack.GetOuterDimensions().Height;
            }
            if (scaleGrid) pos.Y += addGridHeight;
            pos.Y = MathF.Max(pos.Y, c.Height);
            mainListIn.Height.Pixels = pos.Y;
            mainList.Recalculate();
            mainScrollbar.SetView(c.Height, pos.Y);
            RedesingSorted();
        }
        public void AddCategories()
        {
            categoriesHorizontal.Elements.Clear();
            var k = 0;
            foreach (var item in Categories)
            {
                k++;
                var j = k;
                var i = new UIResourcePackCategory()
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 1 },
                    UseLeft = j > 2,
                    UseRight = j < 4
                };
                i.FadedMouseOver();
                i.clicked = delegate
                {
                    if (FilterCategory != j - 1)
                    {
                        FilterCategory = j - 1;
                        FilterCategoryType = 0;
                    }
                    else
                    {
                        FilterCategoryType++;
                        FilterCategoryType %= 2;
                    }
                    AddCategories();
                    Update(new GameTime());
                    UpdateDisplayed();
                };
                var c = item;
                i.Append(new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 1f },
                    Height = { Precent = 1f },
                    Left = { Pixels = 6 },
                    Top = { Pixels = 4 },
                    align = 0.5f,
                    text = string.IsNullOrEmpty(c) ? LocalizedText.Empty : ModManager.Get("Cat_" + c),
                });
                if (j != 1) i.Width.Pixels = DataConfigBrowser.Instance.CategoriesSizes[j - 2];
                else i.Width.Set(100, 0);
                i.OnResizing = Redesign;
                categoriesHorizontal.Append(i);
                var filter = new UITextDots<string>()
                {
                    Width = { Precent = 1f, Pixels = -8 },
                    Height = { Precent = 1f },
                    Top = { Pixels = 4 },
                    color = UIColors.ColorBorderStatic,
                    align = 1f,
                    scale = 1f,
                    text = FilterCategory == j - 1 ? UIModsNew.Filters[FilterCategoryType] : UIModsNew.Filters[2]
                };
                if (j == 1)
                {
                    filter.align = 0.5f;
                    filter.Width.Pixels = 0;
                }
                i.Append(filter);
            }
            for (int i = 1; i < Categories.Count - 1; i++)
            {
                var item = categoriesHorizontal.Elements[i] as UIPanelSizeable;
                for (int j = 1; j < i; j++)
                {
                    item.LeftElem.Add(categoriesHorizontal.Elements[j]);
                }
                for (int j = i + 1; j < Categories.Count - 1; j++)
                {
                    item.RightElem.Add(categoriesHorizontal.Elements[j]);
                }
            }
            categoriesHorizontal.Activate();
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (grabbed == null) return;
            if (sortList.IsMouseHovering)
            {
                var b = true;
                foreach (var item in sortListIn.Elements)
                {
                    if (item._innerDimensions.Y + item._innerDimensions.Height * 0.5f >= Main.mouseY)
                    {
                        grabbed.pack.SortingOrder = (item as UIResourcePackToSort).pack.SortingOrder - 1;
                        b = false;
                        break;
                    }
                }
                if (b) grabbed.pack.SortingOrder = (sortListIn.Elements[sortListIn.Elements.Count - 1] as UIResourcePackToSort).pack.SortingOrder + 1;
                UpdateDisplayerToSort();
            }
            grabbed = null;
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.keyState.IsKeyDown(Keys.Escape))
            {
                Main.MenuUI.GoBack();
                return;
            }

            base.Update(gameTime);

            if (!Main.mouseLeft && grabbed != null) LeftMouseUp(null);

            if (Main.keyState.PressingControl())
            {
                if (Main.oldKeyState.IsKeyUp(Keys.Z) && Main.keyState.IsKeyDown(Keys.Z))
                {
                    if (ToUndo.Count == 0) return;
                    var i = ToUndo[ToUndo.Count - 1];
                    ToRedo.Insert(0, (i.Item1, i.Item1.pack.IsEnabled));
                    ToUndo.Remove(i);
                    i.Item1.Set(i.Item2);
                }
                else if (Main.oldKeyState.IsKeyUp(Keys.Y) && Main.keyState.IsKeyDown(Keys.Y))
                {
                    if (ToRedo.Count == 0) return;
                    var i = ToRedo[0];
                    ToUndo.Add((i.Item1, i.Item1.pack.IsEnabled));
                    ToRedo.Remove(i);
                    i.Item1.Set(i.Item2);
                }
            }

            var p = categoriesHorizontal.Elements[0].Width.Pixels;
            categoriesHorizontal.Elements[0].Width.Pixels = 32;
            if (p != categoriesHorizontal.Elements[0].Width.Pixels) Redesign();
            var res = (categoriesHorizontal._innerDimensions.Width - categoriesHorizontal.TotalSize) / 3f;
            if (res != 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    categoriesHorizontal.Elements[i + 1].Width.Pixels += res;
                }
                categoriesHorizontal.Recalculate();
                Redesign();
            }

            saveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (saveTimer <= 0)
            {
                Save();
            }
        }
        float saveTimer = 5;
        public void Save()
        {
            saveTimer = 2.5f;
            var cfg = DataConfigResourcePack.Instance;

            cfg.RootSize[0] = (int)root.Width.Pixels;
            cfg.RootSize[1] = (int)root.Height.Pixels;

            for (int i = 1; i < Categories.Count - 1; i++)
            {
                cfg.CategoriesSizes[i] = (int)categoriesHorizontal.Elements[i].Width.Pixels;
            }

            cfg.Scale = scale;
            cfg.ScaleGrid = scaleGrid;
            cfg.ScaleText = scaleText;

            cfg.Save();
        }
        public override void DrawChildren(SpriteBatch spriteBatch)
        {
            base.DrawChildren(spriteBatch);
            if (grabbed != null)
            {
                if (sortList.IsMouseHovering)
                {
                    var r = sortListIn.Elements[sortListIn.Elements.Count - 1]._outerDimensions.ToRectangle();
                    r.Y += r.Height - 3;
                    r.Height = 6;
                    foreach (var item in sortListIn.Elements)
                    {
                        if (item._innerDimensions.Y + item._innerDimensions.Height * 0.5f >= Main.mouseY)
                        {
                            r = item._outerDimensions.ToRectangle();
                            r.Y -= 3;
                            r.Height = 6;
                            break;
                        }
                    }
                    spriteBatch.Draw(TextureAssets.BlackTile.Value, r, UIColors.ColorBorderHovered);
                }
                var off = new Vector2(Main.mouseX, Main.mouseY) - grabbedPos;

                grabbed._dimensions.X += off.X;
                grabbed._innerDimensions.X += off.X;
                grabbed._outerDimensions.X += off.X;
                grabbed._dimensions.Y += off.Y;
                grabbed._innerDimensions.Y += off.Y;
                grabbed._outerDimensions.Y += off.Y;
                grabbed.RecalculateChildren();

                grabbed.enableDraw = true;
                grabbed.Draw(spriteBatch);

                grabbed.Recalculate();
            }
        }
    }
}
