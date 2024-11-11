using PoolManager;
using System;
using UnityEngine;

namespace Ingame
{
    public class HoldScrew : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private SpriteRenderer _render;
        [SerializeField] private Transform transf;
        [SerializeField] private Vector3 postion;
        [SerializeField] private Screw.Screw screw;
        public Transform Transf {get { return transf; } set { transf = value; } }
        public int Index { get { return index; } set { index = value; } }
        public Screw.Screw Screw { get => screw;
            set => screw = value;
        }

        public void Start()
        {
            transf = gameObject.GetComponent<Transform>();
            postion = gameObject.GetComponent<Transform>().position;
            _render = GetComponentInChildren<SpriteRenderer>();
        }

        public void ClearScrewOnHold()
        {
            screw = null;
        }
        public void AddScrew(Screw.Screw newScrew,Action<bool> callback = null)
        {
            if (!screw)
            {
                screw = newScrew;
                Debug.Log("Dont  have screw" + index);
                screw.DoMoveToHold(this);
                callback?.Invoke(true);
            }
            else
            {
                Debug.Log("All ready have screw" + index);
                callback?.Invoke(true);
            }
        }

        public Screw.Screw GetScrew()
        {
            return screw == null ? null : screw;
        }
        public bool IsEmpty()
        {
            return screw == null;
        }

        public bool IsContain(Screw.Screw screw)
        {
            if (this.screw == null) return false;
            return this.screw == screw;
        }
    }
}   
