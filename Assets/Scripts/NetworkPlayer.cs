using System;
using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject god;
    public GameObject cat;
    public GameObject human1;
    public GameObject human2;
    
    void Start()
    {
        string[] PLAYER_TYPES = {"Cat", "Human 1", "Human 2", "God"};
        string playerType = NetworkUtilities.PlayerType(this);

        if (playerType != PLAYER_TYPES[0])
            transform.Find(PLAYER_TYPES[0]).gameObject.SetActive(false);

        //if (playerType == "Cat")
        //{
        //    cat.SetActive(true);
        //    if (isServer)
        //    {
        //        Debug.Log("foobar");
        //        GameObject.Find("Rod").GetComponent<NetworkIdentity>().RemoveClientAuthority();
        //        GameObject.Find("Rod").GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient); 
        //    }
        //}

        if (playerType == PLAYER_TYPES[1] || playerType == PLAYER_TYPES[2])
        {
            transform.parent = GameObject.FindWithTag("Simeowlation").transform;
            transform.localRotation = Quaternion.Euler(0, 180f, 0);
            GameObject.FindWithTag("Control Room").transform.localScale *= 10;
            transform.localScale *= 0.1f;
            if (playerType == PLAYER_TYPES[1])
            {
                human1.SetActive(true);
                transform.localPosition = new Vector3(0, 0, 0);
            }
            if (playerType == PLAYER_TYPES[2])
            {
                human2.SetActive(true);
                transform.localPosition = new Vector3(0, 0, 0);
            }
            
        }

        if (!isLocalPlayer)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }

        Debug.Log(GameObject.Find("Rod").GetComponent<NetworkIdentity>().hasAuthority);
    }
}
