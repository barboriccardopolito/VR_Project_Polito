using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System; 

public class NPC_Staff : MonoBehaviour
{
    [Header("Identità NPC")]
    public GameManager.Reparto ruoloNPC; 

    [Header("Animazioni")]
    public Animator animator;

    [Header("Movimento")]
    public float raggioMovimento = 5f;
    public float tempoAttesaMin = 3f;
    public float tempoAttesaMax = 8f;

    [Header("Audio Dialoghi Generici")]
    public AudioClip[] clipsIntroduzione;
    public AudioClip audioTaskCompletata; 
    public AudioClip audioNonEIlMioTurno;

    [Header("Audio Regista (SOLO REGISTA)")] 
    public AudioClip audioCiak; 

    [Header("Audio Consegna Lente (FOTOGRAFO)")]
    public AudioClip audioGrandangolo;
    public AudioClip audioStandard; 
    public AudioClip audioCinema;   

    [Header("Audio Consegna Luce (ELETTRICISTA)")]
    public AudioClip audioFresnel;
    public AudioClip audioSoftbox;
    public AudioClip audioArtistica;

    [Header("Audio Consegna Microfono (FONICO)")]
    public AudioClip audioLavalier;
    public AudioClip audioBoom;
    public AudioClip audioAmbisonic;

    private AudioSource audioSource;
    
    [HideInInspector]
    public bool haGiaParlato = false; 

    private NavMeshAgent agent;
    private float timer;
    private Vector3 puntoIniziale;
    private bool staParlando = false;
    private Transform targetGiocatore;

    // --- NUOVO: RIFERIMENTO FRECCIA ---
    private Evidenziatore evidenziatore;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        puntoIniziale = transform.position;
        timer = tempoAttesaMin;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        // Trova lo script dell'evidenziatore attaccato all'NPC
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();
    }

    void Update()
    {
        GestisciEvidenziatore(); // <--- Controllo costante della freccia

        if (animator != null && agent != null) animator.SetFloat("Speed", agent.velocity.magnitude);

        if (staParlando)
        {
            if (agent.isActiveAndEnabled) agent.isStopped = true;
            RuotaVersoGiocatore();
            return;
        }
        else
        {
            if (agent.isActiveAndEnabled) agent.isStopped = false;
        }

        timer += Time.deltaTime;
        if (timer >= tempoAttesaMax)
        {
            MuoviNPC();
            timer = 0;
            tempoAttesaMax = UnityEngine.Random.Range(tempoAttesaMin, tempoAttesaMax + 2f);
        }
    }

    // --- NUOVA LOGICA FRECCIA SULL'NPC ---
    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        bool isMioTurno = (GameManager.instance.taskAttuale == ruoloNPC);

        // La freccia dell'NPC è accesa SOLO SE:
        // 1. È il momento del suo reparto (es. Fotografia)
        // 2. NON gli ho ancora parlato (haGiaParlato è falso)
        if (isMioTurno && !haGiaParlato)
        {
            evidenziatore.Accendi();
        }
        else
        {
            evidenziatore.Spegni();
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

    public void AttivaInterazione(Transform player)
    {
        staParlando = true;
        targetGiocatore = player;
        if (animator != null) animator.SetBool("IsTalking", true);
        CancelInvoke("FineInterazione");
        Invoke("FineInterazione", 4.0f);
    }

    public void AvviaDialogoIniziale()
    {
        if (GameManager.instance.taskAttuale != ruoloNPC)
        {
            Debug.Log($"<color=yellow>[NPC]: Sono {ruoloNPC}, ma ora devi fare {GameManager.instance.taskAttuale}. Non ti parlo.</color>");
            
            if (audioNonEIlMioTurno != null && !audioSource.isPlaying)
            {
                 audioSource.PlayOneShot(audioNonEIlMioTurno);
            }
            return; 
        }

        if (haGiaParlato) return;
        
        // Appena imposto questa a true, l'Update spegnerà la freccia istantaneamente!
        haGiaParlato = true; 
        staParlando = true;  
        
        CancelInvoke("FineInterazione");
        StartCoroutine(SequenzaDialogo());
    }

    IEnumerator SequenzaDialogo()
    {
        if (animator != null) animator.SetBool("IsTalking", true);
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
        FineInterazione();
    }

    public void ReazioneConsegnaLente(string nomeOggetto) { GestisciReazione(nomeOggetto, "Grandangolo", "Cinematografica", audioGrandangolo, audioCinema, audioStandard); }
    public void ReazioneConsegnaLuce(string nomeOggetto) { GestisciReazione(nomeOggetto, "Fresnel", "Softbox", audioFresnel, audioSoftbox, audioArtistica); }
    public void ReazioneConsegnaMicrofono(string nomeOggetto) { GestisciReazione(nomeOggetto, "Lavalier", "Boom", audioLavalier, audioBoom, audioAmbisonic); }

    void GestisciReazione(string nome, string key1, string key2, AudioClip clip1, AudioClip clip2, AudioClip clipDef)
    {
        AudioClip clip = clipDef;
        if (nome.IndexOf(key1, StringComparison.OrdinalIgnoreCase) >= 0) clip = clip1;
        else if (nome.IndexOf(key2, StringComparison.OrdinalIgnoreCase) >= 0) clip = clip2;
        SuonaAudioReazione(clip);
    }

    public void ReazioneCiak(Action azioneDopoCiak)
    {
        if (ruoloNPC == GameManager.Reparto.Regia) 
        {
             if (audioCiak != null) StartCoroutine(SequenzaCiak(azioneDopoCiak));
             else azioneDopoCiak?.Invoke();
        }
    }

    IEnumerator SequenzaCiak(Action azioneDopoCiak)
    {
        staParlando = true;
        CancelInvoke("FineInterazione");
        if (animator != null) animator.SetBool("IsTalking", true);

        audioSource.Stop();
        audioSource.clip = audioCiak;
        audioSource.Play();

        yield return new WaitForSeconds(audioCiak.length + 0.1f);

        azioneDopoCiak?.Invoke();
        FineInterazione();
    }

    void SuonaAudioReazione(AudioClip clip)
    {
        if (clip != null)
        {
            StopAllCoroutines(); 
            CancelInvoke("FineInterazione");
            staParlando = true; 
            if (animator != null) animator.SetBool("IsTalking", true);

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            Invoke("FineInterazione", clip.length + 0.5f);
        }
    }

    public void ReazioneFineTask(Action azioneAlTermine)
    {
        if (audioTaskCompletata != null) StartCoroutine(SequenzaFineTask(azioneAlTermine));
        else azioneAlTermine?.Invoke();
    }

    IEnumerator SequenzaFineTask(Action azioneAlTermine)
    {
        staParlando = true;
        CancelInvoke("FineInterazione");
        if (animator != null) animator.SetBool("IsTalking", true);

        audioSource.Stop();
        audioSource.clip = audioTaskCompletata;
        audioSource.Play();

        yield return new WaitForSeconds(audioTaskCompletata.length + 0.2f);

        azioneAlTermine?.Invoke();
        FineInterazione();
    }

    void FineInterazione()
    {
        staParlando = false;
        targetGiocatore = null;
        if (animator != null) animator.SetBool("IsTalking", false);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;
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
            if (direzione != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direzione), Time.deltaTime * 5f);
        }
    }
}