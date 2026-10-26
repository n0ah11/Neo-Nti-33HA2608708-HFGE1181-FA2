// FA2 UI Builder - Editor-only helper.
// Builds the HUD (hearts + score), Pause menu, Win/Game Over screen in Level1,
// and the Main Menu scene. Runs automatically when Assets/Editor/BUILD_UI_NOW.txt exists,
// or from the menu FA2 > Build UI. Safe to run again (it replaces what it built before).

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
public static class FA2UIBuilder
{
    const string MARKER = "Assets/Editor/BUILD_UI_NOW.txt";
    const string FONT = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    const string TMP_PKG = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
    const string LEVEL = "Assets/Scenes/Level1.unity";
    const string MENU = "Assets/Scenes/MainMenu.unity";

    static readonly Color Panel = new Color(0, 0, 0, 0.45f);
    static readonly Color Overlay = new Color(0.05f, 0.05f, 0.1f, 0.78f);
    static readonly Color ButtonCol = new Color(0.20f, 0.62f, 0.36f);   // grass green
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f);

    static TMP_FontAsset font;

    static FA2UIBuilder()
    {
        if (!File.Exists(MARKER)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            AssetDatabase.DeleteAsset(MARKER);
            Run();
        };
    }

    [MenuItem("FA2/Build UI")]
    public static void Run()
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT) == null)
        {
            // TextMeshPro essentials (default font) are not in the project yet - import them first.
            AssetDatabase.importPackageCompleted += OnTmpImported;
            AssetDatabase.ImportPackage(TMP_PKG, false);
            return;
        }
        try { BuildAll(); }
        catch (System.Exception e) { Debug.LogError("FA2 UI builder failed: " + e); }
    }

    static void OnTmpImported(string name)
    {
        AssetDatabase.importPackageCompleted -= OnTmpImported;
        AssetDatabase.Refresh();
        EditorApplication.delayCall += () =>
        {
            try { BuildAll(); }
            catch (System.Exception e) { Debug.LogError("FA2 UI builder failed: " + e); }
        };
    }

    static void BuildAll()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT);
        if (font == null) { Debug.LogError("FA2: TMP font not found at " + FONT); return; }

        EditorSceneManager.SaveOpenScenes();
        BuildLevelUI();
        BuildMainMenu();
        EditorSceneManager.OpenScene(LEVEL);
        Debug.Log("FA2: UI built (Level1 HUD, pause, win/lose) and Main Menu scene.");
    }

    // ===================== LEVEL 1 =====================
    static void BuildLevelUI()
    {
        var scene = EditorSceneManager.OpenScene(LEVEL);
        DestroyNamed("UI Canvas");
        DestroyNamed("EventSystem");
        MakeEventSystem();

        var canvas = MakeCanvas("UI Canvas");
        var root = canvas.transform;

        // ---------- HUD ----------
        var hud = Stretch(new GameObject("HUD", typeof(RectTransform)), root);

        // hearts: top-left, grey heart behind each red heart
        var heartsBg = Box("Hearts Panel", hud, new Vector2(0, 1), new Vector2(30, -30), new Vector2(300, 110), Panel);
        var heartsRow = new GameObject("Hearts", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        heartsRow.transform.SetParent(heartsBg, false);
        Stretch(heartsRow, heartsBg);
        var hl = heartsRow.GetComponent<HorizontalLayoutGroup>();
        hl.padding = new RectOffset(20, 20, 15, 15); hl.spacing = 10;
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childControlWidth = hl.childControlHeight = false;
        hl.childForceExpandWidth = hl.childForceExpandHeight = false;

        var grey = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Grey.png");
        var red = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Red.png");
        var redHearts = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var slot = Img("Heart Slot " + (i + 1), heartsRow.transform, grey, new Vector2(80, 80));
            // the supplied grey icon looks almost the same as the full one, so dim it clearly
            slot.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.6f);
            var r = Img("Heart", slot.transform, red, new Vector2(80, 80));
            redHearts[i] = r;
        }

        // score: top-right
        var scoreBg = Box("Score Panel", hud, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(380, 110), Panel);
        var score = Text("Score Text", scoreBg, "Score: 0", 56, TextAlignmentOptions.Center, Color.white);
        Stretch(score.gameObject, scoreBg);
        score.gameObject.AddComponent<ScorePunch>();

        // controls hint: bottom-left
        var hint = Text("Controls Hint", hud, "A/D move   Space jump (x2)   E interact   Esc pause", 26,
                        TextAlignmentOptions.BottomLeft, new Color(1, 1, 1, 0.75f));
        Anchor(hint.rectTransform, new Vector2(0, 0), new Vector2(30, 20), new Vector2(1000, 50));

        // ---------- PAUSE ----------
        var pause = Stretch(new GameObject("PausePanel", typeof(RectTransform), typeof(Image)), root);
        pause.GetComponent<Image>().color = Overlay;
        var pTitle = Text("Title", pause.transform, "PAUSED", 120, TextAlignmentOptions.Center, Color.white);
        Anchor(pTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(900, 160));
        var resume = MakeButton("Resume Button", pause.transform, "Resume", new Vector2(0, 30));
        var pMenu = MakeButton("Main Menu Button", pause.transform, "Main Menu", new Vector2(0, -110));
        var sceneMngr = pMenu.gameObject.AddComponent<SceneMNGR>();
        sceneMngr.SceneName = "MainMenu";
        UnityEventTools.AddPersistentListener(pMenu.onClick, sceneMngr.LoadScene);

        // ---------- WIN / GAME OVER ----------
        var end = Stretch(new GameObject("EndPanel", typeof(RectTransform), typeof(Image)), root);
        end.GetComponent<Image>().color = Overlay;
        var eTitle = Text("Title", end.transform, "LEVEL COMPLETE!", 110, TextAlignmentOptions.Center, Gold);
        Anchor(eTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 260), new Vector2(1400, 150));
        var eScore = Text("Final Score", end.transform, "Score: 0", 90, TextAlignmentOptions.Center, Gold);
        Anchor(eScore.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(1000, 130));
        var again = MakeButton("Play Again Button", end.transform, "Play Again", new Vector2(0, -60));
        var eMenu = MakeButton("Main Menu Button", end.transform, "Main Menu", new Vector2(0, -200));

        // ---------- scripts ----------
        var ui = canvas.gameObject.AddComponent<UIManager>();
        var so = new SerializedObject(ui);
        so.FindProperty("hudUI").objectReferenceValue = hud;
        so.FindProperty("pauseUI").objectReferenceValue = pause;
        so.FindProperty("scoreText").objectReferenceValue = score;
        var hv = so.FindProperty("heartVisuals");
        hv.arraySize = 3;
        for (int i = 0; i < 3; i++) hv.GetArrayElementAtIndex(i).objectReferenceValue = redHearts[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(resume.onClick, ui.Resume);

        var endUI = canvas.gameObject.AddComponent<LevelEndUI>();
        endUI.endPanel = end;
        endUI.titleText = eTitle;
        endUI.finalScoreText = eScore;
        endUI.winColour = Gold;
        UnityEventTools.AddPersistentListener(again.onClick, endUI.Retry);
        UnityEventTools.AddStringPersistentListener(eMenu.onClick, endUI.LoadMainMenu, "MainMenu");

        pause.SetActive(false);
        end.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ===================== MAIN MENU =====================
    static void BuildMainMenu()
    {
        var scene = EditorSceneManager.OpenScene(MENU);
        DestroyNamed("UI Canvas");
        DestroyNamed("EventSystem");
        DestroyNamed("MainMenu");
        MakeEventSystem();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.36f, 0.62f, 0.86f);
        }

        var canvas = MakeCanvas("UI Canvas");
        var root = canvas.transform;

        // sky background with a strip of ground
        var bg = Stretch(new GameObject("Background", typeof(RectTransform), typeof(Image)), root);
        bg.GetComponent<Image>().color = new Color(0.36f, 0.62f, 0.86f);
        var ground = new GameObject("Ground", typeof(RectTransform), typeof(Image));
        ground.transform.SetParent(root, false);
        var grt = ground.GetComponent<RectTransform>();
        grt.anchorMin = new Vector2(0, 0); grt.anchorMax = new Vector2(1, 0);
        grt.pivot = new Vector2(0.5f, 0); grt.sizeDelta = new Vector2(0, 160); grt.anchoredPosition = Vector2.zero;
        ground.GetComponent<Image>().color = new Color(0.55f, 0.33f, 0.2f);
        var grass = new GameObject("Grass", typeof(RectTransform), typeof(Image));
        grass.transform.SetParent(ground.transform, false);
        var gr = grass.GetComponent<RectTransform>();
        gr.anchorMin = new Vector2(0, 1); gr.anchorMax = new Vector2(1, 1);
        gr.pivot = new Vector2(0.5f, 1); gr.sizeDelta = new Vector2(0, 30); gr.anchoredPosition = Vector2.zero;
        grass.GetComponent<Image>().color = ButtonCol;

        // characters standing on the ground
        var playerSprite = FirstSprite("Assets/Simple 2D Platformer BE2/Sprites/Player 16x16.png", "Player 16x16_0");
        var enemySprite = FirstSprite("Assets/Simple 2D Platformer BE2/Sprites/Enemy 16x16.png", "Enemy 16x16_0");
        var coinSprite = FirstSprite("Assets/Simple 2D Platformer BE2/Sprites/Props/Coins.png", "Gold 0");
        var p = Img("Player Art", root, playerSprite, new Vector2(160, 160));
        Anchor(p.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(-520, 160), new Vector2(160, 160), new Vector2(0.5f, 0));
        var e = Img("Enemy Art", root, enemySprite, new Vector2(160, 160));
        Anchor(e.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(520, 160), new Vector2(160, 160), new Vector2(0.5f, 0));
        e.transform.localScale = new Vector3(-1, 1, 1);
        for (int i = 0; i < 3; i++)
        {
            var c = Img("Coin Art " + i, root, coinSprite, new Vector2(70, 70));
            Anchor(c.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(-120 + i * 120, 400 + (i == 1 ? 40 : 0)), new Vector2(70, 70), new Vector2(0.5f, 0));
        }

        var title = Text("Title", root, "COIN CLIMBER", 150, TextAlignmentOptions.Center, Color.white);
        Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1600, 200));
        var sub = Text("Subtitle", root, "Collect the coins. Pull the levers. Reach the flag.", 40, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.9f));
        Anchor(sub.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -300), new Vector2(1600, 70));

        var play = MakeButton("Play Button", root, "Play", new Vector2(0, 60));
        var quit = MakeButton("Quit Button", root, "Quit", new Vector2(0, -80));

        var controls = Text("Controls", root, "A/D move   Space jump (x2)   E interact   Esc pause", 30,
                            TextAlignmentOptions.Center, Color.white);
        Anchor(controls.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(1400, 50), new Vector2(0.5f, 0));

        var mm = new GameObject("MainMenu").AddComponent<MainMenu>();
        mm.firstLevelScene = "Level1";
        UnityEventTools.AddPersistentListener(play.onClick, mm.PlayGame);
        UnityEventTools.AddPersistentListener(quit.onClick, mm.QuitGame);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ===================== helpers =====================
    static void DestroyNamed(string name)
    {
        GameObject go;
        while ((go = GameObject.Find(name)) != null) Object.DestroyImmediate(go);
    }

    static void MakeEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static Canvas MakeCanvas(string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;
        var s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.matchWidthOrHeight = 0.5f;
        return c;
    }

    static GameObject Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot ?? anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Transform Box(string name, GameObject parent, Vector2 anchor, Vector2 pos, Vector2 size, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);
        go.GetComponent<Image>().color = col;
        Anchor(go.GetComponent<RectTransform>(), anchor, pos, size);
        return go.transform;
    }

    static GameObject Img(string name, Transform parent, Sprite sprite, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        go.GetComponent<RectTransform>().sizeDelta = size;
        return go;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string text, float size, TextAlignmentOptions align, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.alignment = align;
        t.color = col;
        t.raycastTarget = false;
        return t;
    }

    static TextMeshProUGUI Text(string name, GameObject parent, string text, float size, TextAlignmentOptions align, Color col)
        => Text(name, parent.transform, text, size, align, col);

    static Button MakeButton(string name, Transform parent, string label, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Anchor(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), pos, new Vector2(460, 110), new Vector2(0.5f, 0.5f));
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        var b = go.GetComponent<Button>();
        var cb = b.colors;
        cb.normalColor = ButtonCol;
        cb.highlightedColor = new Color(0.30f, 0.78f, 0.47f);
        cb.selectedColor = new Color(0.30f, 0.78f, 0.47f);
        cb.pressedColor = new Color(0.13f, 0.42f, 0.24f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        var t = Text("Label", go.transform, label, 54, TextAlignmentOptions.Center, Color.white);
        Stretch(t.gameObject, go.transform);
        return b;
    }

    static Sprite FirstSprite(string png, string name)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(png))
            if (o is Sprite s && s.name == name) return s;
        return null;
    }
}
