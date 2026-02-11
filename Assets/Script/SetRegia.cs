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

    [Header("Oggetti Scenici")]
    public Animator animatoreTappo; // <--- NUOVO: Trascina qui il tappo

    [Header("Configurazione Dialogo")]
    public List<Battuta> copione; 
    
    [Header("Nomi Animazioni (Case Sensitive)")]
    public string nomeStatoAnimazione = "Talking"; // Nome stato attori
    public string nomeStatoTappo = "Take 001";     // Nome stato tappo (Controlla nell'Animator!)

    private Coroutine coroutineDialogo;
    private bool inLoop = false;

    [System.Serializable]
    public struct Battuta
    {
        public enum ChiParla { Attore, Attrice }
        public ChiParla chi;
        public AudioClip clipAudio;
        public float pausaDopo; 
    }

    public void AvviaLoopRecitazione()
    {
        inLoop = true;
        RiavviaTutto();
    }

    public void AvviaCiakUnico()
    {
        inLoop = false; 
        RiavviaTutto();
    }

    public void FermaTutto()
    {
        inLoop = false;
        if (coroutineDialogo != null) StopCoroutine(coroutineDialogo);
        if (audioAttore) audioAttore.Stop();
        if (audioAttrice) audioAttrice.Stop();
        
        // Opzionale: Se vuoi che il tappo si blocchi alla fine del Ciak
        if (animatoreTappo) animatoreTappo.speed = 0; 
    }

    private void RiavviaTutto()
    {
        // 1. Ferma eventuali audio vecchi
        if (coroutineDialogo != null) StopCoroutine(coroutineDialogo);
        if (audioAttore) audioAttore.Stop();
        if (audioAttrice) audioAttrice.Stop();

        // 2. RESET ANIMAZIONI A FRAME 0 (Sincronizzazione Totale)
        
        // Attori
        if (animatoreAttore) {
            animatoreAttore.speed = 1; 
            animatoreAttore.Play(nomeStatoAnimazione, -1, 0f);
        }
        if (animatoreAttrice) {
            animatoreAttrice.speed = 1;
            animatoreAttrice.Play(nomeStatoAnimazione, -1, 0f);
        }

        // --- TAPPO (NUOVO) ---
        if (animatoreTappo) {
            animatoreTappo.speed = 1; // Assicuriamoci che si muova
            animatoreTappo.Play(nomeStatoTappo, -1, 0f); // Reset a 0
        }

        // 3. Riavvia la sequenza audio
        if (RegiaManager.instance.previewInCorso) inLoop = true; 
        
        coroutineDialogo = StartCoroutine(EseguiCopione());
    }

    IEnumerator EseguiCopione()
    {
        foreach (Battuta battuta in copione)
        {
            AudioSource sourceAttuale = (battuta.chi == Battuta.ChiParla.Attore) ? audioAttore : audioAttrice;

            if (sourceAttuale != null && battuta.clipAudio != null)
            {
                sourceAttuale.clip = battuta.clipAudio;
                sourceAttuale.Play();
                yield return new WaitForSeconds(battuta.clipAudio.length + battuta.pausaDopo);
            }
        }

        if (inLoop)
        {
            RiavviaTutto();
        }
    }
}