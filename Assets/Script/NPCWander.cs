using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCWander : MonoBehaviour
{
    [Header("Componenti")]
    public Animator animator;
    public GameObject oggettoRadioFisico; // La radio fisica sul tavolo/cintura

    [Header("Posizioni")]
    public Transform puntoDiUscitaSedia; // Trascina qui l'oggetto vuoto "sicuro"

    [Header("Movimento")]
    public float raggioMovimento = 3f;
    public float tempoAttesaMin = 2f;
    public float tempoAttesaMax = 5f;

    [Header("Audio Dialoghi")]
    public AudioClip[] clipsIntroduzione; // Intro_01 a Intro_06 (PRIMA di premere R)
    
    [Header("--- TUTORIAL RADIO ---")]
    public GameObject promptTastoR;       // Trascina qui la scritta "Premi R"
    public AudioClip clipConsegnaRadio;   // La frase "Ottimo, ecco a te..." (DOPO aver premuto R)

    private AudioSource audioSource;
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
        
        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Audio 3D

        // Se non abbiamo assegnato il punto di uscita, usiamo la posizione attuale come fallback
        if (puntoDiUscitaSedia == null) puntoDiUscitaSedia = transform; 
        puntoIniziale = puntoDiUscitaSedia.position; 

        timer = tempoAttesaMin;
        radioSistema = FindObjectOfType<RadioSistema>();

        // Assicuriamoci che la scritta tutorial sia spenta all'inizio
        if (promptTastoR != null) promptTastoR.SetActive(false);

        // Se il giocatore ha già la radio, l'NPC parte già in piedi che cammina
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
        isSeduto = false;
        if (animator != null) animator.SetBool("IsSeduto", false);

        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);
        
        if (agent != null) 
        {
            agent.enabled = true;
            MuoviNPC();
        }
    }

    void Update()
    {
        // 1. SINCRONIZZA ANIMAZIONE VELOCITÀ
        if (animator != null && agent != null && agent.isActiveAndEnabled)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // Se è seduto, fermati qui.
        if (isSeduto) return;

        // Se sta parlando (ma è in piedi), ruota verso il giocatore e stai fermo
        if (staParlando)
        {
            RuotaVersoGiocatore();
            if(agent.isActiveAndEnabled) agent.isStopped = true; 
            return;
        }
        else
        {
            if(agent.isActiveAndEnabled) agent.isStopped = false;
        }

        // Logica Vagabondaggio (Solo se è in piedi e ha finito di parlare)
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

    // --- INTERAZIONE ---
    public void InterazioneConPlayer()
    {
        // Evita doppi click o interazioni se ha già dato la radio
        if (staParlando || (radioSistema != null && radioSistema.haLaRadio)) return;

        staParlando = true; // Blocca eventuali altri input

        // Avvia la sequenza audio
        StartCoroutine(SequenzaDialogo());
    }

    IEnumerator SequenzaDialogo()
    {
        // 1. Riproduci le frasi di introduzione (Bla bla bla...)
        if (clipsIntroduzione != null)
        {
            foreach (AudioClip clip in clipsIntroduzione)
            {
                if (clip != null)
                {
                    audioSource.Stop();
                    audioSource.clip = clip;
                    audioSource.Play();
                    // Aspetta la durata della clip + piccola pausa
                    yield return new WaitForSeconds(clip.length + 0.2f);
                }
            }
        }

        // --- PAUSA TUTORIAL ---
        if (promptTastoR != null)
        {
            // Mostra scritta "PREMI R"
            promptTastoR.SetActive(true);

            // Aspetta finché non premi R
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.R));

            // Nascondi scritta
            promptTastoR.SetActive(false);
            
            // Pausa scenica per far sembrare che ascolti la radio
            yield return new WaitForSeconds(0.5f);
        }

        // 2. Riproduci la frase finale ("Ottimo, ecco a te...")
        if (clipConsegnaRadio != null)
        {
            audioSource.Stop();
            audioSource.clip = clipConsegnaRadio;
            audioSource.Play();
            yield return new WaitForSeconds(clipConsegnaRadio.length);
        }

        // 3. AUDIO FINITO: Consegna e Azione
        Debug.Log("NPC: 'Ecco a te.'");
        
        // Consegna Logica
        if (radioSistema != null) radioSistema.RiceviRadio(); // ATTENZIONE: Assicurati che RiceviRadio() setti haLaRadio = true
        
        // Nascondi Radio Fisica sul tavolo
        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);

        // Completa Task nel GameManager
        if (GameManager.instance != null) 
            GameManager.instance.CompletaTask(GameManager.Reparto.Produzione);

        // Reset stato parlato
        staParlando = false;

        // 4. ORA MI ALZO
        Alzati();
    }

    void Alzati()
    {
        isSeduto = false;
        if (animator != null) animator.SetBool("IsSeduto", false); 

        if (agent != null)
        {
            // Teletrasportiamo l'agent nel punto sicuro
            if (puntoDiUscitaSedia != null)
            {
                agent.Warp(puntoDiUscitaSedia.position);
                transform.rotation = puntoDiUscitaSedia.rotation; 
            }
            
            agent.enabled = true;
            MuoviNPC(); // Parte subito
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
        if (targetGiocatore == null)
        {
            // Cerchiamo il player in modo sicuro
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) targetGiocatore = playerObj.transform;
            else 
            {
                 // Fallback: cerca l'oggetto che ha lo script interazione
                 InterazioneGiocatore scriptPlayer = FindObjectOfType<InterazioneGiocatore>();
                 if(scriptPlayer != null) targetGiocatore = scriptPlayer.transform;
            }
        }

        if (targetGiocatore != null)
        {
            Vector3 direzione = (targetGiocatore.position - transform.position).normalized;
            direzione.y = 0;
            if (direzione != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direzione), Time.deltaTime * 5f);
        }
    }
}