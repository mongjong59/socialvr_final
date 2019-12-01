using UnityEngine;
using Mirror;

public class ItemActions : NetworkBehaviour
{
    GameObject rod;

    void Awake()
    {
        rod = GameObject.Find("Rod");
    }

    void Update()
    {


        //if (NetworkUtilities.LocalPlayerType() != "Cat")
        //    return;

        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown("space"))
        {
            Vector3 diff = transform.position - rod.transform.position;
            diff.y = 0;
            float distance = diff.magnitude;
            Debug.Log(distance);
            if (distance < 2.5f)
            {
                Camera camera = GetComponentInChildren<Camera>();
                rod.transform.parent = camera.transform;
                rod.transform.localPosition = new Vector3(0.22f, 0, 0.43f);
                rod.transform.localRotation = Quaternion.Euler(66f, -23f, 0);
            }
        }
    }

    [Command]
    public void CmdSetRodAuthority()
    {
        rod.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToServer); 
    }
}
