using UnityEngine;

// 阵营枚举
public enum Faction { Player, Enemy }

// 单位状态机
public enum UnitState { 
    Following,      // 跟随首领
    Combat,         // 战斗状态
    Fleeing,        // 逃跑
    Patrolling,     // 巡逻
    Dead            // 死亡
}

// 基础单位类 - 所有单位都继承此类
public abstract class Unit : MonoBehaviour
{
	[Header("基础属性")]
	public Faction faction;
	public float maxHealth = 100f;
	public float currentHealth;
	public LayerMask enemyLayers;

	// [Header("移动参数")]
	// public string speedParamName = "Speed";
	// public float rotationSpeed = 5f;
	// public float moveSpeed = 1.5f;
	// public float stoppingDistance = 1.0f;

	protected Animator animator;
	protected Motion motion;
	protected EffectHandler effectHandler;
	public UnitState currentState = UnitState.Following;
	protected Unit leader;
	protected Vector3? destination = null;
    public int attackersCount = 0;  // 有多少敌人正在攻击我
 
	// 初始化单位
	public virtual void Initialize(Unit leader)
	{
		this.leader = leader;
		animator = GetComponent<Animator>();
		motion = GetComponent<Motion>();
		effectHandler = GetComponent<EffectHandler>();
		currentHealth = maxHealth;
	}
    
    // 减少攻击者计数
    protected void DecreaseAttackersCount()
    {
        if (attackersCount > 0)
        {
            attackersCount--;
        }
    }
	
	// 接收伤害
	public virtual void TakeDamage(float damage)
	{
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			Die();
		}
		effectHandler.PlayDamage(damage);
	}

	public virtual void OnAttacked(Vector3 hitPos)
	{
		animator.SetTrigger("Hurt");

		effectHandler.PlayAttacked(hitPos);
	}

	// 死亡逻辑
	public virtual void Die()
	{
		currentState = UnitState.Dead;
		// 禁用碰撞体和移动
		GetComponent<Collider>().enabled = false;
		enabled = false;
		// 播放死亡动画
		if (animator != null)
		{
			animator.SetTrigger("Die");
		}

		effectHandler.PlayDie();
		Destroy(gameObject, 2);
	}
 

	// 设置目的地
	public void SetDestination(Vector3 destination)
	{
		this.destination = destination;
	}

	// 转向目标
	protected void RotateTowards(Vector3 targetPosition)
	{
		Vector3 direction = (targetPosition - transform.position).normalized;
		if (direction != Vector3.zero)
		{
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				lookRotation,
				Time.deltaTime * motion.rotationSpeed
			);
		}
	}


	// 移动到目的地
	protected void MoveToDestination(bool back = false)
	{
		if (!destination.HasValue) return;

		//RotateTowards(destination.Value);

		motion.SetTargetPosition(destination.Value, back);
	}

	// 每帧更新
	protected virtual void Update()
	{
		if (currentState == UnitState.Dead) return;

		switch (currentState)
		{
			case UnitState.Following:
				UpdateFollowingState();
				break;
			case UnitState.Combat:
				UpdateCombatState();
				break;
			case UnitState.Fleeing:
				UpdateFleeingState();
				break;
			case UnitState.Patrolling:
				UpdatePatrollingState();
				break;
		}
	}

	// 各状态的更新方法
	protected abstract void UpdateFollowingState();
	protected abstract void UpdateCombatState();
	protected abstract void UpdateFleeingState();
	protected abstract void UpdatePatrollingState();
}