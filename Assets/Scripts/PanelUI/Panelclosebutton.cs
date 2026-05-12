// Assets/Scripts/UI/PanelCloseButton.cs

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 패널 닫기 버튼 (X 버튼)
/// </summary>
public class PanelCloseButton : MonoBehaviour
{
    [Header("닫을 패널")]
    [SerializeField] private GameObject targetPanel;
    
    [Header("닫을 때 실행할 추가 동작")]
    [SerializeField] private PanelType panelType = PanelType.Generic;
    
    private Button button;
    
    public enum PanelType
    {
        Generic,        // 일반 패널 (그냥 비활성화)
        Inventory,      // 인벤토리
        Equipment,      // 장비창
        Cooking,        // 요리창
        Potion,         // 포션창
        Loot            // 전리품
    }
    
    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // targetPanel이 없으면 부모 찾기
        if (targetPanel == null)
        {
            targetPanel = transform.parent?.gameObject;
        }
    }
    
    private void OnCloseButtonClicked()
    {
        switch (panelType)
        {
            case PanelType.Inventory:
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ToggleInventoryOnly();
                }
                break;
                
            case PanelType.Equipment:
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ToggleEquipmentOnly();
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
                // 일반 패널은 그냥 비활성화
                if (targetPanel != null)
                {
                    targetPanel.SetActive(false);
                }
                break;
        }
        
        Debug.Log($"{panelType} 패널 닫기");
    }
}