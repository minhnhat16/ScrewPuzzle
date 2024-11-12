using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            var currentScene = SceneManager.GetActiveScene();

            if (currentScene.name.CompareTo("LevelMaker") != 0)
            {
                InitHingeJoints();

            }
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

            if (bodyConnect.Count  <= 0) return null;
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
                var hinge = hingeJoint2D[i];
                var crBodyConnect = hinge.connectedBody;
                var hingeComp = hinge.GetComponent<HingeObject>();
                hinge.connectedBody = null;
                ClearBody(crBodyConnect);
                HingePool.Instance.pool.ReturnToPool(hingeComp);
            }
            hingeJoint2D.Clear();

        }
        public virtual void ClearBody(Rigidbody2D body)
        {
            bodyConnect.Remove(body);
        }
        public virtual void ClearBody()
        {
            /*foreach(var body in bodyConnect)
            {
                bod
            }*/
            bodyConnect.Clear();
        }
        internal void Reset()
        {
            ClearBody();
        }
    }
}
