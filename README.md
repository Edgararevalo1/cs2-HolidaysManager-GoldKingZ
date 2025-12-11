## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] HolidaysManager-GoldKingZ (1.0.0)

[✔] - Christmas

[ ] - Halloween

[ ] - Easter Egg

[ ] - Valentine’s Day


![HolidaysManager-GoldKingZ](https://github.com/user-attachments/assets/a55f096e-2b9c-49cc-96ab-45308ecb7700)




---

## 📦 Dependencies

[![Metamod:Source](https://img.shields.io/badge/Metamod:Source-2d2d2d?logo=sourceengine)](https://www.sourcemm.net)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-83358F)](https://github.com/roflmuffin/CounterStrikeSharp)


[![MySQL](https://img.shields.io/badge/MySQL-4479A1?logo=mysql&logoColor=white)](https://dev.mysql.com/doc/connector-net/en/) [Included in zip]

[![JSON](https://img.shields.io/badge/JSON-000000?logo=json)](https://www.newtonsoft.com/json) [Included in zip]

[![ValvePak](https://img.shields.io/badge/ValvePak-181717?logo=github&logoColor=white)](https://github.com/ValveResourceFormat/ValvePak) [Included in zip]

[![GeoLite2-City.mmdb](https://img.shields.io/badge/GeoLite2--City.mmdb-181717?logo=github&logoColor=white)](https://github.com/P3TERX/GeoLite.mmdb) [Included in zip]

[![MaxMind.Db](https://img.shields.io/badge/MaxMind.Db-2A4365?logo=database&logoColor=white)](https://www.nuget.org/packages/MaxMind.Db) [Included in zip]

[![MaxMind.GeoIP2](https://img.shields.io/badge/MaxMind.GeoIP2-2A4365?logo=database&logoColor=white)](https://www.nuget.org/packages/MaxMind.GeoIP2) [Included in zip]

---

## 📥 Installation

### Plugin Installation
1. Download the latest `HolidaysManager-GoldKingZ.x.x.x.zip` release
2. Extract contents to your `csgo` directory
3. Configure settings in `HolidaysManager-GoldKingZ/config/config.json`
4. Add Our Workshop `3621476366` Into `multiaddonmanager.cfg` 
4. Restart your server

---

## ⚙️ Configuration

> [!IMPORTANT]
> **Main Configuration**  
> `../HolidaysManager-GoldKingZ/config/config.json`  


## 🛠️ `config/config.json`

<details open>
<summary><b>Main Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `AutoSetPlayerLanguage` | Auto set player language based on country | `true`/`false` | - |
| `Reload_HolidaysManager_CommandsInGame` | Commands to reload plugin | `Console_Commands: css_reloadholidays,css_reloadholiday \| Chat_Commands:` | - |
| `Reload_HolidaysManager_Flags` | Restricted flags for reload command | `SteamIDs: 76561198206086993,STEAM_0:1:507335558 \| Flags: @css/root,@css/admin \| Groups: #css/root,#css/admin` | `Reload_HolidaysManager_CommandsInGame` |
| `Reload_HolidaysManager_Hide` | Hide chat after reload command | `0`-No<br>`1`-Only after success<br>`2`-Always hide | `Reload_HolidaysManager_CommandsInGame` |

</details>

<details>
<summary><b>SkyBox Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `SkyBox_Path` | Path of SkyBox | e.g., `materials/goldkingz/skybox/skybox_snow.vmat`<br>`""` = Disable | - |
| `SkyBox_Color` | SkyBox color in RGBA | Format: `255, 255, 255, 255` | - |
| `SkyBox_Brightness` | SkyBox brightness | Float value, e.g., `0.5` | - |
| `SkyBox_Rotation_XYZ` | SkyBox rotation | Format: `X:  \| Y: 100 \| Z: ` | - |

</details>

<details>
<summary><b>SnowFall Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `SnowFall` | Enable SnowFall | `0`-No<br>`1`-Yes<br>`2`-Togglable, enabled by default<br>`3`-Togglable, disabled by default | - |
| `SnowFall_Path` | Path of snow particle effect | e.g., `particles/goldkingz/snowing/snowing.vpcf`<br>`""` = Disable | `SnowFall = 1,2,3` |
| `SnowFall_Z_Height` | Height of snow on Z-axis | Integer, e.g., `300` | `SnowFall = 1,2,3` |
| `SnowFall_CommandsInGame` | Commands to toggle snow | `Console_Commands: css_snow,css_snowfall \| Chat_Commands:` | `SnowFall = 2,3` |
| `SnowFall_Flags` | Restricted flags for toggle | `SteamIDs: \| Flags: @css/vip,@css/vips \| Groups: #css/vips,#css/vip` | `SnowFall_CommandsInGame` |
| `SnowFall_Hide` | Hide chat after toggle | `0`-No<br>`1`-Only after success<br>`2`-Always hide | `SnowFall_CommandsInGame` |

</details>

<details>
<summary><b>Prop Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `Props` | Enable Props | `0`-No<br>`1`-Yes<br>`2`-Togglable, enabled by default<br>`3`-Togglable, disabled by default | - |
| `Props_CommandsInGame` | Commands to toggle props | `Console_Commands: css_prop,css_props \| Chat_Commands:` | `Props = 2,3` |
| `Props_Flags` | Restricted flags for toggle | `SteamIDs: \| Flags: @css/vip,@css/vips \| Groups: #css/vips,#css/vip` | `Props_CommandsInGame` |
| `Props_Hide` | Hide chat after toggle | `0`-No<br>`1`-Only after success<br>`2`-Always hide | `Props_CommandsInGame` |
| `Props_Edit_Mode_CommandsInGame` | Commands to enter edit mode | `Console_Commands: css_edit,css_editmode \| Chat_Commands:` | - |
| `Props_Edit_Mode_Flags` | Restricted flags for edit mode | `SteamIDs: 76561198206086993,STEAM_0:1:507335558 \| Flags: @css/root,@css/admin \| Groups: #css/root,#css/admin` | `Props_Edit_Mode_CommandsInGame` |
| `Props_Edit_Mode_Hide` | Hide chat after edit command | `0`-No<br>`1`-Only after success<br>`2`-Always hide | `Props_Edit_Mode_CommandsInGame` |
| `Props_Mode2And3_Max` | Max snowballs for prop modes 2/3 | `0`-Unlimited<br>`1+`-Limited count | - |
| `Props_Mode2And3_Refill_CoolDown` | Cooldown for snowball refill (seconds) | `0`-No cooldown<br>`1+`-Cooldown in seconds | - |
| `Immunity_From_Cooldown_Props_Mode2And3_Refill` | Immunity from cooldown | `SteamIDs: \| Flags: @css/root,@css/admin \| Groups: #css/root,#css/admin` | - |

</details>

<details>
<summary><b>Decoy to SnowBall Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `SnowBall_Model` | SnowBall model path | e.g., `models/goldkingz/snowball/snowball.vmdl`<br>`""` = Disable | - |
| `SnowBall_Model_ag2` | SnowBall AG2 model path | e.g., `models/goldkingz/snowball/ag2/snowball_ag2.vmdl`<br>`""` = Disable | - |
| `SnowBall_GiveOnSpawn` | Snowballs given on spawn | `0`-None<br>`1+`-Number of snowballs | - |
| `SnowBall_Trail_Particle` | Trail particle path | e.g., `particles/goldkingz/weapons/snowball_trail/snowball_trail.vpcf`<br>`""` = Disable | - |
| `SnowBall_Trail_Particle_Color` | Trail particle color | Format: `255, 255, 255, 255` | - |
| `SnowBall_Hit_Particle` | Hit particle path | e.g., `particles/goldkingz/weapons/snowball_impact/snowball_impact.vpcf`<br>`""` = Disable | - |
| `SnowBall_Hit_Particle_Color` | Hit particle color | Format: `255, 255, 255, 255` | - |
| `SnowBall_Damage` | Snowball damage | `0`-No damage<br>`1+`-Damage amount | - |
| `SnowBall_Hit_Sound` | Hit sound effect | e.g., `Player.SnowballHit` | - |
| `SnowBall_Message_Announcement` | Show throw messages | `0`-No<br>`1`-Team side (exclude bots)<br>`2`-Team side (include bots)<br>`3`-All (exclude bots)<br>`4`-All (include bots)<br>`5`-Player side (teammates enemies)<br>`6`-All (teammates enemies, exclude bots)<br>`7`-All (teammates enemies, include bots) | - |

</details>

<details>
<summary><b>Local Cookies Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `Cookies_Enable` | Save player data locally | `0`-No<br>`1`-On disconnect<br>`2`-On map change | - |
| `Cookies_AutoRemovePlayerOlderThanXDays` | Auto delete inactive players (days) | `0`-Don't delete<br>`1+`-Days | `Cookies_Enable = 1,2` |

</details>

<details>
<summary><b>MySQL Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `MySql_Enable` | Save player data to MySQL | `0`-No<br>`1`-On disconnect<br>`2`-On map change | - |
| `MySql_ConnectionTimeout` | Connection timeout (seconds) | e.g., `30` | `MySql_Enable = 1,2` |
| `MySql_RetryAttempts` | Retry attempts on failure | e.g., `3` | `MySql_Enable = 1,2` |
| `MySql_RetryDelay` | Delay between retries (seconds) | e.g., `2` | `MySql_Enable = 1,2` |
| `MySql_Servers` | MySQL server configurations | Array of server objects | `MySql_Enable = 1,2` |
| `MySql_AutoRemovePlayerOlderThanXDays` | Auto delete inactive players (days) | `0`-Don't delete<br>`1+`-Days | `MySql_Enable = 1,2` |

</details>

<details>
<summary><b>Utilities Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `AutoUpdateSignatures` | Auto update signatures | `true`/`false` | - |
| `AutoUpdateGeoLocation` | Auto update GeoLocation data | `true`/`false` | - |
| `AutoPrecacheResources` | Auto precache resources | `true`/`false` | - |
| `AutoPrecacheResources_Folders` | Folders to auto precache | Array: `["materials","models","particles","scripts"]` | `AutoPrecacheResources = true` |
| `EnableDebug` | Enable debug mode | `true`/`false` | - |

</details>


---

## ❓ Common Issues

<details>
<summary><b>🛠️ Troubleshooting Common Problems</b> (Click to expand 🔽)</summary>

| Issue | Solution |
|-------|----------|
| **Nothing Change Or Error Models** | 1. Check if workshop is downloaded in: `<SERVER>\game\bin\<SERVER_MACHINE>\steamapps\workshop\content\730`<br>2. If downloaded, ensure `AutoPrecacheResources: true`<br>3. If not found, use force download: `gkz_download <WORKSHOP_ID>` or `gkz_forcedownload <WORKSHOP_ID>`<br>4. Wait for download to complete, then restart server |

</details>

---


## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.0.0]
- Initial plugin release

</details>

---
