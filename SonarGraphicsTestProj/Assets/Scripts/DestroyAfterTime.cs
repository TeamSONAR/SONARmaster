using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{

    void Start()
    {
        Invoke("DeleteSelf", 15);
    }


    void DeleteSelf()
    {
        Destroy(gameObject);
    }
}
