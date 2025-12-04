using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ingame.Screw
{
    public abstract class HingeController : MonoBehaviour
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private Rigidbody2D bodyConnect = new();
        [SerializeField] private HingeJoint2D hingeJoint2D = new();

        public LayerMask Layer
        {
            get => layer;
            set => layer = value;
        }
        public Rigidbody2D BodyConnect { get => bodyConnect; set => bodyConnect = value; }
        public HingeJoint2D HingeJoint2D { get => hingeJoint2D; set => hingeJoint2D = value; }




        //======================================================================
        // INITIALIZATION
        //======================================================================

        public virtual void Start()
        {
            var currentScene = SceneManager.GetActiveScene();

            if (!currentScene.name.Equals("LevelMaker"))
            {
                InitHingeJoints();
            }
        }

        public virtual void InitHingeJoints()
        {
            var hinge = hingeJoint2D;
            var body = bodyConnect;



            if (hinge && body)
            {
                hinge.connectedBody = body;
                hinge.gameObject.name = body.gameObject.name + "_Hinge";

                // REGISTER hinge vào ScrewManager
                ScrewManager sm = LevelManager.ins.ScrewManager;
                if (sm != null)
                    sm.AddHingeConnection(hinge, body.GetComponent<BasePart>());
            }
        }


        //======================================================================
        // FREE HINGES (DETACH + REMOVE FROM MANAGER + RETURN POOL)
        //======================================================================

        public virtual void FreeHinges()
        {
            ScrewManager sm = LevelManager.ins.ScrewManager;

            var hinge = hingeJoint2D;
            if (hinge == null) return;
            var body = hinge.connectedBody;
            // 1) unregister khỏi ScrewManager
            sm?.RemoveHingeConnection(hinge);

            // 2) tách hinge
            hinge.connectedBody = null;

            // 4) Return hinge về pool
            if (hinge.TryGetComponent<HingeObject>(out var hingeObj))
                HingePool.Instance.pool.ReturnToPool(hingeObj);
        }


        //======================================================================
        // CLEAR
        //======================================================================

        public virtual void ClearBody()
        {
            FreeHinges();
        }

        internal void Reset()
        {
            ClearBody();
        }


        //======================================================================
        // UTILS
        //======================================================================

        public virtual string GetStringBodyLayer(int index)
        {
            if (index < 0 ) return "";
            return LayerMask.LayerToName(bodyConnect.gameObject.layer);
        }

        public virtual int GetIntBodyLayer(int index)
        {
            if (index < 0 ) return -1;
            return bodyConnect?.gameObject.layer ?? -1;
        }

        public string GetConnectedBodyRenderLayer(int index)
        {
            if (index < 0 ) return null;

            var sr = bodyConnect.GetComponentInChildren<SpriteRenderer>();
            return sr ? sr.sortingLayerName : null;
        }
    }
}
