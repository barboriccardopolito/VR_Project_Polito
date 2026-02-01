using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    public float raggioMovimento = 10f;
    public float tempoAttesaMin = 2f;
    public float tempoAttesaMax = 5f;

    [Header("Impostazioni Dialogo")]
    public float durataStopDialogo = 5f; // Quanto tempo resta fermo a guardarti

    private NavMeshAgent agent;
    private bool inPausa = false;
    private bool staParlando = false; // NUOVO: Blocca la logica di vagabondaggio

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ScegliNuovaDestinazione();
    }

    void Update()
    {
        // Se sta parlando, non deve fare nulla di movimento
        if (staParlando) return;

        // Logica normale di vagabondaggio
        if (!inPausa && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(PausaRiflessiva());
        }
    }

    void ScegliNuovaDestinazione()
    {
        Vector3 randomDirection = Random.insideUnitSphere * raggioMovimento;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, raggioMovimento, 1))
        {
            if (agent.isOnNavMesh) agent.SetDestination(hit.position);
        }
    }

    IEnumerator PausaRiflessiva()
    {
        inPausa = true;
        float attesa = Random.Range(tempoAttesaMin, tempoAttesaMax);
        yield return new WaitForSeconds(attesa);
        
        // Controllo di sicurezza: se nel frattempo ho iniziato a parlare, non scelgo nuova destinazione ora
        if (!staParlando) 
        {
            ScegliNuovaDestinazione();
            inPausa = false;
        }
    }

    // --- NUOVA FUNZIONE CHIAMATA DALL'INTERAZIONE ---
    public void AscoltaGiocatore(Transform giocatore)
    {
        // Evitiamo di sovrapporre coroutine se clicchi più volte
        if (staParlando) return; 

        StartCoroutine(RoutineDialogo(giocatore));
    }

    IEnumerator RoutineDialogo(Transform giocatore)
    {
        staParlando = true;
        inPausa = true; // Blocca anche il timer della pausa riflessiva
        
        // 1. Ferma l'agente
        agent.isStopped = true;
        // Resetta il percorso per evitare che scivoli via dopo
        agent.ResetPath(); 

        // 2. Ruota verso il giocatore (Solo asse Y per non guardare in alto/basso col corpo)
        Vector3 direzione = (giocatore.position - transform.position).normalized;
        direzione.y = 0; // Appiattisci la direzione
        Quaternion rotazioneTarget = Quaternion.LookRotation(direzione);

        // Rotazione fluida (mezzo secondo)
        float tempoRotazione = 0.5f;
        float t = 0;
        Quaternion rotazioneIniziale = transform.rotation;

        while (t < 1)
        {
            t += Time.deltaTime / tempoRotazione;
            transform.rotation = Quaternion.Slerp(rotazioneIniziale, rotazioneTarget, t);
            yield return null;
        }

        // 3. Aspetta "tot secondi"
        yield return new WaitForSeconds(durataStopDialogo);

        // 4. Riprendi a vagare
        staParlando = false;
        agent.isStopped = false;
        
        // Scegli subito una nuova destinazione per sembrare vivo
        ScegliNuovaDestinazione();
        inPausa = false;
    }
}