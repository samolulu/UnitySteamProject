using UnityEngine;
using System.Collections.Generic;
using System;

public class FormationSystem : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount = 8;
    [SerializeField] private float formationSpacing = 3.0f;
    [SerializeField] private float followDistance = 6.0f;
    [SerializeField] private float rotationSpeed = 10f; // 提高旋转速度，避免阵型偏移
    [SerializeField] private float stoppingDistance = 3.0f;
    [SerializeField] private string playerSpeedParam = "Speed";

    private List<NPCFollow> npcs = new List<NPCFollow>();
    private List<Vector3> formationPositions = new List<Vector3>();
    private List<int> npcLayers = new List<int>(); // 记录每个NPC的阵型层级
    private Animator playerAnimator;
    private bool isPlayerMoving;

    void Start()
    {
        playerAnimator = player.GetComponent<Animator>();
        CreateFormation();
    }

    void Update()
    {
		if (playerAnimator == null) return;
        // 判断主角是否在移动（通过速度参数）
		isPlayerMoving = Math.Abs(playerAnimator.GetFloat(playerSpeedParam)) > 0.02f;
        
        if(Time.frameCount % 5 == 0)UpdateFormationPositions();
        SyncNPCStates();
    }

    // 创建三角形阵型，并分配层级（0=前排，1=中排，2=后排）
    void CreateFormation()
    {
        npcs.Clear();
        formationPositions.Clear();
        npcLayers.Clear();

        int rows = 0;
        int total = 0;
        while (total < npcCount)
        {
            rows++;
            total += rows;
        }

        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            int cols = i + 1;
            int layer = i; // 行号即层级（第0行=前排，第1行=中排...）
            
            for (int j = 0; j < cols; j++)
            {
                if (index >= npcCount) break;

                float rowOffsetZ = -i * formationSpacing * 0.866f;
                float colOffsetX = (j - (cols - 1) * 0.5f) * formationSpacing;
                Vector3 relativePos = new Vector3(colOffsetX, 0, rowOffsetZ);
                formationPositions.Add(relativePos);

                // 实例化NPC，并传入层级
                GameObject go = Instantiate(npcPrefab, GetFormationWorldPos(relativePos), Quaternion.identity, transform);
				var npc = go.GetComponent<NPCFollow>();
                npc.Initialize(
                    player, 
                    relativePos, 
                    followDistance, 
                    stoppingDistance, 
                    rotationSpeed,
                    layer // 传入层级
                );
                npcs.Add(npc);
                npcLayers.Add(layer);

                index++;
            }
        }
    }

    private Vector3 GetFormationWorldPos(Vector3 relativePos)
    {
        Quaternion playerRot = Quaternion.Euler(0, player.eulerAngles.y, 0);
        Vector3 dir = playerRot * relativePos;
        Vector3 targetPos = player.position + dir.normalized * (followDistance + dir.magnitude);
        targetPos.y = player.position.y; // 统一Y轴高度
        return targetPos;
    }

    void UpdateFormationPositions()
    {
        if (player == null || npcs.Count == 0) return;

        for (int i = npcs.Count-1; i >=0 ; i--)
        {
			var npc = npcs[i];
			if (npc == null)
			{
				npcs.Remove(npc);
				continue;
			}
            Vector3 relativePos = formationPositions[i];
            Vector3 targetPos = GetFormationWorldPos(relativePos);
            npc.SetTargetPosition(targetPos);

            // 同步主角速度参数
            if (npc.animator != null && playerAnimator != null)
            {
                float speed = playerAnimator.GetFloat(playerSpeedParam);
                npc.animator.SetFloat(playerSpeedParam, speed);
            }
        }
    }

    // 同步NPC的移动状态（传递主角是否在移动）
    void SyncNPCStates()
    {
        foreach (var npc in npcs)
        {
            npc.SetPlayerMovingState(isPlayerMoving);
        }
    }
}