using System;
using UnityEngine;

public class NetworkPlayerUtilities : MonoBehaviour
{
    public static string[] PlayerTypes()
    {
        string[] playerTypes = { "Cat", "Human1", "Human2", "God" };
        return playerTypes;
    }

    public static string LocalPlayerType()
    {
        NetworkPlayer localPlayer = new NetworkPlayer();
        foreach (NetworkPlayer player in Players())
        {
            if (player.isLocalPlayer)
            {
                localPlayer = player;
                break;
            }
        }
        return PlayerType(localPlayer.gameObject);
    }

    public static string PlayerType(GameObject player)
    {
    
        int playerIndex = Array.IndexOf(Players(), player.GetComponent<NetworkPlayer>());
        return PlayerTypes()[playerIndex];    
        
    }

    public static Camera PlayerCamera(GameObject player)
    {
        Transform wrapper = player.transform.Find(PlayerType(player));
        Transform centerEyeAnchor = wrapper.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor");
        return centerEyeAnchor.GetComponent<Camera>();
    }

    private static NetworkPlayer[] Players()
    {

        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        Array.Sort(players, CompareNetId);
        return players;
    }

    private static int CompareNetId(NetworkPlayer a, NetworkPlayer b)
    {
        return a.netId.CompareTo(b.netId);
    }
}
