using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 3f;
    public float chaseRange = 5f;
    public float attackRange = 1.5f;

    [Header("状态")]
    public bool isChasing = false;
    public bool isAttacking = false;
    public bool isIdling = false;
    public float attackCooldown = 2f;

    private Transform player;
    //private Rigidbody rb;
    private Animator animator;
    private NPCFollow follower;
    private EnemyHealth enemyHealth;

    private float lastAttackTime;
    private float idleTime = 3.0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        follower = GetComponent<NPCFollow>();
        enemyHealth = GetComponent<EnemyHealth>();
        lastAttackTime = -attackCooldown;
    }

	void Update()
	{
		if (enemyHealth != null && enemyHealth.currentHealth <= 0)
			return;

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		// 检测玩家距离
		if (distanceToPlayer <= chaseRange)
		{
			isChasing = true;

			if (distanceToPlayer <= attackRange)
			{
				if (Time.time >= lastAttackTime + attackCooldown)
				{
					isAttacking = true;
					AttackPlayer();
					lastAttackTime = Time.time;
				}
				else
				{
					isChasing = false;
					isAttacking = false;
					isIdling = true;
					idleTime = 3.0f;
					StopMoving();
				}

			}
			else
			{
				isAttacking = false;
				if (isIdling)
				{
					idleTime -= Time.deltaTime;
					if (idleTime <= 0) isIdling = false;
				}
				else
				{
					ChasePlayer();
				}
				
			}
		}
		else
		{
			isChasing = false;
			isAttacking = false;
			StopMoving();
		}

		// 更新动画状态
		if (animator != null)
		{
			//animator.SetBool("IsChasing", isChasing);
			//animator.SetBool("IsAttacking", isAttacking);
			if (isAttacking) animator.SetTrigger("Attack");
		}
 
    }

	void ChasePlayer()
	{
		// 转向玩家
		Vector3 lookDirection = (player.position - transform.position).normalized;
		lookDirection.y = 0; // 保持在地面上
		if (lookDirection != Vector3.zero)
		{
			transform.forward = lookDirection;
		}

		// 向玩家移动
		//Vector3 moveDirection = lookDirection * moveSpeed * Time.deltaTime;
		follower.SetChasingStart(player.position);
    }

    void AttackPlayer()
    {
        // 转向玩家
        Vector3 lookDirection = (player.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = lookDirection;
        }

        // 播放攻击动画
        if (animator != null)
            animator.SetTrigger("Attack");

        // 应用伤害到玩家（需要在玩家身上添加Health组件）
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
		if (playerHealth != null)
		{
			// 敌人攻击力
			this.DelayDoSomething(0.3f, () => { playerHealth.TakeDamage(5); });
        }
    }

	void StopMoving()
	{
		follower.SetChasingEnd();
	}

    // 用于在编辑器中可视化检测范围
    void OnDrawGizmosSelected()
    {
        // 绘制追逐范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}    