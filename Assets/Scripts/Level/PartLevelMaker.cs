

using Ingame;
using Level;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Rigidbody2D)),
    RequireComponent(typeof(PolygonCollider2D))
    , RequireComponent(typeof(SpriteNameUpdater)), RequireComponent(typeof(SpriteChangeNotifier))]

public class PartLevelMaker : BasePart
{
    
    public override void Awake()
    {
        base.Awake();
        Body.bodyType = RigidbodyType2D.Kinematic;
    }
    public PartLevelMaker(Rigidbody2D body, SpriteRenderer renderer, PolygonCollider2D collider) : base(body, renderer, collider)
    {
    }
}