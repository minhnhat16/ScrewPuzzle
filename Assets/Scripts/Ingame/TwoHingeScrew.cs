using System.Collections;
using System.Collections.Generic;
using Ingame;
using UnityEngine;

namespace Ingame
{
public class TwoHingeScrew : Screw
{
    [SerializeField]  private HingeJoint2D _2ndhingeJoint2D;

    public override void Awake()
    {
        _transform = GetComponent<Transform>();
        position = GetComponent<Transform>().position;
        // HingeJoint2D = GetComponent<HingeJoint2D>();
        CircleCollider2D = GetComponent<CircleCollider2D>();
        Renderer = GetComponentInChildren<SpriteRenderer>();
        LayerMask = gameObject.layer;
        // _2ndhingeJoint2D = GetComponent<HingeJoint2D>();
    }
    // Start is called before the first frame update
    public override void FreeHinge()
    {
        Debug.Log("FreeHinge in two hinge screw");
        CircleCollider2D.isTrigger = true;
        HingeJoint2D.connectedBody = null;
        _2ndhingeJoint2D.connectedBody = null;
    }
}
}

