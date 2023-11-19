using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static VersaLootFilterD4.LootFilter;

namespace VersaLootFilterD4
{
    internal class LootFilter
    {
        public enum Result
        {
            None,
            Junk,
            Keep,
            Error,
        }

        internal class StatKeyValuePair
        {
            public readonly Item.StatType Stat;

            /// <summary>
            /// Minimum amount of stat for <see cref="LootFilter"/>, if ignored if 0
            /// </summary>
            public readonly float Value; // TODO: use Stat as type?

            public StatKeyValuePair(Item.StatType statType, float value = 0)
            {
                Stat = statType;
                Value = value;
            }
        }

        internal class Filter
        {
            public readonly string Name;
            public readonly Item.SlotType Slot;
            public readonly int MinPowerLevel = 0;
            public readonly int MaxLevel = 100;
            /// <summary>
            /// Minimum amount of stats matches on item to pass filter
            /// </summary>
            public readonly int MinAmountOfMatches = 3;

            /// <summary>
            /// List of stats to search on item with minimum value, null <see cref="StatKeyValuePair.Value"/> means no minimum stat value restriction;
            /// If required stats by item can have two of same stats (example: one implicit CDR and other random roll), filter should contain two duplicates of this stat
            /// </summary>
            public readonly List<StatKeyValuePair> Stats = new List<StatKeyValuePair>();

            public Filter(string name, Item.SlotType slotType, int minAmountOfMatches, int minPowerLevel = 0, int maxLevel = 100)
            {
                Name = name;
                Slot = slotType;
                MinPowerLevel = minPowerLevel;
                MaxLevel = maxLevel;
                MinAmountOfMatches = minAmountOfMatches;
            }

            public override string ToString()
            {
                string tooltip = string.Format($"[{Name} | {MinPowerLevel}+ | {Slot} | {MinAmountOfMatches}] ");
                foreach (var stat in Stats)
                    tooltip += stat.ToString() + "; ";

                return tooltip;
            }
        }

        static LootFilter()
        {
            CreateDefaultFilters();
        }

        private static readonly List<Filter> Filters = new List<Filter>();

        /// <summary>
        /// Filter item using <see cref="Filters"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="Result.Keep"/> if item passes at least one filter, <see cref="Result.Junk"/> if no filters passed, otherwise <see cref="Result.Error"/></returns>
        public static Result FilterItem(Item item)
        {
            Result result = Result.Junk;

            Dictionary<Filter, int> statistics = new Dictionary<Filter, int>();
            foreach (Filter filter in Filters)
            {
                if (filter.Stats.Count < filter.MinAmountOfMatches)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  >>> Filter {{{filter}}} has less stats than required!");
                    Console.ResetColor();
                    continue; // TODO: return error?
                }

                if (filter.Slot != item.Slot)
                {
                    //Console.WriteLine($"  >>> Filter slot mismatch: F[{filter.Slot}] != I[{item.Slot}], skipping");
                    continue;
                }

                if (item.ItemPower < filter.MinPowerLevel)
                {
                    Console.WriteLine($"  >>> Item power level {item.ItemPower} is less than filter's [{filter.Name}] {filter.MinPowerLevel}, skipping");
                    continue;
                }

                if (item.Level > filter.MaxLevel)
                {
                    Console.WriteLine($"  >>> Item level {item.Level} is greater than filter's [{filter.Name}] maximum allowed {filter.MaxLevel}, skipping");
                    continue;
                }

                if (item.Stats.Count < filter.MinAmountOfMatches)
                {
                    Console.WriteLine($"  >>> Item has less stats ({item.Stats.Count}) than filter's [{filter.Name}] minimum required {filter.MinAmountOfMatches}, skipping");
                    continue;
                }

                List<Item.StatType> requiredStats = new List<Item.StatType>();
                var filterStats = filter.Stats.ToList();
                foreach (var itemStat in item.Stats)
                {
                    int index = filterStats.FindIndex(p => p.Stat == itemStat.StatType && (p.Value == 0 || itemStat.Value >= p.Value));

                    if (index >= 0)
                    {
                        //Console.WriteLine($"Found stat: {itemStat.StatType}"); // DEBUG
                        requiredStats.Add(itemStat.StatType);
                        filterStats.RemoveAt(index);
                    }
                }
                statistics[filter] = requiredStats.Count;

                if (requiredStats.Count >= filter.MinAmountOfMatches)
                {
                    string stats = "";
                    foreach (var stat in requiredStats)
                        stats += stat.ToString() + "; ";

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [+] Item [{item.Name}] has passed filter [{filter.Name} | {filter.Slot}]: {stats}");
                    Console.ResetColor();

                    result = Result.Keep;
                    continue; // TODO: use all filters?
                }
            }

