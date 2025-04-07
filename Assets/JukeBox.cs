using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JukeboxSound
{
    public AudioClip Sound;
    public float Volume;
}


public class JukeBox : MonoBehaviour
{
    public List<JukeboxSound> Sounds;
    public AudioSource AudioSource;


    public float Timer;
    public float MaxTimer;



    public void Start()
    {
        MaxTimer = GetMaxTime();
    }


    // Update is called once per frame
    void Update()
    {
        if (Timer > MaxTimer)
        {
            int _rr = Random.Range(0, Sounds.Count);
            AudioSource.clip = Sounds[_rr].Sound;
            AudioSource.volume = Sounds[_rr].Volume;
            AudioSource.Play();

            Timer = 0;
            MaxTimer = GetMaxTime();
        }
        else
        {
            Timer += Time.deltaTime;
        }
    }

    float GetMaxTime()
    {
        return Random.Range(5f, 15f);
    }
}
