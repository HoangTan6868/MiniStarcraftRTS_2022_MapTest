
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Collections;
using Unity.Networking.Transport.Relay;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayLobbyManager : MonoBehaviour
{
    [Header("NETWORK")]
    public NetworkManager networkManager;
    public UnityTransport unityTransport;

    [Header("ROOM UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField roomCodeInput;
    public TMP_Text playerCountText;
    public TMP_Text[] playerSlots = new TMP_Text[4];
    public TMP_Text statusText;

    [Header("BUTTONS")]
    public Button startMatchButton;

    private const int MaxPlayers = 4;
    private const int MaxClients = 3;

    private const string CountMessage = "LobbyPlayerCount";
    private const string RequestMessage = "LobbyCountRequest";

    private bool isBusy = false;
    private bool messageRegistered = false;
    private bool callbacksRegistered = false;
    private bool matchStarted = false;

    // =========================================
    // 1. KHOI TAO
    // =========================================

    private void Awake()
    {
        if (networkManager == null)
        {
            networkManager =
                FindObjectOfType<NetworkManager>();
        }

        if (networkManager == null)
        {
            Debug.LogError("Khong tim thay NetworkManager!");
            enabled = false;
            return;
        }

        if (unityTransport == null)
        {
            unityTransport =
                networkManager.GetComponent<UnityTransport>();
        }

        if (unityTransport == null)
        {
            Debug.LogError("Khong tim thay UnityTransport!");
            enabled = false;
            return;
        }

        networkManager.NetworkConfig.ConnectionApproval = true;

        networkManager.ConnectionApprovalCallback =
            ApproveConnection;

        networkManager.OnClientConnectedCallback +=
            OnClientConnected;

        networkManager.OnClientDisconnectCallback +=
            OnClientDisconnected;

        callbacksRegistered = true;

        if (roomCodeText != null)
            roomCodeText.text = "ROOM ID: ------";

        UpdatePlayerUI(0);

        // START MATCH chua duoc lap trinh
        if (startMatchButton != null)
            startMatchButton.interactable = false;

        Debug.Log("Relay Lobby Manager READY!");
    }

    // =========================================
    // 2. UNITY SERVICES
    // =========================================

    private async Task InitializeServices()
    {
        if (UnityServices.State !=
            ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();
        }

        Debug.Log("Unity Authentication READY!");
    }

    // =========================================
    // 3. CREATE ROOM - HOST
    // =========================================

    public async void CreateRoom()
    {
        if (isBusy || networkManager == null)
            return;

        if (networkManager.IsListening)
        {
            ShowStatus("Ban da o trong phong!");
            return;
        }

        isBusy = true;
        ShowStatus("Dang tao phong...");

        try
        {
            await InitializeServices();

            // Toi da 3 Client + 1 Host
            Allocation allocation =
                await RelayService.Instance
                    .CreateAllocationAsync(MaxClients);

            string joinCode =
                await RelayService.Instance
                    .GetJoinCodeAsync(allocation.AllocationId);

            unityTransport.SetRelayServerData(
                new RelayServerData(allocation, "dtls")
            );

            // Khoi dong Host truoc
            bool started = networkManager.StartHost();

            if (!started)
            {
                throw new Exception(
                    "Khong the khoi dong HOST!"
                );
            }

            // Dang ky message SAU khi khoi dong
            RegisterCountMessages();

            if (roomCodeText != null)
                roomCodeText.text = "ROOM ID: " + joinCode;

            BroadcastPlayerCount();

            ShowStatus(
                "Tao phong thanh cong! Ban la HOST."
            );

            Debug.Log("ROOM CODE: " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError("CREATE ROOM ERROR: " + e);

            ShowStatus(
                "Khong tao duoc phong: " + e.Message
            );
        }
        finally
        {
            isBusy = false;
        }
    }

    // =========================================
    // 4. JOIN ROOM - CLIENT
    // =========================================

    public async void JoinRoom()
    {
        if (isBusy || networkManager == null)
            return;

        if (networkManager.IsListening)
        {
            ShowStatus("Ban da o trong phong!");
            return;
        }

        if (roomCodeInput == null)
        {
            ShowStatus("Chua gan Room Code Input!");
            return;
        }

        string code = roomCodeInput.text
            .Trim()
            .ToUpperInvariant();

        if (string.IsNullOrEmpty(code))
        {
            ShowStatus("Vui long nhap ma phong!");
            return;
        }

        isBusy = true;
        ShowStatus("Dang tham gia phong...");

        try
        {
            await InitializeServices();

            JoinAllocation allocation =
                await RelayService.Instance
                    .JoinAllocationAsync(code);

            unityTransport.SetRelayServerData(
                new RelayServerData(allocation, "dtls")
            );

            // Khoi dong Client truoc
            bool started = networkManager.StartClient();

            if (!started)
            {
                throw new Exception(
                    "Khong the khoi dong CLIENT!"
                );
            }

            // Dang ky sau khi StartClient
            RegisterCountMessages();

            if (roomCodeText != null)
                roomCodeText.text = "ROOM ID: " + code;

            ShowStatus("Dang ket noi voi HOST...");

            // Neu da ket noi thi yeu cau cap nhat
            if (networkManager.IsConnectedClient)
            {
                RequestPlayerCount();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("JOIN ROOM ERROR: " + e);

            ShowStatus(
                "Khong vao duoc phong: " + e.Message
            );
        }
        finally
        {
            isBusy = false;
        }
    }

    // =========================================
    // 5. KIEM TRA GIOI HAN PHONG
    // =========================================

    private void ApproveConnection(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayers =
            networkManager.ConnectedClientsIds.Count;

        bool canJoin =
            !matchStarted &&
            currentPlayers < MaxPlayers;

        response.Approved = canJoin;
        response.CreatePlayerObject = false;
        response.Pending = false;

        if (!canJoin)
        {
            response.Reason =
                "Phong da day hoac tran da bat dau!";
        }
    }

    // =========================================
    // 6. DANG KY NETWORK MESSAGES
    // =========================================

    private void RegisterCountMessages()
    {
        if (messageRegistered)
            return;

        if (networkManager == null ||
            networkManager.CustomMessagingManager == null)
        {
            Debug.LogWarning(
                "CustomMessagingManager chua san sang!"
            );

            return;
        }

        networkManager.CustomMessagingManager
            .RegisterNamedMessageHandler(
                CountMessage,
                ReceivePlayerCount
            );

        networkManager.CustomMessagingManager
            .RegisterNamedMessageHandler(
                RequestMessage,
                ReceiveCountRequest
            );

        messageRegistered = true;

        Debug.Log("Lobby messages READY!");
    }

    // =========================================
    // 7. CLIENT KET NOI THANH CONG
    // =========================================

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager == null)
            return;

        if (networkManager.IsServer)
        {
            BroadcastPlayerCount();
        }
        else if (clientId == networkManager.LocalClientId)
        {
            ShowStatus("Da ket noi vao phong!");

            if (!messageRegistered)
                RegisterCountMessages();

            RequestPlayerCount();
        }
    }

    // =========================================
    // 8. CLIENT NGAT KET NOI
    // =========================================

    private void OnClientDisconnected(ulong clientId)
    {
        if (networkManager == null)
            return;

        if (networkManager.IsServer)
        {
            BroadcastPlayerCount();
        }
        else if (clientId == networkManager.LocalClientId)
        {
            string reason = networkManager.DisconnectReason;

            if (string.IsNullOrEmpty(reason))
            {
                ShowStatus("Da mat ket noi voi HOST!");
            }
            else
            {
                ShowStatus("Ngat ket noi: " + reason);
            }

            UpdatePlayerUI(0);
        }
    }

    // =========================================
    // 9. CLIENT YEU CAU CAP NHAT SO NGUOI
    // =========================================

    private void RequestPlayerCount()
    {
        if (networkManager == null ||
            networkManager.IsServer ||
            !networkManager.IsConnectedClient ||
            !messageRegistered)
        {
            return;
        }

        using (FastBufferWriter writer =
            new FastBufferWriter(16, Allocator.Temp))
        {
            writer.WriteValueSafe((byte)1);

            networkManager.CustomMessagingManager
                .SendNamedMessage(
                    RequestMessage,
                    NetworkManager.ServerClientId,
                    writer
                );
        }
    }

    // =========================================
    // 10. HOST NHAN YEU CAU TU CLIENT
    // =========================================

    private void ReceiveCountRequest(
        ulong senderId,
        FastBufferReader reader)
    {
        if (networkManager == null ||
            !networkManager.IsServer)
        {
            return;
        }

        // FIX LOI CS0453 VA CS0315:
        // ConnectedClients la dictionary.
        // ContainsKey kiem tra Client ID hop le.
        if (!networkManager.ConnectedClients.ContainsKey(senderId))
        {
            return;
        }

        SendPlayerCount(senderId);
    }

    // =========================================
    // 11. GUI SO NGUOI CHO MOT CLIENT
    // =========================================

    private void SendPlayerCount(ulong clientId)
    {
        if (networkManager == null ||
            !networkManager.IsServer ||
            !messageRegistered)
        {
            return;
        }

        int count =
            networkManager.ConnectedClientsIds.Count;

        using (FastBufferWriter writer =
            new FastBufferWriter(32, Allocator.Temp))
        {
            writer.WriteValueSafe(count);

            networkManager.CustomMessagingManager
                .SendNamedMessage(
                    CountMessage,
                    clientId,
                    writer
                );
        }
    }

    // =========================================
    // 12. HOST DONG BO DANH SACH
    // =========================================

    private void BroadcastPlayerCount()
    {
        if (networkManager == null ||
            !networkManager.IsServer)
        {
            return;
        }

        int count =
            networkManager.ConnectedClientsIds.Count;

        UpdatePlayerUI(count);

        if (!messageRegistered)
            return;

        foreach (ulong clientId in
            networkManager.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.ServerClientId)
                continue;

            SendPlayerCount(clientId);
        }
    }

    // =========================================
    // 13. CLIENT NHAN SO NGUOI
    // =========================================

    private void ReceivePlayerCount(
        ulong senderId,
        FastBufferReader reader)
    {
        if (networkManager == null ||
            networkManager.IsServer)
        {
            return;
        }

        if (senderId != NetworkManager.ServerClientId)
            return;

        reader.ReadValueSafe(out int count);

        UpdatePlayerUI(count);
    }

    // =========================================
    // 14. CAP NHAT PLAYER SLOTS
    // =========================================

    private void UpdatePlayerUI(int count)
    {
        count = Mathf.Clamp(count, 0, MaxPlayers);

        if (playerCountText != null)
        {
            playerCountText.text =
                "PLAYERS " + count + "/" + MaxPlayers;
        }

        for (int i = 0; i < MaxPlayers; i++)
        {
            if (playerSlots == null ||
                i >= playerSlots.Length ||
                playerSlots[i] == null)
            {
                continue;
            }

            string role = "EMPTY";

            if (i < count)
            {
                if (i == 0)
                    role = "HOST";
                else
                    role = "CLIENT";
            }

            playerSlots[i].text =
                "PLAYER " + (i + 1) + " - " + role;
        }
    }

    // =========================================
    // 15. HIEN THI THONG BAO
    // =========================================

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }

    // =========================================
    // 16. DON DEP SU KIEN
    // =========================================

    private void OnDestroy()
    {
        if (networkManager == null)
            return;

        if (callbacksRegistered)
        {
            networkManager.OnClientConnectedCallback -=
                OnClientConnected;

            networkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;

            networkManager.ConnectionApprovalCallback -=
                ApproveConnection;
        }

        if (messageRegistered &&
            networkManager.CustomMessagingManager != null)
        {
            networkManager.CustomMessagingManager
                .UnregisterNamedMessageHandler(
                    CountMessage
                );

            networkManager.CustomMessagingManager
                .UnregisterNamedMessageHandler(
                    RequestMessage
                );
        }
    }
}
