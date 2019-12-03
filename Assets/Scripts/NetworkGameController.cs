using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkGameController : NetworkBehaviour
{
    private readonly int[] _scores = { 0, 0 };

    public void IncrementHumanScore(int playerIndex)
    {
        _scores[playerIndex - 1]++;
        RpcUpdateScore(_scores);
    }
    
    [ClientRpc]
    private void RpcUpdateScore(int[] score)
    {
        Debug.Log("RPC called");
        for (var i = 0; i < 2; i++)
        {
            var scoreText = "Human " + (i + 1) + " Score: " + score[i];
            GameObject.Find("Human" + (i + 1) + "Score").GetComponent<Text>().text = scoreText;
        }
    }
}
