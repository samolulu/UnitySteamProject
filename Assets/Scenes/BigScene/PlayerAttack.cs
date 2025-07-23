using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻击参数")]
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    public LayerMask enemyLayer;

    [Header("攻击效果")]
    public ParticleSystem attackEffect;
    public AudioSource attackSound;

    private float lastAttackTime;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        // 播放攻击动画
        if (animator != null)
            animator.SetTrigger("Attack");

        // 播放攻击音效
        if (attackSound != null)
            attackSound.Play();

        // 播放攻击特效
        if (attackEffect != null)
            attackEffect.Play();

        // 检测攻击范围内的敌人
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // 计算攻击方向
            Vector3 direction = enemy.transform.position - transform.position;
            direction.y = 0; // 忽略Y轴高度差

            // 确保敌人在前方扇形范围内（120度）
            if (Vector3.Angle(transform.forward, direction) < 60f)
            {
                // 调用敌人的受伤方法
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
					this.DelayDoSomething(0.3f, () => { enemyHealth.TakeDamage(attackDamage); });
                    
                }
            }
        }
    }

    // 用于在编辑器中可视化攻击范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制攻击扇形范围
        Vector3 forward = transform.forward * attackRange;
        Quaternion leftRotation = Quaternion.Euler(0, -60, 0);
        Quaternion rightRotation = Quaternion.Euler(0, 60, 0);
        Vector3 leftDirection = leftRotation * forward;
        Vector3 rightDirection = rightRotation * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDirection);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection);
    }
}    