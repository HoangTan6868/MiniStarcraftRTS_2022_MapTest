#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tao prefab Loading bang UI pixel-art co san trong Tiny Swords.
/// Khong kem lai cac PNG co ban quyen trong zip, chi tham chieu sprite da import.
/// </summary>
public static class StartCraftLoadingGenerator
{
    private const string ResourcesFolder = "Assets/StartCraftLoading/Resources";
    private const string PrefabPath = ResourcesFolder + "/StartCraftLoadingCanvas.prefab";
    private const string BootPath = "Assets/Scenes/StartCraftBoot.unity";

    [MenuItem("Tools/StartCraft/1. Generate Bright Loading UI")]
    public static void Generate()
    {
        Directory.CreateDirectory(ResourcesFolder);
        AssetDatabase.Refresh();

        var root = new GameObject("StartCraftLoadingCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster),
            typeof(CanvasGroup), typeof(StartCraftLoadingUI));

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var bg = Image(root.transform, "Sky_Background", new Vector2(0,0),
            new Vector2(1920,1080), Html("#B7F1E9"));
        Stretch(bg.rectTransform);
        var bottom = Image(root.transform, "Grass_Tint", new Vector2(0,-410),
            new Vector2(1920,430), Html("#9CE2AE"));
        // Cac duong trang tri nang dong, sang va pixel.
        Image(root.transform, "SunGlow", new Vector2(725,370), new Vector2(220,220), Html("#FFF0B5"));
        Image(root.transform, "Horizon", new Vector2(0,-330), new Vector2(1920,20), Html("#76C6A9"));
        Image(root.transform, "GroundBand", new Vector2(0,-515), new Vector2(1920,22), Html("#5CA893"));
        Image(root.transform, "Cloud_1", new Vector2(-630,400), new Vector2(220,37), Html("#E9FFFB"));
        Image(root.transform, "Cloud_2", new Vector2(-495,375), new Vector2(170,24), Html("#E9FFFB"));
        Image(root.transform, "Cloud_3", new Vector2(495,422), new Vector2(155,28), Html("#E9FFFB"));

        Sprite leftCastle = GetSprite(new[]
        {
            "Assets/Tiny Swords (Free Pack)/Buildings/Blue Buildings/Castle.png",
            "Assets/Art/MenuUI/Decorations/Blue_Castle.png"
        }, "Castle.png", "Tiny Swords (Free Pack)");
        Sprite rightCastle = GetSprite(new[]
        {
            "Assets/Tiny Swords (Update 010)/Factions/Knights/Buildings/Castle/Castle_Blue.png"
        }, "Castle_Blue.png", "Tiny Swords (Update 010)");
        Sprite banner = GetSprite(new[]
        {
            "Assets/Tiny Swords (Update 010)/UI/Ribbons/Ribbon_Blue_3Slides.png",
            "Assets/Art/MenuUI/Banners/Ribbon_Blue_3Slides.png"
        }, "Ribbon_Blue_3Slides.png", "Tiny Swords (Update 010)");

        Sprite hintDecor = GetSprite(new[]
        {
            "Assets/Tiny Swords (Free Pack)/UI Elements/UI Elements/Human Avatars/Avatars_01.png"
        }, "Avatars_01.png", "Tiny Swords (Free Pack)");

        var castleL = Image(root.transform, "FreePack_BlueCastle", new Vector2(-660,55),
            new Vector2(410,325), Color.white, leftCastle); castleL.preserveAspect = true;
        var castleR = Image(root.transform, "Update010_BlueCastle", new Vector2(665,75),
            new Vector2(410,325), Color.white, rightCastle); castleR.preserveAspect = true;

        Image(root.transform, "BannerShadow", new Vector2(0,332),
            new Vector2(826,230), Html("#68BABB"));
        var ribbon = Image(root.transform, "Update010_Ribbon", new Vector2(0,344),
            new Vector2(850,252), Color.white, banner);
        ribbon.preserveAspect = true;

        Text(root.transform, "Title_STARTCRAFT", "STARTCRAFT", new Vector2(0,355),
            new Vector2(660,120), 76, Html("#FFF3C7"), FontStyles.Bold);
        Text(root.transform, "Subtitle", "XÂY DỰNG • CHIẾN ĐẤU • CHINH PHỤC", new Vector2(0,210),
            new Vector2(1040,60), 34, Html("#18556B"), FontStyles.Bold);

        Image(root.transform, "Card_Shadow", new Vector2(0,-125),
            new Vector2(1085,328), Html("#2C7894"));
        Image(root.transform, "Card_Border", new Vector2(0,-110),
            new Vector2(1090,330), Html("#F5DC91"));
        Image(root.transform, "Card_Inner", new Vector2(0,-110),
            new Vector2(1075,315), Html("#F9FFF3"));
        Text(root.transform, "Loading_Heading", "ĐANG CHUẨN BỊ...", new Vector2(0,-7),
            new Vector2(930,72), 44, Html("#207486"), FontStyles.Bold);
        TMP_Text status = Text(root.transform, "Loading_Status", "ĐANG TẢI BẢN ĐỒ...", new Vector2(0,-83),
            new Vector2(930,56), 31, Html("#356B77"), FontStyles.Normal);

        Image(root.transform, "Progress_Outline", new Vector2(0,-161),
            new Vector2(880,66), Html("#1C6B7E"));
        Image(root.transform, "Progress_Background", new Vector2(0,-161),
            new Vector2(860,48), Html("#D2ECE8"));
        var fill = Image(root.transform, "Progress_ActualFill", new Vector2(0,-161),
            new Vector2(860,48), Html("#F7C75D"));
        fill.type = UnityEngine.UI.Image.Type.Filled;
        fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;

        TMP_Text percent = Text(root.transform, "Percent", "0%", new Vector2(0,-222),
            new Vector2(260,56), 32, Html("#135668"), FontStyles.Bold);
        var tip = Text(root.transform, "Hint", "TIP: Chọn vị trí căn cứ thuận lợi để phòng thủ!", new Vector2(0,-366),
            new Vector2(1350,60), 29, Html("#185E59"), FontStyles.Normal);
        if (hintDecor != null)
        {
            var avatar = Image(root.transform, "FreePack_Avatar", new Vector2(-575,-350),
                new Vector2(120,120), Color.white, hintDecor);
            avatar.preserveAspect = true;
        }

        var ui = root.GetComponent<StartCraftLoadingUI>();
        ui.canvas = canvas;
        ui.canvasGroup = root.GetComponent<CanvasGroup>();
        ui.headingText = null; // Tieu de STARTCRAFT co dinh, khong thay doi khi vao tran.
        ui.statusText = status;
        ui.percentageText = percent;
        ui.hintText = tip;
        ui.progressFill = fill;
        ui.minimumVisibleSeconds = 1.2f;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StartCraft] Created bright Loading prefab: " + PrefabPath);
    }

    [MenuItem("Tools/StartCraft/2. Create optional Boot scene")]
    public static void CreateOptionalBoot()
    {
        Directory.CreateDirectory("Assets/Scenes");
        if (File.Exists(BootPath))
        {
            Debug.Log("[StartCraft] Boot scene already exists: " + BootPath);
            AddScenesIfMissing();
            return;
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var go = new GameObject("StartCraftBoot");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<StartCraftBoot>();
        EditorSceneManager.SaveScene(scene, BootPath);
        EditorSceneManager.CloseScene(scene, true);
        AddScenesIfMissing();
        Debug.Log("[StartCraft] Created optional boot scene. Move it to Build Settings index 0 only after testing.");
    }

    [MenuItem("Tools/StartCraft/3. Add gameplay scene to Build Settings")]
    public static void AddScenesIfMissing()
    {
        string[] candidatePaths = {"Assets/Scenes/GameplayLAN_WithMap.unity", BootPath};
        var all = EditorBuildSettings.scenes.ToList();
        foreach (var path in candidatePaths)
        {
            if (!File.Exists(path) || all.Any(s => s.path == path)) continue;
            all.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = all.ToArray();
        Debug.Log("[StartCraft] Build Settings checked. Existing scene order was not changed.");
    }

    private static Color Html(string hex)
    {
        Color c; return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.white;
    }

    private static UnityEngine.UI.Image Image(Transform parent, string name, Vector2 center,
        Vector2 size, Color color, Sprite sprite = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;
        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text Text(Transform parent, string name, string value,
        Vector2 center, Vector2 size, float fontSize, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (f != null) text.font = f;
        return text;
    }

    private static Sprite GetSprite(string[] preferredPaths, string fileName, string preferredFolder)
    {
        foreach (var path in preferredPaths)
        {
            var s = LoadSpriteAtPath(path);
            if (s != null) return s;
        }
        // Fallback: neu nguoi dung da sap xep assets thanh thu muc khac.
        string key = Path.GetFileNameWithoutExtension(fileName);
        var guids = AssetDatabase.FindAssets(key + " t:Texture2D");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith("/" + fileName)) continue;
            if (path.Contains(preferredFolder))
            {
                var s = LoadSpriteAtPath(path);
                if (s != null) return s;
            }
        }
        Debug.LogWarning("[StartCraft] Sprite not found: " + fileName +
            " - UI still works with fallback colors. Check the two Tiny Swords packs are imported.");
        return null;
    }

    private static Sprite LoadSpriteAtPath(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null) return null;
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        var subs = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (subs.Length > 0) return subs[0];
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            // Chi chuyen sang Sprite voi PNG tai bo tai nguyen ve tinh, neu chua phai sprite.
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }
}
#endif
