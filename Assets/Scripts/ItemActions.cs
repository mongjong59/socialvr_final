using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActions : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey("space"))
        {
            GameObject teaser = GameObject.FindWithTag("Teaser");
            float distance = Vector3.Distance(transform.position, teaser.transform.position);
            if (distance < 2.5f)
            {
                teaser.transform.parent = Camera.current.transform;
                teaser.transform.localPosition = new Vector3(0.22f, 0, 0.43f);
                teaser.transform.localRotation = Quaternion.Euler(66f, -23f, 0);
            }
        }
    }
}
