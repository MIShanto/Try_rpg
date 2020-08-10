using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class EnemySpawnManager : MonoBehaviour
{
    public GameObject[] spawnObject;

    public Transform[] spawnPositions;
    ObjectPooler objectPooler;

    [Tooltip("In seconds")]
    public float spawnInterval;
    public float spawnLimit;
    int i = 0;
    int swordmanCount = 0;
    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        SpawnEnemy();
        StartCoroutine(WaitToSpawn());
    }

    IEnumerator WaitToSpawn()
    {
        
        yield return new WaitForSeconds(spawnInterval);
        SpawnEnemy();
        StartCoroutine(WaitToSpawn());
    }

    void SpawnEnemy()
    {
        if (i < spawnLimit)
        {
            int spawnObjectIndex = UnityEngine.Random.Range(0, spawnObject.Length);
            int spawnPositionIndex = UnityEngine.Random.Range(0, spawnPositions.Length);
            //objectPooler.SpawnFromPool("Arrow", new Vector2(0,0), Quaternion.identity);
            GameObject currentObject = objectPooler.SpawnFromPool(spawnObject[spawnObjectIndex].name, (Vector2)spawnPositions[spawnPositionIndex].position, Quaternion.identity) as GameObject;

            if (spawnObject[spawnObjectIndex].name == "Witch unit")
            {
                for (int i = 0; i < currentObject.transform.childCount; i++)
                {
                    currentObject.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
            i++;
        }
    }

    internal void HandleSwordman()
    {
        swordmanCount++;
        if(swordmanCount == 3)
        {
            i--;
            swordmanCount = 0;
        }
    }
    internal void HandleOther()
    {
        i--;
    }
}
