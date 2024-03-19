using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Class <c>ConnectionApprovalHandler</c> provides control logic to limit new client connections.
/// This class heavily references code from <see href="https://www.youtube.com/watch?v=swIM2z6Foxk"/>.
/// </summary>
public class ConnectionApprovalHandler : MonoBehaviour
{
    private const int MaxPlayers = 4;

    /// <summary>
    /// Method <c>Start</c> subscribes the handler to ApprovalCheck events.
    /// </summary>
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
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
        response.PlayerPrefabHash = null;
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
        {
            response.Approved = false;
            response.Reason = "Player limit reached.";
        } else
        {
            // handle prefab assignment in here!
        }
        response.Pending = false;
    }
}
