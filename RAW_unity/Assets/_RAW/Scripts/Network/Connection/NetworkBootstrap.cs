using System;
using System.Collections.Generic;
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

		public event Action<ulong, string> UserConnected;
		public event Action<ulong, string> UserDisconnected;

        public bool IsListening => networkManager != null && networkManager.IsListening;

		private const int CurrentProtocolVersion = 1;
		private const int MaxUserIdLength = 64;
		private const int MaxPayloadBytes = 1024;

		private readonly Dictionary<ulong, string> userIdByClientId =
			new Dictionary<ulong, string>();
		
		private readonly Dictionary<string, ulong> clientIdByUserId =
			new Dictionary<string, ulong>(
				StringComparer.Ordinal
			);

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

			ClearUserIdentities();
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

			ClearUserIdentities();
			
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
            if (!CanStart())
				return false;

			ClearUserIdentities();
			
			return networkManager.StartServer();
        }

        public void Shutdown()
        {
            if (!IsListening)
                return;

            networkManager.Shutdown();
        }

		public bool TryGetUserId(ulong clientId, out string userId)
		{
			return userIdByClientId.TryGetValue(
				clientId,
				out userId
			);
		}

		public bool TryGetClientId(string userId, out ulong clientId)
		{
			clientId = default;

			if (!TryNormalizeUserId(userId, out string normalizeUserId))
				return false;

			return clientIdByUserId.TryGetValue(
				normalizeUserId,
				out clientId
			);
		}

		public bool IsUserConnected(string userId)
		{
			return TryGetClientId(userId, out _);
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

			if (!networkManager.IsServer)
				return;

			if (!TryGetUserId(clientId, out string userId))
			{
				Debug.LogError($"승인된 사용자의 ID 매핑을 찾을 수 없습니다. ClientId={clientId}", this);
				return;
			}

			Debug.Log(
				$"사용자 연결 완료: " +
				$"ClientId={clientId}, " +
				$"UserId={userId}",
				this
			);

			UserConnected?.Invoke(clientId, userId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"클라이언트 연결 종료: {clientId}", this);

			if (networkManager.IsServer && userIdByClientId.TryGetValue(clientId, out string userId))
			{
				Debug.Log(
					$"사용자 연결 종료: " +
					$"ClientId={clientId}, " +
					$"UserId={userId}",
					this
				);

				UserDisconnected?.Invoke(clientId, userId);

				RemoveUserIdentity(clientId, out _);
			}

			if (!networkManager.IsServer &&
				clientId == networkManager.LocalClientId &&
				!string.IsNullOrWhiteSpace(networkManager.DisconnectReason))
			{
				Debug.LogWarning($"서버 연결 종료 사유: {networkManager.DisconnectReason}", this);
			}

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

			if (!TryReserveUserIdentity(request.ClientNetworkId, normalizeUserId, out string rejectionReason))
			{
				RejectConnection(response, rejectionReason);
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

		private bool TryReserveUserIdentity(
			ulong clientId,
			string userId,
			out string rejectionReason
		)
		{
			rejectionReason = null;

			if (userIdByClientId.TryGetValue(clientId, out string existingUserId))
			{
				if (string.Equals(existingUserId, userId, StringComparison.Ordinal))
					return true;

				rejectionReason = "하나의 연결에서 여러 사용자의 ID를 사용할 수 없습니다.";

				return false;
			}

			if (clientIdByUserId.TryGetValue(userId, out ulong existingClientId))
			{
				rejectionReason = $"이미 접속중인 사용자입니다. UserId={userId}";

				Debug.LogWarning(
					$"중복 사용자 접속 거절: " +
					$"UserId={userId}, " +
					$"ExistingClientId={existingClientId}, " +
					$"RequestClientId={clientId}",
					this
				);

				return false;
			}

			userIdByClientId[clientId] = userId;
			clientIdByUserId[userId] = clientId;

			return true;
		}

		private bool RemoveUserIdentity(ulong clientId, out string userId)
		{
			if (!userIdByClientId.TryGetValue(clientId, out userId))
				return false;

			userIdByClientId.Remove(clientId);

			if (clientIdByUserId.TryGetValue(userId, out ulong mappedClientId) &&
				mappedClientId == clientId)
			{
				clientIdByUserId.Remove(userId);
			}

			return true;
		}

		private void ClearUserIdentities()
		{
			userIdByClientId.Clear();
			clientIdByUserId.Clear();
		}
    }
}
