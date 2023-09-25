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

    [Header("Game")]
    [SerializeField] GameObject ball;
    [SerializeField] GameObject gameUI, menuUI, scoreboardUI;
    [SerializeField] TMP_Text highScore;
    [SerializeField] LoseManager highScoreIntern;


    private void Awake()
    {


        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) InitializeFirebase();
            else print($"No se pueden resolver todas las dependencias de Firebase: {dependencyStatus}");
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
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
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
            UIManager.instance.LoginScreen();
        });
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

            UIManager.instance.RemoveAuth();
            gameUI.SetActive(true);
            menuUI.SetActive(true);
            ball.SetActive(true);
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
                    UserProfile profile = new UserProfile { DisplayName = username };

                    var ProfileTask = user.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate :  () => ProfileTask.IsCompleted);

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
                        UIManager.instance.LoginScreen();
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
}
