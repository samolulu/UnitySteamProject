using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 lookAtAngle = new Vector3(0f, 5f, -10f);
    public float offsetY = 0.9f;
    public float smoothSpeed = 0.02f;
    
    // 视角控制参数
    public float rotationSpeed = 5.0f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 45f;
    
    // 缩放控制参数 - 基础设置
    public float zoomIntensity = 1.5f;      // 缩放强度
    public float minDistance = 3.0f;        // 最小距离
    public float maxDistance = 15.0f;       // 最大距离
    public float zoomDamping = 5.0f;        // 缩放平滑阻尼
    
    // 缩放控制参数 - 对数曲线调整
    public float logBase = 2.0f;            // 对数基数
    public float closeRangeMultiplier = 3.0f; // 近距离额外乘数
    
    private float currentX = 0f;
    private float currentY = 0f;
    private float currentDistance;
    private float targetDistance;
    private bool isRotating = false;

    void Start()
    {
        // 初始化视角角度和距离
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
        currentDistance = lookAtAngle.magnitude;
        targetDistance = currentDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 处理鼠标右键旋转
        HandleRotationInput();
        
        // 处理鼠标滚轮缩放
        HandleZoomInput();
        
        // 平滑过渡缩放
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomDamping);

        // 计算旋转后的偏移
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = lookAtAngle.normalized;
        Vector3 scaledOffset = direction * currentDistance;
        Vector3 rotatedOffset = rotation * scaledOffset;
        
        // 计算相机目标位置
        Vector3 desiredPosition = target.position + rotatedOffset;
        
        // 平滑移动相机
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 让相机始终看向目标
        transform.LookAt(target.position + new Vector3(0, offsetY, 0));
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

    void HandleZoomInput()
    {
        // 获取鼠标滚轮输入
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // 使用对数函数计算动态缩放因子
            // 1. 将当前距离映射到0-1范围
            float normalizedDistance = (currentDistance - minDistance) / (maxDistance - minDistance);
            
            // 2. 计算对数缩放因子 - 近距离时增长快，远距离时增长慢
            float logFactor = Mathf.Log(normalizedDistance * (logBase - 1) + 1, logBase);
            
            // 3. 反转缩放因子，使近距离有更大影响
            float zoomFactor = 1 - logFactor;
            
            // 4. 添加近距离额外乘数，增强近距离的缩放速度
            if (currentDistance < minDistance + 5.0f) // 近距离阈值
            {
                zoomFactor *= closeRangeMultiplier;
            }
			zoomFactor = Mathf.Max(zoomFactor, 0.03f);
			
            // 应用缩放
			float zoomAmount = scrollInput * zoomIntensity * zoomFactor * currentDistance;
            targetDistance -= zoomAmount;
            
            // 限制距离范围
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }
}