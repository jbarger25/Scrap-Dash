using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public float mapSize;
    public UI_Controller uiController;
    private void Start()
    {
        Vector3 playerPosition = new Vector3(Random.Range(mapSize * 0.1f, mapSize * 0.9f), 5, Random.Range(mapSize * 0.1f, mapSize * 0.9f));
        RaycastHit hit;
        while (Physics.SphereCast(playerPosition, 1, Vector3.up, out hit))
        {
            playerPosition = new Vector3(Random.Range(mapSize * 0.1f, mapSize * 0.9f), 5, Random.Range(mapSize * 0.1f, mapSize * 0.9f));
        }
        GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, playerPosition, Quaternion.identity);
        uiController.Player = localPlayer;
        Camera.main.GetComponent<CameraController>().playerTransform = localPlayer.transform;
    }
}
