using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour,IDamageable
{
    [Header("敌人属性")]
    public int maxHealth = 100;
    public float knockbackForce = 5f;
    public float invulnerabilityDuration = 0.5f;

    [Header("受伤效果")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    public Material hitMaterial;
    public Color hitColor = Color.red;

    [Header("UI元素")]
    public Slider healthSlider;
    public GameObject healthBarUI;

    public int currentHealth  { get; set; }
    private bool isInvulnerable = false;
    private Renderer enemyRenderer;
    private Material originalMaterial;
    private AudioSource audioSource;
    //private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponentInChildren<Renderer>();
        originalMaterial = enemyRenderer.material;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        // 初始化血条
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthBarUI != null)
            healthBarUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
		Debug.Log("打到怪物");
        if (isInvulnerable || currentHealth <= 0)
			return;

        // 激活血条UI
        if (healthBarUI != null)
            healthBarUI.SetActive(true);

        // 减少生命值
        currentHealth -= damage;

        // 更新血条
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // 显示受伤效果
        ShowHitEffect();

        // 播放受伤音效
        PlayHitSound();

        // 应用击退效果
        ApplyKnockback();

        // 开始无敌帧
        StartCoroutine(MakeInvulnerable());

        // 检查敌人是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ShowHitEffect()
    {
        // 显示特效
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, 2f);
        }

        // 材质闪烁效果
        if (enemyRenderer != null && hitMaterial != null)
        {
            Material[] materials = enemyRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = hitMaterial;
            }
            enemyRenderer.materials = materials;

            // 恢复原始材质
            Invoke("RestoreOriginalMaterial", 0.1f);
        }
        else if (enemyRenderer != null)
        {
            // 如果没有提供特殊材质，使用颜色闪烁
            Color originalColor = enemyRenderer.material.color;
            enemyRenderer.material.color = hitColor;
            Invoke("RestoreOriginalColor", 0.1f);
        }
    }

    void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    void ApplyKnockback()
    {
        //if (rb != null)
        {
            // 获取攻击方向（假设攻击来自玩家位置）
            //GameObject player = GameObject.FindGameObjectWithTag("Player");
			//if (player != null)
			//{
				animator.SetTrigger("Hurt");
                //Vector3 knockbackDirection = (transform.position - player.transform.position).normalized;
				//knockbackDirection.y = 0.2f; // 添加一点向上的力
				//rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
			//}
        }
    }

    System.Collections.IEnumerator MakeInvulnerable()
    {
        isInvulnerable = true;
        
        // 闪烁效果
        if (enemyRenderer != null)
        {
            float flashInterval = 0.1f;
            int flashCount = Mathf.FloorToInt(invulnerabilityDuration / flashInterval);
            
            for (int i = 0; i < flashCount; i++)
            {
                enemyRenderer.enabled = !enemyRenderer.enabled;
                yield return new WaitForSeconds(flashInterval);
            }
            
            enemyRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }
        
        isInvulnerable = false;
    }

    void RestoreOriginalMaterial()
    {
        if (enemyRenderer != null)
        {
            Material[] materials = enemyRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = originalMaterial;
            }
            enemyRenderer.materials = materials;
        }
    }

    void RestoreOriginalColor()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.white;
        }
    }

    void Die()
    {
        // 禁用碰撞器和刚体
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;
            
        // if (rb != null)
        // {
        //     rb.isKinematic = true;
        //     rb.velocity = Vector3.zero;
        // }

        // 禁用敌人AI
        MonoBehaviour[] aiScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in aiScripts)
        {
            if (script != this && script.enabled)
                script.enabled = false;
        }

        // 播放死亡动画
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
            
            // 动画结束后销毁对象
            float deathAnimationLength = 2f; // 根据实际动画长度调整
            Destroy(gameObject, deathAnimationLength);
        }
        else
        {
            // 如果没有动画，直接销毁
            Destroy(gameObject);
        }

        // 禁用血条UI
        if (healthBarUI != null)
            healthBarUI.SetActive(false);
    }
}    