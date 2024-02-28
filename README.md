# SellMyScrap
Adds a few terminal commands to sell your scrap from the ship. Highly Configurable.

<ins><b>This mod will sell scrap as close to the requested value as possible.</b></ins>

* You must be landed at The Company building to use sell commands.
<br><br>
* This mod will NOT sell Gifts, Shotguns, or Ammo by default.
    * See **Sell Settings** for more info.
<br><br>
* This mod supports excluding custom / modded scrap items from the sell algorithm.
    * See **Advanced Sell Settings** for more info.
<br><br>
* This mod will show how much overtime bonus you will receive on the sell confirmation screen.

## <img src="https://i.imgur.com/TpnrFSH.png" width="20px"> Download

Download [SellMyScrap](https://thunderstore.io/c/lethal-company/p/Zehs/SellMyScrap/) on Thunderstore.

## Terminal Commands
* You must be landed at The Company building to use sell commands.
* Each sell command will sell items based on your config settings.
* Each sell command requires confirmation before selling your scrap.
    * Additional information is given on the confirmation screen.

| Command | Description |
| ----------- | ----------- |
| `sell <amount>` | Will sell scrap for a total of the requested amount. |

* The `sell <amount>` command supports math expressions.

| Command | Description |
| ----------- | ----------- |
| `sell quota` | Will sell scrap to reach the profit quota. |
| `sell all` | Will sell all of your scrap. |

| Command |Description |
| ----------- | ----------- |
| `sell` | Shows a help message for this mod. |
| `view scrap` | Shows a detailed list of all the scrap in the ship. |
| `view config` | Shows your config settings. |
| `edit config` | Edit config settings from the terminal. |

## Config Settings
Use the `edit config` command to edit config settings from the terminal.

**Sell Settings** and **Advanced Sell Settings** will be synced with the host when joining a game.

| Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `sellGifts` | `Boolean` | `false` | Do you want to sell Gifts? |
| `sellShotguns` | `Boolean` | `false` | Do you want to sell Shotguns? |
| `sellAmmo` | `Boolean` | `false` | Do you want to sell Ammo? |
| `sellPickles` | `Boolean` | `true` | Do you want to sell Jar of pickles? |

| Advanced Sell Settings | Setting type |Default value | Example value | Description |
| ----------- | ----------- | ----------- | ----------- | ----------- |
| `sellScrapWorthZero` | `Boolean` | `false` |  | Do you want to sell scrap worth zero? |
| `onlySellScrapOnFloor` | `Boolean` | `false` |  | Do you want to only sell scrap that is on the floor? |
| `dontSellListJson` | `String` | `[]` | `["Maxwell", "Other Item"]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to not sell. |

* Use the `edit config` command to easily edit the `dontSellListJson` config setting from the terminal.
* Use the `view scrap` command or [Echo Scanner](https://lethal-company.fandom.com/wiki/Scanner) to see the correct item names to use.
* Item names are not case-sensitive and spaces do matter.

| Terminal Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `overrideWelcomeMessage` | `Boolean` | `true` | Overrides the terminal welcome message to add additional info. |
| `overrideHelpMessage` | `Boolean` | `true` | Overrides the terminal help message to add additional info. |
| `showFoundItems` | `Boolean` | `true` | Show found items on the confirmation screen. |
| `sortFoundItems` | `Boolean` | `true` | Sorts found items from most to least expensive. |
| `alignFoundItemsPrice` | `Boolean` | `true` | Align all prices of found items. |

| Misc Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `speakInShip` | `Boolean` | `true` | The Company will speak inside your ship after selling from the terminal. |

## Bug Reports, Help, or Suggestions
https://github.com/ZehsTeam/Lethal-Company-SellMyScrap/issues

| Discord server | Channel | Post |
| ----------- | ----------- | ----------- |
| [Lethal Company modding Discord](https://discord.gg/XeyYqRdRGC) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1168655651455639582/1197731003800760320) |
| [Unofficial Lethal Company Community](https://discord.gg/nYcQFEpXfU) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1169792572382773318/1198746789185069177) |

## Screenshots
<div>
    <img src="https://i.imgur.com/ieTZCez.png" width="273px">
    <img src="https://i.imgur.com/atzmgX8.png" width="273px">
    <img src="https://i.imgur.com/fzewHLU.png" width="273px">
</div>
<h4><code>sell &lt;amount&gt;</code></h4>
<div>
    <img src="https://i.imgur.com/W62VoLV.png" width="412px">
    <img src="https://i.imgur.com/sBWwG3v.png" width="412px">
</div>
<h4><code>sell quota</code></h4>
<div>
    <img src="https://i.imgur.com/KzCHE6X.png" width="412px">
    <img src="https://i.imgur.com/6BTUSBX.png" width="412px">
</div>
<h4><code>sell all</code></h4>
<div>
    <img src="https://i.imgur.com/HByklOA.png" width="412px">
    <img src="https://i.imgur.com/5tcgCKR.png" width="412px">
</div>
<div>
<h4><code>view scrap</code></h4>
<div>
    <img src="https://i.imgur.com/bsfeVpk.png" width="100%">
</div>
<h4><code>view config</code></h4>
<div>
    <img src="https://i.imgur.com/UBalPcm.png" width="100%">
</div>
<h4><code>edit config</code></h4>
<div>
    <img src="https://i.imgur.com/lHh2s2u.png" width="412px">
    <img src="https://i.imgur.com/e3Z3ODJ.png" width="412px">
</div>
