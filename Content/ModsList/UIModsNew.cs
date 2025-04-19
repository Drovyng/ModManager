using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
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

        public UIPanelStyled topPanel;
        public UIPanelSizeable collections;

        public UIContentList mainVertical;

        public UIPanelStyled pathHorizontalOut;
        public UIContentList pathHorizontal;

        public UIPanelStyled searchFieldOut;
        public UIInputTextFieldPriority<LocalizedText> searchField;

        public UIPanelStyled categoryRecent;
        public UITextDots<string> categoryRecentText;

        public UIList mainList;
        public UIDoNotDrawNonArea mainListIn;
        public UIScrollbar mainScrollbar;

        public UIList collecList;
        public UIContentList collecListIn;
        public UIScrollbar collecScrollbar;

        public UIList configCollecList;
        public UIContentList configCollecListIn;
        public UIScrollbar configCollecScrollbar;

        public UIPanelStyled categoriesHorizontalOut;
        public UIContentList categoriesHorizontal;

        public Task<List<UIModItemNew>> ReloadTask;

        public List<UIModItemNew> uIMods = new();

        public List<UIModItemNew> SelectedItems = new();
        public UIModItemNew SelectedItem => SelectedItems.Count == 1 ? SelectedItems[0] : null;
        public bool GrabbedItem = false;
        public UIModsCollection SelectedCollection = null;
        public string GrabbedFolder = null;

        public UIImage LoadingImage;

        public UIPanelStyled CantLeave;

        public UIPanelStyled BottomCounter;

        public int ModsChangedEnable;
        public int ModsChangedDisable;
        public int ModsChangedConfig;

        public string Tooltip;

        public bool NeedUpdate;

        public static readonly IReadOnlyList<string> Categories = new List<string>()
        {
            "", "Name", "Author", "Version", "Flags"
        }; public static readonly IReadOnlyList<string> Filters = new List<string> { "▼", "▲", "⎯" };

        public enum UndoRedoEnum : byte
        {
            EnableMods,
            DisableMods,
            RenameMod,
            RenameFolder,
            Move,
            ChangedFolders
        }
        public List<(UndoRedoEnum, object)> ToUndo = new();
        public List<(UndoRedoEnum, object)> ToRedo = new();

        public int FilterCategory = DataConfig.Instance.FilterCategory;
        public int FilterCategoryType = DataConfig.Instance.FilterCategoryType;
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
                try
                {
                    List<UIModItemNew> list = new List<UIModItemNew>();
                    LocalMod[] array = ModOrganizer.FindMods(true);
                    for (int i = 0; i < array.Length; i++)
                    {
                        UIModItemNew item = new UIModItemNew(array[i]);
                        list.Add(item);
                    }
                    return list;
                }
                catch (Exception)
                {
                    return null;
                }
            }, _cts.Token);
        }
        public void ActRename(string value)
        {
            if (SelectedItem == null) return;
            if (SelectedItem.mod != null)
            {
                ToRedo.Clear();
                ToUndo.Add((UndoRedoEnum.RenameMod, (SelectedItem.mod.Name, DataConfig.Instance.ModNames[SelectedItem.mod.Name])));
                DataConfig.Instance.ModNames[SelectedItem.mod.Name] = value;
            }
            else
            {
                var name = SelectedItem.Name;
                var path = string.Join("/", OpenedPath) + "/" + name; if (!path.StartsWith("/")) path = "/" + path;
                var new_path = string.Join("/", OpenedPath) + "/" + value; if (!new_path.StartsWith("/")) new_path = "/" + new_path;
                ActRenameFolder(path, new_path);
                ToRedo.Clear();
                ToUndo.Add((UndoRedoEnum.RenameFolder, (new_path, path)));
            }
            DataConfig.Instance.Save();
            UpdateDisplayed();
        }
        public void ActRenameFolder(string path, string new_path)
        {
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
            buttonOMF = new(LocalizedText.Empty); // Prevents chashing on "Draw()" method

            Elements.Clear();

            Append(new UIAlphaFixer());

            Append(new UIMMTopPanel());

            root = new UIPanelSizeable()
            {
                Width = { Pixels = MathF.Max(DataConfig.Instance.RootSize[0], 500) },
                Height = { Pixels = MathF.Max(DataConfig.Instance.RootSize[1], 400) },
                UseLeft = true,
                UseRight = true,
                UseUp = true,
                UseDown = true,
                HAlign = 0.5f,
                VAlign = 0.5f,
                centered = true,
                MinHorizontal = 500,
                MinVertical = 400,
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
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
                Height = { Pixels = 100 }
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
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
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
                Height = { Pixels = 32 }
            };
            pathHorizontalOut.SetPadding(0);
            mainVertical.Append(pathHorizontalOut);

            pathHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            pathHorizontal.Append(new UIAlphaFixer());
            pathHorizontalOut.Append(pathHorizontal);
            pathHorizontalOut.Append(new UIAlphaFixer());

            var el = new UIElement()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 }
            };
            searchFieldOut = new()
            {
                Width = { Precent = 0.75f },
                Left = { Precent = 0.25f },
                Height = { Pixels = 32 }
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

            categoryRecent = new UIPanelStyled()
            {
                Width = { Precent = 0.25f },
                Height = { Pixels = 32 }
            }; categoryRecent.SetPadding(0); categoryRecent.FadedMouseOver();
            categoryRecent.Append(new UITextDots<LocalizedText>()
            {
                Width = { Precent = 1f, Pixels = -32 },
                Height = { Precent = 1f },
                Left = { Pixels = 6 },
                Top = { Pixels = 4 },
                align = 0.5f,
                text = ModManager.Get("Cat_Recent"),
            });
            categoryRecent.OnLeftClick += delegate
            {
                if (FilterCategory == -5)
                {
                    FilterCategoryType++;
                    FilterCategoryType %= 2;
                }
                else
                {
                    FilterCategory = -5;
                    FilterCategoryType = 0;
                }
                Save();
                AddCategories();
                Update(new());
                Update(new());
                UpdateDisplayed();
                RedesignUIMods();
            };
            categoryRecentText = new UITextDots<string>()
            {
                Width = { Precent = 1f, Pixels = -8 },
                Height = { Precent = 1f },
                Top = { Pixels = 4 },
                color = Color.Black,
                align = 1f,
                scale = 1f,
                text = FilterCategory == -5 ? Filters[FilterCategoryType] : Filters[2]
            };
            categoryRecent.Append(categoryRecentText);
            el.Append(categoryRecent);
            el.Append(searchFieldOut);
            mainVertical.Append(el);

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
            var elem231 = new UIPanelStyled()
            {
                Width = { Precent = 1, Pixels = -30 },
                Height = { Precent = 0.5f },
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
            };
            elem231.SetPadding(0);
            collecList = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true,
            };
            collecList.Append(new UIAlphaFixer());
            collecListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IsVertical = true
            };
            collecList.Append(collecListIn);
            collections.Append(collecScrollbar);
            elem231.Append(collecList);
            elem231.Append(new UIAlphaFixer());
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
            var elem232 = new UIPanelStyled()
            {
                Width = { Precent = 1, Pixels = -30 },
                Height = { Precent = 0.5f },
                VAlign = 1,
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
            };
            elem232.SetPadding(0);
            configCollecList = new()
            {
                Width = { Precent = 1f },
                Height = { Precent = 1f },
                OverflowHidden = true
            };
            configCollecList.Append(new UIAlphaFixer());
            configCollecListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                IsVertical = true
            };
            configCollecList.Append(configCollecListIn);
            collections.Append(configCollecScrollbar);
            elem232.Append(configCollecList);
            elem232.Append(new UIAlphaFixer());
            collections.Append(elem232);
            configCollecList.OnScrollWheel += (e, l) => { collecScrollbar.ViewPosition -= e.ScrollWheelValue; };
            configCollecListIn.OnUpdate += (e) => { collecListIn.Top.Pixels = -collecScrollbar.ViewPosition; };


            categoriesHorizontalOut = new()
            {
                Width = { Precent = 1, Pixels = -32 },
                Height = { Pixels = 32 },
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
            };
            categoriesHorizontalOut.SetPadding(0);
            mainVertical.Append(categoriesHorizontalOut);

            categoriesHorizontal = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 },
                OverflowHidden = true
            };
            categoriesHorizontal.Append(new UIAlphaFixer());
            categoriesHorizontalOut.Append(categoriesHorizontal);
            categoriesHorizontalOut.Append(new UIAlphaFixer());

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
            mainList.Append(new UIAlphaFixer());
            mainListIn = new()
            {
                Width = { Precent = 1 },
                Height = { Precent = 1 }
            };
            mainList.Append(mainListIn);
            mainList.OnScrollWheel += (e, _) => { mainScrollbar.ViewPosition -= e.ScrollWheelValue; };
            mainListIn.OnUpdate += (_) => { mainListIn.Top.Pixels = -mainScrollbar.ViewPosition; };
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
                elem.Append(new UIAlphaFixer());
                elem.Append(LoadingImage);
                mainVertical.Append(elem);
            }
            BottomCounter = new UIPanelStyled()
            {
                Width = { Precent = 1 },
                Height = { Pixels = 32 },
                BackgroundColor = UIColors.ColorBackgroundStatic,
                BorderColor = UIColors.ColorBorderStatic
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
                        Tooltip = string.Join("\n", WorkshopHelpMePlease.ModsRequireUpdates.Select(i => i.Item1));
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
                var ontopSettings = new UIPanelStyled()
                {
                    Width = { Precent = 0.3f },
                    Height = { Precent = 1 },
                    PaddingTop = PaddingBottom = PaddingLeft = PaddingRight = 8
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
                var ontopInfo = new UIPanelStyled()
                {
                    Width = { Precent = 0.3f },
                    Height = { Precent = 1 },
                    Left = { Precent = 0.7f },
                    PaddingTop = PaddingBottom = PaddingLeft = PaddingRight = 8
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
                var ontopButtons = new UIPanelStyled()
                {
                    Left = { Precent = 0.3f },
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1 }
                };
                ontopButtons.SetPadding(0);
                var buttonConfig = new UIPanelStyled()
                {
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1f / 3f }
                }.FadedMouseOver();
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
                    Main.menuMode = Interface.modConfigListID;
                    Interface.modConfig.openedFromModder = true;
                    Interface.modConfig.modderOnClose = () =>
                    {
                        Main.menuMode = Interface.modsMenuID;
                    };
                    if (SelectedItem != null && ModLoader.TryGetMod(SelectedItem.mod.Name, out var mod) && ConfigManager.Configs.ContainsKey(mod) && ConfigManager.Configs[mod].Count > 0)
                    {
                        Interface.modConfig.mod = mod;
                        Interface.modConfigList.ModToSelectOnOpen = mod;
                    }
                };
                ontopButtons.Append(buttonConfig);
                var buttonApply = new UIPanelStyled()
                {
                    Width = { Precent = 0.6f },
                    Height = { Precent = 1f / 3f },
                    Left = { Precent = 0.4f }
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
                var buttonReject = new UIPanelStyled()
                {
                    Width = { Precent = 0.6f },
                    Height = { Precent = 1f / 3f },
                    Left = { Precent = 0.4f },
                    Top = { Precent = 1f / 3f }
                }.FadedMouseOver();
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
                var buttonInfo = new UIPanelStyled()
                {
                    Width = { Precent = 0.4f },
                    Height = { Precent = 1f / 3f },
                    Top = { Precent = 1f / 3f }
                }.FadedMouseOver();
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
                buttonInfo.BackgroundColor = buttonInfo.IgnoresMouseInteraction ? UIColors.ColorBackgroundDisabled : UIColors.ColorBackgroundStatic;
                ChangeSelection += () =>
                {
                    buttonInfo.IgnoresMouseInteraction = SelectedItem == null || SelectedItem.mod == null;
                    buttonInfo.BackgroundColor = buttonInfo.IgnoresMouseInteraction ? UIColors.ColorBackgroundDisabled : UIColors.ColorBackgroundStatic;
                };
                ontopButtons.Append(buttonInfo);

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
                    foreach (var item in uIMods)
                    {
                        if (item.mod != null && !ModManager.BadMods.Contains(item.mod.Name)) item.Set(true);
                    }
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
                    var col = c ? UIColors.ColorBackgroundDisabled : UIColors.ColorBackgroundStatic;
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
        public bool IsModFiltered(UIModItemNew item)
        {
            var s = searchField._currentString.ToLower();
            if (s.Length == 0 || item.mod == null) return true;
            var n1 = item.Name.ToLower().Replace("¶", "/");
            var n2 = item.mod.DisplayNameClean.ToLower();
            return n1.Contains(s) || n2.Contains(s) || item.mod.properties.author.ToLower().Contains(s);
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
                    list.Insert(0, new UIModItemNew(null) { Name = folder.Substring(path.Length), Path = path });
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
            if (evt.Target is UIInputTextField field1)
            {
                field1.Text = "";
                return;
            }
            if (evt.Target is UIInputTextFieldPriority<string> field2)
            {
                field2.Text = "";
                return;
            }
            if (evt.Target is UIInputTextFieldPriority<LocalizedText> field3)
            {
                field3.Text = "";
                return;
            }
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
                var i = new UIModsPathPart(item.Replace("¶", "/") + "/", p);
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
            float addGridHeight = 0;
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
                if (!grid) pos.Y += mod.GetOuterDimensions().Height;
            }
            if (grid) pos.Y += addGridHeight;
            pos.Y = MathF.Max(pos.Y, c.Height);
            mainListIn.Height.Pixels = pos.Y;
            mainList.Recalculate();
            mainScrollbar.SetView(c.Height, pos.Y);
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
            (configCollecListIn.Elements[0] as UIModsConfigCollection).Panel.FadedMouseOver();
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

            categoryRecentText.text = FilterCategory == -5 ? Filters[FilterCategoryType] : Filters[2];

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
                    Update(new());
                    Update(new());
                    UpdateDisplayed();
                    RedesignUIMods();
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
        public void Undo()
        {
            if (ToUndo.Count == 0) return;
            var i = ToUndo[ToUndo.Count - 1];

            switch (i.Item1)
            {
                case UndoRedoEnum.EnableMods:
                    foreach (var item in i.Item2 as List<string>)
                    {
                        var mod1 = uIMods.Find(m => m.mod != null && m.mod.Name == item);
                        mod1?.Set(false, false, true);   // Disabling Back
                    }
                    ToRedo.Add(i);
                    break;
                case UndoRedoEnum.DisableMods:
                    foreach (var item in i.Item2 as List<string>)
                    {
                        var mod2 = uIMods.Find(m => m.mod != null && m.mod.Name == item);
                        mod2?.Set(true, false, true);    // Enabling Back
                    }
                    ToRedo.Add(i);
                    break;
                case UndoRedoEnum.RenameMod:
                    (string, string) j1 = (dynamic)i.Item2;   // Mod Name | Rename Name
                    ToRedo.Add((UndoRedoEnum.RenameMod, (j1.Item1, DataConfig.Instance.ModNames[j1.Item1])));
                    DataConfig.Instance.ModNames[j1.Item1] = j1.Item2;
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.RenameFolder:
                    (string, string) j2 = (dynamic)i.Item2;   // Last Path | Rename Path
                    ToRedo.Add((UndoRedoEnum.RenameFolder, (j2.Item2, j2.Item1)));
                    ActRenameFolder(j2.Item1, j2.Item2);
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.Move:
                    (Dictionary<string, string>, List<string>) j3 = (dynamic)i.Item2;    // Taken from config: ModsPaths | Folders
                    ToRedo.Add((UndoRedoEnum.Move, (DataConfig.Instance.ModPaths.ToDictionary(), DataConfig.Instance.Folders.ToList())));
                    DataConfig.Instance.ModPaths = j3.Item1;
                    DataConfig.Instance.Folders = j3.Item2;
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.ChangedFolders:
                    ToRedo.Add((UndoRedoEnum.ChangedFolders, DataConfig.Instance.Folders.ToList()));
                    DataConfig.Instance.Folders = i.Item2 as List<string>;
                    DataConfig.Instance.Save();
                    break;
            }
            ToUndo.Remove(i);
        }
        public void Redo()
        {
            if (ToRedo.Count == 0) return;
            var i = ToRedo[ToRedo.Count - 1];

            switch (i.Item1)
            {
                case UndoRedoEnum.EnableMods:
                    foreach (var item in i.Item2 as List<string>)
                    {
                        var mod1 = uIMods.Find(m => m.mod != null && m.mod.Name == item);
                        mod1?.Set(true, false, true);   // Disabling Back
                    }
                    ToUndo.Add(i);
                    break;
                case UndoRedoEnum.DisableMods:
                    foreach (var item in i.Item2 as List<string>)
                    {
                        var mod2 = uIMods.Find(m => m.mod != null && m.mod.Name == item);
                        mod2?.Set(false, false, true);    // Enabling Back
                    }
                    ToUndo.Add(i);
                    break;
                case UndoRedoEnum.RenameMod:
                    var j1 = i.Item2 as Tuple<string, string>;   // Mod Name | Rename Name
                    ToUndo.Add((UndoRedoEnum.RenameMod, (j1.Item1, DataConfig.Instance.ModNames[j1.Item1])));
                    DataConfig.Instance.ModNames[j1.Item1] = j1.Item2;
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.RenameFolder:
                    var j2 = i.Item2 as Tuple<string, string>;   // Last Path | Rename Path
                    ToUndo.Add((UndoRedoEnum.RenameFolder, (j2.Item2, j2.Item1)));
                    ActRenameFolder(j2.Item1, j2.Item2);
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.Move:
                    var j3 = i.Item2 as Tuple<Dictionary<string, string>, List<string>>;    // Taken from config: ModsPaths | Folders
                    ToUndo.Add((UndoRedoEnum.Move, (DataConfig.Instance.ModPaths.ToDictionary(), DataConfig.Instance.Folders.ToList())));
                    DataConfig.Instance.ModPaths = j3.Item1;
                    DataConfig.Instance.Folders = j3.Item2;
                    DataConfig.Instance.Save();
                    break;
                case UndoRedoEnum.ChangedFolders:
                    ToUndo.Add((UndoRedoEnum.ChangedFolders, DataConfig.Instance.Folders.ToList()));
                    DataConfig.Instance.Folders = i.Item2 as List<string>;
                    DataConfig.Instance.Save();
                    break;
            }
            ToRedo.Remove(i);
        }
        public override void Update(GameTime gameTime)
        {
            if (NeedUpdate)
            {
                UpdateDisplayed();
                NeedUpdate = false;
            }
            if (Main.keyState.PressingControl())
            {
                if (Main.oldKeyState.IsKeyUp(Keys.Z) && Main.keyState.IsKeyDown(Keys.Z))
                {
                    Undo();
                }
                else if (Main.oldKeyState.IsKeyUp(Keys.Y) && Main.keyState.IsKeyDown(Keys.Y))
                {
                    Redo();
                }
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
                if (ReloadTask.Result != null)
                {
                    uIMods = ReloadTask.Result;
                    LoadingImage.Color = Color.Transparent;
                }
                else
                {
                    uIMods.Clear();
                    waitForReload = true;
                }
                ReloadTask = null;
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

            cfg.FilterCategory = FilterCategory;
            cfg.FilterCategoryType = FilterCategoryType;

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
