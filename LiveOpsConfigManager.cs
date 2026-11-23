using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LiveOpsConfig
{
    public string configVersion;
    public string activeEventId;
    public string eventBannerTitle;
    public string eventBannerDescription;
    public float taskSpawnIntervalMultiplier = 1.0f;
    public float patientDeteriorationMultiplier = 1.0f;
    public int supplyCabinetBonusUnits = 0;
    public float scoreMultiplier = 1.0f;
    public string highlightedRoomId;
}

public class LiveOpsConfigManager : MonoBehaviour
{
    public static LiveOpsConfigManager Instance { get; private set; }

    [Header("Remote Live Ops JSON Endpoint")]
    [Tooltip("Raw JSON URL (e.g., GitHub Gist/Repo or CDN). Falls back to StreamingAssets if offline.")]
    [SerializeField] private string remoteConfigUrl = "https://raw.githubusercontent.com/your-org/last-shift/main/liveops_config.json";
    [SerializeField] private string localFallbackFileName = "liveops_config.json";
    [SerializeField] private float requestTimeoutSeconds = 5f;

    public LiveOpsConfig CurrentConfig { get; private set; }
    public event Action<LiveOpsConfig> OnConfigLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(FetchLiveOpsConfig());
    }

    public IEnumerator FetchLiveOpsConfig()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(remoteConfigUrl))
        {
            request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ApplyJsonConfig(request.downloadHandler.text, "Remote Server");
            }
            else
            {
                Debug.LogWarning($"[LiveOps] Remote fetch failed ({request.error}). Loading local fallback...");
                LoadLocalFallbackConfig();
            }
        }
    }

    private void LoadLocalFallbackConfig()
    {
        string localPath = Path.Combine(Application.streamingAssetsPath, localFallbackFileName);
        if (File.Exists(localPath))
        {
            string json = File.ReadAllText(localPath);
            ApplyJsonConfig(json, "Local StreamingAssets");
        }
        else
        {
            CurrentConfig = new LiveOpsConfig();
            Debug.Log("[LiveOps] No local file found. Using built-in default parameters.");
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }

    private void ApplyJsonConfig(string json, string source)
    {
        try
        {
            CurrentConfig = JsonUtility.FromJson<LiveOpsConfig>(json);
            Debug.Log($"[LiveOps] Loaded '{CurrentConfig.activeEventId}' (v{CurrentConfig.configVersion}) from {source}.");
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LiveOps] Failed to parse JSON configuration: {ex.Message}");
            CurrentConfig = new LiveOpsConfig();
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }
}
