using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFood : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        string triggerName = other.gameObject.name;

        if (triggerName == "TeaserTrigger") {
            Debug.Log("Human collided with food");
            other.gameObject.transform.parent.gameObject.GetComponent<Teaser>().RpcResetTeaser();
            Transform avatarTransform = gameObject.transform.parent;
            int playerIndex = NetworkPlayerUtilities.PlayerIndex(avatarTransform.parent.parent.gameObject);
            GameObject networkGameController = GameObject.Find("NetworkGameController");
            networkGameController.GetComponent<NetworkGameController>().IncrementHumanScore(playerIndex);
        };
    }
}
