using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;


[System.Serializable]
public class VoicePara
{
    public AudioClip Audio;

    [TextArea(2, 3)]
    public List<string> Translation;
}


public class VoiceOver : MonoBehaviour
{
    public bool Next;
    public bool Auto;
    private bool DoneWithPara = true;
    private bool DoneWithTranslation = true;
    private int Para;

    public List<VoicePara> VoiceParas;
    public AudioSource AudioSource;
    public TMP_Text TranslationText;


    // Update is called once per frame
    void Update()
    {
        if (Next == true)
        {
            if (DoneWithPara == true)
            {
                if (Para >= VoiceParas.Count) return;

                if (Auto == false)
                {
                    Next = false;
                }

                DoneWithPara = false;
                DoneWithTranslation = false;


                AudioSource.clip = VoiceParas[Para].Audio;
                AudioSource.Play();

                int chars = 0;
                foreach (var text in VoiceParas[Para].Translation)
                {
                    chars += text.Length;
                }

                chars -= VoiceParas[Para].Translation.Count;

                float typeSpeed = (VoiceParas[Para].Audio.length / chars) - (2.5f * VoiceParas[Para].Translation.Count) / chars;

                Debug.Log(typeSpeed);



                StartCoroutine(TypeText(VoiceParas[Para].Translation, typeSpeed));


                Para++;
            }
            else
            {
                if (AudioSource.isPlaying == false)
                {
                    DoneWithPara = true;
                }
            }
        }
    }


    IEnumerator TypeText(List<string> translations, float typeSpeed)
    {
        foreach (string text in translations)
        {
            TranslationText.text = "";
            yield return StartCoroutine(TypeChar(text, typeSpeed));
            yield return new WaitForSeconds(2f); 
        }

        DoneWithTranslation = true;
    }

    IEnumerator TypeChar(string text, float typeSpeed)
    {
        foreach (char letter in text)
        {
            TranslationText.text += letter;

            float adjustedSpeed = typeSpeed;

            if (letter == ' ')
            {
                adjustedSpeed *= 1.1f; 
            }

            if (char.IsPunctuation(letter))
            {
                adjustedSpeed *= 1.2f;
            }

            yield return new WaitForSeconds(adjustedSpeed);
        }
    }
}
