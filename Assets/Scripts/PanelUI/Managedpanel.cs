using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 스택 관리가 필요한 패널에 붙이는 컴포넌트
/// 패널이 열리고 닫힐 때 PanelStackManager에 알림
/// </summary>
public class ManagedPanel : MonoBehaviour, IPointerDownHandler
{
    #region Serialized Fields
    [Header("패널 정보")]
    [SerializeField] private string panelName = "Unknown Panel";
    [SerializeField] private PanelType panelType = PanelType.Generic;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Public Properties
    public string PanelName => panelName;
    public PanelType PanelType => panelType;
    #endregion
    
    #region Private Fields
    private bool isRegistered = false;
    #endregion
    
    #region Unity Lifecycle
    private void OnEnable()
    {
        RegisterToStack();
        transform.SetAsLastSibling();
    }
    
    private void OnDisable()
    {
        // 패널이 비활성화되면 스택에서 제거
        UnregisterFromStack();
    }
    #endregion
    
    #region Event Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        // 우클릭은 무시
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        // 좌클릭 시 최상위로 올림
        BringToFront();
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 이 패널을 최상위로 올림
    /// </summary>
    public void BringToFront()
    {
        if (PanelStackManager.Instance != null)
        {
            PanelStackManager.Instance.BringPanelToFront(this);
        }
        
        // 실제 Transform도 최상위로
        transform.SetAsLastSibling();
        
        LogDebug("최상위로 이동");
    }
    
    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        LogDebug("패널 닫기 시작");
        
        switch (panelType)
        {
            case PanelType.Inventory:
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.CloseInventoryOnly();
                }
                break;
                
            case PanelType.Equipment:
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.CloseEquipmentOnly();
                }
                break;
                
            case PanelType.Cooking:
                if (CookingManager.Instance != null)
                {
                    CookingManager.Instance.CloseCookingPanel();
                }
                break;

            case PanelType.Potion:
                if (PotionManager.Instance != null)
                {
                    PotionManager.Instance.ClosePotionPanel();
                }
                break;
                
            case PanelType.Loot:
                if (LootManager.Instance != null)
                {
                    LootManager.Instance.CloseLootPanel();
                }
                break;
                
            case PanelType.Generic:
            default:
                gameObject.SetActive(false);
                break;
        }
    }
    #endregion
    
    #region Private Methods
    private void RegisterToStack()
    {
        if (isRegistered) return;
        
        if (PanelStackManager.Instance != null)
        {
            PanelStackManager.Instance.RegisterPanel(this);
            isRegistered = true;
            LogDebug("스택에 등록됨");
        }
    }
    
    private void UnregisterFromStack()
    {
        if (!isRegistered) return;
        
        if (PanelStackManager.Instance != null)
        {
            PanelStackManager.Instance.UnregisterPanel(this);
            isRegistered = false;
            LogDebug("스택에서 제거됨");
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[{panelName}] {message}");
        }
    }
    #endregion
}

/// <summary>
/// 패널 타입 정의
/// </summary>
public enum PanelType
{
    Generic,
    Inventory,
    Equipment,
    Cooking,
    Potion,
    Loot
}