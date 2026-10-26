// FA2 Level Builder - Editor-only helper.
// Menu: FA2 > Build Level 1
// Rebuilds Level1 from the map below: tiles, coins, spikes, enemies (with patrol points),
// disappearing platforms, bridge lever, door lever, spike switch, flag, camera rig and kill box.
// Safe to run again: it clears what it built last time first.
// After you are happy with the level you can delete this file (Assets/Editor/FA2LevelBuilder.cs).

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class FA2LevelBuilder
{
    // If this marker file exists when scripts reload, the level is built automatically (then the marker is deleted).
    const string MARKER = "Assets/Editor/BUILD_LEVEL_NOW.txt";
    static FA2LevelBuilder()
    {
        if (!System.IO.File.Exists(MARKER)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            AssetDatabase.DeleteAsset(MARKER);
            if (EditorSceneManager.GetActiveScene().name != "Level1")
                EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
            try { Build(); }
            catch (System.Exception e) { Debug.LogError("FA2 builder failed: " + e); }
        };
    }

    // One character = one tile. Top row is highest. Bottom-left tile sits at world (OX, OY).
    // #  ground          P  player start     c/s/G  bronze/silver/gold coin
    // ^  spike           E  enemy            a/b    enemy patrol points
    // L  bridge lever    -  hidden bridge    =      disappearing platform
    // K  door lever      D  door (bottom)    S      spike switch   !  switchable spike
    // F  flag
    static readonly string[] Map =
    {
        "..................................................................................",
        "..................G................................................G..............",
        "...........................s..s..s................................................",
        "............................................s......s...........S..................",
        ".........................a...E....b.L....................K..D..#!!!!!!!....a.E.b.F",
        ".................###...##############------####.=..=..=###########################",
        "...............#####^^^##############......####........###########################",
        ".P..c.c.c....########################......####........###########################",
        "#####################################......####........###########################",
        "#####################################......####........###########################",
    };

    const int OX = -10, OY = -6;
    const string GEN = "FA2 ";               // prefix for generated non-prefab objects
    const int GroundLayer = 3;
    const int DoorHeight = 6;

    const string P = "Assets/Prefabs/";
    const string TILES = "Assets/Simple 2D Platformer BE2/Tile Palettes/tileset_sliced 16x16_";
    const string COINS_PNG = "Assets/Simple 2D Platformer BE2/Sprites/Props/Coins.png";
    const string TILESET_PNG = "Assets/Simple 2D Platformer BE2/Sprites/tileset_sliced 16x16.png";
    const string ART = "Assets/Sprites/Props/";

    [MenuItem("FA2/Build Level 1 %&b")]  // Ctrl+Alt+B
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Level1")
        {
            Debug.LogError("FA2: open Assets/Scenes/Level1 and run this again.");
            return;
        }

        FixPrefabs();
        var coinPrefabs = MakeCoinPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ---------- find groups ----------
        Transform env = Group("--- ENVIRONMENT ---");
        Transform col = Group("--- COLLECTIBLES ---");
        Transform haz = Group("--- HAZARDS ---");
        Transform ene = Group("--- ENEMIES ---");
        Transform goal = Group("--- LEVEL GOAL ---");
        Transform inter = Group("--- INTERACTABLES ---");

        ClearOld();

        // ---------- tiles ----------
        var tilemap = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Tilemap");
        if (tilemap == null) { Debug.LogError("No Tilemap called 'Tilemap' found."); return; }
        tilemap.ClearAllTiles();
        TileBase T(int i) => AssetDatabase.LoadAssetAtPath<TileBase>(TILES + i + ".asset");
        TileBase grassL = T(3), grassM = T(4), grassR = T(5), dirtL = T(6), dirtM = T(7), dirtR = T(8);

        int H = Map.Length;
        char At(int x, int y) // y = 0 bottom
        {
            if (y < 0 || y >= H) return '.';
            string row = Map[H - 1 - y];
            return (x < 0 || x >= row.Length) ? '.' : row[x];
        }

        var spikeFloor = new List<Vector3>();
        var enemies = new List<Vector2Int>();
        var patrolA = new List<Vector2Int>();
        var patrolB = new List<Vector2Int>();
        var bridge = new List<Vector2Int>();
        Vector2Int? player = null, flag = null, leverL = null, leverK = null, door = null, spikeSwitch = null;

        int width = Map.Max(r => r.Length);
        for (int y = 0; y < H; y++)
        for (int x = 0; x < width; x++)
        {
            char c = At(x, y);
            var cell = new Vector2Int(x, y);
            switch (c)
            {
                case '#':
                {
                    bool top = At(x, y + 1) != '#';
                    bool left = At(x - 1, y) != '#';
                    bool right = At(x + 1, y) != '#';
                    TileBase t = top ? (left && !right ? grassL : right && !left ? grassR : grassM)
                                     : (left && !right ? dirtL : right && !left ? dirtR : dirtM);
                    tilemap.SetTile(new Vector3Int(x + OX, y + OY, 0), t);
                    break;
                }
                case 'P': player = cell; break;
                case 'c': Place(coinPrefabs[0], col, Center(cell)); break;
                case 's': Place(coinPrefabs[1], col, Center(cell)); break;
                case 'G': Place(coinPrefabs[2], col, Center(cell)); break;
                case '^': Place(Load(P + "Spike.prefab"), haz, Bottom(cell, 0.4f)); break;
                case '!': spikeFloor.Add(Bottom(cell, 0.4f)); break;
                case 'E': enemies.Add(cell); break;
                case 'a': patrolA.Add(cell); break;
                case 'b': patrolB.Add(cell); break;
                case '=': Place(Load(P + "DisapperingPlatform.prefab"), env, Center(cell)); break;
                case '-': bridge.Add(cell); break;
                case 'L': leverL = cell; break;
                case 'K': leverK = cell; break;
                case 'D': door = cell; break;
                case 'S': spikeSwitch = cell; break;
                case 'F': flag = cell; break;
            }
        }

        // ---------- player ----------
        var playerObj = Object.FindFirstObjectByType<PlayerController>();
        if (playerObj != null && player.HasValue)
            playerObj.transform.position = Bottom(player.Value, 0.6f);
        Transform prompt = playerObj != null ? playerObj.transform.Find("InteractPrompt") : null;
        if (playerObj != null && prompt == null)
        {
            // scene instance did not pick up the prefab change yet - add the prompt directly
            var pr = new GameObject("InteractPrompt");
            pr.transform.SetParent(playerObj.transform, false);
            pr.transform.localPosition = new Vector3(0, 1.2f, 0);
            pr.transform.localScale = Vector3.one * 0.8f;
            var psr = pr.AddComponent<SpriteRenderer>();
            psr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "KeyE.png");
            psr.sortingOrder = 20;
            pr.AddComponent<InteractPrompt>();
            pr.SetActive(false);
            prompt = pr.transform;
        }

        // ---------- flag ----------
        if (flag.HasValue) Place(Load(P + "Flag.prefab"), goal, Bottom(flag.Value, 0.7f));

        // ---------- enemies + patrol points (siblings, not children) ----------
        var enemyPrefab = Load(P + "Enemy.prefab");
        int n = 0;
        foreach (var e in enemies)
        {
            n++;
            var groupGo = new GameObject(GEN + "Enemy Group " + n);
            groupGo.transform.SetParent(ene, false);
            groupGo.transform.position = Vector3.zero;
            var a = Nearest(patrolA, e); var b = Nearest(patrolB, e);
            var pa = new GameObject("Patrol Point A").transform; pa.SetParent(groupGo.transform); pa.position = Bottom(a, 0.5f);
            var pb = new GameObject("Patrol Point B").transform; pb.SetParent(groupGo.transform); pb.position = Bottom(b, 0.5f);
            var enemyGo = Place(enemyPrefab, groupGo.transform, Bottom(e, 0.5f));
            var enemy = enemyGo.GetComponent<Enemy>();
            var so = new SerializedObject(enemy);
            var arr = so.FindProperty("patrolPoints");
            arr.arraySize = 2;
            arr.GetArrayElementAtIndex(0).objectReferenceValue = pa;
            arr.GetArrayElementAtIndex(1).objectReferenceValue = pb;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- bridge (hidden until Lever_Bridge is used) ----------
        GameObject bridgeGo = null;
        if (bridge.Count > 0)
        {
            bridgeGo = new GameObject(GEN + "Bridge");
            bridgeGo.transform.SetParent(env, false);
            bridgeGo.transform.position = Vector3.zero;
            bridgeGo.layer = GroundLayer;
            var plank = SpriteFrom(TILESET_PNG, "tileset_sliced 16x16_1");
            foreach (var cell in bridge)
            {
                var piece = new GameObject("Bridge Tile");
                piece.layer = GroundLayer;
                piece.transform.SetParent(bridgeGo.transform);
                piece.transform.position = Center(cell);
                piece.AddComponent<SpriteRenderer>().sprite = plank;
                piece.AddComponent<BoxCollider2D>().size = Vector2.one;
            }
        }

        // ---------- door ----------
        GameObject doorGo = null;
        if (door.HasValue)
        {
            doorGo = new GameObject(GEN + "Door");
            doorGo.layer = GroundLayer;
            doorGo.transform.SetParent(env, false);
            doorGo.transform.position = new Vector3(door.Value.x + OX + 0.5f, door.Value.y + OY + DoorHeight / 2f, 0);
            var sr = doorGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "Door.png");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(1, DoorHeight);
            doorGo.AddComponent<BoxCollider2D>().size = new Vector2(1, DoorHeight);
        }

        // ---------- switchable spike floor ----------
        GameObject spikeFloorGo = null;
        if (spikeFloor.Count > 0)
        {
            spikeFloorGo = new GameObject(GEN + "Spike Floor");
            spikeFloorGo.transform.SetParent(haz, false);
            spikeFloorGo.transform.position = Vector3.zero;
            foreach (var pos in spikeFloor) Place(Load(P + "Spike.prefab"), spikeFloorGo.transform, pos);
        }

        // ---------- interactive objects (prefabs) ----------
        if (leverL.HasValue && bridgeGo != null)
            MakeInteractive("Lever_Bridge", ART + "Lever.png", inter, Bottom(leverL.Value, 0.5f), bridgeGo, false, prompt);
        if (leverK.HasValue && doorGo != null)
            MakeInteractive("Lever_Door", ART + "Lever.png", inter, Bottom(leverK.Value, 0.5f), doorGo, true, prompt);
        if (spikeSwitch.HasValue && spikeFloorGo != null)
            MakeInteractive("Switch_Spikes", ART + "SpikeSwitch.png", inter, Bottom(spikeSwitch.Value, 0.5f), spikeFloorGo, true, prompt);

        // ---------- kill box ----------
        var kill = GameObject.Find("KillBox");
        if (kill != null)
        {
            kill.transform.position = new Vector3(OX + width / 2f, OY - 4, kill.transform.position.z);
            kill.transform.localScale = new Vector3(width + 40, 1, 1);
        }

        // ---------- managers ----------
        var gm = Object.FindFirstObjectByType<GameMNGR>();
        if (gm != null && gm.GetComponent<HealthManager>() == null) gm.gameObject.AddComponent<HealthManager>();

        // ---------- camera rig (so CameraShake works with FollowCam) ----------
        SetupCamera(width, playerObj);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("FA2: Level 1 built. Enemies: " + enemies.Count + ", bridge tiles: " + bridge.Count);
    }

    // ================= helpers =================

    static Vector3 Center(Vector2Int c) => new Vector3(c.x + OX + 0.5f, c.y + OY + 0.5f, 0);
    static Vector3 Bottom(Vector2Int c, float up) => new Vector3(c.x + OX + 0.5f, c.y + OY + up, 0);
    static GameObject Load(string path) => AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static Vector2Int Nearest(List<Vector2Int> list, Vector2Int to)
    {
        if (list.Count == 0) return to;
        return list.OrderBy(p => Mathf.Abs(p.x - to.x) + Mathf.Abs(p.y - to.y) * 10).First();
    }

    static Transform Group(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        return go.transform;
    }

    static GameObject Place(GameObject prefab, Transform parent, Vector3 pos)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        return go;
    }

    static Sprite SpriteFrom(string png, string name) =>
        AssetDatabase.LoadAllAssetsAtPath(png).OfType<Sprite>().FirstOrDefault(s => s.name == name);

    static void ClearOld()
    {
        string[] prefabNames = { "Coin", "Coin_Bronze", "Coin_Silver", "Coin_Gold", "Enemy", "Spike", "DisapperingPlatform",
                                 "Box", "Flag", "Lever_Bridge", "Lever_Door", "Switch_Spikes" };
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (t == null) continue;
            var go = t.gameObject;
            bool isRootInstance = PrefabUtility.IsOutermostPrefabInstanceRoot(go);
            string src = isRootInstance ? PrefabUtility.GetCorrespondingObjectFromSource(go)?.name : null;
            if ((src != null && prefabNames.Contains(src)) || go.name.StartsWith(GEN) || go.name == "Patrol Point B")
                Object.DestroyImmediate(go);
        }
    }

    // Make the prefabs match FA2: Hazard spikes, enemy without child patrol point, flag -> LevelGoal.
    static void FixPrefabs()
    {
        // Spike -> Hazard (trigger)
        var spike = PrefabUtility.LoadPrefabContents(P + "Spike.prefab");
        foreach (var d in spike.GetComponents<DeathOnCollission>()) Object.DestroyImmediate(d);
        var hz = spike.GetComponent<Hazard>();
        if (hz == null) hz = spike.AddComponent<Hazard>();
        hz.damage = 1; hz.knockbackForce = 8;
        foreach (var c in spike.GetComponents<Collider2D>()) c.isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(spike, P + "Spike.prefab");
        PrefabUtility.UnloadPrefabContents(spike);

        // Enemy: remove the child patrol point (patrol points now live next to the enemy in the scene)
        var enemy = PrefabUtility.LoadPrefabContents(P + "Enemy.prefab");
        var child = enemy.transform.Find("Patrol Point A");
        if (child != null) Object.DestroyImmediate(child.gameObject);
        var so = new SerializedObject(enemy.GetComponent<Enemy>());
        so.FindProperty("patrolPoints").arraySize = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(enemy, P + "Enemy.prefab");
        PrefabUtility.UnloadPrefabContents(enemy);

        // Flag: WinOnCollission -> LevelGoal
        var flag = PrefabUtility.LoadPrefabContents(P + "Flag.prefab");
        foreach (var w in flag.GetComponents<WinOnCollission>()) Object.DestroyImmediate(w);
        if (flag.GetComponent<LevelGoal>() == null) flag.AddComponent<LevelGoal>();
        foreach (var c in flag.GetComponents<Collider2D>()) c.isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(flag, P + "Flag.prefab");
        PrefabUtility.UnloadPrefabContents(flag);

        // Disappearing platform: slower, fairer timing
        var plat = PrefabUtility.LoadPrefabContents(P + "DisapperingPlatform.prefab");
        var dp = plat.GetComponent<DisappearingPlatform>();
        if (dp != null) dp.disappearTime = 1.5f;
        plat.layer = GroundLayer;
        PrefabUtility.SaveAsPrefabAsset(plat, P + "DisapperingPlatform.prefab");
        PrefabUtility.UnloadPrefabContents(plat);

        // Player: add the "E" prompt above the head
        var player = PrefabUtility.LoadPrefabContents(P + "Player.prefab");
        if (player.transform.Find("InteractPrompt") == null)
        {
            var pr = new GameObject("InteractPrompt");
            pr.transform.SetParent(player.transform, false);
            pr.transform.localPosition = new Vector3(0, 1.2f, 0);
            pr.transform.localScale = Vector3.one * 0.8f;
            var sr = pr.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "KeyE.png");
            sr.sortingOrder = 20;
            pr.AddComponent<InteractPrompt>();
            pr.SetActive(false);
        }
        PrefabUtility.SaveAsPrefabAsset(player, P + "Player.prefab");
        PrefabUtility.UnloadPrefabContents(player);
    }

    // Bronze 5 / Silver 10 / Gold 25 coin prefab variants, each with a spin animation.
    static GameObject[] MakeCoinPrefabs()
    {
        string[] kind = { "Bronze", "Silver", "Gold" };
        int[] value = { 5, 10, 25 };
        var sprites = AssetDatabase.LoadAllAssetsAtPath(COINS_PNG).OfType<Sprite>().ToList();
        var result = new GameObject[3];
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Coins")) AssetDatabase.CreateFolder("Assets/Animations", "Coins");

        for (int i = 0; i < 3; i++)
        {
            string path = P + "Coin_" + kind[i] + ".prefab";
            var existing = Load(path);
            if (existing != null) { result[i] = existing; continue; }

            var frames = Enumerable.Range(0, 4).Select(f => sprites.FirstOrDefault(s => s.name == kind[i] + " " + f)).ToArray();

            var clip = new AnimationClip { frameRate = 10 };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int f = 0; f < frames.Length; f++) keys[f] = new ObjectReferenceKeyframe { time = f / 10f, value = frames[f] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / 10f, value = frames[0] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            string clipPath = "Assets/Animations/Coins/Coin_" + kind[i] + "_Spin.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Animations/Coins/Coin_" + kind[i] + ".controller", clip);

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(Load(P + "Coin.prefab"));
            inst.name = "Coin_" + kind[i];
            inst.GetComponent<SpriteRenderer>().sprite = frames[0];
            inst.GetComponent<Coin>().value = value[i];
            var anim = inst.GetComponent<Animator>();
            if (anim == null) anim = inst.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            result[i] = PrefabUtility.SaveAsPrefabAsset(inst, path);
            Object.DestroyImmediate(inst);
        }
        return result;
    }

    // Lever / switch: Switch + Interactable on a 3x3 trigger, saved as its own prefab.
    static void MakeInteractive(string name, string spritePath, Transform parent, Vector3 pos,
                                GameObject target, bool startActive, Transform prompt)
    {
        if (!AssetDatabase.IsValidFolder(P + "Interactive")) AssetDatabase.CreateFolder("Assets/Prefabs", "Interactive");
        string path = P + "Interactive/" + name + ".prefab";

        GameObject prefab = Load(path);
        if (prefab == null)
        {
            var go = new GameObject(name);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingOrder = 5;
            var trig = go.AddComponent<BoxCollider2D>();
            trig.isTrigger = true;
            trig.size = new Vector2(3, 3);
            go.AddComponent<Switch>();
            go.AddComponent<Interactable>();
            prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        var inst = Place(prefab, parent, pos);
        var sw = new SerializedObject(inst.GetComponent<Switch>());
        sw.FindProperty("hidingObject").objectReferenceValue = target;
        sw.FindProperty("isActive").boolValue = startActive;
        sw.ApplyModifiedPropertiesWithoutUndo();
        var ia = inst.GetComponent<Interactable>();
        if (prompt != null) { ia.interactUI = prompt.gameObject; EditorUtility.SetDirty(ia); }
        else Debug.LogWarning(name + ": Player has no InteractPrompt child - set Interact UI by hand.");
    }

    static void SetupCamera(int width, PlayerController player)
    {
        var cam = Camera.main;
        if (cam == null) return;
        var follow = cam.GetComponentInParent<FollowCam>();
        Transform rig;
        if (follow != null && follow.gameObject == cam.gameObject)
        {
            // move FollowCam from the camera onto a new parent rig
            var rigGo = new GameObject("CameraRig");
            rigGo.transform.SetParent(cam.transform.parent, false);
            rigGo.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0);
            var newFollow = rigGo.AddComponent<FollowCam>();
            newFollow.target = follow.target;
            Object.DestroyImmediate(follow);
            cam.transform.SetParent(rigGo.transform, true);
            follow = newFollow;
            rig = rigGo.transform;
        }
        else rig = follow != null ? follow.transform : null;

        cam.transform.localPosition = new Vector3(0, 0, -10);
        if (cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();

        if (follow != null)
        {
            if (player != null) follow.target = player.transform;
            // keep the player on screen even in narrow Game views
            follow.leftBound = OX + 6f;
            follow.rightBound = OX + width - 6f;
            follow.lowerBound = OY + 3.5f;
            follow.upperBound = OY + 6f;
        }
    }
}
