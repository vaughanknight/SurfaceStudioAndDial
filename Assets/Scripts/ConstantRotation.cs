using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behaviour for constant rotation.  Was not used in the final
/// demo but intention was to use this when in "idle mode" state
/// </summary>
public class ConstantRotation : MonoBehaviour {

    public float Speed;
    public Vector3 Axis = Vector3.forward;
    private Transform _thisTransform;

	void Start () {
        _thisTransform = this.transform;
	}
    
	void Update () {
        var t = Time.timeSinceLevelLoad;
        var s = Mathf.Sin(t * Mathf.PI * Speed) * 90f + t * 45;
        _thisTransform.localRotation = Quaternion.AngleAxis(s, Axis);
	}
}
