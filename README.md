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

| Command | Description | Optional flags|
| ----------- | ----------- | ----------- |
| `sell <amount>` | Will sell scrap for a total of the requested amount. | `-se`, `-se:<number>`, `-o` |
| `sell quota` | Will sell scrap to reach the profit quota. | `-se`, `-se:<number>` |
| `sell all` | Will sell all of your scrap. | `-se`, `-se:<number>` |
| `sell item <name>` | Will sell scrap by their item name. | `-se`, `-se:<number>` |

* Using the `-se` flag will spawn a random scrap eater.
    * Usage: `<sell-command> -se`
* Using the `-se:<number>` flag will spawn a scrap eater by their index (Starts at 1).
    * 1 = Octolar, 2 = Takey, 3 = Maxwell
    * Usage: `<sell-command> -se:<number>`

<h4>Additional info for the <code>sell &lt;amount&gt;</code> command.</h4>

* This command supports math expressions as the input for <amount>.
    * Usage example: `sell 500 + 50`
* Using the `-o` flag will sell for a less amount so (less amount + overtime bonus) = initial amount.
    * Usage: `sell <amount> -o`

<h4>Additional info for the <code>sell item &lt;name&gt;</code> command.</h4>

* Item names are not case-sensitive but, spaces do matter.
* Usage examples:
    * `sell item Whoopie cushion`
    * `sell item Whoopie`
    * `sell item Whoo`

| Command |Description |
| ----------- | ----------- |
| `sell` | Shows a help message for this mod. |
| `view scrap` | Shows a list of all the scrap in the ship. |
| `view all scrap` | Shows a list of all the registered scrap. |
| `view config` | Shows your config settings. |
| `edit config` | Edit config settings from the terminal. |

## Config Settings
* Use the `edit config` command to edit config settings from the terminal.
* Only the host can edit **Sell Settings** and **Advanced Sell Settings** using the config editor.
* **Sell Settings** and **Advanced Sell Settings** will be synced with the host.

| Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `sellGifts` | `Boolean` | `false` | Do you want to sell Gifts? |
| `sellShotguns` | `Boolean` | `false` | Do you want to sell Shotguns? |
| `sellAmmo` | `Boolean` | `false` | Do you want to sell Ammo? |
| `sellPickles` | `Boolean` | `true` | Do you want to sell Jar of pickles? |

