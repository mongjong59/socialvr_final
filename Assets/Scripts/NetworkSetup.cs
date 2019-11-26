using UnityEngine;

namespace Mirror
{
   public class NetworkSetup : MonoBehaviour
    {
        NetworkManager _networkManager;
        
        void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }
        
        void Start()
        {
            if (!NetworkServer.active)
            {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    _networkManager.StartHost();
                }
            }

            if (NetworkServer.active && !NetworkClient.active)
            {
                if (!NetworkClient.active)
                {
                    _networkManager.StartClient();
                }
            }
        }
    }
}


