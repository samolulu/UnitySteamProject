using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Animations;

public class ClickToMove : MonoBehaviour
{
    [Header("核心引用")]
    public WorldEntityMotionHandle player; // 主角 
    public WorldEntityMotionHandle playerHorse; // 主角马

    public Camera mainCamera; // 主摄像机

    public LayerMask terrainLayer; // 地形图层
    public LayerMask waterLayer; // 水面图层
    public float rayDistance = Mathf.Infinity;

    [Header("移动设置")]
    public bool playerRiding = false;
    public float maxSlopeAngle = 30f; // 最大可通行坡度


    [Header("不可通行提示")]
    public GameObject forbiddenUIPrefab; // 不可通行图标预制体
    public Transform uiParent; // Overlay模式Canvas（替换原worldSpaceCanvas）
    public Vector2 uiScreenOffset = new Vector2(0, 30); // UI在屏幕上的偏移（避免遮挡鼠标）
    public float uiDisplayTime = 2f; // 图标显示时长
    public float minVisibleDistance = 50f; // 最大可见距离（超过则隐藏）


    // 新增：管理活跃的UI标记
    private UIWorldMarker? activeMarker = null;
    bool _isMouseDown = false;
    private bool _hasMouseMoved;
    private Vector3 _initialMousePosition;

    public WorldEntityMotionHandle GetPlayerMotionHandle()
    {
        return playerRiding ? playerHorse : player;
    }

    public void SetPlayerRiding(bool isRiding)
    {
        bool change = playerRiding != isRiding;
        playerRiding = isRiding;
        if (playerRiding)
        {
            playerHorse.transform.position = player.transform.position;
            playerHorse.transform.forward = player.transform.forward;
        }
        playerHorse.gameObject.SetActive(isRiding);

        player.SetAnimBool("Ride", isRiding);
        if(!isRiding && change) player.SetAnimTrigger("UnRide");
        player.SetActiveParentConstraint(isRiding);
        player.SetActiveNavAgent(!isRiding);
        player.gameObject.GetComponent<PositionConstraint>().enabled = isRiding;
    }


    void Start()
    {
        // 初始化角色状态
        SetPlayerRiding(false);

        // 调整：获取Overlay模式Canvas（默认找名为Canvas的对象）
        if (uiParent == null)
            uiParent = GameObject.Find("Canvas").transform;
    }

    void Update()
    {
        HandleClickInput();

        UpdateUIMarkers(); // 新增：实时更新UI位置

        if(Input.GetKeyDown(KeyCode.F))
        {
            SetPlayerRiding(!playerRiding);
        }
    }

