using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mineant
{
    public class MintLootTableScriptableObject : ScriptableObject
    {

    }


    public class MintLootTableScriptableObject<Q, T, V> : MintLootTableScriptableObject where Q : MintLootTable<T, V> where T : MintLoot<V>
    {
        public Q LootTable;
    }
}




///////////////////// Template. Copy to new class
// TYPE = type of loot
// MyGame = game name

// [CreateAssetMenu]
// public class TYPELootTableSO : MintLootTableScriptableObject<TYPELootTable, TYPELoot, TYPE>
// {

// }

// [System.Serializable]
// public class TYPELootTable : MintLootTable<TYPELoot, TYPE>
// {

// }

// [System.Serializable]
// public class TYPELoot : MintLoot<TYPE>
// {

// }
////////////////////