            if (result == Result.Junk)
            {
                string results = "";
                foreach (var filterStat in statistics)
                    results += $"[{filterStat.Key.Name} = {filterStat.Value}/{filterStat.Key.MinAmountOfMatches}]; ";
                Console.WriteLine($"  >>> Item [{item.Name}] has not passed any filters: {(statistics.Count > 0 ? results : "no filters")}");
            }

            return result;
        }

        public static Filter Create(string name, Item.SlotType slotType, int minAmountOfMatches = 3, int minPowerLevel = 0, int maxLevel = 100)
        {
            Filter filter = new Filter(name, slotType, minAmountOfMatches, minPowerLevel, maxLevel);

            Filters.Add(filter);
            return filter;
        }

        public static void CreateDefaultFilters()
        {
            Filter filter = null;

            filter = Create("DEBUG", Item.SlotType.Helm);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmor));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));

            filter = Create("High DPS Weapon2H", Item.SlotType.Weapon2H, 0, 925);
            filter = Create("High DPS Weapon1H", Item.SlotType.Weapon1H, 0, 925);

            #region Pulverize
            filter = Create("Pulverize", Item.SlotType.Weapon2H, minPowerLevel: 925);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AllStats));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CoreSkillDamage));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.VulnerableDamage));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToSlowedEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeDamage));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeDamageWithEarthSkills));

            filter = Create("Pulverize", Item.SlotType.Helm, minPowerLevel: 750, minAmountOfMatches: 3);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmor));

            filter = Create("Pulverize", Item.SlotType.Chest, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));

            filter = Create("Pulverize", Item.SlotType.Gloves, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.RanksOfPulverize));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));

            filter = Create("Pulverize", Item.SlotType.Pants, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));

            filter = Create("Pulverize", Item.SlotType.Boots, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.SpiritCostReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));

            filter = Create("Pulverize", Item.SlotType.Amulet, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.SpiritCostReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));

            filter = Create("Pulverize", Item.SlotType.Ring, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToCloseEnemies));
            #endregion

            #region Stormclaw
            filter = Create("Stormclaw", Item.SlotType.Helm, minPowerLevel: 750, minAmountOfMatches: 3);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BasicSkillAttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmor));

            filter = Create("Stormclaw", Item.SlotType.Chest, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));

            filter = Create("Stormclaw", Item.SlotType.Gloves, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.StormSkillCooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AllStats));

            filter = Create("Stormclaw", Item.SlotType.Pants, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));

            filter = Create("Stormclaw", Item.SlotType.Boots, minPowerLevel: 750, minAmountOfMatches: 3);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmorWhileInWerewolfForm));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));

            filter = Create("Stormclaw", Item.SlotType.Amulet, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.RanksOfTheEnvenomPassive));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));

            filter = Create("Stormclaw", Item.SlotType.Ring, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MaximumLife));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            #endregion

            #region Bulwark Stormclaw
            filter = Create("Bulwark Stormclaw", Item.SlotType.Helm, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BarrierGeneration));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BasicSkillAttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmor));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Helm, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BarrierGeneration));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BasicSkillAttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Helm, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BarrierGeneration));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BasicSkillAttackSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CooldownReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AllStats));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Chest, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.RanksOfEarthenBulwark));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BarrierGeneration));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Gloves, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.Willpower));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.AttackSpeed));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Pants, minPowerLevel: 750);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReduction));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Boots, minPowerLevel: 750, minAmountOfMatches: 2);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.TotalArmorWhileInWerewolfForm));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Amulet, minPowerLevel: 750, minAmountOfMatches: 3);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileInjured));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.RanksOfTheEnvenomPassive));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.MovementSpeed));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.RanksOfAllDefensiveSkills));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionWhileFortified));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageReductionFromEnemiesThatArePoisoned));

            filter = Create("Bulwark Stormclaw", Item.SlotType.Ring, minPowerLevel: 750, minAmountOfMatches: 3);
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.BarrierGeneration));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToCloseEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.DamageToDistantEnemies));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.CriticalStrikeChance));
            filter.Stats.Add(new StatKeyValuePair(Item.StatType.LuckyHitChance));
            #endregion

            #region Sell
            filter = Create("Sell", Item.SlotType.Weapon1H, minPowerLevel: 925, minAmountOfMatches: 0);
            filter = Create("Sell", Item.SlotType.Weapon2H, minPowerLevel: 925, minAmountOfMatches: 0);
            #endregion
        }
    }
}
