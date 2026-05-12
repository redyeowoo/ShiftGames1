using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 개별 슬롯 UI 관리 (드래그 앤 드롭 + 더블클릭 지원)
/// </summary>
public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    
    [Header("Slot Data")]
    public ItemData currentItem;
    public int quantity = 0;
    
    [Header("Slot Type")]
    public SlotType slotType = SlotType.Inventory;
    public ItemType allowedItemType = ItemType.Weapon; // Equipment 슬롯일 때만 사용
    
    [Header("Double Click Settings")]
    [Tooltip("더블클릭으로 인정할 시간 간격 (초)")]
    [SerializeField] private float doubleClickTime = 0.3f;
    
    // 드래그 관련
    private GameObject draggedIcon;
    private Canvas canvas;
    private bool canDrag = false; // 드래그 가능 여부
    
    // 더블클릭 관련
    private float lastClickTime = 0f;
    private int clickCount = 0;
    
    private void Start()
    {
        if (iconImage == null)
        {
            iconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }
        
        if (quantityText == null)
        {
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        
        UpdateUI();
        canvas = GetComponentInParent<Canvas>();
    }
    
    /// <summary>
    /// UI 갱신
    /// </summary>
    public void UpdateUI()
    {
        if (currentItem != null && quantity > 0)
        {
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = currentItem.itemIcon;
                iconImage.color = Color.white;
            }
            
            if (quantityText != null)
            {
                bool isConsumableOrIngredient = currentItem.itemType == ItemType.Consumable || 
                                                 currentItem.itemType == ItemType.Ingredient;
                
                if (isConsumableOrIngredient && quantity > 1)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
            
            if (priceText != null)
            {
                bool isInShopPanel = IsInShopPanel();
                
                if (isInShopPanel)
                {
                    bool isInShopGrid = IsInShopGrid();
                    priceText.gameObject.SetActive(true);
                    priceText.text = isInShopGrid ? $"{currentItem.buyPrice}G" : $"{currentItem.sellPrice}G";
                }
                else
                {
                    priceText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
            
            if (priceText != null)
            {
                priceText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 아이템 추가/설정
    /// </summary>
    public void SetItem(ItemData item, int amount)
    {
        currentItem = item;
        quantity = amount;
        UpdateUI();
    }
    
    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        quantity = 0;
        UpdateUI();
    }
    
    /// <summary>
    /// 이 슬롯이 해당 아이템을 받을 수 있는지 확인
    /// </summary>
    public bool CanAcceptItem(ItemData item)
    {
        if (item == null) return false;
        
        // 요리 재료 슬롯 체크
        if (slotType == SlotType.Cooking)
        {
            return item.itemType == ItemType.Ingredient;
        }
        
        // 포션 재료 슬롯 체크
        if (slotType == SlotType.Potion)
        {
            return item.itemType == ItemType.Ingredient;
        }
        
        // 퀵슬롯은 소비 아이템만 수용
        if (gameObject.name.Contains("Quick"))
        {
            return item.itemType == ItemType.Consumable;
        }
        
        if (slotType == SlotType.Inventory)
        {
            return true;
        }
        else // SlotType.Equipment
        {
            return item.itemType == allowedItemType;
        }
    }
    
    // ─────────────────────────────────────────────
    // 드래그 앤 드롭 이벤트
    // ─────────────────────────────────────────────
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            canDrag = false;
            return;
        }
        
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            canDrag = false;
            return;
        }
        
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            if (transform.parent != null)
            {
                string parentName = transform.parent.name;
                if (parentName == "ShopGrid" || parentName == "InventoryGrid")
                {
                    canDrag = false;
                    return;
                }
            }
        }
        
        if (currentItem == null)
        {
            canDrag = false;
            return;
        }
        
        canDrag = true;
        
        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(canvas.transform, false);
        draggedIcon.transform.SetAsLastSibling();
        
        Image dragImage = draggedIcon.AddComponent<Image>();
        dragImage.sprite = currentItem.itemIcon;
        dragImage.raycastTarget = false;
        
        Color color = dragImage.color;
        color.a = 0.6f;
        dragImage.color = color;
        
        RectTransform rectTransform = draggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(64, 64);
        
        if (iconImage != null)
        {
            Color iconColor = iconImage.color;
            iconColor.a = 0.3f;
            iconImage.color = iconColor;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (canDrag && draggedIcon != null)
        {
            draggedIcon.transform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
            draggedIcon = null;
        }
        
        if (iconImage != null)
        {
            Color iconColor = iconImage.color;
            iconColor.a = 1f;
            iconImage.color = iconColor;
        }
        
        if (!canDrag)
        {
            canDrag = false;
            return;
        }
        
        SlotUI targetSlot = GetSlotUnderMouse(eventData);
        
        if (targetSlot != null)
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && 
                targetSlot.currentItem == null && 
                quantity > 1)
            {
                HandleShiftDrag(targetSlot);
            }
            else
            {
                InventoryManager.Instance.TryMoveOrSwapDrag(this, targetSlot);
            }
        }
        
        canDrag = false;
    }
    
    /// <summary>
    /// 마우스 아래의 슬롯 찾기
    /// </summary>
    private SlotUI GetSlotUnderMouse(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            SlotUI slot = result.gameObject.GetComponent<SlotUI>();
            if (slot != null)
            {
                return slot;
            }
            
            slot = result.gameObject.GetComponentInParent<SlotUI>();
            if (slot != null)
            {
                return slot;
            }
        }
        
        return null;
    }
    
    // ─────────────────────────────────────────────
    // 마우스 클릭 이벤트
    // ─────────────────────────────────────────────
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            return;
        }
        
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            return;
        }
        
        // 우클릭
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                return;
            }
            
            if (currentItem != null && ContextMenu.Instance != null)
            {
                ContextMenu.Instance.OpenMenu(this, eventData.position);
            }
            return;
        }
        
        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                HandleShopSelection();
                return;
            }
            
            // Shift + 클릭
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                HandleShiftClick();
            }
            else
            {
                // 더블클릭 감지
                HandleLeftClick();
            }
        }
    }
    
    // ─────────────────────────────────────────────
    // 더블클릭 처리
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 좌클릭 처리 (더블클릭 감지)
    /// </summary>
    private void HandleLeftClick()
    {
        if (currentItem == null) return;
        
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            clickCount++;
            
            if (clickCount >= 2)
            {
                OnDoubleClickDetected();
                clickCount = 0;
            }
        }
        else
        {
            clickCount = 1;
        }
        
        lastClickTime = Time.time;
    }
    
    /// <summary>
    /// 더블클릭 감지 시 실행
    /// </summary>
    private void OnDoubleClickDetected()
    {
        if (slotType == SlotType.Inventory)
        {
            HandleInventoryDoubleClick();
        }
        else if (slotType == SlotType.Equipment)
        {
            HandleEquipmentDoubleClick();
        }
        else if (slotType == SlotType.Cooking)
        {
            HandleCookingDoubleClick();
        }
        else if (slotType == SlotType.Potion)
        {
            HandlePotionDoubleClick();
        }
    }
    
    /// <summary>
    /// 인벤토리 슬롯 더블클릭
    /// </summary>
    private void HandleInventoryDoubleClick()
    {
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            return;
        }
        
        // 전리품 슬롯
        if (IsLootSlot())
        {
            TransferLootToInventory();
            return;
        }
        
        // 장비 아이템 → 자동 장착
        if (IsEquipmentItem(currentItem))
        {
            EquipItemAuto();
            return;
        }
        
        // 소비 아이템 → 퀵슬롯 장착
        if (currentItem.itemType == ItemType.Consumable)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.EquipToQuickSlot(this);
            }
            return;
        }
        
        // 재료 아이템 + 요리창 열림 → 요리창에 추가
        if (currentItem.itemType == ItemType.Ingredient && IsCookingPanelOpen())
        {
            AddToCookingPanel();
            return;
        }
        
        // 재료 아이템 + 포션창 열림 → 포션창에 추가
        if (currentItem.itemType == ItemType.Ingredient && IsPotionPanelOpen())
        {
            AddToPotionPanel();
            return;
        }
    }
    
    /// <summary>
    /// 장비 슬롯 더블클릭
    /// </summary>
    private void HandleEquipmentDoubleClick()
    {
        UnequipToInventory();
    }
    
    /// <summary>
    /// 요리 슬롯 더블클릭
    /// </summary>
    private void HandleCookingDoubleClick()
    {
        ReturnIngredientToInventory();
    }
    
    /// <summary>
    /// 포션 슬롯 더블클릭
    /// </summary>
    private void HandlePotionDoubleClick()
    {
        ReturnIngredientToInventory();
    }
    
    // ─────────────────────────────────────────────
    // 헬퍼 메서드
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 전리품 슬롯인지 확인
    /// </summary>
    private bool IsLootSlot()
    {
        if (LootManager.Instance != null && LootManager.Instance.lootSlots != null)
        {
            return LootManager.Instance.lootSlots.Contains(this);
        }
        return false;
    }
    
    /// <summary>
    /// 장비 아이템인지 확인
    /// </summary>
    private bool IsEquipmentItem(ItemData item)
    {
        return item.itemType == ItemType.Weapon ||
               item.itemType == ItemType.Helmet ||
               item.itemType == ItemType.Armor ||
               item.itemType == ItemType.Shoes ||
               item.itemType == ItemType.Bag ||
               item.itemType == ItemType.Quiver;
    }
    
    /// <summary>
    /// 요리창이 열려있는지 확인
    /// </summary>
    private bool IsCookingPanelOpen()
    {
        if (CookingManager.Instance == null) return false;
        return CookingManager.Instance.IsCookingOpen();
    }
    
    /// <summary>
    /// 포션창이 열려있는지 확인
    /// </summary>
    private bool IsPotionPanelOpen()
    {
        if (PotionManager.Instance == null) return false;
        return PotionManager.Instance.IsPotionOpen();
    }
    
    /// <summary>
    /// 장비 자동 장착
    /// </summary>
    private void EquipItemAuto()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }
        
        InventoryManager.Instance.EquipItem(this);
    }
    
    /// <summary>
    /// 장비 해제 후 인벤토리로
    /// </summary>
    private void UnequipToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }
        
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            ItemData item = currentItem;
            int qty = quantity;
            
            emptySlot.SetItem(item, qty);
            emptySlot.UpdateUI();
            
            ClearSlot();
        }
    }
    
    /// <summary>
    /// 요리창에 재료 추가
    /// </summary>
    private void AddToCookingPanel()
    {
        if (CookingManager.Instance == null)
        {
            return;
        }
        
        bool success = CookingManager.Instance.TryAddIngredient(currentItem, this);
        
        if (!success)
        {
            Debug.LogWarning($"요리 재료 추가 실패: {currentItem.itemName}");
        }
    }
    
    /// <summary>
    /// 포션창에 재료 추가
    /// </summary>
    private void AddToPotionPanel()
    {
        if (PotionManager.Instance == null)
        {
            return;
        }
        
        bool success = PotionManager.Instance.TryAddIngredient(currentItem, this);
        
        if (!success)
        {
            Debug.LogWarning($"포션 재료 추가 실패: {currentItem.itemName}");
        }
    }
    
    /// <summary>
    /// 재료 슬롯에서 인벤토리로 반환
    /// </summary>
    private void ReturnIngredientToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }
        
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            ItemData item = currentItem;
            int qty = quantity;
            
            emptySlot.SetItem(item, qty);
            emptySlot.UpdateUI();
            
            ClearSlot();
        }
        else
        {
            Debug.LogWarning("인벤토리에 빈 슬롯이 없습니다!");
        }
    }
    
    /// <summary>
    /// 전리품을 인벤토리로 이동
    /// </summary>
    private void TransferLootToInventory()
    {
        if (LootManager.Instance == null)
        {
            return;
        }
        
        LootManager.Instance.TransferLootToInventory(this, 0);
    }
    
    /// <summary>
    /// Shift + 클릭 (아이템 분할)
    /// </summary>
    private void HandleShiftClick()
    {
        if (currentItem == null || quantity <= 1 || currentItem.stackSize <= 1)
        {
            return;
        }
        
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForClick(this);
        }
    }
    
    /// <summary>
    /// Shift + 드래그 (아이템 분할)
    /// </summary>
    private void HandleShiftDrag(SlotUI targetSlot)
    {
        if (currentItem == null || quantity <= 1 || currentItem.stackSize <= 1)
        {
            return;
        }
        
        if (targetSlot.currentItem != null)
        {
            return;
        }
        
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForDrag(this, targetSlot);
        }
    }
    
    /// <summary>
    /// 상점 슬롯 선택
    /// </summary>
    private void HandleShopSelection()
    {
        if (currentItem == null || ShopManager.Instance == null)
        {
            return;
        }
        
        bool isShopSlot = transform.parent != null && transform.parent.name == "ShopGrid";
        ShopManager.Instance.OnSlotClicked(this, isShopSlot);
    }
    
    // ─────────────────────────────────────────────
    // 툴팁
    // ─────────────────────────────────────────────
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && ItemTooltip.Instance != null)
        {
            if (ContextMenu.Instance != null && ContextMenu.Instance.IsMenuOpen())
            {
                return;
            }
            
            ItemTooltip.PanelSource panelSource = DeterminePanelSource();
            
            bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.IsShopOpen();
            bool isSellMode = isShopOpen && ShopManager.Instance.IsSellMode();
            bool isBuyMode = isShopOpen && !ShopManager.Instance.IsSellMode();
            bool isShopItem = panelSource == ItemTooltip.PanelSource.Shop;
            
            ItemTooltip.Instance.ShowTooltip(currentItem, panelSource, isSellMode, isBuyMode && isShopItem);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
    
    /// <summary>
    /// 이 슬롯이 어느 패널에 속하는지 판단
    /// </summary>
    private ItemTooltip.PanelSource DeterminePanelSource()
    {
        Transform current = transform;
        
        for (int i = 0; i < 5 && current != null; i++)
        {
            string nodeName = current.name;
            
            if (nodeName.Contains("ShopPanel") || nodeName == "ShopPanel")
            {
                return ItemTooltip.PanelSource.Shop;
            }
            
            if (nodeName.Contains("LootPanel") || nodeName.Contains("Loot"))
            {
                return ItemTooltip.PanelSource.Loot;
            }
            
            if (nodeName.Contains("EquipmentPanel") || nodeName.Contains("Equipment"))
            {
                return ItemTooltip.PanelSource.Equipment;
            }
            
            if (nodeName == "InventoryPanel")
            {
                return ItemTooltip.PanelSource.Inventory;
            }
            
            current = current.parent;
        }
        
        // 폴백
        if (transform.parent != null)
        {
            string parentName = transform.parent.name;
            
            if (parentName.Contains("Shop"))
            {
                return ItemTooltip.PanelSource.Shop;
            }
            else if (parentName.Contains("Loot"))
            {
                return ItemTooltip.PanelSource.Loot;
            }
            else if (parentName.Contains("Equipment") || slotType == SlotType.Equipment)
            {
                return ItemTooltip.PanelSource.Equipment;
            }
            else if (parentName.Contains("Inventory"))
            {
                return ItemTooltip.PanelSource.Inventory;
            }
        }
        
        return ItemTooltip.PanelSource.Inventory;
    }
    
    /// <summary>
    /// 상점 그리드에 있는지 확인
    /// </summary>
    private bool IsInShopGrid()
    {
        Transform current = transform;
        
        for (int i = 0; i < 5 && current != null; i++)
        {
            if (current.name == "ShopGrid" || current.name.Contains("ShopGrid"))
            {
                return true;
            }
            
            current = current.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// 상점 패널에 있는지 확인
    /// </summary>
    private bool IsInShopPanel()
    {
        Transform current = transform;
        
        for (int i = 0; i < 5 && current != null; i++)
        {
            if (current.name == "ShopPanel" || current.name.Contains("ShopPanel"))
            {
                return true;
            }
            
            current = current.parent;
        }
        
        return false;
    }
}

/// <summary>
/// 슬롯 타입
/// </summary>
public enum SlotType
{
    Inventory,
    Equipment,
    Cooking,
    Potion
}
