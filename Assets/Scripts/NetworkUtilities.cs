using System;
using UnityEngine;

public class NetworkUtilities : MonoBehaviour
{
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
        switch (playerIndex)
        {
            case 0:
                return "God";
            case 1:
                return "Cat";
            case 2:
                return "Human 1";
            case 3:
                return "Human 2";
            default:
                return "Unknown";
        }
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
