#if UNITY_EDITOR
using Enums;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorDropDown : MonoBehaviour
{

    [SerializeField]
    private GameObject btnPrefab;
    public List<Button> buttons = new List<Button>();
    // Start is called before the first frame update
    void Start()
    {
        InitButtons();
    }

    // Update is called once per frame
    void Update()
    {

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
            buttons.Add(btnObj.GetComponent<Button>());
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