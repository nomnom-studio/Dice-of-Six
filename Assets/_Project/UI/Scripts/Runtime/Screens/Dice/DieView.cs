using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button), typeof(Image), typeof(Outline))]
public sealed class DieView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI valueLabel; // �ڽ� TMP
    [SerializeField] private Outline border;             // ��Ʈ Outline
    [SerializeField] private Button button;              // ��Ʈ Button

    private bool _ready;                 // ���ͷ��� ���� ĳ��
    private int _shownValue = int.MinValue;

    // ��Ʈ�ѷ��� �����ϴ� �ݹ�(����/UnityEvent �Ҵ� ����)
    public System.Action<DieView> OnClicked;

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (!border) border = GetComponent<Outline>();
        if (!valueLabel) valueLabel = GetComponentInChildren<TextMeshProUGUI>(true);

        SetRollReady(false);
    }

    // ���Ҵ� Ŭ��
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
        if (valueLabel) valueLabel.SetText("{0}", value); // ToString() �Ҵ� ����
    }
}
