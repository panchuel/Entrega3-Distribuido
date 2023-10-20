using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TitleHandler))]
public class TitleHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;

    public void SetString(string str)
    {
        title.text = str;
    }
}