| Advanced Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `sellScrapWorthZero` | `Boolean` | `false` | Do you want to sell scrap worth zero? |
| `onlySellScrapOnFloor` | `Boolean` | `false` | Do you want to sell scrap that is only on the floor? |
| `dontSellListJson` | `String` | `[]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to not sell. |

<h4>Additional info for the <code>dontSellListJson</code> config setting.</h4>

* Use the `edit config` command to easily edit the `dontSellListJson` config setting from the terminal.
* Use the `view all scrap` command or [Echo Scanner](https://lethal-company.fandom.com/wiki/Scanner) to see the correct item names to use.
* Item names are not case-sensitive but, spaces do matter.
* Example value: `["Maxwell", "Cookie Fumo", "Octolar Plush", "Smol Takey"]`

| Terminal Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `overrideWelcomeMessage` | `Boolean` | `true` | Overrides the terminal welcome message to add additional info. |
| `overrideHelpMessage` | `Boolean` | `true` | Overrides the terminal help message to add additional info. |
| `showFoundItems` | `Boolean` | `true` | Show found items on the confirmation screen. |
| `sortFoundItemsPrice` | `Boolean` | `true` | Sorts found items from most to least expensive. |
| `alignFoundItemsPrice` | `Boolean` | `true` | Aligns all prices of found items. |

| Misc Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `speakInShip` | `Boolean` | `true` | The Company will speak inside your ship after selling from the terminal. |
| `overtimeBonusOffset` | `Int32` | `0` | The overtime bonus offset for the sell confirmation screen. |

| Scrap Eater Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `scrapEaterChance` | `Int32` | `40` | The percent chance a scrap eater will spawn?! |
| `octolarSpawnWeight` | `Int32` | `1` | The spawn chance weight [Octolar](https://www.twitch.tv/thorlar) will spawn?! (scrap eater) |
| `takeySpawnWeight` | `Int32` | `1` | The spawn chance weight [Takey](https://www.twitch.tv/takerst) will spawn?! (scrap eater) |
| `maxwellSpawnWeight` | `Int32` | `1` | The spawn chance weight Maxwell will spawn?! (scrap eater) |

## Bug Reports, Help, or Suggestions
https://github.com/ZehsTeam/Lethal-Company-SellMyScrap/issues

| Discord server | Forum | Post |
| ----------- | ----------- | ----------- |
| [Lethal Company modding Discord](https://discord.gg/XeyYqRdRGC) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1168655651455639582/1197731003800760320) |
| [Unofficial Lethal Company Community](https://discord.gg/nYcQFEpXfU) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1169792572382773318/1198746789185069177) |

## Screenshots
<div>
    <img src="https://i.imgur.com/UyX90Y6.png" width="273px">
    <img src="https://i.imgur.com/lzsWM28.png" width="273px">
    <img src="https://i.imgur.com/aWVozkp.png" width="273px">
</div>
<h4><code>sell &lt;amount&gt;</code></h4>
<div>
    <img src="https://i.imgur.com/apMRYyB.png" width="412px">
    <img src="https://i.imgur.com/UxIFUuj.png" width="412px">
</div>
<h4><code>sell quota</code></h4>
<div>
    <img src="https://i.imgur.com/br7QaA9.png" width="412px">
    <img src="https://i.imgur.com/NJZWJdW.png" width="412px">
</div>
<h4><code>sell all</code></h4>
<div>
    <img src="https://i.imgur.com/oTMQGGt.png" width="412px">
    <img src="https://i.imgur.com/8OsUOZ3.png" width="412px">
</div>
<h4><code>sell item &lt;name&gt;</code></h4>
<div>
    <img src="https://i.imgur.com/8kMD86S.png" width="412px">
    <img src="https://i.imgur.com/ZjSk6sn.png" width="412px">
</div>
<h4><code>view scrap</code></h4>
<div>
    <img src="https://imgur.com/3NL1zPF.png" width="100%">
</div>
<h4><code>view all scrap</code></h4>
<div>
    <img src="https://imgur.com/ZMT7cQE.png" width="412px">
    <img src="https://imgur.com/pyzjL1X.png" width="412px">
</div>
<h4><code>view config</code></h4>
<div>
    <img src="https://i.imgur.com/n8vArOP.png" width="100%">
</div>
<h4><code>edit config</code></h4>
<div>
    <img src="https://i.imgur.com/ZHAjFWm.png" width="412px">
    <img src="https://i.imgur.com/UVLPLMZ.png" width="412px">
</div>

## My Other Mods
| Name | Description |
| ----------- | ----------- |
| [OnlyPresents](https://thunderstore.io/c/lethal-company/p/Zehs/OnlyPresents/) | Moons will only spawn gift boxes. (Server-side) |
| [Hitmarker](https://thunderstore.io/c/lethal-company/p/Zehs/Hitmarker/) | Shows a hitmarker when you successfully hit an enemy. With additional features. (Client-side) |
| [ToilHead](https://thunderstore.io/c/lethal-company/p/Zehs/ToilHead/) | CoilHeads can sometimes spawn with a turret on their head. |
| [CoilHeadSettings](https://thunderstore.io/c/lethal-company/p/Zehs/CoilHeadSettings/) | This mod lets you configure the CoilHead. |
| [OctolarPlush](https://thunderstore.io/c/lethal-company/p/Zehs/OctolarPlush/) | Adds a customizable Octolar Plushie scrap item. |
| [TakeyPlush](https://thunderstore.io/c/lethal-company/p/Zehs/TakeyPlush/) | Adds a customizable Takey Plushie scrap item with some functionalities. 😈 |

<a href="https://thunderstore.io/c/lethal-company/p/Zehs/OnlyPresents/"><img src="https://i.imgur.com/pesSqHI.png" width="80px"></a>
<a href="https://thunderstore.io/c/lethal-company/p/Zehs/Hitmarker/"><img src="https://i.imgur.com/29IA990.png" width="80px"></a>
<a href="https://thunderstore.io/c/lethal-company/p/Zehs/ToilHead/"><img src="https://i.imgur.com/ZNcffJ7.png" width="80px"></a>
<a href="https://thunderstore.io/c/lethal-company/p/Zehs/CoilHeadSettings/"><img src="https://i.imgur.com/QmIID55.png" width="80px"></a>
<a href="https://thunderstore.io/c/lethal-company/p/Zehs/OctolarPlush/"><img src="https://i.imgur.com/3nVYwpO.png" width="80px"></a>
<a href="https://thunderstore.io/c/lethal-company/p/Zehs/TakeyPlush/"><img src="https://i.imgur.com/ENIZdU0.png" width="80px"></a>

You have reached the bottom of the README. Thank you for reading <3