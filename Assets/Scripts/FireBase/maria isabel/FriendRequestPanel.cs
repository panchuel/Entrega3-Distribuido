using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FriendRequestPanel : MonoBehaviour
{
    public Text senderUsernameText;
    public Button acceptButton;
    public Button declineButton;

    private string senderID;
    private string requestID;

    public void InitializePanel(string senderID, string requestID)
    {
        this.senderID = senderID;
        this.requestID = requestID;

        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.RootReference
            .Child("users").Child(senderID).Child("username");

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching sender username: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            string senderUsername = snapshot.Value.ToString();

            senderUsernameText.text = senderUsername;
        });
    }

    /*public void AcceptFriendRequest()
    {
        FriendManager.Instance.AcceptFriendRequest(senderID, AuthManager.Instance.CurrentUserID, requestID);

        gameObject.SetActive(false);
    }*/

    /*public void DeclineFriendRequest()
    {
        FriendManager.Instance.DeclineFriendRequest(AuthManager.Instance.CurrentUserID, requestID);

        gameObject.SetActive(false);
    }*/
}
