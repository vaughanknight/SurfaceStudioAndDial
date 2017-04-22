using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour {

    public float Speed;
    public Vector3 Axis = Vector3.forward;
    private Transform _thisTransform;

	// Use this for initialization
	void Start () {
        _thisTransform = this.transform;
	}
    
	// Update is called once per frame
	void Update () {
        var t = Time.timeSinceLevelLoad;
        var s = Mathf.Sin(t * Mathf.PI * Speed) * 90f + t * 45;
        _thisTransform.localRotation = Quaternion.AngleAxis(s, Axis);
	}
}
