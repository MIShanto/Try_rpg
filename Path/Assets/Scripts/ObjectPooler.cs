using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    #region singleton

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion
    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.name, objectPool);
        }
    }

    /// <summary>
    /// This method is the alternative of Instansiate.
    /// It spwan the object from pool
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string name, Vector2 position, Quaternion rotation)
    {
        if(!poolDictionary.ContainsKey(name))
        {
            Debug.LogWarning("Pool with name" + name + "doesn't exist");
            return null;
        }

        GameObject objectToSpwan = poolDictionary[name].Dequeue();
        
        objectToSpwan.SetActive(true);
        objectToSpwan.transform.position = position;
        objectToSpwan.transform.rotation = rotation;

        poolDictionary[name].Enqueue(objectToSpwan);

        return objectToSpwan;
    }
}
