using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotonLauncher : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public SceneField gameScene;

    public GameObject mainMenu;

    public GameObject findOrCreateGameMenu;
    public TMP_InputField playerNameInputField;

    public GameObject createGameMenu;
    public TMP_InputField createGameNameInputField;

    public GameObject gameLobbyMenu;
    public GameObject gameLobbyMenuList;
    public GameObject gameLobbyMenuItem;

    public GameObject roomLobbyMenu;
    public GameObject roomLobbyMenuList;
    public GameObject roomLobbyMenuItem;
    public GameObject roomLobbyReadyButton;

    private Dictionary<Player, bool> roomReadyStatuses;
    private GameObject selectedRoomItem;
    private bool isLocalPlayerReady = false;

    #region Monobehaviour Methods
    private void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AutomaticallySyncScene = true;
        InitializeNickname();
        if(PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN: OnConnectedToMaster");
        ConnectToMasterServerLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("PUN: OnJoinedLobby");
        ShowFindOrCreateGameMenu();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN: OnJoinedRoom");
        ShowRoomLobbyMenu();
        roomReadyStatuses = new Dictionary<Player, bool>();
        foreach(Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            roomReadyStatuses.Add(player, false);
        }
        UpdateRoomPlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("PUN: OnPlayerEnteredRoom - " + newPlayer.NickName);
        roomReadyStatuses.Add(newPlayer, false);
        UpdateRoomPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("PUN: OnPlayerLeftRoom - " + otherPlayer.NickName);
        roomReadyStatuses.Remove(otherPlayer);
        UpdateRoomPlayerList();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("PUN: OnDisconnected");
        ShowMainMenu();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("PUN: OnRoomListUpdate");
        foreach (Transform gameLobbyListItemTransform in gameLobbyMenuList.transform)
        {
            GameObject gameLobbyListItemObject = gameLobbyListItemTransform.gameObject;
            GameObject.Destroy(gameLobbyListItemObject);
        }

        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                GameObject gameLobbyMenuListItem = GameObject.Instantiate(gameLobbyMenuItem, gameLobbyMenuList.transform);
                gameLobbyMenuListItem.name = room.Name;
                gameLobbyMenuListItem.GetComponentInChildren<TMP_Text>().text = room.Name;
                gameLobbyMenuListItem.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectRoom(gameLobbyMenuListItem);
                });
            }
        }
    }
    #endregion

    #region Public Methods
    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenu.SetActive(true);
    }

    // Entry Point
    public void ConnectToGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhotonMasterServer();
        }
        else
        {
            ConnectToMasterServerLobby();
        }
    }

    public void ShowCreateGameMenu()
    {
        HideAllMenus();
        createGameMenu.SetActive(true);
    }

    public void CreateGame()
    {
        string roomName = createGameNameInputField.text;
        // Create a random game name if no string is entered. Random name is always "Game #XXXX".
        PhotonNetwork.CreateRoom(string.IsNullOrEmpty(roomName) ? "Game #" + Random.Range(1000, 9999) : roomName);
    }

    public void ShowGameLobbyMenu()
    {
        HideAllMenus();
        gameLobbyMenu.SetActive(true);
    }

    public void JoinRoom()
    {
        if(selectedRoomItem == null)
        {
            return;
        }
        PhotonNetwork.JoinRoom(selectedRoomItem.name);
    }

    public void ToggleReady()
    {
        isLocalPlayerReady = !isLocalPlayerReady;
        roomLobbyReadyButton.GetComponentInChildren<TMP_Text>().text = isLocalPlayerReady ? "Unready" : "Ready";
        byte eventCode = PhotonEventCodes.SET_READY_STATUS;
        object[] content = new object[] { isLocalPlayerReady };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, sendOptions);
    }
    #endregion

    #region Private Methods
    private void ConnectToPhotonMasterServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    private void ConnectToMasterServerLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    private void ShowFindOrCreateGameMenu()
    {
        HideAllMenus();
        findOrCreateGameMenu.SetActive(true);
    }

    private string playerNamePrefKey = "PlayerName";

    private void InitializeNickname()
    {
        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            string storedName = PlayerPrefs.GetString(playerNamePrefKey);
            playerNameInputField.text = storedName;
            PhotonNetwork.NickName = storedName;
        }

        playerNameInputField.onValueChanged.AddListener(UpdatePlayerNickname);
    }

    private void UpdatePlayerNickname(string nickname)
    {
        PlayerPrefs.SetString(playerNamePrefKey, nickname);
        PhotonNetwork.NickName = nickname;
    }

    private void ShowRoomLobbyMenu()
    {
        HideAllMenus();
        roomLobbyMenu.SetActive(true);
    }

    // Used to avoid keeping track of what menu is open before opening another.
    // This is more robust, too.
    private void HideAllMenus()
    {
        mainMenu.SetActive(false);
        findOrCreateGameMenu.SetActive(false);
        createGameMenu.SetActive(false);
        gameLobbyMenu.SetActive(false);
        roomLobbyMenu.SetActive(false);
    }

    private void CheckReadyStatus()
    {
        // Check if all players are ready, return if not.
        foreach(bool readyStatus in roomReadyStatuses.Values)
        {
            if(readyStatus == false)
            {
                return;
            }
        }

        // All Players Ready - Start Game
        LaunchGame();
    }

    private void LaunchGame()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(gameScene.SceneName);
    }

    private void SelectRoom(GameObject lobbyListItem)
    {
        if (selectedRoomItem != null)
        {
            selectedRoomItem.GetComponent<Image>().color = Color.white;
        }
        selectedRoomItem = lobbyListItem;
        selectedRoomItem.GetComponent<Image>().color = Color.blue;
    }

    private void UpdateRoomPlayerList()
    {
        foreach (Transform roomLobbyListItemTransform in roomLobbyMenuList.transform)
        {
            GameObject roomLobbyListItemObject = roomLobbyListItemTransform.gameObject;
            GameObject.Destroy(roomLobbyListItemObject);
        }

        foreach (Player player in roomReadyStatuses.Keys)
        {
            GameObject roomLobbyMenuListItem = GameObject.Instantiate(roomLobbyMenuItem, roomLobbyMenuList.transform);
            roomLobbyMenuListItem.name = player.NickName;
            roomLobbyMenuListItem.GetComponentInChildren<TMP_Text>().text = player.NickName;
            roomLobbyMenuListItem.GetComponentInChildren<Toggle>().isOn = roomReadyStatuses[player];
        }
    }
    #endregion

    #region Photon Events
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == PhotonEventCodes.SET_READY_STATUS)
        {
            object[] data = (object[])photonEvent.CustomData;

            bool isReady = (bool)data[0];

            SetReadyStatus(isReady, PhotonNetwork.CurrentRoom.Players[photonEvent.Sender]);
        }
    }

    public void SetReadyStatus(bool isReady, Player sender)
    {
        roomReadyStatuses[sender] = isReady;
        UpdateRoomPlayerList();
        if (PhotonNetwork.IsMasterClient)
        {
            CheckReadyStatus();
        }
    }
    #endregion
}
