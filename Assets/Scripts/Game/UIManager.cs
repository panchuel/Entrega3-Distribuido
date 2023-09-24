using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject loginUI, registerUI, forgotUI, authUI;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
    }

    public void LoginScreen()
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        forgotUI.SetActive(false);
    }

    public void RegisterScreen()
    {
        registerUI.SetActive(true);
        loginUI.SetActive(false);
    }

    public void ForgotPasswordScreen()
    {
        forgotUI.SetActive(true);
        loginUI.SetActive(false);
    }

    public void RemoveAuth()
    {
        authUI.SetActive(false);
    }
}
