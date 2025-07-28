using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Profiling;
using System.Text;
using TMPro;


public class FPSShow : MonoBehaviour
{
    public float UpdateInterval = 1.0F;
    [Header("1%low 统计帧数")]
    public int  UpdateFrame_low1 = 5000;
    TextMeshProUGUI text;
    float timePast;
    float frames;
    float fps;
    string statsText;
    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder verticesRecorder;
    private float f_Fps_low1;
    private List<float> frameTimeList;
    StringBuilder sb;
    // Start is called before the first frame update
    void Start()
    {
  
#if !UNITY_EDITOR
        QualitySettings.vSyncCount = 1;     // 垂直同步
#if UNITY_ANDROID
        Application.targetFrameRate = 30;   // 帧率
        Screen.SetResolution((int)(1080* (float)Screen.width/ Screen.height), 1080, FullScreenMode.FullScreenWindow);
#else
        Application.targetFrameRate = 60;   // 帧率
#endif
       
#endif
        text = GetComponent<TextMeshProUGUI>();
        frameTimeList = new List<float>();
        sb = new StringBuilder(500);
    }
    void OnEnable()
    {
        fps = 30.0f;
        frames = 0.0f;
        timePast = 0.0f;
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
    }
    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
    }
 
    // Update is called once per frame
    void Update()
    {
        
        frames++;
        //1% low FPS
        float delta = Time.unscaledDeltaTime;
        frameTimeList.Add(delta);
        int totalCount = frameTimeList.Count;
        if (totalCount >= UpdateFrame_low1)
        {
            frameTimeList.Sort();
            float totalLowTime = 0.0f;
            int lowCount = (int)Mathf.Ceil(totalCount * 0.01f);
            for (int i = totalCount - 1; i > totalCount - lowCount - 1; i--)
            {
                totalLowTime += frameTimeList[i];
            }
            f_Fps_low1 = lowCount / totalLowTime;
           
            frameTimeList.Clear();
        }
        timePast += delta;
        if (timePast > UpdateInterval)
        {
            fps = frames / timePast;
            frames = 0;
            timePast = 0.0F;
            sb.Clear();
            //sb.AppendLine($"CS: {SystemInfo.supportsComputeShaders.ToString()}");
            sb.AppendLine($"FPS: {fps.ToString("00.00")}");
            sb.AppendLine($"1%low FPS: {f_Fps_low1.ToString("00.00")}");
            // if (setPassCallsRecorder.Valid)
            //     sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
            // if (drawCallsRecorder.Valid)
            //     sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
            // if (verticesRecorder.Valid)
            //     sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
            statsText = sb.ToString();
            text.text = statsText;
        }
 
    }
}
