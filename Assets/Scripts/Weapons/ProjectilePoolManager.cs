using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviourPun
{
    private Dictionary<string, int> _initialPoolSize = new Dictionary<string, int>()
    {
        { "Nerf Bullet", 15 },
    };

    private Dictionary<string, List<Bullet>> _freeBullets = new Dictionary<string, List<Bullet>>();
    private float lastChecked = 0;

    private Bullet CreateBullet(string type)
    {
        var bulletGameObject = PhotonNetwork.InstantiateSceneObject($"Bullets/{type}", new Vector3(0, 10000, 0), Quaternion.identity);
        var bulletScript = bulletGameObject.GetComponent<Bullet>();

        if (bulletScript == null)
        {
            Debug.LogError($"Error caching the bullet {type}, it does not have a Bullet script attached.");
            PhotonNetwork.Destroy(bulletGameObject);
            return null;
        }

        bulletGameObject.SetActive(false);

        return bulletScript;
    }

    private IEnumerable<Bullet> GetFreeBullets(string type, int number)
    {
        if (_freeBullets.TryGetValue(type, out var freeBulletsOfType))
        {
            while (number > 0 && _freeBullets.Count > 0)
            {
                var bullet = freeBulletsOfType[0];
                freeBulletsOfType.RemoveAt(0);
                yield return bullet;
            }
        }

        // just create new bullets if we run out of bullets in the cache
        for (int i = 0; i < number; i++)
        {
            yield return this.CreateBullet(type);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if(Time.time - lastChecked < 1)
        {
            return;
        }

        lastChecked = Time.time;
        
    }
}
