using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ingame.Screw
{
    public abstract class HingeController : MonoBehaviour
    {
        [FormerlySerializedAs("_layer")] [SerializeField] private LayerMask layer;
        [SerializeField] private List<Rigidbody2D> bodyConnect;
        [SerializeField] private List<HingeJoint2D> hingeJoint2D;

        public LayerMask Layer
        {
            get => layer;
            set => layer = value;
        }

        public List<Rigidbody2D> BodyConnect
        {
            get => bodyConnect;
            set => bodyConnect = value;
        }

        public List<HingeJoint2D> HingeJoint2D
        {
            get => hingeJoint2D;
            set => hingeJoint2D = value;
        }


        
       
        // Start is called before the first frame update
        public virtual void Start()
        {
            InitHingeJoints();
        }


        public virtual void InitHingeJoints()
        {
            for (int i  = 0; i < hingeJoint2D.Count; i++)
            {
                if (bodyConnect.Count == 1)
                {
                    hingeJoint2D[i].connectedBody = bodyConnect[0];
                }
                hingeJoint2D[i].connectedBody = bodyConnect[i];
            }
        }

        public string GetConnectedBodyRenderLayer(int index)
        {
            if (bodyConnect[index] == null) return " ";
            var body = bodyConnect[index];
            return body.gameObject.GetComponent<SpriteRenderer>().sortingLayerName;
        }
        public virtual string GetStringBodyLayer(int index)
        {
            return LayerMask.LayerToName(bodyConnect[index].gameObject.layer);
        }

        public virtual int GetIntBodyLayer(int index)
        {
            return bodyConnect[index].gameObject.layer;
        }

        public virtual void FreeHinges()
        {
            for (int i = 0; i < hingeJoint2D.Count; i++)
            {
                hingeJoint2D[i].connectedBody = null;   
            }
        }
    }
}
