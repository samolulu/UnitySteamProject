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
    public bool isPatrolling = false; // 新增：是否在巡逻
    public float attackCooldown = 2f;

    [Header("寻敌参数")]
    public float detectionRange = 20f;  // 检测范围
    public LayerMask targetLayers;      // 可攻击的目标图层

    [Header("巡逻参数")]  // 新增：巡逻相关参数
    public float patrolRadius = 5f;     // 巡逻范围半径（以初始位置为中心）
    public float patrolWaitTime = 2f;   // 到达巡逻点后的等待时间
    public float patrolSpeed = 2f;      // 巡逻移动速度

    private Transform target;
    public LayerMask enemyLayer;
    private Animator animator;
    private NPCFollow follower;
    private EnemyHealth enemyHealth;

    private float lastAttackTime;
    private float idleTime = 3.0f;
    
    // 新增：巡逻相关变量
    private Vector3 patrolOrigin;       // 巡逻起始点（初始位置）
    private Vector3 currentPatrolPoint; // 当前巡逻目标点
    private float patrolWaitTimer;      // 巡逻等待计时器

    void Start()
    {
        FindTarget();
        animator = GetComponent<Animator>();
        follower = GetComponent<NPCFollow>();
        enemyHealth = GetComponent<EnemyHealth>();
        lastAttackTime = Time.time + Random.Range(0.0f, 0.5f * attackCooldown);
        
        // 初始化巡逻起始点为敌人初始位置
        patrolOrigin = transform.position;
        // 生成第一个巡逻点
        GenerateNewPatrolPoint();
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.currentHealth <= 0)
            return;

        // 先尝试寻找目标
        bool hasTarget = FindTarget();

        // 如果有目标，执行原有逻辑
        if (hasTarget)
        {
            isPatrolling = false; // 停止巡逻
            HandleTargetBehavior();
        }
        // 如果没有目标，执行巡逻逻辑
        else
        {
            HandlePatrolBehavior();
        }

        // 更新动画状态
        UpdateAnimations();
    }

    // 处理有目标时的行为（原有逻辑封装）
    private void HandleTargetBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

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
                    idleTime = Random.Range(3.0f, 12.0f);
                    StopMoving();
                }

            }
            else
            {
                isAttacking = false;
                if (isIdling)
                {
                    idleTime -= Time.deltaTime;
                    if (idleTime <= 0)
                    {
                        isIdling = false;
                        animator.SetFloat("Speed", 0);
                    }
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
    }

    // 新增：处理巡逻行为
    private void HandlePatrolBehavior()
    {
        isChasing = false;
        isAttacking = false;
        isIdling = false;
        isPatrolling = true;

        // 计算到当前巡逻点的距离
        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint);

        // 如果到达巡逻点
        if (distanceToPatrolPoint <= 0.5f)
        {
            // 等待一段时间
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                // 生成新的巡逻点
                GenerateNewPatrolPoint();
                patrolWaitTimer = 0;
            }
            else
            {
                // 等待时停止移动
                StopMoving();
                animator.SetFloat("Speed", 0);
            }
        }
        else
        {
            // 向巡逻点移动
            PatrolToPoint();
        }
    }

    // 新增：生成新的巡逻点
    private void GenerateNewPatrolPoint()
    {
        // 在以初始位置为中心的圆形范围内随机生成巡逻点
        Vector2 randomDir = Random.insideUnitCircle * patrolRadius;
        currentPatrolPoint = new Vector3(
            patrolOrigin.x + randomDir.x,
            transform.position.y,  // 保持Y轴高度不变
            patrolOrigin.z + randomDir.y
        );
    }

    // 新增：向巡逻点移动
    private void PatrolToPoint()
    {
        // 转向巡逻点
        Vector3 lookDirection = (currentPatrolPoint - transform.position).normalized;
        lookDirection.y = 0; // 保持在地面上
        if (lookDirection != Vector3.zero)
        {
            transform.forward = lookDirection;
        }

        // 向巡逻点移动（使用NPCFollow组件）
        follower.SetChasingStart(currentPatrolPoint);
        // 可以根据需要调整巡逻速度
        //follower.SetSpeed(patrolSpeed); // 假设NPCFollow有设置速度的方法，若无则忽略
 
    }

    // 统一更新动画状态
    private void UpdateAnimations()
    {
        if (animator == null) return;
		if (HasTarget() == false)
		{
			
			return;
		}
        if (isAttacking)
			{
				animator.SetTrigger("Attack");
			}
			else if (isChasing)
			{
				animator.SetFloat("Speed", 1f);
			}
			else if (isPatrolling)
			{
				// 巡逻时的动画速度由巡逻速度决定
				animator.SetFloat("Speed", patrolSpeed / moveSpeed);
			}
			else if (isIdling)
			{
				animator.SetFloat("Speed", Random.Range(-1f, -0.4f));
			}
			else
			{
				animator.SetFloat("Speed", 0);
			}
    }

    bool FindTarget()
    {
        if (target != null && IsTargetValid(target)) return true;

        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRange, targetLayers);

        Transform closestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider target in targetsInRange)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance < shortestDistance && IsTargetValid(target.transform))
            {
                shortestDistance = distance;
                closestTarget = target.transform;
            }
        }

        target = closestTarget;
        return target != null;
    }

	public bool HasTarget() => IsTargetValid(target);
	bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        
        IDamageable targetHealth = target.GetComponent<IDamageable>();
        if (targetHealth != null && targetHealth.currentHealth <= 0)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > detectionRange) return false;
        return true;
    }

    void ChasePlayer()
    {
        Vector3 lookDirection = (target.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = lookDirection;
        }

        follower.SetChasingStart(target.position);
        animator.SetFloat("Speed", 1f);
    }

    void AttackPlayer()
    {
        Vector3 lookDirection = (target.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = lookDirection;
        }

        if (animator != null)
            animator.SetTrigger("Attack");

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            IDamageable health = enemy.GetComponent<IDamageable>();
            if (health != null)
            {
                this.DelayDoSomething(0.65f, () => { health.TakeDamage(5); });
            }
        }
    }

    void StopMoving()
    {
        follower.SetChasingEnd();
        animator.SetFloat("Speed", 0);
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制追逐范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制巡逻范围（仅在编辑器中显示）
        if (Application.isEditor && !isPatrolling)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolOrigin, patrolRadius);
        }

        // 绘制当前巡逻路径
        if (isPatrolling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentPatrolPoint);
            Gizmos.DrawWireSphere(currentPatrolPoint, 0.5f);
        }
    }
}