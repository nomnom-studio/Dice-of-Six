using UnityEngine;

public sealed class ScreenView : MonoBehaviour
{
    public ScreenId Id;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool setInactiveOnHide = true;

    void Reset() { group = GetComponent<CanvasGroup>(); }
    void OnValidate() { if (!group) group = GetComponent<CanvasGroup>(); }

    public void Show()
    {
        gameObject.SetActive(true);
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        if (setInactiveOnHide) gameObject.SetActive(false);
    }
}
