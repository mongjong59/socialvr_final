using System;
using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject cat;
    public GameObject human1;
    public GameObject human2;

    public Boolean debug;
    
    void Start()
    {
        Debug.Log(isServer);
        if (isServer)
        {

        } else
        {
            cat.SetActive(false);
            human1.SetActive(true);
            transform.parent = GameObject.FindWithTag("Simeowlation").transform;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.Euler(0, 180f, 0);
            GameObject.FindWithTag("Content").transform.localScale *= 10;
            transform.localScale *= 0.1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
