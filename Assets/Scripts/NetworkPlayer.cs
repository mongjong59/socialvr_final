using System;
using Mirror;
using UnityEngine;
using NPU = NetworkPlayerUtilities;

public class NetworkPlayer : NetworkBehaviour
{
    public enum DebugPlayerType { None, Cat, Human, God }
    public DebugPlayerType debugPlayerType;

    // [SyncVar] public int[] scores = { 0, 0, 0 };

    void Start()
    {
        Debug.Log(GameObject.Find("TeaserTrigger").transform.parent.gameObject.GetComponent<Teaser>());
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
        
        var ovrCameraRig = wrapper.transform.Find("OVRCameraRig").gameObject;
        var centerEyeAnchor = NPU.PlayerCenterEyeAnchor(gameObject);
        
        if (isLocalPlayer)
        {
            wrapper.transform.Find("Avatar").gameObject.SetActive(false);
            GameObject canvas = GameObject.Find("Canvas");
            canvas.transform.parent = centerEyeAnchor.transform;
            canvas.GetComponent<Canvas>().worldCamera = centerEyeAnchor.GetComponent<Camera>();
            canvas.transform.localPosition = new Vector3(-0.39f, 0.25f, 0.67f);

            // if (playerType == "Cat")
            // {
            //    canvas.SetActive(false);
            // }
            
            if (playerType.StartsWith("Human"))
            {
                GameObject.Find("ControlRoom").SetActive(false);
                GameObject.Find("Simeowlation").transform.Find("HumanView").gameObject.SetActive(true);
            }
        } else
        {
            centerEyeAnchor.GetComponent<Camera>().enabled = false;
            centerEyeAnchor.GetComponent<AudioListener>().enabled = false;
            wrapper.transform.Find("Hands").gameObject.SetActive(false);
            
            ovrCameraRig.GetComponent<OVRCameraRig>().enabled = false;
            ovrCameraRig.GetComponent<OVRHeadsetEmulator>().enabled = false;
        }

        string order = "";
        if (playerType == "Human") {
            // order = NPU.PlayerIndex(gameObject);
            order = (NPU.PlayerIndex(gameObject) + 1).ToString();
        }
        Debug.Log(playerType + order + "StartPoint");
        var startPoint = GameObject.Find(playerType + order + "StartPoint");
        
        if (startPoint)
        {
            var startPointTransform = startPoint.transform;
            transform.parent = startPointTransform.parent;
            transform.localPosition = startPointTransform.localPosition;
            transform.localRotation = startPointTransform.localRotation;
        }

        // Debug.Log(GameObject.Find("Rod").GetComponent<NetworkIdentity>().hasAuthority);
    }
}
