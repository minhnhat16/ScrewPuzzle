using System;
using System.Collections;
using System.Collections.Generic;
using Ingame;
using Ingame.Screw;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Serialization;

public class HoldScrew : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private SpriteRenderer _render;
    [SerializeField] private Transform transf;
    [SerializeField] private Vector3 postion;
    [SerializeField] private Screw screw;
    public Transform Transf {get { return transf; } set { transf = value; } }
    public int Index { get { return index; } set { index = value; } }
    public Screw Screw { get => screw;
        set => screw = value;
    }

    public void Start()
    {
        transf = gameObject.GetComponent<Transform>();
        postion = gameObject.GetComponent<Transform>().position;
        _render = GetComponentInChildren<SpriteRenderer>();
    }

    public void AddScrew(Screw newScrew)
    {
        if (!screw)
        {
            screw = newScrew;
            screw.DoMoveToHold(this);
        }
        else
        {
            Debug.Log("All ready have screw" + index);
        }
    }

    public bool IsEmpty()
    {
        if (screw != null) return false;
        return true;
    }
}   
