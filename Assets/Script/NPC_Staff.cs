using UnityEngine;
using UnityEngine.AI;

public class NPC_Staff : MonoBehaviour
{
    [Header("Animazioni")]
    public Animator animator;

    [Header("Movimento")]
    public float raggioMovimento = 5f;
    public float tempoAttesaMin = 3f;
    public float tempoAttesaMax = 8f;

    private NavMeshAgent agent;
    private float timer;
    private Vector3 puntoIniziale;
    
    // Stati
    private bool staParlando = false;
    private Transform targetGiocatore;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        puntoIniziale = transform.position; // Il centro della zona è dove lo metti nella scena
        timer = tempoAttesaMin;
    }

    void Update()
    {
        // 1. ANIMAZIONE CAMMINATA (Sincronizzata con velocità reale)
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // 2. LOGICA INTERAZIONE (Priorità assoluta)
        if (staParlando)
        {
            if (agent.isActiveAndEnabled) agent.isStopped = true; // Fermati
            RuotaVersoGiocatore();
            return; // Non fare calcoli di movimento
        }
        else
        {
            if (agent.isActiveAndEnabled) agent.isStopped = false; // Riprendi a camminare
        }

        // 3. LOGICA VAGABONDAGGIO (Wander)
        timer += Time.deltaTime;
        if (timer >= tempoAttesaMax)
        {
            MuoviNPC();
            timer = 0;
            tempoAttesaMax = Random.Range(tempoAttesaMin, tempoAttesaMax + 2f);
        }
    }

    void MuoviNPC()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            Vector3 nuovaPos = RandomNavSphere(puntoIniziale, raggioMovimento, -1);
            agent.SetDestination(nuovaPos);
        }
    }

    // --- INTERAZIONE ---
    
    // Questa funzione viene chiamata da InteragibileNPC
    public void AttivaInterazione(Transform player)
    {
        staParlando = true;
        targetGiocatore = player;

        // Attiva animazione gesticolare
        if (animator != null) animator.SetBool("IsTalking", true);

        // Imposta un timer per smettere di gesticolare dopo 4 secondi (o durata dialogo)
        CancelInvoke("FineInterazione");
        Invoke("FineInterazione", 4.0f);
    }

    void FineInterazione()
    {
        staParlando = false;
        targetGiocatore = null;
        if (animator != null) animator.SetBool("IsTalking", false);
    }

    // --- UTILITÀ ---

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
            return navHit.position;
        return origin;
    }

    void RuotaVersoGiocatore()
    {
        if (targetGiocatore != null)
        {
            Vector3 direzione = (targetGiocatore.position - transform.position).normalized;
            direzione.y = 0; // Mantieni la rotazione solo sull'asse Y (orizzontale)
            if (direzione != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direzione), Time.deltaTime * 5f);
            }
        }
    }
}