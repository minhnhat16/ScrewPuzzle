using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeObject : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 position;
    private HingeJoint2D _hingeJoint2D;
    private Transform _transform;

    public Rigidbody Rb
    {
        get => rb;
        set => rb = value;
    }

    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

    public HingeJoint2D HingeJoint2D
    {
        get => _hingeJoint2D;
        set => _hingeJoint2D = value;
    }

    public Transform Transform
    {
        get => _transform;
        set => _transform = value;
    }

    public HingeObject(Rigidbody rb, Vector3 position, HingeJoint2D hingeJoint2D, Transform transform)
    {
        this.rb = rb;
        this.position = position;
        _hingeJoint2D = hingeJoint2D;
        _transform = transform;
    }
}
