using UnityEngine;

/// <summary>
/// 並列学習用の環境生成スクリプト
/// 複数の訓練環境を自動生成して学習を高速化
/// </summary>
public class TrainingAreaSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject trainingAreaPrefab;
    
    [Header("Grid Settings")]
    public int gridSizeX = 4;
    public int gridSizeZ = 4;
    public float spacing = 25f;
    
    private void Awake()
    {
        SpawnTrainingAreas();
    }
    
    private void SpawnTrainingAreas()
    {
        if (trainingAreaPrefab == null)
        {
            Debug.LogError("Training Area Prefab not assigned!");
            return;
        }
        
        Vector3 startPos = transform.position;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 pos = startPos + new Vector3(x * spacing, 0, z * spacing);
                GameObject area = Instantiate(trainingAreaPrefab, pos, Quaternion.identity, transform);
                area.name = $"TrainingArea_{x}_{z}";
            }
        }
        
        Debug.Log($"Spawned {gridSizeX * gridSizeZ} training environments");
    }
}

/// <summary>
/// 単一の訓練環境を管理
/// Prefab化してTrainingAreaSpawnerで複製する
/// </summary>
public class TrainingArea : MonoBehaviour
{
    [Header("Area Settings")]
    public float areaSize = 10f;
    public float wallHeight = 2f;
    public float wallThickness = 0.5f;
    
    [Header("Agent Prefabs")]
    public GameObject agentPrefab;
    
    private TagAgent tagger;
    private TagAgent runner;
    private TagGameManager gameManager;
    
    private void Start()
    {
        CreateFloor();
        CreateWalls();
        CreateAgents();
        SetupGameManager();
    }
    
    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.SetParent(transform);
        floor.transform.localPosition = Vector3.zero;
        floor.transform.localScale = new Vector3(areaSize / 10f, 1, areaSize / 10f);
        floor.name = "Floor";
        
        // 床の色
        var renderer = floor.GetComponent<Renderer>();
        renderer.material.color = new Color(0.3f, 0.3f, 0.3f);
    }
    
    private void CreateWalls()
    {
        // 4方向の壁を作成
        CreateWall("WallNorth", new Vector3(0, wallHeight / 2, areaSize / 2), 
                   new Vector3(areaSize + wallThickness, wallHeight, wallThickness));
        CreateWall("WallSouth", new Vector3(0, wallHeight / 2, -areaSize / 2), 
                   new Vector3(areaSize + wallThickness, wallHeight, wallThickness));
        CreateWall("WallEast", new Vector3(areaSize / 2, wallHeight / 2, 0), 
                   new Vector3(wallThickness, wallHeight, areaSize));
        CreateWall("WallWest", new Vector3(-areaSize / 2, wallHeight / 2, 0), 
                   new Vector3(wallThickness, wallHeight, areaSize));
    }
    
    private void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;
        wall.name = name;
        wall.layer = LayerMask.NameToLayer("Wall");
        
        var renderer = wall.GetComponent<Renderer>();
        renderer.material.color = Color.gray;
    }
    
    private void CreateAgents()
    {
        if (agentPrefab == null)
        {
            Debug.LogWarning("Agent prefab not set, creating basic agents");
            CreateBasicAgent(true);
            CreateBasicAgent(false);
        }
        else
        {
            var taggerObj = Instantiate(agentPrefab, transform);
            taggerObj.name = "Tagger";
            tagger = taggerObj.GetComponent<TagAgent>();
            tagger.isTagger = true;
            
            var runnerObj = Instantiate(agentPrefab, transform);
            runnerObj.name = "Runner";
            runner = runnerObj.GetComponent<TagAgent>();
            runner.isTagger = false;
        }
        
        // 相互参照
        tagger.opponent = runner;
        runner.opponent = tagger;
        tagger.areaCenter = transform;
        runner.areaCenter = transform;
        tagger.areaSize = areaSize;
        runner.areaSize = areaSize;
    }
    
    private void CreateBasicAgent(bool isTagger)
    {
        GameObject agentObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        agentObj.transform.SetParent(transform);
        agentObj.transform.localScale = Vector3.one * 0.8f;
        agentObj.name = isTagger ? "Tagger" : "Runner";
        
        // Rigidbody
        var rb = agentObj.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 2f;
        
        // 色
        var renderer = agentObj.GetComponent<Renderer>();
        renderer.material.color = isTagger ? Color.red : Color.blue;
        
        // TagAgent
        var agent = agentObj.AddComponent<TagAgent>();
        agent.isTagger = isTagger;
        
        if (isTagger)
            tagger = agent;
        else
            runner = agent;
    }
    
    private void SetupGameManager()
    {
        gameManager = gameObject.AddComponent<TagGameManager>();
        gameManager.tagger = tagger;
        gameManager.runner = runner;
    }
}
