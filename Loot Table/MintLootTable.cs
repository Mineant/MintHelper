using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mineant
{

    [System.Serializable]
    public abstract class MintLootTable<T, V> where T : MintLoot<V>
    {
        /// the list of objects that have a chance of being returned by the table
        [SerializeField]
        public List<T> ObjectsToLoot;

        /// the total amount of weights, for debug purposes only
        [Header("Debug")]
        [Tooltip("DebugOnly")]
        public float DebugWeightsTotal;

        protected float _maximumWeightSoFar = 0f;
        protected bool _weightsComputed = false;


        [Button]
        public virtual void DebugComputeWeights()
        {
            ComputeWeights();
            _weightsComputed = false;
        }



        /// <summary>
        /// Determines, for each object in the table, its chance percentage, based on the specified weights
        /// </summary>
        public virtual void ComputeWeights()
        {
            if (ObjectsToLoot == null)
            {
                return;
            }

            if (ObjectsToLoot.Count == 0)
            {
                return;
            }

            _maximumWeightSoFar = 0f;

            foreach (T lootDropItem in ObjectsToLoot)
            {
                // set the maximum weight and loot range
                if (lootDropItem.Weight >= 0f)
                {
                    lootDropItem.RangeFrom = _maximumWeightSoFar;
                    _maximumWeightSoFar += lootDropItem.Weight;
                    lootDropItem.RangeTo = _maximumWeightSoFar;
                }
                else
                {
                    lootDropItem.Weight = 0f;
                }
            }

            DebugWeightsTotal = _maximumWeightSoFar;

            foreach (T lootDropItem in ObjectsToLoot)
            {
                lootDropItem.DebugChancePercentage = ((lootDropItem.Weight) / DebugWeightsTotal) * 100;
            }



            _weightsComputed = true;
        }

        /// <summary>
        /// Returns one object from the table, picked randomly
        /// </summary>
        /// <returns></returns>
        public virtual T GetLoot()
        {
            if (ObjectsToLoot == null)
            {
                return null;
            }

            if (ObjectsToLoot.Count == 0)
            {
                return null;
            }

            if (!_weightsComputed)
            {
                ComputeWeights();
            }

            float index = Random.Range(0, DebugWeightsTotal);

            foreach (T lootDropItem in ObjectsToLoot)
            {
                if ((index > lootDropItem.RangeFrom) && (index < lootDropItem.RangeTo))
                {
                    return lootDropItem;
                }
            }

            return null;
        }


        public virtual void AddLoot(T loot)
        {
            if (ObjectsToLoot == null) ObjectsToLoot = new();
            ObjectsToLoot.Add(loot);
            ComputeWeights();
        }



        // public virtual List<T> GetLoots(int min, int max, bool isUnique = false)
        // {
        //     return GetLoots(UnityEngine.Random.Range(min, max), isUnique);
        // }

        /// <summary>
        /// This will return a list of loots, it will have duplicates.
        /// </summary>
        public virtual List<T> GetLoots(int count)
        {
            List<T> loots = new();
            for (int i = 0; i < count; i++)
            {
                loots.Add(GetLoot());
            }

            return loots;
        }

        /// <summary>
        /// This is a very costly operation, use this wisely
        /// </summary>
        public virtual List<T> GetUniqueLoots(int count)
        {
            // create a thing to store the things
            var originalLoots = new List<T>(ObjectsToLoot);

            // get the loots by create a new loottable each time after taking out one loot
            List<T> loots = new List<T>();
            T loot = null;
            for (int i = 0; i < count; i++)
            {
                if (ObjectsToLoot.Count == 0) break;

                // Get the loot
                loot = GetLoot();
                loots.Add(loot);

                // prepare the loottable
                ObjectsToLoot.Remove(loot);
                ComputeWeights();
            }

            ObjectsToLoot = originalLoots;

            return loots;
        }
    }
}