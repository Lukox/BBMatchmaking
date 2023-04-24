using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    public float moveSpeed = 5.0f;    

    // Update is called once per frame
    private void Update()
    {
        if(!IsOwner) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);
        movement = movement.normalized;

        transform.position += movement * moveSpeed * Time.deltaTime;

    }
}
