using Godot;
using System;
using Valve.Sockets;

public partial class PNM_GameNetworkingSockets : Node, PunchiesNetworkManager
{
    private ConnectionType _connection = ConnectionType.NOT_CONNECTED;
    private NetworkingUtils _utils;
    private SceneController _sc;
    private GameController _game;
    private NetworkingSockets _self;
    private uint _peer;

    public void Initialize(SceneController sc)
    {
        _sc = sc;
        Library.Initialize();

        _utils = new NetworkingUtils();
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
        StatusCallback statusCallback = (ref StatusInfo status) =>
        {
            switch (status.connectionInfo.state)
            {
                case ConnectionState.Connecting:
                    _peer = status.connection;
                    server.AcceptConnection(_peer);
                    GD.Print("Connected to peer on port " + status.connectionInfo.listenSocket + " at " + status.connectionInfo.address);
                    break;
                case ConnectionState.Connected:
                    StartGameAsHost();
                    break;
                default:
                    break;
            }
        };

        _utils.SetStatusCallback(statusCallback);

        GD.Print("Binding to address");
        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        GD.Print("Creating listening socket on address " + address.GetIP());
        uint listenSocket = server.CreateListenSocket(ref address);

        GD.Print("Done setting up");
        _connection = ConnectionType.HOST;
    }

    private void StartGameAsHost()
    {
        GD.Print("Starting game as host...");
        _game = _sc.StartGameAsHost();
    }


    public void JoinGame(string ip)
    {
        GD.Print("Starting client");
        NetworkingSockets client = new NetworkingSockets();
        _self = client;

        GD.Print("Setting up callback");
        StatusCallback statusCallback = (ref StatusInfo status) =>
        {
            switch (status.connectionInfo.state)
            {
                case ConnectionState.Connected:
                    GD.Print("Connected to peer on port " + status.connectionInfo.listenSocket + " at " + status.connectionInfo.address);
                    StartGameAsClient();
                    break;
                default:
                    break;
            }
        };

        _utils.SetStatusCallback(statusCallback);

        GD.Print("Binding to address");
        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        GD.Print("Connecting");
        _peer = client.Connect(ref address);

        GD.Print("Done setting up");
        _connection = ConnectionType.CLIENT;
    }

    private void StartGameAsClient()
    {
        GD.Print("Starting game as client...");
        _game = _sc.StartGameAsClient();
    }

    public void CancelGame()
    {
        _self = null;
        _utils.SetStatusCallback(null);
        _connection = ConnectionType.NOT_CONNECTED;
    }

    public override void _Process(double delta)
    {
        if (_self != null)
        {
            _self.RunCallbacks();

            _self.ReceiveMessagesOnConnection(_peer, (in NetworkingMessage message) =>
            {
                message.CopyTo(_intBuffer);
                ReceiveInput(BitConverter.ToInt32(_intBuffer));
            }, 32);
        }
    }

    byte[] _intBuffer = new byte[4];

    public void SendInput(int input)
    {
        byte[] bytes = BitConverter.GetBytes(input);
        _self.SendMessageToConnection(_peer, bytes);
    }

    private void ReceiveInput(int input)
    {
        _game.QueueNetworkInput(input);
    }
}
