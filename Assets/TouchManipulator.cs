using UnityEngine;

[AddComponentMenu("Surface Studio and Dial/Touch Manipulator")]
public class TouchManipulator : MonoBehaviour
{
    // Default values are set to the Store deployment
    // numbers
	public float VelocityStrength = 2f;
	public float VelocityRadius = 0.5f;
	public float ParticleStrength = 10000f;
	public float ParticleRadius = 0.5f;
	public FluidSimulator m_fluid;
	public ParticlesArea m_particlesArea;
	public bool m_alwaysOn = false;

    // Private optimisation
    private Vector2 _previousPosition;
    private RaycastHit _hitInfo;
    private Renderer _particlesRenderer;
    private Renderer _fluidRenderer;
    private Collider _particlesCollider;
    private int _particlesAreaWidth;
    private float _particlesRendererWidth;
    private int _fluidWidth;
    private float _fluidRendererWidth;


    void Start()
    {
        // Storing for optimisation 
        _particlesCollider = m_particlesArea.GetComponent<Collider>();
        _particlesRenderer = m_particlesArea.GetComponent<Renderer>();
        _fluidRenderer = m_fluid.GetComponent<Renderer>();

        // Particle 
        _particlesAreaWidth = m_particlesArea.GetWidth();
        _fluidWidth = m_fluid.GetWidth();
        _fluidRendererWidth = _fluidRenderer.bounds.extents.x * 2f; 
        _particlesRendererWidth = _particlesRenderer.bounds.extents.x * 2f; 
    }

    /// <summary>
    /// Using fixed update as too much particle emission causes different
    /// frame rates, and in turn causes very effects even when taking into 
    /// consideration frame jitter and frame delta time.
    /// </summary>
    void FixedUpdate()
	{
        var _touches = Input.touches;
        var _touchCount = _touches.Length;

        // NOTE: Not using var for clarity (and to avoid precision loss)
        float particleRadius = (ParticleRadius * _particlesAreaWidth) / _particlesRendererWidth;
        float fluidRadius = (VelocityRadius * _fluidWidth) / _fluidRendererWidth;

        // For is a lot faster/memory efficient than foreach in the loop.
        for (var i = 0; i < _touchCount; i++)
        {
            var touch = _touches[i];

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                continue;
            }

            var x = touch.position.x;
            var y = touch.position.y;
            var px = touch.position.x - touch.deltaPosition.x;
            var py = touch.position.y - touch.deltaPosition.y;
            _previousPosition.x = px;
            _previousPosition.y = py;
            
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
            
            if (_particlesCollider.Raycast(ray, out _hitInfo, 100))
            {
                // Add particles
                m_particlesArea.AddParticles(_hitInfo.textureCoord, particleRadius, ParticleStrength * Time.deltaTime);
                
                // Add velocity
                Vector3 direction = (touch.position - _previousPosition) * VelocityStrength * Time.deltaTime;
                m_fluid.AddVelocity(_hitInfo.textureCoord, -direction, fluidRadius);
            }
        }
	}
}
