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
            if (distance < 1.5f)
            {
                teaser.transform.parent = Camera.current.transform;
                teaser.transform.localPosition = new Vector3(0.13f, -0.15f, 0.4f);
                teaser.transform.localRotation = Quaternion.Euler(-45f, -30f, 0);
            }
        }
    }
}
