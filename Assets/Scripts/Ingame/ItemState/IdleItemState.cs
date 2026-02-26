using Ingame;
using UnityEngine;

public class IdleItemState : FSMState<ItemController>
{
    private ItemController itemController;


    public IdleItemState(ItemController itemController)
    {
        Setup(itemController);
    }

}
