using Ingame.Screw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(HingeJoint2D))]
public class HingeObject : HingeController, IResetable
{

    public void OnReset()
    {
        ClearBody();
        Debug.Log("[HingeObject] OnReset: " + name);    
        HingePool.Instance.pool.ReturnToPool(this);
    }
}
