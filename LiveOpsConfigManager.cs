using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class EventModifiers
{
    public float vitalsTaskProbabilityMultiplier = 1.0f;
    public float taskSpawnIntervalMultiplier = 1.0f;
    public float feverPatientSpeedBonus = 1.0f;
    public float scoreMultiplier = 1.0f;
}

[Serializable]
public class ActiveEventData
{
    public string eventId = "NONE";
    public bool isActive = false;
    public string eventTitle = "";
    public string eventDescription = "";
    public string themeTintHex = "#FFFFFF";
    public EventModifiers modifiers = new EventModifiers();
    public string rewardBadgeId = "";
}

[Serializable]
public class LiveConfigData
{
    public string configVersion = "1.0.0";
    public ActiveEventData activeEvent = new ActiveEventData();
}

public class LiveOpsConfigManager : MonoBehaviour
{
    public static LiveOpsConfigManager Instance { get; private set; }

    [Header("Remote Live Ops JSON Endpoint")]
    [SerializeField] private string remoteConfigUrl = "https://raw.githubusercontent.com/<your-username>/<your-repo>/main/live-config.json";
    [SerializeField] private string localFallbackFileName = "live-config.json";
    [SerializeField] private float requestTimeoutSeconds = 5f;

    public LiveConfigData CurrentConfig { get; private set; }
    public event Action<LiveConfigData> OnConfigLoaded;

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
        StartCoroutine(FetchLiveConfig());
    }

    public IEnumerator FetchLiveConfig()
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
            CurrentConfig = new LiveConfigData();
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }

    private void ApplyJsonConfig(string json, string source)
    {
        try
        {
            CurrentConfig = JsonUtility.FromJson<LiveConfigData>(json);

            // If isActive is false, reset modifiers to neutral 1.0x baseline
            if (CurrentConfig.activeEvent == null || !CurrentConfig.activeEvent.isActive)
            {
                if (CurrentConfig.activeEvent == null)
                    CurrentConfig.activeEvent = new ActiveEventData();
                CurrentConfig.activeEvent.modifiers = new EventModifiers();
            }

            Debug.Log($"[LiveOps] Loaded '{CurrentConfig.activeEvent.eventId}' (isActive={CurrentConfig.activeEvent.isActive}, v{CurrentConfig.configVersion}) from {source}.");
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LiveOps] JSON parse error: {ex.Message}");
            CurrentConfig = new LiveConfigData();
            OnConfigLoaded?.Invoke(CurrentConfig);
        }
    }
}
