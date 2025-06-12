using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.SaveSystem
{
    public class ExampleGameData : GameData
    {
        public string ExampleString;
    }

    public class ExampleFileDataHandler : FileDataHandler<ExampleGameData>
    {


    }

    public class ExampleSaveManager : DataPersistenceManager<ExampleSaveManager, ExampleFileDataHandler, ExampleGameData>
    {

    }
}