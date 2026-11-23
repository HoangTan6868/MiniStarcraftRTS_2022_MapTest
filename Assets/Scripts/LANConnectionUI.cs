
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class LANConnectionUI : MonoBehaviour
{
    public string hostIP = "10.7.76.152";
    public ushort port = 7777;

    private NetworkManager net;
    private UnityTransport transport;

    void Start()
    {
        net = GetComponent<NetworkManager>();
        transport = GetComponent<UnityTransport>();
    }

    void OnGUI()
    {
        if (net == null || transport == null)
        {
            GUI.Label(new Rect(20, 20, 400, 40),
                "Missing NetworkManager or UnityTransport");
            return;
        }

        
        GUILayout.BeginArea(
            new Rect(15, 15, 270, net.IsListening ? 100 : 195),
            GUI.skin.box
        );


        GUILayout.Label("MINI STARCRAFT RTS - LAN");

        if (!net.IsListening)
        {
            GUILayout.Label("Host IP:");
            hostIP = GUILayout.TextField(hostIP);

            if (GUILayout.Button("HOST GAME", GUILayout.Height(40)))
            {
                transport.SetConnectionData(
                    "127.0.0.1",
                    port,
                    "0.0.0.0"
                );

                if (net.StartHost())
                    Debug.Log("LAN Host started");
                else
                    Debug.LogError("Cannot start Host");
            }

            if (GUILayout.Button("JOIN GAME", GUILayout.Height(40)))
            {
                transport.SetConnectionData(
                    hostIP.Trim(),
                    port
                );

                if (net.StartClient())
                    Debug.Log("Connecting to " + hostIP);
                else
                    Debug.LogError("Cannot start Client");
            }
        }
        else
        {
            if (net.IsHost)
                GUILayout.Label("Status: HOST RUNNING");
            else if (net.IsConnectedClient)
                GUILayout.Label("Status: CONNECTED TO HOST");
            else
                GUILayout.Label("Status: CONNECTING...");

            if (GUILayout.Button("DISCONNECT"))
                net.Shutdown();
        }

        GUILayout.EndArea();
    }
}
