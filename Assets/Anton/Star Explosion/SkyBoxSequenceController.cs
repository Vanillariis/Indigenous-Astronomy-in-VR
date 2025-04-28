using UnityEngine;

public class SkyBoxSequenceController : MonoBehaviour
{
    [Header("Moon Settings")]
    public GameObject moonPrefab;
    public Transform moonTarget;
    public Vector3 moonStartPosition;
    public float moonStartScale = 0.1f;
    public float moonMoveSpeed = 1f;
    public float moonScaleSpeed = 1f;

    [Header("Explosion Settings")]
    public GameObject starExplosion;
    public float explosionDelay = 2f;

    [Header("Skybox Settings")]
    public Material starfieldSkyboxMaterial;
    public float skyboxFadeDelay = 4f;
    public float skyboxFadeDuration = 3f;
    public float maxSkyboxIntensity = 1f;

    private GameObject moonInstance;
    private float timer = 0f;
    private bool explosionTriggered = false;
    private bool fadeStarted = false;
    private float fadeStartTime;

    private bool Doit;

    void Start()
    {
        starfieldSkyboxMaterial.SetFloat("_Fade", 0);
    }

    void Update()
    {
        if (Doit == true)
        {

            timer += Time.deltaTime;

            if (!explosionTriggered && timer >= explosionDelay)
            {
                if (starExplosion != null)
                    starExplosion.SetActive(true);
                explosionTriggered = true;
            }

            if (!fadeStarted && timer >= skyboxFadeDelay)
            {
                fadeStarted = true;
                fadeStartTime = Time.time;
            }

            if (fadeStarted && starfieldSkyboxMaterial != null)
            {
                float t = (Time.time - fadeStartTime) / skyboxFadeDuration;
                float intensity = Mathf.Lerp(0f, maxSkyboxIntensity, t);
                starfieldSkyboxMaterial.SetFloat("_Fade", intensity);
            }
        }
    }

    public void Explode()
    {
        // Spawn and configure moon
        moonInstance = Instantiate(moonPrefab, transform.position, Quaternion.identity);
        var moonMove = moonInstance.GetComponent<MoonMove>();
        moonMove.enabled = false; // temporarily disable so we can set values
        moonMove.target = moonTarget;
        moonMove.moveSpeed = moonMoveSpeed;
        moonMove.scaleSpeed = moonScaleSpeed;
        moonInstance.transform.localScale = Vector3.one * moonStartScale;
        moonMove.enabled = true;

        // Set initial skybox intensity
        if (starfieldSkyboxMaterial != null)
            starfieldSkyboxMaterial.SetFloat("_Fade", 0f);


        Doit = true;
    }
}
