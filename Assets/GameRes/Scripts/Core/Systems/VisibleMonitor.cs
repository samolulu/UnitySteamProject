using UnityEngine;

public class VisibleMonitor : MonoBehaviour
{
    public bool isVisible = true;
 
	public Motion motion;
	
	void Awake()
	{
		motion = GetComponentInParent<Motion>();
	}

	// 监测可见性变化
	private void OnBecameVisible()
	{
		isVisible = true;
		if (motion) motion.OnBecameVisible();
	}

	private void OnBecameInvisible()
	{
		isVisible = false;
		if (motion) motion.OnBecameInvisible();
    }
}
