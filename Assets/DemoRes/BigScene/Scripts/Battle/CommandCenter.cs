using UnityEngine;
using System.Collections.Generic;

// 指挥中心 - 负责单位创建和战斗状态管理
public class CommandCenter : MonoBehaviour
{
    [Header("单位创建")]
    public Unit unitPrefab;
    public Transform unitRoot;
    public int initialUnits = 10;
    public float unitSpawnDelay = 0.5f;
    public float unitSpacing = 5.0f;
    
    [Header("战斗参数")]
    public float combatDetectionRange = 20f;
    public LayerMask enemyLayers;
    
    private Unit leader;
    private List<Unit> units = new List<Unit>();
    private bool isInCombat = false;
    private Unit enemyCommander;
    
    // 所有指挥中心的静态列表
    private static List<CommandCenter> allCommandCenters = new List<CommandCenter>();
    
    void Start()
    {
        leader = GetComponent<Unit>();
        allCommandCenters.Add(this);
        leader.Initialize(leader);

        // 初始化时创建单位
        InvokeRepeating(nameof(SpawnUnit), 1f, unitSpawnDelay);
    }
    
    void OnDestroy()
    {
        allCommandCenters.Remove(this);
    }
    
    // 创建单位
    void SpawnUnit()
    {
        if (units.Count >= initialUnits)
        {
            CancelInvoke(nameof(SpawnUnit));
            return;
        }
        
        // 计算生成位置（在首领后方的三角阵型）
        int index = units.Count + 1;
        int row = Mathf.FloorToInt(Mathf.Sqrt(index));
        int col = index - row * row;
 
        Vector3 spawnPosition = transform.position + 
            transform.rotation * new Vector3(
                (col - row) * unitSpacing,
                0,
                -row * unitSpacing - 2f
            );
        
        // 实例化单位
        Unit newUnit = Instantiate(unitPrefab, spawnPosition, transform.rotation, unitRoot);
        newUnit.faction = leader.faction;
        newUnit.Initialize(leader);
        
        units.Add(newUnit);
    }
    
    void Update()
    {
        // 检测是否与敌方指挥中心相遇
        DetectEnemyCommander();
        
        // 更新所有单位的状态
        UpdateUnitsState();
    }
    
    // 检测敌方指挥中心
    void DetectEnemyCommander()
    {
        // 查找敌方指挥中心
        foreach (CommandCenter commander in allCommandCenters)
        {
            if (commander == this || commander.leader == null || 
                commander.leader.faction == leader.faction)
            {
                continue;
            }
            
            // 检查距离
            float distance = Vector3.Distance(transform.position, commander.transform.position);
            if (distance <= combatDetectionRange)
            {
                enemyCommander = commander.leader;
                isInCombat = true;
                return;
            }
        }
        
        // 如果之前在战斗状态但现在找不到敌人，退出战斗状态
        if (isInCombat && enemyCommander == null)
        {
            isInCombat = false;
        }
    }
    
    // 更新所有单位的状态
    void UpdateUnitsState()
    {
        foreach (Unit unit in units)
        {
            if (unit == null || unit.currentState == UnitState.Dead) continue;
            
            // 如果指挥中心进入战斗状态，且单位不在战斗状态，切换到战斗状态
            if (isInCombat && unit.currentState != UnitState.Combat && 
                unit.currentState != UnitState.Dead)
            {
                if (unit is AIUnit aiUnit)
                {
                    aiUnit.currentState = UnitState.Combat;
                    aiUnit.FindNewTarget();
                }
            }
            // 如果指挥中心退出战斗状态，且单位在战斗状态，切换回跟随状态
            else if (!isInCombat && unit.currentState == UnitState.Combat)
            {
                unit.currentState = UnitState.Following;
            }
        }
    }
    
    // 获取特定阵营的所有单位
    public static List<Unit> GetUnitsInFaction(Faction faction)
    {
        List<Unit> result = new List<Unit>();
        
        foreach (CommandCenter commander in allCommandCenters)
        {
            if (commander.leader != null && commander.leader.faction == faction)
            {
                // 添加指挥中心的首领
                result.Add(commander.leader);
                
                // 添加指挥中心的所有单位
                foreach (Unit unit in commander.units)
                {
                    if (unit != null && unit.currentState != UnitState.Dead)
                    {
                        result.Add(unit);
                    }
                }
            }
        }
        
        return result;
    }
}