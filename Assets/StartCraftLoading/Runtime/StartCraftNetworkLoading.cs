using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gan tren CHINH GameObject NetworkManager cua MultiplayerLobby.
/// Goi StartMatch tu nut START MATCH, chi HOST thuc hien.
/// Ngo scene events giup clients tu thay giao dien loading, KHONG dung SceneManager.LoadSceneAsync.
/// </summary>
public class StartCraftNetworkLoading : MonoBehaviour
{
    [Header("Target scene must be in Build Settings")]
    public string matchScene = "GameplayLAN_WithMap";

    private NetworkManager net;
    private bool listeningToSceneEvents;
    private bool startingMatch;

    private void Awake()
    {
        net = GetComponent<NetworkManager>();
        if (net == null) net = NetworkManager.Singleton;
        if (net == null)
            Debug.LogWarning("[StartCraft] Put StartCraftNetworkLoading on NetworkManager GameObject.");
    }

    private void Update()
    {
        if (net == null) net = NetworkManager.Singleton;
        if (net == null || !net.IsListening || net.SceneManager == null) return;
        if (!listeningToSceneEvents)
        {
            net.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
            listeningToSceneEvents = true;
        }
    }

    public void StartMatch()
    {
        if (net == null || !net.IsListening || !net.IsHost)
        {
            Debug.LogWarning("[StartCraft] Only the HOST can start the match.");
            return;
        }
        if (startingMatch) return;
        if (!Application.CanStreamedLevelBeLoaded(matchScene))
        {
            Debug.LogError("[StartCraft] Add match Scene to Build Settings: " + matchScene);
            return;
        }
        if (!net.NetworkConfig.EnableSceneManagement)
        {
            Debug.LogError("[StartCraft] Enable Scene Management on NetworkManager.");
            return;
        }

        // Sub truoc khi goi SceneManager.LoadScene tren NGO.
        if (!listeningToSceneEvents && net.SceneManager != null)
        {
            net.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
            listeningToSceneEvents = true;
        }

        startingMatch = true;
        StartCraftLoadingUI.Show("STARTCRAFT", "HOST DANG KHOI TAO TRAN...", true);
        SceneEventProgressStatus status = net.SceneManager.LoadScene(matchScene, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            startingMatch = false;
            Debug.LogError("[StartCraft] Cannot load network scene. Status: " + status);
            StartCraftLoadingUI.SetStatus("KHONG THE BAT DAU TRAN!");
            StartCraftLoadingUI.Hide();
        }
    }

    private void OnNetworkSceneEvent(SceneEvent e)
    {
        if (e.SceneName != matchScene) return;

        switch (e.SceneEventType)
        {
            case SceneEventType.Load:
                StartCraftLoadingUI.Show("STARTCRAFT", "DANG TAI BAN DO CHO TRAN DAU...", true);
                break;
            case SceneEventType.LoadComplete:
                StartCraftLoadingUI.SetStatus("DA TAI BAN DO - DANG CHO DONG DOI...");
                break;
            case SceneEventType.LoadEventCompleted:
                StartCraftLoadingUI.SetStatus("TAT CA NGUOI CHOI DA SAN SANG!");
                StartCraftLoadingUI.Hide();
                startingMatch = false;
                break;
        }
    }

    private void OnDestroy()
    {
        if (listeningToSceneEvents && net != null && net.SceneManager != null)
            net.SceneManager.OnSceneEvent -= OnNetworkSceneEvent;
        listeningToSceneEvents = false;
    }
}
