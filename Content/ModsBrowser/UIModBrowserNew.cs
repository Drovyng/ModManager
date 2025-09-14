using Microsoft.Xna.Framework;
using ModManager.Content.ModsList;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using Terraria.UI;

namespace ModManager.Content.ModsBrowser
{
    public class UIModBrowserNew : UIModBrowser
    {
        public class UIAsyncList_DownloadItems : UIAsyncList<ModDownloadItem, UIModBrowserItemNew>
        {
            public bool NeedUpdate;
            public List<UIElement> Items = new();
            public override UIModBrowserItemNew GenElement(ModDownloadItem resource)
            {
                NeedUpdate = true;
                return new UIModBrowserItemNew(resource);
            }
            public override void AddRange(IEnumerable<UIElement> items)
            {
                Items.AddRange(items);
            }
            public override List<SnapPoint> GetSnapPoints() => new List<SnapPoint>();
        }
        public static UIModBrowserNew Instance;

        public float scale = DataConfigBrowser.Instance.Scale;
        public float scaleText = DataConfigBrowser.Instance.ScaleText;
        public bool scaleGrid = DataConfigBrowser.Instance.ScaleGrid;

        public UIPanelSizeable root;

        public UIList mainList;
        public UIDoNotDrawNonArea mainListIn;
        public UIScrollbar mainScrollbar;

        public UIPanelStyled filtersHorizontalOut;
        public UIContentList filtersHorizontal;
        public int currentFiltersSet;

        public UIPanelStyled categoriesHorizontalOut;
        public UIContentList categoriesHorizontal;

        public readonly UIAsyncList_DownloadItems ModsList = new();

        public bool firstLoad;
        public bool needUpdate;
        public string Tooltip;

        public static readonly IReadOnlyList<string> Categories = new List<string>()
        {
            "Name", "Author", "Date", ""
        };

        public UIModBrowserNew(SocialBrowserModule socialBackend) : base(socialBackend)
        {
            Instance = this;
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            Elements.Clear();
            Append(new UIMMTopPanel());

            ModsList.OnFinished += delegate { };
            ModsList.OnStartLoading += delegate { };
            ModsList.Initialize();

            firstLoad = _firstLoad;
            _firstLoad = true;

            root = new()
            {
                Width = { Pixels = MathF.Max(DataConfigBrowser.Instance.RootSize[0], 400) },
                Height = { Pixels = MathF.Max(DataConfigBrowser.Instance.RootSize[1], 350) },
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

            filtersHorizontalOut = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Pixels = 32 },
                Top = { Pixels = 100 },
            };
            filtersHorizontalOut.SetPadding(0);
            root.Append(filtersHorizontalOut);

            filtersHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            filtersHorizontalOut.Append(filtersHorizontal);
            {
                int i = -1;
                foreach (var item in new string[] { "SF_EveryMod", "SF_MyMods", "SF_Custom" })
                {
                    i++;
                    var p = new UIPanelStyled()
                    {
                        Width = { Precent = 1f / 3f },
                        Left = { Precent = i / 3f },
                        Height = { Precent = 1 },
                    }; p.SetPadding(8);
                    p.Append(new UITextDots<LocalizedText>()
                    {
                        Width = { Precent = 1 },
                        Height = { Precent = 1 },
                        Top = { Pixels = 4 },
                        text = ModManager.Get(item),
                        align = 0.5f
                    }); int j = i;
                    p.OnUpdate += delegate {
                        p.BackgroundColor = currentFiltersSet == j ? UIColors.ColorBackgroundSelected : (j == 2 ? UIColors.ColorBackgroundDisabled : (p.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic));
                        p.BorderColor = currentFiltersSet == j ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
                    };
                    p.OnMouseOver += delegate { Terraria.Audio.SoundEngine.PlaySound(in Terraria.ID.SoundID.MenuTick); };
                    if (j != 2) p.OnLeftClick += delegate { SetFiltersSet(j); };
                    filtersHorizontal.Append(p);
                }
            }

