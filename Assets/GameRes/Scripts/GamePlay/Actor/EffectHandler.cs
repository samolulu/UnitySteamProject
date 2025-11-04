using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectHandler : MonoBehaviour
{
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

    [Header("脚步声")]
    [SerializeField] private AudioClip[] footstepClips; // 不同材质的脚步声
 

	
    private bool isInvulnerable = false;
    private Renderer eRenderer;
    private Material originalMaterial;
    public AudioSource audioSource;

    // Start is called before the first frame update
	void Start()
    {
        eRenderer = GetComponentInChildren<Renderer>();
        originalMaterial = eRenderer.material;
        audioSource = GetComponent<AudioSource>();
    }

	public void PlayFootstep()
	{
        if (footstepClips.Length > 0)
        {
            // 随机选择一个脚步声（模拟不同的脚步力度）
            int randomIndex = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[randomIndex]);
        }
	}

	public void PlayDie()
	{
		
	}

	public void PlayDamage(float damage)
	{
		// 激活血条UI
		// if (healthBarUI != null)
		// 	healthBarUI.SetActive(true);

		// // 更新血条
		// if (healthSlider != null)
		// 	healthSlider.value = currentHealth;
	
	}

   	public void PlayAttacked(Vector3 hitPos)
	{

		// 显示受伤效果
		ShowHitEffect(hitPos);

		// 播放受伤音效
		PlayHitSound();

		// 应用击退效果
		ApplyKnockback();

		// 开始无敌帧
		StartCoroutine(MakeInvulnerable());

	}
 
    void ShowHitEffect(Vector3 hitPos)
    {
        // 显示特效
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
            Destroy(hitEffect, 2f);
        }

        // 材质闪烁效果
        if (eRenderer != null && hitMaterial != null)
        {
            Material[] materials = eRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = hitMaterial;
            }
            eRenderer.materials = materials;

            // 恢复原始材质
            Invoke("RestoreOriginalMaterial", 0.1f);
        }
        else if (eRenderer != null)
        {
            // 如果没有提供特殊材质，使用颜色闪烁
            //Color originalColor = renderer.material.color;
            eRenderer.material.color = hitColor;
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
 
    }

    System.Collections.IEnumerator MakeInvulnerable()
    {
        isInvulnerable = true;
        
        // 闪烁效果
        if (eRenderer != null)
        {
            float flashInterval = 0.1f;
            int flashCount = Mathf.FloorToInt(invulnerabilityDuration / flashInterval);
            
            for (int i = 0; i < flashCount; i++)
            {
                eRenderer.enabled = !eRenderer.enabled;
                yield return new WaitForSeconds(flashInterval);
            }
            
            eRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }
        
        isInvulnerable = false;
    }

    void RestoreOriginalMaterial()
    {
        if (eRenderer != null)
        {
            Material[] materials = eRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = originalMaterial;
            }
            eRenderer.materials = materials;
        }
    }

    void RestoreOriginalColor()
    {
        if (eRenderer != null)
        {
            eRenderer.material.color = Color.white;
        }
    }

}