    void HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isMouseDown = true;
            _hasMouseMoved = false;
             _initialMousePosition = Input.mousePosition;
 
        }
        else if (Input.GetMouseButton(0) && _isMouseDown)
        {
            Vector3 mouseDelta = Input.mousePosition - _initialMousePosition;

            // 判断鼠标是否发生有效移动（避免微小抖动被误判为拖拽）
            if (mouseDelta.magnitude > CameraFollow_World.mouseMoveThreshold) // 阈值5像素：可根据需求调整
                _hasMouseMoved = true;
        }

        if (!_hasMouseMoved && Input.GetMouseButtonUp(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // 关键修改：只检测地形和水面图层，忽略其他所有图层
            int targetLayers = terrainLayer | waterLayer;
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayers))
            {
                // 检测水面
                if (((1 << hit.collider.gameObject.layer) & waterLayer) != 0)
                {
                    ShowClickPointUI(hit, false);
                    return;
                }

                // 检测地形
                if (((1 << hit.collider.gameObject.layer) & terrainLayer) != 0)
                {
                    Vector3 realGroundPos = hit.point;


                    // 获取碰撞点所属的Terrain（避免多Terrain场景错误）
                    Terrain hitTerrain = hit.collider.GetComponent<Terrain>();

                    if (hitTerrain == null)
                    { ///点到桥梁之类的非地形通道

                        //ShowClickPointUI(hit, false);
                        //Debug.Log("未找到Terrain组件");
                        //return;
                    }
                    else
                    {
                        // 获取碰撞点的水平坐标（X/Z），忽略Y（树木高度）
                        Vector3 horizontalPos = new Vector3(hit.point.x, 0, hit.point.z);

                        // 采样该水平坐标下的Terrain地面真实高度
                        // SampleHeight返回Terrain本地高度，需加上Terrain的世界Y坐标
                        float groundY = hitTerrain.SampleHeight(horizontalPos) + hitTerrain.transform.position.y;
                        // 构建真实的地面目标点（替换树木高度的Y）
                        realGroundPos = new Vector3(horizontalPos.x, groundY, horizontalPos.z);
                        if (!IsSlopePassable(realGroundPos))
                        {
                            ShowClickPointUI(hit, false);
                            Debug.Log("坡度太大，不可通行");
                            return;
                        }
                    }

                    // 检查导航是否可达
                    if (NavMesh.CalculatePath(player.GetPosition(), realGroundPos, NavMesh.AllAreas, new NavMeshPath()))
                    {
                        MoveToTarget(realGroundPos);
                        ShowClickPointUI(hit, true);
                    }
                    else
                    {
                        ShowClickPointUI(hit, false);
                        Debug.Log("目标位置不可到达");
                        return;
                    }


                }
            }
        }
    }

    // 修改：显示不可通行UI（Overlay模式，固定屏幕大小，跟随场景位置）
    void ShowClickPointUI(RaycastHit hit, bool reachable = false)
    {
 
        if (activeMarker == null)
        {
            if (forbiddenUIPrefab == null || uiParent == null) return;

            // 实例化UI到Overlay Canvas
            GameObject ui = Instantiate(forbiddenUIPrefab, uiParent);
            RectTransform rect = ui.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("UI预制体缺少RectTransform组件！");
                Destroy(ui);
                return;
            }


            // 记录点击的世界位置和UI引用
            activeMarker = new UIWorldMarker(rect);
        }
 
        //重置位置和计时
        activeMarker.worldPosition = hit.point;
        //activeMarker.direction = -hit.normal;
        activeMarker.hideTime = Time.time + uiDisplayTime;

        activeMarker.SetReachableState(reachable);
        activeMarker.SetVisibility(true);

    }

    // 新增：实时更新所有UI标记的屏幕位置
    void UpdateUIMarkers()
    {
        if (activeMarker == null) return;
        if (!activeMarker.isVisible) return;

        // 过期
        if (!activeMarker.reachable && Time.time > activeMarker.hideTime)
        {
            activeMarker.SetVisibility(false);
            return;
        }

        var worldPos = activeMarker.worldPosition;
        // 检查距离镜头是否过远
        float distance = Vector3.Distance(worldPos, mainCamera.transform.position);
        if (distance > minVisibleDistance)
        {
            activeMarker.SetVisibility(false);
            return;
        }

        //检查是否玩家已经到达
        if (activeMarker.reachable)
        {
            var currPos = GetMotionHandlePos();
            if (Vector3.Distance(new Vector3(currPos.x, 0, currPos.z), new Vector3(currTargetPos.x, 0, currTargetPos.z)) < 1f)
            {
                activeMarker.SetVisibility(false);
                return;
            }
        }
 

        // 转换3D位置到屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 检查是否在屏幕范围内
        bool isOnScreen = screenPos.z > 0 && // 在摄像机前方
                            screenPos.x > 0 && screenPos.x < Screen.width &&
                            screenPos.y > 0 && screenPos.y < Screen.height;

        activeMarker.SetVisibility(isOnScreen);
        if (!isOnScreen) return;

        // 转换为UI局部坐标并应用偏移
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiParent.GetComponent<RectTransform>(),
            screenPos + (Vector3)uiScreenOffset,
            null, // Overlay模式无需摄像机
            out Vector2 localPos
        );

        activeMarker.rectTransform.localPosition = localPos;

    }

    // 移除原FaceCamera方法（Overlay模式无需面向摄像机）



    // void OnAnimatorMove()
    // {
    //     //if (!agent.isOnNavMesh) return;
    //     if (agent.isOnNavMesh && playerAnimator.GetFloat("Speed") != 0)
    //     {
    //         var pos = agent.nextPosition;
    //         pos.y = Mathf.Lerp(transform.position.y, agent.nextPosition.y, 0.1f);
    //         transform.position = pos;
    //         // 同步动画的速度大小给导航
    //         agent.speed = Mathf.Max(0.01f, playerAnimator.velocity.magnitude);
    //     }
    //     else
    //     {
    //         // 1. 让动画根运动驱动角色位置和旋转
    //         // 应用动画的根运动位移
    //         transform.position += playerAnimator.deltaPosition;

    //     }

    //     if (playerAnimator.GetFloat("Rotate") == 0)
    //     {
    //         // 应用动画的根运动旋转
    //         transform.rotation = transform.rotation * playerAnimator.deltaRotation;
    //     }



    // }

    // 检查地形坡度（保持不变）
    private bool IsSlopePassable(Vector3 hitPoint)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return false;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int x = (int)(((hitPoint.x - terrainPos.x) / terrainData.size.x) * terrainData.heightmapResolution);
        int y = (int)(((hitPoint.z - terrainPos.z) / terrainData.size.z) * terrainData.heightmapResolution);

        x = Mathf.Clamp(x, 0, terrainData.heightmapResolution - 1);
        y = Mathf.Clamp(y, 0, terrainData.heightmapResolution - 1);

        Vector3 normal = terrainData.GetInterpolatedNormal(
            x / (float)terrainData.heightmapResolution,
            y / (float)terrainData.heightmapResolution
        );

        return Vector3.Angle(normal, Vector3.up) <= maxSlopeAngle;
    }

    // 移动到目标点（保持不变）
    Vector3 currTargetPos;
    private void MoveToTarget(Vector3 targetPos)
    {
        currTargetPos = targetPos;
        //targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + Terrain.activeTerrain.transform.position.y;
        if (!playerRiding) player.SetDestination(targetPos);
        if (playerRiding) playerHorse.SetDestination(targetPos);
        if (playerRiding) player.SetAnimTrigger("RideMove");
    }

    Vector3 GetMotionHandlePos()
    {
        var handle = GetPlayerMotionHandle();
        return handle.GetPosition();
    }

    // 新增：辅助类管理UI标记
    private class UIWorldMarker
    {
        public Vector3 worldPosition; // 场景中的3D位置
        //public Vector3 direction; // 场景中的3D方向
        public RectTransform rectTransform; // UI引用
        public float hideTime; // 隐藏时间
        public bool isVisible = true; // 是否可见
        public bool reachable = false; // 当前显示为可达

        Image image;

        public UIWorldMarker(RectTransform rectTransform)
        {
            worldPosition = Vector3.zero;
            //direction = Vector3.down;
            this.rectTransform = rectTransform;
            hideTime = 0;
            isVisible = true;
            reachable = false;
            image = rectTransform.Find("Image").GetComponent<Image>();
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            rectTransform.gameObject.SetActive(visible);
            //image.transform.forward = direction;
        }

        public void SetReachableState(bool reachable)
        {
            this.reachable = reachable;
            // 根据可达性设置UI标记的颜色或其他属性
            Color color = reachable ? Color.green : Color.red;
            image.color = color;
        }
    }


#region debug
    [ContextMenu("切换骑乘状态", false)]
    private void Test_SwitchRidingState()
    {
        bool isRiding = playerHorse.gameObject.activeSelf;
        SetPlayerRiding(!isRiding);
    }
#endregion

}