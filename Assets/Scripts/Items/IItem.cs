using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IItem
{
    ItemType ItemType { get; }
    bool IsHandling { get; }

    void HandlingItem();
    void Use();
    void Discard();
}

