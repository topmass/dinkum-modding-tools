using BepInEx;
using BepInEx.Logging;
using System.IO;
using System.Text;
using UnityEngine;

namespace ItemIdDumper
{
    [BepInPlugin("com.dinkummods.itemiddumper", "Item ID Dumper", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private bool hasDumped = false;
        private bool wasInGame = false;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Item ID Dumper loaded! Will auto-dump when you enter a world.");
        }

        private void Update()
        {
            bool isInGame = Inventory.Instance != null && WorldManager.Instance != null;

            if (isInGame && !wasInGame && !hasDumped)
            {
                Log.LogInfo("World loaded - starting auto-dump...");
                DumpAllIds();
                hasDumped = true;
            }

            wasInGame = isInGame;

            if (Input.GetKeyDown(KeyCode.F9))
            {
                DumpAllIds();
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                hasDumped = false;
                Log.LogInfo("Auto-dump reset - will dump again on next world load, or press F9.");
            }
        }

        private void DumpAllIds()
        {
            if (Inventory.Instance == null || WorldManager.Instance == null)
            {
                Log.LogError("Game not fully loaded yet! Wait until you're in-game.");
                return;
            }

            Log.LogInfo("Starting ID dump...");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Dinkum Item IDs - Extracted from Game");
            sb.AppendLine("# Generated: " + System.DateTime.Now);
            sb.AppendLine("# Total Inventory Items: " + Inventory.Instance.allItems.Length);
            sb.AppendLine("# Total TileObjects: " + WorldManager.Instance.allObjects.Length);
            sb.AppendLine();

            sb.AppendLine("## Container Items");
            sb.AppendLine();
            sb.AppendLine("| Inventory ID | Item Name | TileObject ID | Container Type | Slots |");
            sb.AppendLine("|--------------|-----------|---------------|----------------|-------|");

            int containerCount = 0;
            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null || item.placeable == null) continue;

                TileObject tileObj = item.placeable;
                if (tileObj.tileObjectChest == null) continue;

                string containerType = GetContainerType(tileObj.tileObjectChest);
                int slots = 24;

                sb.AppendLine("| " + i + " | " + item.itemName + " | " + tileObj.tileObjectId + " | " + containerType + " | " + slots + " |");
                containerCount++;

                Log.LogInfo("CONTAINER: [" + i + "] " + item.itemName + " -> TileObj:" + tileObj.tileObjectId + " (" + containerType + ")");
            }

            sb.AppendLine();
            sb.AppendLine("*Total containers found: " + containerCount + "*");
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## All Placeable/Furniture Items");
            sb.AppendLine();
            sb.AppendLine("| Inventory ID | Item Name | TileObject ID | Value | Is Furniture |");
            sb.AppendLine("|--------------|-----------|---------------|-------|--------------|");

            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null || item.placeable == null) continue;

                sb.AppendLine("| " + i + " | " + item.itemName + " | " + item.placeable.tileObjectId + " | " + item.value + " | " + item.isFurniture + " |");
            }

            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## All Inventory Items");
            sb.AppendLine();
            sb.AppendLine("| ID | Item Name | Value | Stackable | Is Tool | Has Placeable |");
            sb.AppendLine("|----|-----------|-------|-----------|---------|---------------|");

            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null) continue;

                bool hasPlaceable = item.placeable != null;
                sb.AppendLine("| " + i + " | " + item.itemName + " | " + item.value + " | " + item.isStackable + " | " + item.isATool + " | " + hasPlaceable + " |");
            }

            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Consumable Items (Food)");
            sb.AppendLine();
            sb.AppendLine("| ID | Item Name | Stamina | Health | Duration |");
            sb.AppendLine("|----|-----------|---------|--------|----------|");

            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null || item.consumeable == null) continue;

                Consumeable c = item.consumeable;
                sb.AppendLine("| " + i + " | " + item.itemName + " | " + c.staminaGain + " | " + c.healthGain + " | " + c.durationSeconds + "s |");
            }

            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Tools");
            sb.AppendLine();
            sb.AppendLine("| ID | Item Name | Damage | Has Fuel | Fuel Max |");
            sb.AppendLine("|----|-----------|--------|----------|----------|");

            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null || !item.isATool) continue;

                sb.AppendLine("| " + i + " | " + item.itemName + " | " + item.damagePerAttack + " | " + item.hasFuel + " | " + item.fuelMax + " |");
            }

            string gamePath = Path.Combine(Application.persistentDataPath, "DUMPED_ITEM_IDS.md");
            string modPath = Path.Combine(Application.dataPath, "..", "BepInEx", "DUMPED_ITEM_IDS.md");

            File.WriteAllText(gamePath, sb.ToString());
            Log.LogInfo("IDs saved to: " + gamePath);

            try
            {
                File.WriteAllText(modPath, sb.ToString());
                Log.LogInfo("IDs also saved to: " + modPath);
            }
            catch
            {
                Log.LogWarning("Could not save to BepInEx folder");
            }

            Log.LogInfo("=== DUMP COMPLETE ===");
            Log.LogInfo("Found " + containerCount + " containers");
            Log.LogInfo("Press F9 to dump again, F10 to reset auto-dump");
        }

        private string GetContainerType(ChestPlaceable chest)
        {
            if (chest.isStash) return "Stash";
            if (chest.isFishPond) return "FishPond";
            if (chest.isBugTerrarium) return "BugTerrarium";
            if (chest.isAutoSorter) return "AutoSorter";
            if (chest.isAutoPlacer) return "AutoPlacer";
            if (chest.isMannequin) return "Mannequin";
            if (chest.isToolRack) return "ToolRack";
            if (chest.isDisplayStand) return "DisplayStand";
            return "StandardChest";
        }
    }
}
