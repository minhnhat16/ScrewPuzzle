using Level;
using UnityEngine;

public class PartSpriteService : IPartSpriteService
{
    public Sprite GetPartSprite(int levelId, string spriteName, string layer, bool outline)
    {
        SpriteGroup spriteGroup = outline ? SpriteGroup.Outline : SpriteGroup.Main;

        if (outline) spriteName = spriteName.Replace("_a", "_b");

        return SpriteLibControl.Instance.GetSpritePSB(
            levelId,
            spriteGroup,
            layer,
            spriteName
        );
    }
}
