using UnityEngine;
using Mirror;

public class GetFood : NetworkBehaviour
{
    private bool firstFoodGot = false;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(NetworkPlayerUtilities.LocalPlayerType());
        if (NetworkPlayerUtilities.LocalPlayerType() != "Cat") return;
        
        string triggerName = other.gameObject.name;
       
        if (triggerName == "TeaserTrigger") {
            
            other.gameObject.transform.parent.gameObject.GetComponent<Teaser>().RpcResetTeaser();
            Transform avatarTransform = gameObject.transform.parent;
            int playerIndex = NetworkPlayerUtilities.PlayerIndex(avatarTransform.parent.parent.gameObject);
            GameObject networkGameController = GameObject.Find("NetworkGameController");
            networkGameController.GetComponent<NetworkGameController>().IncrementHumanScore(playerIndex);

            if (!firstFoodGot)
            {
                
            }
        };
    }
}
