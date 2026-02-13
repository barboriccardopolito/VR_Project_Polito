using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AttoreFollower : MonoBehaviour
{
    public Transform targetLeader;
    public Vector3 offsetFormazione;
    
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 3.5f;
    }

    void Update()
    {
        if (targetLeader != null)
        {
            Vector3 destinazione = targetLeader.TransformPoint(offsetFormazione);
            agent.SetDestination(destinazione);
        }
    }
}