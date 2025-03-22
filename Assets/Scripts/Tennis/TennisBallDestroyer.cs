using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBallDestroyer : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<TennisBall>() != null)
        {
            Destroy(other.gameObject);
        }
    }
}
