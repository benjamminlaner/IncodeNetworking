﻿using UnityEngine;
using Unity.Networking.Transport;
using Incode.Networking;

public interface IGameLoop
{
    bool Init(string[] args);
    void Update();
    void ShutDown();
    void SendTest();
    IGameLoop WithPlayer(GameObject playerGO);
    void OnPlayerCommand(PlayerCommand cmd);
    void OnReceiveSnapshot(PlayerCommand cmd);
}

public class GameTime
{
    public int TickRate
    {
        get { return this.tickRate; }
        set
        {
            this.tickRate = value;
            this.tickInterval = 1.0f / this.tickRate;
        }
    }

    public float tickInterval { get; private set; }
    public int tick;
    public float tickDuration;

    public GameTime(int tickRate)
    {
        this.tickRate = tickRate;
        this.tickInterval = 1.0f / tickRate;
        this.tick = 1;
        this.tickDuration = 0;
    }

    private int tickRate;
}

public class Game : MonoBehaviour
{

    public static double frameTime;
    public static Game game;
    public bool RunServer = false;


    public GameObject localPlayer;
    public GameObject networkPlayer;


    private ServerGameLoop serverLoop;
    private ClientGameLoop clientLoop;

    public void Awake()
    {
        this.clockFrequency = System.Diagnostics.Stopwatch.Frequency;
        this.clock = new System.Diagnostics.Stopwatch();
        this.clock.Start();

#if UNITY_SERVER
        this.RunServer = true;
#endif
        // StartServerGame();
        // StartClientGame();
    }

    void OnValidate()
    {
        Unity.Collections.NativeLeakDetection.Mode = Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace;
    }

    public void Update()
    {
        if (this == null) { return; }

        frameTime = (double)this.clock.ElapsedTicks / this.clockFrequency;

        if (this.serverLoop == null && this.RunServer)
        {
            this.StartServerGame();
        }

        if (this.serverLoop != null)
        {
            this.serverLoop.Update();
        }

        if (this.clientLoop != null)
        {
            this.clientLoop.Update();
        }

    }

    public void OnDestroy()
    {
        if (serverLoop != null)
        {
            serverLoop.ShutDown();
        }


        if (clientLoop != null)
        {
            clientLoop.ShutDown();
        }

    }

    private void OnApplicationQuit()
    {
        if (clientLoop != null)
        {
            clientLoop.ShutDown();
        }

        if (serverLoop != null)
        {
            serverLoop.ShutDown();
        }
    }

    public void StartClientGame()
    {
        if (clientLoop == null)
        {
            clientLoop = new ClientGameLoop() as ClientGameLoop;
            clientLoop.localPlayerGO = this.localPlayer;
            clientLoop.networkPlayerGO = this.networkPlayer;
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

    public void StartServerGame()
    {

        RunServer = true;

        if (RunServer)
        {
            serverLoop = new ServerGameLoop().WithPlayer(networkPlayer) as ServerGameLoop;
            serverLoop.Init(null);
        }
    }

    private System.Diagnostics.Stopwatch clock;
    private long clockFrequency;
}