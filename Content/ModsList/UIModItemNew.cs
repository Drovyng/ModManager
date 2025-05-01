using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.Social.Base;
using Terraria.UI;

namespace ModManager.Content.ModsList
{
    public class UIModItemNew : UIPanelStyled
    {
        public LocalMod mod;
        public bool loaded;

        public UIImage icon;
        public UITextDots<string> textName;
        public UITextDots<string> textAuthor;
        public UITextDots<string> textVersion;
        public UIElement flagsMarkers;

        public UIImage divider1;
        public UIImage divider2;
        public UIImage divider3;

        public UIImage toggle;

        public UIPanelStyled cantUse;
        public UITextLines cantUseText;

        public bool CanDraw;

        public bool Active;
        public string Name;
        public string Path;

        public float timetograb;
        public bool needUpdate;

        public List<string> References = new();
        public Vector2 grabbedPos;

        public UIModItemNew(LocalMod _mod)
        {
            mod = _mod;
            Name = mod?.DisplayNameClean ?? "Folder";
            if (mod != null)
            {
                loaded = ModLoader.HasMod(mod.Name);
                if (DataConfig.Instance.ModNames.ContainsKey(mod.Name))
                    Name = DataConfig.Instance.ModNames[mod.Name];
            }
        }
        public override void OnInitialize()
        {
            UIModsNew.Instance.OnLeftMouseUp += LeftMouseUp;

            SetPadding(0);

            Width.Precent = 1;

            Asset<Texture2D> texture = mod == null ? TextureAssets.Camera[6] : ModManager.AssetModIcon;
            if (mod != null && mod.modFile.HasFile("icon.png"))
            {
                try
                {
                    using (mod.modFile.Open())
                    {
                        using Stream stream = mod.modFile.GetStream("icon.png");
                        Asset<Texture2D> asset = Main.Assets.CreateUntracked<Texture2D>(stream, ".png");
                        if (asset.Width() == 80 && asset.Height() == 80)
                        {
                            texture = asset;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logging.tML.Error("Unknown error", exception);
                }
            }

            icon = new UIImage(texture)
            {
                ScaleToFit = true,
                IgnoresMouseInteraction = true,
                Top = { Pixels = 3 },
                Left = { Pixels = 10 },
                OverrideSamplerState = SamplerState.PointClamp
            };
            Append(icon);
            textName = new()
            {
                text = Name.Replace("¶", "/"),
                Height = { Precent = 1 },
                Top = { Pixels = 4 }
            };
            Append(textName);
            if (mod != null)
            {
                flagsMarkers = new()
                {
                    Width = { Pixels = 200f },
                    Height = { Precent = 1 },
                    HAlign = 1
                };
                Append(flagsMarkers);
                textAuthor = new()
                {
                    text = mod.properties.author,
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                Append(textAuthor);
                needUpdate = WorkshopHelpMePlease.ModsRequireUpdates.Any(l => l.Item1 == mod.Name);
                textVersion = new()
                {
                    text = "v" + mod.properties.version.ToString() + (needUpdate ? " (" + Language.GetTextValue("Mods.ModManager.NeedUpdate") + ")" : ""),
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                Append(textVersion);
                divider1 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = UIColors.ColorBorderStatic * 0.75f,
                    Height = { Precent = 1 },
                    Width = { Pixels = 4 },
                    ScaleToFit = true
                };
                Append(divider1);
                divider2 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = UIColors.ColorBorderStatic * 0.75f,
                    Height = { Precent = 1 },
                    Width = { Pixels = 4 },
                    ScaleToFit = true
                };
                Append(divider2);
                divider3 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = UIColors.ColorBorderStatic * 0.75f,
                    Height = { Precent = 1 },
                    Width = { Pixels = 4 },
                    ScaleToFit = true
                };
                Append(divider3);

                References = mod.properties.modReferences.Select((x) => x.mod).ToList();

                var availableMods = ModOrganizer.RecheckVersionsToLoad().ToList();

                float offset = 8;

                (string, Version) tuple = ModOrganizer.modsThatUpdatedSinceLastLaunch.FirstOrDefault((x) => x.ModName == mod.Name);
                if (tuple.Item1 != null || tuple.Item2 != null)
                {
                    var img1 = new UIImage(ModManager.AssetSettingsToggle)
                    {
                        Color = tuple.Item2 == null ? Color.Green : new Color(6, 95, 212),
                        NormalizedOrigin = Vector2.One * 0.5f,
                        VAlign = 0.5f,
                        HAlign = 1f,
                        Left = { Pixels = -offset },
                        Width = { Pixels = 22 },
                        Height = { Pixels = 22 },
                    };
                    img1.OnUpdate += (_) => { if (img1.IsMouseHovering) UIModsNew.Instance.Tooltip = tuple.Item2 == null ? Language.GetTextValue("tModLoader.ModAddedSinceLastLaunchMessage") : Language.GetTextValue("tModLoader.ModAddedSinceLastLaunchMessage", tuple.Item2); };
                    flagsMarkers.Append(img1);
                    offset += 24;
                }

                string dependentsOn = string.Join("\n", (from m in availableMods
                                                         where m.properties.RefNames(includeWeak: false).Any((refName) => refName.Equals(mod.Name))
                                                         select m.Name).ToArray());
                if (dependentsOn != "") dependentsOn = Language.GetTextValue("tModLoader.ModDependentsTooltip", dependentsOn);
                void GetDependencies(LocalMod _mod, HashSet<string> allDependencies)
                {
                    if (_mod == null) return;
                    string[] array4 = _mod.properties.modReferences.Select((x) => x.mod).ToArray();
                    foreach (string text4 in array4)
                    {
                        if (allDependencies.Add(text4))
                        {
                            GetDependencies(availableMods.Find((m) => m.Name == text4), allDependencies);
                        }
                    }
                }
                var deps = new HashSet<string>();
                GetDependencies(mod, deps);
                string dep = string.Join("\n", deps);
                if (dep != "") dep = Language.GetTextValue("tModLoader.ModDependencyTooltip", dep);
                if (dep != "" && dependentsOn != "") dep += "\n\n";
                dependentsOn = dep + dependentsOn;
                if (dependentsOn != "")
                {
                    var img2 = new UIImage(UICommon.ButtonDepsTexture)
                    {
                        NormalizedOrigin = Vector2.One * 0.5f,
                        VAlign = 0.5f,
                        HAlign = 1f,
                        Left = { Pixels = -offset },
                        Width = { Pixels = 22 },
                        Height = { Pixels = 22 },
                    };
                    img2.OnUpdate += (_) => { if (img2.IsMouseHovering) UIModsNew.Instance.Tooltip = dependentsOn; };
                    flagsMarkers.Append(img2);
                    offset += 24;
                }

                if (mod.properties.RefNames(includeWeak: true).Any() && mod.properties.translationMod)
                {
                    var s = Language.GetTextValue("tModLoader.TranslationModTooltip", string.Join("\n ", mod.properties.RefNames(includeWeak: true)));
                    var img3 = new UIImage(UICommon.ButtonTranslationModTexture)
                    {
                        NormalizedOrigin = Vector2.One * 0.5f,
                        VAlign = 0.5f,
                        HAlign = 1f,
                        Left = { Pixels = -offset },
                        Width = { Pixels = 22 },
                        Height = { Pixels = 22 },
                    };
                    img3.OnUpdate += (_) => { if (img3.IsMouseHovering) UIModsNew.Instance.Tooltip = s; };
                    flagsMarkers.Append(img3);
                    offset += 24;
                }
                if (mod.properties.side == ModSide.Server || mod.properties.side == ModSide.Both)
                {
                    var img4 = new UIImage(ModManager.AssetIconServer)
                    {
                        NormalizedOrigin = Vector2.One * 0.5f,
                        VAlign = 0.5f,
                        HAlign = 1f,
                        Left = { Pixels = -offset },
                        Width = { Pixels = 22 },
                        Height = { Pixels = 22 },
                    };
                    flagsMarkers.Append(img4);
                    offset += 24;
                }
                if (mod.properties.side == ModSide.Client || mod.properties.side == ModSide.Both)
                {
                    var img5 = new UIImage(ModManager.AssetIconClient)
                    {
                        NormalizedOrigin = Vector2.One * 0.5f,
                        VAlign = 0.5f,
                        HAlign = 1f,
                        Left = { Pixels = -offset },
                        Width = { Pixels = 22 },
                        Height = { Pixels = 22 },
                    };
                    flagsMarkers.Append(img5);
                }
                toggle = new(mod.Enabled ? ModManager.AssetToggleOn : ModManager.AssetToggleOff)
                {
                    ImageScale = 0.75f,
                    NormalizedOrigin = Vector2.One * 0.5f,
                    Color = mod.Enabled ? new Color(0.75f, 1f, 0.75f) : Color.White
                };
                Append(toggle);

                string text2 = null;
                string updateURL = "https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-players#beta-branches";
                Color color = Color.Orange;
                if (BuildInfo.tMLVersion.MajorMinorBuild() < mod.tModLoaderVersion.MajorMinorBuild())
                {
                    text2 = $"v{mod.tModLoaderVersion}";
                    if (mod.tModLoaderVersion.Build == 2)
                    {
                        text2 = "Preview " + text2;
                    }
                }
                var recommended = SocialBrowserModule.GetBrowserVersionNumber(mod.tModLoaderVersion);
                if (recommended != SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion))
                {
                    text2 = $"{recommended} v{mod.tModLoaderVersion}";
                    color = Color.Yellow;
                }
                cantUse = new UIPanelStyled()
                {
                    Width = { Precent = 1 },
                    Height = { Precent = 1 },
                    IgnoresMouseInteraction = true
                };
                Append(cantUse);
                cantUse.BackgroundColor = cantUse.BorderColor = Color.Transparent;
                if (text2 != null)
                {
                    cantUse.IgnoresMouseInteraction = false;
                    cantUse.BorderColor = cantUse.BackgroundColor = Color.Black * 0.65f;
                    cantUse.OnMouseOver += delegate
                    {
                        SoundEngine.PlaySound(in SoundID.MenuTick);
                        cantUse.BorderColor = Color.Gold;
                    };
                    cantUse.OnMouseOut += delegate { cantUse.BorderColor = Color.Black * 0.65f; };
                    cantUse.SetPadding(8);
                    cantUse.OnLeftDoubleClick += (_, _) => Utils.OpenToURL(updateURL);
                    cantUseText = new()
                    {
                        text = Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", text2),
                        color = color,
                        Width = { Precent = 1 },
                        Height = { Pixels = 0 },
                        align = 0.5f,
                        VAlign = 0.5f,
                        Top = { Pixels = 4 }
                    };
                    cantUse.Append(cantUseText);
                }
                else
                {
                    toggle.OnLeftClick += (e, l) =>
                    {
                        Set(null);
                    };
                }
            }
            Redesign();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (UIModsNew.Instance.SelectedItems.Contains(this) && UIModsNew.Instance.GrabbedItem)
            {
                if (CanDraw) base.Draw(spriteBatch);
                CanDraw = false;
                return;
            }
            base.Draw(spriteBatch);
        }
        public void Set(bool? enabled = null, bool check = true, bool doNotCheckIAmPro = false)
        {
            if (mod == null || cantUseText != null) return;

            enabled ??= !mod.Enabled;

            IEnumerable<UIModItemNew> savedList = null;
            var act = UIModsNew.UndoRedoEnum.EnableMods;

            if (!doNotCheckIAmPro)
            {
                if (enabled == true)
                {
                    savedList = UIModsNew.Instance.uIMods.Where(m => m.mod != null && !m.mod.Enabled);
                    var l = References.ToList();
                    foreach (var item in UIModsNew.Instance.uIMods)
                    {
                        if (item.mod != null && l.Contains(item.mod.Name))
                        {
                            if (item.cantUseText != null)
                            {
                                System.Windows.Forms.MessageBox.Show("Dependency Mod \"" + item.mod.DisplayNameClean + "\" Cant Be Enabled!", "Outdated Dependency Mod", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                return;
                            }
                            item.Set(true, false);
                            l.Remove(item.mod.Name);
                        }
                    }
                    savedList = savedList.Where(m => m.mod.Enabled);
                    if (l.Count != 0)
                    {
                        UIModsNew.Instance.AddCollections();
                        UIModsNew.Instance.CheckChanged();
                        System.Windows.Forms.MessageBox.Show("Missing mods: " + string.Join(",", l), "Missing Mods", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    var need = new List<string>();
                    foreach (var item in UIModsNew.Instance.uIMods)
                    {
                        if (check && item.mod != null && item.References.Contains(mod.Name))
                        {
                            item.Set(false, false);
                        }
                        else if (item.mod != null && item.mod.Enabled && item != this)
                        {
                            need.AddRange(item.References);
                        }
                    }
                    savedList = UIModsNew.Instance.uIMods.Where(m => m.mod != null && m.mod.Enabled);
                    act = UIModsNew.UndoRedoEnum.DisableMods;
                    foreach (var item in UIModsNew.Instance.uIMods)
                    {
                        if (item.mod != null && References.Contains(item.mod.Name) && !need.Contains(item.mod.Name))
                        {
                            Logging.PublicLogger.Info("Disabling " + item.mod.Name);
                            item.Set(false, false);
                        }
                    }
                    savedList = savedList.Where(m => !m.mod.Enabled);
                }
            }
            {
                mod.Enabled = enabled.Value;
                if (savedList != null && !doNotCheckIAmPro)
                {
                    var l = savedList.Where(m => m.mod.Enabled).Select(m => m.mod.Name).ToList();
                    l.Add(mod.Name);
                    UIModsNew.Instance.ToRedo.Clear();
                    UIModsNew.Instance.ToUndo.Add((UIModsNew.UndoRedoEnum.EnableMods, l));
                }
            }
            if (toggle != null)
            {
                toggle._texture = mod.Enabled ? ModManager.AssetToggleOn : ModManager.AssetToggleOff;
                toggle.Color = mod.Enabled != loaded ? Color.Gold : mod.Enabled ? new Color(0.75f, 1f, 0.75f) : Color.White;
            }
            if (check)
            {
                UIModsNew.Instance.AddCollections();
                UIModsNew.Instance.CheckChanged();
            }
        }
        public void Redesign()
        {
            float scale = UIModsNew.Instance.scale;
            float scaleText = UIModsNew.Instance.scaleText;
            bool grid = scale >= UIModsNew.Instance.scaleThreshold;
            var e = UIModsNew.Instance.categoriesHorizontal.Elements;

            icon.Height = icon.Width = new(32 * scale - 6, 0);
            icon.Left.Pixels = grid ? 10 : 20;
            textName.Left.Pixels = grid ? 4 : 20 + 32 * scale;
            textName.Top.Pixels = grid ? 32 * scale + 9 + 5 * scaleText : 4;
            textName.Width = grid ? new(-8, 1) : new(e[0].GetOuterDimensions().Width + e[1].GetOuterDimensions().Width - textName.Left.Pixels, 0);
            textName.Height = grid ? new(14 * scaleText, 0) : new(0, 1);
            textName.align = grid ? 0.5f : 0;
            textName.scale = scaleText;
            if (cantUseText != null) cantUseText.scale = scaleText;

            if (mod != null)
            {
                if (DataConfig.Instance.ModNames.ContainsKey(mod.Name))
                    Name = DataConfig.Instance.ModNames[mod.Name];
                textName.text = Name.Replace("¶", "/");
                if (grid)
                {
                    textAuthor.Remove();
                    textVersion.Remove();
                    flagsMarkers.Remove();
                    divider1.Remove();
                    divider2.Remove();
                    divider3.Remove();
                }
                else
                {
                    Append(textAuthor);
                    Append(textVersion);
                    Append(flagsMarkers);
                    Append(divider1);
                    Append(divider2);
                    Append(divider3);
                }
                Elements.Remove(cantUse);
                Elements.Add(cantUse);
                

                toggle.Top = grid ? new(2, 0) : new(-12, 0.5f);
                toggle.Left.Pixels = grid ? 2 : -2;
                toggle.ImageScale = grid ? 1f : 0.75f;

                textAuthor.Left.Pixels = textName.Left.Pixels + textName.Width.Pixels + 6;
                textAuthor.Width.Pixels = e[2].GetOuterDimensions().Width - 6;
                textAuthor.scale = scaleText;
                divider1.Left.Pixels = textAuthor.Left.Pixels - 8;

                textVersion.Left.Pixels = textAuthor.Left.Pixels + textAuthor.Width.Pixels + 6;
                textVersion.Width.Pixels = e[3].GetOuterDimensions().Width - 6;
                textVersion.scale = scaleText;
                divider2.Left.Pixels = textVersion.Left.Pixels - 8;
                divider3.Left.Pixels = divider2.Left.Pixels + textVersion.Width.Pixels + 8;
            }
            icon.Top.Pixels = grid ? 10 : 3;

            Height.Pixels = 32 * scale + (grid ? 16 + 20 * scaleText : 0);

            Width = grid ? new(32 * scale + 16, 0) : new(0, 1);

            Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (timetograb > 0)
            {
                timetograb -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timetograb <= 0 || Vector2.DistanceSquared(grabbedPos, Main.MouseScreen) >= 49 && mod != null)
                {
                    UIModsNew.Instance.GrabbedItem = true;
                    timetograb = -1;
                }
            }
            var t = UIModsNew.Instance.SelectedItems.Contains(this);
            IgnoresMouseInteraction = t && UIModsNew.Instance.GrabbedItem;
            BackgroundColor = t ? UIColors.ColorBackgroundSelected : IsMouseHovering ? UIColors.ColorBackgroundHovered : UIColors.ColorBackgroundStatic;
            BorderColor = t ? UIColors.ColorBorderHovered : UIColors.ColorBorderStatic;
            if (needUpdate)
            {
                BackgroundColor.R = (byte)(MathHelper.Lerp(BackgroundColor.R, UIColors.ColorNeedUpdate.R, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.G = (byte)(MathHelper.Lerp(BackgroundColor.G, UIColors.ColorNeedUpdate.G, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.B = (byte)(MathHelper.Lerp(BackgroundColor.B, UIColors.ColorNeedUpdate.B, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.R * UIColors.ColorNeedUpdate.A / 255f);
                BackgroundColor.A = (byte)(MathHelper.Lerp(BackgroundColor.A, UIColors.ColorNeedUpdate.A, UIColors.ColorNeedUpdate.A / 255f) + UIColors.ColorNeedUpdate.A * UIColors.ColorNeedUpdate.A / 255f);
            }
            if (UIModsNew.Instance.GrabbedItem)
            {
                if (mod == null && !t)
                {
                    BackgroundColor = IsMouseHovering ? UIColors.ColorBackgroundSelected : UIColors.ColorBackgroundHovered;
                    BorderColor = IsMouseHovering ? UIColors.ColorBorderAllowDropHovered : UIColors.ColorBorderAllowDrop;
                    if (IsMouseHovering)
                    {
                        UIModsNew.Instance.GrabbedFolder = string.Join("/", UIModsNew.Instance.OpenedPath) + "/" + Name + "/";
                    }
                    return;
                }
                if (!IgnoresMouseInteraction)
                {
                    var a = BackgroundColor.A;
                    BackgroundColor *= 0.5f;
                    BackgroundColor.A = a;
                }
                return;
            }
        }
        public override void LeftDoubleClick(UIMouseEvent evt)
        {
            timetograb = -2;
            if (mod == null)
            {
                UIModsNew.Instance.GrabbedItem = false;
                UIModsNew.Instance.SelectedItems.Clear();
                UIModsNew.Instance.ChangeSelection();

                UIModsNew.Instance.OpenedPath.Add(Name);
                UIModsNew.Instance.RecalculatePath();
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            timetograb = 0.4f;
            grabbedPos = Main.MouseScreen;
            if (!UIModsNew.Instance.SelectedItems.Contains(this)){
                if (Main.keyState.PressingControl())
                {
                    UIModsNew.Instance.SelectedItems.Add(this);
                }
                else if (Main.keyState.PressingShift() && UIModsNew.Instance.SelectedItems.Count > 0)
                {
                    var i1 = UIModsNew.Instance.mainListIn.Elements.IndexOf(UIModsNew.Instance.SelectedItems[0]);
                    var i2 = UIModsNew.Instance.mainListIn.Elements.IndexOf(this);
                    if (i1 != -1 && i2 != -1)
                    {
                        UIModsNew.Instance.SelectedItems.Clear();
                        var min = Math.Min(i1, i2);
                        var max = Math.Max(i1, i2);
                        for (int i = min; i <= max; i++)
                        {
                            if (UIModsNew.Instance.mainListIn.Elements[i] is UIModItemNew mi)
                            {
                                UIModsNew.Instance.SelectedItems.Add(mi);
                            }
                        }
                    }
                }
                else
                {
                    UIModsNew.Instance.SelectedItems.Clear();
                    UIModsNew.Instance.SelectedItems.Add(this);
                }
            }
            else if (Main.keyState.PressingControl())
            {
                UIModsNew.Instance.SelectedItems.Remove(this);
            }
            UIModsNew.Instance.ChangeSelection();
            if (UIModsNew.Instance.OpenedCollections || Main.keyState.PressingShift() || Main.keyState.PressingControl())
            {
                timetograb = -2;
            }
            base.LeftMouseDown(evt);
        }
        private void LeftMouseUp(UIMouseEvent evt, UIElement _)
        {
            LeftMouseUp(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (timetograb > 0)
            {
                UIModsNew.Instance.SelectedItems.Clear();
                UIModsNew.Instance.SelectedItems.Add(this);
            }
            timetograb = -2;
            if (UIModsNew.Instance.GrabbedItem)
            {
                UIModsNew.Instance.GrabbedItem = false;
                if (UIModsNew.Instance.GrabbedFolder != null)
                {
                    var path = UIModsNew.Instance.GrabbedFolder;
                    if (path.StartsWith("collections/"))
                    {
                        foreach (var item in UIModsNew.Instance.SelectedItems)
                        {
                            if (item.mod != null && !DataConfig.Instance.Collections[path.Substring(12)].Contains(item.mod.Name))
                                DataConfig.Instance.Collections[path.Substring(12)].Add(item.mod.Name);
                        }
                    }
                    else
                    {
                        if (!path.StartsWith("/")) path = "/" + path;

                        UIModsNew.Instance.ToRedo.Clear();
                        UIModsNew.Instance.ToUndo.Add((UIModsNew.UndoRedoEnum.Move, (DataConfig.Instance.ModPaths.ToDictionary(), DataConfig.Instance.Folders.ToList())));

                        foreach (var itemMod in UIModsNew.Instance.SelectedItems)
                        {
                            if (itemMod.mod == null)
                            {
                                path += "/" + itemMod.Name;
                                var curPath = string.Join("/", UIModsNew.Instance.OpenedPath) + "/" + itemMod.Name;
                                if (!curPath.StartsWith("/")) curPath = "/" + curPath;

                                var vls = DataConfig.Instance.ModPaths.ToList();
                                foreach (var item in vls)
                                {
                                    if (item.Value == curPath) DataConfig.Instance.ModPaths[item.Key] = path.Replace("//", "/");
                                    else if (item.Value.StartsWith(curPath)) DataConfig.Instance.ModPaths[item.Key] = (path + item.Value.Substring(curPath.Length)).Replace("//", "/");
                                }
                                var vls2 = DataConfig.Instance.Folders.ToList();
                                foreach (var item in vls2)
                                {
                                    if (item == curPath)
                                    {
                                        DataConfig.Instance.Folders.Remove(item);
                                        DataConfig.Instance.Folders.Add(path.Replace("//", "/"));
                                    }
                                    else if (item.StartsWith(curPath))
                                    {
                                        DataConfig.Instance.Folders.Remove(item);
                                        DataConfig.Instance.Folders.Add((path + item.Substring(curPath.Length)).Replace("//", "/"));
                                    }
                                }
                            }
                            else DataConfig.Instance.ModPaths[itemMod.mod?.Name ?? itemMod.Name] = path;
                        }
                    }
                    DataConfig.Instance.Save();
                }
                UIModsNew.Instance.GrabbedItem = false;
                UIModsNew.Instance.ChangeSelection();
                UIModsNew.Instance.UpdateDisplayed();
            }
            base.LeftMouseUp(evt);
        }
        public int Sort(UIModItemNew other, int FilterCategory, int FilterTypo)
        {
            var mul = FilterTypo == 1 ? -1 : 1;
            if (mod == null && other.mod == null)
            {
                return Name.CompareTo(other.Name) * (FilterCategory > 1 ? mul : 1);
            }
            if (other.mod == null) return 1;
            if (mod == null) return -1;
            if (FilterCategory == -5)
            {
                return mod.lastModified.CompareTo(other.mod.lastModified) * -mul;
            }
            if (FilterCategory == 0)
            {
                return mod.Enabled.CompareTo(other.mod.Enabled) * -mul;
            }
            if (FilterCategory == 1)
            {
                return Name.CompareTo(other.Name) * mul;
            }
            if (FilterCategory == 2)
            {
                return mod.properties.author.CompareTo(other.mod.properties.author) * mul;
            }
            if (FilterCategory == 3)
            {
                return mod.properties.version.CompareTo(other.mod.properties.version) * mul;
            }
            if (FilterCategory == 4)
            {
                return mod.properties.side.ToFriendlyString().CompareTo(other.mod.properties.side.ToFriendlyString()) * mul;
            }
            return 0;
        }
    }
}
