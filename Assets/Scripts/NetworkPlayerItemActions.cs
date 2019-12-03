using UnityEngine;
using Mirror;

public class NetworkPlayerItemActions : NetworkBehaviour
{
    GameObject _rod;

    void Awake()
    {
        _rod = GameObject.Find("Rod");
    }

    void Update()
    {
        if (!Input.GetKeyDown("space"))
            return;
        if (!(NetworkPlayerUtilities.LocalPlayerType() == "Cat" && isLocalPlayer))
            return;
        
        Vector3 diff = transform.position - _rod.transform.position;
        diff.y = 0;
        float distance = diff.magnitude;
        Debug.Log(distance);
        if (distance < 2.5f)
        {
            CmdSetRodParent();
            _rod.transform.localPosition = new Vector3(0.22f, 0, 0.43f);
            _rod.transform.localRotation = Quaternion.Euler(66f, -23f, 0);
        }
        
    }

    // [Command]
    // public void CmdSetRodAuthority()
    // {
    //     _rod.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToServer); 
    // }

    [Command]
    void CmdSetRodParent()
    {
        GameObject centerEyeAnchor = NetworkPlayerUtilities.PlayerCenterEyeAnchor(gameObject);
        _rod.transform.parent = centerEyeAnchor.transform;
    }
}
