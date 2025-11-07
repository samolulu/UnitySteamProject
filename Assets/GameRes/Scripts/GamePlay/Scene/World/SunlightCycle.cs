using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[ExecuteAlways()]
public class SunlightCycle : MonoBehaviour
{
    [Header("时间控制")]
    [Tooltip("一天的总时长（秒）")]
    public float dayDuration = 180f; // 默认3分钟一天

    [Tooltip("当前时间（0-1，0=午夜，0.5=中午，1=午夜）")]
    [Range(0f, 1f)]
    public float currentTimeOfDay = 0.25f; //用于光线表现的时间
    private float timeOfDayTick = 0f; // 真实游戏世界时间
    public int timeOfDaySteps = 10; // 分段数
    public float speedOfStep = 5.0f; //段与段之间过渡提速

    // 日出、日落的时间点（0-1范围）
     [Header("日出、日落的时间点")]
    public Vector2 sunupdownTime = new Vector2(0.2f, 0.8f);
    float sunriseTime { get { return sunupdownTime.x; } }
    float sunsetTime { get { return sunupdownTime.y; } }
 

    [Tooltip("是否自动运行")]
    public bool autoUpdate = true;

    public Light sunLight; // 白天光源
    public Light moonLight; // 夜晚光源
 

    [Header("后期处理设置")]
    public Volume postProcessVolume;
    private Fog volumeFog;


    Material skyboxMaterial;
    private float skyboxFogIntensityBase;

    // 在Inspector面板中可编辑的渐变
    [Header("光线颜色")]
    public Gradient lightColorGradient;

    [Header("天空盒颜色")]
    public Gradient skyColorGradient;

    [Header("全局雾颜色")]
    public Gradient fogGlobalColorGradient;

    [Header("天空盒亮度")]
    public AnimationCurve skyExposureCurve; 
    public float skyExposureScale = 1f;

    [Header("体积雾强度")]
    public AnimationCurve fogVolumeDensityCurve; 
    public float fogVolumeDensityScale = 1f;

    [Header("日光强度")]
    public AnimationCurve lightIntensityCurve_sun; 
    public float lightIntensityScale_sun = 1f;
 
    [Header("月光强度")]
    public AnimationCurve lightIntensityCurve_moon; 
    public float lightIntensityScale_moon = 1f;

    [Header("日光高度")]
    public AnimationCurve lightHeightCurve_sun; 
 
    [Header("月光高度")]
    public AnimationCurve lightHeightCurve_moon; 

    [Header("日光角度")]
    public AnimationCurve lightAngleCurve_sun; 
 
    [Header("月光角度")]
    public AnimationCurve lightAngleCurve_moon;

    [Header("全局环境光强度")]
    public AnimationCurve ambientIntensityCurve; 
    public float ambientIntensityScale = 1f;

    [Header("全局反射光强度")]
    public AnimationCurve reflectionIntensityCurve;
    public float reflectionIntensityScale = 1f;
 
 


    [ContextMenu("刷新时间节点")]
    void ResetTimeNodes()
    {
 
        var newFrameTimes = new float[] { 0f, sunriseTime, 0.5f, sunsetTime, 1f };
        lightColorGradient.ResetKeyframeTime(newFrameTimes);
        skyColorGradient.ResetKeyframeTime(newFrameTimes);
        fogGlobalColorGradient.ResetKeyframeTime(newFrameTimes);
        
        skyExposureCurve.ResetKeyframeTime(newFrameTimes);
        fogVolumeDensityCurve.ResetKeyframeTime(newFrameTimes);
    }

    private void OnEnable()
    {
        if (sunLight == null || moonLight == null)
        {
            Debug.LogError("SunlightCycle requires both sunLight and moonLight to be assigned!", this);
            enabled = false;
            return;
        }

        // 确保是直射光
        if (sunLight.type != LightType.Directional)
        {
            Debug.LogWarning("SunlightCycle should be attached to a Directional Light!", this);
        }

        // 如果未指定天空盒材质，使用当前天空盒
        if (skyboxMaterial == null)
        {
            skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }
        // 获取或添加Fog效果
        if (!postProcessVolume.profile.TryGet(out volumeFog))
        {
            volumeFog = postProcessVolume.profile.Add<Fog>();
        }
        skyboxFogIntensityBase = RenderSettings.fogDensity;

        ResetTimeNodes();
    }

    private void Update()
    {
        if (autoUpdate)
        {
            // 更新时间（循环）
            var delta = Time.deltaTime / dayDuration;
            timeOfDayTick += delta;
            if (timeOfDayTick >= 1f)
            {
                timeOfDayTick -= 1f;
            }

            // 限制时间变化步进，避免过快跳跃
            var target = StepTimeOfDay(timeOfDayTick );
            if (target < currentTimeOfDay && currentTimeOfDay < 1f) target = 1f; // 先完整跨过午夜点
            
            //用几倍的速度过渡到目标时间
            if(target != currentTimeOfDay)
            {
                currentTimeOfDay += delta * speedOfStep;
                currentTimeOfDay = Mathf.Min(currentTimeOfDay, target);
            }
            if (currentTimeOfDay >= 1f)
            {
                currentTimeOfDay = 0f;
            }
        }

        // 更新阳光状态
        UpdateSunlight();

        UpdateAmbient();
    }

