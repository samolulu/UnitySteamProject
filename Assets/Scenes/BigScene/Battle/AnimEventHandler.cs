using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimEventHandler : MonoBehaviour
{
	List<Attacker> attackers;
	EffectHandler effectHandler;

	// Start is called before the first frame update
	void Start()
	{
		attackers = GetComponentsInChildren<Attacker>().ToList();
		effectHandler = GetComponent<EffectHandler>();
    }

    public void EnableAttack()
    {
		attackers.ForEach(d => d.EnableAttack());
    }
 
    public void DisableAttack()
    {
 		attackers.ForEach(d => d.DisableAttack());
    }
	
    public void PlayFootstep()
	{
		effectHandler.PlayFootstep();
	}
}
