using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RAW.Persistence
{
	public abstract class PlayerDataRepository : MonoBehaviour
	{
		public abstract Task<PlayerPersistentData> LoadAsync(
			string userId,
			CancellationToken cancellationToken
		);

		public abstract Task SaveAsync(
			PlayerPersistentData playerData,
			CancellationToken cancellationToken
		);
	}
}