using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TerrainTreeHeightAdjuster : MonoBehaviour
{
    [Tooltip("要调整的地形")]
    public Terrain targetTerrain;

    [Tooltip("高度调整模式：是设置固定高度还是按比例调整")]
    public AdjustMode adjustMode = AdjustMode.Scale;

    [Tooltip("当模式为固定高度时生效，设置树木的固定高度")]
    public float targetHeight = 5f;

    [Tooltip("当模式为比例调整时生效，高度调整的比例系数")]
    [Range(0.1f, 5f)]
    public float scaleFactor = 1f;

    [Tooltip("是否只调整选中范围内的树木")]
    public bool useRange = false;

    [Tooltip("调整范围的中心点")]
    public Vector3 rangeCenter;

    [Tooltip("调整范围的半径")]
    public float rangeRadius = 10f;

    [Tooltip("目标树木原型索引(-1则对全部生效)")]
    public int targetPrototypeIndex = -1;


    public enum AdjustMode
    {
        SetFixedHeight,  // 设置固定高度
        Scale            // 按比例调整
    }

    /// <summary>
    /// 调整所有树木的高度
    /// </summary>
    [ContextMenu("调整树木高度")]
    public void AdjustTreeHeights()
    {
        if (targetTerrain == null)
        {
            Debug.LogError("请指定要调整的地形！");
            return;
        }

        TerrainData terrainData = targetTerrain.terrainData;
        TreeInstance[] trees = terrainData.treeInstances;

        if (trees == null || trees.Length == 0)
        {
            Debug.LogWarning("地形上没有树木可调整！");
            return;
        }

        List<TreeInstance> modifiedTrees = new List<TreeInstance>(trees);
        int adjustedCount = 0;

        for (int i = 0; i < modifiedTrees.Count; i++)
        {
            TreeInstance tree = modifiedTrees[i];

            if(targetPrototypeIndex != -1 && tree.prototypeIndex != targetPrototypeIndex)
                continue;

            // 将树的位置从0-1范围转换为世界坐标
            Vector3 treeWorldPos = Vector3.Scale(tree.position, terrainData.size) + targetTerrain.transform.position;
            
            // 如果使用范围且树木不在范围内，则跳过
            if (useRange && Vector3.Distance(treeWorldPos, rangeCenter) > rangeRadius)
            {
                continue;
            }

            // 获取树原型的实际高度
            float prototypeHeight = GetTreePrototypeHeight(terrainData, tree.prototypeIndex);
            if (prototypeHeight <= 0)
            {
                Debug.LogWarning($"无法获取树原型 {tree.prototypeIndex} 的高度，已跳过");
                continue;
            }

            // 根据模式调整高度
            if (adjustMode == AdjustMode.SetFixedHeight)
            {
                // 设置固定高度
                tree.heightScale = targetHeight / prototypeHeight;
            }
            else
            {
                // 按比例调整
                tree.heightScale *= scaleFactor;
            }

            // 确保高度缩放值在合理范围内
            tree.heightScale = Mathf.Clamp(tree.heightScale, 0.01f, 10f);
            
            modifiedTrees[i] = tree;
            adjustedCount++;
        }

        // 应用修改
        terrainData.treeInstances = modifiedTrees.ToArray();
        Debug.Log($"已调整 {adjustedCount} 棵树的高度");
    }

    /// <summary>
    /// 获取树原型的高度
    /// </summary>
    private float GetTreePrototypeHeight(TerrainData terrainData, int prototypeIndex)
    {
        if (prototypeIndex < 0 || prototypeIndex >= terrainData.treePrototypes.Length)
            return 0;

        TreePrototype prototype = terrainData.treePrototypes[prototypeIndex];
        
        // 尝试从预制体获取高度
        if (prototype.prefab != null)
        {
            // 方法1: 检查是否有Collider并获取高度
            Collider collider = prototype.prefab.GetComponent<Collider>();
            if (collider != null)
            {
                if (collider is CapsuleCollider capsule)
                    return capsule.height;
                if (collider is BoxCollider box)
                    return box.size.y;
            }
            
            // 方法2: 检查渲染器的边界
            Renderer renderer = prototype.prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size.y;
            }
        }
        
        // 如果无法获取实际高度，使用默认值
        return 5f;
    }

    /// <summary>
    /// 重置所有树木高度
    /// </summary>
    //[ContextMenu("重置树木高度")]
    public void ResetTreeHeights()
    {
        if (targetTerrain == null)
        {
            Debug.LogError("请指定要调整的地形！");
            return;
        }

        TerrainData terrainData = targetTerrain.terrainData;
        TreeInstance[] trees = terrainData.treeInstances;

        if (trees == null || trees.Length == 0)
        {
            Debug.LogWarning("地形上没有树木可重置！");
            return;
        }

        for (int i = 0; i < trees.Length; i++)
        {
            TreeInstance tree = trees[i];
            tree.heightScale = 1f; // 重置为原始高度
            trees[i] = tree;
        }

        terrainData.treeInstances = trees;
        Debug.Log($"已重置 {trees.Length} 棵树的高度");
    }

    void OnDrawGizmosSelected()
    {
        if (useRange)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(rangeCenter, rangeRadius);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rangeCenter, rangeRadius);
        }
    }
}
