using UnityEngine;

/// <summary>
/// Gắn vao 1 GameObject trong StartMenu/LoginRegister de chon trong Button.OnClick().
/// Duoc dung cho cac Scene KHONG co session NGO dang chay.
/// </summary>
public class StartCraftSceneButtons : MonoBehaviour
{
    public void OpenLogin() => StartCraftSceneLoader.LoadLocal("LoginRegister");
    public void OpenStartMenu() => StartCraftSceneLoader.LoadLocal("StartMenu");
    public void OpenMultiplayerLobby() => StartCraftSceneLoader.LoadLocal("MultiplayerLobby");
    public void OpenSinglePlayer() => StartCraftSceneLoader.LoadLocal("GameplayLAN_WithMap");
}
