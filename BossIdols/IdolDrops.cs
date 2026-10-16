using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace BossIdols
{
    /// <summary>
    ///     Appends each boss's biome idols (Battle = UpgraderNWeapon, Protection = UpgraderNArmor)
    ///     to the CharacterDrop of its prefab.
    /// </summary>
    internal static class IdolDrops
    {
        // Boss prefab → idol tier. Tier N idols are named "$item_upgrader_tierN" (0 Wooden ... 7 Bloodgold);
        // each biome's treasure chests drop its own tier and the next one up.
        private static readonly (string Boss, int Tier)[] Bosses =
        {
            ("Eikthyr", 0),
            ("gd_king", 1),
            ("Bonemass", 2),
            ("Dragon", 3),
            ("GoblinKing", 4),
            ("SeekerQueen", 5),
            ("Fader", 6),
            // Kall Fimbulbringer: phases FrozenKing and FrozenKing_p2 spawn the next phase as a death
            // effect and drop nothing; only the final phase has the boss loot
            ("FrozenKing_p3", 7),
        };

        // Drops this mod added, so re-applying (world reload, config sync/change) replaces them instead of
        // stacking duplicates. Prefabs survive world reloads, so their m_drops still hold earlier additions.
        private static readonly HashSet<CharacterDrop.Drop> AddedDrops = new HashSet<CharacterDrop.Drop>();

        public static void Apply()
        {
            if (!ZNetScene.instance)
            {
                return;
            }

            bool enabled = BossIdols.Enabled.Value;
            int min = BossIdols.MinPerIdol.Value;
            int max = Mathf.Max(min, BossIdols.MaxPerIdol.Value);
            int added = 0;

            foreach (var (bossName, tier) in Bosses)
            {
                GameObject boss = ZNetScene.instance.GetPrefab(bossName);
                CharacterDrop characterDrop = boss ? boss.GetComponent<CharacterDrop>() : null;
                if (!characterDrop)
                {
                    Logger.LogWarning($"Boss prefab '{bossName}' or its CharacterDrop not found, skipping");
                    continue;
                }

                characterDrop.m_drops.RemoveAll(drop => AddedDrops.Remove(drop));
                if (!enabled)
                {
                    continue;
                }

                foreach (string idolName in new[] { $"Upgrader{tier}Weapon", $"Upgrader{tier}Armor" })
                {
                    GameObject idol = ZNetScene.instance.GetPrefab(idolName);
                    if (!idol)
                    {
                        Logger.LogWarning($"Idol prefab '{idolName}' not found, skipping it for {bossName}");
                        continue;
                    }

                    var drop = new CharacterDrop.Drop
                    {
                        m_prefab = idol,
                        m_amountMin = min,
                        // m_dontScale rolls Random.Range(int, int), whose max is exclusive
                        m_amountMax = max + 1,
                        m_chance = 1f,
                        m_onePerPlayer = false,
                        m_levelMultiplier = false,
                        m_dontScale = true,
                    };
                    characterDrop.m_drops.Add(drop);
                    AddedDrops.Add(drop);
                    added++;
                }
            }

            Logger.LogInfo(enabled
                ? $"Added {added} boss idol drops ({min}-{max} each)"
                : "Disabled; boss idol drops removed");
        }
    }
}