    private void UpdateSunlight()
    {
        // 根据时间读取光线参数
        float angle = lightHeightCurve_sun.Evaluate(currentTimeOfDay);
        float angle_moon = lightHeightCurve_moon.Evaluate(currentTimeOfDay);
        Color color = lightColorGradient.Evaluate(currentTimeOfDay);
        float intensity = lightIntensityCurve_sun.Evaluate(currentTimeOfDay) * lightIntensityScale_sun;
        float intensity_moon = lightIntensityCurve_moon.Evaluate(currentTimeOfDay) * lightIntensityScale_moon;

        // 角度
        var sunAngleY = lightAngleCurve_sun.Evaluate(currentTimeOfDay);// Mathf.Lerp(sunAngleY_min, sunAngleY_max, (Mathf.Clamp(currentTimeOfDay, sunriseTime, sunsetTime) - sunriseTime) / (sunsetTime - sunriseTime));
        var moonAngleY = lightAngleCurve_moon.Evaluate(currentTimeOfDay); //Mathf.Lerp(moonAngleY_min, moonAngleY_max, (Mathf.Clamp(currentTimeOfDay < 0.5f ? currentTimeOfDay + 1 : currentTimeOfDay, sunsetTime, sunriseTime + 1f) - sunsetTime) / (sunriseTime + 1f - sunsetTime));

        sunLight.transform.rotation = GetQuaternion(angle, sunAngleY);
        sunLight.color = color;
        sunLight.intensity = intensity;

        moonLight.transform.rotation = GetQuaternion(angle_moon, moonAngleY);
        moonLight.color = color;
        moonLight.intensity = intensity_moon;

        //阴影
        bool isDay = currentTimeOfDay >= sunriseTime && currentTimeOfDay <= sunsetTime;
        sunLight.shadows = isDay ? LightShadows.Soft : LightShadows.None;
        moonLight.shadows = !isDay ? LightShadows.Soft : LightShadows.None;

        //高光控制
        var waterLayer = LayerMask.NameToLayer("Water") << 2;
        if (isDay)
        {
            sunLight.cullingMask |= waterLayer;
            moonLight.cullingMask &= ~waterLayer;
        }
        else
        {
            sunLight.cullingMask &= ~waterLayer;
            moonLight.cullingMask |= waterLayer;
        }
 
        //全局光源
        SetRenderSunSource(isDay ? sunLight : moonLight);
    }

    private void UpdateAmbient()
    {
        if (skyboxMaterial == null) return;

        Color skyColor = skyColorGradient.Evaluate(currentTimeOfDay);
        float skyExposure = skyExposureCurve.Evaluate(currentTimeOfDay) * skyExposureScale;
        Color fogColor = fogGlobalColorGradient.Evaluate(currentTimeOfDay);
        float fogDensity = fogVolumeDensityCurve.Evaluate(currentTimeOfDay) * fogVolumeDensityScale;
 
        // 应用天空盒颜色 
        if (skyboxMaterial.HasProperty("_Tint"))
            skyboxMaterial.SetColor("_Tint", skyColor);

        if (skyboxMaterial.HasProperty("_Exposure"))
            skyboxMaterial.SetFloat("_Exposure", skyExposure);


        // 更新雾颜色以匹配天空
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = skyboxFogIntensityBase * fogDensity;
        RenderSettings.ambientIntensity = ambientIntensityCurve.Evaluate(currentTimeOfDay) * ambientIntensityScale;
        RenderSettings.reflectionIntensity = reflectionIntensityCurve.Evaluate(currentTimeOfDay) * reflectionIntensityScale;
        volumeFog.alphaMultiplier.value = fogDensity;
    }

    // 可以通过代码设置特定时间
    public void SetTimeOfDay(float time)
    {
        timeOfDayTick = Mathf.Clamp01(time);
        currentTimeOfDay = StepTimeOfDay(timeOfDayTick );
        UpdateSunlight();

        UpdateAmbient();
    }

    float StepTimeOfDay(float time)
    {
        return Mathf.Floor(time * timeOfDaySteps) / timeOfDaySteps;
    }

    Light currentRenderSunSource;
    public void SetRenderSunSource(Light light)
    {
        if(currentRenderSunSource == light) return;
        currentRenderSunSource = light;
        RenderSettings.sun = light;
    }


    // float LerpAngle(float startAngle, float targetAngle, float duration)
    // {
    //     float angle = startAngle;
    //     if (Mathf.Abs(targetAngle - startAngle) < 180)
    //     {
    //         angle = Mathf.LerpAngle(startAngle, targetAngle, duration);
    //     }
    //     return Mathf.Clamp(angle, 5, 175);
    // }

    Quaternion GetQuaternion(float angle, float yAngle)
    {
        // 1. 先绕世界Y轴旋转（确定太阳方位）
        Quaternion yRotation = Quaternion.Euler(0, yAngle, 0);
        // 2. 再绕局部X轴旋转（确定太阳高度）
        Quaternion xRotation = Quaternion.Euler(angle, 0, 0);
        // 3. 组合旋转（先Y后X）
        return yRotation * xRotation;
    }

    void OnValidate()
    {
        SetTimeOfDay(currentTimeOfDay);

    }
}
