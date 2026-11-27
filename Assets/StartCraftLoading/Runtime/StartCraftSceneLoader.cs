using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Chuyen Scene ngoai phien Multiplayer (Menu / Login / Single Player).
/// KHONG dung ham nay de Start Match khi NetworkManager dang ket noi!
/// </summary>
public class StartCraftSceneLoader : MonoBehaviour
{
    private static StartCraftSceneLoader runner;
    private bool isLoading;

    public static void LoadLocal(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[StartCraft] Scene name is empty.");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("[StartCraft] Scene is not in Build Settings: " + sceneName);
            return;
        }

        // Bao ve session NGO/Relay dang ton tai.
        // Neu co NGO trong project, khong goi LoadLocal cho Start Match.
        if (Unity.Netcode.NetworkManager.Singleton != null &&
            Unity.Netcode.NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("[StartCraft] Network session is active. " +
                "Use StartCraftNetworkLoading.StartMatch() on HOST, not LoadLocal().");
            return;
        }

        if (runner == null)
        {
            GameObject go = new GameObject("StartCraftSceneLoader (Runtime)");
            runner = go.AddComponent<StartCraftSceneLoader>();
            DontDestroyOnLoad(go);
        }
        if (!runner.isLoading) runner.StartCoroutine(runner.LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;
        StartCraftLoadingUI.Show("STARTCRAFT", "DANG TAI: " + sceneName);
        yield return null; // Bao dam co 1 frame ve UI truoc khi tai.

        AsyncOperation op = null;
        try { op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single); }
        catch (Exception e)
        {
            Debug.LogError("[StartCraft] LoadSceneAsync failed: " + e);
            StartCraftLoadingUI.SetStatus("KHONG TAI DUOC MAN CHOI");
            StartCraftLoadingUI.Hide();
            isLoading = false;
            yield break;
        }
        if (op == null)
        {
            StartCraftLoadingUI.SetStatus("KHONG TAI DUOC MAN CHOI");
            StartCraftLoadingUI.Hide();
            isLoading = false;
            yield break;
        }

        // Unity progress 0..0.9 khi scene dang load, sau do hoan tat.
        while (!op.isDone)
        {
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            StartCraftLoadingUI.SetProgress(Mathf.Min(realProgress, 0.99f), "DANG CHUAN BI TAI NGUYEN...");
            yield return null;
        }
        StartCraftLoadingUI.SetProgress(1f, "HOAN TAT TAI MAN CHOI!");
        StartCraftLoadingUI.Hide();
        isLoading = false;
    }
}
