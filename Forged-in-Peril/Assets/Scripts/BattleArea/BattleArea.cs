using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnPoint
{
    public Transform spawnTransform;
}

[System.Serializable]
public class BattleArea
{
    public string battleAreaName;
    public Transform cameraStartTransform;
    public Transform cameraEndTransform;
    public Transform characterParent;
    public Transform enemyParent;
    public SpawnPoint characterSpawnPoint;
    public SpawnPoint enemySpawnPoint;
}
