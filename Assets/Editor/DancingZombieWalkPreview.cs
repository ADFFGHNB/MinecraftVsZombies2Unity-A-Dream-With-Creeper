// Temporary preview helper: plays the dancing zombie's walk pose in Edit mode
// inside the prefab stage by sampling the walk-related clips via AnimationMode.
// Menu: Tools/DancingZombiePreview/Play Walk  /  Stop
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class DancingZombieWalkPreview
{
    const string LegsClipPath = "Assets/Animation/Entities/Enemies/dancing_zombie/legs/dancing_zombie_legs_walk.anim";
    const string ArmClipPath = "Assets/Animation/Entities/Enemies/dancing_zombie/arms/dancing_zombie_arm_idle.anim";
    const string BaseClipPath = "Assets/Animation/Entities/Enemies/dancing_zombie/base/dancing_zombie_alive.anim";

    static AnimationClip[] clips;
    static Animator target;
    static double startTime;
    static bool playing;

    [MenuItem("Tools/DancingZombiePreview/Play Walk")]
    static void Play()
    {
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
        {
            Debug.LogError("[DancingZombiePreview] 请先打开 dancing_zombie.prefab 的 Prefab 模式");
            return;
        }
        target = stage.prefabContentsRoot.GetComponentInChildren<Animator>(true);
        if (target == null)
        {
            Debug.LogError("[DancingZombiePreview] 没找到 Animator");
            return;
        }
        clips = new[]
        {
            AssetDatabase.LoadAssetAtPath<AnimationClip>(LegsClipPath),
            AssetDatabase.LoadAssetAtPath<AnimationClip>(ArmClipPath),
            AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath),
        };
        if (!AnimationMode.InAnimationMode())
            AnimationMode.StartAnimationMode();
        startTime = EditorApplication.timeSinceStartup;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        playing = true;
        Debug.Log("[DancingZombiePreview] 行走预览开始，菜单 Tools/DancingZombiePreview/Stop 可停止");
    }

    [MenuItem("Tools/DancingZombiePreview/Stop")]
    static void Stop()
    {
        EditorApplication.update -= Tick;
        AnimationMode.StopAnimationMode();
        playing = false;
        Debug.Log("[DancingZombiePreview] 预览已停止");
    }

    const string PrefabPath = "Assets/GameContent/Assets/mvz2/models/enemy/prologue/dancing_zombie.prefab";

    [MenuItem("Tools/DancingZombiePreview/Open Prefab")]
    static void OpenPrefab()
    {
        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        Debug.Log("[DancingZombiePreview] 已打开 dancing_zombie.prefab");
    }

    static void Tick()
    {
        if (!playing || target == null)
            return;
        double t = EditorApplication.timeSinceStartup - startTime;
        AnimationMode.BeginSampling();
        foreach (var clip in clips)
        {
            if (clip == null) continue;
            AnimationMode.SampleAnimationClip(target.gameObject, clip, (float)(t % clip.length));
        }
        AnimationMode.EndSampling();
    }
}