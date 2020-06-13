using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class ClientGameLoop : IGameLoop, INetworkCallbacks
{
    // private SocketTransport networkTransport;
    private NetworkClient networkClient;
    private ClientState clientState;
    private NetworkDriver driver;
    private int connectRetryCount;
    private string gameMessage;
    private string targetServer = "127.0.0.1";

    private float nextSendTime = Time.time + 0.02f;
    //private static int lastCommandId = 0;

    private DateTime attemptedConnectionTime;

    //private PlayerClient localPlayer;
    private Dictionary<int, GameObject> currentPlayers = new Dictionary<int, GameObject>();

    private int localPlayerId;

    private GameTime predictionTime;

    public GameObject localPlayerGO;
    public GameObject localPlayer;
    public GameObject networkPlayerGO;

    public int movementSpeed = 5;

    public float frameTimeScale = 1.0f;

    public bool Init(string[] args)
    {
        localPlayer = this.localPlayerGO;
        Debug.Log("Starting Client Init...");
        //this.localPlayer = new PlayerClient().AsLocalPlayer();
        stateMachine = new StateMachine<ClientState>();
        // m_StateMachine.Add(ClientState.Browsing,    EnterBrowsingState,     UpdateBrowsingState,    LeaveBrowsingState);
        stateMachine.Add(ClientState.Connecting, EnterConnectingState, UpdateConnectingState, null);
        stateMachine.Add(ClientState.Connected, EnterConnectedState, UpdateConnectedState, null);
        // this.networkTransport = new SocketTransport();
        driver = NetworkDriver.Create(new INetworkParameter[0]);
        networkClient = new NetworkClient(this.driver);

        stateMachine.SwitchTo(ClientState.Connecting);

        Debug.Log("Client initialized");

        return true;
    }


    public void SetPlayer(GameObject playerGO)
    {
        throw new NotImplementedException();
    }

    public void OnConnect(int id)
    {
        Debug.Log("New Player Connected: " + id);


        GameObject player;
        currentPlayers.TryGetValue(id, out player);

        if (player != null) { return; }

        player = networkPlayerGO;
        currentPlayers.Add(id, player);
    }

    public void OnConnectionAck(PlayerCommand cmd)
    {
        predictionTime = new GameTime(60);

        Debug.Log("(Client) Connection Acknowledged. PlayerID: " + cmd.PlayerID);
        localPlayerId = cmd.PlayerID;

        currentPlayers.Add(cmd.PlayerID, localPlayer);

        foreach (int playerId in cmd.currentPlayers)
        {
            GameObject player;
            currentPlayers.TryGetValue(playerId, out player);

            if (player != null) { continue; }

            player = networkPlayerGO;
            currentPlayers.Add(playerId, player);
        }

        PlayerCommand moveCommand = new PlayerCommand()
                                      .OfType(PlayerCommandType.Move)
                                      .WithPlayerId(localPlayerId);

        moveCommand.startingPosition = Vector3.zero;
        moveCommand.endingPosition = localPlayer.transform.localPosition;
        moveCommand.startingRotation = Quaternion.identity;
        moveCommand.endingRotation = localPlayer.transform.localRotation;
        QueueCommand(moveCommand);

        Debug.Log($"Current Players: {currentPlayers}");
    }

    public void OnDisconnect(int id)
    {

        GameObject player;
        currentPlayers.TryGetValue(id, out player);

        if (player == null) { return; }

        UnityEngine.Object.Destroy(player);
        currentPlayers.Remove(id);
    }

    public void OnPlayerCommand(PlayerCommand cmd) { }

    public void OnReceiveSnapshot(PlayerCommand cmd)
    {
        if (cmd.PlayerID == localPlayerId) { return; }

        GameObject player;
        currentPlayers.TryGetValue(cmd.PlayerID, out player);

        if (player == null) { return; }

        player.transform.SetPositionAndRotation(cmd.currentPosition, cmd.currentRotation);
    }

    public void SendTest()
    {
        //this.networkClient.SendTestData();

        PlayerCommand testCommand = new PlayerCommand()
                                        .OfType(PlayerCommandType.Move)
                                        .WithPlayerId(localPlayerId);
        //testCommand.startingPosition = Vector3.zero;
        testCommand.endingPosition = Vector3.right;
        testCommand.endingRotation = Quaternion.Euler(0f, 90f, 0f);

        Debug.LogFormat($"Sending Position. X: {0}, Y: {1}, Z: {2}", testCommand.endingPosition.x, testCommand.endingPosition.y, testCommand.endingPosition.z);
        Debug.LogFormat($"Sending Rotation. X: {0}, Y: {1}, Z: {2}, W: {3}", testCommand.endingRotation.x, testCommand.endingRotation.y, testCommand.endingRotation.z, testCommand.endingRotation.w);
        QueueCommand(testCommand);
    }

    public void ShutDown()
    {
        // this.networkTransport.Shutdown();
        networkClient.Disconnect();
    }

    public void Update()
    {
        if (this.clientState == ClientState.Connected)
        {
            Vector3 startingPosition = localPlayer.transform.localPosition;
            Quaternion startingRotation = localPlayer.transform.localRotation;

            PlayerCommand moveCommand = new PlayerCommand()
                                        .OfType(PlayerCommandType.Move)
                                        .WithPlayerId(localPlayerId);
            moveCommand.startingPosition = startingPosition;
            moveCommand.endingPosition = localPlayer.transform.localPosition;
            moveCommand.startingRotation = startingRotation;
            moveCommand.endingRotation = localPlayer.transform.localRotation;
            QueueCommand(moveCommand);


            if (Time.time >= nextSendTime && commandQueue.Count > 0)
            {
                Debug.Log("Sending Queued Commands...");
                networkClient.SendQueuedCommands(ref commandQueue);
                nextSendTime = Time.time + 0.02f;
            }
        }

        DebuggerController.playerCount = currentPlayers.Count;

        networkClient.Update(this);
        stateMachine.Update();
    }

    private void EnterConnectingState()
    {
        attemptedConnectionTime = DateTime.UtcNow;
        clientState = ClientState.Connecting;
        connectRetryCount = 0;
    }

    void UpdateConnectingState()
    {
        switch (networkClient.ConnectionState)
        {
            case NetworkConnection.State.Connected:
                gameMessage = "Client Connected";
                stateMachine.SwitchTo(ClientState.Connected);
                break;
            case NetworkConnection.State.Connecting:
                gameMessage = "Connecting...";
                if (attemptedConnectionTime.AddSeconds(20) < DateTime.UtcNow)
                {
                    Debug.Log("Connection Timed Out after 20s");
                }
                Debug.Log(gameMessage);
                break;
            case NetworkConnection.State.Disconnected:
                if (connectRetryCount < 2)
                {
                    //TODO: Connect to random player, from available
                    // targetServer = driver.RemoteEndPoint(networkClient.Connection).Address;
                    connectRetryCount++;
                    targetServer = "192.168.1.123";
                    gameMessage = string.Format("Trying to connect to {0} (attempt #{1})...", this.targetServer, connectRetryCount);
                    Debug.Log(gameMessage);
                    networkClient.Connect(targetServer);
                }
                else
                {
                    gameMessage = "Failed to connect to server";
                    Debug.Log(gameMessage);
                    networkClient.Disconnect();
                }
                break;
        }
    }

    private void EnterConnectedState()
    {
        clientState = ClientState.Connected;
        Debug.Log("Entered Connected State");
    }

    private void UpdateConnectedState() { }

    private void CheckConnection()
    {

    }

    private enum ClientState
    {
        Browsing,
        Connecting,
        Connected,
        Loading,
        Playing,
    }
    private StateMachine<ClientState> stateMachine;


    public void QueueCommand(PlayerCommand cmd)
    {
        commandQueue.Enqueue(cmd);
    }

    private List<PlayerCommand> commands = new List<PlayerCommand>();
    private Queue<PlayerCommand> commandQueue = new Queue<PlayerCommand>();

}
