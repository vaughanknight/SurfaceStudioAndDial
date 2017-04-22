using UnityEngine;
using System.Collections;

[AddComponentMenu("Cocuy/Simulator")]
public class FluidSimulator : MonoBehaviour {
	const int READ = 0;
	const int WRITE = 1;

	[HideInInspector]
	public ComputeShader m_addVelocitySplat;
	[HideInInspector]
	public ComputeShader m_initBoundaries;
	[HideInInspector]
	public ComputeShader m_advectVelocity;
	[HideInInspector]
	public ComputeShader m_divergence;
	[HideInInspector]
	public ComputeShader m_clearPressure;
	[HideInInspector]
	public ComputeShader m_poisson;
	[HideInInspector]
	public ComputeShader m_substractGradient;
	[HideInInspector]
	public ComputeShader m_calcVorticity;
	[HideInInspector]
	public ComputeShader m_applyVorticityForce;
	[HideInInspector]
	public ComputeShader m_viscosityCompute;
	[HideInInspector]
	public ComputeShader m_clearObstacles;
	[HideInInspector]
	public ComputeShader m_addObstacleCircle;
	[HideInInspector]
	public ComputeShader m_addObstacleTriangle;

	public bool m_simulate = true;

	[HideInInspector]
	public float m_speed = 500f;
	public float Speed
	{
		get
		{
			return m_speed;
		}
		set
		{
			m_speed = value;
		}
	}

	[HideInInspector]
	public int m_iterations = 50;
	public int Iterations
	{
		get
		{
			return m_iterations;
		}
		set
		{
			m_iterations = value;
		}
	}

	[HideInInspector]
	public float m_velocityDissipation = 1f;
	public float VelocityDissipation
	{
		get
		{
			return m_velocityDissipation;
		}
		set
		{
			m_velocityDissipation = value;
		}
	}

	[HideInInspector]
	public float m_vorticityScale = 0f;
	public float Vorticity
	{
		get
		{
			return m_vorticityScale;
		}
		set
		{
			m_vorticityScale = value;
		}
	}

	[HideInInspector]
	public float m_viscosity = 0f;
	public float Viscosity
	{
		get
		{
			return m_viscosity;
		}
		set
		{
			m_viscosity = value;
		}
	}

	[HideInInspector]
	public int m_nResolution = 512;
	public int Resolution
	{
		get
		{
			return m_nResolution;
		}
		set
		{
			m_nResolution = value;
		}
	}

	ComputeBuffer[] m_velocityBuffer;
	public ComputeBuffer VelocityBuffer
	{
		get
		{
			return m_velocityBuffer[READ];
		}
	}

	ComputeBuffer m_divergenceBuffer;
	public ComputeBuffer DivergenceBuffer
	{
		get
		{
			return m_divergenceBuffer;
		}
	}

	ComputeBuffer[] m_pressure;
	public ComputeBuffer PressureBuffer
	{
		get
		{
			return m_pressure[READ];
		}
	}

	ComputeBuffer m_obstaclesBuffer;
	public ComputeBuffer ObstaclesBuffer
	{
		get
		{
			return m_obstaclesBuffer;
		}
	}

	ComputeBuffer m_vorticityBuffer;

    [HideInInspector]
    public bool m_CacheVelocity = false;
	Vector2[] m_currentVelocity;

	int m_nNumCells;
	int m_nNumGroupsX;
	int m_nNumGroupsY;
	int m_nWidth = 512;
	int m_nHeight = 512;

	//-------------------------------------------
	void Start()
	{
		if (SystemInfo.graphicsShaderLevel >= 50)
		{
			m_nNumCells = m_nWidth * m_nHeight;
		}
		else
		{
			m_simulate = false;
			Debug.LogError("<CocuyError> Cocuy needs DirectX 11 to work. If supported, you need to enable it in yout project settings.");
		}
	}

