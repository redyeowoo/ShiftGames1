using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI 패널을 드래그 가능하게 만드는 컴포넌트
/// 슬롯이나 버튼 위에서는 드래그 불가
/// 드래그 시 PanelStackManager에 알림
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Events
    /// <summary>
    /// 패널이 드래그될 때 발생하는 이벤트
    /// </summary>
    public event System.Action OnPanelDragged;
    #endregion
    
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private ManagedPanel managedPanel;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeReferences();
    }
    #endregion
    
    #region Initialization
    private void InitializeReferences()
    {
        if (dragRectTransform == null)
        {
            dragRectTransform = GetComponent<RectTransform>();
        }
        
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        
        managedPanel = GetComponent<ManagedPanel>();
    }
    #endregion
    
    #region Drag Handlers
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 우클릭은 무시
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            isDragging = false;
            LogDebug("분할 패널 열림 - 드래그 차단");
            return;
        }
        
        // 슬롯이나 버튼 위에서 시작한 드래그는 무시
        if (IsClickOnInteractableUI(eventData))
        {
            isDragging = false;
            LogDebug("상호작용 UI 감지 - 드래그 무시");
            return;
        }
        
        isDragging = true;
        lastMousePosition = eventData.position;
        
        BringToFront();
        OnPanelDragged?.Invoke();
        
        LogDebug("드래그 시작");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.button != PointerEventData.InputButton.Left)
            return;
        
        if (dragRectTransform != null && canvas != null)
        {
            Vector2 delta = eventData.position - lastMousePosition;
            dragRectTransform.anchoredPosition += delta / canvas.scaleFactor;
            lastMousePosition = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            LogDebug("드래그 종료");
        }
        isDragging = false;
    }
    #endregion
    
    #region Helper Methods
    private void BringToFront()
    {
        // ManagedPanel이 있으면 그쪽에서 처리
        if (managedPanel != null)
        {
            managedPanel.BringToFront();
            LogDebug("최상위로 이동 (ManagedPanel 사용)");
            return;
        }
        
        // SetAsLastSibling으로 최상위 이동
        transform.SetAsLastSibling();
        LogDebug("최상위로 이동");
    }
    
    private bool IsClickOnInteractableUI(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            // 자기 자신은 건너뛰기
            if (result.gameObject == gameObject)
                continue;
            
            // SlotUI 확인
            if (HasComponentOrParent<SlotUI>(result.gameObject))
            {
                LogDebug($"SlotUI 감지: {result.gameObject.name}");
                return true;
            }
            
            // Button 확인
            if (HasComponentOrParent<Button>(result.gameObject))
            {
                LogDebug($"Button 감지: {result.gameObject.name}");
                return true;
            }
            
            // Selectable 확인
            if (result.gameObject.GetComponent<Selectable>() != null)
            {
                LogDebug($"Selectable 감지: {result.gameObject.name}");
                return true;
            }
        }
        
        return false;
    }
    
    private bool HasComponentOrParent<T>(GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() != null || obj.GetComponentInParent<T>() != null;
    }
    
    private void LogDebug(string message)
    {
        
    }
    #endregion
}