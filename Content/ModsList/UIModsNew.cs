using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content.ModsList
{
    public class UIModsNew : UIMods
    {
        public static UIModsNew Instance => Interface.modsMenu as UIModsNew;

        public float scaleThreshold = DataConfig.Instance.ScaleThreshold;
        public float scale = DataConfig.Instance.Scale;
        public float scaleText = DataConfig.Instance.ScaleText;

        public List<string> OpenedPath = new();
        public bool OpenedCollections;

        public UIPanelSizeable root;
        public UIModsContextMenu contextMenu;
        public UIModsPopupRename popupRename;
        public UIModsPopupSureDelete popupSureDelete;

        public UIContentList rootVertical;
        public UIContentList rootHorizontal;

        public UIPanel topPanel;
        public UIPanelSizeable collections;

        public UIContentList mainVertical;

        public UIPanel pathHorizontalOut;
        public UIContentList pathHorizontal;

        public UIPanel searchFieldOut;
        public UIInputTextFieldPriority<LocalizedText> searchField;

        public UIList mainList;
        public UIElement mainListIn;
        public UIScrollbar mainScrollbar;

        public UIList collecList;
        public UIContentList collecListIn;
        public UIScrollbar collecScrollbar;

        public UIList configCollecList;
        public UIContentList configCollecListIn;
        public UIScrollbar configCollecScrollbar;

        public UIPanel categoriesHorizontalOut;
        public UIContentList categoriesHorizontal;

        public Task<List<UIModItemNew>> ReloadTask;

        public List<UIModItemNew> uIMods = new();

        public List<UIModItemNew> SelectedItems = new();
        public UIModItemNew SelectedItem => SelectedItems.Count == 1 ? SelectedItems[0] : null;
        public bool GrabbedItem = false;
        public UIModsCollection SelectedCollection = null;
        public string GrabbedFolder = null;

        public UIImage LoadingImage;

        public UIPanel CantLeave;

        public UIPanel BottomCounter;

        public int ModsChangedEnable;
        public int ModsChangedDisable;
        public int ModsChangedConfig;

        public string Tooltip;

        public bool NeedUpdate;

        public static readonly IReadOnlyList<string> Categories = new List<string>()
        {
            "", "Name", "Author", "Version", "Flags"
        }; public static readonly IReadOnlyList<string> Filters = new List<string> { "▼", "▲", "⎯" };

        public int FilterCategory = 1;
        public int FilterCategoryType = 0;
        public bool waitForReload;
        public void ReloadModsTask()
        {
            if (_cts == null) return;
            if (ReloadTask != null && !_cts.IsCancellationRequested)
            {
                OnDeactivate();
                waitForReload = true;
                return;
            }
            mainListIn.Elements.Clear();
            LoadingImage.Color = Color.White;
            ReloadTask = Task.Run(delegate
            {
                List<UIModItemNew> list = new List<UIModItemNew>();
                LocalMod[] array = ModOrganizer.FindMods(true);
                for (int i = 0; i < array.Length; i++)
                {
                    UIModItemNew item = new UIModItemNew(array[i]);
                    list.Add(item);
                }
                return list;
            }, _cts.Token);
        }
        public void ActRename(string value)
        {
            if (SelectedItem == null) return;
            if (SelectedItem.mod != null) DataConfig.Instance.ModNames[SelectedItem.mod.Name] = value;
            else
            {
                var name = SelectedItem.Name;
                var path = string.Join("/", OpenedPath) + "/" + name; if (!path.StartsWith("/")) path = "/" + path;
                var new_path = string.Join("/", OpenedPath) + "/" + value; if (!new_path.StartsWith("/")) new_path = "/" + new_path;
                DataConfig.Instance.Folders.Remove(path);
                var pairs = DataConfig.Instance.ModPaths.ToList();
                foreach (var item in pairs)
                {
                    if (item.Value == path) DataConfig.Instance.ModPaths[item.Key] = new_path;
                    else if (item.Value.StartsWith(path)) DataConfig.Instance.ModPaths[item.Key] = (new_path + item.Value.Substring(path.Length)).Replace("//", "/");
                }
                var list = DataConfig.Instance.Folders.ToList();
                foreach (var item in list)
                {
                    if (item.StartsWith(path) && item.Substring(path.Length).Contains("/"))
                    {
                        DataConfig.Instance.Folders.Remove(item);
                        DataConfig.Instance.Folders.Add((new_path + item.Substring(path.Length)).Replace("//", "/"));
                    }
                }
                DataConfig.Instance.Folders.Add(new_path);
            }
            DataConfig.Instance.Save();
            UpdateDisplayed();
        }
        public void ActDelete()
        {
            if (SelectedItem == null) return;
            if (SelectedItem.mod != null)
            {
                uIMods.Remove(SelectedItem);
                ModOrganizer.DeleteMod(SelectedItem.mod);
            }
            else
            {
                var name = SelectedItem.Name;
                var path = string.Join("/", OpenedPath) + "/" + name; if (!path.StartsWith("/")) path = "/" + path;
                var new_path = string.Join("/", OpenedPath); if (!new_path.StartsWith("/")) new_path = "/" + new_path;
                DataConfig.Instance.Folders.Remove(path);
                var pairs = DataConfig.Instance.ModPaths.ToList();
                foreach (var item in pairs)
                {
                    if (item.Value == path) DataConfig.Instance.ModPaths[item.Key] = new_path;
                    else if (item.Value.StartsWith(path)) DataConfig.Instance.ModPaths[item.Key] = (new_path + item.Value.Substring(path.Length)).Replace("//", "/");
                }
                var list = DataConfig.Instance.Folders.ToList();
                foreach (var item in list)
                {
                    if (item.StartsWith(path) && item.Substring(path.Length).Contains("/"))
                    {
                        DataConfig.Instance.Folders.Remove(item);
                        DataConfig.Instance.Folders.Add((new_path + item.Substring(path.Length)).Replace("//", "/"));
                    }
                }
            }
            DataConfig.Instance.Save();
            UpdateDisplayed();
        }
        public class UIOnRecalculate : UIElement
        {
            public Action OnRecalculate;
            public override void Recalculate()
            {
                OnRecalculate();
            }
        }
        public override void OnInitialize()
        {
            var colors = ManagerConfigColors.Instance;
            
            buttonOMF = new(LocalizedText.Empty); // Prevents chashing on "Draw()" method

            Elements.Clear();

            Append(new UIMMTopPanel());

            root = new UIPanelSizeable()
            {
                Width = { Pixels = DataConfig.Instance.RootSize[0] },
                Height = { Pixels = DataConfig.Instance.RootSize[1] },
                UseLeft = true,
                UseRight = true,
                UseUp = true,
                UseDown = true,
                HAlign = 0.5f,
                VAlign = 0.5f,
                centered = true,
                MinHorizontal = 500,
                MinVertical = 400,
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            root.OnUpdate += (g) => { root.UseLeft = true; };
            root.OnResizing = RedesignUIMods;
            root.SetPadding(0);
            Append(root);

            rootVertical = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IsVertical = true
            };
            root.Append(rootVertical);

            topPanel = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 100 },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            topPanel.SetPadding(8);
            rootVertical.Append(topPanel);

            rootHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1, Pixels = -100 },
            };
            rootVertical.Append(rootHorizontal);

            collections = new()
            {
                Width = { Pixels = DataConfig.Instance.CollectionsSize },
                Height = { Precent = 1 },
                UseRight = true,
                MinHorizontal = 100,
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            collections.OnResizing = RedesignUIMods;
            collections.SetPadding(0);
            rootHorizontal.Append(collections);

            mainVertical = new()
            {
                Width = { Percent = 1, Pixels = -DataConfig.Instance.CollectionsSize },
                Height = { Precent = 1 },
                IsVertical = true
            };
            rootHorizontal.Append(mainVertical);

            collections.RightElem.Add(mainVertical);

            pathHorizontalOut = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            pathHorizontalOut.SetPadding(0);
            mainVertical.Append(pathHorizontalOut);

            pathHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            pathHorizontalOut.Append(pathHorizontal);

            searchFieldOut = new()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            searchFieldOut.SetPadding(0);
            searchField = new(ModManager.Get("SearchModsHint"), 0)
            {
                Top = { Pixels = 5f },
                Height = { Percent = 1f },
                Width = { Percent = 1f },
                Left = { Pixels = 5f },
                VAlign = 0.5f
            };
            searchField.OnTextChange += (v, e) =>
            {
                UpdateDisplayed();
            };
            searchFieldOut.Append(searchField);
            mainVertical.Append(searchFieldOut);

            contextMenu = new();
            contextMenu.OnDraw = DrawGrabbedMod;
            Append(contextMenu);

            popupRename = new();
            popupRename.OnApply = ActRename;
            Append(popupRename);

            popupSureDelete = new();
            popupSureDelete.OnApply = ActDelete;
            Append(popupSureDelete);

            root.OnRightClick += Root_OnRightClick;

            collecScrollbar = new UIScrollbar()
            {
                HAlign = 1,
                Height = { Precent = 0.5f, Pixels = -16 },
                MarginTop = MarginBottom = 0,
                MarginRight = 12,
                Top = { Pixels = 8 }
            }.WithView(100, 1000);
            var elem231 = new UIPanel()
            {
                Width = { Precent = 1, Pixels = -30 },
                Height = { Precent = 0.5f },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            elem231.SetPadding(0);
            collecList = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true,
            };
            collecListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IsVertical = true
            };
            collecList.Append(collecListIn);
            collections.Append(collecScrollbar);
            elem231.Append(collecList);
            collections.Append(elem231);
            collecList.OnScrollWheel += (e, l) => { collecScrollbar.ViewPosition -= e.ScrollWheelValue; };
            collecListIn.OnUpdate += (e) => { collecListIn.Top.Pixels = -collecScrollbar.ViewPosition; };

            configCollecScrollbar = new UIScrollbar()
            {
                VAlign = 1,
                HAlign = 1,
                Height = { Precent = 0.5f, Pixels = -16 },
                MarginTop = MarginBottom = 0,
                MarginRight = 12,
                Top = { Pixels = -8 }
            }.WithView(100, 1000);
            var elem232 = new UIPanel()
            {
                Width = { Precent = 1, Pixels = -30 },
                Height = { Precent = 0.5f },
                VAlign = 1,
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            elem232.SetPadding(0);
            configCollecList = new()
            {
                Width = { Precent = 1f },
                Height = { Precent = 1f },
                OverflowHidden = true
            };
            configCollecListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IsVertical = true
            };
            configCollecList.Append(configCollecListIn);
            collections.Append(configCollecScrollbar);
            elem232.Append(configCollecList);
            collections.Append(elem232);
            configCollecList.OnScrollWheel += (e, l) => { collecScrollbar.ViewPosition -= e.ScrollWheelValue; };
            configCollecListIn.OnUpdate += (e) => { collecListIn.Top.Pixels = -collecScrollbar.ViewPosition; };


            categoriesHorizontalOut = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Pixels = 32 },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            categoriesHorizontalOut.SetPadding(0);
            mainVertical.Append(categoriesHorizontalOut);

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
                Height = { Precent = 1, Pixels = 80 },
                MarginRight = 12
            }.WithView(100, 1000);
            var onRec = new UIOnRecalculate();
            onRec.OnRecalculate = () =>
            {
                mainScrollbar._outerDimensions.Y -= 26;
                mainScrollbar._innerDimensions.Y -= 26;
                mainScrollbar._dimensions.Y -= 26;
                mainScrollbar._outerDimensions.Height += 18;
                mainScrollbar._innerDimensions.Height += 18;
                mainScrollbar._dimensions.Height += 18;
            };
            mainScrollbar.Append(onRec);
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
            mainList.Append(mainListIn);
            mainList.OnScrollWheel += (e, l) => { mainScrollbar.ViewPosition -= e.ScrollWheelValue; };
            mainListIn.OnUpdate += (e) => { mainListIn.Top.Pixels = -mainScrollbar.ViewPosition; };
            {
                var elem = new UIElement()
                {
                    Width = { Precent = 1 },
                    Height = { Precent = 1, Pixels = -128 },
                };
                LoadingImage = new UIImage(ModManager.AssetLoading)
                {
                    Left = { Pixels = -50, Precent = 0.5f },
                    Top = { Pixels = -50, Precent = 0.5f },
                    NormalizedOrigin = Vector2.One * 0.5f,
                    OverrideSamplerState = SamplerState.LinearClamp
                };
                elem.Append(mainScrollbar);
                elem.Append(mainList);
                elem.Append(LoadingImage);
                mainVertical.Append(elem);
            }
            BottomCounter = new UIPanel()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                BackgroundColor = colors.ColorBackgroundStatic,
                BorderColor = colors.ColorBorderStatic
            };
            BottomCounter.SetPadding(0);
            BottomCounter.PaddingLeft = BottomCounter.PaddingRight = 24;
            mainVertical.Append(BottomCounter);
            {
                var numElements = new UITextDots<string>()
                {
                    Width = { Percent = 0.25f },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                BottomCounter.Append(numElements);
                var numSelected = new UITextDots<string>()
                {
                    Width = { Percent = 0.25f },
                    Left = { Percent = 0.25f },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                BottomCounter.Append(numSelected);
                var numEnabled = new UITextDots<string>()
                {
                    Width = { Percent = 0.25f },
                    Left = { Percent = 0.5f },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                BottomCounter.Append(numEnabled);
                var numUpdateStatus = new UITextDots<string>()
                {
                    Width = { Percent = 0.25f },
                    Left = { Percent = 0.75f },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                numUpdateStatus.OnUpdate += delegate
                {
                    if (!WorkshopHelpMePlease.ModsRequireUpdatesLoading && numUpdateStatus._dimensions.ToRectangle().Contains(Main.mouseX, Main.mouseY) && WorkshopHelpMePlease.ModsRequireUpdates.Count != 0)
                    {
                        UICommon.TooltipMouseText(string.Join("\n", WorkshopHelpMePlease.ModsRequireUpdates));
                    }
                };
                BottomCounter.Append(numUpdateStatus);
                void Recalc()
                {
                    var total = 0;
                    var enabl = 0;
                    foreach (var item in uIMods)
                    {
                        if (item.Active) total++;
                        if (item.mod != null && item.mod.Enabled) enabl++;
                    }
                    numElements.text = ModManager.Get("NumberElements").ToString() + ": " + total;
                    numSelected.text = ModManager.Get("NumberSelected").ToString() + ": " + SelectedItems.Count;
                    numEnabled.text = ModManager.Get("NumberEnabled").ToString() + ": " + enabl;
                }
                Recalc();
                void CheckStatus()
                {
                    if (WorkshopHelpMePlease.ModsRequireUpdatesLoading)
                    {
                        numUpdateStatus.text = Language.GetTextValue("Mods.ModManager.UpdateCheckAnalysing");
                        return;
                    }
                    numUpdateStatus.text = WorkshopHelpMePlease.ModsRequireUpdates.Count == 0 ? 
                        Language.GetTextValue("Mods.ModManager.UpdateCheckZero") : 
                        Language.GetTextValue("Mods.ModManager.UpdateCheckNeed", WorkshopHelpMePlease.ModsRequireUpdates.Count);
                }
                CheckStatus();
                WorkshopHelpMePlease.OnCheckedUpdates += CheckStatus;
                ChangeSelection += Recalc;
                CheckChangedCallback += Recalc;
            }
            {
                var ontopSettings = new UIPanel()
                {
                    Width = { Precent = 0.3f },
                    Height = { Precent = 1 },
                    PaddingTop = PaddingBottom = PaddingLeft = PaddingRight = 8,
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                };
                var labelScale = new UITextDots<string>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 6 },
                    Height = { Pixels = 16 },
                    text = "Scale"
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
                var labelTextScale = new UITextDots<string>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 30 },
                    Height = { Pixels = 16 },
                    text = "Text Scale"
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
                var labelThresholdScale = new UITextDots<string>()
                {
                    Width = { Precent = 0.45f },
                    Top = { Pixels = 54 },
                    Height = { Pixels = 16 },
                    text = "Threshold"
                };
                var sliderThresholdScale = new UISliderNew()
                {
                    Width = { Precent = 0.55f },
                    Left = { Precent = 0.45f },
                    Top = { Pixels = 50 },
                    minimum = 1,
                    maximum = 6,
                    value = DataConfig.Instance.ScaleThreshold
                };
                sliderScale.OnChange += () =>
                {
                    scale = sliderScale.value;
                    RedesignUIMods();
                };
                sliderThresholdScale.OnChange += () =>
                {
                    scaleThreshold = sliderThresholdScale.value;
                    RedesignUIMods();
                };
                sliderTextScale.OnChange += () =>
                {
                    scaleText = sliderTextScale.value;
                    RedesignUIMods();
                };
                ontopSettings.Append(labelScale);
                ontopSettings.Append(sliderScale);
                ontopSettings.Append(labelTextScale);
                ontopSettings.Append(sliderTextScale);
                ontopSettings.Append(labelThresholdScale);
                ontopSettings.Append(sliderThresholdScale);
                topPanel.Append(ontopSettings);
            }
            {
                var ontopInfo = new UIPanel()
                {
                    Width = { Precent = 0.3f },
                    Height = { Precent = 1 },
                    Left = { Precent = 0.7f },
                    PaddingTop = PaddingBottom = PaddingLeft = PaddingRight = 8,
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                };
                var icon = new UIImage(ModManager.AssetModIcon)
                {
                    ScaleToFit = true,
                    OverrideSamplerState = SamplerState.PointClamp,
                    Width = { Pixels = 64 },
                    Height = { Pixels = 64 },
                    Left = { Pixels = -4 },
                    VAlign = 0.5f
                };
                var textName = new UITextDots<string>()
                {
                    Width = { Precent = 1f, Pixels = -70 },
                    Left = { Pixels = 65 },
                    Top = { Pixels = 2 },
                    Height = { Pixels = 16 },
                    text = "-",
                    scale = 0.9f
                };
                var textNameInternal = new UITextDots<string>()
                {
                    Width = { Precent = 1f, Pixels = -70 },
                    Left = { Pixels = 65 },
                    Top = { Pixels = 20 },
                    Height = { Pixels = 16 },
                    text = "-",
                    scale = 0.9f
                };
                var textAuthor = new UITextDots<string>()
                {
                    Width = { Precent = 1f, Pixels = -70 },
                    Left = { Pixels = 65 },
                    Top = { Pixels = 38 },
                    Height = { Pixels = 16 },
                    text = "-",
                    scale = 0.9f
                };
                var textVersion = new UITextDots<string>()
                {
                    Width = { Precent = 1f, Pixels = -70 },
                    Left = { Pixels = 65 },
                    Top = { Pixels = 56 },
                    Height = { Pixels = 16 },
                    text = "v0.0.0",
                    scale = 0.9f
                };
                ChangeSelection += () =>
                {
                    textName.text = SelectedItem?.Name ?? "-";
                    textNameInternal.text = SelectedItem?.mod?.Name ?? "-";
                    if (textNameInternal.text != "-") textNameInternal.text += ".tmod";
                    textAuthor.text = "by " + (SelectedItem?.mod?.properties.author ?? "-");
                    textVersion.text = "v" + (SelectedItem?.mod?.Version.ToString() ?? "0.0.0");
                    icon._texture = SelectedItem?.icon._texture ?? ModManager.AssetModIcon;
                };
                ontopInfo.Append(icon);
                ontopInfo.Append(textName);
                ontopInfo.Append(textNameInternal);
                ontopInfo.Append(textAuthor);
                ontopInfo.Append(textVersion);
                topPanel.Append(ontopInfo);
            }
            {
                var ontopButtons = new UIPanel()
                {
                    Left = { Precent = 0.3f },
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1 },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                };
                ontopButtons.SetPadding(0);
                var buttonConfig = new UIPanel()
                {
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1f / 3f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
                buttonConfig.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("ButtonConfig"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonConfig.OnLeftClick += (e, l) =>
                {
                    OnDeactivate();
                    Main.menuMode = Interface.modConfigListID;
                    Interface.modConfig.modderOnClose = () =>
                    {
                        Main.menuMode = Interface.modsMenuID;
                    };
                    Interface.modConfig.openedFromModder = true;
                };
                ontopButtons.Append(buttonConfig);
                var buttonApply = new UIPanel()
                {
                    Width = { Precent = 0.6f },
                    Height = { Precent = 1f / 3f },
                    Left = { Precent = 0.4f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
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
                    var getAct = ModLoader.OnSuccessfulLoad;
                    ModLoader.OnSuccessfulLoad = () =>
                    {
                        ModLoader.OnSuccessfulLoad = getAct;
                        Main.menuMode = Interface.modsMenuID;
                    };
                    ModLoader.Reload();
                };
                buttonApply.OnUpdate += delegate
                {
                    if (buttonApply.IsMouseHovering) Tooltip = ModManager.Get("ButtonApplyChanges").ToString() + ":\n" +
                    ModManager.Get("ChangedEnabled").ToString() + ": " + ModsChangedEnable + "\n" +
                    ModManager.Get("ChangedDisabled").ToString() + ": " + ModsChangedDisable + "\n" +
                    ModManager.Get("ChangedConfigs").ToString() + ": " + ModsChangedConfig;
                };
                ontopButtons.Append(buttonApply);
                var buttonReject = new UIPanel()
                {
                    Width = { Precent = 0.6f },
                    Height = { Precent = 1f / 3f },
                    Left = { Precent = 0.4f },
                    Top = { Precent = 1f / 3f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
                buttonReject.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("ButtonRejectChanges"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonReject.OnLeftClick += (_, _) => RejectChanges();
                buttonReject.OnUpdate += delegate
                {
                    if (buttonReject.IsMouseHovering) Tooltip = ModManager.Get("ButtonRejectChanges").ToString() + ":\n" +
                    ModManager.Get("ChangedEnabled").ToString() + ": " + ModsChangedEnable + "\n" +
                    ModManager.Get("ChangedDisabled").ToString() + ": " + ModsChangedDisable + "\n" +
                    ModManager.Get("ChangedConfigs").ToString() + ": " + ModsChangedConfig;
                };
                ontopButtons.Append(buttonReject);
                var buttonInfo = new UIPanel()
                {
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 1f / 3f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
                buttonInfo.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("ButtonInfo"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 },
                    align = 0.5f
                });
                buttonInfo.OnLeftClick += (e, l) => ShowInfo();
                buttonInfo.IgnoresMouseInteraction = SelectedItem == null || SelectedItem.mod == null;
                buttonInfo.BackgroundColor = buttonInfo.IgnoresMouseInteraction ? colors.ColorBackgroundDisabled : colors.ColorBackgroundStatic;
                ChangeSelection += () =>
                {
                    buttonInfo.IgnoresMouseInteraction = SelectedItem == null || SelectedItem.mod == null;
                    buttonInfo.BackgroundColor = buttonInfo.IgnoresMouseInteraction ? colors.ColorBackgroundDisabled : colors.ColorBackgroundStatic;
                };
                ontopButtons.Append(buttonInfo);

                var buttonEnableAll = new UIPanel()
                {
                    Width = { Precent = 0.5f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 2f / 3f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
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
                    foreach (var item in uIMods)
                    {
                        if (item.mod != null && !ModManager.BadMods.Contains(item.mod.Name)) item.Set(true);
                    }
                };
                ontopButtons.Append(buttonEnableAll);
                var buttonDisableAll = new UIPanel()
                {
                    Width = { Precent = 0.5f },
                    Left = { Precent = 0.5f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 2f / 3f },
                    BackgroundColor = colors.ColorBackgroundStatic,
                    BorderColor = colors.ColorBorderStatic
                }.WithFadedMouseOver(colors.ColorBackgroundHovered, colors.ColorBackgroundStatic, colors.ColorBorderHovered, colors.ColorBorderStatic);
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
                    foreach (var item in uIMods)
                    {
                        item.Set(false);
                    }
                };
                ontopButtons.Append(buttonDisableAll);

                topPanel.Append(ontopButtons);

                CheckChangedCallback += () =>
                {
                    var c = !DoNotClose;
                    var col = c ? colors.ColorBackgroundDisabled : colors.ColorBackgroundStatic;
                    buttonApply.IgnoresMouseInteraction = c;
                    buttonApply.BackgroundColor = col;
                    buttonReject.IgnoresMouseInteraction = c;
                    buttonReject.BackgroundColor = col;
                };
            }
            {
                CantLeave = new()
                {
                    BackgroundColor = UICommon.DefaultUIBlue,
                    Height = { Pixels = 80 },
                    HAlign = 0.5f,
                    Top = { Pixels = -1000000, Precent = 0.5f },
                    IgnoresMouseInteraction = true
                };
                CantLeave.SetPadding(0);

                CantLeave.Append(new UITextDots<LocalizedText>()
                {
                    text = ModManager.Get("CantLeave"),
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    scale = 0.8f,
                    align = 0.5f,
                    Top = { Pixels = 8 },
                    big = true
                });

                //Append(CantLeave);
            }

            AddCategories();
            RecalculatePath();
            AddCollections();
            AddConfigCollections();

            WorkshopHelpMePlease.OnCheckedUpdates += () => { NeedUpdate = true; };
        }
        public Action ChangeSelection;
        public void DrawGrabbedMod()
        {
            if (GrabbedItem)
            {
                float p = 0;
                foreach (var item in SelectedItems)
                {
                    var x = Main.mouseX - item._outerDimensions.Width * 0.5f;
                    item._outerDimensions.X = x;
                    item._outerDimensions.Y = Main.mouseY + p;
                    item._innerDimensions.X = x;
                    item._innerDimensions.Y = Main.mouseY + p;
                    item._dimensions.X = x;
                    item._dimensions.Y = Main.mouseY + p;
                    item.RecalculateChildren();
                    item.CanDraw = true;
                    item.Draw(Main.spriteBatch);
                    p += item._outerDimensions.Height;
                }
            }
            if (Tooltip != null)
            {
                UICommon.TooltipMouseText(Tooltip);
                Tooltip = null;
            }
        }
        private static double StringCompare(string a, string b)
        {
            if (a == b) return 100;
            if (a.Length == 0 || b.Length == 0) return 0;
            double maxLen = a.Length > b.Length ? a.Length : b.Length;
            int minLen = a.Length < b.Length ? a.Length : b.Length;
            int sameCharAtIndex = 0;
            for (int i = 0; i < minLen; i++)
            {
                if (a[i] == b[i])
                {
                    sameCharAtIndex++;
                }
            }
            return sameCharAtIndex / maxLen * 100;
        }
        public bool IsModFiltered(UIModItemNew item)
        {
            var s = searchField._currentString.ToLower();
            if (s.Length == 0 || item.mod == null) return true;
            var n1 = item.Name.ToLower();
            var n2 = item.mod.DisplayNameClean.ToLower();
            return n1.StartsWith(s) || n2.StartsWith(s) || item.mod.properties.author.ToLower().StartsWith(s) || StringCompare(n1, s) >= 35 || StringCompare(n2, s) >= 35 || StringCompare(item.mod.properties.author.ToLower(), s) >= 25;
        }
        public void UpdateDisplayed()
        {
            mainListIn.Elements.Clear();

            var list = uIMods.ToList();

            if (OpenedCollections)
            {
                list.Sort((left, right) => { return left.Sort(right, FilterCategory, FilterCategoryType); });
                foreach (var mod in list)
                {
                    mod.Active = mod.mod != null && (OpenedPath.Count == 0 || DataConfig.Instance.Collections[OpenedPath[0]].Contains(mod.mod.Name)) && IsModFiltered(mod);

                    if (!mod.Active) continue;
                    mainListIn.Append(mod);
                    mod.Activate();
                }
                DataConfig.Instance.Save();
                RedesignUIMods();
                return;
            }
            var path = string.Join("/", OpenedPath) + "/";
            if (!path.StartsWith("/")) path = "/" + path;
            var paths = DataConfig.Instance.ModPaths;
            foreach (var folder in DataConfig.Instance.Folders)
            {
                if (folder.StartsWith(path) && !folder.Substring(path.Length).Contains("/"))
                {
                    list.Insert(0, new UIModItemNew(null) { Name = folder.Substring(path.Length) });
                }
            }
            list.Sort((left, right) => { return left.Sort(right, FilterCategory, FilterCategoryType); });
            foreach (var mod in list)
            {
                if (mod.mod != null && !paths.ContainsKey(mod.mod.Name)) paths[mod.mod.Name] = "/";
                mod.Active = (mod.mod == null || paths[mod.mod.Name] == path) && IsModFiltered(mod);
                if (!mod.Active) continue;
                mainListIn.Append(mod);
                mod.Activate();
            }
            DataConfig.Instance.Save();
            RedesignUIMods();
            AddCollections();
            CheckChanged();
        }
        private void Root_OnRightClick(UIMouseEvent evt, UIElement listeningElement)
        {
            contextMenu.Target = evt.Target;
            contextMenu.Popup();
        }
        public void RecalculatePath()
        {
            pathHorizontal.Elements.Clear();
            pathHorizontal.Append(new UIModsPathPart(OpenedCollections ? "collections/" : "root/", "/"));
            pathHorizontal.Elements[0].OnLeftClick += (m, l) =>
            {
                OpenedPath.Clear();
                OpenedCollections = false;
                RecalculatePath();
                GrabbedItem = false;
                SelectedItems.Clear();
                ChangeSelection();
            };
            var k = 0;
            string path = "/";
            foreach (var item in OpenedPath)
            {
                k++;
                var j = k;
                path += item + "/";
                var p = path;
                var i = new UIModsPathPart(item + "/", p);
                i.OnLeftClick += (m, l) =>
                {
                    if (OpenedCollections)
                    {
                        OpenedPath.Clear();
                        OpenedCollections = false;
                        GrabbedItem = false;
                        SelectedItems.Clear();
                        ChangeSelection();
                    }
                    else while (OpenedPath.Count > j) OpenedPath.RemoveAt(j);
                    RecalculatePath();
                };
                pathHorizontal.Append(i);
            }
            if (OpenedCollections)
            {
                var p = new UIModsPathPart(Language.GetTextValue("Mods.ModManager.ClickToReturn"), "");
                p.OnLeftClick += (m, l) =>
                {
                    OpenedPath.Clear();
                    OpenedCollections = false;
                    RecalculatePath();
                    GrabbedItem = false;
                    SelectedItems.Clear();
                    ChangeSelection();
                };
                p.Text.align = 0.5f;
                p.Width.Pixels += 100;
                var h = new UIElement();
                h.Width.Precent = 1;
                h.Width.Pixels -= pathHorizontal.Elements[0].Width.Pixels;
                if (OpenedPath.Count > 0) h.Width.Pixels -= pathHorizontal.Elements[1].Width.Pixels;
                h.Width.Pixels -= p.Width.Pixels;
                pathHorizontal.Append(h);
                pathHorizontal.Append(p);
            }
            pathHorizontal.Activate();
            UpdateDisplayed();
        }
        public void RedesignUIMods()
        {
            var grid = scale >= scaleThreshold;
            var pos = Vector2.Zero;
            var c = mainList.GetInnerDimensions();
            foreach (var item in mainListIn.Elements)
            {
                var mod = item as UIModItemNew;
                if (!mod.Active) continue;
                mod.Redesign();
                if (grid)
                {
                    float add = 1f / (int)(c.Width / mod.GetOuterDimensions().Width);

                    if (pos.X + mod.GetOuterDimensions().Width / c.Width > 1)
                    {
                        pos.Y += mod.GetOuterDimensions().Height;
                        mod.Left.Precent = 0;
                        pos.X = add;
                    }
                    else
                    {
                        mod.Left.Precent = pos.X;
                        pos.X += add;
                    }
                }
                else mod.Left.Precent = 0;
                mod.Left.Pixels = 0;
                mod.Top.Pixels = pos.Y;
                if (!grid) pos.Y += mod.GetOuterDimensions().Height;
            }
            pos.Y = MathF.Max(pos.Y, mainList.GetInnerDimensions().Height);
            mainListIn.Height.Pixels = pos.Y;
            mainList.Recalculate();
            mainScrollbar.SetView(mainList.GetInnerDimensions().Height, pos.Y);
        }
        public void CheckChanged()
        {
            ModsChangedEnable = 0;
            ModsChangedDisable = 0;
            ModsChangedConfig = 0;
            foreach (var mod in uIMods)
            {
                if (mod.mod != null)
                {
                    if (mod.mod.Enabled && !mod.loaded) ModsChangedEnable++;
                    else if (!mod.mod.Enabled && mod.loaded) ModsChangedDisable++;
                    if (ModLoader.TryGetMod(mod.mod.Name, out var result) && ConfigManager.Configs.ContainsKey(result) && ConfigManager.ModNeedsReload(result))
                        ModsChangedConfig++;
                }
            }
            CheckChangedCallback();
        }
        public Action CheckChangedCallback;
        public void RejectChanges()
        {
            foreach (var mod in uIMods)
            {
                if (mod.mod != null)
                {
                    mod.Set(mod.loaded);
                    if (ModLoader.TryGetMod(mod.mod.Name, out var result) && ConfigManager.Configs.ContainsKey(result) && ConfigManager.ModNeedsReload(result))
                    {
                        var l = ConfigManager.Configs[result];
                        var l2 = ConfigManager.loadTimeConfigs[result];
                        for (int i = 0; i < l.Count; i++)
                        {
                            l[i] = l2[i].Clone();
                        }
                    }
                }
            }
            CheckChanged();
        }
        public void AddConfigCollections()
        {
            configCollecListIn.Elements.Clear();

            configCollecListIn.Append(new UIModsConfigCollection("Your Config") { isAll = true });
            (configCollecListIn.Elements[0] as UIModsConfigCollection).Panel.WithFadedMouseOver();
            (configCollecListIn.Elements[0] as UIModsConfigCollection).Text.align = 0.5f;

            foreach (var item in DataConfig.Instance.ConfigCollections)
            {
                configCollecListIn.Append(new UIModsConfigCollection(item));
            }
            configCollecListIn.Activate();
            configCollecList.Recalculate();
            configCollecScrollbar.SetView(configCollecList.GetInnerDimensions().Height, 0);
        }
        public void AddCollections()
        {
            collecListIn.Elements.Clear();

            collecListIn.Append(new UIModsCollection("All Mods") { isAll = true });

            foreach (var item in DataConfig.Instance.Collections.Keys)
            {
                collecListIn.Append(new UIModsCollection(item));
            }
            collecListIn.Activate();
            collecList.Recalculate();
            collecScrollbar.SetView(collecList.GetInnerDimensions().Height, 0);
        }
        public void AddCategories()
        {
            categoriesHorizontal.Elements.Clear();

            var k = -1;
            foreach (var item in Categories)
            {
                k++;
                var j = k;
                var i = new UIModsTableCategory(item, j - 1);
                if (j > 0 && j < 4) i.Width.Pixels = DataConfig.Instance.CategoriesSizes[j - 1];
                if (j == FilterCategory) i.Filter.text = Filters[FilterCategoryType];
                else i.Filter.text = Filters[2];
                i.OnClick = () =>
                {
                    if (FilterCategory == j)
                    {
                        FilterCategoryType++;
                        FilterCategoryType %= 2;
                    }
                    else
                    {
                        FilterCategory = j;
                        FilterCategoryType = 0;
                    }
                    Save();
                    AddCategories();
                    UpdateDisplayed();
                };
                i.OnResizing = RedesignUIMods;
                categoriesHorizontal.Append(i);
            }
            for (int i = 1; i < Categories.Count; i++)
            {
                var item = categoriesHorizontal.Elements[i] as UIModsTableCategory;
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
        public void ShowInfo()
        {
            if (SelectedItem == null || SelectedItem.mod == null) return;
            SoundEngine.PlaySound(in SoundID.MenuOpen);
            Interface.modInfo.Show(SelectedItem.Name, SelectedItem.mod.DisplayName, 10000, SelectedItem.mod, SelectedItem.mod.properties.description, SelectedItem.mod.properties.homepage);
        }
        private float saveTimer = 0;
        public override void Update(GameTime gameTime)
        {
            if (NeedUpdate)
            {
                UpdateDisplayed();
                NeedUpdate = false;
            }

            UIInputTextFieldPriority.MaxPriority = popupRename.Top.Pixels > -1000 ? 1 : 0;
            if (CantLeaveTimer > 0)
            {
                CantLeaveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (CantLeaveTimer <= 0)
                {
                    CantLeave.Top.Pixels = -1000000;
                    CantLeave.Recalculate();
                }
            }
            {
                var a = (float)gameTime.TotalGameTime.TotalSeconds * 360; a -= a % 22.5f;
                LoadingImage.Rotation = MathHelper.ToRadians(a);
            }
            GrabbedFolder = null;
            if (ReloadTask != null && ReloadTask.IsCompleted)
            {
                uIMods = ReloadTask.Result;
                ReloadTask = null;
                LoadingImage.Color = Color.Transparent;
                if (waitForReload)
                {
                    ReloadModsTask();
                    waitForReload = false;
                }
                else UpdateDisplayed();
            }
            foreach (var item in Elements)
            {
                item.Update(gameTime);
            }

            if (collections != null)
            {
                collections.MaxHorizontal = root.Width.Pixels * 0.4f;
                if (collections.Width.Pixels > collections.MaxHorizontal)
                {
                    collections.Width.Pixels = collections.MaxHorizontal;
                    mainVertical.Width.Pixels = -collections.Width.Pixels;
                }
            }
            saveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (saveTimer <= 0)
            {
                Save();
            }
        }
        public void Save()
        {
            saveTimer = 2.5f;
            var cfg = DataConfig.Instance;

            cfg.RootSize[0] = (int)root.Width.Pixels;
            cfg.RootSize[1] = (int)root.Height.Pixels;

            for (int i = 1; i < Categories.Count; i++)
            {
                cfg.CategoriesSizes[i-1] = (int)categoriesHorizontal.Elements[i].Width.Pixels;
            }
            cfg.CollectionsSize = (int)collections.Width.Pixels;

            cfg.Scale = scale;
            cfg.ScaleThreshold = scaleThreshold;
            cfg.ScaleText = scaleText;

            cfg.Save();
        }
        public override void OnActivate()
        {
            SelectedItems.Clear();
            GrabbedItem = false;

            _cts = new CancellationTokenSource();

            ReloadModsTask();

            WorkshopHelpMePlease.FindHasModUpdates();

            CantLeave.Width.Pixels = 24 + FontAssets.DeathText.Value.MeasureString(Language.GetTextValue("Mods.ModManager.CantLeave")).X * 0.8f;
        }
        public override void OnDeactivate()
        {
            var c = _cts;
            _cts = new CancellationTokenSource();
            c?.Cancel(throwOnFirstException: false);
            c?.Dispose();

            SelectedItems.Clear();
            GrabbedItem = false;
        }
        public bool DoNotClose => ModsChangedConfig != 0 || ModsChangedDisable != 0 || ModsChangedEnable != 0;
        public float CantLeaveTimer = -1;
        public void DoNotCloseCallback()
        {
            CantLeaveTimer = 2;
            CantLeave.Top.Pixels = -40;
            CantLeave.Recalculate();
        }
    }
}
