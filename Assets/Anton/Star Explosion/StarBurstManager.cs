using UnityEngine;
using System.Runtime.InteropServices;

public class StarBurstRenderer : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Material starMaterial;
    [SerializeField] Mesh starMesh;
    [SerializeField] int starCount = 1024;
    [SerializeField] float swirlStrength = 1f;
    [SerializeField] float baseSize = 0.2f;
    [SerializeField] private IcosphereGenerator skyBox;
    [SerializeField] float speed = 5f; // How fast the stars move outward
    public bool doit;

    struct StarData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
        public float startTime;
        public Vector3 color;
    }

    ComputeBuffer starBuffer;
    int kernelID;
    Bounds renderBounds;

    void Start()
    {
        
    }

    public void Explode()
    {
        kernelID = computeShader.FindKernel("CSMain");

        starBuffer = new ComputeBuffer(starCount, Marshal.SizeOf(typeof(StarData)));

        // Calculate origin and sphere surface distance
        Vector3 origin = transform.position;
        Vector3 sphereCenter = skyBox.transform.position;
        float radius = skyBox.radius;

        // Initialize star data
        StarData[] initData = new StarData[starCount];
        for (int i = 0; i < starCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            if (dir.y < 0f) dir.y *= -1f; // Mirror to upper hemisphere
            Vector3 targetPoint = sphereCenter + dir * radius;
            float distance = Vector3.Distance(origin, targetPoint);
            float life = distance / speed;

            initData[i] = new StarData
            {
                position = origin,
                velocity = dir * speed,
                life = life,
                startTime = Time.time,
                color = new Vector3(1.0f, 1.0f, 1.0f)
            };
        }

        starBuffer.SetData(initData);
        computeShader.SetBuffer(kernelID, "starBuffer", starBuffer);

        // Set large bounds for procedural rendering
        renderBounds = new Bounds(transform.position + Vector3.up * 50f, Vector3.one * 200f);

        doit = true;
    }

    void Update()
    {
        if (doit == true)
        {
            float deltaTime = Time.deltaTime;
            float currentTime = Time.time;
            Vector3 origin = transform.position;

            computeShader.SetFloat("deltaTime", deltaTime);
            computeShader.SetFloat("swirlStrength", swirlStrength);
            computeShader.SetVector("origin", origin);
            computeShader.SetFloat("currentTime", currentTime);

            computeShader.Dispatch(kernelID, Mathf.CeilToInt(starCount / 64f), 1, 1);

            starMaterial.SetBuffer("_StarBuffer", starBuffer);
            starMaterial.SetFloat("_TimeNow", currentTime);
            starMaterial.SetFloat("_BaseSize", baseSize);
            starMaterial.SetVector("_Origin", origin);

            //renderBounds.center = transform.position;

            Graphics.DrawMeshInstancedProcedural(starMesh, 0, starMaterial, renderBounds, starCount);
        }
    }

    void OnDestroy()
    {
        starBuffer?.Release();
    }
}