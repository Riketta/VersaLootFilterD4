using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IronOcr.OcrResult;

namespace VersaLootFilterD4
{
    internal class TooltipParser
    {
        public static readonly Regex ItemPowerRegex = new Regex(@"(?<power>\d+) Item Power", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly Regex ItemTierRarityAndTypeRegex = new Regex(@"(?<tier>Ancestral|Sacred|) ?(?<rarity>Unique|Legendary|Rare|Magic) (?<itemType>.+)$", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly Regex ItemRequiredLevelRegex = new Regex(@"Level (?<level>\d+)", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly Regex ItemSellValueRegex = new Regex(@"Sell Value: (?<price>[0-9,]+)", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly string ItemEmptySocketString = "Empty Socket";
        public static readonly string ItemStatsAboveThisLine = "Properties lost when equipped";

        public static Item Parse(List<string> tooltip, bool debug = false)
        {
            for (int i = 0; i < tooltip.Count; i++)
            {
                string line = tooltip[i];
                tooltip[i] = line.Replace("{", "").Replace("}", "");
            }

            Item item = new Item();
            int lastLineWithStats = 0; // interval [0; this]

            int linesForItemNameAndType = 0; // interval [0; this]
            // 725 Item Power
            for (int i = 0; i < tooltip.Count; i++)
            {
                string line = tooltip[i];

                MatchCollection itemPowerMatches = ItemPowerRegex.Matches(line); // TODO: add upgrades (725+25 Item Power)
                if (itemPowerMatches.Count > 0)
                {
                    item.ItemPower = int.Parse(itemPowerMatches[0].Groups["power"].Value);
                    linesForItemNameAndType = i - 1; // from previous line to start
                    tooltip.RemoveAt(i);
                    break;
                }
            }
            if (item.ItemPower == 0)
                throw new Exception("Failed to parse item power!");


            // [_/Sacred/Ancestral] [Magic/Rare/Legendary/Unique] [_SlotType_]
            // Rarity + Type: Sacred Rare Two-Handed Mace & Name: CUT ANVIL
            string itemNameAndType = "";
            for (int i = 0; i <= linesForItemNameAndType; i++)
            {
                itemNameAndType += tooltip[0] + " ";
                tooltip.RemoveAt(0);
            }
            itemNameAndType = itemNameAndType.Trim();

            {
                Match match = ItemTierRarityAndTypeRegex.Match(itemNameAndType);
                if (match.Success)
                {
                    item.Rarity = (Item.RarityType)Enum.Parse(typeof(Item.RarityType), match.Groups["rarity"].Value);
                    try
                    {
                        item.Slot = Slots.Storage[match.Groups["itemType"].Value.TrimEnd(new char[] { '.' }).Replace("- ", "-")];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                    }

                    item.Name = itemNameAndType.Substring(0, match.Index).TrimLowerCaseAndWhiteSpaces();
                }
                if (item.Slot == Item.SlotType.None)
                {
                    Console.WriteLine("Failed to parse item type!");
                    return null;
                }
                if (item.Rarity == Item.RarityType.None)
                {
                    Console.WriteLine("Failed to parse item rarity!");
                    return null;
                }
            }

            // Empty Sockets
            {
                int rows = tooltip.Count;
                for (int i = 0; i < rows; i++)
                {
                    string line = tooltip[i];

                    if (line.Contains(ItemEmptySocketString))
                    {
                        item.Sockets++;
                        tooltip.RemoveAt(i);
                        rows--;
                        i--;
                    }
                }
            }

            // Level: Required Level 100
            for (int i = 0; i < tooltip.Count; i++)
            {
                string line = tooltip[i];

                Match match = ItemRequiredLevelRegex.Match(line);
                if (match.Success)
                {
                    item.Level = int.Parse(match.Groups["level"].Value);
                    lastLineWithStats = i - 1;
                    tooltip.RemoveAt(i);
                    break;
                }
            }
            if (item.Level == 0)
                Console.WriteLine("Failed to parse item level!");


            // Find row with "Properties lost when equipped" for future stats scanning (stats above "Properties lost" and both above "Level")
            {
                int rows = tooltip.Count;
                for (int i = 0; i < rows; i++)
                {
                    string line = tooltip[i];

                    if (line.Contains(ItemStatsAboveThisLine))
                    {
                        lastLineWithStats = i - 1;
                        tooltip.RemoveAt(i);
                        break;
                    }
                }
            }

            // Sell Value
            for (int i = 0; i < tooltip.Count; i++) // TODO: fix not flexible regex
            {
                string line = tooltip[i];

                Match match = ItemSellValueRegex.Match(line);
                if (match.Success)
                {
                    item.SellValue = int.Parse(match.Groups["price"].Value.Replace(",", ""));
                    tooltip.RemoveAt(i);
                    break;
                }
            }
            if (item.SellValue == 0)
            {
                //Console.WriteLine("Failed to parse item price!");
            }


            // Stats
            StringBuilder allStatsSB = new StringBuilder();
            for (int i = 0; i <= lastLineWithStats; i++)
            {
                allStatsSB.Append(tooltip[i]);
                allStatsSB.Append(' ');
            }
            string allStats = allStatsSB.ToString();

            foreach (var statTemplate in StatTemplate.All)
            {
                MatchCollection matches = statTemplate.Regex.Matches(allStats);
                foreach (Match match in matches)
                {
                    string strValue = match.Groups["value"].Value;
                    Item.StatValueType type = (match.Groups["valueType"].Value == "%" ? Item.StatValueType.Percent : Item.StatValueType.Integer);
                    allStats = allStats.Replace(match.Value, "");

                    Stat stat = new Stat()
                    {
                        StatType = statTemplate.StatType,
                        ValueType = type,
                        Value = float.Parse(strValue),
                    };

                    item.Stats.Add(stat);
                }
            }

            if (item.ItemPower > 700)
            {
                bool areStatsMissing = false;
                int usualAmountOfStats = 0;

                switch (item.Slot)
                {
                    case Item.SlotType.Amulet:
                    case Item.SlotType.Helm:
                    case Item.SlotType.Chest:
                    case Item.SlotType.Gloves:
                        usualAmountOfStats = 5;
                        if (item.Stats.Count < usualAmountOfStats)
                            areStatsMissing = true;
                        break;

                    case Item.SlotType.Weapon2H:
                    case Item.SlotType.Weapon1H:
                    case Item.SlotType.OffHand:
                    case Item.SlotType.Ring:
                    case Item.SlotType.Pants:
                    case Item.SlotType.Boots:
                        usualAmountOfStats = 6;
                        if (item.Stats.Count < usualAmountOfStats)
                            areStatsMissing = true;
                        break;
                }

                if (areStatsMissing)
                    Logger.WriteLineInColor(ConsoleColor.DarkYellow, $"Item has less stats than it should have: {item.Stats.Count}/{usualAmountOfStats}!");
            }

            if (debug)
            {
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"  > Remaining: {allStats}");
                Console.ResetColor();
            }

            return item;
        }
    }
}
