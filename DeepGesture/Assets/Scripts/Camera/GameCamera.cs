#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

public class GameCamera : MonoBehaviour {

    public Transform Target = null;
    public Vector3 SelfOffset = Vector3.zero;
    public Vector3 TargetOffset = Vector3.zero;

    [Range(0f, 1f)] public float Smoothing = 0f;
    [Range(0f, 10f)] public float FOV = 5.0f; // 1.25f;

    private Camera Camera;

    private Vector3 PreviousTarget;

    void Awake() {
        Camera = GetComponent<Camera>();
        PreviousTarget = Target.position;
        SelfOffset = new Vector3(0.0f, 0.25f, 0.5f);
        TargetOffset = new Vector3(0.0f, 0.8f, 0.0f);
        FOV = 5.0f;
    }

    void Update() {
        Vector3 target = Vector3.Lerp(PreviousTarget, Target.position, 1f - Smoothing);
        PreviousTarget = target;
        transform.position = target + FOV * SelfOffset;
        transform.LookAt(target + TargetOffset);
    }
}
#endif