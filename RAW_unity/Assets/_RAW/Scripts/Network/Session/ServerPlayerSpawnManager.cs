using System;
using RAW.Persistence;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkManager))]
	public sealed class ServerPlayerSpawnManager : MonoBehaviour
	{
		[SerializeField] private NetworkManager networkManager;
		[SerializeField] private ServerPlayerSessionManager sessionManager;

		[Tooltip("비어 있으면 NetworkManager의 PlayerPrefab을 사용합니다.")]
		[SerializeField]
		private GameObject networkPlayerPrefab;

		[SerializeField]
		private Transform[] spawnPoints;

		public event Action<ulong, NetworkObject, PlayerPersistentData> PlayerSpawned;

		private void Awake()
		{
			if (networkManager == null)
				networkManager = GetComponent<NetworkManager>();

			if (sessionManager == null)
				sessionManager = GetComponent<ServerPlayerSessionManager>();
		}

		private void OnEnable()
		{
			if (sessionManager == null)
				return;

			sessionManager.PlayerDataLoaded += HandlePlayerDataLoaded;
			sessionManager.PlayerDataLoadFailed += HandlePlayerDataLoadFailed;
		}

		private void OnDisable()
		{
			if (sessionManager == null)
				return;

			sessionManager.PlayerDataLoaded -= HandlePlayerDataLoaded;
			sessionManager.PlayerDataLoadFailed -= HandlePlayerDataLoadFailed;
		}

		private void HandlePlayerDataLoaded(ulong clientId, PlayerPersistentData playerData)
		{
			if (networkManager == null || !networkManager.IsServer)
				return;

			if (playerData == null)
			{
				FailPlayerSpawn(clientId, "로드된 플레이어 데이터가 없습니다.");
				return;
			}

			if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient))
			{
				Debug.LogWarning($"데이터 로드 후 클라이언트가 이미 종료되었습니다. ClientId={clientId}", this);
				return;
			}

			if (networkClient.PlayerObject != null)
			{
				Debug.LogWarning(
					$"PlayerObject가 이미 존재합니다. " +
					$"ClientId={clientId}, " +
					$"NetworkObjectId={networkClient.PlayerObject.NetworkObjectId}",
					this
				);
				return;
			}
			
			GameObject playerPrefab = ResolvePlayerPrefab();

			if (playerPrefab == null)
			{
				FailPlayerSpawn(clientId, "NetworkPlayer Prefab이 연결되지 않았습니다.");
				return;
			}

			if (!playerPrefab.TryGetComponent(out NetworkObject prefabNetworkObject))
			{
				FailPlayerSpawn(clientId, "NetworkPlayer Prefab에 NetworkObject가 없습니다.");
				return;
			}

			ResolveSpawnPose(
				clientId,
				playerPrefab,
				out Vector3 spawnPosition,
				out Quaternion spawnRotation
			);

			GameObject playerInstance =
				Instantiate(
					playerPrefab,
					spawnPosition,
					spawnRotation
				);

			if (!playerInstance.TryGetComponent(out NetworkObject playerNetworkObject))
			{
				Destroy(playerInstance);

				FailPlayerSpawn(clientId, "생성된 플레이어에 NetworkObject가 없습니다.");
				return;
			}

			try
			{
				playerNetworkObject.SpawnAsPlayerObject(clientId, false);
			}
			catch (Exception exception)
			{
				if (playerNetworkObject.IsSpawned)
					playerNetworkObject.Despawn(true);
				else
					Destroy(playerInstance);

				Debug.LogError(
					$"PlayerObject Spawn 실패: " +
					$"ClientId={clientId}, " +
					$"Error={exception.Message}",
					this
				);

				FailPlayerSpawn(clientId, "플레이어 생성에 실패했습니다.");

				return;
			}

			if (!playerInstance.TryGetComponent(out NetworkCharacterState networkCharacterState))
			{
				if (playerNetworkObject.IsSpawned)
					playerNetworkObject.Despawn(true);
				else
					Destroy(playerInstance);

				FailPlayerSpawn(clientId, "생성된 플레이어에 NetworkCharacterState가 없습니다.");
				return;
			}

			bool stateApplied =
				networkCharacterState.InitializePersistentStateOnServer(
					playerData.healthPoint,
					playerData.manaPoint
				);

			if (!stateApplied)
			{
				if (playerNetworkObject.IsSpawned)
					playerNetworkObject.Despawn(true);
				else
					Destroy(playerInstance);

				FailPlayerSpawn(clientId, "플레이어 초기 상태 적용에 실패했습니다.");
				return;
			}

			Debug.Log(
				$"데이터 로드 후 PlayerObject 생성 완료: " +
				$"ClientId={clientId}, " +
				$"UserId={playerData.userId}, " +
				$"NetworkObjectId={playerNetworkObject.NetworkObjectId}, " +
				$"HP={networkCharacterState.HP}, " +
				$"MP={networkCharacterState.MP}",
				this
			);

			PlayerSpawned?.Invoke(clientId, playerNetworkObject, playerData);
		}

		private void HandlePlayerDataLoadFailed(ulong clientId, string error)
		{
			Debug.LogError(
				$"플레이어 데이터 로드 실패로 Spawn을 중단합니다. " +
				$"ClientId={clientId}, " +
				$"Error={error}",
				this
			);

			DisconnectAfterFailure(clientId, "플레이어 데이터를 불러오지 못했습니다.");
		}

		private GameObject ResolvePlayerPrefab()
		{
			if (networkPlayerPrefab != null)
				return networkPlayerPrefab;

			if (networkManager == null)
				return null;
			
			return networkManager.NetworkConfig.PlayerPrefab;
		}

		private void ResolveSpawnPose(
			ulong clientId,
			GameObject playerPrefab,
			out Vector3 spawnPosition,
			out Quaternion spawnRotation
		)
		{
			spawnPosition = playerPrefab.transform.position;
			spawnRotation = playerPrefab.transform.rotation;

			if (spawnPoints == null || spawnPoints.Length == 0)
				return;

			int index = (int)(clientId % (ulong) spawnPoints.Length);

			Transform spawnPoint = spawnPoints[index];

			if (spawnPoint == null)
				return;

			spawnPosition = spawnPoint.position;
			spawnRotation = spawnPoint.rotation;
		}

		private void FailPlayerSpawn(ulong clientId, string reason)
		{
			Debug.LogError(
				$"PlayerObject 생성 실패: " +
				$"ClientId={clientId}, " +
				$"Reason={reason}",
				this
			);

			DisconnectAfterFailure(clientId, reason);
		}

		private void DisconnectAfterFailure(ulong clientId, string reason)
		{
			if (networkManager == null || !networkManager.IsServer)
				return;

			if (!networkManager.ConnectedClients.ContainsKey(clientId))
				return;

			if (networkManager.IsHost && clientId == NetworkManager.ServerClientId)
			{
				Debug.LogError("Host 플레이어 생성에 실패하여 네트워크를 종료합니다.", this);

				networkManager.Shutdown();
				return;
			}

			networkManager.DisconnectClient(clientId, reason);
		}
	}
}