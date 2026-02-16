using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCWander : MonoBehaviour
{
    [Header("Componenti")]
    public Animator animator;
    public GameObject oggettoRadioFisico; 

    [Header("Posizioni")]
    public Transform puntoDiUscitaSedia; 

    [Header("Movimento")]
    public float raggioMovimento = 3f;
    public float tempoAttesaMin = 2f;
    public float tempoAttesaMax = 5f;

    [Header("Audio Dialoghi")]
    public AudioClip[] clipsIntroduzione; 
    
    [Header("--- TUTORIAL RADIO ---")]
    public GameObject promptTastoR;       
    public AudioClip clipConsegnaRadio;   

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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        if (puntoDiUscitaSedia == null) puntoDiUscitaSedia = transform; 
        puntoIniziale = puntoDiUscitaSedia.position; 

        timer = tempoAttesaMin;
        radioSistema = FindFirstObjectByType<RadioSistema>();

        if (promptTastoR != null) promptTastoR.SetActive(false);

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
        // 1. INTRODUZIONE
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

        // --- 2. VISUALE LAVAGNA ---
        FocusLavagna lavagna = FindFirstObjectByType<FocusLavagna>();
        if (lavagna != null && lavagna.cameraLavagna != null) // Assicuriamoci che esista!
        {
            lavagna.AvviaInquadratura();
            yield return new WaitForSeconds(0.5f); 
            
            // Aspetta finché il focus è attivo (si spegnerà quando premi E)
            yield return new WaitWhile(() => lavagna.isFocusAttivo);
        }
        else
        {
            Debug.LogWarning("Script FocusLavagna non trovato o Telecamera Lavagna non assegnata! Salto la scena della lavagna.");
        }
        // ----------------------------------

        // 3. CONSEGNA FISICA DELLA RADIO
        if (radioSistema != null) radioSistema.MostraRadioVisivamente();
        if (oggettoRadioFisico != null) oggettoRadioFisico.SetActive(false);

        // 4. TUTORIAL: TEST DELLA RADIO
        if (promptTastoR != null)
        {
            promptTastoR.SetActive(true);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.R));
            promptTastoR.SetActive(false);
            
            if (radioSistema != null) radioSistema.SuonaBipTest();
            yield return new WaitForSeconds(0.5f); 
        }

        // 5. RISPOSTA NPC
        if (clipConsegnaRadio != null)
        {
            audioSource.Stop();
            audioSource.clip = clipConsegnaRadio;
            audioSource.Play();
            yield return new WaitForSeconds(clipConsegnaRadio.length);
        }

        // 6. ATTIVAZIONE FINALE E CHIUSURA TASK
        if (GameManager.instance != null) 
            GameManager.instance.CompletaTask(GameManager.Reparto.Produzione);

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
            MuoviNPC(); 
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
                 InterazioneGiocatore scriptPlayer = FindFirstObjectByType<InterazioneGiocatore>();
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