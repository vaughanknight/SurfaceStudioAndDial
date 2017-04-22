﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Cocuy/Particles Area")]
public class ParticlesArea : MonoBehaviour {
	const int READ = 0;
	const int WRITE = 1;

	public FluidSimulator m_simulation;

    // Colour gradient
	public Gradient m_colourGradient;
    public bool m_updateGradient = false;
	ComputeBuffer m_colourRamp;

	[HideInInspector]
	public ComputeShader m_advectParticles;

	[HideInInspector]
	public ComputeShader m_addParticlesSplat;

	[HideInInspector]
	public float m_densityDissipation = 1f;
	public float Dissipation
	{
		get
		{
			return m_densityDissipation;
		}
		set
		{
			m_densityDissipation = value;
		}
	}

	const int m_nColourRampSize = 256;

	ComputeBuffer[] m_particlesBuffer;
	public ComputeBuffer ParticlesBuffer
	{
		get
		{
			return m_particlesBuffer[READ];
		}
	}

	[HideInInspector]
	public int m_nResolution = 128;
	public int Resolution
	{
		get
		{
			return m_nResolution;
		}
		set
		{
			if (value != m_nResolution)
			{
				m_nResolution = value;
				if (Application.isPlaying && m_particlesBuffer[0] != null && m_particlesBuffer[1] != null)
				{
					int nOldHeight = m_nHeight;
					int nOldWidth = m_nWidth;
					float[] oldParticleData = new float[nOldWidth * nOldHeight];
					m_particlesBuffer[READ].GetData(oldParticleData);
					m_particlesBuffer[0].Dispose();
					m_particlesBuffer[1].Dispose();

					CalculateSize();

					float[] newParticleData = new float[m_nWidth * m_nHeight];
					for (int i = 0; i < m_nHeight; ++i)
					{
						for (int j = 0; j < m_nWidth; ++j)
						{
                            float normX = (float)j / (float)m_nWidth;
                            float normY = (float)i / (float)m_nHeight;
                            int x = (int)(normX * (float)nOldWidth);
                            int y = (int)(normY * (float)nOldHeight);
                            newParticleData[i * m_nWidth + j] = oldParticleData[y * nOldWidth + x];
						}
					}

					m_particlesBuffer = new ComputeBuffer[2];
					for (int i = 0; i < 2; ++i)
					{
						m_particlesBuffer[i] = new ComputeBuffer(m_nWidth * m_nHeight, 4, ComputeBufferType.Default);
					}
                    m_particlesBuffer[READ].SetData(newParticleData);

				}
			}
		}
	}

	int m_nNumGroupsX;
	int m_nNumGroupsY;
	int m_nWidth = 512;
	int m_nHeight = 512;

    // If m_ExposeParticles is true the value of the particles will be cached in memory for use by other systems
    public bool m_CacheParticles = false;
    float[] m_currentParticles;

	void Start()
	{
        if (SystemInfo.graphicsShaderLevel >= 50)
        {
		    CalculateSize();
		    LinkToFluidSimulation();
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogError("<CocuyError> Cocuy needs DirectX 11 to work. If supported, you need to enable it in yout project settings.");
        }
	}

    // Get velocity at the (x,y) coordinate
    public float GetParticles(int x, int y)
    {
        return m_currentParticles[y * m_nWidth + x];
    }

	private void CalculateSize()
	{
		float x = transform.lossyScale.x;
		float y = transform.lossyScale.z;
		if (x > y)
		{
			float fHeight = (y / x) * (float)m_nResolution;
			m_nWidth = m_nResolution;
			m_nHeight = (int)fHeight;
		}
		else
		{
			float fWidth = (x / y) * (float)m_nResolution;
			m_nWidth = (int)fWidth;
			m_nHeight = m_nResolution;
		}

		SetSizeInShaders();
		const int nNumThreads = 32;
		m_nNumGroupsX = Mathf.CeilToInt((float)m_nWidth / (float)nNumThreads);
		m_nNumGroupsY = Mathf.CeilToInt((float)m_nHeight / (float)nNumThreads);
        m_currentParticles = new float[m_nWidth * m_nHeight];
	}

	private void LinkToFluidSimulation()
	{
		float fResolutionRatio = m_simulation.Resolution / (float)m_nResolution;
		m_simulation.SetSize((int)(m_nWidth * fResolutionRatio), (int)(m_nHeight * fResolutionRatio));

		m_simulation.InitShaders();
		m_advectParticles.SetInts("_VelocitySize", new int[] { m_simulation.GetWidth(), m_simulation.GetHeight() });
	}

	void FlipBuffers(ComputeBuffer[] buffers)
	{
		ComputeBuffer aux = buffers[0];
		buffers[0] = buffers[1];
		buffers[1] = aux;
	}

	public Vector2 GetRenderScale()
	{
		return transform.lossyScale;
	}

