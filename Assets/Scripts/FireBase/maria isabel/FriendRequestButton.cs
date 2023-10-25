using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class FriendRequestButton : MonoBehaviour
{
    public InputField searchInput;

    public void SendFriendRequest()
    {
        string searchUsername = searchInput.text;

        if (string.IsNullOrEmpty(searchUsername))
        {
            Debug.LogWarning("Please enter a username to search.");
            return;
        }

        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");

        userRef.OrderByChild("username").EqualTo(searchUsername).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching receiver ID: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.HasChildren)
            {
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    string receiverID = childSnapshot.Key;
                    FriendManager.Instance.SendFriendRequest(receiverID);
                    break;
                }
            }
            else
            {
                Debug.LogWarning("No user found with username: " + searchUsername);
            }
        });
    }
}
