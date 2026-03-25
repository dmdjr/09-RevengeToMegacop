using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class DashAfterimageEffect : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Shader unlitShader;

    private PlayerMovementController movementController;

    private struct MeshEntry
    {
        public MeshFilter filter;
        public Transform transform;
    }

    private struct PoolItem
    {
        public GameObject go;
        public MeshFilter filter;
        public Material material;
    }

    private struct ActiveItem
    {
        public int poolIndex;
        public float elapsed;
    }

    // Shader.PropertyToID로 캐싱하면 매 프레임 문자열 해시 연산을 피할 수 있다.
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private MeshEntry[] meshEntries;
    private float timer;

    private PoolItem[] pool;
    private Queue<int> freeIndices;
    private List<ActiveItem> activeItems;

    void Awake()
    {
        movementController = GetComponent<PlayerMovementController>();

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        meshEntries = new MeshEntry[filters.Length];
        for (int i = 0; i < filters.Length; i++)
        {
            meshEntries[i] = new MeshEntry
            {
                filter = filters[i],
                transform = filters[i].transform
            };
        }

        InitPool();
    }

    private void InitPool()
    {
        // 동시에 살아있을 수 있는 최대 잔상 수 = ceil(fadeDuration / spawnInterval) * meshEntries.Length
        int maxConcurrentSpawns = Mathf.CeilToInt(fadeDuration / spawnInterval) + 1;
        int poolSize = maxConcurrentSpawns * meshEntries.Length;

        if (unlitShader == null)
        {
            Debug.LogError("DashAfterimageEffect: unlitShader is not assigned. Afterimage effect disabled.");
            enabled = false;
            return;
        }

        pool = new PoolItem[poolSize];
        freeIndices = new Queue<int>(poolSize);
        activeItems = new List<ActiveItem>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("Afterimage");
            go.SetActive(false);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            // URP에서 런타임으로 투명 머티리얼을 생성할 때는 셰이더 속성을 직접 설정해야 한다.
            // Inspector에서 Surface Type을 Transparent로 바꾸면 Unity가 내부적으로 아래 값들을 자동 설정하지만,
            // 코드로 생성하면 그 과정이 생략되므로 수동으로 동일하게 맞춰야 한다.
            Material mat = new Material(unlitShader);

            // Surface Type: 0 = Opaque(불투명), 1 = Transparent(반투명)
            mat.SetFloat("_Surface", 1f);

            // Blend Mode: 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
            // Alpha 블렌딩은 픽셀 색상을 알파값에 따라 뒤 오브젝트와 혼합한다.
            mat.SetFloat("_Blend", 0f);

            // GPU 블렌딩 공식: 최종색 = (SrcColor × SrcBlend) + (DstColor × DstBlend)
            // SrcAlpha / OneMinusSrcAlpha 조합이 일반적인 알파 블렌딩이다.
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // ZWrite: 깊이 버퍼 기록 여부. 투명 오브젝트는 0(끔)으로 설정해야
            // 뒤에 있는 오브젝트가 가려지지 않는다.
            mat.SetInt("_ZWrite", 0);

            // URP 셰이더 내부 분기용 키워드. 이 키워드가 없으면 셰이더가
            // Transparent 경로로 컴파일되지 않아 투명도가 적용되지 않는다.
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            // RenderQueue: 투명 오브젝트는 불투명 오브젝트(2000)보다 나중에 그려야
            // 깊이 정렬이 올바르게 된다. Transparent = 3000.
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            mr.material = mat;

            pool[i] = new PoolItem { go = go, filter = mf, material = mat };
            freeIndices.Enqueue(i);
        }
    }

    void Update()
    {
        if (!movementController.IsDashing)
        {
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnAfterimage();
            }
        }

        UpdateActiveItems();
    }

    private void SpawnAfterimage()
    {
        foreach (MeshEntry entry in meshEntries)
        {
            if (entry.filter.sharedMesh == null) continue;
            if (freeIndices.Count == 0) continue;

            int idx = freeIndices.Dequeue();
            PoolItem item = pool[idx];

            item.go.transform.SetPositionAndRotation(entry.transform.position, entry.transform.rotation);
            item.go.transform.localScale = entry.transform.lossyScale;
            item.filter.sharedMesh = entry.filter.sharedMesh;
            item.material.SetColor(BaseColorId, afterimageColor);
            item.go.SetActive(true);

            activeItems.Add(new ActiveItem { poolIndex = idx, elapsed = 0f });
        }
    }

    private void UpdateActiveItems()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            ActiveItem active = activeItems[i];
            active.elapsed += Time.deltaTime;

            if (active.elapsed >= fadeDuration)
            {
                pool[active.poolIndex].go.SetActive(false);
                freeIndices.Enqueue(active.poolIndex);
                activeItems.RemoveAt(i);
                continue;
            }

            float alpha = Mathf.Lerp(afterimageColor.a, 0f, active.elapsed / fadeDuration);
            pool[active.poolIndex].material.SetColor(BaseColorId,
                new Color(afterimageColor.r, afterimageColor.g, afterimageColor.b, alpha));

            activeItems[i] = active;
        }
    }

    private void OnDestroy()
    {
        if (pool == null) return;
        foreach (PoolItem item in pool)
        {
            Destroy(item.material);
            Destroy(item.go);
        }
    }
}
