# Card Zones
## About
Adds the ability to create card zones to help organizing your board without increasing the size of the deck.

![Showcase](https://raw.githubusercontent.com/MSchmoecker/CardZones/master/Docs/Showcase.png)

Drag and drop any card onto the "Make Zone" box at the top to create a new zone for this type.

## Manual Installation
This mod requires BepInEx to work, it is a modding framework that allows multiple mods being loaded.
Furthermore, this mod uses Harmony to patch into the game, which means no game code is distributed and allows multiple mods to change it interdependent.

1. Download and install BepInEx from [Thunderstore](https://stacklands.thunderstore.io/package/BepInEx/BepInExPack_Stacklands)
3. Download this mod and extract it into `BepInEx/plugins/`
4. Launch the game!

## Development
1. Install BepInEx
2. Set your `GAME_PATH` in `CardZones.csproj`
3. This mod requires publicized game code, this removes the need to get private members via heavy Reflection code. Use https://github.com/CabbageCrow/AssemblyPublicizer for example to publicize `Stacklands/Stacklands_Data/Managed/GameScripts.dll`
4. Compile the project. This copies the resulting dll into `<GAME_PATH>/BepInEx/plugins/`

## Links
- Github: [https://github.com/MSchmoecker/CardZones](https://github.com/MSchmoecker/CardZones)
- Thunderstore: [https://stacklands.thunderstore.io/package/MSchmoecker/CardZones](https://stacklands.thunderstore.io/package/MSchmoecker/CardZones)
- Nexus: [https://www.nexusmods.com/stacklands/mods/5](https://www.nexusmods.com/stacklands/mods/5)
- [Offical Stacklands Discord](https://discord.gg/sokpop), my Discord tag: Margmas#9562

## Changelog

0.3.1
- Update for Stacklands 2000 DLC

0.3.0
- Added option to make zone cards sticky, if active they won't be pushed around by other cards. Disabled by default
- Changed zones to allow more cards to be stacked, like producing structures
- Slight performance improvements

0.2.0
- Update to support native Stacklands modding support instead of BepInEx

0.1.4
- Updated mod for game version 1.2.3 (dark forest update). This hides everything on the zone card again and also hides the new label on the zone maker

0.1.3
- Updated Thunderstore Readme again

0.1.2
- Updated Thunderstore Readme

0.1.1
- Updated mod for game version 1.1.4 (islands update)
- Show highlight on zone maker only for possible cards
- Hide coin icon on zones

0.1.0
- Initial release
