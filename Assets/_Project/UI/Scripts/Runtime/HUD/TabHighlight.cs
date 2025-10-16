using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class TabHighlight : MonoBehaviour
{
    public ScreenId target;
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normal = Color.white;
    [SerializeField] private Color active = Color.yellow;
    [SerializeField] private bool disableWhenActive = true;
    [SerializeField] private ScreenRouter router;

    private Button _button;

    void Reset()
    {
        _button = GetComponent<Button>();
        if (_button && _button.targetGraphic is Image img) targetImage = img;
    }

    void Awake()
    {
        if (router == null)
        {
#if UNITY_2023_1_OR_NEWER
            router = FindFirstObjectByType<ScreenRouter>(FindObjectsInactive.Include);
#else
            router = FindObjectOfType<ScreenRouter>(true);
#endif
        }
        if (_button == null) _button = GetComponent<Button>();
        if (targetImage == null && _button?.targetGraphic is Image img2) targetImage = img2;
    }

    void OnEnable()
    {
        if (router != null) router.OnChanged += HandleChange;
        Sync();
    }

    void OnDisable()
    {
        if (router != null) router.OnChanged -= HandleChange;
    }

    void HandleChange(ScreenId _) => Sync();

    void Sync()
    {
        bool isActive = (router != null && router.Current == target);
        if (targetImage) targetImage.color = isActive ? active : normal;
        if (_button) _button.interactable = !isActive || !disableWhenActive;
    }
}
