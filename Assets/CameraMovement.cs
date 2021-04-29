using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public GameObject mainCam;
    [Range(1f, 100f)]
    public float moveSpeed;
    void Start()
    {
        mainCam = this.gameObject;    
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Debug.Log(moveDir);
        mainCam.transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;
    }
}
