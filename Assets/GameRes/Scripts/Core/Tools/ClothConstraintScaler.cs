using UnityEngine;

[RequireComponent(typeof(Cloth))]
public class ClothConstraintScaler : MonoBehaviour
{
    [Range(0.1f, 1f)] // 缩放因子：1=不变，0.5=缩小50%
    public float scaleFactor = 0.8f; // 建议从0.8开始测试
 
    // 在编辑器中点击按钮执行调整（无需进入运行模式）
    [ContextMenu("Scale All Max Distance Constraints")]
    public void ScaleMaxDistanceConstraints()
    {

        var cloth = GetComponent<Cloth>();
        if (cloth == null)
        {
            Debug.LogError("未找到Cloth组件！");
            return;
        }

        var currentConstraints = cloth.coefficients;
        // 获取当前所有顶点的Max Distance约束
        if (currentConstraints.Length == 0)
        {
            Debug.LogError("未检测到任何Max Distance约束，请先设置约束！");
            return;
        }

        // 统一下调：每个顶点的约束值乘以缩放因子
        for (int i = 0; i < currentConstraints.Length; i++)
        {
            // currentConstraints[i].maxDistance *= scaleFactor;
            if (Mathf.Approximately(currentConstraints[i].maxDistance, 0.001f))
            {
                currentConstraints[i].maxDistance = 0.02f;
            }
        }

        cloth.coefficients = currentConstraints;
        Debug.Log($"已将所有顶点的Max Distance统一缩放至 {scaleFactor} 倍");
    }
}
