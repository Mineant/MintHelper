using System.Collections;
using System.Collections.Generic;
using Mineant;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Mineant
{
    [CreateAssetMenu]
    public class BasicStats : ScriptableObject
    {
        public StatDictionaryAllInOne StatDictionaryAllInOne;

        [ContextMenu("AddSkillStats")]
        protected void AddSkillStats()
        {
            // if (!StatDictionaryAllInOne.IsAllEmpty())
            // {
            //     Debug.LogError("Must all stat be empty");
            //     return;
            // }

            var baseDict = new StatFloatDictionary();
            baseDict[Stat.QiCost] = 25;
            baseDict[Stat.AuraSkillCost] = 1;
            baseDict[Stat.AuraRegen] = 25;
            baseDict[Stat.TrueDamage] = 0f;
            baseDict[Stat.PhysicalDamage] = 0f;
            baseDict[Stat.SpellDamage] = 0f;
            baseDict[Stat.RecoveryTime] = 0f;
            baseDict[Stat.SkillTime] = 0f;
            baseDict[Stat.BurstLength] = 1;
            baseDict[Stat.ProjectilesPerShot] = 1;
            baseDict[Stat.Spread] = 0f;
            baseDict[Stat.Range] = 1;
            baseDict[Stat.Recoil] = 0f;

            foreach (var keyValuePair in baseDict)
            {
                if (StatDictionaryAllInOne.BaseValueStatDictionary.ContainsKey(keyValuePair.Key)) continue;

                StatDictionaryAllInOne.BaseValueStatDictionary[keyValuePair.Key] = keyValuePair.Value;
            }

            // StatDictionaryAllInOne.BaseValueStatDictionary = baseDict;
        }

        [ContextMenu("GenerateSkillBookStats")]
        public void GenerateSkillBookStats()
        {
            if (!StatDictionaryAllInOne.IsAllEmpty())
            {
                Debug.LogError("Must all stat be empty");
                return;
            }

            var baseDict = new StatFloatDictionary();
            baseDict[Stat.SkillCapacity] = 3;
            StatDictionaryAllInOne.BaseValueStatDictionary = baseDict;

            var flatDict = new StatFloatDictionary();
            flatDict[Stat.MaxQi] = 100;
            flatDict[Stat.QiRegen] = 30;
            flatDict[Stat.RecoveryTime] = 1;
            flatDict[Stat.SkillTime] = 0.4f;
            StatDictionaryAllInOne.FlatModifierStatDictionary = flatDict;
        }

        [ContextMenu("GenerateEnemyStats")]
        protected void GenerateEnemyStats()
        {
            if (!StatDictionaryAllInOne.IsAllEmpty())
            {
                Debug.LogError("Must all stat be empty");
                return;
            }

            var baseDict = new StatFloatDictionary();
            baseDict[Stat.Health] = 50;
            StatDictionaryAllInOne.BaseValueStatDictionary = baseDict;
        }


    }

    public enum Stat
    {
        Health = 0, // Character Stats

        QiCost = 100, // Skill Stats
        AuraSkillCost,
        AuraRegen,

        TrueDamage = 200, // Damage Type Stats
        PhysicalDamage,
        SpellDamage,

        BaseDamageReceived = 250, // Damage Resistence.


        SkillCapacity = 300, // Skill Book Stats
        MaxQi,
        QiRegen,
        RecoveryTime,
        SkillTime,

        BurnDamage = 350, // Damage Stat Modifier
        LightningStrikeDamage,

        BurstLength = 400, // Weapon Stats
        ProjectilesPerShot,
        Spread,
        Range,
        Recoil,

    }


    [System.Serializable]
    public class StatFloatDictionary : UnitySerializedDictionary<Stat, float> { }

    [System.Serializable]
    public class StatDictionaryAllInOne
    {
        [Space]
        public StatFloatDictionary BaseValueStatDictionary;

        [Space]
        public StatFloatDictionary FlatModifierStatDictionary;

        [Space]
        public StatFloatDictionary PercentAddModifierStatDictionary;

        [Space]
        public StatFloatDictionary PercentMultModifierStatDictionary;

        public bool IsAllEmpty()
        {
            return BaseValueStatDictionary.Count == 0 && FlatModifierStatDictionary.Count == 0 && PercentAddModifierStatDictionary.Count == 0 && PercentMultModifierStatDictionary.Count == 0;
        }
    }

}
