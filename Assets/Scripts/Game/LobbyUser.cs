using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUser : MonoBehaviour
{
    [SerializeField] Button addFriendButton;

    string userName;
    string userID;

    public void Set(string userName, string userID)
    {
        addFriendButton.onClick.RemoveAllListeners();

        this.userName = userName;
        this.userID = userID;

        //addFriendButton.onClick.AddListener();
    }
}
