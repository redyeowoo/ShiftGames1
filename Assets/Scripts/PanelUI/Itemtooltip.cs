using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 아이템 위에 마우스 올리면 정보 표시
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }
    
    /// <summary>
    /// 툴팁이 어느 창의 아이템인지 구분
    /// </summary>
    public enum PanelSource
    {
        None,
        Inventory,
        Equipment,
        Shop,
        Loot
    }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private float showDelay = 1f; // 1초 대기
    [SerializeField] private Vector2 offset = new Vector2(10, -10); // 마우스에서 얼마나 떨어질지
    #endregion
    
    #region Private Fields
    private Coroutine showCoroutine;
    private RectTransform tooltipRect;
    private Canvas canvas;
    
    private PanelSource currentPanelSource = PanelSource.None;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
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
    /// 툴팁 표시 시작 (1초 후)
    /// </summary>
    public void ShowTooltip(ItemData item, PanelSource panelSource, bool isSellMode = false, bool isBuyMode = false)
    {
        if (item == null) return;
        
        currentPanelSource = panelSource;
        
        // 기존 코루틴 중지
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        
        showCoroutine = StartCoroutine(ShowTooltipDelayed(item, isSellMode, isBuyMode));
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void HideTooltip()
    {
        // 코루틴 중지
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        currentPanelSource = PanelSource.None;
    }
    
    /// <summary>
    /// 특정 창의 툴팁만 숨기기
    /// </summary>
    public void HideTooltipIfFromPanel(PanelSource panelSource)
    {
        // 현재 표시 중인 툴팁이 해당 창의 아이템이면 숨김
        if (currentPanelSource == panelSource)
        {
            HideTooltip();
        }
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// 1초 후 툴팁 표시
    /// </summary>
    private IEnumerator ShowTooltipDelayed(ItemData item, bool isSellMode, bool isBuyMode)
    {
        yield return new WaitForSeconds(showDelay);
        
        if (!IsSourcePanelOpen(currentPanelSource))
        {
            yield break;
        }
        
        // 아이템 이름
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }
                if (priceText != null)
        {
            if (isBuyMode)
            {
                // 구매 모드: "판매가 (구매가: XXX Gold)"
                priceText.text = $"{item.sellPrice} Gold (구매 가격: {item.buyPrice} Gold)";
            }
            else
            {
                // 판매 모드 또는 일반: 판매 가격만 표시
                priceText.text = $"{item.sellPrice} Gold";
            }
            
            priceText.color = new Color(1f, 0.86f, 0f); // 노란색
            
        }
        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrEmpty(item.description) ? "설명 없음" : item.description;
        }
        // 패널 표시
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            
            UpdateTooltipPosition();
        }
            }
    
    /// <summary>
    /// 툴팁 위치 업데이트 (마우스 따라다님)
    /// </summary>
    /// <summary>
    /// 툴팁 위치 업데이트 (화면 이탈 방지 포함)
    /// 
    /// 로직:
    /// 1. 기본 위치: 마우스 오른쪽 하단 (offset: +20, -20)
    /// 2. 화면 경계 체크: 오른쪽/아래쪽으로 잘리는지 확인
    /// 3. 자동 조정:
    ///    - 오른쪽 잘림 → 마우스 왼쪽으로 이동 (pivot 변경)
    ///    - 아래쪽 잘림 → 마우스 위쪽으로 이동 (pivot 변경)
    ///    - 둘 다 잘림 → 마우스 왼쪽 위로 이동
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (tooltipRect == null || canvas == null)
        {
            return;
        }
        
        Vector2 mousePosition = Input.mousePosition;
        
        // 툴팁 크기
        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;
        
        // 기본 오프셋 (마우스 오른쪽 하단)
        Vector2 offset = new Vector2(20, -20);
        
        // 기본 Pivot (왼쪽 위: 0, 1)
        Vector2 pivot = new Vector2(0, 1);
        
        // ===== 오른쪽 경계 체크 =====
        bool willOverflowRight = (mousePosition.x + offset.x + tooltipWidth) > Screen.width;
        
        if (willOverflowRight)
        {
            // 마우스 왼쪽으로 이동
            offset.x = -20;
            pivot.x = 1; // Pivot을 오른쪽으로
        }
        
        // ===== 아래쪽 경계 체크 =====
        bool willOverflowBottom = (mousePosition.y + offset.y - tooltipHeight) < 0;
        
        if (willOverflowBottom)
        {
            // 마우스 위쪽으로 이동
            offset.y = 20;
            pivot.y = 0; // Pivot을 아래쪽으로
        }
        
        // ===== 왼쪽 경계 체크 (왼쪽으로 이동했는데도 화면 밖이면) =====
        if (pivot.x == 1 && (mousePosition.x + offset.x - tooltipWidth) < 0)
        {
            // 최소 왼쪽 경계 보정
            offset.x = -mousePosition.x + tooltipWidth + 10;
        }
        
        // ===== 위쪽 경계 체크 (위쪽으로 이동했는데도 화면 밖이면) =====
        if (pivot.y == 0 && (mousePosition.y + offset.y + tooltipHeight) > Screen.height)
        {
            // 최대 위쪽 경계 보정
            offset.y = Screen.height - mousePosition.y - tooltipHeight - 10;
        }
        
        // Pivot 적용
        tooltipRect.pivot = pivot;
        
        // 최종 위치 계산
        Vector2 finalPosition = mousePosition + offset;
        
        tooltipPanel.transform.position = finalPosition;
        
    }
    
    /// <summary>
    /// 특정 창이 열려있는지 확인
    /// </summary>
    private bool IsSourcePanelOpen(PanelSource panelSource)
    {
        switch (panelSource)
        {
            case PanelSource.Shop:
                return ShopManager.Instance != null && ShopManager.Instance.IsShopOpen();
                
            case PanelSource.Inventory:
                return UIManager.Instance != null && UIManager.Instance.IsInventoryOpen();
                
            case PanelSource.Equipment:
                return UIManager.Instance != null && UIManager.Instance.IsEquipmentOpen();
                
            case PanelSource.Loot:
                return LootManager.Instance != null && LootManager.Instance.IsLootPanelOpen();
                
            default:
                return false;
        }
    }
    #endregion
    
    #region Update
    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            // 툴팁이 속한 창이 닫혔으면 즉시 숨김
            if (!IsSourcePanelOpen(currentPanelSource))
            {
                HideTooltip();
                return;
            }
            
            UpdateTooltipPosition();
        }
    }
    #endregion
}