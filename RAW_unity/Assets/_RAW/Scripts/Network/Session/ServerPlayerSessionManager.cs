using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RAW.Network
{
	public enum PlayerDataLoadState
	{
		None,
		Loading,
		Ready,
		Failed
	}

	[DisallowMultipleComponent]
	public sealed class ServerPlayerSessionManager : MonoBehaviour
	{
		private sealed class LoadingSession
		{
			public ulong clientId;
			public string userId;
			public PlayerDataLoadState state;
			public PlayerPersistentData playerData;
			public CancellationTokenSource cancellation;
		}

		[SerializeField]
		private NetworkBootstrap networkBootstrap;

		[SerializeField]
		private PlayerDataRepository playerDataRepository;

		private readonly Dictionary<ulong, LoadingSession> sessionsByClientId =
			new Dictionary<ulong, LoadingSession>();

		public event Action<ulong, PlayerPersistentData> PlayerDataLoaded;
		public event Action<ulong, string> PlayerDataLoadFailed;

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

			if (!sessionsByClientId.TryGetValue(clientId, out LoadingSession session))
				return false;

			if (session.state != PlayerDataLoadState.Ready || session.playerData == null)
				return false;

			playerData = session.playerData;
			return true;
		}

		public PlayerDataLoadState GetLoadState(ulong clientId)
		{
			if (!sessionsByClientId.TryGetValue(clientId, out LoadingSession session))
				return PlayerDataLoadState.None;

			return session.state;
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

			LoadingSession session =
				new LoadingSession
				{
					clientId = clientId,
					userId = userId,
					state = PlayerDataLoadState.Loading,
					cancellation = new CancellationTokenSource()
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

		private async Task LoadPlayerDataAsync(LoadingSession session)
		{
			try
			{
				PlayerPersistentData playerData = 
					await playerDataRepository.LoadAsync(
						session.userId,
						session.cancellation.Token
					);

				if (!sessionsByClientId.TryGetValue(session.clientId, out LoadingSession currentSession) ||
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
			RemoveSession(clientId);
		}

		private void RemoveSession(ulong clientId)
		{
			if (!sessionsByClientId.TryGetValue(clientId, out LoadingSession session))
				return;

			sessionsByClientId.Remove(clientId);

			if (session.cancellation != null)
			{
				session.cancellation.Cancel();
				session.cancellation.Dispose();
				session.cancellation = null;
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
