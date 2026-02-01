using UnityEngine;
using UnityEngine.AI; // Necessario per l'IA
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    public float raggioMovimento = 10f; // Raggio dell'area in cui può vagare
    public float tempoAttesaMin = 2f;  // Minimo tempo di stop
    public float tempoAttesaMax = 5f;  // Massimo tempo di stop

    private NavMeshAgent agent;
    private bool inPausa = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ScegliNuovaDestinazione();
    }

    void Update()
    {
        // Se non è in pausa E ha raggiunto la destinazione
        if (!inPausa && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(PausaRiflessiva());
        }
    }

    void ScegliNuovaDestinazione()
    {
        // Trova un punto a caso dentro una sfera
        Vector3 randomDirection = Random.insideUnitSphere * raggioMovimento;
        randomDirection += transform.position;

        NavMeshHit hit;
        // Controlla che il punto sia valido sul NavMesh (pavimento blu)
        if (NavMesh.SamplePosition(randomDirection, out hit, raggioMovimento, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator PausaRiflessiva()
    {
        inPausa = true;
        float attesa = Random.Range(tempoAttesaMin, tempoAttesaMax);
        yield return new WaitForSeconds(attesa);
        
        ScegliNuovaDestinazione();
        inPausa = false;
    }
}