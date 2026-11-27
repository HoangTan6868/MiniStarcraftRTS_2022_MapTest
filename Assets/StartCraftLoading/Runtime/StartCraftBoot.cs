using UnityEngine;

/// <summary>
/// Gan san trong StartCraftBoot.unity do menu editor tao.
/// Chi doi Build Settings index 0 sang StartCraftBoot SAU khi da kiem thu.
/// </summary>
public class StartCraftBoot : MonoBehaviour
{
    private void Start() => StartCraftSceneLoader.LoadLocal("LoginRegister");
}
