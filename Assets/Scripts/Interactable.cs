using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject interactUI;

    private void Start()
    {
        interactUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            interactUI.SetActive(false);
        }
    }
}
