using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestHandler : MonoBehaviour
{
    string userID;
    string requestKey;
    [SerializeField] Button acceptRequestButton;
    [SerializeField] Button cancelRequestButton;

    private void Awake()
    {
        acceptRequestButton.onClick.AddListener(() =>
        {
            print("<color=cyan>ACCEPTED FRIEND REQUEST</color>");
            AuthManager.instance.HandleFriendRequest(userID, requestKey, true);
            Destroy(gameObject);
        });

        cancelRequestButton.onClick.AddListener(() =>
        {
            print("<color=magenta>REJECTED FRIEND REQUEST</color>");
            AuthManager.instance.HandleFriendRequest(userID, requestKey, false);
            Destroy(gameObject);
        });
    }

    public void Set(string userID, string requestKey)
    {
        this.userID = userID;
        this.requestKey = requestKey;
    }
}
