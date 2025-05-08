using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    public IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;
            fadeImage.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadeImage.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
    }
}