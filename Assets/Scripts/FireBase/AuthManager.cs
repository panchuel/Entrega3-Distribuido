using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;
using System.Linq;
using Google.MiniJSON;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbReference;

    //Login variables
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text warningLoginText;
    [SerializeField] TMP_Text confirmationPasswordText;

    //Register variables
    [Header("Register")]
    [SerializeField] private TMP_InputField usernameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text warningRegisterText;

    [Header("Forgot Password")]
    [SerializeField] TMP_InputField forgotPasswordEmail;
    [SerializeField] private TMP_Text warningForgetPasswordText;

    [Header("UserData")]
    [SerializeField] TMP_Text usernameField;
    [SerializeField] GameObject scoreElement;
    [SerializeField] Transform scoreboardContent;
    [SerializeField] List<SystemUsers> userList = new List<SystemUsers>();

    [Header("Game")]
    [SerializeField] GameObject ball;
    [SerializeField] GameObject gameUI, menuUI, scoreboardUI;
    [SerializeField] TMP_Text highScore;
    [SerializeField] LoseManager highScoreIntern;

    public static AuthManager instance;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            
        }
        
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"No se pueden resolver todas las dependencias de Firebase: {dependencyStatus}");
            }
        });


    }

    void InitializeFirebase()
    {
        print($"Configurando autorización de Firebase");
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;        
    }

   

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
        StartCoroutine(Lobby());
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    public void AddFriend(string userID)
    {
        SendFriendRequest(userID);
    }

    public void ForgotPasswordButton()
    {
        if (string.IsNullOrEmpty(forgotPasswordEmail.text))
        {
            warningForgetPasswordText.text = $"No has colocado correo electronico";
            return;
        }

        FogotPassword(forgotPasswordEmail.text);
    }

    public void ScoreBoardButton()
    {
        StartCoroutine(LoadScoreBoard());
    }

    void FogotPassword(string forgotPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgotPasswordEmail).ContinueWithOnMainThread(RestoreTask => {

            if (RestoreTask.IsCanceled)
            {
                Debug.LogError($"El cambio de contraseña ha sido cancelado");
            }

            else if(RestoreTask.IsFaulted)
            {
                foreach(FirebaseException exception in RestoreTask.Exception.Flatten().InnerExceptions) 
                { 
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if(firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                    }

                }
            }

            confirmationPasswordText.text = "El correo para reestablecer la contraseña ha sido enviado";
            UIManager.instance.SetLoginScreen();
        });
    }

    public void SendFriendRequest(string receiverUid)
    {
        /*
        // Verifica si el usuario ya es amigo del destinatario
        DatabaseReference currentUserFriendsRef = dbReference.Child("users").Child(user.UserId).Child("friends");
        currentUserFriendsRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.Result.Exists)
            {
                if (!task.Result.HasChild(friendUserID))
                {
                    // El usuario no es amigo del destinatario, por lo que puede enviar una solicitud
                    // Crea una nueva solicitud de amistad
                    FriendRequest request = new FriendRequest
                    {
                        senderUserId = user.UserId,
                        recipientUserId = friendUserID,
                        accepted = false
                    };

                    // Agrega la solicitud a la lista de solicitudes de amistad del destinatario
                    DatabaseReference recipientUserRef = dbReference.Child("users").Child(friendUserID).Child("friendRequests");
                    recipientUserRef.Child(user.UserId).SetRawJsonValueAsync(JsonUtility.ToJson(request));
                    Debug.Log("Solicitud de amistad enviada.");
                }
                else
                {
                    Debug.LogWarning("Ya eres amigo de este usuario.");
                }
            }
        });
        */

        print($"Called SendFriendRequest to UID {receiverUid}");

        // Create a unique key for the friend request (e.g., using Push)
        DatabaseReference friendRequestRef = dbReference.Database.GetReference("friend_request");
        string requestKey = friendRequestRef.Push().Key;
        string senderUid = user.UserId;

        FriendRequest newFriendRequest = new FriendRequest
        {
            senderUserId = senderUid,
            recipientUserId = receiverUid,
            accepted = false
        };

        string jsonData = JsonUtility.ToJson(newFriendRequest);
        print(jsonData);
        friendRequestRef.Child(requestKey).SetRawJsonValueAsync(jsonData);

        /*
        // Define the friend request data
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            { "senderUid", senderUid },
            { "receiverUid", receiverUid },
            { "status", "pending" }
            // You can add more information if needed, such as timestamps
        };

        // Set the friend request data in the database under the unique key
        friendRequestRef.Child(requestKey).SetValueAsync(requestData);
        */
    }


    public void HandleFriendRequest(string friendUid, string requestKey, bool accept)
    {
        // If the user accepts the request
        if (accept)
        {
            // Add the sender to the user's friend list
            AddFriendInDataBase(friendUid);
        }

        // Remove the friend request from the database
        RemoveFriendRequest(requestKey);
    }
    public void ShowFriendRequests()
    {
        // Obtén la lista de solicitudes de amistad del usuario actual
        DatabaseReference currentUserRef = dbReference.Child("users").Child(user.UserId).Child("friendRequests");
        currentUserRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                List<FriendRequest> friendRequests = new List<FriendRequest>();
                foreach (var childSnapshot in snapshot.Children)
                {
                    FriendRequest request = JsonUtility.FromJson<FriendRequest>(childSnapshot.GetRawJsonValue());
                    friendRequests.Add(request);
                }

                // Aquí debes mostrar las solicitudes de amistad en tu UI, por ejemplo, en una lista.
                // Puedes utilizar Unity's UI elements para mostrar la lista de solicitudes.
                foreach (var request in friendRequests)
                {
                    // Agregar lógica para mostrar la solicitud en la interfaz de usuario.
                    // Por ejemplo, puedes crear elementos de lista en Unity y configurar su contenido.
                    Debug.Log("Solicitud de amistad de: " + request.senderUserId);
                }
            }
        });
    }
    public void UpdateScore()
    {
        StartCoroutine(Score(int.Parse(highScore.text)));
    }

    public void ShowFriends()
    {
        // Obtén la lista de amigos del usuario actual
        DatabaseReference currentUserRef = dbReference.Child("users").Child(user.UserId).Child("friends");
        currentUserRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                List<string> friendIds = new List<string>();
                foreach (var childSnapshot in snapshot.Children)
                {
                    friendIds.Add(childSnapshot.Key);
                }

                // Aquí debes mostrar la lista de amigos en tu UI.
                // Puedes utilizar Unity's UI elements para mostrar la lista de amigos.
                foreach (var friendId in friendIds)
                {
                    // Agregar lógica para mostrar la lista de amigos en la interfaz de usuario.
                    // Por ejemplo, puedes crear elementos de lista en Unity y configurar su contenido.
                    Debug.Log("Amigo: " + friendId);
                }
            }
        });
    }


    IEnumerator Login(string email, string password)
    {
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        confirmationPasswordText.text = "";
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        
        if(LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Fallo en el inicio con {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Ingreso Fallido!";
            switch(errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Falta correo";
                    break;
                case AuthError.MissingPassword:
                    message = "Falta contraseña";
                    break;
                case AuthError.WrongPassword:
                    message = "Contraseña incorrecta";
                    break;
                case AuthError.InvalidEmail:
                    message = "Correo invalido";
                    break;
                case AuthError.UserNotFound:
                    message = "Usuario no encontrado";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            confirmationPasswordText.text = "";
            user = LoginTask.Result.User;
            Debug.LogFormat("Usuario iniciado excitosamente: {0} ({1})", user.DisplayName, user.Email);
            warningLoginText.text = "";

            StartCoroutine(LoadData());
            StartCoroutine(CheckForLobbyUpdatedCoroutine());

            yield return new WaitForSeconds(1);

            usernameField.text = user.DisplayName;

            UIManager.instance.SetHomeScreen();

            //gameUI.SetActive(true);
            //menuUI.SetActive(true);
            //ball.SetActive(true);
        }
    }

    IEnumerator Register(string email, string password, string username)
    {
        if (username == "") warningRegisterText.text = "Falta usuario";
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text) warningRegisterText.text = "Las contraseñas no coinciden";
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Fallo en el registro con {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Registro fallido";

                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Falta correo";
                        break;
                    case AuthError.MissingPassword:
                        message = "Falta contraseña";
                        break;
                    case AuthError.WeakPassword:
                        message = "Contraseña debil";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Correo ya en uso";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                user = RegisterTask.Result.User;

                if (user != null)
                {
                    //List<SystemUsers> emptyFriendList = new List<SystemUsers>(); 
                    //List<SystemUsers> emptyNotiList = new List<SystemUsers>();
                        
                    UserProfile profile = new UserProfile { DisplayName = username };

                    var ProfileTask = user.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Fallo el registro con {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Configuración de usuario fallida";
                    }
                    else
                    {
                        DBUser newUser = new DBUser();
                        newUser.username = username;
                        newUser.score = 0;

                        // Añadir placeholder de amigo
                        newUser.friends = new List<Friend>();
                        Friend placeholderFriend = new Friend()
                        {
                            uid = "-1"
                        };
                        newUser.friends.Add(placeholderFriend);

                        string jsonUser = JsonUtility.ToJson(newUser);
                        print(jsonUser);

                        dbReference.Child("users").Child(user.UserId).SetRawJsonValueAsync(jsonUser);

                        /*
                        var DBTask = dbReference.Child("users").Child(user.UserId).Child("username").SetValueAsync(username);
                        DBTask = dbReference.Child("users").Child(user.UserId).Child("score").SetValueAsync(0.ToString());

                        DBTask = dbReference.Child("users").Child(user.UserId).Child("friend_requests").SetRawJsonValueAsync(JsonUtility.ToJson(emptyFriendRequests));

                        // Create an empty friends dictionary
                        //Dictionary<string, object> emptyFriends = new Dictionary<string, object>();
                        //DBTask = dbReference.Child("users").Child(user.UserId).Child("friends").SetRawJsonValueAsync(JsonUtility.ToJson(emptyFriends));

                        UIManager.instance.SetLoginScreen();
                        warningRegisterText.text = "";

                        print("Setted values 1");
                        */
                    }
                }
            }
        }
    }

    IEnumerator Score(int score)
    {
        var DBTask = dbReference.Child("users").Child(user.UserId).Child("score").SetValueAsync(score);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Exception != null) Debug.LogWarning($"Fallo al registrar la tarea {DBTask.Exception}");
    }

    IEnumerator LoadData()
    {
        yield return null;

        /*
        var DBTask = dbReference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning("Fallo al cargar datos del usuario: " + DBTask.Exception);
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            highScore.text = snapshot.Child("score").Value.ToString();
            highScoreIntern.highScore = int.Parse(highScore.text);

            string friendsJson = snapshot.Child("friends").GetRawJsonValue();
            List<string> friendsList = JsonUtility.FromJson<List<string>>(friendsJson);

            // Verificar las solicitudes de amistad pendientes
            if (snapshot.Child("friendRequests").Exists)
            {
                foreach (var requestSnapshot in snapshot.Child("friendRequests").Children)
                {
                    string senderUserId = requestSnapshot.Key;
                    bool accepted = (bool)requestSnapshot.Value;

                    foreach (string friendUserId in friendsList)
                    {
                        Debug.Log("Amigo: " + friendUserId);
                    }

                    if (!accepted)
                    {
                        
                        Debug.Log("Solicitud de amistad pendiente de: " + senderUserId);
                    }
                }
            }
        }
        */
    }

    IEnumerator LoadScoreBoard()
    {
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Destruyo todos los elementos de la tabla
            foreach(Transform child in scoreboardContent.transform) Destroy(child.gameObject);

            foreach(DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int score = int.Parse(childSnapshot.Child("score").Value.ToString());

                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, score);
            }
        }

        scoreboardUI.SetActive(true);
        gameUI.SetActive(false);
        menuUI.SetActive(false);
        ball.SetActive(false);
    }

    IEnumerator Lobby()
    {
        /*
        DBUser selfUser = null;

        var DBTaskGetSelf = dbReference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTaskGetSelf.IsCompleted);

        if (DBTaskGetSelf.Exception != null) Debug.LogWarning($"Fallo en registrar tarea de obtenerse a si mismo {DBTaskGetSelf.Exception}");
        else
        {
            DataSnapshot snapshot = DBTaskGetSelf.Result;

            if (snapshot.Exists)
            {
                // Parse the snapshot into a DBUser object
                string jsonData = snapshot.GetRawJsonValue();
                selfUser = JsonUtility.FromJson<DBUser>(jsonData);

                // Now selfUser contains the data from the database
                Debug.Log($"Retrieved self user data: {selfUser.username}, {selfUser.score}");

                // Check if the "friends" field is null, and if so, initialize it
                if (selfUser.friends == null)
                {
                    selfUser.friends = new List<Friend>();
                }
            }
            else
            {
                Debug.LogWarning("No data found for the user");
            }
        }
        */

        yield return new WaitForSeconds(3f);

        DBUser selfUser = null;
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Destruyo todos los elementos de la tabla
            UIManager.instance.ClearLobbyUsers();

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId))
                {
                    string jsonData = childSnapshot.GetRawJsonValue();
                    selfUser = JsonUtility.FromJson<DBUser>(jsonData);
                    break;
                }
            }

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId)) continue;

                string userId = childSnapshot.Key;
                string userName = childSnapshot.Child("username").Value.ToString();

                bool isFriend = false;
                for (int i = 0; i < selfUser.friends.Count; i++)
                {
                    if (string.Equals(childSnapshot.Key, selfUser.friends[i].uid))
                    {
                        isFriend = true;
                        break;
                    }
                }

                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                UIManager.instance.AddUserToLobby(userName, userId, isFriend);
            }
        }
    }

    IEnumerator FriendsRoom()
    {
        yield return new WaitForSeconds(3f);

        DBUser selfUser = null;
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Destruyo todos los elementos de la tabla
            UIManager.instance.ClearFriendUsers();

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId))
                {
                    string jsonData = childSnapshot.GetRawJsonValue();
                    selfUser = JsonUtility.FromJson<DBUser>(jsonData);
                    break;
                }
            }

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId)) continue;

                string userId = childSnapshot.Key;
                string userName = childSnapshot.Child("username").Value.ToString();
                bool isFriend = false;

                for (int i = 0; i < selfUser.friends.Count; i++)
                {
                    if (string.Equals(childSnapshot.Key, selfUser.friends[i].uid))
                    {
                        isFriend = true;
                        break;
                    }
                }

                if (isFriend)
                {
                    GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                    UIManager.instance.AddUserToFriends(userName, userId);
                }            
            }
        }
    }

    IEnumerator CheckForLobbyUpdatedCoroutine()
    {
        yield return new WaitForSeconds(5);
        SubscribeToFriendRequest();

        while (true) // A bit dangerous, should work for now - Gotensfer
        {
            StartCoroutine(Lobby());
            StartCoroutine(FriendsRoom());
            yield return new WaitForSeconds(5);
        }
    }

    #region"Friend request handling"
    DatabaseReference friendRequestRef;

    void SubscribeToFriendRequest()
    {
        // Get a reference to the Firebase database
        friendRequestRef = dbReference.Database.GetReference("friend_request");
        print($"friendRequestRef null ? {friendRequestRef == null}");
        print("OE Y MI PRINT???");

        // Add a listener to the friend requests node
        friendRequestRef.ChildAdded += HandleIncomingFriendRequest;
    }

    void HandleIncomingFriendRequest(object sender, ChildChangedEventArgs args)
    {
        print("--- - HandleIncomingFriendRequest - ---");
        StartCoroutine(HandleFriendRequestAdded(sender, args));
    }

    // This method will be called when a new friend request is added
    IEnumerator HandleFriendRequestAdded(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("<color=red>RECEIVED FRIEND REQUEST</color>");
        string usernameSender = "";

        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            yield break;
        }

        DataSnapshot friendRequestSnapshot = args.Snapshot;
        string requestKey = friendRequestSnapshot.Key;

        string jsonData = friendRequestSnapshot.GetRawJsonValue();
        FriendRequest friendRequest = JsonUtility.FromJson<FriendRequest>(jsonData);

        // You can retrieve information from the friend request here
        string senderUid = friendRequest.senderUserId; // Replace with the actual field names in your database

        // Encontrar user del request
        DBUser requestUser = null;
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshotUsers = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshotUsers.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, senderUid))
                {
                    string jsonDataUser = childSnapshot.GetRawJsonValue();
                    requestUser = JsonUtility.FromJson<DBUser>(jsonDataUser);
                    usernameSender = requestUser.username;
                    break;
                }
            }
        }

        // Check if this request is for the current user (User B)
        if (string.Equals(friendRequest.recipientUserId, user.UserId) && !friendRequest.accepted)
        {
            Debug.Log("<color=green>RECEIVED REQUEST FOR MYSELF</color>");
            // Display a notification or trigger the UI popup to inform User B about the friend request
            UIManager.instance.PopUpFriendRequest(usernameSender, senderUid, senderUid);
        }
    }
    #endregion

    void AddFriendInDataBase(string senderUid)
    {
        print("Attempting adding friend. . .");
        StartCoroutine(AddFriendInDataBaseCoroutine(senderUid));
    }

    private IEnumerator AddFriendInDataBaseCoroutine(string senderUid)
    {
        // Encontrar este user para actualizarlo
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        DBUser selfUser = null;
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshotUsers = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshotUsers.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId))
                {
                    string jsonDataUser = childSnapshot.GetRawJsonValue();
                    selfUser = JsonUtility.FromJson<DBUser>(jsonDataUser);

                    Friend newFriend = new Friend();
                    newFriend.uid = senderUid;

                    selfUser.friends.Add(newFriend);

                    break;
                }
            }
        }

        // Encontrar al otro user para actualizarlo
        var DBTaskOther = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        DBUser otherUser = null;
        yield return new WaitUntil(predicate: () => DBTaskOther.IsCompleted);

        if (DBTaskOther.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTaskOther.Exception}");
        else
        {
            DataSnapshot snapshotUsers = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshotUsers.Children.Reverse<DataSnapshot>())
            {
                if (string.Equals(childSnapshot.Key, user.UserId))
                {
                    string jsonDataUser = childSnapshot.GetRawJsonValue();
                    otherUser = JsonUtility.FromJson<DBUser>(jsonDataUser);

                    Friend newFriend = new Friend();
                    newFriend.uid = user.UserId;

                    otherUser.friends.Add(newFriend);

                    break;
                }
            }
        }


        string jsonUpdate = JsonUtility.ToJson(selfUser);
        dbReference.Child("users").Child(user.UserId).SetRawJsonValueAsync(jsonUpdate);
        print(". . . Finished adding friend");
    }

    private void RemoveFriendRequest(string requestKey)
    {
        // Remove the friend request using the requestKey
        FriendRequest friendRequest = new FriendRequest()
        {
            senderUserId = "-1",
            recipientUserId = "-1",
            accepted = true
        };

        string jsonUpdate = JsonUtility.ToJson(friendRequest);

        dbReference.Child("friend_request").Child(requestKey).SetRawJsonValueAsync(jsonUpdate);
        print("Removed friend request");
    }
}


[System.Serializable]
public class SystemUsers
{
    public string userId;
    public string userName;
    public List<string> friends = new List<string>(); 
    public List<FriendRequest> friendRequests = new List<FriendRequest>(); 
}


[System.Serializable]
public class FriendRequest
{
    public string senderUserId;
    public string recipientUserId;
    public bool accepted;
}

[System.Serializable]
public class Friend
{
    public string uid;
}

[System.Serializable]
public class DBUser
{
    public string username;
    public int score;
    public List<Friend> friends;
}
