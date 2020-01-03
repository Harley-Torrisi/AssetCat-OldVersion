using System.Collections;
using UnityEngine;

public class PopupItemController : MonoBehaviour {

    private Animator myAnim;

    void Start () {
        this.gameObject.SetActive(true);
        myAnim = GetComponent<Animator>();
        myAnim.SetTrigger("Show");
    }

    public void ShowDialog(float ShowDelay = 0)
    {
        Invoke("Show", ShowDelay);
    }

    public void ShowWithParse(GameObject obj)
    {
        GameObject go = Instantiate(this.gameObject);
        go.SendMessage("ObjectParse", obj);
    }

    private void Show()
    {
        Instantiate(this.gameObject);
    }

    public void HideDialog(float HideDelay = 0)
    {
        Invoke("Hide", HideDelay);
    }

    private void Hide()
    {
        myAnim.SetTrigger("Hide");
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }
}
