using UnityEngine;


/// <summary>
/// 地表类型
/// </summary>
public enum TerrainSurfaceType
{
    Flatland, // 平原
    Grassland, // 草原
    Mountains, // 山地
    Forest, // 森林
    Desert, // 沙漠  
    Water, // 水域  
    Road, // 道路  
    Unknown
}


/// <summary>
/// 地形区域类型检测器
/// 用于检测某个坐标位置下的地形类型;后边需拓展支持多terrain,水域等
/// </summary>
public class TerrainTypeDetector : MonoBehaviour
{
    public Terrain targetTerrain;

    [Range(0, 5)]  [Tooltip("采样半径(1时相当于周围九宫格范围)")]
    public int sampleRadius = 1;   // 采样半径(1时相当于周围九宫格范围),单位像素,用于平滑地形类型

    /// <summary>
    /// 地形纹理图层到地表类型的映射关系; 水域通过其他方式判断(射线)
    /// </summary>
    public TerrainSurfaceType[] terrainLayer2SurfaceType = {
        TerrainSurfaceType.Grassland,
        TerrainSurfaceType.Grassland,
        TerrainSurfaceType.Grassland,
        TerrainSurfaceType.Road,
        TerrainSurfaceType.Road,
        TerrainSurfaceType.Mountains,
        TerrainSurfaceType.Flatland,
        TerrainSurfaceType.Flatland,
        TerrainSurfaceType.Road,
        TerrainSurfaceType.Road,
        TerrainSurfaceType.Flatland,
        TerrainSurfaceType.Flatland,
        TerrainSurfaceType.Flatland,
        TerrainSurfaceType.Forest,
        TerrainSurfaceType.Forest,
        TerrainSurfaceType.Forest,
        TerrainSurfaceType.Forest,
        TerrainSurfaceType.Desert,
        TerrainSurfaceType.Desert,
        TerrainSurfaceType.Mountains,
        TerrainSurfaceType.Mountains,
        TerrainSurfaceType.Mountains,
        TerrainSurfaceType.Mountains,
        TerrainSurfaceType.Mountains,
    };

    [SerializeField] private TerrainSurfaceType currentTerrainType = TerrainSurfaceType.Unknown;
    private TerrainData terrainData;
    private int alphamapResolution;
    private int layerCount;

    void Start()
    {
        if (targetTerrain == null)
        {
            targetTerrain = Terrain.activeTerrain;
        }

        terrainData = targetTerrain.terrainData;
        alphamapResolution = terrainData.alphamapResolution;
        layerCount = terrainData.alphamapLayers;
    }

    void Update()
    {
        /// 每帧更新当前地形类型;后面绑定单位后优化为单位移动时才更新
        currentTerrainType = GetCurrentTerrainType(transform.position);
    }

    public TerrainSurfaceType GetCurrentTerrainType(Vector3 targetPos)
    {
        Vector3 terrainPosition = targetTerrain.transform.position;

        // 计算相对于地形的坐标比例
        float x = (targetPos.x - terrainPosition.x) / terrainData.size.x;
        float z = (targetPos.z - terrainPosition.z) / terrainData.size.z;

        // 计算在alphamap中的像素坐标
        int alphamapX = Mathf.Clamp((int)(x * alphamapResolution), 0, alphamapResolution - 1);
        int alphamapZ = Mathf.Clamp((int)(z * alphamapResolution), 0, alphamapResolution - 1);

        // 获取该位置的纹理图层权重
        int L = sampleRadius * 2 + 1; //计算采样区域正方形边长
        //GetAlphamaps获取区域移动(-sampleRadius,-sampleRadius)令区域中心点处于(alphamapX,alphamapZ)
        float[,,] alphamaps = terrainData.GetAlphamaps(alphamapX- sampleRadius, alphamapZ - sampleRadius, L, L);
        // alphamaps[0, 0, i] gives the weight for layer i at the specified position

        float[] layerWeightSum = new float[layerCount];
        for (int i = 0; i < layerCount; i++)
        {
            layerWeightSum[i] = 0;
        }
        for (int i = 0; i < layerCount; i++)
        {
            for (int xOffset = -sampleRadius; xOffset <= sampleRadius; xOffset++)
            {
                for (int zOffset = -sampleRadius; zOffset <= sampleRadius; zOffset++)
                {
                    //离目标点越远权重越低
                    int dis =  Mathf.Max(Mathf.Abs(xOffset), Mathf.Abs(zOffset)) ;
                    float weightScale = L - dis;
                    layerWeightSum[i] += alphamaps[xOffset + sampleRadius, zOffset + sampleRadius, i] * weightScale;
                }
            }
        }

        // 找到权重最高的图层
        int maxIndex = 0;
        float maxWeight = 0;
        for (int i = 0; i < layerCount; i++)
        {
            float weight = layerWeightSum[i];
            if (weight > maxWeight)
            {
                maxWeight = weight;
                maxIndex = i;
            }
        }

        // 返回对应的地形类型
        if (maxIndex < terrainLayer2SurfaceType.Length)
        {
            return terrainLayer2SurfaceType[maxIndex];
        }

        return TerrainSurfaceType.Unknown;
    }


}