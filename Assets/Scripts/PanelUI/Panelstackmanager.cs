using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 패널의 Z-Order 스택을 관리
/// ESC 키로 최상위 패널부터 닫을 수 있도록 지원
/// </summary>
public class PanelStackManager : MonoBehaviour
{
    public static PanelStackManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    /// <summary>
    /// 활성화된 패널들의 스택 (마지막 요소가 최상위)
    /// </summary>
    private List<ManagedPanel> panelStack = new List<ManagedPanel>();
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
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
    /// 패널이 열릴 때 스택에 추가
    /// </summary>
    public void RegisterPanel(ManagedPanel panel)
    {
        if (panel == null) return;
        
        // 이미 스택에 있으면 제거 후 다시 추가 (최상위로)
        if (panelStack.Contains(panel))
        {
            panelStack.Remove(panel);
        }
        
        panelStack.Add(panel);
        
        LogDebug($"패널 등록: {panel.PanelName} (스택 크기: {panelStack.Count})");
        PrintStack();
    }
    
    /// <summary>
    /// 패널이 닫힐 때 스택에서 제거
    /// </summary>
    public void UnregisterPanel(ManagedPanel panel)
    {
        if (panel == null) return;
        
        panelStack.Remove(panel);
        
        LogDebug($"패널 제거: {panel.PanelName} (스택 크기: {panelStack.Count})");
        PrintStack();
    }
    
    /// <summary>
    /// 패널이 드래그되어 최상위로 올라갈 때 스택 업데이트
    /// </summary>
    public void BringPanelToFront(ManagedPanel panel)
    {
        if (panel == null || !panelStack.Contains(panel)) return;
        
        // 스택에서 제거 후 다시 추가 (최상위로)
        panelStack.Remove(panel);
        panelStack.Add(panel);
        
        LogDebug($"패널을 최상위로: {panel.PanelName}");
        PrintStack();
    }
    
    /// <summary>
    /// ESC 키로 최상위 패널 닫기
    /// </summary>
    public bool CloseTopPanel()
    {
        if (panelStack.Count == 0)
        {
            LogDebug("닫을 패널이 없습니다.");
            return false;
        }
        
        // 스택의 마지막 패널 (최상위)
        ManagedPanel topPanel = panelStack[panelStack.Count - 1];
        
        LogDebug($"최상위 패널 닫기 시도: {topPanel.PanelName}");
        
        // 패널 닫기
        topPanel.ClosePanel();
        
        return true;
    }
    
    /*
    /// <summary>
    /// 모든 패널 닫기
    /// </summary>
    public void CloseAllPanels()
    {
        // 역순으로 닫기
        for (int i = panelStack.Count - 1; i >= 0; i--)
        {
            if (panelStack[i] != null)
            {
                panelStack[i].ClosePanel();
            }
        }
        
        panelStack.Clear();
        LogDebug("모든 패널 닫힘");
    }
    
    /// <summary>
    /// 현재 열린 패널 개수
    /// </summary>
    public int GetOpenPanelCount()
    {
        return panelStack.Count;
    }
    */
    #endregion
    
    #region Helper Methods
    private void PrintStack()
    {
        if (!showDebugLogs) return;
        
        Debug.Log("=== 현재 패널 스택 ===");
        for (int i = 0; i < panelStack.Count; i++)
        {
            Debug.Log($"  [{i}] {panelStack[i].PanelName}");
        }
        Debug.Log("===================");
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PanelStackManager] {message}");
        }
    }
    #endregion
}