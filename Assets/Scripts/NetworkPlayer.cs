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
        if (isServer)
        {
            cat.SetActive(false);
            human1.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
