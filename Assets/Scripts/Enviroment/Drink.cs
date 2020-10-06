using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drink : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
       if(collider.tag == "Animal")
       {
            collider.GetComponent<AnimalUnit>().CanDrink = true;
       }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Animal")
        {
            collider.GetComponent<AnimalUnit>().CanDrink = false;
        }
    }
}