	// Sets the size of the simulation
	// /nWidth: Width of the simulation
	// /nHeight: Height of the simulation
	public void SetSize(int nWidth, int nHeight)
	{
		const int nNumThreads = 32;
		m_nWidth = nWidth;
		m_nHeight = nHeight;
		m_nNumCells = m_nWidth * m_nHeight;
		m_nNumGroupsX = Mathf.CeilToInt((float)m_nWidth / (float)nNumThreads);
		m_nNumGroupsY = Mathf.CeilToInt((float)m_nHeight / (float)nNumThreads);
	}

	// Get the width of the simulation
	public int GetWidth()
	{
		return m_nWidth;
	}

	// Get the height of the simulation
	public int GetHeight()
	{
		return m_nHeight;
	}

	// Adds a velocity splat to the simulation at /position with a specific /fRadius and /velocity.
	// /position: Centre of the splat as a normalised value [0-1] of the simulation space.
	// /fRadius: Radius of the splat in simulation space.
	// /velocity: Velocity at the centre of the splat. The value is linearly diffused towards the edges of the splat.
	public void AddVelocity(Vector2 position, Vector2 velocity, float fRadius)
	{
		if (m_addVelocitySplat != null && m_velocityBuffer != null && m_velocityBuffer.Length >= 2)
		{
			float[] pos = { position.x, position.y };
			m_addVelocitySplat.SetFloats("_Position", pos);
			float[] val = { velocity.x, velocity.y};
			m_addVelocitySplat.SetFloats("_Value", val);
			m_addVelocitySplat.SetFloat("_Radius", fRadius);
			m_addVelocitySplat.SetInts("_Size", new int[] { m_nWidth, m_nHeight });
			m_addVelocitySplat.SetBuffer(0, "_VelocityIn", m_velocityBuffer[READ]);
			m_addVelocitySplat.SetBuffer(0, "_VelocityOut", m_velocityBuffer[WRITE]);
			m_addVelocitySplat.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
			FlipBuffers(m_velocityBuffer);
		}
	}

    // Deprecated
	public void AddObstacle(Vector2 position, float fRadius)
	{
        AddObstacleCircle(position, fRadius);
	}

    // position in normalised local space
    // fRadius in world space
    public void AddObstacleCircle(Vector2 position, float fRadius, bool bStatic = false)
    {
        if (m_addObstacleCircle != null)
        {
            float[] pos = { position.x, position.y };
            m_addObstacleCircle.SetFloats("_Position", pos);
            m_addObstacleCircle.SetFloat("_Radius", fRadius);
            m_addObstacleCircle.SetInt("_Static", bStatic ? 1 : 0);
            m_addObstacleCircle.SetInts("_Size", new int[] { m_nWidth, m_nHeight });
            m_addObstacleCircle.SetBuffer(0, "_Buffer", m_obstaclesBuffer);
            m_addObstacleCircle.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
        }      
    }

    // points in normalised local space
    public void AddObstacleTriangle(Vector2 p1, Vector2 p2, Vector2 p3, bool bStatic = false)
    {
        if (m_addObstacleTriangle != null)
        {
            float[] pos1 = { p1.x, p1.y };
            float[] pos2 = { p2.x, p2.y };
            float[] pos3 = { p3.x, p3.y };
            m_addObstacleTriangle.SetFloats("_P1", pos1);
            m_addObstacleTriangle.SetFloats("_P2", pos2);
            m_addObstacleTriangle.SetFloats("_P3", pos3);
            m_addObstacleTriangle.SetInt("_Static", bStatic ? 1 : 0);
            m_addObstacleTriangle.SetInts("_Size", new int[] { m_nWidth, m_nHeight });
            m_addObstacleTriangle.SetBuffer(0, "_Buffer", m_obstaclesBuffer);
            m_addObstacleTriangle.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
        }
    }

	// Get velocity at the (x,y) coordinate
	public Vector2 GetVelocity(int x, int y)
	{
		return m_currentVelocity[y * m_nWidth + x] * m_speed;
	}

