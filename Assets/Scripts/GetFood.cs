using UnityEngine;
using Mirror;

public class GetFood : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        
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
