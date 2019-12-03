using System;
using Mirror;
using UnityEngine;
using NPU = NetworkPlayerUtilities;

public class NetworkPlayer : NetworkBehaviour
{
    public enum DebugPlayerType { None, Cat, Human, God }
    public DebugPlayerType debugPlayerType;

    [SyncVar] public int[] scores = { 0, 0, 0 };

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
            order = NPU.PlayerIndex(gameObject).ToString();
        }
        Debug.Log(playerType + order + "StartPoint");
        Transform startPoint = GameObject.Find(playerType + order + "StartPoint").transform;
        
        if (startPoint)
        {
            transform.parent = startPoint.parent;
            transform.localPosition = startPoint.localPosition;
            transform.localRotation = startPoint.localRotation;
        }

        // Debug.Log(GameObject.Find("Rod").GetComponent<NetworkIdentity>().hasAuthority);
    }

    [Command]
    public void CmdIncrementScore(int index) {
        scores[index] += 1;
    }
}
