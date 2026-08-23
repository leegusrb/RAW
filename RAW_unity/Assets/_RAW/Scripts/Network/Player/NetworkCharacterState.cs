using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(Char_State))]
	public class NetworkCharacterState : NetworkBehaviour
	{
		[SerializeField] private Char_State characterState;

		private readonly NetworkVariable<int> healthPoint = 
			new NetworkVariable<int>(
				100,
				NetworkVariableReadPermission.Everyone,
				NetworkVariableWritePermission.Server
			);

		private readonly NetworkVariable<int> manaPoint = 
			new NetworkVariable<int>(
				100,
				NetworkVariableReadPermission.Everyone,
				NetworkVariableWritePermission.Server
			);

		private readonly NetworkVariable<bool> isMovable = 
			new NetworkVariable<bool>(
				true,
				NetworkVariableReadPermission.Everyone,
				NetworkVariableWritePermission.Server
			);

		public int HP => healthPoint.Value;
		public int MP => manaPoint.Value;
		public bool IsMovable => isMovable.Value;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();
		}

		private void CacheComponents()
		{
			if (characterState == null)
				characterState = GetComponent<Char_State>();
		}

		public override void OnNetworkSpawn()
		{
			healthPoint.OnValueChanged += HandleHealthChanged;
			manaPoint.OnValueChanged += HandleManaChanged;
			isMovable.OnValueChanged += HandleMovableChanged;

			ApplyCurrentState();
		}

		public override void OnNetworkDespawn()
		{
			healthPoint.OnValueChanged -= HandleHealthChanged;
			manaPoint.OnValueChanged -= HandleManaChanged;
			isMovable.OnValueChanged -= HandleMovableChanged;
		}

		private void ApplyCurrentState()
		{
			if (characterState == null)
				return;
			
			characterState.HP = healthPoint.Value;
			characterState.MP = manaPoint.Value;
			characterState.isMovable = isMovable.Value;
		}

		private void HandleHealthChanged(int previousValue, int newValue)
		{
			if (characterState != null)
				characterState.HP = newValue;
		}

		private void HandleManaChanged(int previousValue, int newValue)
		{
			if (characterState != null)
				characterState.MP = newValue;
		}

		private void HandleMovableChanged(bool previousValue, bool newValue)
		{
			if (characterState != null)
				characterState.isMovable = newValue;
		}

		public void ApplyDamage(int amount)
		{
			if (!IsServer || amount <= 0)
				return;

			healthPoint.Value = Mathf.Max(0, healthPoint.Value - amount);

			if (healthPoint.Value == 0)
				isMovable.Value = false;
		}

		public void Heal(int amount)
		{
			if (!IsServer || amount <= 0)
				return;

			healthPoint.Value += amount;
		}

		public bool TryConsumeMana(int amount)
		{
			if (!IsServer || amount <= 0)
				return false;

			if (manaPoint.Value < amount)
				return false;

			manaPoint.Value -= amount;
			return true;
		}

		public void RestoreMana(int amount)
		{
			if (!IsServer || amount <= 0)
				return;

			manaPoint.Value += amount;
		}

		public void SetMovable(bool movable)
		{
			if (!IsServer)
				return;

			isMovable.Value = movable;
		}

		public bool InitializePersistentStateOnServer(int loadedHealthPoint, int loadedManaPoint)
		{
			if (!IsSpawned || !IsServer)
			{
				Debug.LogWarning("Spawn된 서버 NetworkPlayer에서만 초기 상태를 적용할 수 있습니다.", this);
				return false;
			}

			if (characterState == null)
			{
				Debug.LogError("초기 상태를 적용할 Char_State가 연결되지 않았습니다.", this);
				return false;
			}

			if (loadedHealthPoint < 0 || loadedManaPoint < 0)
			{
				Debug.LogError(
					$"영속 상태 값이 올바르지 않습니다. " +
					$"HP={loadedHealthPoint}, MP={loadedManaPoint}",
					this
				);

				return false;
			}

			healthPoint.Value = loadedHealthPoint;
			manaPoint.Value = loadedManaPoint;
			isMovable.Value = healthPoint.Value > 0;

			ApplyCurrentState();

			Debug.Log(
				$"플레이어 영구 상태 적용 완료: " +
				$"OwnerClientId={OwnerClientId}, " +
				$"HP={healthPoint.Value}, " +
				$"MP={manaPoint.Value}",
				this
			);

			return true;
		}

		#if UNITY_EDITOR

		[ContextMenu("Test - Apply 10 Damage")]
		private void TestApplyDamage()
		{
			if (!CanRunContextTest())
				return;

			ApplyDamage(10);

			Debug.Log($"테스트 데미지 적용: HP={healthPoint.Value}", this);
		}

		[ContextMenu("Test - Heal 10")]
		private void TestHeal()
		{
			if (!CanRunContextTest())
				return;

			Heal(10);

			Debug.Log($"테스트 회복 적용: HP={healthPoint.Value}", this);
		}

		[ContextMenu("Test - Consume 10 Mana")]
		private void TestConsumeMana()
		{
			if (!CanRunContextTest())
				return;

			bool succeeded = TryConsumeMana(10);

			Debug.Log(
				$"테스트 마나 소모: 성공={succeeded}, MP={manaPoint.Value}",
				this
			);
		}

		[ContextMenu("Test - Restore 10 Mana")]
		private void TestRestoreMana()
		{
			if (!CanRunContextTest())
				return;

			RestoreMana(10);

			Debug.Log($"테스트 마나 회복: MP={manaPoint.Value}", this);
		}

		[ContextMenu("Test - Toggle Movable")]
		private void TestToggleMovable()
		{
			if (!CanRunContextTest())
				return;

			SetMovable(!isMovable.Value);

			Debug.Log($"이동 가능 상태 변경: {isMovable.Value}", this);
		}

		private bool CanRunContextTest()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Play Mode에서만 테스트할 수 있습니다.", this);
				return false;
			}

			if (!IsSpawned)
			{
				Debug.LogWarning("NetworkObject가 Spawn되지 않았습니다.", this);
				return false;
			}

			if (!IsServer)
			{
				Debug.LogWarning("서버 또는 Host에서만 실행할 수 있습니다.", this);
				return false;
			}

			return true;
		}

		#endif
	}
}
