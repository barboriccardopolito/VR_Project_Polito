using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    [Header("Componenti")]
    public Animator animator;
    public GameObject oggettoRadioFisico;

    [Header("Posizioni")]
    public Transform puntoDiUscitaSedia; // <-- NUOVO: Trascina qui un oggetto vuoto dove vuoi che appaia in piedi

    [Header("Movimento")]
    public float raggioMovimento = 3f;
    public float tempoAttesaMin = 2f;
    public float tempoAttesaMax = 5f;

    private NavMeshAgent agent;
    private RadioSistema radioSistema;
    private float timer;
    private Vector3 puntoIniziale;
    
    private bool isSeduto = true;
    private bool staParlando = false;
    private Transform targetGiocatore;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Se non abbiamo assegnato il punto di uscita, usiamo la posizione attuale come fallback
        if (puntoDiUscitaSedia == null) puntoDiUscitaSedia = transform; 
        puntoIniziale = puntoDiUscitaSedia.position; // Il centro del vagabondaggio è il punto sicuro, non la sedia!

        timer = tempoAttesaMin;
        radioSistema = FindObjectOfType<RadioSistema>();

        if (radioSistema != null && radioSistema.haLaRadio)
            StartInPiedi();
        else
            StartSeduto();
    }

    void StartSeduto()
    {
        isSeduto = true;
        if (animator != null) animator.SetBool("IsSeduto", true);
        if (agent != null) agent.enabled = false;
    }

    void StartInPiedi()
    {
        Alzati(); // Usa la logica completa di alzata
        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);
    }

    void Update()
    {
        // 1. SINCRONIZZA ANIMAZIONE VELOCITÀ (Fix Moonwalking)
        if (animator != null && agent != null && agent.isActiveAndEnabled)
        {
            // Passiamo la velocità reale dell'agente all'animator
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isSeduto) return;

        if (staParlando)
        {
            RuotaVersoGiocatore();
            // Se parla, fermiamo l'agent ma l'animazione andrà in Idle grazie al parametro Speed = 0
            if(agent.isActiveAndEnabled) agent.isStopped = true; 
            return;
        }
        else
        {
            if(agent.isActiveAndEnabled) agent.isStopped = false;
        }

        // Logica Vagabondaggio
        timer += Time.deltaTime;
        if (timer >= tempoAttesaMax)
        {
            MuoviNPC();
            timer = 0;
            tempoAttesaMax = Random.Range(tempoAttesaMin, 5f);
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

    public void InterazioneConPlayer()
    {
        if (radioSistema != null && radioSistema.haLaRadio) return;

        Debug.Log("NPC: 'Ecco a te.'");
        if (radioSistema != null) radioSistema.RiceviRadio();
        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);

        Invoke("Alzati", 30.0f);
    }

    void Alzati()
    {
        isSeduto = false;
        if (animator != null) animator.SetBool("IsSeduto", false);

        if (agent != null)
        {
            // --- FIX TRAVERSAMENTO TAVOLO ---
            // Teletrasportiamo l'agent nel punto sicuro (es. di fianco alla scrivania)
            // Usiamo Warp che è il modo corretto per spostare un NavMeshAgent istantaneamente
            if (puntoDiUscitaSedia != null)
            {
                agent.Warp(puntoDiUscitaSedia.position);
                transform.rotation = puntoDiUscitaSedia.rotation; // La giriamo già verso la stanza
            }
            
            agent.enabled = true;
            MuoviNPC(); // Diamo subito una destinazione per farla partire
        }
    }

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
            direzione.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direzione), Time.deltaTime * 5f);
        }
    }
}