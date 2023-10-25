using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUser : MonoBehaviour
{
    [SerializeField] Button addFriendButton;
    [SerializeField] TextMeshProUGUI userNameDisplay;
    [SerializeField] Image onlineStatus;

    string userID;

    private void Awake()
    {
        addFriendButton.onClick.AddListener(() => AuthManager.instance.AddFriend(this.userID));
    }

    public void Set(string userName, string userID)
    {
        addFriendButton.gameObject.SetActive(true);
        this.userID = userID;

        userNameDisplay.text = userName;
    }

    public void SetStatus(bool isFriend)
    {
        if (isFriend)
        {
            addFriendButton.gameObject.SetActive(false);
        }
    }

    public void SetOnlineStatus(bool isOnline)
    {
        if(isOnline)
        {
            onlineStatus.color = Color.green;           
        }
        else
        {
            onlineStatus.color = Color.red;
        }
    }
}
