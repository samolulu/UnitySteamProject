using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻击参数")]
    //public float attackRange = 2f;
    //public int attackDamage = 10;
    public float attackCooldown = 1f;
    public float turnSpeed = 50f; // 新增：转向速度
    //public LayerMask enemyLayer;
    public LayerMask groundLayer; // 新增：地面图层

    [Header("攻击效果")]
    public ParticleSystem attackEffect;
    public AudioSource attackAudioSource;
    public AudioClip attackSound;
    public AudioClip attackSound_B;

    private float lastAttackTime;
    private Animator animator;
    private bool isTurning = false; // 新增：转向状态
    private Vector3 targetDirection; // 新增：目标转向方向

	
	void Start()
    {
        animator = GetComponent<Animator>();
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (isTurning)
        {
            TurnToTargetDirection();
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Vector3? clickWorldPos = GetClickWorldPosition();
            if (clickWorldPos.HasValue)
            {
                targetDirection = clickWorldPos.Value - transform.position;
                targetDirection.y = 0; // 忽略Y轴
                isTurning = true;
            }
        }
    }

    // 新增：获取鼠标点击的世界坐标
    private Vector3? GetClickWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        return null;
    }

    // 新增：转向目标方向
    private void TurnToTargetDirection()
    {
        if (targetDirection.sqrMagnitude < 0.1f)
        {
            isTurning = false;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            targetRotation, 
            turnSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
        {
            isTurning = false;
            Attack();
            lastAttackTime = Time.time;
        }
    }

    int attackId = 0;
    void Attack()
    {
        // 播放攻击动画
        if (animator != null)
            animator.SetTrigger(attackId %3 ==0 ? "Attack_B" :"Attack");

        // 播放攻击音效
        if (attackAudioSource != null)
            attackAudioSource.PlayOneShot(attackId %3 ==0 ? attackSound_B : attackSound);

        // 播放攻击特效
        if (attackEffect != null)
            attackEffect.Play();

        // // 检测攻击范围内的敌人
        // Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        // foreach (Collider enemy in hitEnemies)
        // {
        //     // 计算攻击方向
        //     Vector3 direction = enemy.transform.position - transform.position;
        //     direction.y = 0; // 忽略Y轴高度差

        //     // 确保敌人在前方扇形范围内（120度）
        //     if (Vector3.Angle(transform.forward, direction) < 60f)
        //     {
        //         // 调用敌人的受伤方法
        //         IDamageable enemyHealth = enemy.GetComponent<IDamageable>();
        //         if (enemyHealth != null)
        //         {
        //             this.DelayDoSomething(attackId %3 ==0 ? 1.0f :0.65f, () => { enemyHealth.TakeDamage(attackDamage); });
        //         }
        //     }
        // }

        attackId++;
    }

    // 用于在编辑器中可视化攻击范围
    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, attackRange);

    //     // 绘制攻击扇形范围
    //     Vector3 forward = transform.forward * attackRange;
    //     Quaternion leftRotation = Quaternion.Euler(0, -60, 0);
    //     Quaternion rightRotation = Quaternion.Euler(0, 60, 0);
    //     Vector3 leftDirection = leftRotation * forward;
    //     Vector3 rightDirection = rightRotation * forward;

    //     Gizmos.DrawLine(transform.position, transform.position + leftDirection);
    //     Gizmos.DrawLine(transform.position, transform.position + rightDirection);
    // }
}