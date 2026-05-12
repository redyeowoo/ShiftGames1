// Assets/Scripts/Inventory/InventoryTester.cs

using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("테스트 아이템들 (Inspector에 드래그)")]
    [SerializeField] private ItemData[] testItems;
    private int currentIndex = 0;
    
    private void Start()
    {
        // InventoryManager 확인
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager가 씬에 없습니다! GameManager 오브젝트를 확인하세요.");
        }
        
        // 검증
        if (testItems == null || testItems.Length == 0)
        {
            Debug.LogError("testItems 배열이 비어있습니다! Inspector에서 아이템을 할당하세요.");
            return;
        }
        
        for (int i = 0; i < testItems.Length; i++)
        {
            if (testItems[i] == null)
                Debug.LogWarning($"testItems[{i}]가 null입니다!");
            else
                Debug.Log($"아이템 [{i}] 준비됨: {testItems[i].itemName}");
        }
        
        Debug.Log("=== 테스트 준비 완료 ===");
        Debug.Log("I: 인벤토리에 아이템 추가");
        Debug.Log("O: 인벤토리에서 아이템 제거");
        Debug.Log("L: 전리품에 아이템 추가");
        Debug.Log("P: 전리품 창 열기/닫기");
        Debug.Log("T: 모든 전리품 획득");
        Debug.Log("G: 골드 100 추가 / R: 골드 50 감소");
    }
    
    private void Update()
    {
        // I 키는 UIManager에서 처리하므로 제거
        
        // U 키: 재료 아이템 추가 (순환)
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager가 없습니다!");
                return;
            }
            
            if (testItems != null && testItems.Length > 0 && testItems[currentIndex] != null)
            {
                InventoryManager.Instance.AddItem(testItems[currentIndex], 1);
                Debug.Log($"✓ 인벤토리에 추가: {testItems[currentIndex].itemName}");
                currentIndex = (currentIndex + 1) % testItems.Length;
            }
            else
            {
                Debug.LogError($"현재 인덱스 [{currentIndex}]의 아이템이 null입니다!");
            }
        }
        
        // O 키: 인벤토리에서 아이템 제거
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager가 없습니다!");
                return;
            }
            
            if (testItems != null && testItems.Length > 0 && testItems[currentIndex] != null)
            {
                ItemData itemToRemove = testItems[currentIndex];
                int count = InventoryManager.Instance.GetItemCount(itemToRemove);
                
                if (count > 0)
                {
                    InventoryManager.Instance.RemoveItem(itemToRemove, 1);
                    Debug.Log($"✓ 인벤토리에서 제거: {itemToRemove.itemName} (남은 개수: {InventoryManager.Instance.GetItemCount(itemToRemove)})");
                }
                else
                {
                    Debug.LogWarning($"{itemToRemove.itemName}이(가) 인벤토리에 없습니다!");
                }
                
                currentIndex = (currentIndex + 1) % testItems.Length;
            }
            else
            {
                Debug.LogError($"현재 인덱스 [{currentIndex}]의 아이템이 null입니다!");
            }
        }
        
        // L 키: 전리품에 아이템 추가
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (LootManager.Instance == null)
            {
                Debug.LogError("LootManager가 없습니다!");
                return;
            }
            
            if (testItems != null && testItems.Length > 0 && testItems[currentIndex] != null)
            {
                LootManager.Instance.AddLoot(testItems[currentIndex], 1);
                Debug.Log($"✓ 전리품에 추가: {testItems[currentIndex].itemName}");
                currentIndex = (currentIndex + 1) % testItems.Length;
            }
            else
            {
                Debug.LogError($"현재 인덱스 [{currentIndex}]의 아이템이 null입니다!");
            }
        }
        
        // T 키: 모든 전리품 획득
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (LootManager.Instance != null)
            {
                LootManager.Instance.TakeAllLoot();
            }
            else
            {
                Debug.LogError("LootManager가 없습니다!");
            }
        }
        
        // G 키: 골드 추가
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddGold(100);
            }
            else
            {
                Debug.LogError("InventoryManager가 없습니다!");
            }
        }
        
        // R 키: 골드 감소
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveGold(50);
            }
            else
            {
                Debug.LogError("InventoryManager가 없습니다!");
            }
        }
    }
}