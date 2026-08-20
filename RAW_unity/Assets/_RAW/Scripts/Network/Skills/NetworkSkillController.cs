using System;
using System.Collections.Generic;
using Unity.Collections;
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

		private uint lastIssuedRequestSequence;
		private uint lastReceivedRequestSequence;
		private bool hasLastReceivedRequestSequence;

		public event Action CooldownChanged;
		public event Action LoadoutChanged;

		public event Action<SkillUseRejectedEvent> SkillUseRejected;
		public event Action<SkillCastEvent> SkillCast;
		public event Action<SkillHitEvent> SkillHit;

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

			if (IsOwner)
				lastIssuedRequestSequence = 0;

			if (IsServer)
			{
				lastReceivedRequestSequence = 0;
				hasLastReceivedRequestSequence = false;

				InitializeDefaultLoadoutOnServer();
			}
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

		public bool TryGetSkill(string skillId, out SkillSpec skill)
		{
			if (skillCatalog == null)
			{
				skill = null;
				return false;
			}

			return skillCatalog.TryGetSkill(skillId, out skill);
		}

		public double GetRemainingCooldown(string skillId)
		{
			if (!IsSpawned || (!IsOwner && !IsServer) || NetworkManager == null)
				return 0d;

			return GetRemainingCooldownAt(skillId, NetworkManager.ServerTime.Time);
		}

		public bool TryRequestUseSkill(KeyMapping skillSlot)
		{
			if (!Enum.IsDefined(typeof(KeyMapping), skillSlot))
			{
				Debug.LogWarning($"유효하지 않은 스킬 슬롯입니다: {skillSlot}", this);
				return false;
			}

			if (!TryGetSkillForSlot(skillSlot, out SkillSpec skill))
			{
				Debug.LogWarning($"스킬이 장착되지 않은 슬롯입니다: {skillSlot}");
				return false;
			}

			SkillUseRequest request =
				new SkillUseRequest
				{
					skillId = skill.SkillId,
					requestSequence = 0,
					target = null
				};

			return TryRequestUseSkill(request);
		}

		public bool TryRequestUseSkill(SkillUseRequest request)
		{
			if (!IsSpawned)
			{
				Debug.LogWarning("NetworkPlayer가 아직 Spawn되지 않았습니다.", this);
				return false;
			}

			if (!IsOwner)
			{
				Debug.LogWarning("자신이 소유한 캐릭터만 스킬을 요청할 수 있습니다.", this);
				return false;
			}

			if (!NetworkSkillContractMapper.TryToNetwork(
				request,
				out NetworkSkillUseRequest networkRequest
			))
			{
				Debug.LogWarning("스킬 요청을 네트워크 데이터로 변환할 수 없습니다.", this);
				return false;
			}

			networkRequest.RequestSequence = IssueRequestSequence();

			if (IsServer)
			{
				TryProcessSkillUseRequestOnServer(networkRequest);
			}
			else
			{
				RequestUseSkillRpc(networkRequest);
			}

			return true;
		}

		private uint IssueRequestSequence()
		{
			do
			{
				lastIssuedRequestSequence++;
			}
			while (lastIssuedRequestSequence == 0);

			return lastIssuedRequestSequence;
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestUseSkillRpc(NetworkSkillUseRequest request)
		{
			TryProcessSkillUseRequestOnServer(request);
		}

		private bool TryProcessSkillUseRequestOnServer(NetworkSkillUseRequest request)
		{
			if (!IsServer)
				return false;

			if (!TryRegisterRequestSequence(request.RequestSequence))
			{
				Debug.LogWarning(
					$"스킬 요청 무시: 중복되었거나 이전 요청입니다. " +
					$"OwnerClientId={OwnerClientId}, " +
					$"Sequence={request.RequestSequence}, " +
					$"LastSequence={lastReceivedRequestSequence}",
					this
				);

				return false;
			}

			double serverTime = NetworkManager.ServerTime.Time;

			if (!TryValidateSkillUseOnServer(
					request,
					serverTime,
					out string skillId,
					out SkillSpec skill,
					out SkillUseRejectionReason rejectionReason
				))
			{
				Debug.LogWarning(
					$"스킬 요청 거절: " +
					$"OwnerClientId={OwnerClientId}, " +
					$"SkillId={skillId}, " +
					$"Sequence={request.RequestSequence}, " +
					$"Reason={rejectionReason}",
					this
				);

				SendSkillUseRejectedEvent(request, rejectionReason);

				return false;
			}

			if (skill.ManaCost > 0 && !characterState.TryConsumeMana(skill.ManaCost))
			{
				Debug.LogWarning(
					$"스킬 요청 상태 반영 실패: " +
					$"OwnerClientId={OwnerClientId}, " +
					$"SkillId={skillId}, " +
					$"Sequence={request.RequestSequence}",
					this
				);

				SendSkillUseRejectedEvent(
					request,
					SkillUseRejectionReason.NotEnoughResource
				);

				return false;
			}

			SetCooldownOnServer(skillId, skill.CooldownSeconds, serverTime);

			SendSkillCastEvent(request, serverTime);

			Debug.Log(
				$"스킬 요청 승인: " +
				$"OwnerClientId={OwnerClientId}, " +
				$"SkillId={skillId}, " +
				$"Mana={characterState.MP}, " +
				$"Cooldown={skill.CooldownSeconds:F2}, " +
				$"Sequence={request.RequestSequence}",
				this
			);

			return true;
		}

		private bool TryRegisterRequestSequence(uint requestSequence)
		{
			if (requestSequence == 0)
				return false;

			if (hasLastReceivedRequestSequence &&
				!IsNewerRequestSequence(
					requestSequence,
					lastReceivedRequestSequence
				))
			{
				return false;
			}

			lastReceivedRequestSequence = requestSequence;
			hasLastReceivedRequestSequence = true;

			return true;
		}

		private static bool IsNewerRequestSequence(uint candidate, uint previous)
		{
			return unchecked((int)(candidate - previous)) > 0;
		}

		private bool TryValidateSkillUseOnServer(
			NetworkSkillUseRequest request,
			double serverTime,
			out string skillId,
			out SkillSpec skill,
			out SkillUseRejectionReason rejectionReason
		)
		{
			skillId = request.SkillId.ToString();
			skill = null;
			rejectionReason = SkillUseRejectionReason.Unknown;

			if (string.IsNullOrWhiteSpace(skillId))
			{
				rejectionReason = SkillUseRejectionReason.SkillNotFound;
				return false;
			}

			if (skillCatalog == null || !skillCatalog.TryGetSkill(skillId, out skill))
			{
				rejectionReason = SkillUseRejectionReason.SkillNotFound;
				return false;
			}

			if (!IsSkillEquipped(request.SkillId))
			{
				rejectionReason = SkillUseRejectionReason.SkillNotEquipped;
				return false;
			}

			if (characterState == null || characterState.HP <= 0 || !characterState.IsMovable)
			{
				rejectionReason = SkillUseRejectionReason.InvalidState;
				return false;
			}

			double remainingCooldown = GetRemainingCooldownAt(skillId, serverTime);

			if (remainingCooldown > 0d)
			{
				rejectionReason = SkillUseRejectionReason.Cooldown;
				return false;
			}

			if (skill.ManaCost > 0 && characterState.MP < skill.ManaCost)
			{
				rejectionReason = SkillUseRejectionReason.NotEnoughResource;
				return false;
			}

			return true;
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

		private bool IsSkillEquipped(FixedString64Bytes skillId)
		{
			for (int i = 0; i < skillLoadout.Count; i++)
			{
				if (skillLoadout[i].SkillId.Equals(skillId))
					return true;
			}

			return false;
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

		private void SendSkillUseRejectedEvent(NetworkSkillUseRequest request, SkillUseRejectionReason reason)
		{
			if (!IsServer)
				return;

			NetworkSkillUseRejectedEvent networkEvent =
				new NetworkSkillUseRejectedEvent(
					request.SkillId,
					request.RequestSequence,
					reason
				);

			NotifySkillUseRejectedRpc(networkEvent);
		}

		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
		private void NotifySkillUseRejectedRpc(NetworkSkillUseRejectedEvent networkEvent)
		{
			SkillUseRejectedEvent rejectedEvent =
				NetworkSkillContractMapper.ToContract(networkEvent);

			SkillUseRejected?.Invoke(rejectedEvent);
		}

		private void SendSkillCastEvent(NetworkSkillUseRequest request, double castServerTime)
		{
			if (!IsServer)
				return;

			NetworkSkillCastEvent networkEvent =
				new NetworkSkillCastEvent(
					NetworkObjectId,
					request.SkillId,
					transform.position,
					request.Target,
					request.RequestSequence,
					castServerTime
				);

			NotifySkillCastRpc(networkEvent);
		}

		[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
		private void NotifySkillCastRpc(NetworkSkillCastEvent networkEvent)
		{
			SkillCastEvent castEvent = NetworkSkillContractMapper.ToContract(networkEvent);

			SkillCast?.Invoke(castEvent);
		}

		private void SendSkillHitEvent(
			ulong targetObjectId,
			FixedString64Bytes skillId,
			int damage,
			int targetHpAfterHit,
			Vector3 hitPosition,
			uint requestSequence,
			ushort hitIndex
		)
		{
			if (!IsServer)
				return;

			double hitServerTime = NetworkManager.ServerTime.Time;

			NetworkSkillHitEvent networkEvent =
				new NetworkSkillHitEvent(
					NetworkObjectId,
					targetObjectId,
					skillId,
					damage,
					targetHpAfterHit,
					hitPosition,
					requestSequence,
					hitIndex,
					hitServerTime
				);

			NotifySkillHitRpc(networkEvent);
		}

		[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
		private void NotifySkillHitRpc(NetworkSkillHitEvent networkEvent)
		{
			SkillHitEvent hitEvent = NetworkSkillContractMapper.ToContract(networkEvent);

			SkillHit?.Invoke(hitEvent);
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

			TryRequestUseSkill(KeyMapping.W);
		}

		[ContextMenu("Test - Request Invalid Slot")]
		private void TestRequestInvalidSlot()
		{
			if (!CanRunOwnerTest())
				return;

			TryRequestUseSkill((KeyMapping)999);
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

