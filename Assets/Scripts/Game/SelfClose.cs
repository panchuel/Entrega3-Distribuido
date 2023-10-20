using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SelfClose : MonoBehaviour
{
    [SerializeField] GameObject parentObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => Destroy(parentObject));
    }
}
