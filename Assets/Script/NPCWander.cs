using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    [Header("Zona di Movimento")]
    public float raggioMovimento = 3f; // Quanto può allontanarsi dalla base
    public float tempoAttesaMin = 2f;
    public float tempoAttesaMax = 5f;

    private NavMeshAgent agent;
    private float timer;
    private Transform targetGiocatore;
    private bool staParlando = false;
    
    // Questa variabile salva dove hai messo l'NPC nell'Editor
    private Vector3 puntoInizialeDiPartenza; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = tempoAttesaMin;

        // MEMORIZZA LA POSIZIONE INIZIALE (LA "CUCCIA")
        puntoInizialeDiPartenza = transform.position;
    }

    void Update()
    {
        if (staParlando)
        {
            RuotaVersoGiocatore();
            return;
        }

        timer += Time.deltaTime;

        if (timer >= tempoAttesaMax)
        {
            // Calcola una nuova posizione MA partendo dal punto iniziale, non da dove si trova ora
            Vector3 nuovaPos = RandomNavSphere(puntoInizialeDiPartenza, raggioMovimento, -1);
            agent.SetDestination(nuovaPos);
            
            timer = 0;
            tempoAttesaMax = Random.Range(tempoAttesaMin, 5f);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    public void AscoltaGiocatore(Transform giocatore)
    {
        staParlando = true;
        targetGiocatore = giocatore;
        if(agent != null) agent.isStopped = true;
    }

    void RuotaVersoGiocatore()
    {
        if (targetGiocatore == null) return;
        Vector3 direzione = (targetGiocatore.position - transform.position).normalized;
        direzione.y = 0;
        Quaternion rotazione = Quaternion.LookRotation(direzione);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotazione, Time.deltaTime * 5f);
    }
}