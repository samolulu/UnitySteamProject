using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Reflection;

/// <summary>
/// 重载MenuItem"GameObject/UI”下部分UI组件的创建逻辑，主要是处理Graph RaycastTarget 的默认值
/// </summary>
[InitializeOnLoad]
static internal class UIMenuOptionsExtend
{
    // 缓存反射获取的方法和资源
    private static MethodInfo _getDefaultResourceMethod;
    private static MethodInfo _placeUIElementRootMethod;
    private static object _standardResources;
    private static bool _initializationFailed;

    static UIMenuOptionsExtend()
    {
        EditorApplication.update += InitializeOnEditorUpdate;
    }

    // 延迟初始化，避免在Unity完全加载前执行反射操作
    private static void InitializeOnEditorUpdate()
    {
        if (_initializationFailed || _getDefaultResourceMethod != null)
        {
            EditorApplication.update -= InitializeOnEditorUpdate;
            return;
        }

        Initialize();
        EditorApplication.update -= InitializeOnEditorUpdate;
    }

    private static void Initialize()
    {
        try
        {
            // 获取UnityEditor.UI程序集
            Assembly uiEditorAssembly = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "UnityEditor.UI")
                {
                    uiEditorAssembly = assembly;
                    break;
                }
            }

            if (uiEditorAssembly == null)
            {
                Debug.LogError("无法找到UnityEditor.UI程序集");
                _initializationFailed = true;
                return;
            }

            // 获取所需的方法
            var menuOptionsType = uiEditorAssembly.GetType("UnityEditor.UI.MenuOptions");
            _getDefaultResourceMethod = menuOptionsType.GetMethod(
                "GetStandardResources", 
                BindingFlags.NonPublic | BindingFlags.Static
            );
            
            _placeUIElementRootMethod = menuOptionsType.GetMethod(
                "PlaceUIElementRoot", 
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // 缓存标准资源，避免每次创建UI都反射调用
            _standardResources = _getDefaultResourceMethod?.Invoke(null, null);

            if (_getDefaultResourceMethod == null || _placeUIElementRootMethod == null || _standardResources == null)
            {
                Debug.LogError("无法获取必要的UI创建方法或资源");
                _initializationFailed = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化UI菜单扩展时出错: {e}");
            _initializationFailed = true;
        }
    }

    // 禁用内置Text菜单项
    [MenuItem("GameObject/UI/Legacy/Text", true)]
    private static bool DisableBuiltinTextMenuItem()
    {
        return false;
    }

    // 添加自定义Text菜单项
    // [MenuItem("GameObject/UI/Legacy/Text - ", false, 2000)]
    // static public void AddText(MenuCommand menuCommand)
    // {
    //     if (CreateUIElement("Text", menuCommand, DefaultControls.CreateText, out var go))
    //     {
    //         // 可以在这里添加Text组件的自定义设置
    //     }
    // }

    // 禁用内置Image菜单项
    [MenuItem("GameObject/UI/Image", true)]
    private static bool DisableBuiltinImageMenuItem()
    {
        return false;
    }

    // 添加自定义Image菜单项，默认关闭raycastTarget
    [MenuItem("GameObject/UI/Image - ", false, 2000)]
    static public void AddImage(MenuCommand menuCommand)
    {
        if (CreateUIElement("Image", menuCommand, DefaultControls.CreateImage, out var go))
        {
            // 关闭Image的射线检测
            if (go != null && go.TryGetComponent<Image>(out var image))
            {
                image.raycastTarget = false;
            }
        }
    }

    // 通用UI元素创建方法
    private static bool CreateUIElement(string elementType, MenuCommand menuCommand, 
        Func<DefaultControls.Resources, GameObject> createFunc, out GameObject? go)
    {
		go = null;
        if (_initializationFailed || _placeUIElementRootMethod == null || _standardResources == null)
		{
			Debug.LogError($"无法创建{elementType}，初始化失败或缺少必要资源");
			return false;
		}

        try
        {
            // 创建UI元素
            var resources = (DefaultControls.Resources)_standardResources;
            go = createFunc(resources);
            
            // 放置UI元素
            _placeUIElementRootMethod.Invoke(null, new object[] { go, menuCommand });
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"创建{elementType}时出错: {e}");
            return false;
        }
    }
}