using UnityEngine;

public class NPCFollow : MonoBehaviour
{
    private Transform player;
    private Vector3 targetPosition;
    private float stoppingDistance;
    private float rotationSpeed;
    [HideInInspector]public Animator animator;
    private bool isPlayerMoving; // 主角是否在移动
 
    
    // 延迟参数（可在Inspector调整范围）
    [SerializeField] private float minStartDelay = 0.1f;
    [SerializeField] private float maxStartDelay = 0.3f;
    [SerializeField] private float speed = 2.0f; //  
    
    private float startDelayTimer; // 启动延迟计时器
    private bool isMoving;         // 当前NPC是否在移动（考虑延迟后）
    private int formationLayer;    // 阵型层级（0=前排，1=中排，2=后排）

    // 初始化时传入阵型层级（用于层级延迟）
    public void Initialize(Transform player, Vector3 formationOffset, float followDistance, 
                          float stoppingDistance, float rotationSpeed, int layer)
    {
        this.player = player;
        this.stoppingDistance = stoppingDistance;
        this.rotationSpeed = rotationSpeed;
        this.formationLayer = layer; // 记录当前NPC所在层级
        animator = GetComponent<Animator>();
        targetPosition = transform.position;

		speed = Random.Range(1.5f, 2.3f);
		GenRandomDelayTime();
    }

	public void GenRandomDelayTime()
	{ 
        // 随机初始化延迟计时器（避免所有NPC延迟相同）
        startDelayTimer = Random.Range(minStartDelay, maxStartDelay) * formationLayer + 0.5f;
	}

	public void SetTargetPosition(Vector3 position)
	{
		// 锁定Y轴，避免高度差影响朝向
		position.y = transform.position.y;
		targetPosition = position;
	}

    // 接收主角是否移动的状态
    public void SetPlayerMovingState(bool isMoving)
    {
        isPlayerMoving = isMoving;
    }

    void Update()
    {
        if (player == null || animator == null) return;
		if (!isMoving && !isPlayerMoving) return;
		
        // 计算水平距离（忽略Y轴）
		float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        // 处理启动延迟（主角移动时）
        if (!isMoving && distance > stoppingDistance*1.5f)
        {
            startDelayTimer -= Time.deltaTime;
            if (startDelayTimer <= 0)
            {
                isMoving = true;
				GenRandomDelayTime();
 
            }
        }
        // 处理停止延迟（主角停止时）
        else if (isMoving)
        {
			if (distance < stoppingDistance)
			{
				isMoving = false;
            }
        }


		// 更新动画速度
		var s = Mathf.Clamp(speed*0.5f * distance / stoppingDistance, 0, speed);
        animator.SetFloat("Speed", isMoving ? s : 0);

        // 控制朝向（仅在移动时修正）
        if (isMoving)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0; // 投影到地面
            direction.Normalize();
            
            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // 应用Root Motion（仅水平位移）
    private void OnAnimatorMove()
    {
        if (animator && isMoving)
        {
            Vector3 deltaPos = animator.deltaPosition;
            deltaPos.y = 0;
            transform.Translate(deltaPos, Space.World);
        }
    }
}