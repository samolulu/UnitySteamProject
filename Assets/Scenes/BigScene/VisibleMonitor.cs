using UnityEngine;

public class VisibleMonitor : MonoBehaviour
{
    public bool isVisible = true;
 
	public NPCFollow npc;
 
	// 监测可见性变化
	private void OnBecameVisible()
	{
		isVisible = true;
		if (npc) npc.OnBecameVisible();
	}

	private void OnBecameInvisible()
	{
		isVisible = false;
		if (npc) npc.OnBecameInvisible();
    }
}
