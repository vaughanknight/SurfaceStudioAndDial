using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript;

public class TouchData
{
    private Vector3 m_previousMousePosition;
}

[AddComponentMenu("Cocuy/Touch Manipulator")]
public class TouchManipulator : MonoBehaviour {
    //private Dictionary<int, TouchData> PreviousTouchData = new Dictionary<int, TouchData>();

    //private Vector3 m_previousMousePosition;

	public float VelocityStrength = 10f;
	
	public float VelocityRadius = 0.5f;

	public float ParticleStrength = 2000f;
	public float ParticleRadius = 0.5f;

	public FluidSimulator m_fluid;
	public ParticlesArea m_particlesArea;
	public bool m_alwaysOn = false;

	void DrawGizmo()
	{
		float col = ParticleStrength / 10000.0f;
		Gizmos.color = Color.Lerp(Color.yellow, Color.red, col);
		Gizmos.DrawWireSphere(transform.position, ParticleRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, VelocityRadius);
	}

	void OnDrawGizmosSelected()
	{
		DrawGizmo();
	}

	void OnDrawGizmos()
	{

		DrawGizmo();
	}

    private IList<TouchPoint> _touches = new List<TouchPoint>();

    void Update()
    {
        
    }

    private Vector2 _previousPosition;
    private RaycastHit _hitInfo;
    private Renderer _particlesRenderer;
    private Renderer _fluidRenderer;
    private Collider _particlesCollider;

    void Start()
    {
        _particlesCollider = m_particlesArea.GetComponent<Collider>();
        _particlesRenderer = m_particlesArea.GetComponent<Renderer>();
        _fluidRenderer = m_fluid.GetComponent<Renderer>();
    }

    void FixedUpdate()
	{
        var _touches = Input.touches;
        var _touchCount = _touches.Length;

        // Semi optimised, removing this from the for loop
        int particleAreaWidth = m_particlesArea.GetWidth();
        //int fluiWidth = m_fluid.GetWidth();
        float particleWidth = _particlesRenderer.bounds.extents.x * 2f;
        float particleRadius = (ParticleRadius * particleAreaWidth) / particleWidth;
        float fluidRendererWidth = _fluidRenderer.bounds.extents.x * 2f;
        float fluidRadius = (VelocityRadius * m_fluid.GetWidth()) / fluidRendererWidth;

        // For is a lot more performance than foreach in the loop.
        // Many theories on why (heap vs stack allocations), but it's proven in testing
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

  //      if (Input.GetMouseButton(0) || m_alwaysOn)
  //      {
		//	//Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		//	RaycastHit hitInfo = new RaycastHit();
		//	if (m_particlesArea.GetComponent<Collider>().Raycast(ray, out hitInfo, 100))
		//	{
		//		float fWidth = m_particlesArea.GetComponent<Renderer>().bounds.extents.x * 2f;
		//		float fRadius = (m_particlesRadius * m_particlesArea.GetWidth()) / fWidth;
		//		m_particlesArea.AddParticles(hitInfo.textureCoord, fRadius, m_particlesStrength * Time.deltaTime);
		//	}
		//}

		//if (Input.GetMouseButtonDown(0))
		//{
		//	m_previousMousePosition = Input.mousePosition;
		//}

		//if (Input.GetMouseButton(0) || m_alwaysOn)
		//{
		//	Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		//	RaycastHit hitInfo = new RaycastHit();
  //          if (m_fluid.GetComponent<Collider>().Raycast(ray, out hitInfo, 100))
		//	{
		//		Vector3 direction = (Input.mousePosition - m_previousMousePosition) * m_velocityStrength * Time.deltaTime;
		//		float fWidth = m_fluid.GetComponent<Renderer>().bounds.extents.x * 2f;
  //              float fRadius = (m_velocityRadius * m_fluid.GetWidth()) / fWidth;

		//		if (Input.GetMouseButton(0))
		//		{
  //                  m_fluid.AddVelocity(hitInfo.textureCoord, -direction, fRadius);
		//		}
		//		else
		//		{
  //                  m_fluid.AddVelocity(hitInfo.textureCoord, direction, fRadius);

		//		}
		//	}
		//	m_previousMousePosition = Input.mousePosition;
		//}
	}
}
