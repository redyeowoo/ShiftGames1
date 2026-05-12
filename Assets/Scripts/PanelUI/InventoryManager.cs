using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 인벤토리 전체 관리 (싱글톤)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 24;
    [SerializeField] private int goldAmount = 0;
    
    [Header("UI References")]
    [SerializeField] private Transform inventoryGridParent;
    [SerializeField] private GameObject inventorySlotPrefab;
    
    [Header("Equipment Slots")]
    [SerializeField] private SlotUI weaponSlot;
    [SerializeField] private SlotUI helmetSlot;
    [SerializeField] private SlotUI armorSlot;
    [SerializeField] private SlotUI shoesSlot;
    [SerializeField] private SlotUI bagSlot;
    [SerializeField] private SlotUI quiverSlot;
    
    [Header("Quick Slot")]
    [SerializeField] private SlotUI quickSlot;
    
    [Header("Money Display")]
    [SerializeField] private TextMeshProUGUI goldText;
    
    // 런타임 데이터
    private List<SlotUI> inventorySlots = new List<SlotUI>();
    
    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeInventorySlots();
        UpdateGoldUI();
    }
    
    /// <summary>
    /// 인벤토리 슬롯 생성
    /// </summary>
    private void InitializeInventorySlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGridParent);
            SlotUI slot = slotObj.GetComponent<SlotUI>();
            
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                inventorySlots.Add(slot);
            }
        }
    }
    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        
        // 1) 스택 가능한 아이템이면 기존 슬롯에 추가 시도
        if (item.stackSize > 1)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.currentItem == item && slot.quantity < item.stackSize)
                {
                    int addable = Mathf.Min(amount, item.stackSize - slot.quantity);
                    slot.quantity += addable;
                    slot.UpdateUI();
                    amount -= addable;
                    
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }
        }
        
        // 2) 빈 슬롯에 추가
        while (amount > 0)
        {
            SlotUI emptySlot = FindEmptySlot();
            if (emptySlot == null)
            {
                return false;
            }
            
            int addAmount = Mathf.Min(amount, item.stackSize);
            emptySlot.SetItem(item, addAmount);
            amount -= addAmount;
        }
        
        return true;
    }
    
    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        
        int remaining = amount;
        
        // 인벤토리에서 해당 아이템을 찾아서 제거
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == item && slot.quantity > 0)
            {
                int removeAmount = Mathf.Min(remaining, slot.quantity);
                slot.quantity -= removeAmount;
                remaining -= removeAmount;
                
                // 수량이 0이 되면 슬롯 비우기
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.UpdateUI();
                }
                
                if (remaining <= 0)
                {
                    return true;
                }
            }
        }
        
        if (remaining > 0)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 특정 아이템의 총 개수 확인
    /// </summary>
    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        
        int total = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == item)
            {
                total += slot.quantity;
            }
        }
        return total;
    }
    
    /// <summary>
    /// 빈 슬롯 찾기 (Public)
    /// </summary>
    public SlotUI FindEmptyInventorySlot()
    {
        return FindEmptySlot();
    }
    
    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    private SlotUI FindEmptySlot()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == null || slot.quantity <= 0)
            {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        goldAmount += amount;
        UpdateGoldUI();
    }
    
    /// <summary>
    /// 현재 골드 가져오기
    /// </summary>
    public int GetGold()
    {
        return goldAmount;
    }
    
    /// <summary>
    /// 골드 제거
    /// </summary>
    public bool RemoveGold(int amount)
    {
        if (goldAmount < amount)
        {
            return false;
        }
        
        goldAmount -= amount;
        UpdateGoldUI();
        return true;
    }
    
    /// <summary>
    /// 골드 UI 갱신
    /// </summary>
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{goldAmount} Gold";
        }
    }
    
    // ─────────────────────────────────────────────
    // 슬롯 이벤트 (드래그 앤 드롭)
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 아이템 이동 또는 교체 시도
    /// </summary>
    public void TryMoveOrSwapDrag(SlotUI fromSlot, SlotUI toSlot)
    {
        // 자기 자신에게 드롭한 경우 무시
        if (fromSlot == toSlot)
        {
            return;
        }
        
        if (fromSlot.currentItem != null && toSlot.currentItem != null && 
            fromSlot.currentItem == toSlot.currentItem)
        {
            TryStackItems(fromSlot, toSlot);
            return;
        }
        
        if (fromSlot.currentItem != null && toSlot.currentItem != null)
        {
            SwapItems(fromSlot, toSlot);
            return;
        }
        
        if (toSlot.currentItem == null)
        {
            MoveItem(fromSlot, toSlot);
            return;
        }
        
    }
    
    /// <summary>
    /// 같은 아이템끼리 스택 시도
    /// </summary>
    private void TryStackItems(SlotUI fromSlot, SlotUI toSlot)
    {
        
        int maxStack = fromSlot.currentItem.stackSize;
        int availableSpace = maxStack - toSlot.quantity;
        
        if (availableSpace <= 0)
        {
            return;
        }
        
        // 합칠 수 있는 만큼 이동
        int amountToMove = Mathf.Min(fromSlot.quantity, availableSpace);
        
        toSlot.quantity += amountToMove;
        fromSlot.quantity -= amountToMove;
        
        // 원본 슬롯이 비었으면 제거
        if (fromSlot.quantity <= 0)
        {
            fromSlot.ClearSlot();
        }
        else
        {
            fromSlot.UpdateUI();
        }
        
        toSlot.UpdateUI();
        
    }
    
    /// <summary>
    /// 아이템 장착 (컨텍스트 메뉴용)
    /// </summary>
    public void EquipItem(SlotUI inventorySlot)
    {
        if (inventorySlot == null || inventorySlot.currentItem == null) return;
        
        // 장비 슬롯 찾기
        SlotUI equipSlot = GetEquipmentSlotByType(inventorySlot.currentItem.itemType);
        
        if (equipSlot == null)
        {
            return;
        }
        
        // 드래그 앤 드롭 로직 재사용
        TryMoveOrSwapDrag(inventorySlot, equipSlot);
    }
    
    /// <summary>
    /// 퀵슬롯에 소비 아이템 장착
    /// </summary>
    public void EquipToQuickSlot(SlotUI inventorySlot)
    {
        if (inventorySlot == null || inventorySlot.currentItem == null)
        {
            return;
        }
        
        // 소비 아이템만 퀵슬롯에 장착 가능
        if (inventorySlot.currentItem.itemType != ItemType.Consumable)
        {
            return;
        }
        
        if (quickSlot == null)
        {
            return;
        }
        
        // 퀵슬롯에 이미 아이템이 있으면 교체
        if (quickSlot.currentItem != null)
        {
            // 같은 아이템이면 합치기 (최대 5개)
            if (quickSlot.currentItem == inventorySlot.currentItem)
            {
                int maxQuickSlot = 5;
                int spaceLeft = maxQuickSlot - quickSlot.quantity;
                
                if (spaceLeft > 0)
                {
                    int addAmount = Mathf.Min(inventorySlot.quantity, spaceLeft);
                    quickSlot.quantity += addAmount;
                    inventorySlot.quantity -= addAmount;
                    
                    if (inventorySlot.quantity <= 0)
                    {
                        inventorySlot.ClearSlot();
                    }
                    else
                    {
                        inventorySlot.UpdateUI();
                    }
                    
                    quickSlot.UpdateUI();
                }
                                return;
            }
            
            // 다른 아이템이면 교체
            ItemData tempItem = quickSlot.currentItem;
            int tempQuantity = quickSlot.quantity;
            
            // 최대 5개까지만
            int swapAmount = Mathf.Min(inventorySlot.quantity, 5);
            quickSlot.SetItem(inventorySlot.currentItem, swapAmount);
            
            inventorySlot.quantity -= swapAmount;
            if (inventorySlot.quantity <= 0)
            {
                inventorySlot.SetItem(tempItem, tempQuantity);
            }
            else
            {
                inventorySlot.UpdateUI();
                // 교체된 아이템을 인벤토리에 추가
                AddItem(tempItem, tempQuantity);
            }
            
            quickSlot.UpdateUI();
        }
        else
        {
            // 빈 슬롯에 장착 (최대 5개)
            int moveAmount = Mathf.Min(inventorySlot.quantity, 5);
            quickSlot.SetItem(inventorySlot.currentItem, moveAmount);
            inventorySlot.quantity -= moveAmount;
            
            if (inventorySlot.quantity <= 0)
            {
                inventorySlot.ClearSlot();
            }
            else
            {
                inventorySlot.UpdateUI();
            }
            
            quickSlot.UpdateUI();
        }
    }
    
    /// <summary>
    /// 아이템 타입에 맞는 장비 슬롯 가져오기
    /// </summary>
    private SlotUI GetEquipmentSlotByType(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return weaponSlot;
            case ItemType.Helmet: return helmetSlot;
            case ItemType.Armor: return armorSlot;
            case ItemType.Shoes: return shoesSlot;
            case ItemType.Bag: return bagSlot;
            case ItemType.Quiver: return quiverSlot;
            default: return null;
        }
    }
    
    // ─────────────────────────────────────────────
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 아이템 교체 (두 슬롯 모두 아이템이 있을 때)
    /// </summary>
    private void SwapItems(SlotUI slotA, SlotUI slotB)
    {
        
        bool canSwap = CanSwapItems(slotA, slotB);
        
        if (!canSwap)
        {
            return;
        }
        
        // 임시 저장
        ItemData tempItem = slotA.currentItem;
        int tempQuantity = slotA.quantity;
        
        // A → B 데이터 복사
        slotA.SetItem(slotB.currentItem, slotB.quantity);
        
        // 임시 → B
        slotB.SetItem(tempItem, tempQuantity);
        
        // UI 업데이트
        slotA.UpdateUI();
        slotB.UpdateUI();
        
    }
    
    /// <summary>
    /// 아이템 이동 (빈 슬롯으로 이동)
    /// </summary>
    private void MoveItem(SlotUI fromSlot, SlotUI toSlot)
    {
        
        if (!toSlot.CanAcceptItem(fromSlot.currentItem))
        {
            return;
        }
        
        if (toSlot.currentItem == fromSlot.currentItem)
        {
            // 스택 가능한지 확인
            int maxStack = fromSlot.currentItem.stackSize;
            int availableSpace = maxStack - toSlot.quantity;
            
            if (availableSpace > 0)
            {
                int amountToMove = Mathf.Min(fromSlot.quantity, availableSpace);
                
                toSlot.quantity += amountToMove;
                fromSlot.quantity -= amountToMove;
                
                if (fromSlot.quantity <= 0)
                {
                    fromSlot.ClearSlot();
                }
                else
                {
                    fromSlot.UpdateUI();
                }
                
                toSlot.UpdateUI();
                
                return;
            }
            else
            {
                return;
            }
        }
        
        toSlot.SetItem(fromSlot.currentItem, fromSlot.quantity);
        fromSlot.ClearSlot();
        
        toSlot.UpdateUI();
        fromSlot.UpdateUI();
        
    }
    
    /// <summary>
    /// 두 슬롯의 아이템이 서로 들어갈 수 있는지 확인 (장비 슬롯 제약 조건)
    /// </summary>
    private bool CanSwapItems(SlotUI slotA, SlotUI slotB)
    {
        // A의 아이템이 B 슬롯에 들어갈 수 있는지
        bool aToB = slotB.CanAcceptItem(slotA.currentItem);
        
        // B의 아이템이 A 슬롯에 들어갈 수 있는지
        bool bToA = slotA.CanAcceptItem(slotB.currentItem);
        
                        return aToB && bToA;
    }
    
    /// <summary>
    /// 모든 인벤토리 아이템 가져오기 (상점에서 사용)
    /// </summary>
    public List<(ItemData item, int quantity)> GetAllItems()
    {
        List<(ItemData, int)> items = new List<(ItemData, int)>();
        
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem != null)
            {
                items.Add((slot.currentItem, slot.quantity));
            }
            else
            {
                // 빈 슬롯도 추가 (null, 0)
                items.Add((null, 0));
            }
        }
        
        return items;
    }
}