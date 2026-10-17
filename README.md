# BossIdols

Every boss drops its own biome's **Battle Idol** and **Protection Idol** when it dies. Each idol is rolled separately, 1–3 of each by default, with a 100% chance. The boss's normal drops (trophy, key items, etc.) are unchanged.

| Boss | Idols |
|------|-------|
| Eikthyr | Wooden |
| The Elder | Bronze |
| Bonemass | Iron |
| Moder | Silver |
| Yagluth | Black Metal |
| The Queen | Black Marble |
| Fader | Flametal |
| Kall Fimbulbringer | Bloodgold (dropped by the final phase) |

The amount isn't affected by the boss's star level or by world modifier loot settings.

## Installation

Requires [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/). Every player and the server must have the mod (same minor version).

Manual install: put `BossIdols.dll` in `BepInEx/plugins`.

## Configuration

`BepInEx/config/com.voxxsidus.bossidols.cfg`, section `[General]`. All options are admin-only and synced from the server to every client.

| Option | Default | Description |
|--------|---------|-------------|
| `Enabled` | `true` | Whether bosses drop the idols. |
| `MinPerIdol` | `1` | Minimum number of each idol (1–100). |
| `MaxPerIdol` | `3` | Maximum number of each idol (1–100). Values below `MinPerIdol` count as `MinPerIdol`. |

Changes apply right away, including to bosses that are already alive; there's no need to restart or reload the world. The config file is watched, so editing it on a dedicated server takes effect when you save and is pushed to connected players. In-game changes through a configuration manager work the same way.

## Changelog

- 1.0.0: Initial release.

## Building

Built on the [JotunnModStub](https://github.com/Valheim-Modding/JotunnModStub) template. Create an `Environment.props` at the solution root that sets `VALHEIM_INSTALL`, `BEPINEX_PATH` and `MOD_DEPLOYPATH` (see the [Jötunn docs](https://valheim-modding.github.io/Jotunn/guides/overview.html)), then:

```
dotnet build BossIdols.sln -c Debug     # copies the plugin to MOD_DEPLOYPATH
dotnet build BossIdols.sln -c Release   # zips BossIdols/Package for Thunderstore
```
