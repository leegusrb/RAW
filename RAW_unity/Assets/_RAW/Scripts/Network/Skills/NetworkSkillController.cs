using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(NetworkCharacterState))]
	public class NetworkSkillController : NetworkBehaviour
	{
		[SerializeField] private NetworkCharacterState characterState;
		[SerializeField] private SkillCatalog skillCatalog;

		[SerializeField]
		private List<DefaultSkillLoadoutEntry> defaultSkillLoadout =
			new List<DefaultSkillLoadoutEntry>();

		private NetworkList<NetworkSkillCooldownEntry> cooldownList;
		private NetworkList<NetworkSkillLoadoutEntry> skillLoadout;

		public event Action CooldownChanged;
		public event Action LoadoutChanged;

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

			skillLoadout =
				new NetworkList<NetworkSkillLoadoutEntry>(
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
			skillLoadout.OnListChanged += HandleSkillLoadoutChanged;

			if (IsServer)
				InitializeDefaultLoadoutOnServer();
		}

		public override void OnNetworkDespawn()
		{
			cooldownList.OnListChanged -= HandleCooldownListChanged;
			skillLoadout.OnListChanged -= HandleSkillLoadoutChanged;
		}

		private void CacheComponents()
		{
			if (characterState == null)
				characterState = GetComponent<NetworkCharacterState>();
		}

		private void InitializeDefaultLoadoutOnServer()
		{
			if (!IsServer)
				return;

			skillLoadout.Clear();

			HashSet<KeyMapping> assignedSlots = new HashSet<KeyMapping>();

			for (int i = 0; i < defaultSkillLoadout.Count; i++)
			{
				DefaultSkillLoadoutEntry entry = defaultSkillLoadout[i];

				if (entry == null || entry.Skill == null)
				{
					Debug.LogError($"기본 스킬 슬롯의 {i}번 항목이 비어 있습니다.", this);
					continue;
				}

				if (!Enum.IsDefined(typeof(KeyMapping), entry.Slot))
				{
					Debug.LogError($"유효하지 않은 스킬 슬롯입니다: {entry.Slot}", this);
					continue;
				}

				if (!assignedSlots.Add(entry.Slot))
				{
					Debug.LogError($"중복된 기본 스킬 슬롯입니다: {entry.Slot}", this);
					continue;
				}

				if (!skillCatalog.TryGetSkill(entry.Skill.SkillId, out SkillSpec registeredSkill))
				{
					Debug.LogError($"SkillCatalog에 등록되지 않은 기본 스킬입니다. {entry.Skill.name}", this);
					continue;
				}

				skillLoadout.Add(new NetworkSkillLoadoutEntry(entry.Slot, registeredSkill.SkillId));
			}
		}

		public bool TryGetSkillForSlot(KeyMapping slot, out SkillSpec skill)
		{
			if (!TryGetSkillIdForSlot(slot, out string skillId))
			{
				skill = null;
				return false;
			}

			return skillCatalog.TryGetSkill(skillId, out skill);
		}

		private bool TryGetSkillIdForSlot(KeyMapping slot, out string skillId)
		{
			for (int i = 0; i < skillLoadout.Count; i++)
			{
				NetworkSkillLoadoutEntry entry = skillLoadout[i];

				if (entry.Slot != slot)
					continue;

				skillId = entry.SkillId.ToString();
				return true;
			}

			skillId = null;
			return false;
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

		public void RequestUseSkill(KeyMapping skillSlot)
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

			if (!Enum.IsDefined(typeof(KeyMapping), skillSlot))
			{
				Debug.LogWarning($"유효하지 않은 스킬 슬롯입니다: {skillSlot}", this);
				return;
			}

			if (IsServer)
			{
				TryStartSkillOnServer(skillSlot);
			}
			else
			{
				RequestUseSkillRpc(skillSlot);
			}
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestUseSkillRpc(KeyMapping skillSlot)
		{
			TryStartSkillOnServer(skillSlot);
		}

		private bool TryStartSkillOnServer(KeyMapping skillSlot)
		{
			if (!IsServer)
				return false;

			if (!Enum.IsDefined(typeof(KeyMapping), skillSlot))
			{
				Debug.LogWarning($"스킬 요청 거절: 유효하지 않은 슬롯입니다. OwnerClientId={OwnerClientId}, Slot={skillSlot}", this);
				return false;
			}

			if (!TryGetSkillIdForSlot(skillSlot, out string skillId))
			{
				Debug.LogWarning($"스킬 요청 거절: 비어 있는 스킬 슬롯입니다. OwnerClientId={OwnerClientId}, Slot={skillSlot}", this);
				return false;
			}

			if (!skillCatalog.TryGetSkill(skillId, out SkillSpec skill))
			{
				Debug.LogWarning($"스킬 요청 거절: 등록되지 않은 스킬입니다. OwnerClientId={OwnerClientId}, Slot={skillSlot}, SkillId={skillId}", this);
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

			Debug.Log($"스킬 요청 승인: OwnerClientId={OwnerClientId}, Slot={skillSlot}, SkillId={skillId}, Mana={characterState.MP}, Cooldown={skill.CooldownSeconds:F2}", this);

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
				CooldownChanged?.Invoke();
		}

		private void HandleSkillLoadoutChanged(NetworkListEvent<NetworkSkillLoadoutEntry> changeEvent)
		{
			if (IsOwner)
				LoadoutChanged?.Invoke();
		}

#if UNITY_EDITOR

		[ContextMenu("Test - Request W Skill")]
		private void TestRequestWSkill()
		{
			if (!CanRunOwnerTest())
				return;

			RequestUseSkill(KeyMapping.W);
		}

		[ContextMenu("Test - Request Invalid Slot")]
		private void TestRequestInvalidSlot()
		{
			if (!CanRunOwnerTest())
				return;

			RequestUseSkill((KeyMapping)999);
		}

		[ContextMenu("Test - Print W Skill Cooldown")]
		private void TestPrintWSkillCooldown()
		{
			if (!CanRunOwnerTest())
				return;

			if (!TryGetSkillForSlot(KeyMapping.W, out SkillSpec skill))
			{
				Debug.LogWarning("W 슬롯에 등록된 스킬이 없습니다.", this);
				return;
			}

			Debug.Log(
				$"W 스킬 남은 쿨다운: " +
				$"{GetRemainingCooldown(skill.SkillId):F2}",
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

