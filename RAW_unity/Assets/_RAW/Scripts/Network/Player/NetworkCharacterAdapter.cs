using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacterAdapter : NetworkBehaviour
    {
        [SerializeField] private Char_Control characterControl;
        [SerializeField] private GameObject[] localOnlyVisuals;

        private readonly NetworkVariable<bool> isFacingLeft =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

        private bool lastFacingLeft;

        private void Reset()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            if (!IsSpawned || !IsOwner)
                return;

            bool currentFacingLeft = transform.localScale.x < 0f;

            if (currentFacingLeft == lastFacingLeft)
                return;

            lastFacingLeft = currentFacingLeft;
            
            if (IsServer)
                isFacingLeft.Value  = currentFacingLeft;
            else
                SubmitFacingRPC(currentFacingLeft);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SubmitFacingRPC(bool facingLeft)
        {
            isFacingLeft.Value = facingLeft;
        }

        private void HandleFacingChanged(bool previousValue, bool newValue)
        {
            if (!IsOwner)
                ApplyFacing(newValue);
        }

        private void ApplyFacing(bool facingLeft)
        {
            Vector3 scale = transform.localScale;
            float xMagnitude = Mathf.Abs(scale.x);

            if (xMagnitude < 0.0001f)
                xMagnitude = 1f;

            scale.x = facingLeft ? -xMagnitude : xMagnitude;
            transform.localScale = scale;
        }

        public override void OnNetworkSpawn()
        {
            HideLocalOnlyVisuals();

            if (characterControl == null)
            {
                Debug.LogError("Char_Control 컴포넌트를 찾을 수 없습니다.", this);

                enabled = false;
                return;
            }

            // 자신이 소유한 캐릭터만 로컬 입력 처리
            characterControl.enabled = IsOwner;

            isFacingLeft.OnValueChanged += HandleFacingChanged;

            lastFacingLeft = transform.localScale.x < 0f;

            if (IsOwner)
            {
                if (IsServer)
                    isFacingLeft.Value = lastFacingLeft;
                else
                    SubmitFacingRPC(lastFacingLeft);
            }
            else
            {
                ApplyFacing(isFacingLeft.Value);
            }

            Debug.Log($"네트워크 플레이어 생성: OwnerClientId={OwnerClientId}, IsOwner={IsOwner}", this);
        }

        public override void OnNetworkDespawn()
        {
            isFacingLeft.OnValueChanged -= HandleFacingChanged;
            HideLocalOnlyVisuals();

            if (characterControl != null)
                characterControl.enabled = false;
        }

        private void CacheComponents()
        {
            if (characterControl == null)
                characterControl = GetComponent<Char_Control>();
        }

        private void HideLocalOnlyVisuals()
        {
            if (localOnlyVisuals == null)
                return;

            foreach (GameObject visual in localOnlyVisuals)
            {
                if (visual != null)
                    visual.SetActive(false);
            }
        }
    }
}
