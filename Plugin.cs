using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace enemyalert
{
    [BepInPlugin(firearmlicense.PluginInfo.PLUGIN_GUID, firearmlicense.PluginInfo.PLUGIN_NAME, firearmlicense.PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private static ConfigEntry<int> gunPrice;
        private static ConfigEntry<bool> gunEnabled;
        private static ConfigEntry<int> shellPrice;
        private static ConfigEntry<bool> shellEnabled;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"firearmlicense v. {firearmlicense.PluginInfo.PLUGIN_VERSION}");
            Log.LogInfo($"Allows you to purchase Nutcracker shotguns from the store");

            gunPrice = Config.Bind<int>("Shotgun", "StorePrice", 1000, "Store price for the Nutcracker shotgun");
            gunEnabled = Config.Bind<bool>("Shotgun", "Enabled", true, "If true, the shotgun will be available in store");
            shellPrice = Config.Bind<int>("Shotgun", "ShellStorePrice", 200, "Store price for the shotgun shells");
            shellEnabled = Config.Bind<bool>("Shotgun", "ShellEnabled", true, "If true, the shotgun shells will be available in store");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (gunEnabled.Value)
            {
                Item shotgun = Item.Instantiate(getItem("Shotgun"));
                shotgun.name = "PurchasableShotgun";
                shotgun.isScrap = false;
                shotgun.creditsWorth = gunPrice.Value;
                var itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                itemInfo.name = $"ShotgunInfoNode";
                itemInfo.displayText = $"The Company takes no liability for casualties inflicted by this item. Remember the 5 firearm handling rules: [INFORMATION EXPUNGED]\n\n";
                itemInfo.clearPreviousText = true;
                itemInfo.maxCharactersToType = 25;
                Items.RegisterShopItem(shotgun, null, null, itemInfo, gunPrice.Value);
                Log.LogInfo($"shotgun added to the store with price {gunPrice.Value}");
            }
            if (shellEnabled.Value)
            {
                Item shells = Item.Instantiate(getItem("GunAmmo"));
                shells.name = "PurchasableShells";
                shells.isScrap = false;
                shells.creditsWorth = shellPrice.Value;
                var itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                itemInfo.name = $"ShellInfoNode";
                itemInfo.displayText = $"Ammo required for the shotgun to remain a practical weapon.\n\n";
                itemInfo.clearPreviousText = true;
                itemInfo.maxCharactersToType = 25;
                Items.RegisterShopItem(shells, null, null, itemInfo, shellPrice.Value);

                Log.LogInfo($"shells added to the store with price {shellPrice.Value}");
            }
        }

        private static Item getItem(string v)
        {
            foreach (Item it in Resources.FindObjectsOfTypeAll<Item>())
            {
                if (it.name == v) return it;
            }
            return null;
        }
    }
}
