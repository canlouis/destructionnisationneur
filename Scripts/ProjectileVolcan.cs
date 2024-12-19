using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVolcan : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Eau")
        {
            Destroy(gameObject);
        }
    }
}
