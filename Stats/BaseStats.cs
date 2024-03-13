using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kryz.CharacterStats;
using System;
using System.Linq;

namespace Mineant
{
    public class BaseStats : MonoBehaviour
    {
        public BasicStats BasicStats;
        public List<BaseStats> RelatedBaseStats;

        protected Dictionary<Stat, CharacterStat> _statAndCharacterStats;

        // protected Dictionary<Stat, float> _value;

        void Awake()
        {
            _statAndCharacterStats = new Dictionary<Stat, CharacterStat>();
            foreach (var keyValuePair in BasicStats.StatDictionaryAllInOne.BaseValueStatDictionary)
            {
                InitializeStat(keyValuePair.Key, keyValuePair.Value);
            }

            foreach (var keyValuePair in BasicStats.StatDictionaryAllInOne.FlatModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.Flat));
            }

            foreach (var keyValuePair in BasicStats.StatDictionaryAllInOne.PercentAddModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.PercentAdd));
            }

            foreach (var keyValuePair in BasicStats.StatDictionaryAllInOne.PercentMultModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.PercentMult));
            }
        }

        public CharacterStat InitializeStat(Stat stat, float baseValue = 0f)
        {
            if (_statAndCharacterStats.TryGetValue(stat, out var characterStat1)) return characterStat1;

            CharacterStat characterStat = new CharacterStat(baseValue);
            _statAndCharacterStats[stat] = characterStat;
            return characterStat;
        }

        public bool HasStat(Stat stat)
        {
            return _statAndCharacterStats.ContainsKey(stat);
        }

        /// <summary>
        /// Only get stat from this base stat.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float GetSelfStat(Stat stat)
        {
            if (_statAndCharacterStats.TryGetValue(stat, out var characterStat)) return characterStat.Value;
            else return 0f;
        }

        public void AddModifier(Stat stat, StatModifier modifier)
        {
            InitializeStat(stat);

            _statAndCharacterStats[stat].AddModifier(modifier);
        }

        public bool RemoveModifier(Stat stat, StatModifier modifier)
        {
            if (!_statAndCharacterStats.ContainsKey(stat)) return false;

            return _statAndCharacterStats[stat].RemoveModifier(modifier);
        }

        public bool RemoveAllModifiersFromSource(Stat stat, object source)
        {
            if (!_statAndCharacterStats.ContainsKey(stat)) return false;

            return _statAndCharacterStats[stat].RemoveAllModifiersFromSource(source);
        }

        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var item in _statAndCharacterStats)
            {
                item.Value.RemoveAllModifiersFromSource(source);
            }
        }

        public void SetBaseValue(Stat stat, float value)
        {
            InitializeStat(stat);

            _statAndCharacterStats[stat].BaseValue = value;
        }


        public float GetStat(Stat stat)
        {
            if (RelatedBaseStats == null || RelatedBaseStats.Count == 0) return GetSelfStat(stat);


            var characterStats = RelatedBaseStats.Where(u => u._statAndCharacterStats.ContainsKey(stat)).Select(u => u._statAndCharacterStats[stat]).ToList();
            if (this._statAndCharacterStats.TryGetValue(stat, out var characterStat)) characterStats.Add(characterStat);

            float finalValue = characterStats.Sum(s => s.BaseValue);
            float sumPercentAdd = 0;

            var statModifiers = characterStats.SelectMany(s => s.StatModifiers).ToList();
            statModifiers.Sort(CharacterStat.CompareModifierOrder);

            for (int i = 0; i < statModifiers.Count; i++)
            {
                StatModifier mod = statModifiers[i];

                if (mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;

                    if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                }
                else if (mod.Type == StatModType.PercentMult)
                {
                    finalValue *= 1 + mod.Value;
                }
            }

            // Workaround for float calculation errors, like displaying 12.00001 instead of 12
            return (float)Math.Round(finalValue, 4);
        }

        public void AddRelatedBaseStats(BaseStats baseStats)
        {
            if (baseStats == null) Debug.LogError("Shouldnt be null");
            if (RelatedBaseStats.Contains(baseStats)) return;
            RelatedBaseStats.Add(baseStats);
        }

        public bool RemoveRelatedBaseStats(BaseStats baseStats)
        {
            return RelatedBaseStats.Remove(baseStats);
        }

        public void AddBasicStatModifiers(StatDictionaryAllInOne statDictionaryAllInOne, object source)
        {
            if (statDictionaryAllInOne == null) return;
            if (statDictionaryAllInOne.BaseValueStatDictionary.Count > 0)
            {
                Debug.LogWarning($"adding base stat will not add any base values, but [{statDictionaryAllInOne}] has base values.");
            }

            foreach (var keyValuePair in statDictionaryAllInOne.FlatModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.Flat, source));
            }

            foreach (var keyValuePair in statDictionaryAllInOne.PercentAddModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.PercentAdd, source));
            }

            foreach (var keyValuePair in statDictionaryAllInOne.PercentMultModifierStatDictionary)
            {
                AddModifier(keyValuePair.Key, new StatModifier(keyValuePair.Value, StatModType.PercentMult, source));
            }
        }

    }


}
