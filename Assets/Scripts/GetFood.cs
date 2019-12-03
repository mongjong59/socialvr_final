using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFood : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        string triggerName = other.gameObject.name;
        Debug.Log(triggerName);
        if (triggerName == "TeaserTrigger") {
            Debug.Log(other.gameObject.transform.parent);
            other.gameObject.transform.parent.gameObject.GetComponent<Teaser>().RpcResetTeaser();
        };
        
    }
}
