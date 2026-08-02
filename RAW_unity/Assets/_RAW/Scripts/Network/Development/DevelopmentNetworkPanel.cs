using UnityEngine;

namespace RAW.Network
{
    public class DevelopmentNetworkPanel : MonoBehaviour
    {
        [SerializeField] private NetworkBootstrap networkBootstrap;
        [SerializeField] private string address = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
		[SerializeField] private string developmentUserId = "dev-player";

        private void OnGUI()
        {
            GUILayout.BeginArea(
                new Rect(20, 20, 260, 220),
                GUI.skin.box
            );

            GUILayout.Label($"상태: {networkBootstrap.CurrentMode}");

			GUILayout.Label("사용자 ID");
			developmentUserId = GUILayout.TextField(developmentUserId);

            GUILayout.Label("주소");
            address = GUILayout.TextField(address);

            GUILayout.Label($"포트: {port}");

            if (!networkBootstrap.IsListening)
            {
                if (GUILayout.Button("Host 시작"))
                {
                    if (networkBootstrap.ConfigureIdentity(developmentUserId) &&
						networkBootstrap.ConfigureEndpoint(
							address,
							port,
							"0.0.0.0"))
                    {
                        networkBootstrap.StartHost();
                    }
                }

                if (GUILayout.Button("Client 시작"))
                {
                    if (networkBootstrap.ConfigureIdentity(developmentUserId) &&
						networkBootstrap.ConfigureEndpoint(address, port))
                    {
                        networkBootstrap.StartClient();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("연결 종료"))
                    networkBootstrap.Shutdown();
            }

            GUILayout.EndArea();
        }
    }
}