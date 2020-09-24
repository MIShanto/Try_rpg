using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class EnemySpawnManager : MonoBehaviour
{
   
    [Serializable]
    public struct SpawnObjects
    {
        [HideInInspector]public int checkLimit;
        public int limit;
        public GameObject characterObject;
    }
    public SpawnObjects[] spawnObject;

    public Transform[] spawnPositions;
    ObjectPooler objectPooler;

    [Tooltip("In seconds")]
    public float spawnInterval;
    public float spawnLimit;

    private int enemyCount = 0;
    private int swordmanCount = 0;
    private void Start()
    {
        for (int i = 0; i < spawnObject.Length; i++)
        {
            spawnObject[i].checkLimit = 0;
        }
        
        objectPooler = ObjectPooler.Instance;
        SpawnEnemy();
    }

    IEnumerator WaitToSpawn()
    {
        
        yield return new WaitForSeconds(spawnInterval);
        SpawnEnemy();
       
    }

    void SpawnEnemy()
    {
        int loopVar = 0;
        int loopLimit = 50;

        while (loopVar < loopLimit)
        {
            if (enemyCount < spawnLimit)
            {
                int spawnObjectIndex = UnityEngine.Random.Range(0, spawnObject.Length);

                if (spawnObject[spawnObjectIndex].checkLimit < spawnObject[spawnObjectIndex].limit)
                {
                    int spawnPositionIndex = UnityEngine.Random.Range(0, spawnPositions.Length);

                    GameObject currentObject = objectPooler.SpawnFromPool(spawnObject[spawnObjectIndex].characterObject.name, (Vector2)spawnPositions[spawnPositionIndex].position, Quaternion.identity) as GameObject;

                    //increase limit
                    spawnObject[spawnObjectIndex].checkLimit++;

                    Debug.Log("name :" + spawnObject[spawnObjectIndex].characterObject.name);

                    if (spawnObject[spawnObjectIndex].characterObject.name == "Witch unit")
                    {
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

                    break;
                }
              
            }
            loopVar++;
        }
        
        //spawn again
        StartCoroutine(WaitToSpawn());
    }

    internal void HandleEnemyForCount(GameObject myGameObject)
    {
        for (int i = 0; i < spawnObject.Length; i++)
        {

            if (myGameObject.tag == spawnObject[i].characterObject.tag)
            {
                spawnObject[i].checkLimit--;
                Debug.LogError(spawnObject[i].checkLimit);
            }
            
        }
        enemyCount--;
       /* if (myGameObject.tag == "Swordman")
        {
            swordmanCount++;
            if (swordmanCount == 3)
            {
                enemyCount--;
                swordmanCount = 0;
            }
        }
        else
        {
            enemyCount--;
        }*/
    }
}
