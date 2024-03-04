using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mineant
{
    [System.Serializable]
    public abstract class MintLoot<T>
    {
        /// the object to return
        public T Loot;
        /// the weight attributed to this specific object in the table
        public float Weight = 1f;
        /// the chance percentage to display for this object to be looted. ChancePercentages are meant to be computed by the MMLootTable class
        [Tooltip("DebugOnly")]
        public float DebugChancePercentage;

        /// the computed low bound of this object's range
        public float RangeFrom { get; set; }
        /// the computed high bound of this object's range
        public float RangeTo { get; set; }

        public MintLoot() { }

        public MintLoot(T loot, float weight)
        {
            Loot = loot;
            Weight = weight;
        }
    }

}
