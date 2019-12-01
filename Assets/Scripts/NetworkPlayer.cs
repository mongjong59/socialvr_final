using System;
using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject god;
    public GameObject cat;
    public GameObject human1;
    public GameObject human2;

    public Boolean debug;
    
    void Start()
    {
        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        Array.Sort(players, compareNetId);
        int playerIndex = Array.IndexOf(players, GetComponent<NetworkPlayer>());

        if (playerIndex > 0)
            god.SetActive(false);

        if (playerIndex == 1)
        {
            cat.SetActive(true);
        }

        if (playerIndex == 2 || playerIndex == 3)
        {
            transform.parent = GameObject.FindWithTag("Simeowlation").transform;
            transform.localRotation = Quaternion.Euler(0, 180f, 0);
            GameObject.FindWithTag("Control Room").transform.localScale *= 10;
            transform.localScale *= 0.1f;
            if (playerIndex == 2)
            {
                human1.SetActive(true);
                transform.localPosition = new Vector3(0, 0, 0);
            }
            if (playerIndex == 3)
            {
                human2.SetActive(true);
                transform.localPosition = new Vector3(0, 0, 0);
            }
            
        }

        Debug.Log(isLocalPlayer);
        if (!isLocalPlayer)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private int compareNetId(NetworkPlayer a, NetworkPlayer b)
    {
        return a.netId.CompareTo(b.netId);
    }

}
