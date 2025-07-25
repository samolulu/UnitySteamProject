using UnityEngine;

public class RootMotionCharacterController : MonoBehaviour
{
	[SerializeField] private string speedParamName = "Speed";
	[SerializeField] private string speedHParamName = "H";
	[SerializeField] private float acceleration = 2.0f;
	[SerializeField] private float deceleration = 5.0f;
	[SerializeField] private float maxSpeed = 1.0f;
	[SerializeField] private float turnSpeed = 180.0f;

	private Transform tran_camera;
	private Animator animator;
	private float currentSpeed = 0f;
	private float currentSpeed_H = 0f;
	private Vector3 lastFrameRootMotion;
	bool isMoving;
	bool isMoving_H;

	void Start()
	{
		tran_camera = Camera.main.transform;
		animator = GetComponent<Animator>();
		if (animator)
		{
			animator.applyRootMotion = true;
			// 禁用自动脚本来处理旋转
			animator.updateMode = AnimatorUpdateMode.Normal;
		}
	}

	void Update()
	{
		float verticalInput = Input.GetAxis("Vertical");
		float horizontalInput = Input.GetAxis("Horizontal");

		// 处理速度控制
		isMoving = Mathf.Abs(verticalInput) > 0.01f;
		isMoving_H = !isMoving && Mathf.Abs(horizontalInput) > 0.01f;
		if (isMoving)
			currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * verticalInput, acceleration * Time.deltaTime);
		else
			currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, (deceleration + verticalInput) * Time.deltaTime);

		if (isMoving_H)
			currentSpeed_H = Mathf.MoveTowards(currentSpeed_H, maxSpeed * horizontalInput, acceleration * Time.deltaTime);
		else
			currentSpeed_H = Mathf.MoveTowards(currentSpeed_H, 0f, (deceleration + horizontalInput) * Time.deltaTime);

		animator.SetFloat(speedParamName, currentSpeed);
		animator.SetFloat(speedHParamName, currentSpeed_H);

		// 仅在非低速前进且有水平输入时允许旋转
		//if ( Mathf.Abs(horizontalInput) > 0.1f && currentSpeed > 0.1f)
		if (currentSpeed > 0.0f) //向前移动过程中自动转向镜头前方
		{
			Vector3 crossProduct = Vector3.Cross(transform.forward, tran_camera.forward );

			// 判断目标向量是否在参考向量的右侧
			//bool isOnRight = crossProduct.y > 0;
			transform.Rotate(0, crossProduct.y * turnSpeed * Time.deltaTime, 0);
		}
 
	}

	public void SetMovementSpeed(float speed)
	{
		animator.SetFloat(speedParamName, speed);
	}

	// 精确控制Root Motion应用
	void OnAnimatorMove()
	{
		if (animator && (currentSpeed != 0.0f || currentSpeed_H != 0.0f))
		{
			// 仅应用XZ平面的位移，忽略Y轴和旋转
			Vector3 positionDelta = animator.deltaPosition;
			positionDelta.y = 0; // 防止上下浮动
			transform.Translate(positionDelta, Space.World);

			// 仅在没有手动旋转输入时使用动画的旋转
			if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f)
			{
				transform.rotation = animator.rootRotation;
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		if (animator && Application.isPlaying)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + animator.deltaPosition * 10f);
			
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position, transform.forward * 2f);
		}
	}
}