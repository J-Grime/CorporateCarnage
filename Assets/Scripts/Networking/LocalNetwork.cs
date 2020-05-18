using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LocalNetwork : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom("local-test", new RoomOptions()
            {
                MaxPlayers = 1,
            });
        }

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
