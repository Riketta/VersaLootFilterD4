using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VersaLootFilterD4
{
    internal class Stat
    {
        public Item.StatType StatType = Item.StatType.None;
        public Item.StatValueType ValueType = Item.StatValueType.None;

        /// <summary>
        /// Min possible value of <see cref="Value"/> for current stat; not used for <see cref="LootFilter"/>!
        /// </summary>
        public float MinValue;

        /// <summary>
        /// Max possible value of <see cref="Value"/> for current stat; not used for <see cref="LootFilter"/>!
        /// </summary>
        public float MaxValue;

        /// <summary>
        /// Value of <see cref="StatType"/> for existing <see cref="Item"/> or minimum amount of stat for <see cref="LootFilter"/> (ignored if 0)
        /// </summary>
        public float Value;

        public override string ToString()
        {
            return $"{StatType} = {Value}{(ValueType == Item.StatValueType.Percent ? "%" : "")}{(MaxValue == MinValue ? "" : $" [{MinValue}-{MaxValue}]")}";
        }
    }
}
