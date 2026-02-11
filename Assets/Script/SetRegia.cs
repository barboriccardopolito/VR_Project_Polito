using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestoreRecitazione : MonoBehaviour
{
    [Header("Attori")]
    public Animator animatoreAttore;
    public Animator animatoreAttrice;
    public AudioSource audioAttore;
    public AudioSource audioAttrice;

    [Header("Configurazione Dialogo")]
    // Qui definisci chi parla e cosa dice, riga per riga
    public List<Battuta> copione; 
    
    [Header("Animazioni")]
    public string nomeStatoAnimazione = "Talking"; // Il nome esatto dello stato nell'Animator (es. "Scene", "Talk")

    private Coroutine coroutineDialogo;
    private bool inLoop = false;

    [System.Serializable]
    public struct Battuta
    {
        public enum ChiParla { Attore, Attrice }
        public ChiParla chi;
        public AudioClip clipAudio;
        public float pausaDopo; // Tempo extra dopo la battuta prima che parli l'altro
    }

    // --- CHIAMATA DALLA PREVIEW ---
    public void AvviaLoopRecitazione()
    {
        inLoop = true;
        RiavviaTutto();
    }

    // --- CHIAMATA DAL CIAK (RESET TOTALE) ---
    public void AvviaCiakUnico()
    {
        inLoop = false; // Niente loop, deve essere la "buona"
        RiavviaTutto();
    }

    public void FermaTutto()
    {
        inLoop = false;
        if (coroutineDialogo != null) StopCoroutine(coroutineDialogo);
        if (audioAttore) audioAttore.Stop();
        if (audioAttrice) audioAttrice.Stop();
    }

    private void RiavviaTutto()
    {
        // 1. Ferma eventuali audio vecchi
        FermaTutto();

        // 2. RESET ANIMAZIONI A FRAME 0 (Sincronizzazione Totale)
        // Play("NomeStato", Layer, TempoNormalizzato 0=inizio)
        if (animatoreAttore) animatoreAttore.Play(nomeStatoAnimazione, -1, 0f);
        if (animatoreAttrice) animatoreAttrice.Play(nomeStatoAnimazione, -1, 0f);

        // 3. Riavvia la sequenza audio
        // Riattiva la flag loop se necessario perché FermaTutto l'ha spenta
        if (RegiaManager.instance.previewInCorso) inLoop = true; 
        
        coroutineDialogo = StartCoroutine(EseguiCopione());
    }

    IEnumerator EseguiCopione()
    {
        // Scorri tutta la lista delle battute
        foreach (Battuta battuta in copione)
        {
            AudioSource sourceAttuale = (battuta.chi == Battuta.ChiParla.Attore) ? audioAttore : audioAttrice;

            if (sourceAttuale != null && battuta.clipAudio != null)
            {
                sourceAttuale.clip = battuta.clipAudio;
                sourceAttuale.Play();

                // Aspetta la fine della clip + eventuale pausa
                yield return new WaitForSeconds(battuta.clipAudio.length + battuta.pausaDopo);
            }
        }

        // Se siamo in Preview, ricomincia da capo (LOOP)
        if (inLoop)
        {
            RiavviaTutto();
        }
    }
}