# SellMyScrap
#### Adds a few terminal commands to sell your scrap from the ship. Highly Configurable. SellFromTerminal +

#### This mod will sell scrap as close to the requested value as possible.

#### This mod is for all clients!
<br>

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
<details>
  <summary>Expand</summary>
<br>

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
| `sell list` | Will sell all the scrap from the `sellListJson` config setting. | `-se`, `-se:<number>` |

* Using the `-se` flag will spawn a random scrap eater.
    * Usage: `<sell-command> -se`
* Using the `-se:<number>` flag will spawn a scrap eater by their index (Starts at 1).
    * 1 = Octolar, 2 = Takey, 3 = Maxwell, 4 = Yippee, 5 = Cookie Fumo, 6 = Psycho, 7 = Zombies
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

<h4>Additional info for the <code>sell list</code> command.</h4>

* This command will sell all the items from the `sellListJson` config setting.
* This command will bypass the `dontSellListJson` config setting.

| Command |Description |
| ----------- | ----------- |
| `sell` | Shows a help message for this mod. |
| `view overtime` | Shows your current overtime bonus. |
| `view scrap` | Shows a list of all the scrap in the ship. |
| `view all scrap` | Shows a list of all the registered scrap. |
| `view config` | Shows your config settings. |
| `edit config` | Edit config settings from the terminal. |

</details>

## Config Settings
<details>
  <summary>Expand</summary>
<br>

* Use the `edit config` command to edit config settings from the terminal.
* Only the host can edit **Sell Settings** and **Advanced Sell Settings** using the config editor.
* **Sell Settings** and **Advanced Sell Settings** will be synced with the host.

| Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `SellGifts` | `Boolean` | `false` | Do you want to sell Gifts? |
| `SellShotguns` | `Boolean` | `false` | Do you want to sell Shotguns? |
| `SellAmmo` | `Boolean` | `false` | Do you want to sell Ammo? |
| `SellKnives` | `Boolean` | `false` | Do you want to sell Kitchen knives? |
| `SellPickles` | `Boolean` | `true` | Do you want to sell Jar of pickles? |

| Advanced Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `SellScrapWorthZero` | `Boolean` | `false` | Do you want to sell scrap worth zero? |
| `OnlySellScrapOnFloor` | `Boolean` | `false` | Do you want to sell scrap that is only on the floor? |
| `DontSellListJson` | `String` | `[]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to not sell. |
| `SellListJson` | `String` | `["Whoopie cushion", "Easter egg", "Tragedy", "Comedy"]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to sell when using the `sell list` command. |

<h4>Additional info for the <code>dontSellListJson</code> config setting.</h4>

* Use the `edit config` command to easily edit the `dontSellListJson` config setting from the terminal.
* Use the `view scrap` or `view all scrap` command to see the correct item names to use.
* Item names are not case-sensitive but, spaces do matter.
* Example value: `["Maxwell", "Cookie Fumo", "Octolar Plush", "Smol Takey"]`

<h4>Additional info for the <code>sellListJson</code> config setting.</h4>

* Use the `edit config` command to easily edit the `sellListJson` config setting from the terminal.
* Use the `view scrap` or `view all scrap` command to see the correct item names to use.
* Item names are not case-sensitive but, spaces do matter.

| Terminal Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `OverrideWelcomeMessage` | `Boolean` | `true` | Overrides the terminal welcome message to add additional info. |
| `OverrideHelpMessage` | `Boolean` | `true` | Overrides the terminal help message to add additional info. |
| `ShowFoundItems` | `Boolean` | `true` | Show found items on the confirmation screen. |
| `SortFoundItemsPrice` | `Boolean` | `true` | Sorts found items from most to least expensive. |
| `AlignFoundItemsPrice` | `Boolean` | `true` | Aligns all prices of found items. |

| Misc Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `SpeakInShip` | `Boolean` | `true` | The Company will speak inside your ship after selling from the terminal. |
| `RareVoiceLineChance` | `Single` | `5` | The percent chance the Company will say a rare microphone voice line after selling. |

