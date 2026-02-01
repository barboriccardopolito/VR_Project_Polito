using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AttoreFollower : MonoBehaviour
{
    public Transform targetLeader;   // Il capo invisibile da seguire
    public Vector3 offsetFormazione; // La posizione relativa (es: destra, sinistra)
    
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 3.5f; // Un po' più lenti o uguali al leader
    }

    void Update()
    {
        if (targetLeader != null)
        {
            // Trasforma l'offset locale in posizione nel mondo reale
            // Così se il leader ruota, la formazione ruota con lui
            Vector3 destinazione = targetLeader.TransformPoint(offsetFormazione);
            agent.SetDestination(destinazione);
        }
    }
}