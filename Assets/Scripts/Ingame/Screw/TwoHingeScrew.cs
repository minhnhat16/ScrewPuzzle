using UnityEngine;

namespace Ingame.Screw
{
public class TwoHingeScrew : Screw
{
    public override void Awake()
    {
        _transform = GetComponent<Transform>();
        position = GetComponent<Transform>().position;
        // HingeJoint2D = GetComponent<HingeJoint2D>();
        CircleCollider2D = GetComponentInChildren<CircleCollider2D>();
        Renderer = GetComponentInChildren<SpriteRenderer>();
        LayerMask = gameObject.layer;
        // _2ndhingeJoint2D = GetComponent<HingeJoint2D>();
    }
    // Start is called before the first frame update
    public override void FreeHinge()
    {
        Debug.Log("FreeHinge in two hinge screw");
        CircleCollider2D.isTrigger = true;
        _hingeController.FreeHinges();
    }
}
}

