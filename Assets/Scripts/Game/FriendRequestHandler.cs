using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestHandler : MonoBehaviour
{
    string userID;
    [SerializeField] Button acceptRequestButton;

    private void Awake()
    {
        acceptRequestButton.onClick.AddListener(() =>
        {
            print("Accepted friend request");
            AuthManager.instance.AcceptFriendRequest(userID);
        }); 
    }

    public void Set(string userID)
    {
        this.userID = userID;
    }
}
