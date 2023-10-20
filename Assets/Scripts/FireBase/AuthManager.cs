using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;
using System.Linq;

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

    public void SendFriendRequest(string friendUserID)
    {
        // Obtener la referencia al usuario actual en la base de datos
        DatabaseReference currentUserRef = dbReference.Child("users").Child(user.UserId);
        // Verificar si el usuario actual ya ha enviado una solicitud de amistad a esta persona
        DatabaseReference friendRequestsRef = currentUserRef.Child("friendRequests");
        friendRequestsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                if (!task.Result.HasChild(friendUserID))
                {
                    // Si no ha enviado una solicitud previamente, envía la solicitud
                    friendRequestsRef.Child(friendUserID).SetValueAsync(true);
                    Debug.Log("Solicitud de amistad enviada.");
                }
                else
                {
                    Debug.LogWarning("Ya has enviado una solicitud de amistad a este usuario.");
                }
            }
            else
            {
                // Si no hay solicitud previa, crea la lista y envía la solicitud
                friendRequestsRef.SetValueAsync(friendUserID, true);
                Debug.Log("Solicitud de amistad enviada.");
            }
        });
    }

    public void AcceptFriendRequest(string requesterUserID)
    {
        // Obtener la referencia al usuario actual en la base de datos
        DatabaseReference currentUserRef = dbReference.Child("users").Child(user.UserId);
        // Obtener la referencia al usuario que envió la solicitud
        DatabaseReference requesterUserRef = dbReference.Child("users").Child(requesterUserID);
        // Agregar el requesterUserID a la lista de amigos del usuario actual
        currentUserRef.Child("friends").Child(requesterUserID).SetValueAsync(true);
        // Eliminar la solicitud de amistad del usuario actual
        currentUserRef.Child("friendRequests").Child(requesterUserID).RemoveValueAsync();
        // Agregar al usuario actual a la lista de amigos del solicitante
        requesterUserRef.Child("friends").Child(user.UserId).SetValueAsync(true);

        Debug.Log("Solicitud de amistad aceptada.");
    }
    public void UpdateScore()
    {
        StartCoroutine(Score(int.Parse(highScore.text)));
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
                    List<SystemUsers> emptyFriendList = new List<SystemUsers>();

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
                        var DBTask = dbReference.Child("users").Child(user.UserId).Child("username").SetValueAsync(username);
                        DBTask = dbReference.Child("users").Child(user.UserId).Child("score").SetValueAsync(0.ToString());
                        DBTask = dbReference.Child("users").Child(user.UserId).Child("IsMyFriend").SetValueAsync(false);
                        DBTask = dbReference.Child("users").Child(user.UserId).Child("Friends").SetValueAsync(emptyFriendList);
                        UIManager.instance.SetLoginScreen();
                        warningRegisterText.text = "";
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
        var DBTask = dbReference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Result.Value == null)
        {
            highScore.text = "0";
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            highScore.text = snapshot.Child("score").Value.ToString();
            highScoreIntern.highScore = int.Parse(highScore.text);
        }
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
        var DBTask = dbReference.Child("users").OrderByChild("score").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Destruyo todos los elementos de la tabla
            foreach (Transform child in scoreboardContent.transform) Destroy(child.gameObject);

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string userId = childSnapshot.Key;
                string userName = childSnapshot.Child("username").Value.ToString();
                bool Friends = (bool)childSnapshot.Child("IsMyFriend").Value;

                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                UIManager.instance.AddUserToLobby(userName, userId, Friends);
            }
        }
    }
}


[System.Serializable]
public class SystemUsers
{
    public string userId;
    public string userName;   
}
