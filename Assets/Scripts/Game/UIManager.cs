using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject UIMenu;

    [Header("Screens")]
    [SerializeField] GameObject loginScreen;
    [SerializeField] GameObject registerScreen;
    [SerializeField] GameObject forgotScreen;
    [SerializeField] GameObject homeScreen;
    [SerializeField] GameObject matchScreen;

    [Header("Home subscreens")]
    [SerializeField] GameObject lobbyScreen;
    [SerializeField] GameObject friendsScreen;

    [Header("Popups")]
    [SerializeField] GameObject popUpFriendRequest;
    [SerializeField] GameObject popUpFriendIsOnline;
    [SerializeField] GameObject popUpMatchFound;
    [SerializeField] Transform popUpContainer;

    [Header("Lobby and Friends related")]
    [SerializeField] Transform containerLobby;
    [SerializeField] Transform containerFriends;
    [SerializeField] GameObject lobbyUserPrefab;

    List<GameObject> lobbyUsers = new List<GameObject>();
    List<GameObject> friendUsers = new List<GameObject>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        /* Testing methods for lobbyusers
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddUserToLobby("redman", "dada231231241", true);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            AddUserToLobby("rodas", "dada231231241", false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AddUserToFriends("gotensfer", "2121515125");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ClearAllUsers();
        }
        */

        /* Testing methods for popups
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PopUpFriendConnected("Gotensfer");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PopUpFriendRequest("Rodas", "sssssss");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PupUpMatchFound("Caco");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ClearAllPopups();
        }
        */
    }

    #region"Utility methods"
    void ClearScreens()
    {
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);
        forgotScreen.SetActive(false);
        homeScreen.SetActive(false);
        matchScreen.SetActive(false);
    }

    void ClearSubscreens()
    {
        lobbyScreen.SetActive(false);
        friendsScreen.SetActive(false);
    }

    void ClearAllPopups()
    {
        int len = popUpContainer.childCount;

        for (int i = len - 1; i >= 0; i--)
        {
            Destroy(popUpContainer.GetChild(i).gameObject);
        }
    }
    #endregion

    #region"Set screen methods"
    public void SetLoginScreen()
    {
        ClearScreens();
        loginScreen.SetActive(true);
    }

    public void SetRegisterScreen()
    {
        ClearScreens();
        registerScreen.SetActive(true);
    }

    public void SetForgotPasswordScreen()
    {
        ClearScreens();
        forgotScreen.SetActive(true);
    }

    public void SetHomeScreen()
    {
        ClearScreens();
        ClearSubscreens();

        homeScreen.SetActive(true);
        lobbyScreen.SetActive(true);
    }

    public void SetMatchScreen()
    {
        ClearScreens();
        matchScreen.SetActive(true);
    }

    public void DisableMenuUIs()
    {
        ClearScreens();
        ClearAllPopups();
        UIMenu.SetActive(false);
    }

    public void EnableMenuUIs()
    {
        ClearScreens();
        ClearAllPopups();
        UIMenu.SetActive(true);
    }
    #endregion

    #region"Set home subscreen methods"
    public void SetSubscreenLobby()
    {
        friendsScreen.SetActive(false);
        lobbyScreen.SetActive(true);
    }

    public void SetSubscreenFriends()
    {
        lobbyScreen.SetActive(false);
        friendsScreen.SetActive(true);
    }
    #endregion

    #region"Popups methods"
    public void PopUpFriendRequest(string userName, string userID, string requestKey)
    {
        GameObject newPopup = Instantiate(popUpFriendRequest, popUpContainer);
        newPopup.GetComponent<TitleHandler>().SetString($"{userName} wants to be friends!");
        newPopup.GetComponent<FriendRequestHandler>().Set(userID, requestKey);
    }

    public void PopUpFriendConnected(string userName)
    {
        GameObject newPopup = Instantiate(popUpFriendIsOnline, popUpContainer);
        newPopup.GetComponent<TitleHandler>().SetString($"{userName} is online!");
    }

    public void PopUpMatchFound(string userName)
    {
        GameObject newPopup = Instantiate(popUpMatchFound, popUpContainer);
        newPopup.GetComponent<TitleHandler>().SetString($"Match found with {userName}");

        Destroy(newPopup, 2f);
        Invoke(nameof(Oklahoma), 2f);
    }

    void Oklahoma()
    {
        print("Juego iniciado");
    }
    #endregion

    #region"Lobby user methods"
    public void ClearLobbyUsers()
    {
        int len = lobbyUsers.Count;
        for (int i = 0; i < len; i++)
        {
            Destroy(lobbyUsers[i]);
        }

        lobbyUsers.Clear();
    }

    public void ClearFriendUsers()
    {
        int len = friendUsers.Count;
        for (int i = 0; i < len; i++)
        {
            Destroy(friendUsers[i]);
        }

        friendUsers.Clear();
    }

    public void ClearAllUsers()
    {
        ClearLobbyUsers();
        ClearFriendUsers();
    }

    public GameObject AddUserToLobby(string userName, string userID, bool isFriend)
    {
        GameObject newLobbyUser = Instantiate(lobbyUserPrefab, containerLobby);
        newLobbyUser.GetComponent<LobbyUser>().Set(userName, userID);
        newLobbyUser.GetComponent<LobbyUser>().SetStatus(isFriend);

        lobbyUsers.Add(newLobbyUser);

        return newLobbyUser;
    }

    public GameObject AddUserToFriends(string userName, string userID)
    {
        GameObject newLobbyUser = Instantiate(lobbyUserPrefab, containerFriends);
        newLobbyUser.GetComponent<LobbyUser>().Set(userName, userID);
        newLobbyUser.GetComponent<LobbyUser>().SetStatus(true);

        friendUsers.Add(newLobbyUser);

        return newLobbyUser;
    }
    #endregion
}
