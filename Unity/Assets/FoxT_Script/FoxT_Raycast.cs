using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FoxT_Raycast : MonoBehaviour
{
    public Transform self;
    public BoxCollider2D box;
    public LayerMask layerMask;
    public bool isGrounded;
    public float sign;
    public float skinWithdMultiplayer = 0.95f;

    public float RaycastBoxHorizontal(float movement)
    {
        RaycastHit2D result = Physics2D.BoxCast(
            self.position,
            new Vector2(box.size.x * self.lossyScale.x* skinWithdMultiplayer, box.size.y * self.lossyScale.y * skinWithdMultiplayer),
            0f,
            Vector2.right * Mathf.Sign(movement),
            Mathf.Abs(movement),
            layerMask);



        if (result.collider != null)
        {
            return 0f;
        }

        return movement;
    }

    public float RaycastBoxVertical(float movement)
    {
        RaycastHit2D result = Physics2D.BoxCast(
            self.position,
            new Vector2(box.size.x * self.lossyScale.x * skinWithdMultiplayer, box.size.y * self.lossyScale.y * skinWithdMultiplayer), 
            0f,
            Vector2.up * Mathf.Sign(movement),
            Mathf.Abs(movement),
            layerMask);
        Debug.DrawLine(self.localPosition, result.point, Color.red);


        if (result.collider != null)
        {
            float startPoint = self.position.y + (box.size.y * self.lossyScale.y * 0.5f * Mathf.Sign(movement));
            float newDistance = Mathf.Sign(movement) * Mathf.Abs(result.point.y - startPoint);
            isGrounded = true;
            return newDistance;
        }

        isGrounded = false;
        return movement;
    }
}
