using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.Social.Steam;

namespace ModManager.Content
{
    public static class WorkshopHelpMePlease
    {
        public static Task FindingModUpdatesTask;
        public static List<(string, ModPubId_t)> ModsRequireUpdates = new();
        public static bool ModsRequireUpdatesLoading = true;
        public static Action OnCheckedUpdates;
        public static void FindHasModUpdates()
        {
            if (!SteamedWraps.SteamAvailable || FindingModUpdatesTask != null) return;
            OnCheckedUpdates?.Invoke();
            FindingModUpdatesTask = Task.Run(() =>
            {
                FindModUpdatesTask(ref ModsRequireUpdates, ref ModsRequireUpdatesLoading, ref OnCheckedUpdates, ref FindingModUpdatesTask);
            });
        }
        private static void FindModUpdatesTask(ref List<(string, ModPubId_t)> modsNeedUpdate, ref bool modsNeedUpdateLoading, ref Action onYep, ref Task myTask)
        {
            modsNeedUpdateLoading = true;
            modsNeedUpdate.Clear();
            int c = 0;
            try
            {
                List<ModDownloadItem> installedModDownloadItems = Interface.modBrowser.SocialBackend.GetInstalledModDownloadItems();
                foreach (ModDownloadItem item in installedModDownloadItems)
                {
                    item.UpdateInstallState();
                    if (item.NeedUpdate) modsNeedUpdate.Add((item.ModName, item.PublishId));
                }
            }
            catch (Exception e)
            {
                Logging.PublicLogger.Error(e);
            }
            modsNeedUpdateLoading = false;
            onYep?.Invoke();
            myTask = null;
        }
    }
}
