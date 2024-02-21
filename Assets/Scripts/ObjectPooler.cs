using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject pooledObject; // Prefab of the object to be pooled
    public int pooledAmount = 10;   // Initial pooled amount
    public bool willGrow = true;    // Should the pool grow if required?

    private List<GameObject> pooledObjects;

    void Start()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Instantiate(pooledObject, transform);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                pooledObjects[i].SetActive(true);
                return pooledObjects[i];
            }
        }

        // If no available object is found and the pool can grow, create a new one
        if (willGrow)
        {
            GameObject obj = Instantiate(pooledObject, transform);
            pooledObjects.Add(obj);
            return obj;
        }

        return null; // Return null if no object is available and pool can't grow
    }
}
