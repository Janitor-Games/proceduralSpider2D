using UnityEngine;
using System.Collections.Generic;

public class objectPoolers : MonoBehaviour
{
    [Header("General Values And References")]
    public List<GameObject> objs;
    public List<int> amounts;
    private List<GameObject>[] pools;

    void Start()
    {
        pools = new List<GameObject>[objs.Count];
        for (int x = 0; x < pools.Length; x++)
        {
            List<GameObject> pool = new List<GameObject>();
            for (int x1 = 0; x1 < amounts[x]; x1++)
            {
                var inst = Instantiate(objs[x]);
                inst.SetActive(false);
                pool.Add(inst);
            }
            pools[x] = pool;
        }
    }

    public GameObject getObjFromPool(int x)
    {
        for (int x1 = 0; x1 < pools[x].Count; x1++)
        {
            if (!pools[x][x1].activeInHierarchy) return pools[x][x1];
        }
        return null;
    }
}
