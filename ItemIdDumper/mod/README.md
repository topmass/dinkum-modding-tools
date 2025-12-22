# ItemIdDumper Mod

Pre-built mod DLL ready to install.

## Installation

1. Copy `ItemIdDumper.dll` to your Dinkum `BepInEx/plugins/` folder

**Linux:**
```bash
cp ItemIdDumper.dll ~/.steam/steam/steamapps/common/Dinkum/BepInEx/plugins/
```

**Windows:**
```
Copy to: C:\Program Files (x86)\Steam\steamapps\common\Dinkum\BepInEx\plugins\
```

## Usage

1. Launch Dinkum
2. Load a save and enter the world
3. The mod will automatically dump all item IDs when the world loads
4. Check output at:
   - `BepInEx/DUMPED_ITEM_IDS.md`
   - Or your Dinkum save folder

## Manual Controls

- **F9** - Dump IDs again manually
- **F10** - Reset auto-dump (will dump again on next world load)

## Requirements

- BepInEx 6.0.0-pre.1 installed in Dinkum
