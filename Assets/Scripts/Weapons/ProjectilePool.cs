using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviourPun
{
    private static ProjectilePool _instance;

    public static ProjectilePool Instance => _instance;

    private Dictionary<string, List<Bullet>> _freeBullets = new Dictionary<string, List<Bullet>>();
    private Dictionary<string, List<Bullet>> _usedBullets = new Dictionary<string, List<Bullet>>();

    private Dictionary<string, int> _synchedFreeBulletCounts = new Dictionary<string, int>();

    private bool _hasPoolChanged = false;
    private PhotonView _photonView;

    public List<string> ProjectileTypesUsed { get; private set; } = new List<string>();

    internal Bullet GetFreeBullet(string type)
        => _freeBullets[type][_freeBullets[type].Count - 1];

    internal void PoolBullet(Bullet bullet)
    {
        if (!_photonView.IsMine) return;

        if (!_freeBullets.TryGetValue(bullet.ResourceName, out var bulletList))
        {
            bulletList = new List<Bullet>();
            _freeBullets.Add(bullet.ResourceName, bulletList);

            this.ProjectileTypesUsed.Add(bullet.ResourceName);

            _photonView.RPC(nameof(this.SetProjectileTypesUsedRPC), RpcTarget.MasterClient, this.ProjectileTypesUsed.ToArray());
        }

        // If the bullet is already pooled then dont re-pool it
        if (bulletList.Contains(bullet))
        {
            return;
        }

        bullet.gameObject.GetPhotonView().RequestOwnership();
        bulletList.Add(bullet);
        bullet.gameObject.SetActive(false);
        _hasPoolChanged = true;
    }

    internal void UnpoolBullet(Bullet bullet)
    {
        if (!_photonView.IsMine) return;

        if (_freeBullets.TryGetValue(bullet.ResourceName, out var bulletList))
        {
            bulletList.Remove(bullet);
        }

        if (_usedBullets.TryGetValue(bullet.ResourceName, out var usedBulletList))
        {
            usedBulletList.Remove(bullet);
        }

        bullet.Reserve();
        _hasPoolChanged = true;
    }

    internal bool MakeBulletUsed(Bullet bullet)
    {
        if (!_photonView.IsMine) return false;

        if (!_freeBullets.TryGetValue(bullet.ResourceName, out var freeBulletsList)) return false;

        if (freeBulletsList.Remove(bullet))
        {
            if (!_usedBullets.TryGetValue(bullet.ResourceName, out var usedBulletsList))
            {
                usedBulletsList = new List<Bullet>();
            }

            if (!usedBulletsList.Contains(bullet))
            {
                usedBulletsList.Add(bullet);
            }

            return true;
        }

        _hasPoolChanged = true;
        return false;
    }

    internal bool MakeBulletFree(Bullet bullet)
    {
        if (!_photonView.IsMine) return false;

        if (!_usedBullets.TryGetValue(bullet.ResourceName, out var usedBulletsList))
        {
            return false;
        }

        if (!usedBulletsList.Contains(bullet))
        {
            return false;
        }

        if (!_freeBullets.TryGetValue(bullet.ResourceName, out var freeBulletsList))
        {
            freeBulletsList = new List<Bullet>();
            _freeBullets.Add(bullet.ResourceName, freeBulletsList);
        }

        freeBulletsList.Add(bullet);
        _hasPoolChanged = true;
        return true;
    }

    internal int GetBulletsFree(string type)
        => _synchedFreeBulletCounts.TryGetValue(type, out var amount) ? amount : 0;

    void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _photonView = GetComponent<PhotonView>();
        _instance = this;
    }

    void Update()
    {
        if (!_photonView.IsMine) return;

        if (_hasPoolChanged)
        {
            foreach (var bulletPair in _freeBullets)
            {
                string bulletResourceName = bulletPair.Key;
                int remaining = bulletPair.Value.Count;

                _photonView.RPC(nameof(this.SetProjectilesFreeRPC), RpcTarget.MasterClient, bulletResourceName, remaining);
            }

            _hasPoolChanged = false;
        }
    }

    #region"Networking"

    [PunRPC]
    public void ProjectileAllocatedRPC(int viewId)
    => this.PoolBullet(PhotonNetwork.GetPhotonView(viewId).gameObject.GetComponent<Bullet>());

    [PunRPC]
    public void ProjectileDeallocatedRPC(int viewId)
        => this.UnpoolBullet(PhotonNetwork.GetPhotonView(viewId).gameObject.GetComponent<Bullet>());

    [PunRPC]
    public void SetProjectilesFreeRPC(string type, int amount)
        => this._synchedFreeBulletCounts[type] = amount;

    [PunRPC]
    public void SetProjectileTypesUsedRPC(string[] typesUsed)
        => this.ProjectileTypesUsed = new List<string>(typesUsed);

    #endregion
}