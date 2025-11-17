#if UNITY_EDITOR
using Enums;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorDropDown : MonoBehaviour
{

    [SerializeField]
    private GameObject btnPrefab;
    public List<BtnColor> buttons = new List<BtnColor>();
    // Start is called before the first frame update

    public UnityEvent<int, ColorEnum> onTotalScrewChange = new();
    public void OnEnable()
    {
        onTotalScrewChange.AddListener(ChangeTotalScrew);
    }
    void Start()
    {
        onTotalScrewChange = LevelMaker.instance.ontotalScrewChanged;
        InitButtons();
    }

    // Update is called once per frame
    public void ChangeTotalScrew(int total, ColorEnum color)
    {

        Debug.Log("Change total screw " + total);
        var btn = buttons.FirstOrDefault(b => b.Color == color);
        btn.SetColorTotal(total);
    }

    public void UpdateAllScrewTotal()
    {
        for (int i = 0; i < buttons.Count; i++)
        {

            var btn = buttons[i];
            int total = GameObjectToLevelConverter.ins.GetScrewTotal(btn.Color);
            btn.SetColorTotal(total);
        }
    }
    void InitButtons()
    {
        var sprites = Resources.LoadAll<Sprite>(GameConstants.SCREW_SPRITE_PATH);


        Debug.Assert(sprites != null && sprites.Length > 0, "No sprites found in the specified path.");
        sprites = sprites.Where(s => s.name.CompareTo("Hole") != 0).ToArray();

        for (int i = 0; i < sprites.Count(); i++)
        {
            int index = i; // Local copy to avoid closure issues
            var btnObj = Instantiate(btnPrefab, this.transform);
            btnObj.GetComponent<Image>().sprite = sprites[i];


            ColorEnum s = (ColorEnum)System.Enum.Parse(typeof(ColorEnum), sprites[i].name);
            var btn = btnObj.GetComponent<BtnColor>();
            btn.Color = s;
            buttons.Add(btn);

            btnObj.GetComponent<Button>().onClick.AddListener(() => OnButtonClicked(s));
        }
    }


    void OnButtonClicked(ColorEnum buttonIndex)
    {
        Debug.Log($"Button {buttonIndex} clicked!");
        LevelMaker.instance.currentScrewColorID = buttonIndex;
        // You can handle what happens when the button is clicked here
        // For example, use buttonIndex to select a color or perform an action.
    }
}
#endif