using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float maxSlopeAngle = 45f; // 最大爬坡角度
    public float groundOffset = 0.1f; // 离地高度

	Transform tran_cam;
	void Start()
	{
		tran_cam = Camera.main.transform;
	}

	void Update()
	{

		// 获取输入
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		Vector3 camprojforward = Vector3.ProjectOnPlane(tran_cam.forward, Vector3.up);

		Vector3 inputDir = camprojforward * v + tran_cam.right * h;// new Vector3(h, 0, v);


		// 应用移动
		transform.position += inputDir * moveSpeed * Time.deltaTime;

		
		//transform.forward = Vector3.Lerp(transform.forward, inputDir, 0.1f);
    }

 
}