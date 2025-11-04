using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 世界空间下的相机跟随控制器
/// 支持目标跟随、鼠标拖拽平移、右键旋转、滚轮缩放以及碰撞避让功能
/// </summary>
public class CameraFollow_World : MonoBehaviour
{
    #region 公共参数配置   
    /// <summary>
    /// 鼠标移动检测阈值(像素)，用于判断是否为有效拖拽
    /// </summary>
    public static float mouseMoveThreshold = 15.0f;

    [Header("目标与基础设置")]
    /// <summary>
    /// 相机跟随的目标对象
    /// </summary>
    public Transform target;
    /// <summary>
    /// 默认的注视点偏移量(相对于目标位置)
    /// </summary>
    public Vector3 defaultLookAtOffset = new Vector3(0f, 0.4f, 0f);
    /// <summary>
    /// 位置平滑移动速度系数
    /// </summary>
    public float positionSmoothSpeed = 0.8f;

    [Header("平移控制")]
    /// <summary>
    /// 平移灵敏度系数
    /// </summary>
    public float moveSensitivity = 1f;
    /// <summary>
    /// 平移平滑时间
    /// </summary>
    public float moveSmoothTime = 0.1f;

    [Header("旋转控制")]
    /// <summary>
    /// 旋转速度系数
    /// </summary>
    public float rotationSpeed = 10.0f;
    /// <summary>
    /// Y轴旋转角度限制(最小值, 最大值)
    /// </summary>
    public Vector2 yRotationLimits = new Vector2(-5, 89);
    /// <summary>
    /// 注视点平滑旋转时间
    /// </summary>
    public float lookAtSmoothTime = 0.1f;
    /// <summary>
    /// 旋转平滑时间
    /// </summary>
    public float rotationSmoothTime = 0.1f;
    /// <summary>
    /// 垂直旋转阻尼系数
    /// </summary>
    public float verticalRotationDamping = 0.6f;

    [Header("缩放控制")]
    /// <summary>
    /// 缩放强度系数
    /// </summary>
    public float zoomIntensity = 3.0f;
    /// <summary>
    /// 最小缩放距离
    /// </summary>
    public float minDistance = 1.0f;
    /// <summary>
    /// 最大缩放距离
    /// </summary>
    public float maxDistance = 2500.0f;

    /// <summary>
    /// 缩放阻尼系数
    /// </summary>
    public float zoomDamping = 0.3f;
 
