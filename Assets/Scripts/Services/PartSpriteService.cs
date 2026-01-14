using UnityEngine;

public class PartSpriteService : IPartSpriteService
{
    public Sprite GetPartSprite(int levelId, string spriteName, bool outline)
    {
        SpriteGroup spriteGroup = outline ? SpriteGroup.Outline : SpriteGroup.Main;
        return SpriteLibControl.Instance.GetSpritePSB(
            levelId,
            spriteGroup,
            spriteName
        );
    }
}
