using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// AI控制的单位
[RequireComponent(typeof(Motion))]
public class AIUnit : Unit
{
    [Header("AI参数")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float followDistance = 3f;
    public float followDelay = 0.5f;  // 跟随延迟
    public float fleeThreshold = 3f;  // 被多少敌人攻击时逃跑
    
    [Header("巡逻参数")]
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;
    
    private Unit currentTarget;
    private float attackTimer = 0f;
    private float chaseTimer = 0f;
    private float fleeTimer = 0f;
    private float followTimer = 0f;
    private Vector3? formationPosition = null;
    private float patrolTimer = 0f;
    private Vector3 patrolDestination;


    // 初始化
	public override void Initialize(Unit leader)
	{
		base.Initialize(leader);
		CalculateFormationPosition();
	}
    
    // 计算在阵型中的位置
    private void CalculateFormationPosition()
    {
        if (leader == null) return;
        
        // 获取所有同阵营单位
        List<Unit> sameFactionUnits = CommandCenter.GetUnitsInFaction(faction);
        
        // 计算自己在列表中的索引
        int index = sameFactionUnits.IndexOf(this);
        if (index <= 0) return;  // 首领不需要计算阵型位置
        
        // 简单的三角阵型计算
        int row = Mathf.FloorToInt(Mathf.Sqrt(index));
        int col = index - row * row;
        
        // 计算相对位置
        float spacing = 1.5f;
        Vector3 relativePos = new Vector3(
            (col - row) * spacing,
            0,
            -row * spacing - followDistance
        );
        
        // 阵型位置相对于首领后方
        formationPosition = leader.transform.position + 
            leader.transform.rotation * relativePos;
    }
    
    // 更新跟随状态
    protected override void UpdateFollowingState()
    {
        // 检查首领是否死亡
        if (leader == null || leader.currentState == UnitState.Dead)
        {
            currentState = UnitState.Patrolling;
            return;
        }
        
        // 定期更新阵型位置
        if (Random.value < 0.05f)  // 5%的概率每帧更新
        {
            CalculateFormationPosition();
        }
        
        if (formationPosition.HasValue)
        {
            // 应用跟随延迟
            followTimer += Time.deltaTime;
            if (followTimer >= followDelay)
            {
                followTimer = 0f;
                
                // 设置目的地为阵型中的位置
                SetDestination(formationPosition.Value);
                MoveToDestination();
            }
        }
    }
    
    // 更新战斗状态
    protected override void UpdateCombatState()
    {
        // 检查是否需要逃跑
        if (attackersCount >= fleeThreshold && Random.value < 0.1f)  // 10%的概率触发逃跑
        {
            currentState = UnitState.Fleeing;
            return;
        }
        
        // 攻击冷却
        attackTimer += Time.deltaTime;
        
        // 如果没有目标，寻找新目标
        if (currentTarget == null || currentTarget.currentState == UnitState.Dead)
        {
            FindNewTarget();
            return;
        }
        
        // 计算到目标的距离
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

		// 如果在攻击范围内且冷却完毕
		if (distanceToTarget <= attackRange && attackTimer >= attackCooldown)
		{
			AttackTarget();
			attackTimer = 0f;
			fleeTimer = Time.time;
        }
		// 否则向目标移动
		else if (distanceToTarget > attackRange * 0.8f)  // 稍微靠近一点再攻击
		{
			if (Time.time > chaseTimer + 3)
			{
				chaseTimer = Time.time;
				SetDestination(currentTarget.transform.position);
				MoveToDestination();
			}

		}
		else
		{
			// 在攻击范围内但冷却未完成，稍微拉开距离
			if (Time.time > fleeTimer + 2)
			{
				fleeTimer = Time.time;
				Vector3 awayDirection = (transform.position - currentTarget.transform.position).normalized;
				SetDestination(transform.position + awayDirection * attackRange * 2.5f);
				MoveToDestination(true);
			}

		}
    }
    
    // 更新逃跑状态
    protected override void UpdateFleeingState()
    {
        // 向远离敌人的方向逃跑
        if (currentTarget != null)
        {
            Vector3 fleeDirection = (transform.position - currentTarget.transform.position).normalized;
            SetDestination(transform.position + fleeDirection * 10f);
            MoveToDestination();
        }
        
        // 一段时间后恢复战斗状态
        if (Random.value < 0.02f)  // 约5秒后有10%的概率恢复
        {
            currentState = UnitState.Combat;
            FindNewTarget();
        }
    }
    
    // 更新巡逻状态
    protected override void UpdatePatrollingState()
    {
        // 如果巡逻目的地为空或者到达了目的地，生成新的巡逻点
        if (!destination.HasValue || 
            Vector3.Distance(transform.position, destination.Value) < 0.5f)
        {
            // 生成新的巡逻点
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                patrolTimer = 0f;
                Vector3 randomOffset = Random.insideUnitSphere * patrolRadius;
                randomOffset.y = 0; // 保持在地面上
                SetDestination(transform.position + randomOffset);
            }
        }
        else
        {
            // 向巡逻点移动
            MoveToDestination();
        }
    }
    
    // 寻找新目标
    public void FindNewTarget()
    {
        // 获取所有敌人单位
        List<Unit> enemyUnits = CommandCenter.GetUnitsInFaction(
            faction == Faction.Player ? Faction.Enemy : Faction.Player
        );
        
        // 优先选择无人攻击的目标
        Unit bestTarget = null;
        float shortestDistance = Mathf.Infinity;
        
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null || enemy.currentState == UnitState.Dead) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            // 检查敌人是否已经被攻击
            //AIUnit enemyAI = enemy as AIUnit;
            bool isBeingAttacked =  enemy.attackersCount > 0;
            
            // 优先选择未被攻击的敌人，或者距离最近的敌人
            if ((!isBeingAttacked && distance < shortestDistance) ||
                (isBeingAttacked && bestTarget == null && distance < detectionRange))
            {
                shortestDistance = distance;
                bestTarget = enemy;
            }
        }
        
        currentTarget = bestTarget;
        
        // 如果找到了目标，增加攻击者计数
        if (currentTarget != null)
        {
			currentTarget.attackersCount++;
        }
    }
    
    // 攻击目标
    private void AttackTarget()
    {
        if (currentTarget == null) return;
        
        // 转向目标
        RotateTowards(currentTarget.transform.position);
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 攻击判定会由武器上的Attacker组件处理
    }
    
    // 单位被攻击时调用
    public override void OnAttacked(Vector3 hitPos)
    {
		base.OnAttacked(hitPos);

        attackersCount++;
        
        // 如果不在战斗状态，切换到战斗状态
        if (currentState == UnitState.Following)
        {
            currentState = UnitState.Combat;
        }
        
        // 一段时间后减少攻击者计数
        Invoke(nameof(DecreaseAttackersCount), 5f);
    } 

    
    // 重写死亡方法
    public override void Die()
    {
        // 从攻击者计数中移除自己
        if (currentTarget != null)
        {
            AIUnit targetAI = currentTarget as AIUnit;
            if (targetAI != null)
            {
                targetAI.DecreaseAttackersCount();
            }
        }
        
        base.Die();
    }
}