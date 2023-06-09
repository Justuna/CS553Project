using Godot;
using System;
using System.Reflection;

public partial class PNM_ENet : Node, PunchiesNetworkManager
{
    private ConnectionType _connection = ConnectionType.NOT_CONNECTED;
    private SceneController _sc;
    private GameController _game = null;

    private long _peer;

    public void Initialize(SceneController sc)
    {
        _sc = sc;
    }

    public PNMType GetManagerType()
    {
        return PNMType.ENet;
    }

    public void HostGame(string ip)
    {
        ENetMultiplayerPeer eNetPeer;
        if (GetTree().GetMultiplayer().MultiplayerPeer != null && GetTree().GetMultiplayer().MultiplayerPeer is ENetMultiplayerPeer)
        {
            eNetPeer = (ENetMultiplayerPeer) GetTree().GetMultiplayer().MultiplayerPeer;
        }
        else
        {
            eNetPeer = new ENetMultiplayerPeer();
        }
        
        eNetPeer.TransferMode = MultiplayerPeer.TransferModeEnum.Reliable;

        GetTree().GetMultiplayer().PeerConnected += StartGameAsHost;

        eNetPeer.SetBindIP(ip);
        Error error = eNetPeer.CreateServer(PunchiesNetworkManager.SERVER_PORT, PunchiesNetworkManager.MAX_CLIENTS);

        GetTree().GetMultiplayer().MultiplayerPeer = eNetPeer;

        if (error != Error.Ok)
        {
            throw new Exception("Could not establish server: " + error);
        }

        _connection = ConnectionType.HOST;
    }

    public void JoinGame(string ip)
    {
        ENetMultiplayerPeer eNetPeer;
        if (GetTree().GetMultiplayer().MultiplayerPeer != null && GetTree().GetMultiplayer().MultiplayerPeer is ENetMultiplayerPeer)
        {
            eNetPeer = (ENetMultiplayerPeer)GetTree().GetMultiplayer().MultiplayerPeer;
        }
        else
        {
            eNetPeer = new ENetMultiplayerPeer();
        }

        eNetPeer.TransferMode = MultiplayerPeer.TransferModeEnum.Reliable;

        GetTree().GetMultiplayer().ConnectedToServer += StartGameAsClient;

        Error error = eNetPeer.CreateClient(ip, PunchiesNetworkManager.SERVER_PORT);

        GetTree().GetMultiplayer().MultiplayerPeer = eNetPeer;

        if (error != Error.Ok)
        {
            throw new Exception("Could not establish client: " + error);
        }

        _connection = ConnectionType.CLIENT;
    }

    public void CancelGame()
    {
        GetTree().GetMultiplayer().MultiplayerPeer.Close();
        if (_connection == ConnectionType.CLIENT)
        {
            GetTree().GetMultiplayer().ConnectedToServer -= StartGameAsClient;
        }
        else if (_connection == ConnectionType.HOST)
        {
            GetTree().GetMultiplayer().PeerConnected -= StartGameAsHost;
        }

        _connection = ConnectionType.NOT_CONNECTED;
    }

    private void StartGameAsHost(long id)
    {
        GD.Print("Starting game as host...");
        _game = _sc.StartGameAsHost();

        _peer = id;
    }

    private void StartGameAsClient()
    {
        GD.Print("Starting game as client...");
        _game = _sc.StartGameAsClient();

        _peer = 1;
    }

    public void SendInput(int input)
    {
        RpcId(_peer, "ReceiveInput", input);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ReceiveInput(int input)
    {
        _game.QueueNetworkInput(input);
    }
}
