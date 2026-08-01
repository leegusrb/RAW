using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillCooldownEntry :
		INetworkSerializable,
		IEquatable<NetworkSkillCooldownEntry>
	{
		public FixedString64Bytes SkillId;
		public double CooldownEndTime;

		public NetworkSkillCooldownEntry(string skillId, double cooldownEndTime)
		{
			SkillId = new FixedString64Bytes(skillId);
			CooldownEndTime = cooldownEndTime;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref CooldownEndTime);
		}

		public bool Equals(NetworkSkillCooldownEntry other)
		{
			return SkillId.Equals(other.SkillId) &&
				CooldownEndTime.Equals(other.CooldownEndTime);
		}
	}

	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(NetworkCharacterState))]
	public class NetworkSkillController : NetworkBehaviour
	{
		[SerializeField] private NetworkCharacterState characterState;
		[SerializeField] private SkillCatalog skillCatalog;

		private NetworkList<NetworkSkillCooldownEntry> cooldownList;

		public event Action OnCooldownChanged;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();

			cooldownList = 
				new NetworkList<NetworkSkillCooldownEntry>(
					null,
					NetworkVariableReadPermission.Owner,
					NetworkVariableWritePermission.Server
				);
		}

		public override void OnNetworkSpawn()
		{
			if (characterState == null)
			{
				Debug.LogError("NetworkCharacterState가 연결되지 않았습니다.", this);

				enabled = false;
				return;
			}

			if (skillCatalog == null)
			{
				Debug.LogError("SkillCatalog가 연결되지 않았습니다.", this);

				enabled = false;
				return;
			}

			cooldownList.OnListChanged += HandleCooldownListChanged;
		}

		public override void OnNetworkDespawn()
		{
			cooldownList.OnListChanged -= HandleCooldownListChanged;
		}

		private void CacheComponents()
		{
			if (characterState == null)
				characterState = GetComponent<NetworkCharacterState>();
		}

		public bool TryGetSkill(string skillId, out SkillSpec skill)
		{
			if (skillCatalog == null)
			{
				skill = null;
				return false;
			}

			return skillCatalog.TryGetSkill(skillId, out skill);
		}

		public void RequestUseSkill(string skillId)
		{
			if (!IsSpawned)
			{
				Debug.LogWarning("NetworkPlayer가 아직 Spawn되지 않았습니다.", this);
				return;
			}

			if (!IsOwner)
			{
				Debug.LogWarning("자신이 소유한 캐릭터만 스킬을 요청할 수 있습니다.", this);
				return;
			}

			if (string.IsNullOrEmpty(skillId))
			{
				Debug.LogWarning("Skill ID가 비어 있습니다.", this);
				return;
			}

			if (skillId.Length > 60)
			{
				Debug.LogWarning($"Skill ID가 너무 깁니다: {skillId.Length}", this);
				return;
			}

			if (IsServer)
			{
				TryStartSkillOnServer(skillId);
			}
			else
			{
				RequestUseSkillRpc(new FixedString64Bytes(skillId));
			}
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestUseSkillRpc(FixedString64Bytes skillId)
		{
			TryStartSkillOnServer(skillId.ToString());
		}

		private bool TryStartSkillOnServer(string skillId)
		{
			if (!IsServer)
				return false;

			if (!skillCatalog.TryGetSkill(skillId, out SkillSpec skill))
			{
				Debug.LogWarning($"스킬 요청 거절: 등록되지 않은 스킬입니다. OwnerClientId={OwnerClientId}, SkillId={skillId}", this);
				return false;
			}

			if (characterState.HP <= 0 || !characterState.IsMovable)
			{
				Debug.LogWarning($"스킬 요청 거절: 현재 스킬을 사용할 수 없는 상태입니다. OwnerClientId={OwnerClientId}, SkillId={skillId}", this);
				return false;
			}

			double serverTime = NetworkManager.ServerTime.Time;

			double remainingCooldown = GetRemainingCooldownAt(skillId, serverTime);

			if (remainingCooldown > 0d)
			{
				Debug.LogWarning($"스킬 요청 거절: 쿨다운 중입니다. OwnerClientId={OwnerClientId}, SkillId={skillId}, Remaining={remainingCooldown:F2}", this);
				return false;
			}

			if (skill.ManaCost > 0 && !characterState.TryConsumeMana(skill.ManaCost))
			{
				Debug.LogWarning($"스킬 요청 거절: 마나가 부족합니다. OwnerClientId={OwnerClientId}, SkillId={skillId}, Required={skill.ManaCost}, Current={characterState.MP}", this);
				return false;
			}

			SetCooldownOnServer(skillId, skill.CooldownSeconds, serverTime);

			Debug.Log($"스킬 요청 승인: OwnerClientId={OwnerClientId}, SkillId={skillId}, Mana={characterState.MP}, Cooldown={skill.CooldownSeconds:F2}", this);

			return true;
		}

		public double GetRemainingCooldown(string skillId)
		{
			if (!IsSpawned || (!IsOwner && !IsServer) || NetworkManager == null)
				return 0d;

			return GetRemainingCooldownAt(skillId, NetworkManager.ServerTime.Time);
		}

		public double GetRemainingCooldownAt(string skillId, double currentTime)
		{
			int index = FindCooldownIndex(skillId);

			if (index < 0)
				return 0d;

			double remaining = cooldownList[index].CooldownEndTime - currentTime;

			return Math.Max(0d, remaining);
		}

		private int FindCooldownIndex(string skillId)
		{
			for (int i = 0; i < cooldownList.Count; i++)
			{
				if (string.Equals(cooldownList[i].SkillId.ToString(), skillId, StringComparison.Ordinal))
					return i;
			}
			
			return -1;
		}

		private void SetCooldownOnServer(string skillId, float cooldownSeconds, double currentTime)
		{
			if (!IsServer)
				return;

			double cooldownEndTime = currentTime + Math.Max(0f, cooldownSeconds);

			NetworkSkillCooldownEntry entry = new NetworkSkillCooldownEntry(skillId, cooldownEndTime);

			int index = FindCooldownIndex(skillId);

			if (index >= 0)
				cooldownList[index] = entry;
			else
				cooldownList.Add(entry);
		}	

		private void HandleCooldownListChanged(NetworkListEvent<NetworkSkillCooldownEntry> changeEvent)
		{
			if (IsOwner)
				OnCooldownChanged?.Invoke();
		}

#if UNITY_EDITOR

		[ContextMenu("Test - Request Arrow Charge")]
		private void TestRequestArrowCharge()
		{
			if (!CanRunOwnerTest())
				return;

			RequestUseSkill("arrow_charge");
		}

		[ContextMenu("Test - Request Invalid Skill")]
		private void TestRequestInvalidSkill()
		{
			if (!CanRunOwnerTest())
				return;

			RequestUseSkill("invalid_skill");
		}

		[ContextMenu("Test - Print Arrow Charge Cooldown")]
		private void TestPrintArrowChargeCooldown()
		{
			if (!CanRunOwnerTest())
				return;

			Debug.Log(
				$"Arrow Charge 남은 쿨다운: " +
				$"{GetRemainingCooldown("arrow_charge"):F2}",
				this
			);
		}

		private bool CanRunOwnerTest()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning(
					"Play Mode에서만 테스트할 수 있습니다.",
					this
				);

				return false;
			}

			if (!IsSpawned)
			{
				Debug.LogWarning(
					"NetworkObject가 Spawn되지 않았습니다.",
					this
				);

				return false;
			}

			if (!IsOwner)
			{
				Debug.LogWarning(
					"소유 중인 NetworkPlayer에서 실행해야 합니다.",
					this
				);

				return false;
			}

			return characterState != null &&
				   skillCatalog != null;
		}

#endif
	}
}

