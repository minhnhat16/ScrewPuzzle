using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CloseBtn : MonoBehaviour
{
     private Button btn;

    private void OnEnable()
    {
        btn.onClick.AddListener(CloseParent);

    }
    public  void OnDisable()
    {
        btn.onClick.RemoveListener(CloseParent);
    }
    private void Awake()
    {
        btn = GetComponent<Button>();
        
    }

    private void CloseParent()
    {
        // 1. Check if inside a dialog
        BaseDialog dialog = GetComponentInParent<BaseDialog>();
        if (dialog != null)
        {
            DialogManager.ins.HideDialog(dialog.dialogIndex);
            return;
        }

        // 2. Check if inside a view
        BaseView view = GetComponentInParent<BaseView>();
        if (view != null)
        {

            string currentScence = SceneManager.GetActiveScene().name;
            if (string.Compare("Buffer", currentScence) == 0){
                ViewManager.Instance.SwitchView(ViewIndex.MainScreenView,null);
            }
            return;
        }

        Debug.LogWarning("CloseBtn: No BaseDialog or BaseView found in parents.");
    }
}
