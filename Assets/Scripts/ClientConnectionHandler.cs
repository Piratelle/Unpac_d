using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.Requests;

/// <summary>
/// Class <c>ClientConnectionHandler</c> provides control logic to limit new client connections.
/// This class heavily references code from <see href="https://www.youtube.com/watch?v=swIM2z6Foxk"/>.
/// </summary>
public class ClientConnectionHandler : Singleton<ClientConnectionHandler>
{
    [SerializeField] private List<uint> playerPrefabs;
    [SerializeField] private Game game;
    private readonly SortedSet<int> availablePlayers = new();
    private readonly Dictionary<ulong, int> clientPlayers = new();

    /// <summary>
    /// Method <c>Start</c> subscribes the handler to ApprovalCheck events.
    /// </summary>
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += DisconnectPlayer;

        // Build our list of available players
        for (int i = 0; i < playerPrefabs.Count; i++)
        {
            availablePlayers.Add(i);
        }
    }

    /// <summary>
    /// Method <c>ApprovalCheck</c> approves client connection only if there are available client slots.
    /// </summary>
    /// <param name="request">the client connection request.</param>
    /// <param name="response">the handler's response to the request.</param>
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Validating connection...");
        response.Approved = true;
        response.CreatePlayerObject = true;
        //response.PlayerPrefabHash = null;
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers())
        {
            response.Approved = false;
            response.Reason = "Player limit reached.";
            Debug.Log("Connection from Client " + request.ClientNetworkId + " refused as game is full.");
        } else if (!game.IsGameOver.Value)
        {
            response.Approved= false;
            response.Reason = "Game in progress, new connections disabled.";
            Debug.Log("Connection from Client " + request.ClientNetworkId + " refused as game is in progress.");
        } else
        {
            int playerNum = availablePlayers.First();
            availablePlayers.Remove(playerNum);
            response.PlayerPrefabHash = playerPrefabs[playerNum];
            clientPlayers.Add(request.ClientNetworkId, playerNum);
            Debug.Log("Connection from Client " + request.ClientNetworkId + " approved as Player " + (playerNum + 1));
            Debug.Log("Currently available: " + GetAvailable());
        }
        response.Pending = false;
    }

    /// <summary>
    /// Method <c>DisconnectPlayer</c> frees up the player slot for a disconnected client.
    /// </summary>
    /// <param name="clientId">the network ID of the client that is disconnecting.</param>
    private void DisconnectPlayer(ulong clientId)
    {
        if (clientPlayers.Count == 0) return; // connections are only tracked on the server!

        int playerNum = clientPlayers[clientId];
        clientPlayers.Remove(clientId);
        availablePlayers.Add(playerNum);
        Debug.Log("Client " + clientId + " disconnected from Player " + playerNum + ", now available: " + GetAvailable());
    }

    /// <summary>
    /// Method <c>GetAvailable</c> acts as a ToString() accessor for the list of currently available player slots.
    /// </summary>
    /// <returns>a string representation of the currently available player slots.</returns>
    private string GetAvailable()
    {
        string available = "[";
        foreach (int player in availablePlayers)
        {
            available += " " + (player + 1);
        }
        available += " ]";
        return available;
    }

    /// <summary>
    /// Method <c>MaxPlayers</c> acts as an accessor for maximum number of players permitted.
    /// </summary>
    /// <returns>the maximum number of players permitted.</returns>
    public int MaxPlayers()
    {
        return playerPrefabs.Count;
    }
}
