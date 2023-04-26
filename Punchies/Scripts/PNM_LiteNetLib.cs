using Godot;
using System;
using LiteNetLib;
using LiteNetLib.Utils;

public partial class PNM_LiteNetLib : Node, PunchiesNetworkManager
{
    private ConnectionType _connection = ConnectionType.NOT_CONNECTED;
    private SceneController _sc;
    private GameController _game;

    private EventBasedNetListener _listener;
    private NetManager _netManager;
    private NetPeer _peer;

    private const string NETWORK_KEY = "california_girls";

    public void Initialize(SceneController sc)
    {
        _sc = sc;
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener);
    }

    public override void _Process(double delta)
    {
        if (_netManager != null) _netManager.PollEvents();
    }

    public PNMType GetManagerType()
    {
        return PNMType.LiteNetLib;
    }

    public void HostGame()
    {
        _netManager.Start(PunchiesNetworkManager.SERVER_PORT);

        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnClientConnected;

        _connection = ConnectionType.HOST;
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (_netManager.ConnectedPeersCount < PunchiesNetworkManager.MAX_CLIENTS)
        {
            request.AcceptIfKey(NETWORK_KEY);
            GD.Print("Accepted peering request");
        }
        else
        {
            request.Reject();
            GD.Print("Rejected peering request");
        }
    }

    private void OnClientConnected(NetPeer peer)
    {
        GD.Print("Connected to peer on port " + peer.EndPoint.Port + " at " + peer.EndPoint.Address);
        _game = _sc.StartGameAsHost();

        BeforeGameStart(peer);
    }

    public void JoinGame(string ip)
    {
        _netManager.Start();
        _netManager.Connect(ip, PunchiesNetworkManager.SERVER_PORT, NETWORK_KEY);

        _listener.PeerConnectedEvent += OnConnectToServer;

        _connection = ConnectionType.CLIENT;
    }

    private void OnConnectToServer(NetPeer peer)
    {
        GD.Print("Connected to server on port " + peer.EndPoint.Port + " at " + peer.EndPoint.Address);
        _game = _sc.StartGameAsClient();

        BeforeGameStart(peer);
    }

    private void BeforeGameStart(NetPeer peer)
    {
        _peer = peer;

        _listener.NetworkReceiveEvent += ReceiveInput;
    }

    public void CancelGame()
    {
        _netManager.Stop();
        if (_connection == ConnectionType.CLIENT)
        {
            _listener.PeerConnectedEvent -= OnConnectToServer;
        }
        else if (_connection == ConnectionType.HOST)
        {
            _listener.ConnectionRequestEvent -= OnConnectionRequest;
            _listener.PeerConnectedEvent -= OnClientConnected;
        }

        _connection = ConnectionType.NOT_CONNECTED;
    }

    public void SendInput(int input)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(input);       
        _peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void ReceiveInput(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int input = reader.GetInt();
        _game.QueueNetworkInput(input);
        GD.Print(_connection + " received: " + input);
    }
}
