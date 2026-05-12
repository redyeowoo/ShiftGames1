using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 우클릭 컨텍스트 메뉴 관리
/// 슬롯 타입과 아이템 타입에 따라 적절한 메뉴 표시
/// </summary>
public class ContextMenu : MonoBehaviour
{
    public static ContextMenu Instance { get; private set; }
    
    #region UI References
    [Header("메뉴 패널")]
    [SerializeField] private GameObject equipmentMenuPanel;
    [SerializeField] private GameObject consumableMenuPanel;
    [SerializeField] private GameObject unequipmentMenuPanel;
    [SerializeField] private GameObject quickSlotMenuPanel;
    [SerializeField] private GameObject ingredientMenuPanel;
    [SerializeField] private GameObject cookingIngredientMenuPanel;
    [SerializeField] private GameObject cookingSlotMenuPanel;
    [SerializeField] private GameObject potionIngredientMenuPanel;
    [SerializeField] private GameObject potionSlotMenuPanel;
    [SerializeField] private GameObject lootMenuPanel;
    
    [Header("확인 팝업")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    [Header("장비 메뉴 버튼")]
    [SerializeField] private Button useButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private TextMeshProUGUI useButtonText;
    
    [Header("소비 아이템 메뉴 버튼")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button consumeButton;
    [SerializeField] private Button discardButton2;
    
    [Header("장비 해제 메뉴 버튼")]
    [SerializeField] private Button unequipButton;
    [SerializeField] private Button discardButton3;
    
    [Header("퀵슬롯 메뉴 버튼")]
    [SerializeField] private Button unequipButton2;
    [SerializeField] private Button consumeButton2;
    [SerializeField] private Button discardButton4;
    
    [Header("재료 메뉴 버튼")]
    [SerializeField] private Button discardButton5;
    
    [Header("인벤토리 요리 재료 메뉴 버튼")]
    [SerializeField] private Button addToCookingButton;
    [SerializeField] private Button discardButton6;
    
    [Header("요리 슬롯 메뉴 버튼")]
    [SerializeField] private Button removeFromCookingButton;
    [SerializeField] private Button discardButton7;

    [Header("인벤토리 포션 재료 메뉴 버튼")]
    [SerializeField] private Button addToPotionButton;
    [SerializeField] private Button discardButton8;

    [Header("포션 슬롯 메뉴 버튼")]
    [SerializeField] private Button removeFromPotionButton;
    [SerializeField] private Button discardButton9;
    
    [Header("전리품 메뉴 버튼")]
    [SerializeField] private Button transferToInventoryButton;
    [SerializeField] private Button discardLootButton;
    #endregion
    
    #region Private Fields
    private SlotUI targetSlot;
    private SlotType targetSlotType;
    
    private RectTransform equipmentMenuRect;
    private RectTransform consumableMenuRect;
    private RectTransform unequipmentMenuRect;
    private RectTransform quickSlotMenuRect;
    private RectTransform ingredientMenuRect;
    private RectTransform cookingIngredientMenuRect;
    private RectTransform cookingSlotMenuRect;
    private RectTransform potionIngredientMenuRect;
    private RectTransform potionSlotMenuRect;
    private RectTransform lootMenuRect;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
        CacheRectTransforms();
    }
    
    private void Start()
    {
        RegisterButtonEvents();
        DeactivateAllMenus();
    }
    
    private void Update()
    {
        HandleOutsideClick();
        HandlePanelCloseKeys();
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
    
    private void CacheRectTransforms()
    {
        equipmentMenuRect = equipmentMenuPanel.GetComponent<RectTransform>();
        consumableMenuRect = GetRectTransformSafe(consumableMenuPanel);
        unequipmentMenuRect = GetRectTransformSafe(unequipmentMenuPanel);
        quickSlotMenuRect = GetRectTransformSafe(quickSlotMenuPanel);
        ingredientMenuRect = GetRectTransformSafe(ingredientMenuPanel);
        cookingIngredientMenuRect = GetRectTransformSafe(cookingIngredientMenuPanel);
        cookingSlotMenuRect = GetRectTransformSafe(cookingSlotMenuPanel);
        potionIngredientMenuRect = GetRectTransformSafe(potionIngredientMenuPanel);
        potionSlotMenuRect = GetRectTransformSafe(potionSlotMenuPanel);
        lootMenuRect = GetRectTransformSafe(lootMenuPanel);
    }
    
    private RectTransform GetRectTransformSafe(GameObject obj)
    {
        return obj != null ? obj.GetComponent<RectTransform>() : null;
    }
    
    private void RegisterButtonEvents()
    {
        // 장비 메뉴
        useButton.onClick.AddListener(OnUseButtonClicked);
        discardButton.onClick.AddListener(() => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 소비 아이템 메뉴
        RegisterButtonSafe(equipButton, OnEquipButtonClicked);
        RegisterButtonSafe(consumeButton, OnConsumeButtonClicked);
        RegisterButtonSafe(discardButton2, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 장비 해제 메뉴
        RegisterButtonSafe(unequipButton, OnUnequipButtonClicked);
        RegisterButtonSafe(discardButton3, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 퀵슬롯 메뉴
        RegisterButtonSafe(unequipButton2, OnUnequipButtonClicked);
        RegisterButtonSafe(consumeButton2, OnConsumeButtonClicked);
        RegisterButtonSafe(discardButton4, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 재료 메뉴
        RegisterButtonSafe(discardButton5, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 인벤토리 요리 재료 메뉴
        RegisterButtonSafe(addToCookingButton, OnAddToCookingButtonClicked);
        RegisterButtonSafe(discardButton6, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 요리 슬롯 메뉴
        RegisterButtonSafe(removeFromCookingButton, OnRemoveFromCookingButtonClicked);
        RegisterButtonSafe(discardButton7, () => ShowConfirmDialog("정말 버리시겠습니까?"));

        // 인벤토리 포션 재료 메뉴
        RegisterButtonSafe(addToPotionButton, () => OnAddToPotionButtonClicked());
        RegisterButtonSafe(discardButton8, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 포션 슬롯 메뉴
        RegisterButtonSafe(removeFromPotionButton, OnRemoveFromPotionButtonClicked);
        RegisterButtonSafe(discardButton9, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 전리품 메뉴
        RegisterButtonSafe(transferToInventoryButton, OnTransferToInventoryButtonClicked);
        RegisterButtonSafe(discardLootButton, () => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 확인 팝업
        RegisterButtonSafe(confirmYesButton, OnConfirmYes);
        RegisterButtonSafe(confirmNoButton, OnConfirmNo);
    }
    
    private void RegisterButtonSafe(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }
    
    private void DeactivateAllMenus()
    {
        equipmentMenuPanel.SetActive(false);
        SetActiveSafe(consumableMenuPanel, false);
        SetActiveSafe(unequipmentMenuPanel, false);
        SetActiveSafe(quickSlotMenuPanel, false);
        SetActiveSafe(ingredientMenuPanel, false);
        SetActiveSafe(cookingIngredientMenuPanel, false);
        SetActiveSafe(cookingSlotMenuPanel, false);
        SetActiveSafe(potionIngredientMenuPanel, false);
        SetActiveSafe(potionSlotMenuPanel, false);
        SetActiveSafe(lootMenuPanel, false);
        SetActiveSafe(confirmPanel, false);
    }
    
    private void SetActiveSafe(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }
    #endregion
    
    #region Menu Control
    public void OpenMenu(SlotUI slot, Vector2 position)
    {
        if (!IsValidSlot(slot)) return;
        
        targetSlot = slot;
        targetSlotType = slot.slotType;
        CloseAllMenus();
        
        MenuType menuType = DetermineMenuType(slot);
        ShowMenu(menuType, position);
    }
    
    private bool IsValidSlot(SlotUI slot)
    {
        return slot != null && slot.currentItem != null;
    }
    
    private MenuType DetermineMenuType(SlotUI slot)
    {
        if (IsLootSlot(slot))
            return MenuType.Loot;
        
        // 1. 요리 슬롯
        if (slot.slotType == SlotType.Cooking)
            return MenuType.CookingSlot;
        
        // 2. 포션 슬롯
        if (slot.slotType == SlotType.Potion)
            return MenuType.PotionSlot;
        
        // 3. 재료 아이템 — 어떤 창이 열려 있는지 함께 체크
        if (slot.currentItem.itemType == ItemType.Ingredient)
        {
            bool isCookingOpen = CookingManager.Instance != null && CookingManager.Instance.IsCookingOpen();
            if (isCookingOpen) return MenuType.CookingIngredient;
            
            bool isPotionOpen = PotionManager.Instance != null && PotionManager.Instance.IsPotionOpen();
            if (isPotionOpen) return MenuType.PotionIngredient;
            
            return MenuType.Ingredient;
        }
        
        // 4. 퀵슬롯 소비 아이템
        if (IsQuickSlotConsumable(slot))
            return MenuType.QuickSlot;
        
        // 5. 장비 슬롯
        if (slot.slotType == SlotType.Equipment)
            return MenuType.Unequipment;
        
        // 6. 인벤토리 소비 아이템
        if (slot.slotType == SlotType.Inventory && slot.currentItem.itemType == ItemType.Consumable)
            return MenuType.Consumable;
        
        // 7. 기본: 장비 메뉴
        return MenuType.Equipment;
    }
    
    private bool IsLootSlot(SlotUI slot)
    {
        if (LootManager.Instance != null && LootManager.Instance.lootSlots != null)
        {
            return LootManager.Instance.lootSlots.Contains(slot);
        }
        return false;
    }
    
    private bool IsQuickSlotConsumable(SlotUI slot)
    {
        return slot.slotType == SlotType.Inventory && 
               slot.gameObject.name.Contains("Quick") && 
               slot.currentItem.itemType == ItemType.Consumable;
    }
    
    private void ShowMenu(MenuType menuType, Vector2 position)
    {
        GameObject menuPanel = null;
        RectTransform menuRect = null;
        
        switch (menuType)
        {
            case MenuType.Loot:
                menuPanel = lootMenuPanel;
                menuRect = lootMenuRect;
                break;
                
            case MenuType.CookingSlot:
                menuPanel = cookingSlotMenuPanel;
                menuRect = cookingSlotMenuRect;
                break;
                
            case MenuType.CookingIngredient:
                menuPanel = cookingIngredientMenuPanel;
                menuRect = cookingIngredientMenuRect;
                break;
            
            case MenuType.PotionSlot:
                menuPanel = potionSlotMenuPanel;
                menuRect = potionSlotMenuRect;
                break;
            
            case MenuType.PotionIngredient:
                menuPanel = potionIngredientMenuPanel;
                menuRect = potionIngredientMenuRect;
                break;
                
            case MenuType.Ingredient:
                menuPanel = ingredientMenuPanel;
                menuRect = ingredientMenuRect;
                break;
                
            case MenuType.QuickSlot:
                menuPanel = quickSlotMenuPanel;
                menuRect = quickSlotMenuRect;
                break;
                
            case MenuType.Unequipment:
                menuPanel = unequipmentMenuPanel;
                menuRect = unequipmentMenuRect;
                break;
                
            case MenuType.Consumable:
                menuPanel = consumableMenuPanel;
                menuRect = consumableMenuRect;
                break;
                
            case MenuType.Equipment:
                UpdateEquipmentButtonText();
                menuPanel = equipmentMenuPanel;
                menuRect = equipmentMenuRect;
                break;
        }
        
        if (menuPanel != null && menuRect != null)
        {
            menuPanel.SetActive(true);
            menuRect.position = position;
            AdjustMenuPosition(menuRect);
        }
    }
    
    private void UpdateEquipmentButtonText()
    {
        useButtonText.text = IsEquipmentItem(targetSlot.currentItem) ? "장착하기" : "사용하기";
    }
    
    public void CloseMenu()
    {
        CloseAllMenus();
        targetSlot = null;
        targetSlotType = SlotType.Inventory;
    }
    
    private void CloseAllMenus()
    {
        equipmentMenuPanel.SetActive(false);
        SetActiveSafe(consumableMenuPanel, false);
        SetActiveSafe(unequipmentMenuPanel, false);
        SetActiveSafe(quickSlotMenuPanel, false);
        SetActiveSafe(ingredientMenuPanel, false);
        SetActiveSafe(cookingIngredientMenuPanel, false);
        SetActiveSafe(cookingSlotMenuPanel, false);
        SetActiveSafe(potionIngredientMenuPanel, false);
        SetActiveSafe(potionSlotMenuPanel, false);
        SetActiveSafe(lootMenuPanel, false);
    }
    
    private void AdjustMenuPosition(RectTransform rectTransform)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        float rightOverflow = corners[2].x - Screen.width;
        if (rightOverflow > 0)
        {
            rectTransform.position -= new Vector3(rightOverflow, 0, 0);
        }
        
        float bottomOverflow = -corners[0].y;
        if (bottomOverflow > 0)
        {
            rectTransform.position += new Vector3(0, bottomOverflow, 0);
        }
    }
    #endregion
    
    #region Input Handling
    private void HandleOutsideClick()
    {
        if (confirmPanel != null && confirmPanel.activeSelf) return;
        if (!Input.GetMouseButtonDown(0)) return;
        
        if (IsAnyMenuActive() && !IsMouseOverAnyMenu())
        {
            CloseMenu();
        }
    }
    
    private void HandlePanelCloseKeys()
    {
        if (confirmPanel != null && confirmPanel.activeSelf) return;
        if (!IsAnyMenuActive()) return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (targetSlotType == SlotType.Inventory && !IsLootSlot(targetSlot))
            {
                CloseMenu();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (targetSlot != null && IsLootSlot(targetSlot))
            {
                CloseMenu();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (targetSlotType == SlotType.Equipment)
            {
                CloseMenu();
            }
        }
    }
    
    private bool IsAnyMenuActive()
    {
        return equipmentMenuPanel.activeSelf ||
                (consumableMenuPanel != null && consumableMenuPanel.activeSelf) ||
                (unequipmentMenuPanel != null && unequipmentMenuPanel.activeSelf) ||
                (quickSlotMenuPanel != null && quickSlotMenuPanel.activeSelf) ||
                (ingredientMenuPanel != null && ingredientMenuPanel.activeSelf) ||
                (cookingIngredientMenuPanel != null && cookingIngredientMenuPanel.activeSelf) ||
                (cookingSlotMenuPanel != null && cookingSlotMenuPanel.activeSelf) ||
                (potionIngredientMenuPanel != null && potionIngredientMenuPanel.activeSelf) ||
                (potionSlotMenuPanel != null && potionSlotMenuPanel.activeSelf) ||
                (lootMenuPanel != null && lootMenuPanel.activeSelf);
    }
    
    private bool IsMouseOverAnyMenu()
    {
        Vector2 mousePos = Input.mousePosition;
        
        return IsMouseOverMenu(equipmentMenuPanel, equipmentMenuRect, mousePos) ||
               IsMouseOverMenu(consumableMenuPanel, consumableMenuRect, mousePos) ||
               IsMouseOverMenu(unequipmentMenuPanel, unequipmentMenuRect, mousePos) ||
               IsMouseOverMenu(quickSlotMenuPanel, quickSlotMenuRect, mousePos) ||
               IsMouseOverMenu(ingredientMenuPanel, ingredientMenuRect, mousePos) ||
               IsMouseOverMenu(cookingIngredientMenuPanel, cookingIngredientMenuRect, mousePos) ||
               IsMouseOverMenu(cookingSlotMenuPanel, cookingSlotMenuRect, mousePos) ||
               IsMouseOverMenu(potionIngredientMenuPanel, potionIngredientMenuRect, mousePos) ||
               IsMouseOverMenu(potionSlotMenuPanel, potionSlotMenuRect, mousePos) ||
               IsMouseOverMenu(lootMenuPanel, lootMenuRect, mousePos);
    }
    
    private bool IsMouseOverMenu(GameObject panel, RectTransform rect, Vector2 mousePos)
    {
        return panel != null && panel.activeSelf && 
               rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos);
    }
    #endregion
    
    #region Button Callbacks
    private void OnEquipButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        InventoryManager.Instance.EquipToQuickSlot(targetSlot);
        CloseMenu();
    }
    
    private void OnUseButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        if (IsEquipmentItem(targetSlot.currentItem))
        {
            InventoryManager.Instance.EquipItem(targetSlot);
        }
                CloseMenu();
    }
    
    private void OnConsumeButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        UseConsumable(targetSlot);
        CloseMenu();
    }
    
    private void OnUnequipButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            TransferItem(targetSlot, emptySlot);
        }
                CloseMenu();
    }
    
    private void OnAddToCookingButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        ItemData ingredient = targetSlot.currentItem;
        
        if (ingredient.itemType != ItemType.Ingredient)
        {
            CloseMenu();
            return;
        }
        
        if (CookingManager.Instance == null)
        {
            CloseMenu();
            return;
        }
        
        bool success = CookingManager.Instance.TryAddIngredient(ingredient, targetSlot);
        
        CloseMenu();
    }
    
    private void OnRemoveFromCookingButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            TransferItem(targetSlot, emptySlot);
        }
                CloseMenu();
    }

    private void OnAddToPotionButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        ItemData ingredient = targetSlot.currentItem;
        
        if (ingredient.itemType != ItemType.Ingredient)
        {
            CloseMenu();
            return;
        }
        
        if (PotionManager.Instance == null)
        {
            CloseMenu();
            return;
        }
        
        bool success = PotionManager.Instance.TryAddIngredient(ingredient, targetSlot);
        
        CloseMenu();
    }

    private void OnRemoveFromPotionButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            TransferItem(targetSlot, emptySlot);
        }
        CloseMenu();
    }
    
    private void OnTransferToInventoryButtonClicked()
    {
        if (!IsValidTargetSlot()) return;
        
        if (LootManager.Instance == null)
        {
            CloseMenu();
            return;
        }
        
        if (!LootManager.Instance.IsLootSlot(targetSlot))
        {
            CloseMenu();
            return;
        }
        
        LootManager.Instance.TransferLootToInventory(targetSlot, 0);
        CloseMenu();
    }
    
    private void ShowConfirmDialog(string message)
    {
        if (confirmPanel == null) return;
        
        CloseAllMenus();
        
        if (confirmMessageText != null)
        {
            confirmMessageText.text = message;
        }
        
        confirmPanel.SetActive(true);
    }
    
    private void OnConfirmYes()
    {
        if (targetSlot != null && targetSlot.currentItem != null)
        {
            string itemName = targetSlot.currentItem.itemName;
            int quantity = targetSlot.quantity;
            
            targetSlot.ClearSlot();
        }
        
        SetActiveSafe(confirmPanel, false);
        targetSlot = null;
    }
    
    private void OnConfirmNo()
    {
        SetActiveSafe(confirmPanel, false);
        targetSlot = null;
    }
    #endregion
    
    #region Helper Methods
    private bool IsValidTargetSlot()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return false;
        }
        return true;
    }
    
    private void UseConsumable(SlotUI slot)
    {
        slot.quantity--;
        
        if (slot.quantity <= 0)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.UpdateUI();
        }
    }
    
    private void TransferItem(SlotUI from, SlotUI to)
    {
        to.SetItem(from.currentItem, from.quantity);
        from.ClearSlot();
        
        to.UpdateUI();
        from.UpdateUI();
    }
    
    private bool IsEquipmentItem(ItemData item)
    {
        return item.itemType == ItemType.Weapon ||
               item.itemType == ItemType.Helmet ||
               item.itemType == ItemType.Armor ||
               item.itemType == ItemType.Shoes ||
               item.itemType == ItemType.Bag ||
               item.itemType == ItemType.Quiver;
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 컨텍스트 메뉴가 열려있는지 확인
    /// </summary>
    public bool IsMenuOpen()
    {
        return  (equipmentMenuPanel != null && equipmentMenuPanel.activeSelf) ||
                (consumableMenuPanel != null && consumableMenuPanel.activeSelf) ||
                (unequipmentMenuPanel != null && unequipmentMenuPanel.activeSelf) ||
                (quickSlotMenuPanel != null && quickSlotMenuPanel.activeSelf) ||
                (ingredientMenuPanel != null && ingredientMenuPanel.activeSelf) ||
                (cookingIngredientMenuPanel != null && cookingIngredientMenuPanel.activeSelf) ||
                (cookingSlotMenuPanel != null && cookingSlotMenuPanel.activeSelf) ||
                (potionIngredientMenuPanel != null && potionIngredientMenuPanel.activeSelf) ||
                (potionSlotMenuPanel != null && potionSlotMenuPanel.activeSelf) ||
                (lootMenuPanel != null && lootMenuPanel.activeSelf) ||
                (confirmPanel != null && confirmPanel.activeSelf);
    }
    #endregion
    
    #region Nested Types
    private enum MenuType
    {
        Equipment,
        Consumable,
        Unequipment,
        QuickSlot,
        Ingredient,
        CookingIngredient,
        CookingSlot,
        PotionIngredient,
        PotionSlot,
        Loot
    }
    #endregion
}