using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkGameController : NetworkBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;
    public AudioSource audioSource4;
    public AudioSource audioSource5;
    public AudioSource audioSource6;
    public AudioSource audioSource7;
    public AudioSource audioSource8;
    public AudioSource audioSource9;

    public int state = 0;
    private readonly int[] _scores = { 0, 0 };

    void Start()
    {
        Invoke(nameof(IncrementState), 10.0f);
    }

    void Update()
    {
      
    }

    public void IncrementState()
    {
        state++;

        if (isServer && state == 1)
        {
            float prevLength = 0f;
            audioSource1.Play();
            prevLength += audioSource1.clip.length;
            audioSource2.PlayDelayed(prevLength);
            prevLength += audioSource2.clip.length;
            audioSource3.PlayDelayed(prevLength);
            prevLength += audioSource3.clip.length;
            audioSource4.PlayDelayed(prevLength);
            prevLength += audioSource4.clip.length;
            audioSource5.PlayDelayed(prevLength);
            prevLength += audioSource5.clip.length;
            Invoke(nameof(IncrementState), prevLength);
        }
        
        if (isServer && state == 2)
        {
            float prevLength = 0f;
            prevLength += 3f;
            audioSource3.PlayDelayed(prevLength);
            prevLength += audioSource3.clip.length;
            audioSource4.PlayDelayed(prevLength);
            prevLength += audioSource4.clip.length;
            audioSource5.PlayDelayed(prevLength);
        }
    }

    public void GoToState(int nextState)
    {
        state = nextState;

        if (isServer && state == 3)
        {
            float prevLength = 0f;
         
            audioSource6.PlayDelayed(prevLength);
            prevLength += audioSource6.clip.length;
            audioSource7.PlayDelayed(prevLength);
            prevLength += audioSource7.clip.length + 5f;
            audioSource6.PlayDelayed(prevLength);
            prevLength += audioSource6.clip.length;
            audioSource7.PlayDelayed(prevLength);
        }

        if (isServer && state == 4)
        {
            audioSource8.PlayDelayed(3f);
            Invoke(nameof(IncrementState), 3f + audioSource8.clip.length);
        }
        
        if (isServer && state == 5)
        {
            
            audioSource9.PlayDelayed(3f);
        }
    }
    
    public void IncrementHumanScore(int playerIndex)
    {
        _scores[playerIndex - 1]++;
        RpcUpdateScore(_scores);
        if (_scores[playerIndex - 1] == 10)
        {
            GoToState(4);
            Debug.Log("revealed");
            RpcReveal();
        }
            

    }
    
    [ClientRpc]
    private void RpcUpdateScore(int[] score)
    {
        Debug.Log("RPC called");
        for (var i = 0; i < 2; i++)
        {
            var scoreText = "Human " + (i + 1) + " Score: " + score[i];
            GameObject.Find("Human" + (i + 1) + "Score").GetComponent<Text>().text = scoreText;

            if (score[i] == 16 && NetworkPlayerUtilities.LocalPlayerType() == "Human")
            {
                GameObject.Find("HumanView").SetActive(false);
                GameObject.Find("Content").transform.Find("Simeowlation").gameObject.SetActive(true);
            }
        }
    }
    
    [ClientRpc]
    private void RpcReveal()
    {
        GameObject.Find("HumanView").SetActive(false);
        GameObject.Find("Content").transform.Find("ControlRoom").gameObject.SetActive(true);
    }
}
