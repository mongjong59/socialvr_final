using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Teaser : NetworkBehaviour
{
    [ClientRpc]
    public void RpcResetTeaser()
    {
        foreach (Transform food in transform.Find("Food")) {
            food.gameObject.SetActive(false);
        };
        transform.Find("Hook").gameObject.SetActive(true);
        
        GameObject.Find("TeaserTrigger").GetComponent<BoxCollider>().enabled = false;
    }
}
