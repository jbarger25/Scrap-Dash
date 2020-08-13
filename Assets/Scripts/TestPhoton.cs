using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhoton : MonoBehaviourPunCallbacks
{
    public override void OnConnectedToMaster()
    {
        Debug.Log("We did it!");
    }
}
