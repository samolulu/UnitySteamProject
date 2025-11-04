using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class FisheyeLens : MonoBehaviour
{ 
    [Header("相机设置")]
    [Tooltip("要监测高度的相机")]
    public Camera targetCamera;
    
    [Header("触发设置")]
    [Tooltip("开始启用鱼眼效果的高度阈值")]
    public float activationHeight = 10.0f;
    [Tooltip("达到最大鱼眼效果的高度")]
    public float maxEffectHeight = 50.0f;
    
    [Header("鱼眼效果参数")]
    [Tooltip("最大畸变强度")]
    [Range(0, 10)] public float maxIntensity = 5.0f;
    [Tooltip("最大缩放值")]
    [Range(1, 5)] public float maxScale = 2.0f;
    
    private Volume postProcessVolume;
    private LensDistortion lensDistortion;
    private bool isEffectActive = false;

    private void Awake()
    {
        // 获取后期处理组件
        postProcessVolume = GetComponent<Volume>();
        
        // 获取或添加LensDistortion效果
        if (!postProcessVolume.profile.TryGet(out lensDistortion))
        {
            lensDistortion = postProcessVolume.profile.Add<LensDistortion>();
        }
        
        // 初始禁用鱼眼效果
        lensDistortion.active = false;
        lensDistortion.intensity.value = 0;
        lensDistortion.scale.value = 1;
        
        // 如果未指定相机，默认使用主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;
        
        // 获取当前相机高度
        float currentHeight = targetCamera.transform.position.y;
        
        // 计算高度比例（0到1之间）
        float heightRatio = Mathf.Clamp01((currentHeight - activationHeight) / (maxEffectHeight - activationHeight));
        
        // 激活或禁用效果
        if (currentHeight >= activationHeight)
        {
            if (!isEffectActive)
            {
                lensDistortion.active = true;
                isEffectActive = true;
            }
            
            // 根据高度比例设置鱼眼参数
            lensDistortion.intensity.value = Mathf.Lerp(0, maxIntensity, heightRatio);
            lensDistortion.scale.value = Mathf.Lerp(1, maxScale, heightRatio);
        }
        else if (isEffectActive)
        {
            // 当低于激活高度时，关闭效果
            lensDistortion.active = false;
            lensDistortion.intensity.value = 0;
            lensDistortion.scale.value = 1;
            isEffectActive = false;
        }
    }
}
