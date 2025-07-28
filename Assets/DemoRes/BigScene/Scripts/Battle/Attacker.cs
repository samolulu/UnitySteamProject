using UnityEngine;

// 武器上的攻击判定组件
public class Attacker : MonoBehaviour
{
    [Header("攻击参数")]
    public float attackDamage = 10f;
    public LayerMask targetLayers;
    //public float attackCooldown = 1f;
    
    //private bool isOnCooldown = false;
    private Collider weaponCollider;
    //private Unit unit;
    
    void Start()
    {
        weaponCollider = GetComponent<Collider>();
		//unit = GetComponentInParent<Unit>();
		DisableAttack();
    }
    
    // 启用攻击判定（通常由动画事件调用）
    public void EnableAttack()
    {
        if (weaponCollider == null) return;
        
        weaponCollider.enabled = true;
		
    }
    
    // 禁用攻击判定（通常由动画事件调用）
    public void DisableAttack()
    {
        if (weaponCollider == null) return;
        
        weaponCollider.enabled = false;
        //isOnCooldown = true;
        //Invoke(nameof(ResetCooldown), attackCooldown);
    }

	// 重置冷却
	// void ResetCooldown()
	// {
	// 	isOnCooldown = false;
    // }
    
    // 碰撞检测
    void OnTriggerEnter(Collider other)
    {
        // 检查是否是目标阵营
        if ((targetLayers & (1 << other.gameObject.layer)) == 0) return;
        
        // 尝试获取目标的Unit组件
        Unit targetUnit = other.GetComponentInParent<Unit>();
        if (targetUnit != null)
        {
            // 造成伤害
            targetUnit.TakeDamage(attackDamage);

			// 通知目标被攻击
        	// 计算两个边界的最近点作为碰撞位置
			Vector3 collisionPosition = weaponCollider.bounds.ClosestPoint(other.bounds.center);
            targetUnit.OnAttacked(collisionPosition);

			// 禁用攻击判定，防止多次伤害
			DisableAttack();
        }
    }
}