using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전리품 시스템 관리 (완전 독립)
/// </summary>
public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject lootPanel;
    [SerializeField] private Transform lootGrid;
    
    [Header("Slot Settings")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int maxLootSlots = 12;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Public Fields
    public List<SlotUI> lootSlots = new List<SlotUI>();
    #endregion
    
    #region Private Fields
    private bool isLootOpen = false;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeLootSlots();
        CloseLootPanel();
    }
    
    private void Update()
    {
        HandleInput();
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
    
    private void InitializeLootSlots()
    {
        // 기존 슬롯 수집
        CollectExistingSlots();
        
        // 부족한 슬롯 생성
        CreateAdditionalSlots();
        
        LogDebug($"전리품 슬롯 {lootSlots.Count}개 초기화 완료");
    }
    
    private void CollectExistingSlots()
    {
        foreach (Transform child in lootGrid)
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                lootSlots.Add(slot);
            }
        }
    }
    
    private void CreateAdditionalSlots()
    {
        while (lootSlots.Count < maxLootSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, lootGrid);
            SlotUI slotUI = newSlot.GetComponent<SlotUI>();
            
            if (slotUI != null)
            {
                slotUI.slotType = SlotType.Inventory;
                lootSlots.Add(slotUI);
            }
        }
    }
    #endregion
    
    #region Input Handling
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleLootPanel();
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 전리품 창 열기/닫기
    /// </summary>
    public void ToggleLootPanel()
    {
        if (isLootOpen)
        {
            CloseLootPanel();
        }
        else
        {
            OpenLootPanel();
        }
    }
    
    /// <summary>
    /// 전리품 창 열기
    /// </summary>
    public void OpenLootPanel()
    {
        if (!ValidateLootPanel()) return;
        
        isLootOpen = true;
        lootPanel.SetActive(true);
        
        LogDebug("전리품 창 열림");
    }
    
    /// <summary>
    /// 전리품 창 닫기
    /// </summary>
    public void CloseLootPanel()
    {
        if (!ValidateLootPanel()) return;
        
        isLootOpen = false;
        lootPanel.SetActive(false);
        
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Loot);
        }
        
        LogDebug("전리품 창 닫힘");
    }
    
    /// <summary>
    /// 전리품 창이 열려있는지 확인
    /// </summary>
    public bool IsLootPanelOpen()
    {
        return isLootOpen;
    }
    
    /// <summary>
    /// 전리품에 아이템 추가
    /// </summary>
    public void AddLoot(ItemData item, int amount = 1)
    {
        if (item == null)
        {
            return;
        }
        
        // 기존 슬롯에 스택 가능하면 추가
        amount = TryStackToExistingSlots(item, amount);
        
        // 남은 수량을 빈 슬롯에 추가
        amount = TryAddToEmptySlots(item, amount);
        
        // 공간 부족 경고
    }
    
    /// <summary>
    /// 전리품 전체 획득
    /// </summary>
    public void TakeAllLoot()
    {
        int successCount = 0;
        int failCount = 0;
        
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem != null && slot.quantity > 0)
            {
                bool success = InventoryManager.Instance.AddItem(slot.currentItem, slot.quantity);
                
                if (success)
                {
                    successCount++;
                    slot.ClearSlot();
                }
                else
                {
                    failCount++;
                }
            }
        }
        
        // 모두 가져갔으면 창 닫기
        if (failCount == 0 && successCount > 0)
        {
            CloseLootPanel();
        }
    }
    
    /// <summary>
    /// 특정 슬롯의 전리품을 인벤토리로 옮기기
    /// </summary>
    public void TransferLootToInventory(SlotUI lootSlot, int amount = 0)
    {
        if (lootSlot == null || lootSlot.currentItem == null)
        {
            return;
        }
        
        if (!lootSlots.Contains(lootSlot))
        {
            return;
        }
        
        ItemData item = lootSlot.currentItem;
        int amountToTransfer = amount > 0 ? Mathf.Min(amount, lootSlot.quantity) : lootSlot.quantity;
        
        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(item, amountToTransfer);
            
            if (success)
            {
                lootSlot.quantity -= amountToTransfer;
                
                if (lootSlot.quantity <= 0)
                {
                    lootSlot.ClearSlot();
                }
                else
                {
                    lootSlot.UpdateUI();
                }
                
            }
                    }
    }
    
    /// <summary>
    /// 특정 슬롯이 전리품 슬롯인지 확인
    /// </summary>
    public bool IsLootSlot(SlotUI slot)
    {
        return lootSlots.Contains(slot);
    }
    #endregion
    
    #region Private Methods
    private bool ValidateLootPanel()
    {
        if (lootPanel == null)
        {
            return false;
        }
        return true;
    }
    
    private int TryStackToExistingSlots(ItemData item, int amount)
    {
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == item && slot.quantity < item.stackSize)
            {
                int space = item.stackSize - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                
                slot.quantity += addAmount;
                slot.UpdateUI();
                
                amount -= addAmount;
                if (amount <= 0) break;
            }
        }
        
        return amount;
    }
    
    private int TryAddToEmptySlots(ItemData item, int amount)
    {
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == null)
            {
                int addAmount = Mathf.Min(item.stackSize, amount);
                slot.SetItem(item, addAmount);
                
                amount -= addAmount;
                if (amount <= 0) break;
            }
        }
        
        return amount;
    }
    
    private void LogDebug(string message) { }
    #endregion
}