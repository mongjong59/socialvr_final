using System;
using Mirror;
using UnityEngine;
using NPU = NetworkPlayerUtilities;

public class NetworkPlayer : NetworkBehaviour
{
    public enum DebugPlayerType { None, Cat, Human1, Human2, God }
    public DebugPlayerType debugPlayerType;

    void Start()
    {
        string playerType;
        if (debugPlayerType != DebugPlayerType.None)
        {
            playerType = debugPlayerType.ToString();
            Debug.Log(playerType);
        } else
        {
            playerType = NPU.PlayerType(gameObject);
        }

        transform.Find(playerType).gameObject.SetActive(true);

        // disable camera of non-local player
        if (!isLocalPlayer)
        {
            NPU.PlayerCenterEyeAnchor(gameObject).GetComponent<Camera>().enabled = false;
            NPU.PlayerCenterEyeAnchor(gameObject).GetComponent<AudioListener>().enabled = false;
        }

        Transform startPoint = GameObject.Find(playerType + "StartPoint").transform;
        if (startPoint)
        {
            transform.parent = startPoint.parent;
            transform.localPosition = startPoint.localPosition;
            transform.localRotation = startPoint.localRotation;
        }

        if (playerType.StartsWith("Human"))
        {
            // GameObject.Find("ControlRoom").SetActive(false);
            Debug.Log(GameObject.Find("Simeowlation").transform.Find("HumanView"));
            GameObject.Find("Simeowlation").transform.Find("HumanView").gameObject.SetActive(true);
        }
        //    GameObject.FindWithTag("Control Room").transform.localScale *= 10;
        //    transform.localScale *= 0.1f;
        //    if (playerType == PLAYER_TYPES[1])
        //    {
        //        human1.SetActive(true);
        //        transform.localPosition = new Vector3(0, 0, 0);
        //    }
        //    if (playerType == PLAYER_TYPES[2])
        //    {
        //        human2.SetActive(true);
        //        transform.localPosition = new Vector3(0, 0, 0);
        //    }

        //}



        // Debug.Log(GameObject.Find("Rod").GetComponent<NetworkIdentity>().hasAuthority);
    }
}
