using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 아이템 수량 분할 UI 관리자
/// Shift + 클릭/드래그 시 표시
/// </summary>
public class ItemSplitManager : MonoBehaviour
{
    public static ItemSplitManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private SlotUI sourceSlot;
    private SlotUI targetSlot;
    private SplitMode currentMode;
    private int maxQuantity;
    private int selectedQuantity = 1;
    #endregion
    
    #region Enums
    private enum SplitMode
    {
        ClickToEmpty,      // Shift + 클릭 → 빈 슬롯에 분할
        DragToEmpty        // Shift + 드래그 → 빈 슬롯에 분할
    }
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    private void Update()
    {
        HandleEscapeKey();
    }
    #endregion
    
    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeUI()
    {
        // 버튼 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                OnConfirmClicked();
            });
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                OnCancelClicked();
            });
        }
        
        // 슬라이더 이벤트 연결
        if (quantitySlider != null)
        {
            quantitySlider.onValueChanged.RemoveAllListeners();
            quantitySlider.onValueChanged.AddListener((value) => {
                OnSliderValueChanged(value);
            });
            
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = 10;
            quantitySlider.wholeNumbers = true;
            quantitySlider.value = 1;
            
            LogDebug("슬라이더 초기화 완료");
        }
        
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
        
        LogDebug("UI 초기화 완료 - ItemSplitPanel 비활성화");
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Shift + 클릭으로 분할 패널 열기
    /// </summary>
    public void OpenForClick(SlotUI source)
    {
        if (source == null || source.currentItem == null || source.quantity <= 1)
        {
            LogDebug("분할할 수 없는 슬롯입니다.");
            return;
        }
        
        sourceSlot = source;
        targetSlot = null;
        currentMode = SplitMode.ClickToEmpty;
        
        ShowPanel();
    }
    
    /// <summary>
    /// Shift + 드래그로 분할 패널 열기
    /// </summary>
    public void OpenForDrag(SlotUI source, SlotUI target)
    {
        if (source == null || source.currentItem == null || source.quantity <= 1)
        {
            LogDebug("분할할 수 없는 슬롯입니다.");
            return;
        }
        
        if (target == null || target.currentItem != null)
        {
            LogDebug("대상 슬롯이 비어있어야 합니다.");
            return;
        }
        
        sourceSlot = source;
        targetSlot = target;
        currentMode = SplitMode.DragToEmpty;
        
        ShowPanel();
    }
    
    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
            LogDebug("ItemSplitPanel 비활성화");
        }
        
        // 데이터 초기화
        sourceSlot = null;
        targetSlot = null;
        selectedQuantity = 1;
        
        LogDebug("패널 닫힘");
    }
    
    /// <summary>
    /// 패널이 열려있는지 확인
    /// </summary>
    public bool IsOpen()
    {
        return panelObject != null && panelObject.activeSelf;
    }
    #endregion
    
    #region Private Methods
    private void ShowPanel()
    {
        if (panelObject == null)
        {
            return;
        }
        
        maxQuantity = sourceSlot.quantity;
        
        panelObject.SetActive(true);
        LogDebug("ItemSplitPanel 활성화");
        
        // 슬라이더 설정
        if (quantitySlider != null)
        {
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = 1;
        }
        
        // 아이템 정보 표시
        if (itemNameText != null)
        {
            itemNameText.text = sourceSlot.currentItem.itemName;
        }
        
        if (itemIconImage != null)
        {
            itemIconImage.sprite = sourceSlot.currentItem.itemIcon;
            itemIconImage.gameObject.SetActive(true);
        }
        
        // 수량 텍스트 업데이트
        UpdateQuantityText();
        
        LogDebug($"패널 열림 - 최대 수량: {maxQuantity}");
    }
    
    private void OnSliderValueChanged(float value)
    {
        selectedQuantity = Mathf.RoundToInt(value);
        UpdateQuantityText();
    }
    
    private void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            quantityText.text = $"{selectedQuantity} / {sourceSlot.quantity}";
        }
    }
    
    private void OnConfirmClicked()
    {
        if (sourceSlot == null)
        {
            ClosePanel();
            return;
        }
        
        if (selectedQuantity <= 0)
        {
            LogDebug("0개는 분할할 수 없습니다.");
            ClosePanel();
            return;
        }
        
        LogDebug($"분할 확인: {selectedQuantity}개");
        
        ItemData itemToHold = sourceSlot.currentItem;
        
        sourceSlot.quantity -= selectedQuantity;
        
        if (sourceSlot.quantity <= 0)
        {
            sourceSlot.ClearSlot();
        }
        else
        {
            sourceSlot.UpdateUI();
        }
        
        if (ItemCursorFollower.Instance != null)
        {
            ItemCursorFollower.Instance.StartHolding(itemToHold, selectedQuantity, sourceSlot);
            LogDebug($"ItemCursorFollower.StartHolding 호출 완료");
        }
                // 패널 닫기
        ClosePanel();
    }
    
    private void OnCancelClicked()
    {
        LogDebug("분할 취소");
        ClosePanel();
    }
    
    private void HandleEscapeKey()
    {
        if (IsOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClicked();
            Input.ResetInputAxes();
        }
    }
    
    private void LogDebug(string message)
    {
        
    }
    #endregion
}