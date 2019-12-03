using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        string triggerName = other.gameObject.name;
        Debug.Log(triggerName);
        var food = GameObject.Find("Food").transform;
        var activeFood = food.Find(triggerName);
        if (!activeFood) return; 
        foreach (Transform child in food)
        {
            child.gameObject.SetActive(false);
        }
        activeFood.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
