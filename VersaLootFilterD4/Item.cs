using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaLootFilterD4
{
    internal class Item
    {
        public enum RarityType
        {
            None,
            Magic,
            Rare,
            Legendary,
            Unique,
        }

        public enum SlotType
        {
            None,
            Helm,
            Chest,
            Gloves,
            Pants,
            Boots,
            Amulet,
            Ring,
            Weapon1H,
            Weapon2H,
            OffHand,
        }

        public enum StatValueType
        {
            None,
            Integer,
            Percent,
        }

        public enum StatType
        {
            None,
            Armor,
            AdditionalLife,
            Strength,
            Intelligence,
            Willpower,
            Dexterity,
            Thorns,
            CooldownReduction,
            StormSkillCooldownReduction,
            FireResistance,
            ColdResistance,
            LightningResistance,
            PoisonResistance,
            ShadowResistance,
            ResistanceToAllElements,
            MaximumLife,
            HealingReceived,
            LifeOnKill,
            LifeRegenerationWhileNotDamagedRecently,
            AllStats,
            ResourceGeneration,
            MaximumSpirit,
            SpiritCostReduction,
            TotalArmor,
            TotalArmorWhileInWerewolfForm,
            TotalArmorWhileInWerebearForm,
            BarrierGeneration,
            FortifyGeneration,
            AttackSpeed,
            AttacksPerSecond,
            BasicSkillAttackSpeed,
            CrowdControlDuration,
            SlowDurationReduction,
            ControlImpairedDurationReduction,
            DamageReduction,
            DamageReductionFromCloseEnemies,
            DamageReductionFromDistantEnemies,
            DamageReductionWhileInjured,
            DamageReductionWhileFortified,
            DamageReductionFromEnemiesThatArePoisoned,
            Damage,
            VulnerableDamage,
            DamageWhileShapeshifted,
            DamageWhileInHumanForm,
            LightningDamage,
            PoisonDamage,
            PhysicalDamage,
            StormSkillDamage,
            EarthSkillDamage,
            WerewolfSkillDamage,
            WerebearSkillDamage,
            CompanionSkillDamage,
            DamageToSlowedEnemies,
            DamageToStunnedEnemies,
            DamageToCrowdControlledEnemies,
            DamageToHealthyEnemies,
            DamageToInjuredEnemies,
            DamageToCloseEnemies,
            DamageToDistantEnemies,
            DamageOverTime,
            DamageToPoisonedEnemies,
            DamageAfterKillingAnElite,
            BasicSkillDamage,
            CoreSkillDamage,
            UltimateSkillDamage,
            DodgeChance,
            DodgeChanceAgainstDistantEnemies,
            CriticalStrikeChance,
            CriticalStrikeChanceAgainstInjuredEnemies,
            CriticalStrikeDamage,
            CriticalStrikeDamageWithWerewolfSkills,
            CriticalStrikeDamageWithWerebearSkills,
            CriticalStrikeDamageWithEarthSkills,
            LightningCriticalStrikeDamage,
            OverpowerDamage,
            OverpowerDamageWithWerebearSkills,
            LuckyHitChance,
            ChanceToSlow,
            ChanceToHeal,
            ChanceToRestoreResource,
            MovementSpeed,
            MovementSpeedAfterKillingAnElite,
            ShrineBuffDuration,
            ChanceToExecuteInjuredNonElites,
            RanksOfRavens,
            DamagePerSecond,
            RanksOfTheEnvenomPassive,
            RanksOfPulverize,
        }

        // Basic
        /// <summary>
        /// Items can have name withing that rows amount of tooltip:
        /// <see cref="RarityType.Magic"/> 1-2;
        /// <see cref="RarityType.Rare"/> 1;
        /// <see cref="RarityType.Legendary"/> 1-3;
        /// <see cref="RarityType.Unique"/> 1-2;
        /// </summary>
        public string Name;
        public SlotType Slot = SlotType.None;
        public RarityType Rarity = RarityType.None;
        public int ItemPower;
        public int Upgrades;

        public List<Stat> Stats = new List<Stat>();

        // Sockets
        public int Sockets;

        // Other
        public int Level;
        public int SellValue;
        public int Durability;
        public bool AccountBound;
        public bool Class; // TODO: add class enum

        public override string ToString()
        {
            string tooltip = string.Format($"[{Name} | {ItemPower} | {Slot}] ");
            foreach (var stat in Stats)
                tooltip += stat.ToString() + "; ";

            return tooltip;
        }
    }
}
