using UnityEngine;

namespace Mirror
{
   public class NetworkSetup : MonoBehaviour
    {
        NetworkManager _networkManager;
        public GameObject godCamera;

        public bool server;

        void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }
        
        void Start()
        {
            if (server)
            {
                InvokeRepeating("startServer", 0f, 1f);
            } else
            {
                InvokeRepeating("startClient", 0f, 1f);
            }
        }

        void startServer()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer && !NetworkServer.active)
            {
                _networkManager.StartHost();
            }
        }

        void startClient()
        {
            if (!NetworkClient.isConnected)
            {
                _networkManager.StartClient();
            }
        }
    }
}


