using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // GetComponent<Rigidbody>().isKinematic = !isLocalPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        string triggerName = other.gameObject.name;
        GameObject activeChild = transform.Find(triggerName).gameObject;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        activeChild.SetActive(true);
    }
}
