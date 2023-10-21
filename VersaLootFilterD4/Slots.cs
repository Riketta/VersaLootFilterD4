using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaLootFilterD4
{
    internal class Slots
    {
        internal static Dictionary<string, Item.SlotType> Storage { get; } = new Dictionary<string, Item.SlotType>()
        {
            ["Helm"] = Item.SlotType.Helm,
            ["Chest Armor"] = Item.SlotType.Chest,
            ["Gloves"] = Item.SlotType.Gloves,
            ["Pants"] = Item.SlotType.Pants,
            ["Boots"] = Item.SlotType.Boots,
            ["Amulet"] = Item.SlotType.Amulet,
            ["Ring"] = Item.SlotType.Ring,

            // One Hand Weapons
            ["Axe"] = Item.SlotType.Weapon1H,
            ["Mace"] = Item.SlotType.Weapon1H,
            ["Sword"] = Item.SlotType.Weapon1H,
            ["Dagger"] = Item.SlotType.Weapon1H,
            ["Wand"] = Item.SlotType.Weapon1H,

            // Two Hand Weapons
            ["Staff"] = Item.SlotType.Weapon2H,
            ["Two-Handed Axe"] = Item.SlotType.Weapon2H,
            ["Two-Handed Mace"] = Item.SlotType.Weapon2H,

            // Off Hands
            ["Shield"] = Item.SlotType.OffHand,
            ["Totem"] = Item.SlotType.OffHand,
            ["Focus"] = Item.SlotType.OffHand,
        };
    }
}
