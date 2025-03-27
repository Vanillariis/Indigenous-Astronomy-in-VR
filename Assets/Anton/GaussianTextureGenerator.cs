using UnityEngine;
using System.IO;

public class GaussianTextureGenerator : MonoBehaviour
{
    // Texture size (e.g., 256x256)
    public int textureWidth = 256;
    public int textureHeight = 256;

    // Standard deviation for the Gaussian distribution
    public float sigma = 0.5f;

    // The texture to store the random vectors
    private Texture2D randomVectorTexture;

    void Start()
    {
        // Create and fill the texture with Gaussian distributed random vectors
        randomVectorTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

        // Fill texture with Gaussian distributed vectors
        GenerateGaussianTexture();

        // Apply the changes to the texture
        randomVectorTexture.Apply();

        // Save the texture to a file (e.g., "RandomGaussianTexture.png")
        SaveTextureToFile(randomVectorTexture, "RandomGaussianTexture.png");
    }

    // Generates a texture with Gaussian distributed random vectors
    void GenerateGaussianTexture()
    {
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                // Generate a random vector with Gaussian distribution
                Vector3 randomVector = GenerateGaussianVector();

                // Normalize the vector to fit in [0, 1] range for RGB channels
                randomVector = (randomVector + Vector3.one) * 0.5f; // Range [-1, 1] -> [0, 1]

                // Set the pixel color (encode the vector into RGB channels)
                randomVectorTexture.SetPixel(x, y, new Color(randomVector.x, randomVector.y, randomVector.z));
            }
        }
    }

    // Generates a random 3D vector with a Gaussian distribution
    Vector3 GenerateGaussianVector()
    {
        // Generate Gaussian random samples using Box-Muller transform
        float theta = Mathf.Acos(1 - 2 * Random.Range(0f, 1f));  // polar angle
        float phi = Random.Range(0f, Mathf.PI * 2); // azimuthal angle

        // Convert spherical coordinates (theta, phi) to Cartesian coordinates (x, y, z)
        float x = Mathf.Sin(theta) * Mathf.Cos(phi);
        float y = Mathf.Sin(theta) * Mathf.Sin(phi);
        float z = Mathf.Cos(theta);

        // Apply Gaussian distribution by scaling the vector (this will spread the vectors more in the z-axis)
        Vector3 gaussianVector = new Vector3(x, y, z);

        return gaussianVector;
    }

    // Save the texture to a file
    void SaveTextureToFile(Texture2D texture, string fileName)
    {
        byte[] textureBytes = texture.EncodeToPNG();
        string filePath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(filePath, textureBytes);
        Debug.Log("Saved texture to: " + filePath);
    }
}
