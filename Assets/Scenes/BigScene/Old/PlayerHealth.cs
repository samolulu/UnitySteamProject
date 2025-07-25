using UnityEngine;
using UnityEngine.UI;

public interface IDamageable
{
	public int currentHealth { get; set; }
	public void TakeDamage(int amount);
}

public class PlayerHealth : MonoBehaviour,IDamageable
{
	public int maxHealth = 100;
	public int currentHealth { get; set; }

	public Slider healthSlider;
	public Image damageImage;
	public float flashSpeed = 5f;
	public Color flashColor = new Color(1f, 0f, 0f, 0.1f);

	private bool isDamaged;
	private Animator anim;
	private RootMotionCharacterController playerMovement;
	private bool isDead;

	void Start()
	{
		currentHealth = maxHealth;
		anim = GetComponent<Animator>();
		playerMovement = GetComponent<RootMotionCharacterController>();

		// 初始化血条
		if (healthSlider != null)
		{
			healthSlider.maxValue = maxHealth;
			healthSlider.value = currentHealth;
		}
	}

	void Update()
	{
		// 显示受伤效果
		if (isDamaged)
		{
			if (damageImage != null)
				damageImage.color = flashColor;
		}
		else
		{
			if (damageImage != null)
				damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
		}
		isDamaged = false;
	}

	public void TakeDamage(int amount)
	{
		Debug.Log("打到玩家");
		isDamaged = true;

		// 减少生命值
		currentHealth -= amount;

		// 更新血条
		if (healthSlider != null)
			healthSlider.value = currentHealth;

		// 播放受伤动画
		if (anim != null)
			anim.SetTrigger("Hurt");

		// 检查是否死亡
		if (currentHealth <= 0 && !isDead)
		{
			Die();
		}
	}

	void Die()
	{
		isDead = true;

		// 禁用移动和攻击
		if (playerMovement != null)
			playerMovement.enabled = false;

		// 播放死亡动画
		if (anim != null)
			anim.SetTrigger("Die");

		// 禁用玩家控制器
		enabled = false;
	}
}    