# v1.15.3

- Removed milk button and uploaded it as a separate mod.

## v1.15.2

- Added milk button.
- Made the April Fools stuff automatic based on the current date.

## v1.15.1

- Tested and verified that this mod works for Lethal Company version 80

## v1.15.0

- Added Glitch?!

## v1.14.4

- Fixed compatibility with the [ShipInventoryUpdated](https://thunderstore.io/c/lethal-company/p/LethalCompanyModding/ShipInventoryUpdated/) mod v2.0.8+

## v1.14.3

- Small fix.

## v1.14.2

- Improved `OnlySellScrapOnFloor` accuracy.

## v1.14.1

- ~~Hopefully improved `OnlySellScrapOnFloor` accuracy.~~

## v1.14.0

- Updated to support v73+

<br><br><br>
<details><summary>Older Versions</summary>

## v1.13.1

- Added missing AssetBundle file.

## v1.13.0

- Changed the `ScrapEaterChance` config setting's default value from `75` to `0`.
  - *This was changed at the request of the High Quota community.*

## v1.12.5

- Removed April Fools update.

## v1.12.4

- April Fools update.

## v1.12.3

- Fixed compatibility with the [ShipInventoryUpdated](https://thunderstore.io/c/lethal-company/p/LethalCompanyModding/ShipInventoryUpdated/) mod v1.2.9+

## v1.12.2

- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.2.6+
- This mod is compatible with the [ShipInventoryUpdated](https://thunderstore.io/c/lethal-company/p/SoftDiamond/ShipInventoryUpdated/) mod v1.2.8+
- Fixed selling items from the ship inventory where trying to sell too many items (over the `Chute max capacity`) would cause the command to time out because no more items were spawning out of the chute.

## v1.12.1

- Fixed trailing comma in the `DontSellList` config setting value not allowing you to use the `sell <amount>`, `sell quota`, and `sell all` commands.

## v1.12.0

- Added support for all game versions v40 to v69+
- Command flags are now case-insensitive.

## v1.11.1

- Small changes.

## v1.11.0

- Added more algorithms to find scrap match (#15)
  - The scrap match algorithms are:
    - 1 → Default (Recommended)
    - 2 → Brute Force
    - 3 → Super Fast (Recommended when selling 10k+)
  - Added command flag `-a:<number>` to some sell commands to select a scrap match algorithm.
    - `<number>` is the index of the scrap match algorithm.
    - *Example: `sell 10000 -a:3`*
- Selling now works on any moon that has a sell desk.
  - This includes the moon Galetry.
- You can now sell scrap items that are placed on PlaceableShipObjects.
- The `view scrap` command now includes scrap items worth $0 when the `SellScrapWorthZero` config setting is false.
- Updated Takey (scrap eater)

## v1.10.2

- Updated Takey (scrap eater)

## v1.10.1

- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.2.0+

## v1.10.0

- Updated Takey (scrap eater)

## v1.9.1

- Fixed the `view overtime` command not working properly.
- Removed patches to fix the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.7 breaking the saving and loading of items.

## v1.9.0

- Removed the `view config` and `edit config` commands.
  - I recommend you use the [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) mod to edit the config settings.
- Updated config settings.
  - Old config setting values will migrate to the new config settings.
- Improved config sync.
- Updated Takey (scrap eater)
- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.7+
- Fixed the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod breaking the sell commands and view scrap command when using an incompatible version.
- Added some patches to fix the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.7 breaking the saving and loading of items.
- Other changes and improvements.

## v1.8.1

- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.6+

## v1.8.0

- Added `PrioritySellList` config setting to **Advanced Sell Settings**.
  - *Description: Array of item names to prioritize when selling.*
- Updated Takey (scrap eater)
- Added Wolfy?!
- Other changes and improvements.

## v1.7.2 + v1.7.3

- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.5+
- Updated Takey (scrap eater)
- Updated Zombies (scrap eater)
- Updated Psycho (scrap eater)
- Small bug fixes.
- Other changes and improvements.

> If you were using the ShipInventory mod and selling items wasn't removing them from the ShipInventory, this has been fixed in this update.

## v1.7.1

- Fixed compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.2+

## v1.7.0

- Added compatibility with the [ShipInventory](https://thunderstore.io/c/lethal-company/p/WarperSan/ShipInventory/) mod v1.1.1+
- Added [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) integration.
- Updated Takey (scrap eater)
- Made a few changes to config settings.
- Bug fixes and improvements.

## v1.6.2

- Updated Takey (scrap eater)
- Small fixes.

## v1.6.1

- Fixed missing texture in Takey (scrap eater)
- Updated Takey (scrap eater)

## v1.6.0

- Improved sell algorithm.
- Made some changes to sell commands output.
- Updated Takey (scrap eater)
- Added `ShowQuotaWarning` config setting.
    - *Description: If enabled, will show a warning when you try to pull the ship's lever when the quota hasn't been fulfilled at the Company building with 0 days left.*

## v1.5.24

- Small changes.

## v1.5.23

- Updated Takey (scrap eater)
- Scrap eaters can now properly eat player ragdolls.
- You can no longer get a scrap eater when selling after you pulled the ship's lever.
- Added a warning if you try to start the ship at the Company building on the last day when the profit quota is not fulfilled.

## v1.5.22

- Small changes.

## v1.5.21

- Added support for vehicles.
- Changed all config settings keys.
    - Previous config values will migrate to the new config settings.

## v1.5.20

- Fixed Takey (scrap eater) audio.

## v1.5.19

- Small update to Octolar (scrap eater)
- Optimized assets.

## v1.5.18

- Updated README.

## v1.5.17

- Fixed rare microphone voice lines not playing at the Company after selling.
- Added `rareVoiceLineChance` config setting.
    - Description: *The percent chance the Company will say a rare microphone voice line after selling.*

## v1.5.16

- Changed `scrapEaterChance` config setting default value from 40 to 75.

## v1.5.15

- Added Zombies?!
- Fixed `sellListJson` in the terminal config editor to work properly.

## v1.5.14

- Added `sell list` command.
    - *Description: Will sell all the scrap from the `sellListJson` config setting.*
- Added `sellListJson` config setting.
    - *Description: [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to sell when using the `sell list` command.*
- Made some changes and additions to scrap eaters.
- Scrap eaters will now only revive dead players if all players are dead.

## v1.5.13

1. Removed `overrideSetNewProfitQuota` config setting.
2. Fixed incompatibility with quota rollover mods.
3. Fixed overtime bonus always being 15 credits more when selling on the last day.

> 1. This config setting is now obsolete.
> 2. This was previously fixed by setting `overrideSetNewProfitQuota` to false.
> 3. The overtime bonus always being 15 credits more was because I was clamping `daysUntilDeadline` to 0 when selling on the last day. There's a vanilla bug that sets `daysUntilDeadline` to either 0 or -1 when selling on the last day because of certain circumstances. I've now learned what these circumstances are and can now accurately calculate the overtime bonus to be shown on the sell confirmation screen.

## v1.5.12

- Improved random percent calculations.
- Small update to Takey (scrap eater)
- Small update to Yippee (scrap eater)
- Improved README and CHANGELOG.
- Added XML file for summaries in scrap eater classes.

## v1.5.11

- Fixed some issues when trying to sell.

## v1.5.10

- Made some changes and additions to scrap eaters.
- Other changes.

## v1.5.9

- Added `psychoSpawnWeight` config setting.
- Made Psycho (scrap eater) available to everyone.

## v1.5.8

- Updated Psycho (scrap eater)

## v1.5.7

- Added Psycho?!
- Added `reset` command to the config editor.
    - This will reset all your config settings to their default value.

## v1.5.6

- The `sell item <name>` command now shows found items when the `showFoundItems` config setting is set to false.
- Made some small changes and additions to scrap eaters.

## v1.5.5

- Renamed `sellKnife` config setting to `sellKnives`.
- Updated all sell commands.
- Updated `view overtime` command.
- Made some small changes and additions to scrap eaters.
- Other small changes.

## v1.5.4

- Added `sellKnife` config setting.
- Added `view overtime` command.

## v1.5.3

- Fixed oversight where `scrapEaterChance` set to 0 has a less than 1% chance to spawn a scrap eater.

## v1.5.2

- Small changes and fixes.

## v1.5.1

- Tested and working in version 50.
- Fixed the DepositItemsDesk microphone audio clip desync.

## v1.5.0

- Made big changes and additions to scrap eaters.
- Fixed Yippee (scrap eater) wings not rendering.
- Added Cookie Fumo?!
- Added `cookieFumoSpawnWeight` config setting.
- Added `overrideSetNewProfitQuota` config setting.
- Fixed the DepositItemsDesk playing the microphone audio twice.

## v1.4.2

- Added Yippee?!
- Added `yippeeSpawnWeight` config setting.
- Made Maxwell slightly more evil :3

## v1.4.1

- Removed `overtimeBonusOffset` config setting.
- Made some changes and additions to scrap eaters.
- Other small changes.

## v1.4.0

- Added support for modders to easily add their own scrap eaters.
- Added Maxwell?!
- Added `maxwellSpawnWeight` config setting.
- Added `-se` / `-se:<number>` flag for all sell commands.
    - Using the `-se` flag will spawn a random scrap eater.
        - Usage: `<sell-command> -se`
    - Using the `-se:<number>` flag will spawn a scrap eater by their index (Starts at 1).
        - 1 = Octolar, 2 = Takey, 3 = Maxwell
        - Usage: `<sell-command> -se:<number>`
- Added `clear all` command to the JsonListEditor (`dontSellListJson` config editor).
- Improved the DisplayCreditsEarning popup.
- Other small changes.

## v1.3.16

- Hopefully fixed the issue where the overtime bonus on the sell confirmation screen was sometimes inaccurate by $15.
- Added `overtimeBonusOffset` config setting.
    - Description: The overtime bonus offset for the sell confirmation screen.
- The `overtimeBonusOffset` config setting will get automatically set after the current quota is completed if the calculated overtime bonus on the sell confirmation screen is wrong.
- Moved `scrapEaterChance`, `octolarSpawnWeight`, and `takeySpawnWeight` config settings to the new **Scrap Eater Settings*- category.
- Other small changes.

## v1.3.15

- Added `sell item <name>` command.
- Added `view all scrap` command.

## v1.3.14

- Made some changes and additions to scrap eaters.
- Other small changes.

## v1.3.13

- Added `-o` flag to the `sell <amount>` command.
    - Using the `-o` flag will sell for a less amount so (less amount + overtime bonus) = initial amount.
    - Usage: `sell <amount> -o`
- Other small changes.

## v1.3.12

- Added Takey?!
- Removed `octolarSpawnChance` config setting.
- Added `scrapEaterChance` config setting.
- Added `octolarSpawnWeight` config setting.
- Added `takeySpawnWeight` config setting.
- The DepositItemsDesk microphone audio clip is now synced with all players.

## v1.3.9 → v1.3.11

- Small changes.

## v1.3.8

- Added Octolar?!

## v1.3.7

- The overtime bonus now additionally shows the total value you will receive on the sell confirmation screen. 
- Changed `sortFoundItems` config setting key to `sortFoundItemsPrice`.

## v1.3.6

- Small changes.

## v1.3.5

- The `sell <amount>` command now supports math expressions.

## v1.3.4

- Improved sell algorithm.
- The sell confirmation screen will now show how much overtime bonus you will receive.
- Added `onlySellScrapOnFloor` config setting.
- Changed description.

## v1.3.3

- Added `edit config` command.
- Changed description.

## v1.3.2

- Added `sellScrapWorthZero` config setting.

## v1.3.1

- Fixed selling not working with specific BepInEx configurations.

## v1.3.0

- Changed custom welcome terminal node message.
- All clients will now see all sold items on the DisplayCreditsEarning popup.
- Improved DisplayCreditsEarning popup.
- Fixed the DisplayCreditsEarning popup smooth scroll.
    - Sometimes scrolls too far down. I will try and fix that in a later update.
- Fixed custom terminal help message to show [numberOfItemsOnRoute].
- Removed `showFoundItemsLimit` config setting.
    - Will add back if anyone has any performance issues with showing too many items.
- Improved terminal sell message when company buying rate is not 100%.
- Improved scrap calculator algorithm.
    - Works better when the company buying rate is not 100%.

## v1.2.5

- Simplified README. Command aliases are still in place.
- Added `overrideWelcomeMessage` and `overrideHelpMessage` config settings.
- Moved some config settings to the new **Terminal Settings*- category.
- Added `view config` command.

## v1.2.4

- Added aliases for commands.
- Added custom welcome and help message for the terminal.
- Added sell help command.
- The host will now get a notification after a client uses a sell command.
- Moved the `speakInShip` config setting to the **Misc Settings*- category.
- Fixed a rare case where the mod would try and sell a little bit under the specified amount.

## v1.2.3

- Removed the `sellHomemadeFlashbang` config setting.
- The Company will now only speak inside the ship after selling from the terminal.

## v1.2.2

- Fixed synced config for clients.
- Fixed sell confirmations so it won't still be active if you quit the terminal before confirming or denying.

## v1.2.1

- Fixed the `view scrap` command.
- Sell commands are no longer only accessible by the host.
- Added `speakInShip` config setting.

## v1.2.0

- Added `view scrap` command.
- Changed terminal sell commands confirmation messages.
- Added synced config for clients.
- The Company will speak inside the ship after selling scrap.

## v1.1.4

- Added more config settings.

## v1.1.3

- Added config settings for selling scrap.
- Adjusted some terminal output messages.

## v1.1.0 → v1.1.2

## v1.0.0

- Initial release.