	void UpdateParams()
	{
		m_advectVelocity.SetFloat("_Dissipation", m_velocityDissipation);
		m_advectVelocity.SetFloat("_ElapsedTime", Time.deltaTime);
		m_advectVelocity.SetFloat("_Speed", m_speed);
	}

	void Update()
	{
		if (m_simulate)
		{
			UpdateParams();
			CreateBuffersIfNeeded();

			// INIT BOUNDARIES
			m_initBoundaries.SetBuffer(0, "_Velocity", m_velocityBuffer[READ]);
			m_initBoundaries.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);

			// ADVECT
			m_advectVelocity.SetBuffer(0, "_Obstacles", m_obstaclesBuffer);
			m_advectVelocity.SetBuffer(0, "_VelocityIn", m_velocityBuffer[READ]);
			m_advectVelocity.SetBuffer(0, "_VelocityOut", m_velocityBuffer[WRITE]);
			m_advectVelocity.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
			FlipBuffers(m_velocityBuffer);

			// VORTICITY CONFINEMENT
			// Calculate vorticity
			m_calcVorticity.SetBuffer(0, "_Velocity", m_velocityBuffer[READ]);
			m_calcVorticity.SetBuffer(0, "_Output", m_vorticityBuffer);
			m_calcVorticity.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);

			// Apply vorticity force
			m_applyVorticityForce.SetBuffer(0, "_VelocityIn", m_velocityBuffer[READ]);
			m_applyVorticityForce.SetBuffer(0, "_Vorticity", m_vorticityBuffer);
			m_applyVorticityForce.SetBuffer(0, "_VelocityOut", m_velocityBuffer[WRITE]);
			m_applyVorticityForce.SetFloat("_VorticityScale", m_vorticityScale);
			m_applyVorticityForce.SetFloat("_ElapsedTime", Time.deltaTime);
			m_applyVorticityForce.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
			FlipBuffers(m_velocityBuffer);

			// VISCOSITY
			if (m_viscosity > 0.0f)
			{
				for (int i = 0; i < m_iterations; ++i)
				{
					float centreFactor = 1.0f / (m_viscosity);
					float stencilFactor = 1.0f / (4.0f + centreFactor);
					m_viscosityCompute.SetFloat("_Alpha", centreFactor);
					m_viscosityCompute.SetFloat("_rBeta", stencilFactor);
					m_viscosityCompute.SetBuffer(0, "_Velocity", m_velocityBuffer[READ]);
					m_viscosityCompute.SetBuffer(0, "_Output", m_velocityBuffer[WRITE]);
					m_viscosityCompute.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
					FlipBuffers(m_velocityBuffer);
				}
			}

