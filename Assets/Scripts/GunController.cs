using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviourPunCallbacks
{
    public float muzzleVelocity = 1200f;
    public float damage = 5f;
    public float ammoCapacity = 5f;
    public float ammoInGun = 5f;
    public float totalAmmo = 20f;
    public float reserveAmmo = 0f;
    public GameObject bullet;
    public PlayerStats playerStats;
    public bool isEquipped = true;
    public AudioClip gunshot;
    public AudioClip reload;
    public AudioSource audioSource;
    void Start()
    {
        if (!this.transform.root.GetComponent<PhotonView>().IsMine)
        {
            this.enabled = false;
            return;
        }
        isEquipped = true;
        audioSource = GetComponent<AudioSource>();
        ammoCapacity = 5f;
        totalAmmo = playerStats.totalAmmo;
    }

    void Update()
    {
        reserveAmmo = totalAmmo - ammoInGun;
        if (reserveAmmo < 0)
            reserveAmmo = 0;
        if (isEquipped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (ammoInGun > 0)
                {
                    GunFire();
                    ammoInGun--;
                    totalAmmo--;
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                GunReload();
            }
        }
    }

    [PunRPC]
    public void GunEquipped(int viewID)
    {
        PhotonView playerView = PhotonNetwork.GetPhotonView(viewID);

        this.transform.rotation = playerView.gameObject.transform.rotation;
        this.transform.SetParent(playerView.gameObject.transform.GetChild(0));
        this.transform.position = playerView.gameObject.transform.GetChild(0).position;
        this.GetComponent<Rigidbody>().isKinematic = true;

        if (playerView.Owner == this.photonView.Owner)
        {
            playerStats = this.transform.root.gameObject.GetComponent<PlayerStats>();
            isEquipped = true;
            totalAmmo = playerStats.totalAmmo;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            this.GetComponent<PhotonTransformView>().m_SynchronizePosition = false;
            this.GetComponent<PhotonTransformView>().m_SynchronizeRotation = false;
        }

        
    }

    [PunRPC]
    public void GunDropped()
    {
        if(this.photonView.IsMine)
        {
            isEquipped = false;
        }
        this.transform.SetParent(null);
        if (PhotonNetwork.IsMasterClient)
        {
            this.GetComponent<PhotonTransformView>().m_SynchronizePosition = true;
            this.GetComponent<PhotonTransformView>().m_SynchronizeRotation = true;
            this.GetComponent<Rigidbody>().AddForce(transform.right * 150);
        }
        this.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void GunReload()
    {
        if (ammoInGun == 0)
        {
            if (totalAmmo >= ammoCapacity)
            {
                ammoInGun += ammoCapacity;
              //  totalAmmo -= ammoCapacity;
            }
            else if (totalAmmo > 0)
            {
                ammoInGun += totalAmmo;
               // totalAmmo -= totalAmmo;
            }
        }
        else if(ammoInGun < ammoCapacity)
        {
            float difference = ammoCapacity - ammoInGun;
            if (difference <= reserveAmmo)
                ammoInGun += difference;
            else {
                ammoInGun += reserveAmmo;
            }
            //totalAmmo -= difference;
        }
        playerStats.totalAmmo = totalAmmo;
        audioSource.clip = reload;
        audioSource.Play();
    }
    public void GunFire()
    {
        GameObject bulletObj = PhotonNetwork.Instantiate(bullet.name, Vector3.zero, Quaternion.identity);
        bulletObj.transform.position = transform.GetChild(0).position + transform.forward * 3f;
        bulletObj.transform.rotation = transform.rotation;
        bulletObj.SetActive(true);
        Rigidbody bulletRB = bulletObj.GetComponent<Rigidbody>();
        bulletRB.AddForce(bulletObj.transform.forward * muzzleVelocity);

        audioSource.clip = gunshot;
        audioSource.Play();
    }
}
