# SellMyScrap
Easily sell your scrap to The Company using the ship's terminal.

This mod will try to sell scrap as close to the target amount as possible. You might find some rare cases where you might sell over the target amount even if a suitable match is possible due to performance reasons.

* This mod will NOT sell Gifts, Shotguns, or Ammo by default.
    * See config settings for more info.

## ![Download Icon](https://i.imgur.com/TpnrFSH.png) Download

Download [SellMyScrap](https://thunderstore.io/c/lethal-company/p/Zehs/SellMyScrap/) on Thunderstore.

## Terminal Commands
| Command | Description | Usage example |
| ----------- | ----------- | ----------- |
| `sell <amount>` | Will sell scrap for a total of the `<amount>` specified. `<amount>` is a positive integer. | `sell 500` |
| `sell-quota` | Will sell scrap to reach the profit quota. | |
| `sell-all` | Will sell all of your scrap. | |

* Only the host can use these commands for now.
* You must be landed at The Company building to use these commands.
* Each command will sell items based on the config settings.
* Each command requires confirmation before selling your scrap.
* Additional information is given on the confirmation screen.
    * See screenshots for details.

| Command | Description |
| ----------- | ----------- |
| `view-scrap` | Shows a detailed list of all the scrap in the ship on the terminal. |

## Config Settings
| Sell Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `sellGifts` | `Boolean` | `false` | Do you want to sell Gifts? |
| `sellShotguns` | `Boolean` | `false` | Do you want to sell Shotguns? |
| `sellAmmo` | `Boolean` | `false` | Do you want to sell Ammo? |
| `sellHomemadeFlashbang` | `Boolean` | `true` | Do you want to sell Homemade flashbangs? |
| `sellPickles` | `Boolean` | `true` | Do you want to sell Jar of pickles? |

| Advanced Sell Settings | Setting type |Default value | Example value | Description |
| ----------- | ----------- | ----------- | ----------- | ----------- |
| `dontSellListJson` | `String` | `[]` | `["Gift", "Shotgun", "Ammo"]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to not sell. |

* Use the `view-scrap` command or scan in-world to see the correct item names to use.
* Item names are not case-sensitive.
* Spaces do matter for item names.

| Confirmation Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `showFoundItems` | `Boolean` | `true` | Show found items on the confirmation screen. |
| `showFoundItemsLimit` | `Int32` | `100` | Won't show found items if the total item count is over the limit. |
| `sortFoundItems` | `Boolean` | `true` | Sorts found items from most to least expensive on the confirmation screen. |
| `alignFoundItemsPrice` | `Boolean` | `true` | Align all prices of found items on the confirmation screen. |

## Screenshots
<h4><code>sell <amount></code></h4>
<div>
    <img src="https://i.imgur.com/4WjGIrH.png" width="503px">
    <img src="https://i.imgur.com/ZfRkS1c.png" width="503px">
</div>
<h4><code>sell-quota</code></h4>
<div>
    <img src="https://i.imgur.com/R53wn2s.png" width="503px">
    <img src="https://i.imgur.com/XB4MDgh.png" width="503px">
</div>
<h4><code>sell-all</code></h4>
<div>
    <img src="https://i.imgur.com/TwPeAYV.png" width="503px">
    <img src="https://i.imgur.com/zDsPJLG.png" width="503px">
</div>
<div>
<h4><code>view-scrap</code></h4>
<div>
    <img src="https://i.imgur.com/CWnmoZW.png" width="100%">
</div>