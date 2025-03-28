﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModManager.Content.ModsList
{
    public class UIModItemNew : UIPanel
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

        public bool Active;
        public string Name;

        public float timetograb;
        public bool grabbed;

        public Vector2 grabbedPos = Vector2.Zero;
        public float grabbedListPos = 0;

        public List<string> References;

        public UIModItemNew(LocalMod _mod)
        {
            mod = _mod;
            Name = mod?.DisplayNameClean ?? "Folder";
            if (mod != null)
            {
                loaded = ModLoader.HasMod(mod.Name);
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
                text = Name,
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
                textVersion = new()
                {
                    text = "v" + mod.properties.version.ToString(),
                    Height = { Precent = 1 },
                    Top = { Pixels = 4 }
                };
                Append(textVersion);
                divider1 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = new Color(0, 0, 0, 0.5f),
                    Height = { Precent = 1 },
                    Width = { Pixels = 4 },
                    ScaleToFit = true
                };
                Append(divider1);
                divider2 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = new Color(0, 0, 0, 0.5f),
                    Height = { Precent = 1 },
                    Width = { Pixels = 4 },
                    ScaleToFit = true
                };
                Append(divider2);
                divider3 = new(TextureAssets.MagicPixel)
                {
                    OverrideSamplerState = SamplerState.PointClamp,
                    Color = new Color(0, 0, 0, 0.5f),
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
                if (dependentsOn != "") dependentsOn = Language.GetTextValue("tModLoader.ModDependentsTooltip", "\n" + dependentsOn);
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
                if (dep != "") dep = Language.GetTextValue("tModLoader.ModDependencyTooltip", "\n" + dep);
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
                toggle.OnLeftClick += (e, l) =>
                {
                    Set(null);
                };
                Append(toggle);
            }
            Redesign();
        }
        public void Set(bool? enabled = null)
        {
            enabled ??= !mod.Enabled;

            if (enabled == true)
            {
                var l = References.ToList();
                foreach (var item in UIModsNew.Instance.uIMods)
                {
                    if (item.mod != null && l.Contains(item.mod.Name))
                    {
                        item.Set(true);
                        l.Remove(item.mod.Name);
                    }
                }
                if (l.Count != 0)
                {
                    System.Windows.Forms.MessageBox.Show("Missing mods: " + string.Join(",", l), "Missing Mods", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
            }
            mod.Enabled = enabled.Value;

            if (toggle != null)
            {
                toggle._texture = mod.Enabled ? ModManager.AssetToggleOn : ModManager.AssetToggleOff;
                toggle.Color = mod.Enabled != loaded ? Color.Gold : mod.Enabled ? new Color(0.75f, 1f, 0.75f) : Color.White;
            }
            UIModsNew.Instance.AddCollections();
            UIModsNew.Instance.CheckChanged();
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
            textName.Width = grid ? new(-8, 1) : new(e[0].GetOuterDimensions().Width - textName.Left.Pixels, 0);
            textName.Height = grid ? new(14 * scaleText, 0) : new(0, 1);
            textName.align = grid ? 0.5f : 0;
            textName.scale = scaleText;

            if (mod != null)
            {
                if (DataConfig.Instance.ModNames.ContainsKey(mod.Name))
                    Name = DataConfig.Instance.ModNames[mod.Name];
                textName.text = Name;
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

                toggle.Top = grid ? new(0, 0) : new(-12, 0.5f);
                toggle.Left.Pixels = grid ? 0 : -2;
                toggle.ImageScale = grid ? 1f : 0.75f;

                textAuthor.Left.Pixels = textName.Left.Pixels + textName.Width.Pixels + 6;
                textAuthor.Width.Pixels = e[1].GetOuterDimensions().Width - 6;
                textAuthor.scale = scaleText;
                divider1.Left.Pixels = textAuthor.Left.Pixels - 8;

                textVersion.Left.Pixels = textAuthor.Left.Pixels + textAuthor.Width.Pixels + 6;
                textVersion.Width.Pixels = e[2].GetOuterDimensions().Width - 6;
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
            //Tooltip = null;
            base.Update(gameTime);

            if (timetograb > 0)
            {
                timetograb -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timetograb <= 0 || Vector2.DistanceSquared(grabbedPos, Main.MouseScreen) >= 49 && mod != null)
                {
                    UIModsNew.Instance.GrabbedItem = this;
                    grabbed = true;
                    timetograb = -1;
                }
            }
            if (grabbed)
            {
                var pos = Main.MouseScreen - grabbedPos;
                Left.Pixels += pos.X;
                Top.Pixels += pos.Y + UIModsNew.Instance.mainScrollbar._viewPosition - grabbedListPos;
                grabbedListPos = UIModsNew.Instance.mainScrollbar._viewPosition;
                grabbedPos += pos;
                Recalculate();
            }
            IgnoresMouseInteraction = UIModsNew.Instance.GrabbedItem == this;
            BackgroundColor = UIModsNew.Instance.SelectedItem == this ? new Color(103, 112, 201) : IsMouseHovering ? new Color(93, 102, 171) * 0.7f : new Color(63, 82, 151) * 0.7f;
            BorderColor = UIModsNew.Instance.SelectedItem == this ? Color.Gold : Color.Black;
            if (UIModsNew.Instance.GrabbedItem != null)
            {
                if (mod == null && UIModsNew.Instance.GrabbedItem != this)
                {
                    BackgroundColor = IsMouseHovering ? new Color(63, 82, 151) : new Color(63, 82, 151) * 0.75f;
                    BorderColor = IsMouseHovering ? Color.LightYellow : Color.Lime;
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
            grabbed = false;
            UIModsNew.Instance.GrabbedItem = null;
            UIModsNew.Instance.SelectedItem = null;
            UIModsNew.Instance.ChangeSelection();
            if (mod == null)
            {
                UIModsNew.Instance.OpenedPath.Add(Name);
                UIModsNew.Instance.RecalculatePath();
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            UIModsNew.Instance.SelectedItem = this;
            UIModsNew.Instance.ChangeSelection();
            if (!UIModsNew.Instance.OpenedCollections)
            {
                timetograb = 0.4f;
                grabbedPos = Main.MouseScreen;
                grabbedListPos = UIModsNew.Instance.mainScrollbar._viewPosition;
            }
            base.LeftMouseDown(evt);
        }
        private void LeftMouseUp(UIMouseEvent evt, UIElement _)
        {
            LeftMouseUp(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            grabbed = false;
            timetograb = -2;
            if (UIModsNew.Instance.GrabbedItem == this)
            {
                grabbed = false;
                if (UIModsNew.Instance.GrabbedFolder != null)
                {
                    var path = UIModsNew.Instance.GrabbedFolder;
                    if (path.StartsWith("collections/"))
                    {
                        if (mod != null && !DataConfig.Instance.Collections[path.Substring(12)].Contains(mod.Name))
                            DataConfig.Instance.Collections[path.Substring(12)].Add(mod.Name);
                    }
                    else
                    {
                        if (!path.StartsWith("/")) path = "/" + path;

                        if (mod == null)
                        {
                            path += "/" + Name;
                            var curPath = string.Join("/", UIModsNew.Instance.OpenedPath) + "/" + Name;
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
                        else DataConfig.Instance.ModPaths[mod?.Name ?? Name] = path;
                    }
                    DataConfig.Instance.Save();
                }
                UIModsNew.Instance.SelectedItem = this;
                UIModsNew.Instance.GrabbedItem = null;
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
