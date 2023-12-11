using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using MonoMod.RuntimeDetour;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static LethalLib.Modules.Items;

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
        private static bool itemsAdded = false;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"firearmlicense v. {firearmlicense.PluginInfo.PLUGIN_VERSION}");
            Log.LogInfo($"Allows you to purchase Nutcracker shotguns from the store");

            gunPrice = Config.Bind<int>("Shotgun", "StorePrice", 1000, "Store price for the Nutcracker shotgun");
            gunEnabled = Config.Bind<bool>("Shotgun", "Enabled", true, "If true, the shotgun will be available in store");
            shellPrice = Config.Bind<int>("Shotgun", "ShellStorePrice", 200, "Store price for the shotgun shells");
            shellEnabled = Config.Bind<bool>("Shotgun", "ShellEnabled", true, "If true, the shotgun shells will be available in store");

            new Harmony(firearmlicense.PluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void OnRoundAwakens(Terminal terminal)
        {
            if (itemsAdded)
            {
                return;
            }

            var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
            var confirmKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "confirm");
            var denyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "deny");
            var itemsList = terminal.buyableItemsList.ToList();

            if (gunEnabled.Value)
            {
                Item shotgun = Item.Instantiate(getItem("Shotgun"));
                shotgun.name = "PurchasableShotgun";
                shotgun.isScrap = false;
                shotgun.creditsWorth = gunPrice.Value;
                if (shotgun != null)
                {
                    itemsList.Add(shotgun);

                    var confirmationNode = ScriptableObject.CreateInstance<TerminalNode>();
                    confirmationNode.name = $"ShotgunBuyNode2";
                    confirmationNode.displayText = "Ordered [variableAmount] shotguns. Your new balance is [playerCredits].\n\n" +
                        "Our contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                    confirmationNode.clearPreviousText = true;
                    confirmationNode.maxCharactersToType = 15;
                    confirmationNode.buyItemIndex = itemsList.Count - 1;
                    confirmationNode.isConfirmationNode = false;
                    confirmationNode.itemCost = gunPrice.Value;
                    confirmationNode.playSyncedClip = 0;

                    var orderNode = ScriptableObject.CreateInstance<TerminalNode>();
                    orderNode.name = $"ShotgunBuyNode1";
                    orderNode.displayText = $"You have requested to order shotguns. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    orderNode.clearPreviousText = true;
                    orderNode.maxCharactersToType = 35;
                    orderNode.buyItemIndex = itemsList.Count - 1;
                    orderNode.isConfirmationNode = true;
                    orderNode.overrideOptions = true;
                    orderNode.itemCost = gunPrice.Value;
                    orderNode.terminalOptions = new CompatibleNoun[2]
                    {
                        new CompatibleNoun()
                        {
                            noun = confirmKeyword,
                            result = confirmationNode
                        },
                        new CompatibleNoun()
                        {
                            noun = denyKeyword,
                            result = cancelPurchaseNode
                        }
                    };
                    var keyword = TerminalUtils.CreateTerminalKeyword("shotgun", defaultVerb: buyKeyword);
                    var allKeywords = terminal.terminalNodes.allKeywords.ToList();
                    allKeywords.Add(keyword);
                    terminal.terminalNodes.allKeywords = allKeywords.ToArray();

                    var nouns = buyKeyword.compatibleNouns.ToList();
                    nouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = orderNode
                    });
                    buyKeyword.compatibleNouns = nouns.ToArray();

                    var itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                    itemInfo.name = $"ShotgunInfoNode";
                    itemInfo.displayText = $"The Company takes no liability for casualties inflicted by this item. Remember the 5 firearm handling rules: [INFORMATION EXPUNGED]\n\n";
                    itemInfo.clearPreviousText = true;
                    itemInfo.maxCharactersToType = 25;
                    terminal.terminalNodes.allKeywords = allKeywords.ToArray();
                    var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                    itemInfoNouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = itemInfo
                    });
                    infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
                    Log.LogInfo($"shotgun added to the store with price {gunPrice.Value}");
                    Log.LogInfo($"{Items.shopItems.Count} items now");
                }
            }
            if (shellEnabled.Value)
            {
                Item shells = Item.Instantiate(getItem("GunAmmo"));
                shells.name = "PurchasableShells";
                shells.isScrap = false;
                shells.creditsWorth = shellPrice.Value;
                if (shells != null)
                {
                    itemsList.Add(shells);

                    var confirmationNode = ScriptableObject.CreateInstance<TerminalNode>();
                    confirmationNode.name = $"ShellsBuyNode2";
                    confirmationNode.displayText = "Ordered [variableAmount] shotgun ammo. Your new balance is [playerCredits].\n\n" +
                        "Our contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                    confirmationNode.clearPreviousText = true;
                    confirmationNode.maxCharactersToType = 15;
                    confirmationNode.buyItemIndex = itemsList.Count - 1;
                    confirmationNode.isConfirmationNode = false;
                    confirmationNode.itemCost = shellPrice.Value;
                    confirmationNode.playSyncedClip = 0;

                    var orderNode = ScriptableObject.CreateInstance<TerminalNode>();
                    orderNode.name = $"ShellsBuyNode1";
                    orderNode.displayText = $"You have requested to order shotgun ammo. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    orderNode.clearPreviousText = true;
                    orderNode.maxCharactersToType = 35;
                    orderNode.buyItemIndex = itemsList.Count - 1;
                    orderNode.isConfirmationNode = true;
                    orderNode.overrideOptions = true;
                    orderNode.itemCost = shellPrice.Value;
                    orderNode.terminalOptions = new CompatibleNoun[2]
                    {
                        new CompatibleNoun()
                        {
                            noun = confirmKeyword,
                            result = confirmationNode
                        },
                        new CompatibleNoun()
                        {
                            noun = denyKeyword,
                            result = cancelPurchaseNode
                        }
                    };
                    var keyword = TerminalUtils.CreateTerminalKeyword("ammo", defaultVerb: buyKeyword);
                    var allKeywords = terminal.terminalNodes.allKeywords.ToList();
                    allKeywords.Add(keyword);
                    terminal.terminalNodes.allKeywords = allKeywords.ToArray();

                    var nouns = buyKeyword.compatibleNouns.ToList();
                    nouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = orderNode
                    });
                    buyKeyword.compatibleNouns = nouns.ToArray();

                    var itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                    itemInfo.name = $"ShellInfoNode";
                    itemInfo.displayText = $"Ammo required for the shotgun to remain a practical weapon.\n\n";
                    itemInfo.clearPreviousText = true;
                    itemInfo.maxCharactersToType = 25;
                    terminal.terminalNodes.allKeywords = allKeywords.ToArray();
                    var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                    itemInfoNouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = itemInfo
                    });
                    infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
                    Log.LogInfo($"shells added to the store with price {shellPrice.Value}");
                    Log.LogInfo($"{Items.shopItems.Count} items now");
                }
            }
            terminal.buyableItemsList = itemsList.ToArray();
            itemsAdded = true;
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

    [HarmonyPatch(typeof(Terminal))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AfterRoundAwakens(Terminal __instance)
        {
            Plugin.OnRoundAwakens(__instance);
        }
    }
}
