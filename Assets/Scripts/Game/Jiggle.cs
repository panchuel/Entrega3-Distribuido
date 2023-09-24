using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Jiggle : MonoBehaviour
{
    public static Jiggle Instance;
    [HideInInspector] public Rigidbody2D rb;
    [SerializeField] float jumpForce;
    public UnityEvent scoreIncreasser;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && rb.gravityScale != 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 wp = Camera.main.ScreenToWorldPoint(touch.position);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            if (touch.phase == TouchPhase.Began && GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchPos))
            {
                Jump();
                scoreIncreasser.Invoke();
            }
        }
    }

    void Jump()
    {
        rb.AddForce(new Vector2(Random.Range(30f,100f), jumpForce), ForceMode2D.Force);
    }

}
