using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace BossIdols
{
    /// <summary>
    ///     Appends each boss's biome idols (Battle = UpgraderNWeapon, Protection = UpgraderNArmor)
    ///     to the CharacterDrop of its prefab and of any living instances.
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

                var idols = new List<GameObject>();
                foreach (string idolName in new[] { $"Upgrader{tier}Weapon", $"Upgrader{tier}Armor" })
                {
                    GameObject idol = ZNetScene.instance.GetPrefab(idolName);
                    if (idol)
                    {
                        idols.Add(idol);
                    }
                    else
                    {
                        Logger.LogWarning($"Idol prefab '{idolName}' not found, skipping it for {bossName}");
                    }
                }

                var drops = new List<CharacterDrop.Drop>();
                if (enabled)
                {
                    foreach (GameObject idol in idols)
                    {
                        drops.Add(new CharacterDrop.Drop
                        {
                            m_prefab = idol,
                            m_amountMin = min,
                            // m_dontScale rolls Random.Range(int, int), whose max is exclusive
                            m_amountMax = max + 1,
                            m_chance = 1f,
                            m_onePerPlayer = false,
                            m_levelMultiplier = false,
                            m_dontScale = true,
                        });
                    }
                }

                SetIdolDrops(characterDrop, idols, drops);
                added += drops.Count;

                // Instances copied m_drops from the prefab when they spawned
                int bossHash = bossName.GetStableHashCode();
                foreach (Character character in Character.GetAllCharacters())
                {
                    if (character.TryGetComponent(out ZNetView nview) && nview.GetZDO()?.GetPrefab() == bossHash &&
                        character.TryGetComponent(out CharacterDrop liveDrop))
                    {
                        SetIdolDrops(liveDrop, idols, drops);
                    }
                }
            }

            Logger.LogInfo(enabled
                ? $"Added {added} boss idol drops ({min}-{max} each)"
                : "Disabled; boss idol drops removed");
        }

        /// <summary>
        ///     Replaces the idol drops in a boss's drop list. Vanilla boss drop tables contain no idols, so any
        ///     idol drop found there was added by an earlier Apply (prefabs and their m_drops survive world reloads).
        /// </summary>
        private static void SetIdolDrops(CharacterDrop characterDrop, List<GameObject> idols, List<CharacterDrop.Drop> drops)
        {
            characterDrop.m_drops.RemoveAll(drop => idols.Contains(drop.m_prefab));
            characterDrop.m_drops.AddRange(drops);
        }
    }
}
