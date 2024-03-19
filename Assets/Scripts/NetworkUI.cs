using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Class <c>NetworkUI</c> provides UI instance-starting logic.
/// This class heavily references code from <see href="https://www.youtube.com/watch?v=swIM2z6Foxk"/>.
/// </summary>
public class NetworkUI : MonoBehaviour
{
    /// <summary>
    /// Method <c>StartServer</c> starts a server instance.
    /// </summary>
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    /// <summary>
    /// Method <c>StartClient</c> starts a client instance.
    /// </summary>
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    /// <summary>
    /// Method <c>StartHost</c> starts a host instance.
    /// </summary>
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
