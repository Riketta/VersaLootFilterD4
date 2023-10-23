using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static IronOcr.OcrResult;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace VersaLootFilterD4
{
    internal class StatTemplate : IEquatable<StatTemplate>, IComparable<StatTemplate>
    {
        public readonly Item.StatType StatType = Item.StatType.None;
        public readonly Regex Regex = null;
        public readonly int DescriptionLength;

        static StatTemplate()
        {
            All.Sort();
        }

        private StatTemplate() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="regexString">Regex to match variables with in tooltip, pass <see cref="null"/> to use default template <see cref="DefaultRegexTemplate"/></param>
        /// <param name="description">Stat description with {} as placeholder for regex string at place the of expected value</param>
        public StatTemplate(Item.StatType type, string regexString, string description)
        {
            if (string.IsNullOrEmpty(regexString))
                regexString = DefaultRegexTemplate;
            
            StatType = type;
            DescriptionLength = description.Length;
            Regex = new Regex(description.Replace("{}", regexString), RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public bool Equals(StatTemplate other)
        {
            if (other == null) return false;
            if (other.StatType != StatType) return false;
            if (other.DescriptionLength != DescriptionLength) return false;

            return true;
        }

        public int CompareTo(StatTemplate other)
        {

            if (DescriptionLength > other.DescriptionLength)
                return -1;
            else if (DescriptionLength < other.DescriptionLength)
                return 1;

            return 0;
        }

        public static string DefaultRegexTemplate = @"\+?(?<value>[0-9\.]+)(?<valueType>%?)";
        internal static List<StatTemplate> All { get; } = new List<StatTemplate>()
        {
            // Resistances
            new StatTemplate(Item.StatType.FireResistance, DefaultRegexTemplate, @"{} Fire Resistance"),
            new StatTemplate(Item.StatType.ColdResistance, DefaultRegexTemplate, @"{} Cold Resistance"),
            new StatTemplate(Item.StatType.LightningResistance, DefaultRegexTemplate, @"{} Lightning Resistance"),
            new StatTemplate(Item.StatType.PoisonResistance, DefaultRegexTemplate, @"{} Poison Resistance"),
            new StatTemplate(Item.StatType.ShadowResistance, DefaultRegexTemplate, @"{} Shadow Resistance"),
            new StatTemplate(Item.StatType.ResistanceToAllElements, DefaultRegexTemplate, @"{} Resistance to All Elements"),
            new StatTemplate(Item.StatType.ResistanceToAllElements, DefaultRegexTemplate, @"{} Resistance to All Eleme.+? "),

            // Stats // TODO: separate % and flat?
            new StatTemplate(Item.StatType.AllStats, DefaultRegexTemplate, @"{} All Stats"),
            new StatTemplate(Item.StatType.Strength, DefaultRegexTemplate, @"{} Strength"),
            new StatTemplate(Item.StatType.Dexterity, DefaultRegexTemplate, @"{} Dexterity"),
            new StatTemplate(Item.StatType.Intelligence, DefaultRegexTemplate, @"{} Intelligence"),
            new StatTemplate(Item.StatType.Willpower, DefaultRegexTemplate, @"{} Willpower"),

            // Resource
            new StatTemplate(Item.StatType.ResourceGeneration, DefaultRegexTemplate, @"{} Resource Generation"),
            new StatTemplate(Item.StatType.MaximumSpirit, DefaultRegexTemplate, @"{} Maximum Spirit"),
            new StatTemplate(Item.StatType.SpiritCostReduction, DefaultRegexTemplate, @"{} Spirit Cost Reduction"),

            // Health
            new StatTemplate(Item.StatType.MaximumLife, DefaultRegexTemplate, @"{} Maximum Life"),
            new StatTemplate(Item.StatType.HealingReceived, DefaultRegexTemplate, @"{} Healing Received"),
            new StatTemplate(Item.StatType.LifeOnKill, DefaultRegexTemplate, @"{} Life On Kill"),
            new StatTemplate(Item.StatType.LifeRegenerationWhileNotDamagedRecently, DefaultRegexTemplate, @"{} Life Regeneration while Not Damaged Recently"),

            // Armor
            new StatTemplate(Item.StatType.Armor, DefaultRegexTemplate, @"{} Armor"),
            new StatTemplate(Item.StatType.TotalArmor, DefaultRegexTemplate, @"{} Total Armor"),
            new StatTemplate(Item.StatType.TotalArmorWhileInWerewolfForm, DefaultRegexTemplate, @"{} Total Armor while in Werewolf Form"),
            new StatTemplate(Item.StatType.TotalArmorWhileInWerebearForm, DefaultRegexTemplate, @"{} Total Armor while in Werebear Form"),

            // Defensive Buffs
            new StatTemplate(Item.StatType.BarrierGeneration, DefaultRegexTemplate, @"{} Barrier Generation"),
            new StatTemplate(Item.StatType.FortifyGeneration, DefaultRegexTemplate, @"{} Fortify Generation"),

            // Dodge
            new StatTemplate(Item.StatType.DodgeChance, DefaultRegexTemplate, @"{} Dodge Chance"),
            new StatTemplate(Item.StatType.DodgeChanceAgainstDistantEnemies, DefaultRegexTemplate, @"{} Dodge Chance Against Distant Enemies"),

            // Damage Reduction
            new StatTemplate(Item.StatType.DamageReduction, DefaultRegexTemplate, @"{} Damage Reduction"),
            new StatTemplate(Item.StatType.DamageReductionFromCloseEnemies, DefaultRegexTemplate, @"{} Damage Reduction from Close Enemies"),
            new StatTemplate(Item.StatType.DamageReductionFromDistantEnemies, DefaultRegexTemplate, @"{} Damage Reduction from Distant Enemies"),
            new StatTemplate(Item.StatType.DamageReductionWhileInjured, DefaultRegexTemplate, @"{} Damage Reduction while Injured"),
            new StatTemplate(Item.StatType.DamageReductionWhileFortified, DefaultRegexTemplate, @"{} Damage Reduction while Fortified"),
            new StatTemplate(Item.StatType.DamageReductionFromEnemiesThatArePoisoned, DefaultRegexTemplate, @"{} Damage Reduction from Enemies That Are Poisoned"),

            // Cooldown Reduction
            new StatTemplate(Item.StatType.CooldownReduction, DefaultRegexTemplate, @"{} Cooldown Reduction"),
            new StatTemplate(Item.StatType.StormSkillCooldownReduction, DefaultRegexTemplate, @"{} Storm Skill Cooldown Reduction"),

            // Attack Speed
            new StatTemplate(Item.StatType.AttacksPerSecond, DefaultRegexTemplate, @"{} Attacks Per Second"),
            new StatTemplate(Item.StatType.AttackSpeed, DefaultRegexTemplate, @"{} Attack Speed"),
            new StatTemplate(Item.StatType.BasicSkillAttackSpeed, DefaultRegexTemplate, @"{} Basic Skill Attack Speed"),

            // Critical Strike Chance
            new StatTemplate(Item.StatType.CriticalStrikeChance, DefaultRegexTemplate, @"{} Critical Strike Chance"),
            new StatTemplate(Item.StatType.CriticalStrikeChanceAgainstInjuredEnemies, DefaultRegexTemplate, @"{} Critical Strike Chance Against Injured Enemies"),

            // Critical Strike Damage
            new StatTemplate(Item.StatType.CriticalStrikeDamage, DefaultRegexTemplate, @"{} Critical Strike Damage"),
            new StatTemplate(Item.StatType.CriticalStrikeDamageWithWerewolfSkills, DefaultRegexTemplate, @"{} Critical Strike Damage with Werewolf Skills"),
            new StatTemplate(Item.StatType.CriticalStrikeDamageWithWerebearSkills, DefaultRegexTemplate, @"{} Critical Strike Damage with Werebear Skills"),
            new StatTemplate(Item.StatType.CriticalStrikeDamageWithEarthSkills, DefaultRegexTemplate, @"{} Critical Strike Damage with Earth Skills"),
            new StatTemplate(Item.StatType.LightningCriticalStrikeDamage, DefaultRegexTemplate, @"{} Lightning Critical Strike Damage"),

            // Overpower Damage
            new StatTemplate(Item.StatType.OverpowerDamage, DefaultRegexTemplate, @"{} Overpower Damage"),
            new StatTemplate(Item.StatType.OverpowerDamageWithWerebearSkills, DefaultRegexTemplate, @"{} Overpower Damage with Werebear Skills"),

            // Damage
            new StatTemplate(Item.StatType.DamagePerSecond, DefaultRegexTemplate.Replace('.', ','), @"{} Damage Per Second"),
            new StatTemplate(Item.StatType.Damage, DefaultRegexTemplate, @"{} Damage"),
            new StatTemplate(Item.StatType.VulnerableDamage, DefaultRegexTemplate, @"{} Vulnerable Damage"),
            new StatTemplate(Item.StatType.DamageWhileShapeshifted, DefaultRegexTemplate, @"{} Damage while Shapeshifted"),
            new StatTemplate(Item.StatType.DamageWhileInHumanForm, DefaultRegexTemplate, @"{} Damage while in Human Form"),
            new StatTemplate(Item.StatType.LightningDamage, DefaultRegexTemplate, @"{} Lightning Damage"),
            new StatTemplate(Item.StatType.PoisonDamage, DefaultRegexTemplate, @"{} Poison Damage"),
            new StatTemplate(Item.StatType.PhysicalDamage, DefaultRegexTemplate, @"{} Physical Damage"),
            new StatTemplate(Item.StatType.DamageToSlowedEnemies, DefaultRegexTemplate, @"{} Damage to Slowed Enemies"),
            new StatTemplate(Item.StatType.DamageToStunnedEnemies, DefaultRegexTemplate, @"{} Damage to Stunned Enemies"),
            new StatTemplate(Item.StatType.DamageToCrowdControlledEnemies, DefaultRegexTemplate, @"{} Damage to Crowd Controlled Enemies"),
            new StatTemplate(Item.StatType.DamageToHealthyEnemies, DefaultRegexTemplate, @"{} Damage to Healthy Enemies"),
            new StatTemplate(Item.StatType.DamageToInjuredEnemies, DefaultRegexTemplate, @"{} Damage to Injured Enemies"),
            new StatTemplate(Item.StatType.DamageToCloseEnemies, DefaultRegexTemplate, @"{} Damage to Close Enemies"),
            new StatTemplate(Item.StatType.DamageToDistantEnemies, DefaultRegexTemplate, @"{} Damage to Distant Enemies"),
            new StatTemplate(Item.StatType.DamageOverTime, DefaultRegexTemplate, @"{} Damage Over Time"),
            new StatTemplate(Item.StatType.DamageToPoisonedEnemies, DefaultRegexTemplate, @"{} Damage to Poisoned Enemies"),
            new StatTemplate(Item.StatType.DamageAfterKillingAnElite, DefaultRegexTemplate, @"{} Damage for 4 Seconds After Killing an Elite"),

            // Skill Damage
            new StatTemplate(Item.StatType.BasicSkillDamage, DefaultRegexTemplate, @"{} Basic Skill Damage"),
            new StatTemplate(Item.StatType.CoreSkillDamage, DefaultRegexTemplate, @"{} Core Skill Damage"),
            new StatTemplate(Item.StatType.UltimateSkillDamage, DefaultRegexTemplate, @"{} Ultimate Skill Damage"),
            new StatTemplate(Item.StatType.StormSkillDamage, DefaultRegexTemplate, @"{} Storm Skill Damage"),
            new StatTemplate(Item.StatType.EarthSkillDamage, DefaultRegexTemplate, @"{} Earth Skill Damage"),
            new StatTemplate(Item.StatType.WerewolfSkillDamage, DefaultRegexTemplate, @"{} Werewolf Skill Damage"),
            new StatTemplate(Item.StatType.WerebearSkillDamage, DefaultRegexTemplate, @"{} Werebear Skill Damage"),
            new StatTemplate(Item.StatType.CompanionSkillDamage, DefaultRegexTemplate, @"{} Companion Skill Damage"),

            // Druid Ranks
            new StatTemplate(Item.StatType.RanksOfRavens, DefaultRegexTemplate, @"{} Ranks? of Ravens"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Poison Creeper"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Wolves"),
            new StatTemplate(Item.StatType.RanksOfEarthenBulwark, DefaultRegexTemplate, @"{} Ranks? of Earthen Bulwark"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Debilitating Roar"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Cyclone Armor"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Blood Howl"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Landslide"),
            new StatTemplate(Item.StatType.RanksOfPulverize, DefaultRegexTemplate, @"{} Ranks? of Pulverize"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Tornado"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Lightning Storm"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Shred"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Trample"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Hurricane"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Boulder"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of Rabies"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Wild Impulses Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Call of the Wild Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Nature's Reach Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Crushing Earth Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Stone Guard Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Toxic Claws Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Defiance Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Natural Disaster Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Resonance Passive"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of the Quickshift Passive"),
            new StatTemplate(Item.StatType.RanksOfTheEnvenomPassive, DefaultRegexTemplate, @"{} Ranks? of the Envenom Passive"),

            // Ranks
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of All Wrath Skills"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of All Companion Skills"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"{} Ranks? of All Defensive Skills"),

            // Crowd Control
            new StatTemplate(Item.StatType.CrowdControlDuration, DefaultRegexTemplate, @"{} Crowd Control Duration"),
            new StatTemplate(Item.StatType.SlowDurationReduction, DefaultRegexTemplate, @"{} Slow Duration Reduction"),
            new StatTemplate(Item.StatType.ControlImpairedDurationReduction, DefaultRegexTemplate, @"{} Control Impaired Duration Reduction"),

            // Lucky Hits
            new StatTemplate(Item.StatType.LuckyHitChance, DefaultRegexTemplate, @"{} Lucky Hit Chance"),
            new StatTemplate(Item.StatType.ChanceToSlow, DefaultRegexTemplate, @"Lucky Hit: Up to a {} Chance to Slow"),
            new StatTemplate(Item.StatType.ChanceToHeal, DefaultRegexTemplate, @"Lucky Hit: Up to a 5% Chance to Heal {} Life"),
            new StatTemplate(Item.StatType.ChanceToRestoreResource, DefaultRegexTemplate, @"Lucky Hit: Up to a 5% Chance to Restore {} Primary Resource"),
            new StatTemplate(Item.StatType.ChanceToExecuteInjuredNonElites, DefaultRegexTemplate, @"Lucky Hit: Up to a {} Chance to Execute Injured Non-Elites"),

            // Implicit Pants
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"While Injured, Your Potion Also Grants {} Maximum Life as Barrier"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"While Injured, Your Potion Also Grants {} Movement Speed for 2 Seconds"),
            new StatTemplate(Item.StatType.None, DefaultRegexTemplate, @"While Injured, Your Potion Also Restores {} Resource"),

            // Implicit Boots
            new StatTemplate(Item.StatType.MaximumEvadeCharges, DefaultRegexTemplate, @"{} Maximum Evade Charges"),
            new StatTemplate(Item.StatType.EvadeGrantsMovementSpeed, DefaultRegexTemplate, @"Evade Grants {} Movement Speed for 1 Second"),
            new StatTemplate(Item.StatType.AttacksReduceEvadesCooldown, DefaultRegexTemplate, @"Attacks Reduce Evade[’']s Cooldown by {} Second"),

            // Movement Speed
            new StatTemplate(Item.StatType.MovementSpeed, DefaultRegexTemplate, @"{} Movement Speed"),
            new StatTemplate(Item.StatType.MovementSpeedAfterKillingAnElite, DefaultRegexTemplate, @"{} Movement Speed for 4 Seconds After Killing an Elite"),

            // Other
            new StatTemplate(Item.StatType.Thorns, DefaultRegexTemplate, @"{} Thorns"),
            new StatTemplate(Item.StatType.ShrineBuffDuration, DefaultRegexTemplate, @"{} Shrine Buff Duration"),
            new StatTemplate(Item.StatType.PotionDropRate, DefaultRegexTemplate, @"{} Potion Drop Rate"),
        };
    }
}
