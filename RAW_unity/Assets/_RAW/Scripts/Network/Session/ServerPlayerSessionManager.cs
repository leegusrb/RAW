using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RAW.Persistence;
using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	public sealed class ServerPlayerSessionManager : MonoBehaviour
	{
		private sealed class PlayerSession
		{
			public ulong clientId;
			public string userId;
			public PlayerDataLoadState state;
			public PlayerPersistentData playerData;
			public CancellationTokenSource loadCancellation;

			public NetworkPlayerPersistenceAdapter persistenceAdapter;

			public bool hasCapturedFinalSnapshot;
			public string finalSnapshotCaptureError;
			public bool saveStarted;
		}

		[SerializeField]
		private NetworkBootstrap networkBootstrap;

		[SerializeField]
		private PlayerDataRepository playerDataRepository;

		private readonly Dictionary<ulong, PlayerSession> sessionsByClientId =
			new Dictionary<ulong, PlayerSession>();

		public event Action<ulong, PlayerPersistentData> PlayerDataLoaded;
		public event Action<ulong, string> PlayerDataLoadFailed;
		public event Action<ulong, PlayerPersistentData> PlayerDataSaved;
		public event Action<ulong, string> PlayerDataSaveFailed;

		private void Awake()
		{
			if (networkBootstrap == null)
				networkBootstrap = GetComponent<NetworkBootstrap>();

			if (playerDataRepository == null)
				playerDataRepository = GetComponent<PlayerDataRepository>();
		}

		private void OnEnable()
		{
			if (networkBootstrap == null)
				return;

			networkBootstrap.UserConnected += HandleUserConnected;
			networkBootstrap.UserDisconnected += HandleUserDisconnected;
		}

		private void OnDisable()
		{
			if (networkBootstrap != null)
			{
				networkBootstrap.UserConnected -= HandleUserConnected;
				networkBootstrap.UserDisconnected -= HandleUserDisconnected;
			}

			CancelAllSessions();
		}

		public bool TryGetReadyPlayerData(ulong clientId, out PlayerPersistentData playerData)
		{
			playerData = null;

			if (!sessionsByClientId.TryGetValue(clientId, out PlayerSession session))
				return false;

			if (session.state != PlayerDataLoadState.Ready || session.playerData == null)
				return false;

			playerData = session.playerData;
			return true;
		}

		public PlayerDataLoadState GetLoadState(ulong clientId)
		{
			if (!sessionsByClientId.TryGetValue(clientId, out PlayerSession session))
				return PlayerDataLoadState.None;

			return session.state;
		}

		public bool TryAttachPlayerPersistence(
			ulong clientId,
			NetworkPlayerPersistenceAdapter persistenceAdapter,
			out string error
		)
		{
			if (!sessionsByClientId.TryGetValue(
				clientId,
				out PlayerSession session
			))
			{
				error = "플레이어 세션이 없습니다.";
				return false;
			}

			if (session.state != PlayerDataLoadState.Ready || session.playerData == null)
			{
				error = "플레이어 데이터가 Ready 상태가 아닙니다.";
				return false;
			}

			if (persistenceAdapter == null ||
				!persistenceAdapter.IsSpawned ||
				!persistenceAdapter.IsServer)
			{
				error = "Spawn된 서버 PersistentAdapter가 아닙니다.";
				return false;
			}

			if (persistenceAdapter.OwnerClientId != clientId)
			{
				error = "PersistenceAdapter의 OwnerClientId가 일치하지 않습니다.";
				return false;
			}

			DetachPlayerPersistence(session);

			session.persistenceAdapter = persistenceAdapter;

			session.hasCapturedFinalSnapshot = false;
			session.finalSnapshotCaptureError = null;

			persistenceAdapter.PersistentStateCaptureBeforeDespawn += HandlePersistentStateCaptureBeforeDespawn;
			persistenceAdapter.PersistentStateCaptureFailedBeforeDespawn += HandlePersistentStateCaptureFailedBeforeDespawn;

			error = null;
			return true;
		}

		private void HandlePersistentStateCaptureBeforeDespawn(
			ulong clientId,
			PlayerPersistentData snapshot
		)
		{
			if (!sessionsByClientId.TryGetValue(
				clientId,
				out PlayerSession session
			))
			{
				return;
			}

			if (snapshot == null ||
				!string.Equals(
					snapshot.userId,
					session.userId,
					StringComparison.Ordinal
				))
			{
				session.hasCapturedFinalSnapshot = false;
				session.finalSnapshotCaptureError = "캡쳐된 데이터의 UserId가 일치하지 않습니다.";

				return;
			}

			session.playerData = snapshot.DeepCopy();
			session.hasCapturedFinalSnapshot = true;
			session.finalSnapshotCaptureError = null;

			DetachPlayerPersistence(session);

			Debug.Log(
				$"연결 종료 전 플레이어 상태 캡쳐 완료: " +
				$"ClientId={clientId}, " +
				$"UserId={session.userId}",
				this
			);
		}

		private void HandlePersistentStateCaptureFailedBeforeDespawn(
			ulong clientId,
			string error
		)
		{
			if (!sessionsByClientId.TryGetValue(
				clientId,
				out PlayerSession session
			))
			{
				return;
			}

			session.hasCapturedFinalSnapshot = false;
			session.finalSnapshotCaptureError = error;

			DetachPlayerPersistence(session);

			Debug.LogError(
				$"연결 종료 전 플레이어 상태 캡쳐 실패: " +
				$"ClientId={clientId}, " +
				$"UserId={session.userId}, " +
				$"Error={error}",
				this
			);
		}

		private void DetachPlayerPersistence(PlayerSession session)
		{
			if (session == null || session.persistenceAdapter == null)
				return;

			session.persistenceAdapter.PersistentStateCaptureBeforeDespawn -= HandlePersistentStateCaptureBeforeDespawn;
			session.persistenceAdapter.PersistentStateCaptureFailedBeforeDespawn -= HandlePersistentStateCaptureFailedBeforeDespawn;

			session.persistenceAdapter = null;
		}

		private void HandleUserConnected(ulong clientId, string userId)
		{
			if (playerDataRepository == null)
			{
				Debug.LogError("PlayerDataRepository가 연결되지 않았습니다.", this);
				
				PlayerDataLoadFailed?.Invoke(clientId, "플레이어 데이터 저장소가 없습니다.");

				return;
			}

			RemoveSession(clientId);

			PlayerSession session =
				new PlayerSession
				{
					clientId = clientId,
					userId = userId,
					state = PlayerDataLoadState.Loading,
					loadCancellation = new CancellationTokenSource()
				};

			sessionsByClientId[clientId] = session;

			Debug.Log(
				$"플레이어 데이터 로드 시작: " +
				$"ClientId={clientId}, " +
				$"UserId={userId}",
				this
			);

			_ = LoadPlayerDataAsync(session);
		}

		private async Task LoadPlayerDataAsync(PlayerSession session)
		{
			try
			{
				PlayerPersistentData playerData = 
					await playerDataRepository.LoadAsync(
						session.userId,
						session.loadCancellation.Token
					);

				if (!sessionsByClientId.TryGetValue(session.clientId, out PlayerSession currentSession) ||
					!ReferenceEquals(currentSession, session))
				{
					return;
				}

				if (playerData == null)
					throw new InvalidOperationException("로드된 플레이어 데이터가 null입니다.");

				if (!string.Equals(
					playerData.userId,
					session.userId,
					StringComparison.Ordinal
				))
				{
					throw new InvalidOperationException("로드된 데이터의 UserId가 일치하지 않습니다.");
				}

				session.playerData = playerData;
				session.state = PlayerDataLoadState.Ready;

				Debug.Log(
					$"플레이어 데이터 로드 완료: " +
					$"ClientId={session.clientId}, " +
					$"UserId={session.userId}",
					this
				);

				PlayerDataLoaded?.Invoke(session.clientId, playerData);
			}
			catch (OperationCanceledException)
			{
				Debug.Log(
					$"플레이어 데이터 로드 취소: " +
					$"ClientId={session.clientId}, " +
					$"UserId={session.userId}",
					this
				);
			}
			catch (Exception exception)
			{
				session.state = PlayerDataLoadState.Failed;

				Debug.LogError(
					$"플레이어 데이터 로드 실패: " +
					$"ClientId={session.clientId}, " +
					$"UserId={session.userId}, " +
					$"Error={exception.Message}",
					this
				);

				PlayerDataLoadFailed?.Invoke(session.clientId, exception.Message);
			}
		}

		private void HandleUserDisconnected(ulong clientId, string userId)
		{
			if (!sessionsByClientId.TryGetValue(
				clientId,
				out PlayerSession session
			))
			{
				return;
			}

			if (!string.Equals(
				session.userId,
				userId,
				StringComparison.Ordinal
			))
			{
				PlayerDataSaveFailed?.Invoke(
					clientId,
					"연결 종료 사용자의 UserId가 일치하지 않습니다."
				);

				RemoveSession(clientId, session);
				return;
			}

			if (session.state != PlayerDataLoadState.Ready || session.playerData == null)
			{
				RemoveSession(clientId, session);
				return;
			}

			if (!session.hasCapturedFinalSnapshot)
			{
				string error =
					string.IsNullOrWhiteSpace(session.finalSnapshotCaptureError)
						? "연결 종료 전 최종 상태를 캡쳐하지 못했습니다."
						: session.finalSnapshotCaptureError;

				PlayerDataSaveFailed?.Invoke(clientId, error);

				Debug.LogError(
					$"플레이어 연결 종료 저장 중단: " +
					$"ClientId={clientId}, " +
					$"UserId={userId}, " +
					$"Error={error}",
					this
				);

				RemoveSession(clientId, session);
				return;
			}

			_ = SaveAndRemoveSessionAsync(session);
		}

		private async Task SaveAndRemoveSessionAsync(PlayerSession session)
		{
			if (session == null || session.saveStarted)
				return;

			session.saveStarted = true;

			PlayerPersistentData snapshot = session.playerData.DeepCopy();

			try
			{
				if (!PlayerPersistentDataValidator.TryValidate(
					snapshot,
					out string validationError
				))
				{
					throw new InvalidOperationException($"저장 데이터 검증 실패: {validationError}");
				}

				await playerDataRepository.SaveAsync(snapshot, CancellationToken.None);

				Debug.Log(
					$"플레이어 데이터 저장 완료: " +
					$"ClientId={session.clientId}, " +
					$"UserId={session.userId}",
					this
				);

				PlayerDataSaved?.Invoke(
					session.clientId,
					snapshot.DeepCopy()
				);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					$"플레이어 데이터 저장 실패: " +
					$"ClientId={session.clientId}, " +
					$"UserId={session.userId}, " +
					$"Error={exception.Message}",
					this
				);

				PlayerDataSaveFailed?.Invoke(
					session.clientId,
					exception.Message
				);
			}
			finally
			{
				RemoveSession(session.clientId, session);
			}
		}

		private void RemoveSession(
			ulong clientId,
			PlayerSession expectedSession = null
		)
		{
			if (!sessionsByClientId.TryGetValue(clientId, out PlayerSession session))
				return;

			if (expectedSession != null && !ReferenceEquals(session, expectedSession))
				return;

			sessionsByClientId.Remove(clientId);

			DetachPlayerPersistence(session);

			if (session.loadCancellation != null)
			{
				session.loadCancellation.Cancel();
				session.loadCancellation.Dispose();
				session.loadCancellation = null;
			}
		}

		private void CancelAllSessions()
		{
			List<ulong> clientIds = new List<ulong>(sessionsByClientId.Keys);

			for (int i = 0; i < clientIds.Count; i++)
				RemoveSession(clientIds[i]);
		}
	}
}
