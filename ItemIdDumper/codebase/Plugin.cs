using BepInEx;
using BepInEx.Logging;
using System.IO;
using System.Text;
using UnityEngine;

namespace ItemIdDumper
{
    [BepInPlugin("topmass.itemiddumper", "Item ID Dumper", "1.2.0")]
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
            sb.AppendLine("# Total Carriables in World: " + (WorldManager.Instance.allCarriables != null ? WorldManager.Instance.allCarriables.Count : 0));
            sb.AppendLine();

            // Section 1: Machines (Processing items - furnaces, grinders, etc.)
            sb.AppendLine("## Machines (Processing Items)");
            sb.AppendLine();
            sb.AppendLine("Items that process/transform other items (furnaces, grinders, cooking stations, etc.)");
            sb.AppendLine();
            sb.AppendLine("| Inventory ID | Item Name | TileObject ID | Machine Type | Uses Wind | Uses Solar |");
            sb.AppendLine("|--------------|-----------|---------------|--------------|-----------|------------|");

            int machineCount = 0;
            for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
            {
                InventoryItem item = Inventory.Instance.allItems[i];
                if (item == null || item.placeable == null) continue;

                TileObject tileObj = item.placeable;
                if (tileObj.tileObjectItemChanger == null) continue;

                ItemDepositAndChanger changer = tileObj.tileObjectItemChanger;
                string machineType = changer.MyVerb.ToString();
                bool usesWind = changer.useWindMill;
                bool usesSolar = changer.useSolar;

                sb.AppendLine("| " + i + " | " + item.itemName + " | " + tileObj.tileObjectId + " | " + machineType + " | " + usesWind + " | " + usesSolar + " |");
                machineCount++;

                Log.LogInfo("MACHINE: [" + i + "] " + item.itemName + " -> TileObj:" + tileObj.tileObjectId + " (" + machineType + ")");
            }

            sb.AppendLine();
            sb.AppendLine("*Total machines found: " + machineCount + "*");
            sb.AppendLine();

            // Section 2: Containers
            sb.AppendLine("---");
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

            // Section 3: All Placeable/Furniture Items
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

            // Section 4: All Inventory Items
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

            // Section 5: Consumables (Food)
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

            // Section 6: Tools
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

            sb.AppendLine();

            // Section 7: Carriables/Pickupables (physical objects players can pick up and throw)
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Carriables / Pickupables");
            sb.AppendLine();
            sb.AppendLine("Physical world objects that players can pick up, carry, and throw.");
            sb.AppendLine("These have collision physics and can break if fragile.");
            sb.AppendLine();
            sb.AppendLine("| Prefab ID | Name | Can Pick Up | Is Investigation | Photo Requestable |");
            sb.AppendLine("|-----------|------|-------------|------------------|-------------------|");

            int carriableCount = 0;
            System.Collections.Generic.HashSet<int> seenPrefabIds = new System.Collections.Generic.HashSet<int>();

            if (WorldManager.Instance.allCarriables != null)
            {
                foreach (PickUpAndCarry carriable in WorldManager.Instance.allCarriables)
                {
                    if (carriable == null) continue;
                    if (seenPrefabIds.Contains(carriable.prefabId)) continue;
                    seenPrefabIds.Add(carriable.prefabId);

                    string carriableName = carriable.GetName();
                    bool canPickUp = carriable.canBePickedUp;
                    bool isInvestigation = carriable.investigationItem;
                    bool photoRequestable = carriable.photoRequestable;

                    sb.AppendLine("| " + carriable.prefabId + " | " + carriableName + " | " + canPickUp + " | " + isInvestigation + " | " + photoRequestable + " |");
                    carriableCount++;

                    Log.LogInfo("CARRIABLE: [" + carriable.prefabId + "] " + carriableName + " (CanPickUp:" + canPickUp + ")");
                }
            }

            sb.AppendLine();
            sb.AppendLine("*Total unique carriable types found: " + carriableCount + "*");
            sb.AppendLine();
            sb.AppendLine("**Note:** Carriables are physical world objects, not inventory items.");
            sb.AppendLine("They have Rigidbody physics for collision. Some are fragile and break on hard impact.");
            sb.AppendLine();

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
            Log.LogInfo("Found " + machineCount + " machines");
            Log.LogInfo("Found " + containerCount + " containers");
            Log.LogInfo("Found " + carriableCount + " unique carriable types");
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
