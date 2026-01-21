using UnityEngine;

/// <summary>
/// Adds a satisfying pop-in animation when creatures spawn.
/// Attach this script to spawned creatures or call it from ARSpawner.
/// </summary>
public class SpawnAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool playParticles = true;
    
    private Vector3 targetScale;
    private float elapsedTime = 0f;
    private bool isAnimating = false;
    
    /// <summary>
    /// Start the spawn animation
    /// </summary>
    public void PlaySpawnAnimation()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        elapsedTime = 0f;
        isAnimating = true;
        
        // Optional: Play particle effect
        if (playParticles)
        {
            PlaySpawnParticles();
        }
    }
    
    private void Update()
    {
        if (!isAnimating) return;
        
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / animationDuration);
        
        // Apply scale curve
        float curveValue = scaleCurve.Evaluate(progress);
        transform.localScale = targetScale * curveValue;
        
        // Add a little bounce at the end
        if (progress > 0.8f)
        {
            float bounce = Mathf.Sin((progress - 0.8f) * Mathf.PI * 5f) * 0.1f;
            transform.localScale = targetScale * (curveValue + bounce);
        }
        
        if (progress >= 1f)
        {
            transform.localScale = targetScale;
            isAnimating = false;
        }
    }
    
    private void PlaySpawnParticles()
    {
        // Create a simple particle burst effect
        GameObject particleObj = new GameObject("SpawnParticles");
        particleObj.transform.position = transform.position;
        
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.maxParticles = 20;
        main.duration = 0.3f;
        main.loop = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 20) 
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.yellow, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        ps.Play();
        
        // Destroy after playing
        Destroy(particleObj, 1f);
    }
}