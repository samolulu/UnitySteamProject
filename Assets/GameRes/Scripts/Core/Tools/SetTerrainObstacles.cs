using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetTerrainObstacles : MonoBehaviour
{
    TreeInstance[] obstacleTrees;
    Terrain terrain;
    float terrainWidth;
    float terrainLength;
    float terrainHeight;
    bool isError;
    [ContextMenu("Gen Terrain Obstacles")]
    public void GenTerrainObstacles()
    {
 
        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("没有找到激活的地形！");
            return;
        }

        obstacleTrees = GetTreesWithColliders();
        if (obstacleTrees.Length == 0)
        {
            Debug.LogWarning("没有找到带有碰撞体的树木原型！");
            return;
        }

        // 获取地形尺寸
        terrainWidth = terrain.terrainData.size.x;
        terrainLength = terrain.terrainData.size.z;
        terrainHeight = terrain.terrainData.size.y;
        Debug.Log($"地形尺寸: {terrainWidth} x {terrainHeight} x {terrainLength}");

        // 创建父物体管理所有障碍物
        GameObject parent = new GameObject("Tree_Obstacles");
        parent.transform.position = terrain.transform.position; // 与地形位置对齐

        Debug.Log($"开始生成 {obstacleTrees.Length} 个障碍物实体...");

        int index = 0;
        foreach (TreeInstance tree in obstacleTrees)
        {
            // 计算树木在世界空间中的位置
            Vector3 worldPos = new Vector3(
                tree.position.x * terrainWidth + terrain.transform.position.x,
                tree.position.y * terrainHeight + terrain.transform.position.y,
                tree.position.z * terrainLength + terrain.transform.position.z
            );

            // 计算树木旋转（仅Y轴旋转）
            Quaternion worldRot = Quaternion.AngleAxis(tree.rotation * Mathf.Rad2Deg, Vector3.up);

            // 获取树木原型的碰撞体信息
            var treePrototype = terrain.terrainData.treePrototypes[tree.prototypeIndex];
            Collider prototypeCollider = treePrototype.prefab.GetComponent<Collider>();

            if (prototypeCollider == null)
            {
                isError = true;
                Debug.LogError($"树木原型 {treePrototype.prefab.name} 没有碰撞体组件！");
                break;
            }

            // 根据碰撞体类型生成对应实体
            GameObject obstacle;
            if (prototypeCollider is CapsuleCollider capsuleColl)
            {
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ConfigureCapsule(obstacle, capsuleColl, treePrototype.prefab.transform.lossyScale);
            }
            else if (prototypeCollider is BoxCollider boxColl)
            {
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ConfigureBox(obstacle, boxColl, treePrototype.prefab.transform.lossyScale);
            }
            else
            {
                isError = true;
                Debug.LogError($"树木原型 {treePrototype.prefab.name} 使用了不支持的碰撞体类型 ({prototypeCollider.GetType().Name})，仅支持Capsule和Box！");
                break;
            }

            // 设置障碍物的位置、旋转和父物体
            obstacle.name = $"Obstacle_{index}_{treePrototype.prefab.name}";
            obstacle.transform.SetParent(parent.transform);
            obstacle.transform.position = worldPos;
            obstacle.transform.rotation = worldRot;
            
            // 可选：添加NavMeshObstacle使其影响导航
           // AddNavMeshObstacle(obstacle, prototypeCollider);

            index++;
        }

        if (!isError)
        {
            Debug.Log($"成功生成 {index} 个障碍物实体！");
        }
    }

    // 配置胶囊体参数（考虑原始模型的缩放）
    void ConfigureCapsule(GameObject capsule, CapsuleCollider prototypeCollider, Vector3 prototypeScale)
    {
        // 应用原型的缩放影响
        capsule.transform.localScale = new Vector3(prototypeScale.x * prototypeCollider.radius,
                                                prototypeScale.y * prototypeCollider.height,
                                                prototypeScale.z * prototypeCollider.radius);
        
        // 设置碰撞体参数
        //CapsuleCollider collider = capsule.GetComponent<CapsuleCollider>();
        //collider.direction = prototypeCollider.direction;
    }

    // 配置立方体参数（考虑原始模型的缩放）
    void ConfigureBox(GameObject cube, BoxCollider prototypeCollider, Vector3 prototypeScale)
    {
        // 应用原型的缩放影响
        cube.transform.localScale = new Vector3(prototypeScale.x * prototypeCollider.size.x,
                                                prototypeScale.y * prototypeCollider.size.y,
                                                prototypeScale.z * prototypeCollider.size.z);

    }

    // 添加导航障碍物组件
    void AddNavMeshObstacle(GameObject obstacle, Collider prototypeCollider)
    {
        NavMeshObstacle navObstacle = obstacle.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true; // 允许切割导航网格
        navObstacle.carveOnlyStationary = true; // 仅静态时切割

        // 根据碰撞体类型设置导航障碍物形状
        if (prototypeCollider is CapsuleCollider capsule)
        {
            navObstacle.shape = NavMeshObstacleShape.Capsule;
            navObstacle.radius = capsule.radius;
            navObstacle.height = capsule.height;
            navObstacle.center = capsule.center;
        }
        else if (prototypeCollider is BoxCollider box)
        {
            navObstacle.shape = NavMeshObstacleShape.Box;
            navObstacle.size = box.size;
            navObstacle.center = box.center;
        }
    }

    // 获取带有碰撞体的树木实例
    TreeInstance[] GetTreesWithColliders()
    {
        List<TreeInstance> validTrees = new List<TreeInstance>();
        TreeInstance[] allTrees = terrain.terrainData.treeInstances;

        foreach (var tree in allTrees)
        {
            var prototype = terrain.terrainData.treePrototypes[tree.prototypeIndex];
            if (prototype.prefab.GetComponentInChildren<Collider>() != null)
            {
                validTrees.Add(tree);
            }
        }

        return validTrees.ToArray();
    }
}
