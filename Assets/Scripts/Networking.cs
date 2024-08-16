using Unity.Netcode;
using UnityEngine;

public class Networking : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    public void OnHost()
    {
        networkManager.StartHost();
    }
    
    public void OnJoin()
    {
        networkManager.StartClient();
    }
}
