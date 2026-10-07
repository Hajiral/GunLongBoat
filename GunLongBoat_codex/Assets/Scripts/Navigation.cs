using UnityEngine;
using UnityEngine.AI;

public class EnemyPathLine : MonoBehaviour
{
    public NavMeshAgent nav;
    public Transform dest;

    private void Update()
    {
        nav.SetDestination(dest.position);
    }
}