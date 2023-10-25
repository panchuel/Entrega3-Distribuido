using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

public class FriendManager : MonoBehaviour
{
    DatabaseReference databaseReference;
    FirebaseUser currentUser;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        });
    }

    public void SendFriendRequest(string receiverID)
    {
        string senderID = currentUser.UserId;

        FriendRequestData newRequest = new FriendRequestData(senderID, "pending");

        databaseReference.Child("users").Child(receiverID).Child("friend_requests").Push().SetValueAsync(newRequest);
    }

    public void AcceptFriendRequest(string senderID, string receiverID, string requestID)
    {
        databaseReference.Child("users").Child(receiverID).Child("friend_requests").Child(requestID).SetValueAsync(null);

        databaseReference.Child("users").Child(receiverID).Child("friends").Child(senderID).SetValueAsync(true);
        databaseReference.Child("users").Child(senderID).Child("friends").Child(receiverID).SetValueAsync(true);
    }

    public void DeclineFriendRequest(string receiverID, string requestID)
    {
        databaseReference.Child("users").Child(receiverID).Child("friend_requests").Child(requestID).SetValueAsync(null);
    }
}

[System.Serializable]
public class FriendRequestData
{
    public string from;
    public string status;

    public FriendRequestData(string from, string status)
    {
        this.from = from;
        this.status = status;
    }
}
