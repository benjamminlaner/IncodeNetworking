using UnityEngine;
using Unity.Networking.Transport;
using Incode.Networking;

public interface IGameLoop
{
    bool Init(string[] args);
    void Update();
    void ShutDown();
    void SendTest();
    void SetPlayer(GameObject playerGO);
    void OnPlayerCommand(PlayerCommand cmd);
    void OnReceiveSnapshot(PlayerCommand cmd);
}

public class GameTime
{
    public int TickRate
    {
        get { return tickRate; }
        set
        {
            tickRate = value;
            tickInterval = 1.0f / tickRate;
        }
    }

    public float tickInterval { get; private set; }
    public int tick;
    public float tickDuration;

    public GameTime(int _tickRate)
    {
        tickRate = _tickRate;
        tickInterval = 1.0f / tickRate;
        tick = 1;
        tickDuration = 0;
    }

    private int tickRate;
}

public class Game : MonoBehaviour
{
    public static double frameTime;
    public static Game game;
    public bool RunServerOnAwake = false;


    public GameObject localPlayer;
    public GameObject networkPlayer;


    private ServerGameLoop serverLoop;
    private ClientGameLoop clientLoop;

    private System.Diagnostics.Stopwatch clock;
    private long clockFrequency;

    public void Awake()
    {
        clockFrequency = System.Diagnostics.Stopwatch.Frequency;
        clock = new System.Diagnostics.Stopwatch();
        clock.Start();

#if UNITY_SERVER
        StartServerGame();
#endif
    }

    void OnValidate()
    {
        Unity.Collections.NativeLeakDetection.Mode = Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace;
    }

    public void Update()
    {
        if (this == null) { return; }

        frameTime = (double)clock.ElapsedTicks / clockFrequency;

        if (serverLoop == null && RunServerOnAwake)
        {
            StartServerGame();
        }

        if (serverLoop != null)
        {
            serverLoop.Update();
        }

        if (clientLoop != null)
        {
            clientLoop.Update();
        }

    }

    public void OnDestroy()
    {
        if (serverLoop != null)
        {
            serverLoop.ShutDown();
            serverLoop = null;
        }


        if (clientLoop != null)
        {
            clientLoop.ShutDown();
            clientLoop = null;
        }

    }

    private void OnApplicationQuit()
    {
        if (serverLoop != null)
        {
            serverLoop.ShutDown();
            serverLoop = null;
        }

        if (clientLoop != null)
        {
            clientLoop.ShutDown();
            clientLoop = null;
        }
    }

    public void StartServerGame()
    {
        serverLoop = new ServerGameLoop();
        serverLoop.SetPlayer(networkPlayer);
        serverLoop.Init(null);
    }

    public void StartClientGame()
    {
        if (clientLoop == null)
        {
            clientLoop = new ClientGameLoop() as ClientGameLoop;
            clientLoop.localPlayerGO = localPlayer;
            clientLoop.networkPlayerGO = networkPlayer;
            clientLoop.Init(null);
        }
        else
        {
            Debug.Log("Client Game Already Started");
        }
    }

    public void SendTestData()
    {
        if (clientLoop != null)
        {
            clientLoop.SendTest();
        }
    }

    public void SendPing()
    {
        long pingTime = NetworkUtils.PingServer("localhost");
        Debug.Log($"Current Ping: {pingTime}ms");
    }
}