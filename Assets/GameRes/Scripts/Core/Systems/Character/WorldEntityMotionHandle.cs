using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class WorldEntityMotionHandle : MonoBehaviour
{
    public float rotateSensitivity = 8f;
    public float AnimationLocomotionValueDampTime = 0.1f;
    public bool hasRootMotion = true;

    Animator entityAnimator;
    NavMeshAgent entityNavMeshAgent;
    ParentConstraint entityParentConstraint;

    private float currSpeed;
    float _animationLocomotionValue = 0f;


    public void SetDestination(Vector3 destination)
    {
        entityNavMeshAgent.SetDestination(destination);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetActiveParentConstraint(bool isActive)
    {
        if (entityParentConstraint == null) return;
        entityParentConstraint.enabled = isActive;
    }
    public void SetActiveNavAgent(bool isActive)
    {
        if (entityNavMeshAgent == null) return;
        entityNavMeshAgent.enabled = isActive;
    }

    public float GetParentAnimValue(string name)
    {
        if (entityParentConstraint == null) return 0f;
 
        Animator parentAnimator = entityParentConstraint.GetSource(0).sourceTransform.GetComponent<Animator>();
        if (parentAnimator != null)
        {
            float parentValue = parentAnimator.GetFloat(name);
            return parentValue;
        }

        return 0f;
    }

    public void SetAnimBool(string name, bool value)
    {
        if (entityAnimator == null) return;
        entityAnimator.SetBool(name, value);
    }
    
    public void SetAnimTrigger(string name)
    {
        if (entityAnimator == null) return;
        entityAnimator.SetTrigger(name);
    }

    public void SetSpeed(float value)
    {
        entityAnimator.SetFloat("Speed", value);

    }

    public void SetRotate(float value)
    {
        entityAnimator.SetFloat("Rotate", value);

    }

    // Start is called before the first frame update
    void Start()
    {
        entityNavMeshAgent = GetComponent<NavMeshAgent>();
        entityAnimator = GetComponent<Animator>();
        entityParentConstraint = GetComponent<ParentConstraint>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
        if (!hasRootMotion) UpdateMovement();
    }

    void UpdateAnimation()
    {
        if (entityAnimator == null) return;
        if (!entityNavMeshAgent.enabled)
        {
            SetSpeed(GetParentAnimValue("Speed"));
            SetRotate(GetParentAnimValue("Rotate"));
            return;
        }

        // 获取移动速度（0-1范围）
        currSpeed = entityNavMeshAgent.speed == 0 ? 0 : Mathf.SmoothDamp(currSpeed, entityNavMeshAgent.velocity.magnitude / entityNavMeshAgent.speed, ref _animationLocomotionValue, AnimationLocomotionValueDampTime);
        SetSpeed(currSpeed);

        // 处理转向动画
        if (entityNavMeshAgent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(entityNavMeshAgent.velocity.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSensitivity * Time.deltaTime);

            // 计算转向角度（用于动画参数）
            float rotateAngle = Quaternion.Angle(transform.rotation, targetRot);
            if (Mathf.Abs(rotateAngle) > 1f)
                SetRotate(rotateAngle / 90.0f);
            else
                SetRotate(0);
        }
        else
        {
            SetRotate(0);
        }
    }


    // 根运动处理（保持不变）
    void OnAnimatorMove()
    {
        if (!hasRootMotion) return;
        if (!entityNavMeshAgent.enabled) return;
        // if (!entityNavMeshAgent.isOnNavMesh) return;
        // if (entityAnimator.GetBool("Ride"))
        // {
        //     entityNavMeshAgent.speed = 5;
        //     transform.position = entityNavMeshAgent.nextPosition;
        // }
        // else
        //{
        // 用导航位置同步角色位置，高度加个Lerp弹性处理
        var pos = entityNavMeshAgent.nextPosition;
        pos.y = Mathf.Lerp(transform.position.y, entityNavMeshAgent.nextPosition.y, 0.1f);
        transform.position = pos;
        // 同步动画的速度大小给导航
        entityNavMeshAgent.speed = entityAnimator.velocity.magnitude;
        //}

    }

    void UpdateMovement()   
    {
        if (!entityNavMeshAgent.enabled) return;

        // 用导航位置同步角色位置，高度加个Lerp弹性处理
        var pos = entityNavMeshAgent.nextPosition;
        pos.y = Mathf.Lerp(transform.position.y, entityNavMeshAgent.nextPosition.y, 0.1f);
        transform.position = pos;

    }
}
