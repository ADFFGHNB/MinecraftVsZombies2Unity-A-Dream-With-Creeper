#nullable enable
using System.IO;
using System.Linq;
using System.Text;
using MVZ2Logic.Models;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 临时工具：测量“嵌入 prefab 里的帽子”和“运行时盔甲锚点”之间的偏移，
/// 结果打印到 Console 并写入 D:/ZCode/tmp_unity_gui/helmet_align.txt，
/// 用来填 models.xml 的 armorconfig offset。
/// 菜单：Tools/DWC/Measure Helmet Offsets
/// </summary>
public static class FootballHelmetAlign
{
    const string FootballPrefab = "Assets/GameContent/Assets/mvz2/models/enemy/prologue/football_zombie.prefab";
    const string DancingPrefab = "Assets/GameContent/Assets/mvz2/models/enemy/prologue/dancing_zombie.prefab";
    const string OutputPath = "D:/ZCode/tmp_unity_gui/helmet_align.txt";

    [MenuItem("Tools/DWC/Measure Helmet Offsets")]
    static void Measure()
    {
        var all = new StringBuilder();
        all.AppendLine(MeasureOne(FootballPrefab, "football_helmet", "FOOTBALL"));
        all.AppendLine(MeasureOne(DancingPrefab, "dance_hat", "DANCE"));
        Debug.Log(all.ToString());
        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
        File.WriteAllText(OutputPath, all.ToString());
        Debug.Log($"[DWC] 测量结果已写入 {OutputPath}");
    }

    static string MeasureOne(string prefabPath, string hatName, string tag)
    {
        var sb = new StringBuilder();
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            sb.AppendLine($"==== {tag} ====");

            // 1. 找盔甲锚点：优先 ModelAnchor.key == "armor"，兜底找名字是 Armor 的节点
            var anchors = root.GetComponentsInChildren<ModelAnchor>(true);
            sb.AppendLine($"  ModelAnchor keys: [{string.Join(",", anchors.Select(a => a.key ?? "null"))}]");
            var armorAnchor = anchors.FirstOrDefault(a => a.key == "armor");
            if (armorAnchor == null)
            {
                var byName = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "Armor" || t.name == "armor");
                sb.AppendLine($"  [警告] 没找到 key=armor 的锚点，按名字兜底: {(byName != null ? byName.name : "也没有")}");
                if (byName != null) armorAnchor = byName.GetComponent<ModelAnchor>();
            }

            // 2. 找嵌入的帽子节点：名字匹配且带 SpriteRenderer（自身或子代）
            Transform? hat = null;
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name != hatName) continue;
                if (FindRenderableUnder(tr) != null) { hat = tr; break; }
            }
            sb.AppendLine($"  嵌入帽子: {(hat != null ? GetPath(hat, root.transform) : "未找到")}");

            if (armorAnchor == null || hat == null)
            {
                sb.AppendLine("  [失败] 缺少锚点或帽子");
                return sb.ToString();
            }

            var anchorTr = armorAnchor.transform;
            var hatRenderTr = FindRenderableUnder(hat)!;

            sb.AppendLine($"  锚点节点: {anchorTr.name} 世界pos={Fmt(anchorTr.position)} 缩放={Fmt(anchorTr.lossyScale)}");
            sb.AppendLine($"  帽子渲染节点: {hatRenderTr.name} 世界pos={Fmt(hatRenderTr.position)} 缩放={Fmt(hatRenderTr.lossyScale)}");

            // 3. 运行时会把盔甲模型挂在锚点下，localPosition = 配置 offset × LawnToTransScale(0.01)
            //    所以写进 models.xml 的数值要用“草坪单位” = 变换单位 × 100
            var offset = anchorTr.InverseTransformPoint(hatRenderTr.position);
            var lawn = offset * 100f;
            sb.AppendLine($"  (变换单位) x={offset.x:F6} y={offset.y:F6} z={offset.z:F6}");
            sb.AppendLine($"  >>> OFFSET {tag} x={lawn.x:F4} y={lawn.y:F4} z={lawn.z:F4}   （草坪单位，直接填 XML）");
            return sb.ToString();
        }
        catch (System.Exception e)
        {
            sb.AppendLine($"  [异常] {e}");
            return sb.ToString();
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Transform? FindRenderableUnder(Transform t)
    {
        if (t.GetComponent<SpriteRenderer>() != null) return t;
        var sr = t.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault();
        return sr != null ? sr.transform : null;
    }

    static string Fmt(Vector3 v) => $"({v.x:F4},{v.y:F4},{v.z:F4})";

    static string GetPath(Transform t, Transform root)
    {
        var s = t.name;
        while (t.parent != null && t.parent != root)
        {
            t = t.parent;
            s = t.name + "/" + s;
        }
        return s;
    }
}
