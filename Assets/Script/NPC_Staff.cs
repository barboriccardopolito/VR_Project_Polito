using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System; 

public class NPC_Staff : MonoBehaviour
{
    [Header("Identità NPC")]
    // IMPORTANTE: Imposta questo valore nell'Inspector per ogni NPC!
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
    public AudioClip audioNonEIlMioTurno; // (Opzionale) Audio per "Non ho tempo ora"

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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        puntoIniziale = transform.position;
        timer = tempoAttesaMin;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 
    }

    void Update()
    {
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
        // Questo metodo serve solo per bloccare l'NPC visivamente
        staParlando = true;
        targetGiocatore = player;
        if (animator != null) animator.SetBool("IsTalking", true);
        CancelInvoke("FineInterazione");
        Invoke("FineInterazione", 4.0f);
    }

    // --- 1. AUDIO INTRODUZIONE (MODIFICATO CON CONTROLLO) ---
    public void AvviaDialogoIniziale()
    {
        // --- BLOCCO DI SICUREZZA ---
        // Se la task globale non corrisponde al ruolo di questo NPC, IGNORA.
        // Esempio: Se sono il Regista ma siamo alla fase "Luci", non ti parlo.
        if (GameManager.instance.taskAttuale != ruoloNPC)
        {
            Debug.Log($"<color=yellow>[NPC]: Sono {ruoloNPC}, ma ora devi fare {GameManager.instance.taskAttuale}. Non ti parlo.</color>");
            
            // Opzionale: Se vuoi che dica "Sono occupato"
            if (audioNonEIlMioTurno != null && !audioSource.isPlaying)
            {
                 audioSource.PlayOneShot(audioNonEIlMioTurno);
            }
            return; 
        }

        if (haGiaParlato) return;
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

    // --- ALTRI METODI RIMANGONO INVARIATI ---
    // (Sono chiamati specificamente quando consegni oggetti, quindi lì il controllo turno è implicito)

    public void ReazioneConsegnaLente(string nomeOggetto) { /* Logica uguale... */ GestisciReazione(nomeOggetto, "Grandangolo", "Cinematografica", audioGrandangolo, audioCinema, audioStandard); }
    public void ReazioneConsegnaLuce(string nomeOggetto) { /* Logica uguale... */ GestisciReazione(nomeOggetto, "Fresnel", "Softbox", audioFresnel, audioSoftbox, audioArtistica); }
    public void ReazioneConsegnaMicrofono(string nomeOggetto) { /* Logica uguale... */ GestisciReazione(nomeOggetto, "Lavalier", "Boom", audioLavalier, audioBoom, audioAmbisonic); }

    // Helper per pulire il codice sopra
    void GestisciReazione(string nome, string key1, string key2, AudioClip clip1, AudioClip clip2, AudioClip clipDef)
    {
        AudioClip clip = clipDef;
        if (nome.IndexOf(key1, StringComparison.OrdinalIgnoreCase) >= 0) clip = clip1;
        else if (nome.IndexOf(key2, StringComparison.OrdinalIgnoreCase) >= 0) clip = clip2;
        SuonaAudioReazione(clip);
    }

    public void ReazioneCiak(Action azioneDopoCiak)
    {
        // Anche qui per sicurezza
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