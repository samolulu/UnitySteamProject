using UnityEngine;

// 玩家单位 - 由玩家控制的首领
public class PlayerUnit : Unit
{
 
    
    void Start()
    {
 
    }
    
    protected override void UpdateFollowingState()
    {
        
    }
    
    // 其他状态更新方法
    protected override void UpdateCombatState()
    {
        // 玩家战斗状态可以实现特殊的战斗控制
        UpdateFollowingState(); // 简化实现，战斗时仍可自由移动
    }
    
    protected override void UpdateFleeingState()
    {
        // 玩家一般不会逃跑
        currentState = UnitState.Following;
    }
    
    protected override void UpdatePatrollingState()
    {
        // 玩家不需要巡逻
        currentState = UnitState.Following;
    }
}