using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkPlayerVrMovement : NetworkBehaviour
{
    void Start()
    {
        GetComponent<NetworkPlayerKeyboardMovement>().enabled = false;
    }
    
    void Update()
    {
        if (!isLocalPlayer)
            return;
        var centerEyeAnchor = NetworkPlayerUtilities.PlayerCenterEyeAnchor(gameObject);
        Debug.Log(centerEyeAnchor.transform.position);
        var anchorPosition = centerEyeAnchor.transform.position;

        var playerTransform = transform;
        anchorPosition.y = playerTransform.position.y;
        playerTransform.position = anchorPosition;
    }
}
