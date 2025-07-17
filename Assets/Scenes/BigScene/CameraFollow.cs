using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float smoothSpeed = 0.02f;
    
    // 视角控制参数
    public float rotationSpeed = 5.0f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 45f;
    
    private float currentX = 0f;
    private float currentY = 0f;
    private bool isRotating = false;

    void Start()
    {
        // 初始化视角角度
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 处理鼠标右键旋转
        HandleRotationInput();

        // 计算旋转后的偏移
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 rotatedOffset = rotation * offset;
        
        // 计算相机目标位置
        Vector3 desiredPosition = target.position + rotatedOffset;
        
        // 平滑移动相机
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 让相机始终看向目标
        transform.LookAt(target);
    }

    void HandleRotationInput()
    {
        // 右键按下开始旋转
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // 右键释放停止旋转
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // 处理旋转
        if (isRotating)
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            
            // 限制垂直旋转角度
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }
    }
}