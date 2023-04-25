using Godot;
using System;

public partial class ClientServer : Node
{
    public const int SERVER_PORT = 42069;
    public const string SERVER_ADDRESS = "localhost";
    public const int MAX_CLIENTS = 2;

    [Export]
    private Button _serverButton;
    [Export]
    private Button _clientButton;
    [Export]
    private Button _messageButton;
    [Export]
    private TextEdit _messageTextBox;

    private long _peer;

    public override void _Ready()
    {
        _serverButton.Pressed += () => Initialize(true);
        _clientButton.Pressed += () => Initialize(false);
    }

    public void Initialize(bool isServer)
    {
        if (isServer)
        {
            ENetMultiplayerPeer eNetPeer = new ENetMultiplayerPeer();
            eNetPeer.TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered;

            eNetPeer.PeerConnected += HandleConnection;
            eNetPeer.PeerDisconnected += HandleDisconnect;

            Error error = eNetPeer.CreateServer(SERVER_PORT, MAX_CLIENTS);

            GetTree().GetMultiplayer().MultiplayerPeer = eNetPeer;

            switch (error)
            {
                case Error.Ok:
                    GD.Print("Server established");
                    break;
                case Error.AlreadyInUse:
                    GD.Print("Peer connection already in use");
                    break;
                case Error.CantConnect:
                    GD.Print("Could not create server");
                    break;
            }

            GD.Print("Unique ID: " + eNetPeer.GetUniqueId());
        }
        else
        {
            ENetMultiplayerPeer eNetPeer = new ENetMultiplayerPeer();
            eNetPeer.TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered;

            GetTree().GetMultiplayer().ConnectedToServer += ConnectionEstablished;

            Error error = eNetPeer.CreateClient(SERVER_ADDRESS, SERVER_PORT);

            GetTree().GetMultiplayer().MultiplayerPeer = eNetPeer;

            switch (error)
            {
                case Error.Ok:
                    GD.Print("Client established");
                    break;
                case Error.AlreadyInUse:
                    GD.Print("Peer connection already in use");
                    break;
                case Error.CantConnect:
                    GD.Print("Could not create client");
                    break;
            }
            GD.Print(error);
        }

        _serverButton.Visible = false;
        _clientButton.Visible = false;
        _messageButton.Visible = true;
        _messageButton.Pressed += SendMessage;
        _messageTextBox.Visible = true;
    }

    public void HandleConnection(long id)
    {
        try
        {
            ENetMultiplayerPeer eNetPeer = (ENetMultiplayerPeer)GetTree().GetMultiplayer().MultiplayerPeer;
            ENetPacketPeer client = eNetPeer.GetPeer((int)id);
            GD.Print("New connection established by a client on port " + client.GetRemotePort() + " at " + client.GetRemoteAddress());
            GD.Print(client.GetChannels() + " channels available");
        }
        catch (InvalidCastException e)
        {
            GD.Print(e.Message);
        }

        _peer = id;
    }

    public void ConnectionEstablished()
    {
        GD.Print("Connected to server on port " + SERVER_PORT + " at " + SERVER_ADDRESS);
        ENetMultiplayerPeer eNetPeer = (ENetMultiplayerPeer)GetTree().GetMultiplayer().MultiplayerPeer;
        GD.Print("Unique ID: " + eNetPeer.GetUniqueId());
        GD.Print(eNetPeer.GetPeer(1).GetChannels() + " channels available");

        _peer = 1;
    }

    public void HandleDisconnect(long id)
    {
        try
        {
            ENetMultiplayerPeer eNetPeer = (ENetMultiplayerPeer)GetTree().GetMultiplayer().MultiplayerPeer;
            ENetPacketPeer client = eNetPeer.GetPeer((int)id);
            GD.Print("Client on port " + client.GetRemotePort() + " at " + client.GetRemoteAddress() + " disconnected.");
        }
        catch (InvalidCastException e)
        {
            GD.Print(e.Message);
        }
    }

    public void SendMessage()
    {
        string msg = _messageTextBox.Text.Trim();
        if (msg.Length > 0)
        {
            _messageTextBox.Text = "";
            RpcId(_peer, "ReceiveMessage", msg);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ReceiveMessage(string msg)
    {
        GD.Print("Message received: \"" + msg + "\"");
    }

}
