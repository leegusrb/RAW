using System;
using System.Text;
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

		private const int CurrentProtocolVersion = 1;
		private const int MaxUserIdLength = 64;
		private const int MaxPayloadBytes = 1024;

		[SerializeField]
		private string developmentUserId = "dev-player";

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
			networkManager.NetworkConfig.ConnectionApproval = true;
        }

        private void OnEnable()
        {
            if (networkManager == null)
                return;

			networkManager.ConnectionApprovalCallback = HandleConnectionApproval;
            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            if (networkManager == null)
                return;

			if (networkManager.ConnectionApprovalCallback == HandleConnectionApproval)
				networkManager.ConnectionApprovalCallback = null;

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

		public bool ConfigureIdentity(string userId)
		{
			if (IsListening)
			{
				Debug.LogWarning("네트워크 실행 중에는 사용자 정보를 변경할 수 없습니다.", this);
				return false;
			}

			if (!TryNormalizeUserId(userId, out string normalizeUserId))
			{
				Debug.LogError("사용자 ID가 올바르지 않습니다.", this);
				return false;
			}

			developmentUserId = normalizeUserId;
			return true;
		}

		private static bool TryNormalizeUserId(string userId, out string normalizeUserId)
		{
			normalizeUserId = null;

			if (string.IsNullOrWhiteSpace(userId))
				return false;

			string trimmedUserId = userId.Trim();

			if (trimmedUserId.Length > MaxUserIdLength)
				return false;

			for (int i = 0; i < trimmedUserId.Length; i++)
			{
				char character = trimmedUserId[i];

				bool isAllowed =
					char.IsLetterOrDigit(character) ||
					character == '-' ||
					character == '_' ||
					character == '.';

				if (!isAllowed)
					return false;
			}

			normalizeUserId = trimmedUserId;
			return true;
		}

		private bool TryConfigureConnectionPayload()
		{
			if (!TryNormalizeUserId(developmentUserId, out string normalizeUserId))
			{
				Debug.LogError("개발용 사용자 ID가 올바르지 않습니다.", this);
				return false;
			}

			NetworkConnectionPayload payload =
				new NetworkConnectionPayload
				{
					protocolVersion = CurrentProtocolVersion,
					userId = normalizeUserId
				};

			string json = JsonUtility.ToJson(payload);
			byte[] payloadBytes = Encoding.UTF8.GetBytes(json);

			if (payloadBytes.Length > MaxPayloadBytes)
			{
				Debug.LogError($"접속 데이터가 너무 큽니다: {payloadBytes.Length} bytes", this);
				return false;
			}

			networkManager.NetworkConfig.ConnectionData = payloadBytes;
			return true;
		}

        public bool StartHost()
        {
			if (!CanStart() || !TryConfigureConnectionPayload())
				return false;
			
            return networkManager.StartHost();
        }

        public bool StartClient()
        {
			if (!CanStart() || !TryConfigureConnectionPayload())
				return false;

            return networkManager.StartClient();
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

		private void HandleConnectionApproval(
			NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response
		)
		{
			response.Approved = false;
			response.CreatePlayerObject = false;
			response.PlayerPrefabHash = null;
			response.Position = null;
			response.Rotation = null;
			response.Pending = false;
			response.Reason = null;

			if (request.Payload == null || request.Payload.Length == 0)
			{
				RejectConnection(response, "접속 정보가 없습니다.");
				return;
			}

			if (request.Payload.Length > MaxPayloadBytes)
			{
				RejectConnection(response, "접속 정보가 너무 큽니다.");
				return;
			}

			NetworkConnectionPayload payload;

			try
			{
				string json = Encoding.UTF8.GetString(request.Payload);

				payload = JsonUtility.FromJson<NetworkConnectionPayload>(json);
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"접속 정보 해석 실패: {exception.Message}", this);

				RejectConnection(response, "접속 정보 형식이 올바르지 않습니다.");

				return;
			}

			if (payload == null)
			{
				RejectConnection(response, "접속 정보 형식이 올바르지 않습니다.");
				return;
			}

			if (payload.protocolVersion != CurrentProtocolVersion)
			{
				RejectConnection(response, "클라이언트 서버 버전이 다릅니다.");
				return;
			}

			if (!TryNormalizeUserId(payload.userId, out string normalizeUserId))
			{
				RejectConnection(response, "사용자 ID가 올바르지 않습니다.");
				return;
			}

			response.Approved = true;
			response.CreatePlayerObject = true;

			Debug.Log(
				$"접속 승인: " +
				$"ClientId={request.ClientNetworkId}, " +
				$"UserId={normalizeUserId}, " +
				$"Protocol={payload.protocolVersion}",
				this
			);
		}

		private void RejectConnection(
			NetworkManager.ConnectionApprovalResponse response,
			string reason
		)
		{
			response.Approved = false;
			response.CreatePlayerObject = false;
			response.Pending = false;
			response.Reason = reason;

			Debug.LogWarning($"접속 거절: {reason}", this);
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
