using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전체 UI 관리 (인벤토리, 장비, 요리 등)
/// ESC 키로 최상위 패널부터 닫기 지원
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; // 인벤토리 패널
    [SerializeField] private GameObject equipmentPanel; // 장비 패널
    [SerializeField] private GameObject cookingPanel; // 요리 패널
    
    [Header("Cooking Button")]
    [SerializeField] private GameObject cookingButtonPanel; // 요리 버튼 패널
    [SerializeField] private Button cookingButton; // 요리 버튼
    
    [Header("Potion Button")]
    [SerializeField] private GameObject potionButtonPanel; // 포션 버튼 패널
    [SerializeField] private Button potionButton; // 포션 버튼
    
    private bool isInventoryOpen = false;
    private bool isEquipmentOpen = false;
    private bool isCookingOpen = false;
    private bool isCookingButtonOpen = false;
    private bool isPotionButtonOpen = false;
    
    private void Awake()
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
    
    private void Start()
    {
        // 초기에는 모두 비활성화
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
        
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
        }
        
        if (potionButtonPanel != null)
        {
            potionButtonPanel.SetActive(false);
        }

        // 요리 버튼 이벤트 연결
        if (cookingButton != null)
        {
            cookingButton.onClick.AddListener(OnCookingButtonClicked);
        }

        // 포션 버튼 이벤트 연결
        if (potionButton != null)
        {
            potionButton.onClick.AddListener(OnPotionButtonClicked);
        }
    }
    
    private void Update()
    {
        bool isSplitPanelOpen = ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen();
        
        bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.IsShopOpen();
        
        // ESC 키: 항상 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
            return;
        }
        
        if (isSplitPanelOpen || isShopOpen)
        {
            return;
        }
        
        // I 키: 인벤토리만 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryOnly();
        }
        
        // G 키: 장비창만 토글
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleEquipmentOnly();
        }
        
        // H 키: 요리 버튼 토글
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleCookingButton();
        }

        // J 키: 요리 버튼 토글
        if (Input.GetKeyDown(KeyCode.J))
        {
            TogglePotionButton();
        }
    }
    
    /// <summary>
    /// ESC 키 처리 - 최상위 패널 닫기
    /// </summary>
    private void HandleEscapeKey()
    {
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            ItemSplitManager.Instance.ClosePanel();
            return;
        }
        
        if (PanelStackManager.Instance != null)
        {
            bool closed = PanelStackManager.Instance.CloseTopPanel();
        }
    }
    
    /// <summary>
    /// 인벤토리만 열기/닫기
    /// </summary>
    public void ToggleInventoryOnly()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }
    
    /// <summary>
    /// 인벤토리만 닫기 (외부 호출용)
    /// </summary>
    public void CloseInventoryOnly()
    {
        if (isInventoryOpen)
        {
            isInventoryOpen = false;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            
            if (ItemTooltip.Instance != null)
            {
                ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Inventory);
            }
        }
    }
    
    /// <summary>
    /// 장비창만 열기/닫기
    /// </summary>
    public void ToggleEquipmentOnly()
    {
        isEquipmentOpen = !isEquipmentOpen;
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(isEquipmentOpen);
        }
    }
    
    /// <summary>
    /// 장비창만 닫기 (외부 호출용)
    /// </summary>
    public void CloseEquipmentOnly()
    {
        if (isEquipmentOpen)
        {
            isEquipmentOpen = false;
            if (equipmentPanel != null)
            {
                equipmentPanel.SetActive(false);
            }
            
            if (ItemTooltip.Instance != null)
            {
                ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Equipment);
            }
        }
    }
    
    /// <summary>
    /// 요리 버튼 표시/숨김
    /// </summary>
    public void ToggleCookingButton()
    {
        if (cookingButtonPanel != null)
        {
            bool isActive = cookingButtonPanel.activeSelf;
            cookingButtonPanel.SetActive(!isActive);
        }
    }

    /// <summary>
    /// 요리 버튼 클릭 시
    /// </summary>
    private void OnCookingButtonClicked()
    {        
        // 요리 버튼 숨기기
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
        }
        
        // 요리창 열기
        if (CookingManager.Instance != null)
        {
            CookingManager.Instance.OpenCookingPanel();
        }
                // 인벤토리만 자동으로 열기 (장비창은 제외)
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true; // 상태 업데이트
        }
    }
    
    /// <summary>
    /// 포션 버튼 표시/숨김
    /// </summary>
    public void TogglePotionButton()
    {
        if (potionButtonPanel != null)
        {
            bool isActive = potionButtonPanel.activeSelf;
            potionButtonPanel.SetActive(!isActive);
        }
    }
    
    /// <summary>
    /// 포션 버튼 클릭 시
    /// </summary>
    private void OnPotionButtonClicked()
    {
        // 포션 버튼 숨기기
        if (potionButtonPanel != null)
        {
            potionButtonPanel.SetActive(false);
        }
        
        // 포션창 열기
        if (PotionManager.Instance != null)
        {
            PotionManager.Instance.OpenPotionPanel();
        }
                // 인벤토리만 자동으로 열기 (장비창은 제외)
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true; // 상태 업데이트
        }
    }
    
    /// <summary>
    /// 모든 패널 닫기 (상점 열 때 사용)
    /// </summary>
    public void CloseAllPanels()
    {
        // 인벤토리 닫기
        if (isInventoryOpen && inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        
        // 장비창 닫기
        if (isEquipmentOpen && equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            isEquipmentOpen = false;
        } 
    }
    
    /// <summary>
    /// 인벤토리가 열려있는지 확인
    /// </summary>
    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }
    
    /// <summary>
    /// 장비창이 열려있는지 확인
    /// </summary>
    public bool IsEquipmentOpen()
    {
        return isEquipmentOpen;
    }
}