			// DIVERGENCE
			m_divergence.SetBuffer(0, "_Velocity", m_velocityBuffer[READ]);
			m_divergence.SetBuffer(0, "_Obstacles", m_obstaclesBuffer);
			m_divergence.SetBuffer(0, "_Divergence", m_divergenceBuffer);
			m_divergence.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);

			// CLEAR PRESSURE
			m_clearPressure.SetBuffer(0, "_Buffer", m_pressure[READ]);
			m_clearPressure.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);

			// POISSON
			for (int i = 0; i < m_iterations; ++i)
			{
				m_poisson.SetBuffer(0, "_Divergence", m_divergenceBuffer);
				m_poisson.SetBuffer(0, "_Pressure", m_pressure[READ]);
				m_poisson.SetBuffer(0, "_Output", m_pressure[WRITE]);
				m_poisson.SetBuffer(0, "_Obstacles", m_obstaclesBuffer);
				m_poisson.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
				FlipBuffers(m_pressure);
			}

			// SUBSTRACT GRADIENT
			m_substractGradient.SetBuffer(0, "_Pressure", m_pressure[READ]);
			m_substractGradient.SetBuffer(0, "_VelocityIn", m_velocityBuffer[READ]);
			m_substractGradient.SetBuffer(0, "_VelocityOut", m_velocityBuffer[WRITE]);
			m_substractGradient.SetBuffer(0, "_Obstacles", m_obstaclesBuffer);
			m_substractGradient.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
			FlipBuffers(m_velocityBuffer);

			// CLEAR BUFFERS
			m_clearObstacles.SetBuffer(0, "_Buffer", m_obstaclesBuffer);
			m_clearObstacles.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);

			// Cache velocities
            if (m_CacheVelocity)
            {
                m_velocityBuffer[READ].GetData(m_currentVelocity);
            }
		}
	}

	public void InitShaders()
	{
		CreateBuffersIfNeeded();
		UpdateParams();
		int[] size = new int[] { m_nWidth, m_nHeight };
		m_initBoundaries.SetInts("_Size", size);
		m_advectVelocity.SetInts("_Size", size);
		m_divergence.SetInts("_Size", size);
		m_clearPressure.SetInts("_Size", size);
		m_poisson.SetInts("_Size", size);
		m_substractGradient.SetInts("_Size", size);
		m_addVelocitySplat.SetInts("_Size", size);
		m_addObstacleCircle.SetInts("_Size", size);
        m_addObstacleTriangle.SetInts("_Size", size);
		m_viscosityCompute.SetInts("_Size", size);
		m_calcVorticity.SetInts("_Size", size);
		m_applyVorticityForce.SetInts("_Size", size);
		m_clearObstacles.SetInts("_Size", size);
		m_currentVelocity = new Vector2[m_nNumCells];
	}

	void CreateBuffersIfNeeded()
	{
		if (m_velocityBuffer == null)
		{
			m_velocityBuffer = new ComputeBuffer[2];
			for (int i = 0; i < 2; ++i)
			{
				m_velocityBuffer[i] = new ComputeBuffer(m_nNumCells, 8, ComputeBufferType.Default);
			}
		}
		if (m_divergenceBuffer == null)
		{
			m_divergenceBuffer = new ComputeBuffer(m_nNumCells, 4, ComputeBufferType.Default);
		}
		if (m_pressure == null)
		{
			m_pressure = new ComputeBuffer[2];
			for (int i = 0; i < 2; ++i)
			{
				m_pressure[i] = new ComputeBuffer(m_nNumCells, 4, ComputeBufferType.Default);
			}
		}
		if (m_obstaclesBuffer == null)
		{
			m_obstaclesBuffer = new ComputeBuffer(m_nNumCells, 8, ComputeBufferType.Default);
		}
		if (m_vorticityBuffer == null)
		{
			m_vorticityBuffer = new ComputeBuffer(m_nNumCells, 4, ComputeBufferType.Default);
		}
	}

	public float GetSimulationSpeed()
	{
		return m_speed;
	}

	void OnDisable()
	{
		if (m_velocityBuffer != null && m_velocityBuffer.Length == 2)
		{
			if (m_velocityBuffer[0] != null)
			{
				m_velocityBuffer[0].Dispose();
			}
			if (m_velocityBuffer[1] != null)
			{
				m_velocityBuffer[1].Dispose();
			}
		}
		if (m_divergenceBuffer != null)
		{
			m_divergenceBuffer.Dispose();
		}
		if (m_pressure != null && m_pressure.Length == 2)
		{
			if (m_pressure[0] != null)
			{
				m_pressure[0].Dispose();
			}
			if (m_pressure[1] != null)
			{
				m_pressure[1].Dispose();
			}
		}
		if (m_obstaclesBuffer != null)
		{
			m_obstaclesBuffer.Dispose();
		}
		if (m_vorticityBuffer != null)
		{
			m_vorticityBuffer.Dispose();
		}
	}

	void FlipBuffers(ComputeBuffer[] buffers)
	{
		ComputeBuffer aux = buffers[0];
		buffers[0] = buffers[1];
		buffers[1] = aux;
	}
}
