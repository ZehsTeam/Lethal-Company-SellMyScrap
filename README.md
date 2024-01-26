# SellMyScrap
A Lethal Company mod to easily sell your scrap to The Company using the ship's terminal.

This mod will try to sell scrap as close to the target amount as possible. You might find some rare cases where you might sell over the target amount even if a suitable match is possible due to performance reasons.

* You must be landed at The Company building to use these commands.
* This mod will NOT sell Gifts, Shotguns, Ammo, or Homemade flashbangs by default.
    * See config settings for more info.
 
## ![Download Icon](https://i.imgur.com/TpnrFSH.png) Download

Download [SellMyScrap](https://thunderstore.io/c/lethal-company/p/Zehs/SellMyScrap/) on Thunderstore.

## Terminal Commands
| Command | Description | Example |
| ----------- | ----------- | ----------- |
| `sell <amount>` | Will sell scrap for a total of the amount specified. | `sell 500` |
| `sell-quota` | Will sell scrap to reach the profit quota. | |
| `sell-all` | Will sell all of your scrap. | |

* Each command will sell items based on the config settings.
* Each command requires confirmation before selling your scrap.
* Additional information is given on the confirmation screen.
    * See screenshots for details.

## Config settings
| Sell Settings | Setting type | Default value |
| ----------- | ----------- | ----------- |
| `sellGifts` | `Boolean` | `false` |
| `sellShotguns` | `Boolean` | `false` |
| `sellAmmo` | `Boolean` | `false` |  |
| `sellHomemadeFlashbang` | `Boolean` | `false` |
| `sellPickles` | `Boolean` | `true` |

| Advanced Sell Settings | Setting type |Default value | Example value | Description |
| ----------- | ----------- | ----------- | ----------- | ----------- |
| `dontSellListJson` | `String` | `[]` | `["Gift", "Shotgun", "Ammo"]` | [JSON array](https://www.w3schools.com/js/js_json_arrays.asp) of item names to not sell. |

| Confirmation Settings | Setting type | Default value | Description |
| ----------- | ----------- | ----------- | ----------- |
| `showFoundItems` | `Boolean` | `true` | Show found items on the confirmation screen. |
| `showFoundItemsLimit` | `Int32` | `100` | Won't show founds items if the total item count is over the limit. |
| `alignFoundItemsPrice` | `Boolean` | `true` | Align all prices of found items on the confirmation screen. |

## Screenshots
![Sell 500](https://i.imgur.com/30EUfbH.png)
![Sell 500 Complete](https://i.imgur.com/aHkk7VO.png)