using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ingame.Screw
{
    public abstract class HingeController : MonoBehaviour
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private Rigidbody2D bodyConnect;
        [SerializeField] private HingeJoint2D hingeJoint2D;

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
            // Ensure references are assigned (fix null errors after reload)
            if (hingeJoint2D == null)
                hingeJoint2D = GetComponent<HingeJoint2D>();

            var hinge = hingeJoint2D;
            var body = bodyConnect;

            if (hinge && body)
            {
                hinge.connectedBody = body;
                hinge.gameObject.name = body.gameObject.name + "_Hinge";

                ScrewManager sm = LevelManager.ins.ScrewManager;
                Debug.Log("[HingeController] InitHingeJoints: hinge=" + hinge.name + " connected to body=" + body.name + " ScrewManager: " + (sm == null));
                if (sm != null)
                {
                    var part = body.GetComponent<BasePart>();
                    sm.AddHingeConnection(hinge, part);
                }
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
            sm.RemoveHingeConnection(hinge);

            hinge.connectedBody = null;

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
            Debug.Log("HingeController Reset called");
            ClearBody();
        }

        //======================================================================
        // UTILS
        //======================================================================

        public virtual string GetStringBodyLayer(int index)
        {
            if (index < 0) return "";
            return LayerMask.LayerToName(bodyConnect != null ? bodyConnect.gameObject.layer : 0);
        }

        public virtual int GetIntBodyLayer(int index)
        {
            if (index < 0 || bodyConnect == null) return -1;
            return bodyConnect?.gameObject.layer ?? -1;
        }

        public string GetConnectedBodyRenderLayer(int index)
        {
            if (index < 0 || bodyConnect == null) return null;

            var sr = bodyConnect.GetComponentInChildren<SpriteRenderer>();
            return sr ? sr.sortingLayerName : null;
        }
    }
}