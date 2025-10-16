using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button), typeof(Image), typeof(Outline))]
public sealed class DieView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI valueLabel; // 자식 TMP
    [SerializeField] private Outline border;             // 루트 Outline
    [SerializeField] private Button button;              // 루트 Button

    private bool _ready;                 // 인터랙션 상태 캐시
    private int _shownValue = int.MinValue;

    // 컨트롤러가 대입하는 콜백(람다/UnityEvent 할당 없음)
    public System.Action<DieView> OnClicked;

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (!border) border = GetComponent<Outline>();
        if (!valueLabel) valueLabel = GetComponentInChildren<TextMeshProUGUI>(true);

        SetRollReady(false);
    }

    // 무할당 클릭
    public void OnPointerClick(PointerEventData _) { if (_ready) OnClicked?.Invoke(this); }

    public void SetRollReady(bool ready)
    {
        if (_ready == ready) return;
        _ready = ready;
        if (button) button.interactable = ready;
        if (border) border.enabled = ready;
    }

    public void SetValue(int value)
    {
        if (value == _shownValue) return;
        _shownValue = value;
        if (valueLabel) valueLabel.SetText("{0}", value); // ToString() 할당 제거
    }
}
