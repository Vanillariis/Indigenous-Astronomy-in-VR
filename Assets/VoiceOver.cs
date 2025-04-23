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

    [TextArea(5, 30)]
    public string Translation;
}


public class VoiceOver : MonoBehaviour
{
    public bool Next;
    public bool Auto;
    public bool ShowText;
    private bool DoneWithPara = true;
    private bool DoneWithTranslation = true;
    private int Para;

    public GameObject OstrichPare;

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

                if (Para == 3)
                {
                    OstrichPare.SetActive(true);
                }

                if (ShowText == true)
                {
                    TranslationText.gameObject.SetActive(true);

                    float typeSpeed = (VoiceParas[Para].Audio.length / VoiceParas[Para].Translation.Length);

                    StartCoroutine(TypeText(VoiceParas[Para].Translation, typeSpeed));
                }
                else
                {
                    TranslationText.gameObject.SetActive(false);
                }

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


    IEnumerator TypeText(string translations, float typeSpeed)
    {
        TranslationText.text = "";
        yield return StartCoroutine(TypeChar(translations, typeSpeed));

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

            if (letter == ',')
            {
                adjustedSpeed *= 1.1f;
            }

            if (letter == '.')
            {
                adjustedSpeed *= 1.4f;
            }

            yield return new WaitForSeconds(adjustedSpeed);

            if (letter == '.')
            {
                TranslationText.text = "";
            }
        }
    }
}
