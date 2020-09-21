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

    private int enemyCount = 0;
    private int swordmanCount = 0;
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
        if (enemyCount < spawnLimit)
        {
            int spawnObjectIndex = UnityEngine.Random.Range(0, spawnObject.Length);
            int spawnPositionIndex = UnityEngine.Random.Range(0, spawnPositions.Length);
            //objectPooler.SpawnFromPool("Arrow", new Vector2(0,0), Quaternion.identity);
            GameObject currentObject = objectPooler.SpawnFromPool(spawnObject[spawnObjectIndex].name, (Vector2)spawnPositions[spawnPositionIndex].position, Quaternion.identity) as GameObject;

            if (spawnObject[spawnObjectIndex].name == "Witch unit")
            {
                Debug.Log("WHY???");
                for (int i = 0; i < currentObject.transform.childCount; i++)
                {
                    currentObject.transform.GetChild(i).gameObject.SetActive(true);
                }
                enemyCount += 3;
            }
            else
            {
                enemyCount++;
            }
        }
    }

    internal void HandleSwordman()
    {
        swordmanCount++;
        if(swordmanCount == 3)
        {
            enemyCount--;
            swordmanCount = 0;
        }
    }
    internal void HandleOther()
    {
        enemyCount--;
    }
}