    /// <summary>
    /// 缩放速度曲线(上行和下行曲线)
    /// </summary>
    public AnimationCurve zoomSpeedCurve_up = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);
    public AnimationCurve zoomSpeedCurve_down = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);
    public AnimationCurve rotateYDistanceCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);

    /// <summary>
    /// Y轴旋转限制的最小距离阈值
    /// </summary>
    public float minRotateYDistance = 2f;
    /// <summary>
    /// Y轴旋转限制的最大距离阈值
    /// </summary>
    public float maxRotateYDistance = 100f;

    [Header("镜头避让设置")]
    /// <summary>
    /// 是否启用碰撞避让功能
    /// </summary>
    public bool enableAvoidance = true;
    /// <summary>
    /// 需要检测的障碍物层级
    /// </summary>
    public LayerMask obstacleLayers;
    /// <summary>
    /// 碰撞检测球半径
    /// </summary>
    public float detectSphereRadius = 0.3f;
    /// <summary>
    /// 额外检测距离
    /// </summary>
    public float extraDetectDistance = 0.5f;
 
    /// <summary>
    /// 避让提速
    /// </summary>
    public float avoidSpeed = 1.0f;
    /// <summary>
    /// 避让检测迭代次数
    /// </summary>
    public int avoidIter = 8;
    /// <summary>
    /// 避让时暂时忽略输入的帧数
    /// </summary>
    public int avoidIgnoreInputFrame = 2;
    /// <summary>
    /// 避让时阻尼系数缩减比例
    /// </summary>
    public float avoidDampingScale = 0.5f;
    /// <summary>
    /// 发生震荡时的逃逸速度
    /// </summary>
    public float avoidShockSpeed = 2.0f;


    [Header("调试Gizmos")]
    /// <summary>
    /// 是否显示调试Gizmos
    /// </summary>
    public bool showGizmos = true;
    /// <summary>
    /// 目标点Gizmos颜色
    /// </summary>
    public Color targetPointColor = Color.white;
    /// <summary>
    /// 理想位置Gizmos颜色
    /// </summary>
    public Color desiredPosColor = new Color(0, 1, 1, 0.5f);
    /// <summary>
    /// 检测射线Gizmos颜色
    /// </summary>
    public Color detectRayColor = Color.yellow;

    #endregion

    #region 私有变量
    /// <summary>
    /// 主相机引用
    /// </summary>
    private Camera _mainCamera;
    
    // 平移相关
    /// <summary>
    /// 当前注视点偏移量(用于平滑过渡)
    /// </summary>
    private Vector3 _currLookAtOffset;
    /// <summary>
    /// 目标注视点偏移量
    /// </summary>
    private Vector3 _targetLookAtOffset;
    /// <summary>
    /// 目标注视点基准位置
    /// </summary>
    private Vector3 _targetLookAtBasePos;
    /// <summary>
    /// 鼠标是否按下状态
    /// </summary>
    private bool _isMouseDown;
    /// <summary>
    /// 鼠标按下时的初始位置
    /// </summary>
    private Vector3 _initialMousePosition;
    /// <summary>
    /// 临时注视点偏移量(用于拖拽开始时记录初始状态)
    /// </summary>
    private Vector3 _tempLookAtOffset;
    /// <summary>
    /// 鼠标是否发生有效移动
    /// </summary>
    private bool _hasMouseMoved;

    // 旋转相关
    /// <summary>
    /// 当前X轴旋转角度
    /// </summary>
    private float _currentX, _currentY;
    /// <summary>
    /// 上一帧X轴旋转角度
    /// </summary>
    private float _lastX, _lastY;
    /// <summary>
    /// 目标X轴旋转角度
    /// </summary>
    private float _targetX, _targetY;
    /// <summary>
    /// 是否正在旋转状态
    /// </summary>
    private bool _isRotating;
    /// <summary>
    /// 旋转角度增量
    /// </summary>
    private Vector2 _rotationDelta;
    /// <summary>
    /// X轴旋转速度(用于SmoothDamp)
    /// </summary>
    private float _xVelocity, _yVelocity;
    /// <summary>
    /// 当前Y轴旋转的最小限制(基于距离动态调整)
    /// </summary>
    private float _currMinYLimit;

    // 缩放相关
    /// <summary>
    /// 当前相机距离
    /// </summary>
    private float _currentDistance;
    /// <summary>
    /// 目标相机距离
    /// </summary>
    private float _targetDistance;
    /// <summary>
    /// 距离变化速度(用于SmoothDamp)
    /// </summary>
    private float _distanceVelocity;

    // 位置与碰撞检测
    /// <summary>
    /// 理想相机位置
    /// </summary>
    private Vector3 _desiredPosition;
    /// <summary>
    /// 上一帧相机位置
    /// </summary>
    private Vector3 _lastPosition;
    /// <summary>
    /// 上两帧相机位置
    /// </summary>
    private Vector3 _last2Position;
    /// <summary>
    /// 实际位置变化量
    /// </summary>
    private Vector3 _posDelta;
    /// <summary>
    /// 理想位置变化量
    /// </summary>
    private Vector3 _desiredDelta;
    /// <summary>
    /// 避让碰撞信息
    /// </summary>
    private RaycastHit _avoidanceHit;
    /// <summary>
    /// 是否检测到障碍物
    /// </summary>
    private bool _hasObstacleHit;

    /// <summary>
    /// 碰撞法线与速度点乘
    /// </summary>
    private float dot;
    /// <summary>
    /// 碰撞时暂时忽略输入 
    /// </summary>
    private int _avoidIgnoreInputCurrFrame = 0;
    /// <summary>
    /// 碰撞累计位移
    /// </summary>
    private Vector3 _avoidPosDelta;

    #endregion

    #region 生命周期
    /// <summary>
    /// 初始化相机参数
    /// </summary>
    private void Start()
    {
        _mainCamera = GetComponent<Camera>();

        InitRotation();
        InitDistance();
        InitBaseParams();
    }

    void Update()
    {
        if (target == null) return;
        HandleInput();          // 处理输入
    }

    /// <summary>
    /// 每帧更新相机状态(在所有Update之后执行)
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        CalculateDesiredPosition();  // 根据当前输入,计算理想位置

        dot = 0;
        // 检测碰撞并调整 
        _hasObstacleHit = CheckObstacleCollision(0);
        if (_hasObstacleHit)
        {
            _avoidIgnoreInputCurrFrame = avoidIgnoreInputFrame;
            //_distanceVelocity = 0f; // 避让时锁定距离
            //理想位置有变,则修正输入记录参数
            ReCalculateTargetParams();
        }  

        UpdateCameraTransform();    // 应用相机变换
        RecordFrameData();      // 记录帧数据
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化旋转参数
    /// </summary>
    private void InitRotation()
    {
        Vector3 initAngles = transform.eulerAngles;
        _lastX = _currentX = _targetX = initAngles.y;
        _lastY = _currentY = _targetY = initAngles.x;
        _currMinYLimit = GetDistanceBasedMinYLimit();
    }

    /// <summary>
    /// 初始化距离参数
    /// </summary>
    private void InitDistance()
    {
        float initDistance = 10f;
        _currentDistance = _targetDistance = Mathf.Clamp(initDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// 初始化基础参数
    /// </summary>
    private void InitBaseParams()
    {
        _last2Position = _lastPosition = transform.position;
        _currLookAtOffset = _targetLookAtOffset = defaultLookAtOffset;
    }
    #endregion

    #region 输入处理
    /// <summary>
    /// 统一处理所有输入
    /// </summary>
    private void HandleInput()
    {
        HandleMoveInput();      // 处理平移输入
        HandleZoomInput();      // 处理缩放输入
        HandleRotationInput();  // 处理旋转输入
    }

    /// <summary>
    /// 处理鼠标左键平移输入
    /// </summary>
    private void HandleMoveInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isMouseDown = true;
            _hasMouseMoved = false;
            _initialMousePosition = Input.mousePosition;
            _tempLookAtOffset = _currLookAtOffset;
            if (_targetLookAtOffset == defaultLookAtOffset)
                _targetLookAtBasePos = target.position;
        }
        else if (Input.GetMouseButton(0) && _isMouseDown)
        {
            Vector3 mouseDelta = Input.mousePosition - _initialMousePosition;

            // 判断鼠标是否发生有效移动（避免微小抖动被误判为拖拽）
            if (mouseDelta.magnitude > CameraFollow_World.mouseMoveThreshold)
            {
                _hasMouseMoved = true;
                if (_mainCamera != null)
                {
                    Vector3 cameraRight = _mainCamera.transform.right;
                    Vector3 cameraForward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;

                    // 计算平移偏移量(基于相机视角和距离动态调整)
                    Vector3 offsetX = cameraRight * -mouseDelta.x * moveSensitivity * _currentDistance * 0.01f;
                    Vector3 offsetZ = cameraForward * -mouseDelta.y * moveSensitivity * _currentDistance * 0.01f;
                    _targetLookAtOffset = _tempLookAtOffset + offsetX + offsetZ;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && _isMouseDown)
        {
            _isMouseDown = false;
            // 如果没有有效移动，恢复默认偏移
            if (!_hasMouseMoved)
                _targetLookAtOffset = defaultLookAtOffset;
        }

        // 平滑过渡到目标偏移
        _currLookAtOffset = Vector3.Lerp(_currLookAtOffset, _targetLookAtOffset, moveSmoothTime);
        if (_currLookAtOffset == defaultLookAtOffset)
            _targetLookAtBasePos = target.position;
    }

    /// <summary>
    /// 处理鼠标右键旋转输入
    /// </summary>
    private void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isRotating = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isRotating = false;
            _rotationDelta = Vector2.zero;
        }

        if (_isRotating)
        {
            // 计算旋转增量
            _rotationDelta.x = Input.GetAxis("Mouse X") * rotationSpeed;
            _rotationDelta.y = Input.GetAxis("Mouse Y") * verticalRotationDamping * rotationSpeed;
            _targetX += _rotationDelta.x;
            _targetY -= _rotationDelta.y;
        }

        // 限制Y轴旋转角度
        _targetY = Mathf.Clamp(_targetY, _currMinYLimit, yRotationLimits.y);
        // 平滑过渡到目标旋转角度
        _lastX = _currentX;
        _lastY = _currentY;
        var smoothTime = _avoidIgnoreInputCurrFrame > 0 ? rotationSmoothTime * avoidDampingScale : rotationSmoothTime; // 避让时加快过渡
        _currentX = Mathf.SmoothDamp(_currentX, _targetX, ref _xVelocity, smoothTime);
        _currentY = Mathf.SmoothDamp(_currentY, _targetY, ref _yVelocity, smoothTime);
    }

    /// <summary>
    /// 处理鼠标滚轮缩放输入
    /// </summary>
    private void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        // 旋转时不响应缩放
        if (scrollInput != 0 && !_isRotating)
        {
            // 计算归一化距离
            float normalizedDist = (_currentDistance - minDistance) / (maxDistance - minDistance);
            float zoomFactor = (scrollInput < 0 ? zoomSpeedCurve_up : zoomSpeedCurve_down).Evaluate(normalizedDist);


            // 计算缩放增量并限制范围
            float zoomAmount = scrollInput * zoomIntensity * zoomFactor;
            _targetDistance = Mathf.Clamp(_targetDistance - zoomAmount, minDistance, maxDistance);
        }

        // 旋转时锁定距离
        if (_isRotating)
        {
            _targetDistance = _currentDistance;
            _distanceVelocity = 0f;
        }

        // 平滑过渡到目标距离
        float smoothTime = _avoidIgnoreInputCurrFrame > 0 ? zoomDamping * avoidDampingScale : zoomDamping; // 避让时加快过渡
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _distanceVelocity, smoothTime);
        // 更新基于距离的Y轴旋转限制
        _currMinYLimit = GetDistanceBasedMinYLimit();
        // 缩放时调整Y轴旋转
        AdjustYRotationOnZoom();
    }

    /// <summary>
    /// 缩放时调整Y轴旋转角度，避免视角异常
    /// </summary>
    private void AdjustYRotationOnZoom()
    {
        //if(_hasObstacleHit) return;
        //避让时锁定距离引发的Y轴调整
        if (_avoidIgnoreInputCurrFrame > 0)
        {
            return;
        }

        // 距离变化很小时不调整
        if (Mathf.Abs(_distanceVelocity) < 0.001f) return;

        float newY = _currMinYLimit;
        // 拉近镜头时确保不低于最小角度限制
        if (_distanceVelocity > 0)
            newY = Mathf.Max(_currMinYLimit, _currentY);
        // 拉远镜头时确保不高于最小角度限制
        else if (_distanceVelocity < 0)
            newY = Mathf.Min(_currMinYLimit, _currentY);
            
        _targetY = Mathf.Clamp(newY, yRotationLimits.x, yRotationLimits.y);
    }
    #endregion

    #region 碰撞检测与避让
    
    private bool CheckObstacleCollision(int iter = 0)
    {
        if (!enableAvoidance) return false;
        if (iter >= avoidIter) return false; // 避免过多递归
        if (_desiredDelta.magnitude < 0.0001f) return false;

        // 使用球形检测避免穿透
        if (Physics.SphereCast(
            _lastPosition,
            detectSphereRadius,
            _desiredDelta,
            out _avoidanceHit,
            _desiredDelta.magnitude + extraDetectDistance,
            obstacleLayers))
        {
            dot = Vector3.Dot(_desiredDelta.normalized, _avoidanceHit.normal);
            if (showGizmos) Debug.DrawLine(_avoidanceHit.point, _avoidanceHit.normal * 5 + _avoidanceHit.point, Color.red, 0f);
            //发生碰撞则修正位置,并用新的速度再次进行检验
            CalculateAvoidPosition();
            CheckObstacleCollision(iter + 1);
            return true;
        }
        return false;
    }

 
    void CalculateAvoidPosition()
    {   

        // 碰撞后沿障碍物表面滑动(修正速度为输入速度在碰撞法线平面的投影)
        var magnitude = _desiredDelta.magnitude;
 
        _desiredDelta = Vector3.ProjectOnPlane(_desiredDelta, _avoidanceHit.normal);
 
        bool shockAvoid = false;
        // 处理极端情况,如连续避让时方向剧烈变化,则被认为可能发生震荡,并累计避让趋势作为避让方向
        if (Vector3.Dot(_desiredDelta, _avoidPosDelta) < -0.8f)
        {
            _desiredDelta = _avoidPosDelta; // 强制使用累计避让方向来逃逸震荡

            shockAvoid = true;
 
        }
        if (showGizmos) Debug.DrawLine(_lastPosition, _desiredDelta * 5 + _lastPosition, shockAvoid ? Color.green :Color.yellow, 0f);

        _desiredDelta = _desiredDelta.normalized * magnitude * avoidSpeed; //确保速率相同
        if(shockAvoid) _desiredDelta *= avoidShockSpeed; //震荡时加快避让速度
        // 计算新的理想位置
        _desiredPosition = _lastPosition + _desiredDelta;
        
    }

    void ReCalculateTargetParams()
    {
        // 根据安全位置计算新的旋转和距离目标值
        CalculateTargetParamsByPoint(_desiredPosition, out float newTargetX, out float newTargetY, out float newTargetDistance);

        // 平滑过渡到新的目标值
        _currentX = _targetX = Mathf.Lerp(_targetX, newTargetX, 1);
        _currentY = _targetY = Mathf.Lerp(_targetY, newTargetY, 1);
        _currentDistance = Mathf.Lerp(_currentDistance, newTargetDistance, 1);

        // 确保在限制范围内
        //_currentY = _targetY = Mathf.Clamp(_targetY, _currMinYLimit, yRotationLimits.y);
        _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);
    }
  
    
    /// <summary>
    /// 根据当前距离计算Y轴旋转的最小限制值
    /// 距离越近，限制越严格（防止视角过度向下）
    /// </summary>
    private float GetDistanceBasedMinYLimit()
    {
        //避让时锁定距离过渡
        if (_avoidIgnoreInputCurrFrame > 0)
        {
            _avoidIgnoreInputCurrFrame--;
            return _currMinYLimit;
        }

        //if(_hasObstacleHit) return _currMinYLimit;
        float normalizedDist = (_currentDistance - minRotateYDistance) / (maxRotateYDistance - minRotateYDistance);
        float distFactor = Mathf.Clamp01(rotateYDistanceCurve.Evaluate(normalizedDist));
        float baseMinLimit = yRotationLimits.x;
        // 基于距离计算动态最小限制
        float distMinLimit = yRotationLimits.x + (yRotationLimits.y - yRotationLimits.x) * distFactor;
        return Mathf.Clamp(distMinLimit, baseMinLimit, yRotationLimits.y);
    }
    #endregion

    #region 相机变换更新
    /// <summary>
    /// 获取相机的目标注视点
    /// </summary>
    private Vector3 GetTargetPoint()
    {
        return _targetLookAtOffset != defaultLookAtOffset 
            ? _targetLookAtBasePos + _currLookAtOffset 
            : target.position + _currLookAtOffset;
    }

    /// <summary>
    /// 计算相机的理想位置
    /// </summary>
    private Vector3 CalculateDesiredPosition()
    {
        Quaternion cameraRot = Quaternion.Euler(_currentY, _currentX, 0f);
        // 基于旋转和距离计算理想位置
        _desiredPosition = GetTargetPoint() + cameraRot * (Vector3.back * _currentDistance);
        _desiredDelta = _desiredPosition - _lastPosition; // 计算速度
        return _desiredPosition;
 
    }

    /// <summary>
    /// 更新相机的位置和旋转
    /// </summary>
    private void UpdateCameraTransform()
    {
        // 旋转时加快位置平滑速度
        float smoothFactor = _isRotating ? positionSmoothSpeed * 1.5f : positionSmoothSpeed;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, _desiredPosition, smoothFactor);
        transform.position = smoothedPosition;

        // 计算目标旋转（始终看向目标点）
        Quaternion targetRot = Quaternion.LookRotation(GetTargetPoint() - transform.position);
        // 旋转时加快旋转平滑速度
        float rotationFactor = _isRotating ? lookAtSmoothTime * 2f : lookAtSmoothTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationFactor);
    }

    /// <summary>
    /// 记录当前帧数据用于下一帧计算
    /// </summary>
    private void RecordFrameData()
    {
        _last2Position = _lastPosition;
        _posDelta = transform.position - _lastPosition;
        _lastPosition = transform.position;
        if(_hasObstacleHit) 
            _avoidPosDelta += _posDelta;
        else
            _avoidPosDelta = Vector3.zero;
        
    }
    #endregion

    #region 外部工具方法
    /// <summary>
    /// 根据目标点计算相机应有的旋转和距离参数
    /// </summary>
    /// <param name="targetPoint">目标位置</param>
    /// <param name="resultTargetX">输出X轴旋转目标值</param>
    /// <param name="resultTargetY">输出Y轴旋转目标值</param>
    /// <param name="resultTargetDistance">输出距离目标值</param>
    public void CalculateTargetParamsByPoint(
        Vector3 targetPoint, 
        out float resultTargetX, 
        out float resultTargetY, 
        out float resultTargetDistance)
    {
        if (_mainCamera == null || target == null)
        {
            resultTargetX = _targetX;
            resultTargetY = _targetY;
            resultTargetDistance = _targetDistance;
            return;
        }

        Vector3 dirToPoint = GetTargetPoint() - targetPoint;
        if (dirToPoint.sqrMagnitude < 0.0001f)
        {
            resultTargetX = _targetX;
            resultTargetY = _targetY;
            resultTargetDistance = _targetDistance;
            return;
        }

        // 计算看向目标点的旋转
        Quaternion lookRot = Quaternion.LookRotation(dirToPoint.normalized);
        Vector3 lookAngles = lookRot.eulerAngles;
        resultTargetX = NormalizeAngle(lookAngles.y);
        resultTargetY = NormalizeAngle(lookAngles.x);

        // 限制角度和距离范围
        resultTargetY = Mathf.Clamp(resultTargetY, _currMinYLimit, yRotationLimits.y);
        resultTargetDistance = Mathf.Clamp(dirToPoint.magnitude, minDistance, maxDistance);
    }

    /// <summary>
    /// 标准化角度到[-180, 180]范围
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
    #endregion

    #region Gizmos调试增强
    /// <summary>
    /// 绘制调试Gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || target == null) return;

        // 绘制目标注视点
        Gizmos.color = targetPointColor;
        DrawWireSphere(GetTargetPoint(), 0.1f);
        DrawLabel(GetTargetPoint(), "目标注视点");

        if (!Application.isPlaying) return;

        // 绘制位置轨迹相关
        Gizmos.color = desiredPosColor;
        DrawWireSphere(_desiredPosition, 0.1f);
        DrawLabel(_desiredPosition, "理想位置");
        DrawWireSphere(_lastPosition, 0.1f);
        DrawLabel(_lastPosition, "上一帧");
        DrawWireSphere(_last2Position, 0.1f);
        DrawLabel(_last2Position, "上两帧");

        // 绘制位置轨迹线
        Gizmos.color = new Color(desiredPosColor.r, desiredPosColor.g, desiredPosColor.b, 0.3f);
        Gizmos.DrawLine(_last2Position, _lastPosition);
        Gizmos.DrawLine(_lastPosition, transform.position);

        // 绘制碰撞检测相关Gizmos
        if (_desiredDelta.sqrMagnitude < 0.0001f) return;

        // 绘制检测射线
        Vector3 startPos = _lastPosition;
 
        Gizmos.color = _hasObstacleHit ? Color.red : detectRayColor;
        Gizmos.DrawLine(startPos, _desiredPosition);
 
    }

    /// <summary>
    /// 绘制线框球体（Gizmos.DrawWireSphere的替代实现）
    /// </summary>
    private void DrawWireSphere(Vector3 center, float radius)
    {
        int segments = 10;
        
        // 绘制水平圆
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            Gizmos.DrawLine(p1, p2);
        }
        
        // 绘制垂直圆
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
            Gizmos.DrawLine(p1, p2);
        }
    }

    /// <summary>
    /// 在指定位置绘制文本标签（仅编辑器中可见）
    /// </summary>
    private void DrawLabel(Vector3 position, string text)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(position, text);
#endif
    }
    #endregion
}