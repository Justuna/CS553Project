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
    }

    public PNMType GetManagerType()
    {
        return PNMType.GameNetworkingSockets;
    }

    public void HostGame(string ip)
    {
        NetworkingSockets server = new NetworkingSockets();
        _self = server;

        StatusCallback statusCallback = ServerCallback;
        _utils.SetStatusCallback(statusCallback);

        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        Address test = new Address();

        _listenSocket = server.CreateListenSocket(ref address);
        
        _self.GetListenSocketAddress(_listenSocket, ref test);

        _connection = ConnectionType.HOST;
    }

    private void ServerCallback(ref StatusInfo status) {
        switch (status.connectionInfo.state)
            {
                case ConnectionState.Connecting:
                    _peer = status.connection;
                    _self.AcceptConnection(_peer);
                    break;
                case ConnectionState.Connected:
                    StartGameAsHost();
                    break;
                default:
                    break;
            }
    }

    private void StartGameAsHost()
    {
        _game = _sc.StartGameAsHost();
    }


    public void JoinGame(string ip)
    {
        NetworkingSockets client = new NetworkingSockets();
        _self = client;

        StatusCallback statusCallback = ClientCallback;
        _utils.SetStatusCallback(statusCallback);

        Address address = new Address();
        address.SetAddress(ip, PunchiesNetworkManager.SERVER_PORT);

        _peer = client.Connect(ref address);

        _connection = ConnectionType.CLIENT;
    }

    private void ClientCallback (ref StatusInfo status)
    {
        switch (status.connectionInfo.state)
        {
            case ConnectionState.Connected:
                StartGameAsClient();
                break;
            default:
                break;
        }
    }

    private void StartGameAsClient()
    {
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
        _self.SendMessageToConnection(_peer, bytes, SendFlags.NoNagle);
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
