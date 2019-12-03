using System;
using UnityEngine;

public class CatActions : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "StartGameZone")
        {
            Debug.Log("Cat entered start game zone");
            GameObject.Find(nameof(NetworkGameController)).GetComponent<NetworkGameController>().GoToState(3);
        }
    }
}
