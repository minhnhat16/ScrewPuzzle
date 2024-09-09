using DG.Tweening;
using UnityEngine;

public class Card : MonoBehaviour
{
    public CardColorPallet cardColor;
    public Color currentColor;
    public SpriteRenderer sprite;
    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }
    private void SFXRandom()
    {
        int ran = Random.Range(1, 4);
        switch (ran)
        {
            case 1:
                SoundManager.instance.PlaySFX(SoundManager.SFX.DealCardSFX_1);
                break;
            case 2:
                SoundManager.instance.PlaySFX(SoundManager.SFX.DealCardSFX_2);
                break;
            case 3:
                SoundManager.instance.PlaySFX(SoundManager.SFX.DealCardSFX_3);
                break;
            case 4:
                SoundManager.instance.PlaySFX(SoundManager.SFX.DealCardSFX_4);
                break;
            default:
                //Debug.LogWarning("Unexpected random value: " + ran);
                break;
        }
    }
    public Tween PlayAnimation(Slot targetSlot, float duration, float height, Ease e, float offsetY, float offsetZ, float delay)
    {
        //Debug.Log("targetSlot pos " + targetSlot.transform.position);
        var rotationVector = new Vector3();
        var currentRotation = transform.rotation.eulerAngles;

        rotationVector = GetMovementDirection(targetSlot.transform) switch
        {
            Direction.Up => new Vector3(currentRotation.x + 180, 0, 0),
            Direction.Down => new Vector3(currentRotation.x - 180, 0, 0),
            Direction.Right => new Vector3(0, 0, currentRotation.z - 180),
            Direction.Left => new Vector3(0, 0, currentRotation.z + 180),
            Direction.UpperRight => new Vector3(currentRotation.x + 180, 0, 0),
            Direction.UpperLeft => new Vector3(currentRotation.x + 180, 0, 0),
            Direction.DownRight => new Vector3(currentRotation.x - 180, 0, 0),
            Direction.DownLeft => new Vector3(currentRotation.x - 180, 0, 0),

            _ => rotationVector
        };
        var position = targetSlot.transform.position;
        var p = new Vector3(position.x, offsetY, offsetZ);

        //Debug.Log("Position " + p);
        Vector3 addZ = new Vector3(0, 0, -5);
        Tween tween = transform.DOJump(p + addZ, height, 1, duration).SetEase(e).SetDelay(delay);
        transform.DORotate(rotationVector, duration).SetEase(e).SetDelay(delay)
            .OnComplete(() =>
             {
                 SFXRandom();
                 transform.position -= addZ;
                 transform.rotation = Quaternion.Euler(Vector3.zero);
                 //tween.Kill(true);
             });
        return tween;
    }

    private Direction GetMovementDirection(Transform target)
    {
        var dir = target.position - transform.position;

        var result = dir.x switch
        {
            0 when dir.z > 0 => Direction.Up,
            0 when dir.z < 0 => Direction.Down,
            > 0 when dir.z == 0 => Direction.Right,
            < 0 when dir.z == 0 => Direction.Left,
            > 0 when dir.z > 0 => Direction.UpperRight,
            < 0 when dir.z > 0 => Direction.UpperLeft,
            > 0 when dir.z < 0 => Direction.DownRight,
            < 0 when dir.z < 0 => Direction.DownLeft,
            _ => Direction.Unknown
        };

        return result;
    }
    internal void ColorSetBy(CardColorPallet cardColor, CardType currentType, Color recordColor)
    {
        this.cardColor = cardColor;
        currentColor = recordColor;
        sprite.sprite = SpriteLibControl.Instance.GetCardSpriteByCategory(currentType, (int)cardColor);
    }
}
