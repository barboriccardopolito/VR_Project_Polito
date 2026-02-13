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
        if (animator != null && agent != null && agent.isActiveAndEnabled)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isSeduto) return;

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
        if (staParlando || (radioSistema != null && radioSistema.haLaRadio)) return;

        staParlando = true;

        StartCoroutine(SequenzaDialogo());
    }

IEnumerator SequenzaDialogo()
    {
        // 1. NPC fa l'introduzione (es. "Ti serve questa per comunicare")
        if (clipsIntroduzione != null)
        {
            foreach (AudioClip clip in clipsIntroduzione)
            {
                if (clip != null)
                {
                    audioSource.Stop();
                    audioSource.clip = clip;
                    audioSource.Play();
                    yield return new WaitForSeconds(clip.length + 0.2f);
                }
            }
        }

        // 2. CONSEGNA FISICA DELLA RADIO
        // La radio scompare dal tavolo/cintura e appare in mano al giocatore
        if (radioSistema != null) radioSistema.MostraRadioVisivamente();
        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);

        // 3. TUTORIAL: TEST DELLA RADIO
        if (promptTastoR != null)
        {
            // Mostra la scritta "PREMI R PER TESTARE"
            promptTastoR.SetActive(true);

            // Il gioco aspetta qui finché non premi fisicamente R
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.R));

            // Nascondi scritta
            promptTastoR.SetActive(false);
            
            // Suona il BIP dalla radio del giocatore
            if (radioSistema != null) radioSistema.SuonaBipTest();
            
            yield return new WaitForSeconds(0.5f); // Piccola pausa naturale dopo il bip
        }

        // 4. RISPOSTA NPC ("Perfetto, la radio funziona...")
        if (clipConsegnaRadio != null)
        {
            audioSource.Stop();
            audioSource.clip = clipConsegnaRadio;
            audioSource.Play();
            yield return new WaitForSeconds(clipConsegnaRadio.length);
        }

        Debug.Log("NPC: 'Consegna completata e testata.'");

        // 5. ATTIVAZIONE FINALE
        // Segna la task della Produzione come completata nel GameManager
        if (GameManager.instance != null) 
            GameManager.instance.CompletaTask(GameManager.Reparto.Produzione);

        // ORA la radio diventa attiva (haLaRadio = true) e fa partire la voce della Fotografia
        if (radioSistema != null) radioSistema.AttivaLogicaRadio();

        staParlando = false;
        Alzati();
    }

    void Alzati()
    {
        isSeduto = false;
        if (animator != null) animator.SetBool("IsSeduto", false); 

        if (agent != null)
        {
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
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) targetGiocatore = playerObj.transform;
            else 
            {
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