            categoriesHorizontalOut = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Pixels = 32 },
                Top = { Pixels = 132 },
            };
            categoriesHorizontalOut.SetPadding(0);
            root.Append(categoriesHorizontalOut);

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
                Height = { Precent = 1, Pixels = -108 },
                MarginRight = 12,
                Top = { Pixels = 108 }
            }.WithView(1000, 1000);
            mainList = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Precent = 1, Pixels = -164 },
                Top = { Pixels = 164 },
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

            root.Append(mainScrollbar);
            root.Append(mainList);

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
                    Width = { Precent = 0.3f },
                    Height = { Precent = 1 },
                    PaddingTop = PaddingBottom = PaddingLeft = PaddingRight = 8
                };
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
            void NeedUpdate() {
                if (_specialModPackFilter == null) needUpdate = true;
                else UpdateDisplayed();
            }
            {
                var ontopFilters = new UIPanelStyled()
                {
                    Width = { Precent = 0.7f },
                    Left = { Precent = 0.3f },
                    Height = { Precent = 1 },
                }; ontopFilters.SetPadding(0);
                var textBoxOut = new UIPanelStyled()
                {
                    VAlign = 1,
                    Width = { Precent = 2f / 3f },
                    Height = { Pixels = 28 }
                }; textBoxOut.SetPadding(0);
                FilterTextBox = new UIInputTextFieldPriority<LocalizedText>(Language.GetText("tModLoader.ModsTypeToSearch"), 0)
                {
                    Width = { Precent = 1, Pixels = -16 },
                    Height = { Precent = 1 },
                    Left = { Pixels = 8 },
                    Top = { Pixels = 4 }
                };
                FilterTextBox.OnTextChange += delegate { NeedUpdate(); };
                textBoxOut.Append(FilterTextBox);
                ontopFilters.Append(textBoxOut);

                var fSort = new UISwitchingButton<ModBrowserSortMode>(ModManager.Get("BF_Sort"), SortModeFilterToggle)
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                };
                fSort.OnChange += NeedUpdate;
                ontopFilters.Append(fSort);

                var fPeriod = new UISwitchingButton<ModBrowserTimePeriod>(ModManager.Get("BF_Period"), TimePeriodToggle)
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                    Left = { Precent = 1f / 3f }
                };
                fPeriod.OnChange += NeedUpdate;
                ontopFilters.Append(fPeriod);

                var fUpdate = new UISwitchingButton<UpdateFilter>(ModManager.Get("BF_Update"), UpdateFilterToggle)
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                    Left = { Precent = 2f / 3f }
                };
                fUpdate.OnChange += NeedUpdate;
                ontopFilters.Append(fUpdate);

                var fSearch = new UISwitchingButton<SearchFilter>(ModManager.Get("BF_Search"), SearchFilterToggle)
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                    Top = { Precent = 0.5f, Pixels = -14 }
                };
                fSearch.OnChange += NeedUpdate;
                ontopFilters.Append(fSearch);

                var fSide = new UISwitchingButton<ModSideFilter>(ModManager.Get("BF_Side"), ModSideFilterToggle)
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                    Top = { Precent = 0.5f, Pixels = -14 },
                    Left = { Precent = 1f / 3f }
                };
                fSide.OnChange += NeedUpdate;
                ontopFilters.Append(fSide);

                var fUpdateAll = new UIPanelStyled()
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Pixels = 28 },
                    VAlign = 1,
                    HAlign = 1,
                };
                fUpdateAll.Append(new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 1, Pixels = -16 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    Left = { Pixels = 8 },
                    text = Language.GetText("tModLoader.MBUpdateAll"), align = 0.5f
                });
                fUpdateAll.OnLeftClick += UpdateAllMods;
                fUpdateAll.OnUpdate += delegate
                {
                    fUpdateAll.IgnoresMouseInteraction = WorkshopHelpMePlease.ModsRequireUpdates.Count == 0;
                    fUpdateAll.BackgroundColor = fUpdateAll.IgnoresMouseInteraction ? UIColors.ColorBackgroundDisabled : (IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
                };
                ontopFilters.Append(fUpdateAll);

                var fLanguage = new UIPanelStyled()
                {
                    Width = { Precent = 1f / 3f },
                    Height = { Precent = 0.5f, Pixels = -14 },
                    Top = { Precent = 0.5f, Pixels = -14 },
                    Left = { Precent = 2f / 3f }
                }.FadedMouseOver(); fLanguage.SetPadding(0);
                fLanguage.Append(new UITextDots<LocalizedText>()
                {
                    Width = { Precent = 1, Pixels = -16 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    Left = { Pixels = 8 },
                    text = ModManager.Get("BF_Language"), align = 0.5f
                });
                fLanguage.OnLeftClick += delegate
                {
                    if (root.HasChild(modTagFilterDropdown)) root.RemoveChild(modTagFilterDropdown);
                    else root.Append(modTagFilterDropdown);
                };
                fLanguage.OnUpdate += delegate
                {
                    fLanguage.BorderColor = fLanguage.IsMouseHovering || modTagFilterDropdown.Parent != null ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
                };
                ontopFilters.Append(fLanguage);
                topPanel.Append(ontopFilters);

                modTagFilterDropdown.BackgroundColor = Color.Transparent;
                modTagFilterDropdown.BorderColor = Color.Transparent;
                modTagFilterDropdown.Left.Pixels += 8;
                modTagFilterDropdown.Top.Pixels += 16;
                modTagFilterDropdown.Elements[0].HAlign = 1;
                if (modTagFilterDropdown.Elements[0] is UIPanel pan1)
                {
                    pan1.BackgroundColor = UIColors.ColorBackgroundStatic * 2f;
                    pan1.BorderColor = UIColors.ColorBorderStatic * 2f;
                }
                modTagFilterDropdown.OnClickingTag -= delegate { UpdateNeeded = true; };
                modTagFilterDropdown.OnClickingTag += delegate { needUpdate = true; };
                foreach (var item in modTagFilterDropdown.Elements[0].Elements)
                {
                    if (item is UIPanel pan)
                    {
                        pan.BackgroundColor = UIColors.ColorBackgroundStatic * 1.5f;
                        pan.BorderColor = UIColors.ColorBorderStatic * 1.5f;
                    }
                    if (item is GroupOptionButton<int> opt)
                    {
                        opt._UseOverrideColors = false;
                        opt._opacity = 1;
                        opt._whiteLerp = 0;
                        opt.OnUpdate += delegate
                        {
                            var s = opt.IsSelected;
                            opt._hovered = true;
                            opt._color = s ? UIColors.ColorBackgroundSelected : (opt.IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic);
                            opt._borderColor = s ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
                        };
                    }
                }
            }

            AddCategories();
            UpdateDisplayed();
        }
        public void SetFiltersSet(int f)
        {
            if (f == 1)
            {
                SetFiltersMods(ModOrganizer.FindWorkshopMods().ToArray().Select(mod =>
                {
                    WorkshopBrowserModule.Instance.GetModIdFromLocalFiles(mod.modFile, out var item);
                    return item;
                }).ToList());
            }
            else if (f == 0)
            {
                _specialModPackFilter = null;
            }
            currentFiltersSet = f;
            needUpdate = true;
        }
        public void SetFiltersMods(List<ModPubId_t> ids)
        {
            _specialModPackFilter = ids;
            currentFiltersSet = 2;
            needUpdate = true;
        }
        public override void OnActivate()
        {
            UIModDownloadItem.ModIconDownloadFailCount = 0;
            if (firstLoad)
            {
                SocialBackend.Initialize();
                _specialModPackFilter = null;
                _specialModPackFilterTitle = null;
                Populate();
                firstLoad = false;
            }
            WorkshopHelpMePlease.FindHasModUpdates();
        }
        public void Populate()
        {
            UIModDownloadItem.ModIconDownloadFailCount = 0;
            TimePeriodToggle.Disabled = SortMode != ModBrowserSortMode.Hot || !string.IsNullOrEmpty(Filter);
            ModsList.Clear();
            ModsList.Items.Clear();
            mainListIn.Elements.Clear();
            _specialModPackFilterTitle = null;
            ModsList.SetEnumerable(SocialBackend.QueryBrowser(FilterParameters));
        }
        public override void Update(GameTime gameTime)
        {
            if (needUpdate)
            {
                Populate();
                needUpdate = false;
            }
            ModsList.Update(gameTime);
            if (ModsList.NeedUpdate)
            {
                if (ModsList.Items.Count >= 750) ModsList.Cancel();
                UpdateDisplayed();
                ModsList.NeedUpdate = false;
            }
            foreach (UIElement element in Elements)
            {
                element.Update(gameTime);
            }
            var p = categoriesHorizontal.Elements[3].Width.Pixels;
            categoriesHorizontal.Elements[3].Width.Pixels = 80 * scale * 0.8f;
            if (p != categoriesHorizontal.Elements[3].Width.Pixels) Redesign();
            var res = (categoriesHorizontal._innerDimensions.Width - categoriesHorizontal.TotalSize) / 3f;
            if (res != 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    categoriesHorizontal.Elements[i].Width.Pixels += res;
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
            var cfg = DataConfigBrowser.Instance;

            cfg.RootSize[0] = (int)root.Width.Pixels;
            cfg.RootSize[1] = (int)root.Height.Pixels;

            for (int i = 1; i < Categories.Count - 1; i++)
            {
                cfg.CategoriesSizes[i - 1] = (int)categoriesHorizontal.Elements[i].Width.Pixels;
            }

            cfg.Scale = scale;
            cfg.ScaleGrid = scaleGrid;
            cfg.ScaleText = scaleText;

            cfg.Save();
        }
        public bool IsModFiltered(UIModBrowserItemNew item)
        {
            if (!item._isInitialized) item.Activate();
            var s = Filter.ToLower();
            if (s.Length == 0) return true;
            return (item.textName.text != null && item.textName.text.ToLower().Contains(s)) || (item.textAuthor.text != null && item.textAuthor.text.ToLower().Contains(s));
        }
        public void UpdateDisplayed()
        {
            mainListIn.Elements.Clear();
            foreach (var item in ModsList.Items)
            {
                if (_specialModPackFilter != null && !IsModFiltered(item as UIModBrowserItemNew)) continue;
                mainListIn.Append(item);
                item.Activate();
            }
            mainListIn.Recalculate();
            Redesign();
        }
        public void Redesign()
        {
            var pos = Vector2.Zero;
            var c = mainList.GetInnerDimensions();
            float addGridHeight = 0;
            foreach (var item in mainListIn.Elements)
            {
                var mod = item as UIModBrowserItemNew;
                mod.Redesign();
                if (scaleGrid)
                {
                    float add = 1f / (int)(c.Width / mod.GetOuterDimensions().Width);
                    if (pos.X + mod.GetOuterDimensions().Width / c.Width > 1)
                    {
                        pos.Y += mod.GetOuterDimensions().Height;
                        mod.Left.Precent = 0;
                        pos.X = add;
                        addGridHeight = 0;
                    }
                    else
                    {
                        mod.Left.Precent = pos.X;
                        pos.X += add;
                        addGridHeight = mod.GetOuterDimensions().Height;
                    }
                }
                else mod.Left.Precent = 0;
                mod.Left.Pixels = 0;
                mod.Top.Pixels = pos.Y;
                if (!scaleGrid) pos.Y += mod.GetOuterDimensions().Height;
            }
            if (scaleGrid) pos.Y += addGridHeight;
            pos.Y = MathF.Max(pos.Y, c.Height);
            mainListIn.Height.Pixels = pos.Y;
            mainList.Recalculate();
            mainScrollbar.SetView(c.Height, pos.Y);
        }
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
                    UseLeft = j != 1 && j != 4,
                    UseRight = j < 3
                };
                i.OnUpdate += delegate
                {
                    i.BorderColor = i.IsMouseHovering ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
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
                if (j != 4) i.Width.Pixels = DataConfigBrowser.Instance.CategoriesSizes[j - 1];
                else i.Width.Set(100, 0);
                i.OnResizing = Redesign;
                categoriesHorizontal.Append(i);
            }
            for (int i = 0; i < Categories.Count - 1; i++)
            {
                var item = categoriesHorizontal.Elements[i] as UIPanelSizeable;
                for (int j = 0; j < i; j++)
                {
                    item.LeftElem.Add(categoriesHorizontal.Elements[j]);
                }
                for (int j = i + 1; j < Categories.Count; j++)
                {
                    item.RightElem.Add(categoriesHorizontal.Elements[j-1]);
                }
            }
            categoriesHorizontal.Activate();
        }
    }
}
