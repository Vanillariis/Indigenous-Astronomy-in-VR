using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


[System.Serializable]
public class VoicePara
{
    public AudioClip Audio;
    public AudioClip AudioDub;

    [TextArea(10, 30)]
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
    public AudioSource AudioSourceDub;
    public TMP_Text TranslationText;

    public OstrichLogic ostrichLogic;

    [Header("Dub")]
    public float fadeDuration;
    [Range(0f, 1f)] public float AudioSourceVolume_Min;
    [Range(0f, 1f)] public float AudioSourceVolume_Max;

    [Header("God Speaking")]
    public bool Instruction;
    public bool Done;
    public AudioSource LightGod_AudioSource;
    public AudioSource DarkGod_AudioSource;

    [Header("DontDestroyOnLoad")]
    public static VoiceOver Instance { get; private set; }

    void Awake()
    {
        // If there isn’t already one, make this the singleton instance
        if (Instance == null)
        {
            Instance = this;

            // Prevent this GameObject from being destroyed on scene load
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance exists, destroy this one to enforce singleton
            Destroy(gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Next == true)
        {
            if (DoneWithPara == true)
            {
                if (Instruction == true)
                {
                    StartCoroutine(SpeakInstruction());
                    return;
                }

                if (Para >= VoiceParas.Count) return;

                if (Done == false) return;

                if (Auto == false)
                {
                    Next = false;
                }

                DoneWithPara = false;
                DoneWithTranslation = false;


                AudioSource.clip = VoiceParas[Para].Audio;
                AudioSourceDub.clip = VoiceParas[Para].AudioDub;
                AudioSource.Play();

                StartCoroutine(PipiEffect());

                if (Para == 2)
                {
                    GoToScene("GodScene");
                }

                if (Para == 4) //4
                {
                    OstrichPare.SetActive(true);
                    ostrichLogic.FeatherHasBeenAttached = false;
                }

                if (Para == 5)
                {
                    ostrichLogic.FeatherHasBeenAttached = true;
                    Instruction = true;
                }

                if (Para == 9)
                {
                    FindAnyObjectByType<Kite>().EndScene = true;
                }

                if (Para == 12)
                {
                    GoToScene("SunsetScene");
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



    IEnumerator SpeakInstruction()
    {
        while (Instruction == true)
        {
            LightGod_AudioSource.Play();
            DarkGod_AudioSource.Play();

            // Wait until it's time to fade out the real clip
            yield return new WaitForSeconds(40);
        }
    }




    IEnumerator PipiEffect()
    {
        // Calculate start time so the dub clip is centered within the real clip
        float realClipLength = AudioSource.clip.length;
        float dubClipLength = AudioSourceDub.clip.length;
        float dubStartTime = Mathf.Max(0f, (realClipLength - dubClipLength) * 0.5f);

        Debug.Log("wait: " + Mathf.Max(0f, dubStartTime - fadeDuration));

        // Wait until it's time to fade out the real clip
        yield return new WaitForSeconds(Mathf.Max(0f, dubStartTime - fadeDuration));

        // Fade down the real clip
        yield return StartCoroutine(FadeVolume(AudioSource, AudioSource.volume, AudioSourceVolume_Min, fadeDuration));

        // Play the dub clip
        AudioSourceDub.Play();

        // Wait for the dub clip to finish
        yield return new WaitForSeconds(AudioSourceDub.clip.length);

        // Fade the real clip back up
        yield return StartCoroutine(FadeVolume(AudioSource, AudioSource.volume, AudioSourceVolume_Max, fadeDuration));
    }

    IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        source.volume = to;
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


    // Call this method to switch scenes
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
