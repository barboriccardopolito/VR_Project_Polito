using UnityEngine;
using System.Collections;

public class RadioSistema : MonoBehaviour
{
    public bool haLaRadio = false;

    [Header("Oggetto Fisico")]
    public GameObject radioAddossoAlPlayer; 

    [Header("Componenti Audio")]
    public AudioSource audioSource;
    public AudioClip suonoRicezione;

    [Header("1. Pre-Task (Inizio Fase - Automatico)")]
    [Tooltip("0=Inizio Foto, 1=Inizio Luci, 2=Inizio Fonico, 3=Inizio Regia")]
    public AudioClip[] audioPreTask; 

    [Header("2. Intermedi (Tasto R - Aiuto)")]
    [Tooltip("0=Durante Foto, 1=Durante Luci, 2=Durante Fonico, 3=Durante Regia")]
    public AudioClip[] audioIntermedi; 

    [Header("3. Post-Task (Fine Fase - Automatico)")]
    [Tooltip("0=Finito Foto, 1=Finito Luci, 2=Finito Fonico")] 
    public AudioClip[] audioPostTask; 


    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (haLaRadio && Input.GetKeyDown(KeyCode.R))
        {
            RiproduciMessaggioIntermedio();
        }
    }

    // Sostituisci la vecchia funzione RiceviRadio() con queste tre:

    public void MostraRadioVisivamente()
    {
        // Fa solo comparire la radio addosso al player (niente suoni o attivazioni)
        if (radioAddossoAlPlayer != null) radioAddossoAlPlayer.SetActive(true);
    }

    public void SuonaBipTest()
    {
        // Suona solo il bip di conferma quando il player preme R
        if (audioSource != null && suonoRicezione != null) audioSource.PlayOneShot(suonoRicezione);
    }

    public void AttivaLogicaRadio()
    {
        if (haLaRadio) return;
        haLaRadio = true;

        RiproduciPreTask(GameManager.Reparto.Fotografia);

        Debug.Log("<color=green>[SYSTEM]</color> Radio pienamente operativa!");
    }

    public void GestisciCambioTask(GameManager.Reparto vecchiaTask, GameManager.Reparto nuovaTask)
    {
        StartCoroutine(SequenzaCambioTask(vecchiaTask, nuovaTask));
    }

    IEnumerator SequenzaCambioTask(GameManager.Reparto vecchia, GameManager.Reparto nuova)
    {
        AudioClip clipFine = OttieniClip(audioPostTask, (int)vecchia);

        int indexFine = (int)vecchia - 1;
        if (indexFine >= 0 && indexFine < audioPostTask.Length && audioPostTask[indexFine] != null)
        {
            Debug.Log($"[Radio] Post-Task: {vecchia}");
            audioSource.Stop();
            audioSource.clip = audioPostTask[indexFine];
            audioSource.Play();
            yield return new WaitForSeconds(audioPostTask[indexFine].length + 0.5f);
        }

        RiproduciPreTask(nuova);
    }

    void RiproduciPreTask(GameManager.Reparto fase)
    {
        int index = (int)fase - 1;
        
        if (index >= 0 && index < audioPreTask.Length && audioPreTask[index] != null)
        {
            Debug.Log($"[Radio] Pre-Task: {fase}");
            audioSource.Stop();
            audioSource.PlayOneShot(audioPreTask[index]);
        }
    }

    void RiproduciMessaggioIntermedio()
    {
        if (GameManager.instance == null) return;
        
        int index = (int)GameManager.instance.taskAttuale - 1;

        if (index >= 0 && index < audioIntermedi.Length && audioIntermedi[index] != null)
        {
            Debug.Log($"[Radio] Aiuto Intermedio: {GameManager.instance.taskAttuale}");
            audioSource.Stop(); 
            audioSource.PlayOneShot(audioIntermedi[index]);
        }
    }

    AudioClip OttieniClip(AudioClip[] array, int rawIndex)
    {
        int index = rawIndex - 1; 
        if (index >= 0 && index < array.Length) return array[index];
        return null;
    }
}