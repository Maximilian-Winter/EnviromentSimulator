using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Animal")
        {
            collider.GetComponent<AnimalUnit>().CanEat = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Animal")
        {
            collider.GetComponent<AnimalUnit>().CanEat = false;
        }
    }
}
