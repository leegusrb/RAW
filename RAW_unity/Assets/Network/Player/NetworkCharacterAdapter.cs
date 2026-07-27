using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacterAdapter : NetworkBehaviour
    {
        [SerializeField] private Char_Control characterControl;

        private void Reset()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        public override void OnNetworkSpawn()
        {
            if (characterControl == null)
            {
                Debug.LogError("Char_Control 컴포넌트를 찾을 수 없습니다.", this);

                enabled = false;
                return;
            }

            // 자신이 소유한 캐릭터만 로컬 입력 처리
            characterControl.enabled = IsOwner;

            Debug.Log($"네트워크 플레이어 생성: OwnerClientId={OwnerClientId}, IsOwner={IsOwner}", this);
        }

        public override void OnNetworkDespawn()
        {
            if (characterControl != null)
                characterControl.enabled = false;
        }

        private void CacheComponents()
        {
            if (characterControl == null)
                characterControl = GetComponent<Char_Control>();
        }
    }
}
