# Dinkum Item ID Dumper

A BepInEx mod that extracts all item IDs from Dinkum and saves them to a markdown file.

## Quick Install

Just want the mod? Copy `mod/ItemIdDumper.dll` to your `BepInEx/plugins/` folder.

## What It Does

When you load into a world, this mod automatically dumps:
- All container items (chests, storage, etc.) with both Inventory ID and TileObject ID
- All placeable/furniture items
- All inventory items with properties
- All consumables with stamina/health values
- All tools with damage/fuel info

Output is saved to `BepInEx/DUMPED_ITEM_IDS.md`

## Folder Structure

```
ItemIdDumper/
├── README.md          # This file
├── mod/
│   ├── ItemIdDumper.dll   # Pre-built mod - just install this
│   └── README.md          # Installation instructions
└── codebase/
    ├── Plugin.cs          # Source code
    ├── ItemIdDumper.csproj
    ├── .gitignore
    └── Libs/
        └── README.md      # Instructions for adding build dependencies
```

## Building From Source

See `codebase/Libs/README.md` for required DLLs, then:

```bash
cd codebase
dotnet build -c Release
```

Output: `codebase/bin/Release/net472/ItemIdDumper.dll`

## Requirements

- Dinkum (Steam)
- BepInEx 6.0.0-pre.1

## License

MIT - Feel free to use, modify, and distribute.
