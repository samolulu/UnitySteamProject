using UnityEngine;

/// <summary>
/// 处理人物根运动和优化
/// </summary>
public class Motion : MonoBehaviour
{
	//private Transform target;
	private Vector3 targetPosition;

	[Header("移动参数")]
	public string speedParamName = "Speed";
	public float stoppingDistance = 2.0f;
	public float rotationSpeed = 10.0f;
	public float speed = 1.5f;
	[SerializeField] public Animator animator;
	public bool isMoving;


	// 新增：渲染状态与驱动模式
	[SerializeField]private bool isVisible = true;        // 是否在镜头内
	[SerializeField]private bool useRootMotion = true;    // 是否使用Root Motion
	private Vector3 lastRootMotionPos;    // 记录Root Motion最后位置
	private Quaternion lastRootMotionRot; // 记录Root Motion最后旋转

	// 新增：不可见时的移动参数
	private float invisibleMoveSpeed;     // 不可见时的移动速度
	private Quaternion targetRotation;    // 不可见时的目标旋转

	// 【新增】距离优化相关参数
	[SerializeField] private float distanceThreshold = 30f; // 距离阈值（米）
	private Camera mainCamera;            // 主摄像机引用
	[SerializeField]private bool isTooFar = false;        // 是否超过距离阈值
	private float visibilityCheckInterval = 0.2f; // 距离检测间隔
	private float visibilityCheckTimer;   // 检测计时器

	void Start()
	{
		animator = GetComponent<Animator>();

		// 设置Animator Culling为Complete（不可见时完全停止更新）
		if (animator != null)
		{
			animator.cullingMode = AnimatorCullingMode.CullCompletely;
		}

		// 初始化状态
		isVisible = true;
		useRootMotion = true;
		lastRootMotionPos = transform.position;
		lastRootMotionRot = transform.rotation;
		targetPosition = transform.position;

		// 【新增】获取主摄像机引用
		mainCamera = Camera.main;
		if (mainCamera == null)
		{
			Debug.LogWarning("未找到主摄像机，请确保主摄像机的Tag设置为MainCamera");
		}
	}


	public Motion SetTargetPosition(Vector3 position)
	{
		position.y = transform.position.y;
		targetPosition = position;
		return this;
	}

	public Motion SetSpeed(float speed)
	{
		this.speed = speed;
		return this;
	}
 
	// 监测可见性变化
	public void OnBecameVisible()
	{
		isVisible = true;
		// 【新增】距离过远时不切换回RootMotion
		if (!isTooFar)
		{
			SwitchToRootMotion();
		}
	}

	public void OnBecameInvisible()
	{
		isVisible = false;
		SwitchToTransformMove();
	}

	// 切换到Root Motion驱动
	private void SwitchToRootMotion()
	{
		if (useRootMotion || isTooFar) return; // 【新增】距离过远时不切换

		// 恢复Animator并记录当前状态
		if (animator != null)
		{
			animator.enabled = true;

			// 重置位置和旋转为当前Transform状态（避免跳变）
			transform.position = lastRootMotionPos;
			transform.rotation = lastRootMotionRot;
		}

		useRootMotion = true;
	}

	// 切换到Transform直接移动
	private void SwitchToTransformMove()
	{
		if (!useRootMotion) return;

		// 记录当前状态并禁用Animator
		if (animator != null)
		{
			lastRootMotionPos = transform.position;
			lastRootMotionRot = transform.rotation;
			animator.enabled = false;
		}

		// 计算当前移动速度用于Transform驱动
		invisibleMoveSpeed = animator != null ? animator.GetFloat(speedParamName) : 0;
		targetRotation = transform.rotation;

		useRootMotion = false;
	}

	void Update()
	{
 
		// 【新增】距离检测逻辑（每0.2秒检测一次）
		if (mainCamera != null)
		{
			visibilityCheckTimer += Time.deltaTime;
			if (visibilityCheckTimer >= visibilityCheckInterval)
			{
				CheckDistanceToCamera();
				visibilityCheckTimer = 0;
			}
		}

		float distance = Vector3.Distance(
			new Vector3(transform.position.x, 0, transform.position.z),
			new Vector3(targetPosition.x, 0, targetPosition.z)
		);
		
        // 处理启动/停止逻辑（与原代码相同）
		if (!isMoving && distance > stoppingDistance * 1.5f)
		{
			isMoving = true;
		}
		else if (isMoving && distance <= stoppingDistance)
		{
			isMoving = false;
		}

		// 更新动画速度（仅在可见且未超距时）
		if (isVisible && !isTooFar && animator != null) // 【新增】!isTooFar判断
		{
			var s = Mathf.Clamp(speed * 0.5f * distance / stoppingDistance, 0, speed);
 
			animator.SetFloat(speedParamName, isMoving ? s : 0);
		}

		// 【新增】更新不可见或超距时的移动速度
		if ((!isVisible || isTooFar) && isMoving)
		{
			invisibleMoveSpeed = Mathf.Clamp(speed * 0.5f * distance / stoppingDistance, 0, speed);
		}

		// 控制朝向（可见时通过Root Motion，不可见时直接修改Transform）
		if (isMoving)
		{
			Vector3 direction = targetPosition - transform.position;
			direction.y = 0;
			direction.Normalize();

			if (direction.sqrMagnitude < 0.01f)
				direction = transform.forward;

			targetRotation = Quaternion.LookRotation(direction, Vector3.up);

			if (useRootMotion)
			{
				// Root Motion模式下的旋转（可见时）
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					targetRotation,
					rotationSpeed * Time.deltaTime
				);
			}
		}
	}

	// 【新增】检测与摄像机的距离
	private void CheckDistanceToCamera()
	{
		float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
		bool wasTooFar = isTooFar;
		isTooFar = distanceToCamera > distanceThreshold;

		// 距离状态变化时切换模式
		if (isTooFar && !wasTooFar)
		{
			// 超过距离阈值，强制切换到Transform驱动
			SwitchToTransformMove();
		}
		else if (!isTooFar && wasTooFar && isVisible)
		{
			// 回到距离阈值内且可见，切换回RootMotion
			SwitchToRootMotion();
		}
	}

	// 应用Root Motion（仅在可见且启用Root Motion时）
	private void OnAnimatorMove()
	{
		if (useRootMotion && animator && isMoving && isVisible && !isTooFar) // 【新增】!isTooFar判断
		{
			Vector3 deltaPos = animator.deltaPosition;
			deltaPos.y = 0;
			transform.Translate(deltaPos, Space.World);
		}
	}

	// 不可见时的移动（通过Transform直接驱动）
	private void LateUpdate()
	{
		if (!useRootMotion && isMoving)
		{
			// 直接修改位置
			Vector3 moveDir = (targetPosition - transform.position).normalized;
			transform.position += moveDir * invisibleMoveSpeed * 2 * Time.deltaTime;

			// 直接修改旋转
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				rotationSpeed * Time.deltaTime
			);

			// 记录位置用于可见时恢复
			lastRootMotionPos = transform.position;
			lastRootMotionRot = transform.rotation;
		}
	}
}