| Scrap Eater Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `ScrapEaterChance` | `Int32` | `75` | The percent chance a scrap eater will spawn?! |
| `OctolarSpawnWeight` | `Int32` | `1` | The spawn chance weight [Octolar](https://www.twitch.tv/thorlar) will spawn?! (scrap eater) |
| `TakeySpawnWeight` | `Int32` | `1` | The spawn chance weight [Takey](https://www.twitch.tv/takerst) will spawn?! (scrap eater) |
| `MaxwellSpawnWeight` | `Int32` | `1` | The spawn chance weight Maxwell will spawn?! (scrap eater) |
| `YippeeSpawnWeight` | `Int32` | `1` | The spawn chance weight Yippee will spawn?! (scrap eater) |
| `CookieFumoSpawnWeight` | `Int32` | `1` | The spawn chance weight Cookie Fumo will spawn?! (scrap eater) |
| `PsychoSpawnWeight` | `Int32` | `1` | The spawn chance weight [Psycho](https://www.twitch.tv/psychohypnotic) will spawn?! (scrap eater) |
| `ZombiesSpawnWeight` | `Int32` | `1` | The spawn chance weight [Zombies](https://www.twitch.tv/zombiesatemychannel) will spawn?! (scrap eater) |

</details>

## Developer Contact
#### Report bugs or give suggestions here:
https://github.com/ZehsTeam/Lethal-Company-SellMyScrap/issues

| Discord server | Forum | Post |
| ----------- | ----------- | ----------- |
| [Lethal Company modding Discord](https://discord.gg/XeyYqRdRGC) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1168655651455639582/1197731003800760320) |
| [Unofficial Lethal Company Community](https://discord.gg/nYcQFEpXfU) | `#mod-releases` | [SellMyScrap](https://discord.com/channels/1169792572382773318/1198746789185069177) |

#### Twitch: [CritHaxXoG](https://www.twitch.tv/crithaxxog) - YouTube: [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

## Credits
* [LethalThings](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalThings/) - *Maxwell and Cookie Fumo (models + sounds)*

## Screenshots
<details>
  <summary>Expand</summary>
<br>

<div>
    <img src="https://i.imgur.com/UyX90Y6.png" width="273px">
    <img src="https://i.imgur.com/lzsWM28.png" width="273px">
    <img src="https://i.imgur.com/zyDW9TD.png" width="273px">
</div>
<h4><code>sell &lt;amount&gt;</code></h4>
<div>
    <img src="https://i.imgur.com/BYeYs4d.png" width="412px">
    <img src="https://i.imgur.com/bYQtN1Y.png" width="412px">
</div>
<h4><code>sell quota</code></h4>
<div>
    <img src="https://i.imgur.com/r6SVSBB.png" width="412px">
    <img src="https://i.imgur.com/L1vih92.png" width="412px">
</div>
<h4><code>sell all</code></h4>
<div>
    <img src="https://i.imgur.com/XCz93Yc.png" width="412px">
    <img src="https://i.imgur.com/9eHs2zQ.png" width="412px">
</div>
<h4><code>sell item &lt;name&gt;</code></h4>
<div>
    <img src="https://i.imgur.com/cOQhtLt.png" width="412px">
    <img src="https://i.imgur.com/Z8qRk91.png" width="412px">
</div>
<h4><code>view overtime</code></h4>
<div>
    <img src="https://i.imgur.com/Z6nUhNQ.png" width="412px">
    <img src="https://i.imgur.com/Ff8E5sw.png" width="412px">
</div>
<h4><code>view scrap</code></h4>
<div>
    <img src="https://i.imgur.com/EsoJkSu.png" width="100%">
</div>
<h4><code>view all scrap</code></h4>
<div>
    <img src="https://i.imgur.com/VRSSGmC.png" width="412px">
    <img src="https://i.imgur.com/SuOPV4n.png" width="412px">
</div>
<h4><code>view config</code></h4>
<div>
    <img src="https://i.imgur.com/wzBdRq3.png" width="100%">
</div>
<h4><code>edit config</code></h4>
<div>
    <img src="https://i.imgur.com/8WhcmNE.png" width="273px">
    <img src="https://i.imgur.com/Wkk1Thz.png" width="273px">
    <img src="https://i.imgur.com/lfPwUt2.png" width="273px">
</div>

</details>