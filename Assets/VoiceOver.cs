using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    public Kite kite;

    [Header("Dub")]
    public float fadeDuration;
    [Range(0f, 1f)] public float AudioSourceVolume_Min;
    [Range(0f, 1f)] public float AudioSourceVolume_Max;

    [Header("God Speaking")]
    public bool Instruction;
    public bool InstructionOnce;
    public bool Done;

    [Header("DontDestroyOnLoad")]
    public static VoiceOver Instance { get; private set; }
    
    [Header("BlinkEffect")]
    public Image blinkFadeImage;
    public float blinkFadeDuration = 0.5f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
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
                    if (InstructionOnce == false)
                    {
                        StartCoroutine(SpeakInstruction());
                        InstructionOnce = true;
                    }

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

                if (Para == 2) //2
                {
                    StartCoroutine(FadeAndSwitchScenes("GodScene"));
                }

                if (Para == 4) //4
                {
                    OstrichPare.SetActive(true);
                    ostrichLogic.FeatherHasBeenAttached = false;
                }

                if (Para == 5) // 5
                {
                    ostrichLogic.FeatherHasBeenAttached = true;
                    Instruction = true;
                }

                if (Para == 9) //9
                {
                    kite.EndScene = true;
                }

                if (Para == 12) // 12
                {
                    StartCoroutine(FadeAndSwitchScenes("SunsetScene"));
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
            Debug.Log("God Speaking");

            ostrichLogic.LightGod_AudioSource.Play();
            ostrichLogic.DarkGod_AudioSource.Play();

            // Wait until it's time to fade out the real clip
            yield return new WaitForSeconds(25);
        }
    }




    IEnumerator PipiEffect()
    {
        // Calculate start time so the dub clip is centered within the real clip
        float realClipLength = AudioSource.clip.length;
        float dubClipLength = AudioSourceDub.clip.length;
        float dubStartTime = Mathf.Max(0f, (realClipLength - dubClipLength) * 0.5f);

        float waitTime = Mathf.Max(0f, dubStartTime - fadeDuration);

        float realfadeDuration = fadeDuration;

        if (waitTime <= 5)
        {
            realfadeDuration = 1;
        }

        // Wait until it's time to fade out the real clip
        yield return new WaitForSeconds(waitTime);

        // Fade down the real clip
        yield return StartCoroutine(FadeVolume(AudioSource, AudioSource.volume, AudioSourceVolume_Min, realfadeDuration));

        // Play the dub clip
        AudioSourceDub.Play();

        // Wait for the dub clip to finish
        yield return new WaitForSeconds(AudioSourceDub.clip.length);

        // Fade the real clip back up
        yield return StartCoroutine(FadeVolume(AudioSource, AudioSource.volume, AudioSourceVolume_Max, realfadeDuration));
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

    

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Kite k = FindObjectOfType<Kite>();
        if (k != null)
        {
            kite = k;
            kite.VoiceOver = this;

            ostrichLogic = kite.OstrichLogic;
            OstrichPare = ostrichLogic.transform.parent.gameObject;

            ostrichLogic.VoiceOver = this;
        }
        
        if (scene.name == "SunsetScene")
        {
            // 1. Handle audio switch
            GameObject audioManager = GameObject.Find("AudioManager");
            if (audioManager != null)
            {
                Transform twilight = audioManager.transform.Find("Twilight");
                Transform night = audioManager.transform.Find("Night");

                if (twilight != null && night != null)
                {
                    AudioSource twilightSource = twilight.GetComponent<AudioSource>();
                    AudioSource nightSource = night.GetComponent<AudioSource>();

                    if (twilightSource != null) twilightSource.Stop();
                    if (nightSource != null) nightSource.Play();
                }
            }

            // 2. Enable skybox GameObject
            GameObject skybox = GameObject.Find("Skybox");
            if (skybox != null)
            {
                skybox.SetActive(true);
            }

            // 3. Switch lights
            GameObject mainLight = GameObject.Find("Directional Light");
            GameObject nightLight = GameObject.Find("Night Directional Light");

            if (mainLight != null) mainLight.SetActive(false);
            if (nightLight != null) nightLight.SetActive(true);
        }
    }
    
    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade back in
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        Color color = blinkFadeImage.color;

        while (timer < blinkFadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / blinkFadeDuration);
            blinkFadeImage.color = new Color(color.r, color.g, color.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        blinkFadeImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
