﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;

namespace ModManager.Content.ModsList
{
    public class UIModsContextMenu : UIPanelSizeable
    {
        public UIElement Target;
        public List<UIModsContextMenuAction> Actions = new();
        private float timeout = 1f;
        private float posY = 1;
        public Action OnDraw;
        public void Popup()
        {
            BackgroundColor = UICommon.DefaultUIBlue * 0.825f;
            timeout = 1;
            PaddingBottom = PaddingTop = PaddingLeft = PaddingRight = 0;
            Elements.Clear();
            Actions.Clear();
            Width.Set(150, 0);
            Left.Set(Main.mouseX - 4, 0);
            Top.Set(Main.mouseY - 4, 0);
            UseRight = true;
            MinHorizontal = 150;
            posY = 1;

            UIModItemNew uIMod = null;
            UIModsCollection coll = null;
            UIModsConfigCollection colld = null;

            if (Target is UIModItemNew uIMod1) uIMod = uIMod1;
            if (Target.Parent is UIModItemNew uIMod2) { uIMod = uIMod2; Target = Target.Parent; }

            if (Target is UIModsCollection coll1) coll = coll1;
            if (Target.Parent is UIModsCollection coll2) coll = coll2;
            if (Target.Parent.Parent is UIModsCollection coll3) coll = coll3;

            if (Target is UIModsConfigCollection colld1) colld = colld1;
            if (Target.Parent is UIModsConfigCollection colld2) colld = colld2;
            if (Target.Parent.Parent is UIModsConfigCollection colld3) colld = colld3;

            if (uIMod != null)
            {
                if (!UIModsNew.Instance.SelectedItems.Contains(uIMod))
                {
                    UIModsNew.Instance.SelectedItems.Clear();
                    UIModsNew.Instance.SelectedItems.Add(uIMod);
                    UIModsNew.Instance.ChangeSelection();
                }
                if (uIMod.mod == null && UIModsNew.Instance.SelectedItem != null)
                {
                    AddAction("CreateFolder");
                    AddAction("Rename");
                    AddAction("Delete");
                }
                else
                {
                    if (!UIModsNew.Instance.OpenedCollections)
                    {
                        AddAction("CreateFolder");
                        AddSeparator();
                    }
                    if (UIModsNew.Instance.SelectedItem != null)
                    {
                        AddAction("Info");
                        AddAction(uIMod.mod.Enabled ? "Disable" : "Enable");
                        AddAction(uIMod.Locked ? "Unlock" : "Lock");
                        AddAction("Config", !(ModLoader.TryGetMod(UIModsNew.Instance.SelectedItem.mod.Name, out var mod) && ConfigManager.Configs.ContainsKey(mod) && ConfigManager.Configs[mod].Count > 0));
                        AddAction("Rename");
                        AddAction(UIModsNew.Instance.OpenedCollections ? "Remove" : "Delete");
                    }
                    else
                    {
                        AddAction("Enable");
                        AddAction("Disable");
                        bool onlyMods = true;
                        bool onlyFolders = true;
                        foreach (var item in UIModsNew.Instance.SelectedItems)
                        {
                            if (item.mod == null) onlyMods = false;
                            else onlyFolders = false;
                        }
                        if (onlyMods)
                        {
                            AddAction("Lock");
                            AddAction("Unlock");
                        }
                        if (UIModsNew.Instance.OpenedCollections) AddAction("Remove");
                        else if (onlyMods || onlyFolders) AddAction("Delete");
                    }
                }
            }
            else if (coll != null && !coll.isAll)
            {
                AddAction("CreateCollection");
                AddAction("RenameCollection");
                AddAction("DeleteCollection");
                UIModsNew.Instance.SelectedCollection = coll;
            }
            else if (colld != null && !colld.isAll)
            {
                Target = colld;
                AddAction("CreateConfigCollection");
                AddAction("RenameConfigCollection");
                AddAction("DeleteConfigCollection");
            }
            else if (Target == UIModsNew.Instance.mainListIn && !UIModsNew.Instance.OpenedCollections)
            {
                AddAction("CreateFolder");
            }
            else if (Target == UIModsNew.Instance.mainList && !UIModsNew.Instance.OpenedCollections)
            {
                AddAction("CreateFolder");
            }
            else if (Target == UIModsNew.Instance.collecListIn)
            {
                AddAction("CreateCollection");
            }
            else if (Target == UIModsNew.Instance.configCollecListIn)
            {
                AddAction("CreateConfigCollection");
            }
            else
            {
                AddAction("NoActions", true);
            }
            Height.Set(posY, 0);
            Recalculate();
        }
        private bool RenameCollection(string value)
        {
            var cfg = DataConfig.Instance;
            if (cfg.Collections.ContainsKey(value)) return false;
            var l = cfg.Collections[UIModsNew.Instance.SelectedCollection.Text.text];
            cfg.Collections.Remove(UIModsNew.Instance.SelectedCollection.Text.text);
            cfg.Collections[value] = l;
            cfg.Save();
            UIModsNew.Instance.AddCollections();
            if (UIModsNew.Instance.OpenedCollections && UIModsNew.Instance.OpenedPath.Count > 0 && UIModsNew.Instance.OpenedPath[0] == UIModsNew.Instance.SelectedCollection.Text.text)
            {
                UIModsNew.Instance.OpenedPath.Clear();
                UIModsNew.Instance.OpenedPath.Add(value);
                UIModsNew.Instance.RecalculatePath();
            }
            return true;
        }
        public void ClickAction(string Name)
        {
            switch (Name)
            {
                case "Config":
                    if (UIModsNew.Instance.SelectedItem == null || UIModsNew.Instance.SelectedItem.mod == null) return;
                    if (ModLoader.TryGetMod(UIModsNew.Instance.SelectedItem.mod.Name, out var mod) && ConfigManager.Configs.ContainsKey(mod) && ConfigManager.Configs[mod].Count > 0)
                    {
                        Main.menuMode = Interface.modConfigListID;
                        Interface.modConfig.openedFromModder = true;
                        Interface.modConfig.modderOnClose = () =>
                        {
                            Main.menuMode = Interface.modsMenuID;
                        };
                        Interface.modConfig.mod = mod;
                        Interface.modConfigList.ModToSelectOnOpen = mod;
                    }
                    return;
                case "CreateFolder":
                    var cfg = DataConfig.Instance;
                    var path = string.Join("/", UIModsNew.Instance.OpenedPath) + "/";
                    if (!path.StartsWith("/")) path = "/" + path;
                    int i = 0;
                    while (true)
                    {
                        var name = path + "New Folder" + (i == 0 ? "" : " (" + i + ")");
                        if (!cfg.Folders.Contains(name))
                        {
                            UIModsNew.Instance.ToRedo.Clear();
                            UIModsNew.Instance.ToUndo.Add((UIModsNew.UndoRedoEnum.ChangedFolders, DataConfig.Instance.Folders.ToList()));

                            cfg.Folders.Add(name);
                            cfg.Save();
                            UIModsNew.Instance.UpdateDisplayed();
                            /*
                            UIModsNew.Instance.SelectedItems.Clear();
                            UIModsNew.Instance.SelectedItems.Add(UIModsNew.Instance.uIMods.Find((el) => el.Active && el.mod == null && el.Name == name));
                            UIModsNew.Instance.popupRename.OnApplyCustom = null;
                            UIModsNew.Instance.popupRename.Popup(name.Substring(path.Length), name.Substring(path.Length));
                            UIModsNew.Instance.popupRename.Input._currentString = "";
                            */
                            return;
                        }
                        i++;
                    }
                case "CreateCollection":
                    var cfg1 = DataConfig.Instance;
                    int i1 = 0;
                    while (true)
                    {
                        var name = "New Collection" + (i1 == 0 ? "" : " (" + i1 + ")");
                        if (!cfg1.Collections.ContainsKey(name))
                        {
                            cfg1.Collections[name] = new List<string>();
                            cfg1.Save();
                            UIModsNew.Instance.AddCollections();

                            UIModsNew.Instance.SelectedCollection = UIModsNew.Instance.collecListIn.Elements.Find((el) => ((UIModsCollection)el).Text.text == name) as UIModsCollection;
                            UIModsNew.Instance.popupRename.Popup(UIModsNew.Instance.SelectedCollection.Text.text, UIModsNew.Instance.SelectedCollection.Text.text, RenameCollection);
                            UIModsNew.Instance.popupRename.Input._currentString = "";
                            return;
                        }
                        i1++;
                    }
                case "CreateConfigCollection":
                    var cfg2 = DataConfig.Instance;
                    int i2 = 0;
                    while (true)
                    {
                        var name = "New Config Collection" + (i2 == 0 ? "" : " (" + i2 + ")");
                        if (!cfg2.ConfigCollections.Contains(name))
                        {
                            UIModsConfigCollection.SaveAll(name);
                            cfg2.ConfigCollections.Add(name);
                            cfg2.Save();
                            UIModsNew.Instance.AddConfigCollections();
                            return;
                        }
                        i2++;
                    }
                case "RenameConfigCollection":
                    var t = Target as UIModsConfigCollection;
                    UIModsNew.Instance.popupRename.Popup(t.Text.text, t.Text.text, (value) =>
                    {
                        var cfg = DataConfig.Instance;
                        if (cfg.ConfigCollections.Contains(value)) return false;
                        try
                        {
                            UIModsConfigCollection.Rename(t.Text.text, value);
                        }
                        catch (Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show((e is IOException) ? "Access to the collection path is denied.\nYou can run game as admin to fix that." : e.ToString(), "Failed To Rename Config", icon: System.Windows.Forms.MessageBoxIcon.Error);
                            ModManager.OpenFolder(Path.Combine(ConfigManager.ModConfigPath, "Collection_" + t.Text.text));
                            ModManager.OpenFolder(Path.Combine(ConfigManager.ModConfigPath, "Collection_" + value));
                        }
                        cfg.ConfigCollections.Remove(t.Text.text);
                        cfg.ConfigCollections.Add(value);
                        cfg.Save();
                        UIModsNew.Instance.AddConfigCollections();
                        return true;
                    });
                    return;
                case "DeleteConfigCollection":
                    var t2 = Target as UIModsConfigCollection;
                    try
                    {
                        UIModsConfigCollection.Delete(t2.Text.text);
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show((e is IOException) ? "Access to the collection path is denied.\nYou can run game as admin to fix that." : e.ToString(), "Failed To Delete Config", icon: System.Windows.Forms.MessageBoxIcon.Error);
                        ModManager.OpenFolder(Path.Combine(ConfigManager.ModConfigPath, "Collection_" + t2.Text.text));
                    }
                    DataConfig.Instance.ConfigCollections.Remove((Target as UIModsConfigCollection).Text.text);
                    DataConfig.Instance.Save();
                    UIModsNew.Instance.AddConfigCollections();
                    return;
                case "Info":
                    if (UIModsNew.Instance.SelectedItem == null) return;
                    UIModsNew.Instance.ShowInfo();
                    return;
                case "Rename":
                    if (UIModsNew.Instance.SelectedItem == null) return;
                    UIModsNew.Instance.popupRename.Popup(UIModsNew.Instance.SelectedItem.mod?.DisplayNameClean ?? UIModsNew.Instance.SelectedItem.Name, UIModsNew.Instance.SelectedItem.Name);
                    return;
                case "RenameCollection":
                    if (UIModsNew.Instance.SelectedCollection == null) return;
                    UIModsNew.Instance.popupRename.Popup(UIModsNew.Instance.SelectedCollection.Text.text, UIModsNew.Instance.SelectedCollection.Text.text, RenameCollection);
                    return;
                case "Delete":
                    UIModsNew.Instance.popupSureDelete.Popup();
                    return;
                case "DeleteCollection":
                    if (UIModsNew.Instance.SelectedCollection == null) return;
                    DataConfig.Instance.Collections.Remove(UIModsNew.Instance.SelectedCollection.Text.text);
                    if (UIModsNew.Instance.OpenedCollections && UIModsNew.Instance.OpenedPath.Count != 0 && UIModsNew.Instance.OpenedPath[0] == UIModsNew.Instance.SelectedCollection.Text.text)
                    {
                        UIModsNew.Instance.OpenedPath.Clear();
                        UIModsNew.Instance.OpenedCollections = false;
                        UIModsNew.Instance.SelectedCollection = null;
                        UIModsNew.Instance.RecalculatePath();
                    }
                    DataConfig.Instance.Save();
                    UIModsNew.Instance.AddCollections();
                    return;
                case "Remove":
                    if (!UIModsNew.Instance.OpenedCollections || UIModsNew.Instance.OpenedPath.Count == 0) return;
                    foreach (var item in UIModsNew.Instance.SelectedItems)
                    {
                        DataConfig.Instance.Collections[UIModsNew.Instance.OpenedPath[0]].Remove(item.mod.Name);
                    }
                    UIModsNew.Instance.UpdateDisplayed();
                    UIModsNew.Instance.AddCollections();
                    return;
                case "Enable":
                    foreach (var item in UIModsNew.Instance.SelectedItems)
                    {
                        if (item.mod != null) item.Set(true);
                    }
                    return;
                case "Disable":
                    foreach (var item in UIModsNew.Instance.SelectedItems)
                    {
                        if (item.mod != null) item.Set(false);
                    }
                    return;
                case "Lock":
                    foreach (var item in UIModsNew.Instance.SelectedItems)
                    {
                        if (item.mod != null) item.Lock(true);
                    }
                    return;
                case "Unlock":
                    foreach (var item in UIModsNew.Instance.SelectedItems)
                    {
                        if (item.mod != null) item.Lock(false);
                    }
                    return;
            }
        }
        public void AddSeparator()
        {
            var e = new UIImage(TextureAssets.MagicPixel)
            {
                ScaleToFit = true,
                Width = { Precent = 1 },
                Height = { Pixels = 5 },
                Top = { Pixels = posY + 2 },
                Color = Color.Black
            };
            posY += 9;
            Append(e);
        }
        public void AddAction(string Name, bool inactive = false)
        {
            var act = new UIModsContextMenuAction();
            act.IgnoresMouseInteraction = act.inactive = inactive;
            act.Name = Name;
            act.Text = Name.Replace("DeleteCollection", "Delete").Replace("RenameCollection", "Rename").Replace("DeleteConfigCollection", "Delete").Replace("RenameConfigCollection", "Rename");
            act.Top.Pixels = posY;
            var n = Name;
            act.OnLeftClick += (e, l) =>
            {
                ClickAction(n);
                resizing = false;
                Top.Pixels = -100000000;
                Recalculate();
            };
            Height.Pixels += 32;
            Width.Pixels = Math.Max(Width.Pixels, 24 + FontAssets.MouseText.Value.MeasureString(Language.GetTextValue("Mods.ModManager." + act.Text)).X);
            posY += 32;
            Actions.Add(act);
            Append(act);
            act.Activate();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            OnDraw?.Invoke();
            base.DrawSelf(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsMouseHovering) timeout = 0.5f;
            else timeout -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeout <= 0 && Top.Pixels > -100000)
            {
                resizing = false;
                Top.Pixels = -100000000;
                Recalculate();
            }
        }
    }
    public class UIModsContextMenuAction : UIPanelStyled
    {
        public bool inactive;
        public string Name;
        public string Text;
        public UITextDots<LocalizedText> Txt;
        public override void OnInitialize()
        {
            PaddingBottom = PaddingTop = PaddingLeft = PaddingRight = 0;
            Height.Set(32, 0);
            Width.Set(0, 1);
            Txt = new UITextDots<LocalizedText>()
            {
                Width = { Precent = 1, Pixels = -16 },
                Height = { Precent = 1 },
                text = ModManager.Get(Text),
                Top = { Pixels = 4 },
                Left = { Pixels = 8 },
                color = inactive ? new Color(0.85f, 0.85f, 0.85f) : Color.White
            };
            Append(Txt);
            if (inactive)
            {
                BackgroundColor = Color.Lerp(new Color(63, 82, 151) * 0.25f, Color.DarkGray, 0.6f);
                BackgroundColor.A = 64;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (inactive) return;
            BackgroundColor = IsMouseHovering ? new Color(63, 82, 151) * 0.75f : new Color(63, 82, 151) * 0.25f;
            BorderColor = IsMouseHovering ? Color.Gold : Color.Black;
        }
    }
}
