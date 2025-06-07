using System.Collections.Generic;
using UnityEngine;

public class EnemyPersist : MonoBehaviour
{
    private static List<EnemyPersist> _instances = new List<EnemyPersist>();

    void Awake()
    {
        foreach (var inst in _instances)
        {
            if (inst.gameObject.name == gameObject.name)
            {
                Destroy(gameObject);
                return;
            }
        }
        _instances.Add(this);
        DontDestroyOnLoad(this.gameObject);
    }

    void OnDestroy()
    {
        _instances.Remove(this);
    }
}
