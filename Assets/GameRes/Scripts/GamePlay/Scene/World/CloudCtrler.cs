using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudCtrler : MonoBehaviour
{
    [Range(0, 1)]
    public float globalDensity = 1.0f;

    public float cycleTime = 5.0f;
    private float cycleStart = 0;
    
    private float targetDensity;
    private float lastDensity;
    private float currentDensity;
    // Start is called before the first frame update
    void Start()
    {
        lastDensity = targetDensity = currentDensity = globalDensity;
        RandomCycle();
    }

    // Update is called once per frame
    void Update()
    {
        float l = (Time.time - cycleStart) / cycleTime;
        currentDensity = Mathf.Lerp(lastDensity, targetDensity, l);
        if (l >= 1) RandomCycle();

        Shader.SetGlobalFloat("_GlobalCloundDensity", currentDensity);
    }

    void RandomCycle()
    {
        cycleStart = Time.time;
        lastDensity = targetDensity;
        targetDensity = Random.Range(0f, 1f);

    }
}
