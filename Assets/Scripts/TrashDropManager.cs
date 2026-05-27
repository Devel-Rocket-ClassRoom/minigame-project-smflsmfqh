using System.Collections.Generic;
using UnityEngine;

public class TrashDropManager : MonoBehaviour
{
    public static TrashDropManager Instance { get; private set; }

    [SerializeField] private float _itemLifeTime = 20f;

    private readonly Dictionary<ItemData, Stack<MissionItem>> _pool = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnItem(ItemData data, Vector3 position)
    {
        MissionItem item = GetOrCreate(data);
        item.transform.SetPositionAndRotation(position, Quaternion.identity);
        item.gameObject.SetActive(true);
    }

    private MissionItem GetOrCreate(ItemData data)
    {
        if (!_pool.TryGetValue(data, out var stack))
        {
            stack = new Stack<MissionItem>();
            _pool[data] = stack;
        }

        return stack.Count > 0 ? stack.Pop() : CreateItem(data);
    }

    private MissionItem CreateItem(ItemData data)
    {
        GameObject go = Instantiate(data.dropPrefab);
        MissionItem item = go.GetComponent<MissionItem>();

        if (item == null)
        {
            Debug.LogError($"[TrashDropManager] {data.itemName}의 dropPrefab에 MissionItem 컴포넌트가 없습니다.");
            Destroy(go);
            return null;
        }

        item.Init(data);
        item.IsPooled = true;
        item.LifeTime = _itemLifeTime;
        item.OnDeactivated += (mi) => ReturnToPool(data, mi);

        go.SetActive(false);
        return item;
    }

    private void ReturnToPool(ItemData data, MissionItem item)
    {
        if (!_pool.TryGetValue(data, out var stack))
        {
            stack = new Stack<MissionItem>();
            _pool[data] = stack;
        }
        stack.Push(item);
        Debug.Log($"[TrashDropManager] {data.itemName} 풀 반납 (잔여: {stack.Count})");
    }
}