	public Vector2 GetRenderSize()
	{
		return GetComponent<Renderer>().bounds.size;
	}

	public int GetWidth()
	{
		return m_nWidth;
	}

	public int GetHeight()
	{
		return m_nHeight;
	}

	void Update()
	{
        if (m_particlesBuffer == null)
        {
            m_particlesBuffer = new ComputeBuffer[2];
            for (int i = 0; i < 2; ++i)
            {
                m_particlesBuffer[i] = new ComputeBuffer(m_nWidth * m_nHeight, 4, ComputeBufferType.Default);
            }
        }

        m_advectParticles.SetFloat("_Dissipation", m_densityDissipation);
        m_advectParticles.SetFloat("_ElapsedTime", Time.deltaTime);
        m_advectParticles.SetFloat("_Speed", m_simulation.GetSimulationSpeed());

        // ADVECT (Particles)
        m_advectParticles.SetBuffer(0, "_Obstacles", m_simulation.ObstaclesBuffer);
        m_advectParticles.SetBuffer(0, "_Velocity", m_simulation.VelocityBuffer);
        m_advectParticles.SetBuffer(0, "_ParticlesIn", m_particlesBuffer[READ]);
        m_advectParticles.SetBuffer(0, "_ParticlesOut", m_particlesBuffer[WRITE]);
        m_advectParticles.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
        FlipBuffers(m_particlesBuffer);

        if (m_colourRamp == null)
        {
            m_colourRamp = new ComputeBuffer(m_nColourRampSize, 16, ComputeBufferType.Default);
            UpdateGradient();
        }

        if(m_updateGradient)
        {
            UpdateGradient();
        }

        //RENDER
        GetComponent<Renderer>().material.SetBuffer("_Particles", m_particlesBuffer[READ]);
        GetComponent<Renderer>().material.SetBuffer("_ColourRamp", m_colourRamp);
        GetComponent<Renderer>().material.SetVector("_Size", new Vector2(m_nWidth, m_nHeight));

        // Cache particles
        if (m_CacheParticles)
        {
            m_particlesBuffer[READ].GetData(m_currentParticles);
        }
	}

    private void UpdateGradient()
    {
        Vector4[] colourData = new Vector4[m_nColourRampSize];
        for (int i = 0; i < m_nColourRampSize; ++i)
        {
            colourData[i] = m_colourGradient.Evaluate(i / 255.0f);
        }
        m_colourRamp.SetData(colourData);
    }

	void SetSizeInShaders()
	{
		m_advectParticles.SetInts("_ParticleSize", new int[] { m_nWidth, m_nHeight });
		m_addParticlesSplat.SetInts("_Size", new int[] { m_nWidth, m_nHeight });
		GetComponent<Renderer>().material.SetVector("_Size", new Vector2(m_nWidth, m_nHeight));
	}

	// Adds a particles splat to the simulation at /position with a specific /fRadius and /fStrength.
	// /position: Centre of the splat as a normalised value [0-1] of the simulation space.
	// /fRadius: Radius of the splat in simulation space.
	// /fStrength: Strength at the centre of the splat. The value is linearly diffused towards the edges of the splat.
	public void AddParticles(Vector2 position, float fRadius, float fStrength)
	{
		if (m_addParticlesSplat != null && m_particlesBuffer != null && m_particlesBuffer.Length >= 2)
		{
			float[] pos = { position.x, position.y };
			m_addParticlesSplat.SetFloats("_Position", pos);
			m_addParticlesSplat.SetFloat("_Value", fStrength);
			m_addParticlesSplat.SetFloat("_Radius", fRadius);
			m_addParticlesSplat.SetInts("_Size", new int[] { m_nWidth, m_nHeight });
			m_addParticlesSplat.SetBuffer(0, "_ParticlesIn", m_particlesBuffer[READ]);
			m_addParticlesSplat.SetBuffer(0, "_ParticlesOut", m_particlesBuffer[WRITE]);
			m_addParticlesSplat.Dispatch(0, m_nNumGroupsX, m_nNumGroupsY, 1);
			FlipBuffers(m_particlesBuffer);
			GetComponent<Renderer>().material.SetBuffer("_Particles", m_particlesBuffer[READ]);
		}
	}

	void OnDisable()
	{
		if (m_colourRamp != null)
		{
			m_colourRamp.Dispose();
		}
		if (m_particlesBuffer != null && m_particlesBuffer.Length == 2)
		{
			if (m_particlesBuffer[0] != null)
			{
				m_particlesBuffer[0].Dispose();
			}
			if (m_particlesBuffer[1] != null)
			{
				m_particlesBuffer[1].Dispose();
			}
		}
	}

	void OnDrawGizmos()
	{
		if (GetComponent<Renderer>() != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, GetComponent<Renderer>().bounds.extents * 2f);
		}
	}
}