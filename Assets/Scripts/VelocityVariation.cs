using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityVariation : MonoBehaviour {

    private VelocityManipulator _VelocityManipulator;
    public float _Maximum;
    public float _Speed;

    //private static Random _random = new Random();

	// Use this for initialization
	void Start () {
        _VelocityManipulator = gameObject.GetComponent<VelocityManipulator>();
	}
	
	// Update is called once per frame
	void Update () {
        _Speed = Random.value * _Maximum;
        _VelocityManipulator.m_fluidVelocitySpeed = _Speed;
	}
}
