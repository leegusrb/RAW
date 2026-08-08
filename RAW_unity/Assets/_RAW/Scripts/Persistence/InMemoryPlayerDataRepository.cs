using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RAW.Persistence
{
	public sealed class InMemoryPlayerDataRepository : PlayerDataRepository
	{
		private readonly Dictionary<string, PlayerPersistentData> playerDataByUserId =
			new Dictionary<string, PlayerPersistentData>(StringComparer.Ordinal);

		public override Task<PlayerPersistentData> LoadAsync(
			string userId,
			CancellationToken cancellationToken
		)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (string.IsNullOrWhiteSpace(userId))
				throw new ArgumentException("UserId가 비어있습니다.", nameof(userId));

			if (!playerDataByUserId.TryGetValue(userId, out PlayerPersistentData storedData))
			{
				storedData = PlayerPersistentData.CreateDefault(userId);

				playerDataByUserId[userId] = storedData.DeepCopy();

				Debug.Log($"신규 플레이어 기본 데이터 생성: UserId={userId}", this);
			}

			return Task.FromResult(storedData.DeepCopy());
		}

		public override Task SaveAsync(
			PlayerPersistentData playerData,
			CancellationToken cancellationToken
		)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (playerData == null)
				throw new ArgumentException(nameof(playerData));

			if (string.IsNullOrWhiteSpace(playerData.userId))
				throw new ArgumentException("저장할 데이터의 UserId가 비어 있습니다.", nameof(playerData));

			playerDataByUserId[playerData.userId] = playerData.DeepCopy();

			Debug.Log($"플레이어 데이터 메모리 저장: UserId={playerData.userId}", this);

			return Task.CompletedTask;
		}
	}
}