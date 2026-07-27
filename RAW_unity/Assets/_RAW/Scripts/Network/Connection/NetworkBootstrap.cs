using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace RAW.Network
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport unityTransport;

        public event Action<ulong> ClientConnected;
        public event Action<ulong> ClientDisconnected;

        public bool IsListening => networkManager != null && networkManager.IsListening;

        public NetworkSessionMode CurrentMode
        {
            get
            {
                if (!IsListening)
                    return NetworkSessionMode.Offline;

                if (networkManager.IsHost)
                    return NetworkSessionMode.Host;

                if (networkManager.IsServer)
                    return NetworkSessionMode.Server;

                if (networkManager.IsClient)
                    return NetworkSessionMode.Client;

                return NetworkSessionMode.Offline;
            }
        }

        private void Reset()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();

            if (networkManager == null || unityTransport == null)
            {
                Debug.LogError("NetworkManager 또는 UnityTransport가 없습니다.", this);

                enabled = false;
                return;
            }

            networkManager.NetworkConfig.NetworkTransport = unityTransport;
        }

        private void OnEnable()
        {
            if (networkManager == null)
                return;

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            if (networkManager == null)
                return;

            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        public bool ConfigureEndpoint(string address, ushort port, string listenAddress = null)
        {
            if (IsListening)
            {
                Debug.LogWarning("네트워크 실행 중에는 접속 정보를 변경할 수 없습니다.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(address) || port == 0)
            {
                Debug.LogError("접속 주소 또는 포트가 올바르지 않습니다.", this);
                return false;
            }

            string serverListenAddress = string.IsNullOrWhiteSpace(listenAddress) ? address.Trim() : listenAddress.Trim();

            unityTransport.SetConnectionData(address.Trim(), port, serverListenAddress);

            return true;
        }

        public bool StartHost()
        {
            return CanStart() && networkManager.StartHost();
        }

        public bool StartClient()
        {
            return CanStart() && networkManager.StartClient();
        }

        public bool StartServer()
        {
            return CanStart() && networkManager.StartServer();
        }

        public void Shutdown()
        {
            if (!IsListening)
                return;

            networkManager.Shutdown();
        }

        private bool CanStart()
        {
            if (networkManager == null || unityTransport == null)
                return false;

            if (networkManager.IsListening)
            {
                Debug.LogWarning($"이미 네트워크가 실행 중입니다. {CurrentMode}", this);

                return false;
            }

            return true;
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"클라이언트 연결: {clientId}", this);
            ClientConnected?.Invoke(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"클라이언트 연결 종료: {clientId}", this);
            ClientDisconnected?.Invoke(clientId);
        }

        private void CacheComponents()
        {
            if (networkManager == null)
                networkManager = GetComponent<NetworkManager>();

            if (unityTransport == null)
                unityTransport = GetComponent<UnityTransport>();
        }
    }
}
