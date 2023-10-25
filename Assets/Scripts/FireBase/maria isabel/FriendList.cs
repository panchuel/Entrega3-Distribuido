using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class FriendListPanel : MonoBehaviour
{
    public Transform friendButtonContainer;
    public GameObject friendButtonPrefab;

    void Start()
    {
        LoadFriendList();
    }

    void LoadFriendList()
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.RootReference
            .Child("users").Child(AuthManager.Instance.CurrentUserID).Child("friends");

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching friend list: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (DataSnapshot friendSnapshot in snapshot.Children)
            {
                string friendID = friendSnapshot.Key;

                DatabaseReference usernameRef = FirebaseDatabase.DefaultInstance.RootReference
                    .Child("users").Child(friendID).Child("username");

                usernameRef.GetValueAsync().ContinueWithOnMainThread(usernameTask =>
                {
                    if (usernameTask.IsFaulted || usernameTask.IsCanceled)
                    {
                        Debug.LogError("Error fetching friend username: " + usernameTask.Exception);
                        return;
                    }

                    DataSnapshot usernameSnapshot = usernameTask.Result;
                    string friendUsername = usernameSnapshot.Value.ToString();

                    CreateFriendButton(friendUsername);
                });
            }
        });
    }

    void CreateFriendButton(string friendUsername)
    {
        GameObject friendButton = Instantiate(friendButtonPrefab, friendButtonContainer);
        Text buttonText = friendButton.GetComponentInChildren<Text>();
        buttonText.text = friendUsername;
    }
}
