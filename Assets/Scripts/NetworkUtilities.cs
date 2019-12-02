using System;
using UnityEngine;

public class NetworkUtilities : MonoBehaviour
{
    public static string[] PlayerTypes()
    {
        string[] playerTypes = { "Cat", "Human 1", "Human 2", "God" };
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
        return PlayerType(localPlayer);
    }

    public static string PlayerType(NetworkPlayer player)
    {
    
        int playerIndex = Array.IndexOf(Players(), player.GetComponent<NetworkPlayer>());
        return PlayerTypes()[playerIndex];    
        
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
