using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 아이템을 마우스 커서를 따라다니게 하는 클래스
/// 분할된 아이템을 들고 다니다가 슬롯에 드롭
/// </summary>
public class ItemCursorFollower : MonoBehaviour
{
    public static ItemCursorFollower Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject followerObject;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    
    [Header("Settings")]
    [SerializeField] private Vector2 iconSize = new Vector2(64, 64);
    [SerializeField] private float iconAlpha = 0.8f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private ItemData heldItem;
    private int heldQuantity;
    private SlotUI sourceSlot;
    private bool isHolding = false;
    private Canvas canvas;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        if (followerObject != null)
        {
            followerObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (isHolding)
        {
            UpdateFollowerPosition();
            HandleInput();
        }
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
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 아이템을 들기 시작
    /// </summary>
    public void StartHolding(ItemData item, int quantity, SlotUI source)
    {
        if (item == null || quantity <= 0)
        {
            return;
        }
        
        heldItem = item;
        heldQuantity = quantity;
        sourceSlot = source;
        isHolding = true;
        
        // UI 표시
        if (followerObject != null)
        {
            followerObject.SetActive(true);
            followerObject.transform.SetAsLastSibling(); // 최상위로
        }
                // 아이콘 설정
        if (itemIconImage != null)
        {
            itemIconImage.sprite = item.itemIcon;
            itemIconImage.enabled = true;
            
            // 반투명 효과
            Color color = itemIconImage.color;
            color.a = iconAlpha;
            itemIconImage.color = color;
            
            // 크기 설정
            RectTransform rectTransform = itemIconImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = iconSize;
            }
            
        }
                // 수량 텍스트 설정
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }
    }
    
    /// <summary>
    /// 아이템 들기 중단
    /// </summary>
    public void StopHolding()
    {
        isHolding = false;
        heldItem = null;
        heldQuantity = 0;
        sourceSlot = null;
        
        if (followerObject != null)
        {
            followerObject.SetActive(false);
        }
        
        LogDebug("아이템 들기 중단");
    }
    
    /// <summary>
    /// 현재 아이템을 들고 있는지 확인
    /// </summary>
    public bool IsHolding()
    {
        return isHolding;
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// 마우스 위치로 팔로워 이동
    /// </summary>
    private void UpdateFollowerPosition()
    {
        if (followerObject != null)
        {
            followerObject.transform.position = Input.mousePosition;
        }
    }
    
    /// <summary>
    /// 입력 처리 (클릭/ESC)
    /// </summary>
    private void HandleInput()
    {
        // 좌클릭: 슬롯에 드롭
        if (Input.GetMouseButtonDown(0))
        {
            SlotUI targetSlot = GetSlotUnderMouse();
            
            if (targetSlot != null)
            {
                TryDropToSlot(targetSlot);
            }
            else
            {
                LogDebug("슬롯이 아닌 곳을 클릭했습니다.");
            }
        }
        
        // 우클릭 또는 ESC: 취소 (원래 슬롯으로 되돌리기)
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToSource();
        }
    }
    
    /// <summary>
    /// 마우스 아래의 슬롯 찾기
    /// </summary>
    private SlotUI GetSlotUnderMouse()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            SlotUI slot = result.gameObject.GetComponent<SlotUI>();
            if (slot != null) return slot;
            
            slot = result.gameObject.GetComponentInParent<SlotUI>();
            if (slot != null) return slot;
        }
        
        return null;
    }
    
    /// <summary>
    /// 슬롯에 아이템 드롭 시도
    /// </summary>
    private void TryDropToSlot(SlotUI targetSlot)
    {
        if (targetSlot == null)
        {
            LogDebug("대상 슬롯이 null입니다.");
            return;
        }
        
        // 같은 슬롯에 드롭 (취소)
        if (targetSlot == sourceSlot)
        {
            ReturnToSource();
            return;
        }
        
        // 빈 슬롯에 드롭
        if (targetSlot.currentItem == null)
        {
            targetSlot.SetItem(heldItem, heldQuantity);
            targetSlot.UpdateUI();
            LogDebug($"빈 슬롯에 드롭: {heldItem.itemName} x{heldQuantity}");
            StopHolding();
        }
        // 같은 아이템이 있는 슬롯에 드롭 (합치기)
        else if (targetSlot.currentItem == heldItem)
        {
            targetSlot.quantity += heldQuantity;
            targetSlot.UpdateUI();
            LogDebug($"같은 아이템 합치기: {heldItem.itemName} x{heldQuantity}");
            StopHolding();
        }
        // 다른 아이템이 있는 슬롯에 드롭 → 아무것도 안 함 (마우스에 계속 남아있음)
        else
        {
            LogDebug("다른 아이템이 있는 슬롯입니다. 드롭 무시 (마우스에 계속 남아있음)");
        }
    }
    
    /// <summary>
    /// 원래 슬롯으로 되돌리기
    /// </summary>
    private void ReturnToSource()
    {
        if (sourceSlot != null)
        {
            sourceSlot.quantity += heldQuantity;
            sourceSlot.UpdateUI();
            LogDebug($"원래 슬롯으로 복구: {heldItem.itemName} x{heldQuantity}");
        }
                StopHolding();
    }
    
    private void LogDebug(string message) { }
    #endregion
}