using Godot;
using System;
using Valve.Sockets;

public partial class PNM_GameNetworkingSockets : Node, PunchiesNetworkManager
{
    private const int MAX_MESSAGES = 20;
    private ConnectionType _connection = ConnectionType.NOT_CONNECTED;
    private NetworkingUtils _utils;
    private SceneController _sc;
    private GameController _game;
    private NetworkingSockets _self;
    private NetworkingMessage[] _netMessages = new NetworkingMessage[MAX_MESSAGES];
    private uint _peer;
    private uint _listenSocket;

    public void Initialize(SceneController sc)
    {
        _sc = sc;
        Library.Initialize();

        _utils = new NetworkingUtils();

        DebugCallback debugCallback = (DebugType verbosity, string message) => {
            GD.PrintErr("GameNetworkingSockets " + _connection + " says: " + message);
        };

        _utils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    public PNMType GetManagerType()
    {
        return PNMType.GameNetworkingSockets;
    }

    public void HostGame(string ip)
    {
        GD.Print("Starting server");
        NetworkingSockets server = new NetworkingSockets();
        _self = server;

        GD.Print("Setting up callback");
        StatusCallback statusCallback = ServerCallback;
        _utils.SetStatusCallback(statusCallback);

        GD.Print("Binding to address");
        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        Address test = new Address();

        _listenSocket = server.CreateListenSocket(ref address);
        
        _self.GetListenSocketAddress(_listenSocket, ref test);
        GD.Print("Creating listening socket on address " + test.GetIP() + ":" + test.port);

        GD.Print("Done setting up");
        _connection = ConnectionType.HOST;
    }

    private void ServerCallback(ref StatusInfo status) {
        GD.Print("Identity: " + status.connectionInfo.identity);
        GD.Print("User data: " + status.connectionInfo.userData);
        GD.Print("Listening socket: " + ((ushort)status.connectionInfo.listenSocket));
        GD.Print("Address: " + status.connectionInfo.address.ip + " " + status.connectionInfo.address.port);
        GD.Print("State: " + status.connectionInfo.state);
        GD.Print("End reason: " + status.connectionInfo.endReason);
        GD.Print("End reason: " + status.connectionInfo.endDebug);
        GD.Print("Desc: " + status.connectionInfo.connectionDescription);
        switch (status.connectionInfo.state)
            {
                case ConnectionState.Connecting:
                    _peer = status.connection;
                    _self.AcceptConnection(_peer);
                    GD.Print("Connected to peer on port " + status.connectionInfo.listenSocket + " at " + status.connectionInfo.address);
                    break;
                case ConnectionState.Connected:
                    StartGameAsHost();
                    break;
                default:
                    GD.Print("Host state: " + status.connectionInfo.state);
                    break;
            }
    }

    private void StartGameAsHost()
    {
        GD.Print("Starting game as host...");
        _game = _sc.StartGameAsHost();
    }


    public void JoinGame(string ip)
    {
        //GD.Print("Starting client");
        NetworkingSockets client = new NetworkingSockets();
        _self = client;

        //GD.Print("Setting up callback");
        StatusCallback statusCallback = ClientCallback;
        _utils.SetStatusCallback(statusCallback);

        //GD.Print("Binding to address");
        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        GD.Print("Connecting");

        _peer = client.Connect(ref address);

        //GD.Print("Done setting up");
        _connection = ConnectionType.CLIENT;
    }

    private void ClientCallback (ref StatusInfo status)
    {
        GD.Print("Identity: " + status.connectionInfo.identity);
        GD.Print("User data: " + status.connectionInfo.userData);
        GD.Print("Listening socket: " + ((ushort)status.connectionInfo.listenSocket));
        GD.Print("Address: " + status.connectionInfo.address.ip + " " + status.connectionInfo.address.port);
        GD.Print("State: " + status.connectionInfo.state);
        GD.Print("End reason: " + status.connectionInfo.endReason);
        GD.Print("End reason: " + status.connectionInfo.endDebug);
        GD.Print("Desc: " + status.connectionInfo.connectionDescription);
        switch (status.connectionInfo.state)
        {
            case ConnectionState.Connected:
                GD.Print("Connected to peer on port " + status.connectionInfo.listenSocket + " at " + status.connectionInfo.address);
                StartGameAsClient();
                break;
            default:
                GD.Print("Client state: " + status.connectionInfo.state);
                break;
        }
    }

    private void StartGameAsClient()
    {
        GD.Print("Starting game as client...");
        _game = _sc.StartGameAsClient();
    }

    public void CancelGame()
    {
        _self.CloseConnection(_peer);
        if (_connection == ConnectionType.HOST) {
            _self.CloseListenSocket(_listenSocket);
        }
        
        _self = null;
        _utils.SetStatusCallback(null);
        _connection = ConnectionType.NOT_CONNECTED;
    }

    public override void _Process(double delta)
    {
        if (_self != null)
        {
            _self.RunCallbacks();

            int netMessagesCount = _self.ReceiveMessagesOnConnection(_peer, _netMessages, MAX_MESSAGES);

            if (netMessagesCount > 0)
            {
                for (int i = 0; i < netMessagesCount; i++)
                {
                    ref NetworkingMessage message = ref _netMessages[i];
                    message.CopyTo(_intBuffer);
                    ReceiveInput(BitConverter.ToInt32(_intBuffer));
                }
            }
        }
    }

    byte[] _intBuffer = new byte[4];

    public void SendInput(int input)
    {
        byte[] bytes = BitConverter.GetBytes(input);
        _self.SendMessageToConnection(_peer, bytes, SendFlags.Reliable);
    }

    private void ReceiveInput(int input)
    {
        _game.QueueNetworkInput(input);
    }

    public override void _ExitTree()
    {
        Library.Deinitialize();
        base._ExitTree();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest) {
            Library.Deinitialize();
        }
        base._Notification(what);
    }
}
