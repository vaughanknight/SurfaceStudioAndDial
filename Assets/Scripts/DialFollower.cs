using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the dial particle emitter follow a path 
/// using a basic sin/cos formula.  Movement is semi random
/// with the intention that the dial creates colour across the screen.
/// </summary>
public class DialFollower : MonoBehaviour {
    public float HorizontalMovement;
    public float VerticalMovement;
    public float HorizontalSpeed = 5f;
    public float VerticalSpeed = 5f;
    public float RotationSpeed = 5f;
    private Transform _transform;
    public float MoveInterval = 7f;
    private Vector2 NextPosition = new Vector2(0, 0);
    public ParticlesAreaManipulator _ParticleAreaManipulator;

    /// <summary>
    /// How long it generates particles for each move
    /// </summary>
    public float _DrawTime = 0.8f;

    void Start ()
    {
        _transform = this.transform;
        StartMovement(0);
    }

    /// <summary>
    /// Start moving the emitter after the delay
    /// </summary>
    /// <param name="delay">Delay in seconds</param>
    private void StartMovement(float delay)
    {
        NextPosition = CalculateNextPosition();
        iTween.ValueTo(this.gameObject, iTween.Hash("from", 0, "to", 1f, "time", _DrawTime, "delay", delay, "onupdate", "OnValueUpdate", "oncomplete", "OnValueComplete"));
    }

    /// <summary>
    /// Esnure we are emitting on each value update, and 
    /// lerp the emitter towards the target position
    /// </summary>
    /// <param name="f">Not used at this stage.</param>
    public void OnValueUpdate(float f)
    {
        _ParticleAreaManipulator.m_strength = 10000f;
        MoveEmitter();
    }

    /// <summary>
    /// On complete stop emitting and trigger again
    /// with a delay
    /// </summary>
    public void OnValueComplete()
    {
        _ParticleAreaManipulator.m_strength = 0f;
        StartMovement(MoveInterval);
    }


	// Update is called once per frame
	void FixedUpdate ()
    {
        
    }

    /// <summary>
    /// Calculates the next position based on some simple maths
    /// </summary>
    private Vector2 CalculateNextPosition()
    {
        var x = Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI / HorizontalSpeed) * HorizontalMovement;
        var y = Mathf.Cos(Time.timeSinceLevelLoad * Mathf.PI / VerticalSpeed) * VerticalMovement;

        return new Vector2(x, y);
    }

    /// <summary>
    /// Move the emitter towards next position through lerp
    /// </summary>
    private void MoveEmitter()
    {
        var p = _transform.position;

        var dx = Mathf.Lerp(p.x, NextPosition.x, 0.05f);
        var dy = Mathf.Lerp(p.y, NextPosition.y, 0.05f);

        _transform.position = new Vector3(dx, dy, p.z);
        _transform.Rotate(Vector3.back, Mathf.PI / (Time.deltaTime * RotationSpeed));
    }
}
