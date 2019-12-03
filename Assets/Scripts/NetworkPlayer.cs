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

        GameObject wrapper = transform.Find(playerType).gameObject;
        wrapper.SetActive(true);
        
        if (isLocalPlayer)
        {
            wrapper.transform.Find("Avatar").gameObject.SetActive(false);
            
            if (playerType.StartsWith("Human"))
            {
                // GameObject.Find("ControlRoom").SetActive(false);
                Debug.Log(GameObject.Find("Simeowlation").transform.Find("HumanView"));
                GameObject.Find("Simeowlation").transform.Find("HumanView").gameObject.SetActive(true);
            }
        } else
        {
            NPU.PlayerCenterEyeAnchor(gameObject).GetComponent<Camera>().enabled = false;
            NPU.PlayerCenterEyeAnchor(gameObject).GetComponent<AudioListener>().enabled = false;
            wrapper.transform.Find("Hands").gameObject.SetActive(false);
            var ovrCameraRig = wrapper.transform.Find("OVRCameraRig").gameObject;
            ovrCameraRig.GetComponent<OVRCameraRig>().enabled = false;
            ovrCameraRig.GetComponent<OVRHeadsetEmulator>().enabled = false;
        }

        string order = "";
        if (playerType == "Human") {
            order = (NPU.PlayerIndex(gameObject) + 1).ToString();
        }
        Transform startPoint = GameObject.Find(playerType + order + "StartPoint").transform;
        Debug.Log(playerType + order + "StartPoint");
        if (startPoint)
        {
            transform.parent = startPoint.parent;
            transform.localPosition = startPoint.localPosition;
            transform.localRotation = startPoint.localRotation;
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
