using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class RouteLine : MonoBehaviour
{
    // 출발점과 목적지의 Transform
    [SerializeField] Transform start;
    [SerializeField] Transform goal;
    
    // Line 높이
    [SerializeField] float heightOffset = 1.1f;

    // 출발점/목적지가 NavMesh에서 살짝 벗어났을 때 주변을 찾는 거리
    [SerializeField] float sampleRadius = 10f;

    // 위치가 바뀌었을 때 경로를 다시 계산하는 간격(초)
    [SerializeField] float repathInterval = 0.5f;

    LineRenderer line;
    NavMeshPath path;
    float nextRepathTime;

    void Awake()
    {
        // 이미 붙어 있는 LineRenderer를 사용하고, 경로 좌표는 월드 좌표로 넣습니다.
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 0;
        path = new NavMeshPath();
    }

    void Update()
    {
        // 매 프레임 계산하지 않고 설정한 간격마다 경로를 갱신합니다.
        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        // 두 위치를 가까운 NavMesh 위로 보정한 뒤 길을 찾습니다.
        // 위치나 경로를 찾지 못하면 이전에 그린 선도 지웁니다.
        if (start == null || goal == null ||
            !NavMesh.SamplePosition(start.position, out NavMeshHit from, sampleRadius, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(goal.position, out NavMeshHit to, sampleRadius, NavMesh.AllAreas) ||
            !NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) ||
            path.status == NavMeshPathStatus.PathInvalid)
        {
            line.positionCount = 0;
            return;
        }

        // corners는 경로가 꺾이는 지점들입니다. 바닥보다 조금 높여 순서대로 연결합니다.
        Vector3[] corners = path.corners;
        line.positionCount = corners.Length;
        for (int i = 0; i < corners.Length; i++)
            line.SetPosition(i, corners[i] + Vector3.up * heightOffset);
    }
}
