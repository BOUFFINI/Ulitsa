using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxT_PlayerControler : MonoBehaviour
{
    [Header("Déplacements - Gauche/Droite")]
    public float maxSpeed = 10f, maxAcceleration = 10f;
    public AnimationCurve dashCurveAcceleration = AnimationCurve.Constant(0, 0.25f, 1);

    [Header("Mouvement Vertical")]
    public float gravity = 9.81f, gravityBackUp;
    public AnimationCurve jumpCurve = AnimationCurve.Constant(0, 0.1f, 1);
    public AnimationCurve levitationOnCurve = AnimationCurve.Constant(0, 0.20f, 1);
    public AnimationCurve levitationCurve = AnimationCurve.Constant(0, 0.20f, 1);
    public float jumpReleaseMultiplayer = 3f, levitationReleaseMultiplayer = 3f;

    [Header("Touches Correspondantes")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode dash = KeyCode.E;
    public KeyCode levitation = KeyCode.A;

    [System.NonSerialized]
    // Inputs
    public bool jumpKey, jumpKeyDown, jumpKeyUp, dashKey, dashKeyDown, levitationKey, levitationKeyDown;
    public bool jumpKeyDownBeforeUpdate, dashKeyDownBeforeUpdate, levitationKeyDownBeforeUpdate;

    // Divers
    public Vector3 movementVector, displacement, jumpVector, dashVector,playerJoystickLeft, levitationVector;
    public Vector3 velocity;
    public FoxT_Raycast raycaster;
    public bool isGrounded { get { return raycaster.isGrounded; } }
    public bool isJumping, isDashing, isLevitating;
    float timeSinceJumped, timeSinceDashed, timeSinceLevitate;
    public float movementThreshold = 0.0015f;
    public int dashAlloewedLeft = 1;
    public int lastXAxeDirection = 1;
    public bool levitationTransitionOn, levitationPositionAbove;
    /*public Transform self;*/

    void Start()
    {
        
    }

    void Update()
    {
        InputUpdate();
        JumpUpdate();
        MouvementUpdate();
        PostMovementJumpUpdate();
    }

    void InputUpdate()
    {
        InputHandler();

        // Joystick Gauche
        playerJoystickLeft.x = Input.GetAxis("Horizontal");
        if (playerJoystickLeft.x != 0 && playerJoystickLeft.x < 0) lastXAxeDirection = -1;
        if (playerJoystickLeft.x != 0 && playerJoystickLeft.x > 0) lastXAxeDirection = 1;
        playerJoystickLeft.y = 0f;
        MouvementStart();

        // Bouton A
        if (jumpKeyDown) TestJump();

        //Dash
        if (dashKeyDown) DashStart();

        //Levitation
        if (levitationKeyDown) LevitationTest();
    } //à faire = Faire fonctionner une manette type Xbox (pour l'instant, seul le clavier fonctionne).

    void MouvementStart()
    {
        // Gauche - Droite
        velocity = new Vector2(playerJoystickLeft.x, 0f) * maxSpeed;
        Vector2 desiredVelocity = velocity * maxAcceleration;
        displacement = desiredVelocity * Time.deltaTime;

    }

    void MouvementUpdate()
    {
        movementVector.x = displacement.x;
        DashUpdate();
        LevitationUpdate();

        Move(movementVector);
    }

    void PostMovementJumpUpdate()
    {
        // si on touche le sol, le saut est fini aussi
        if (isGrounded)
        {
            isJumping = false;
            timeSinceJumped = 0;
        }
    }

    void TestJump()
    {
        if (!isGrounded) return;
        if (isJumping) return;
        JumpStart();
    }

    void JumpStart()
    {
        isJumping = true;
        jumpVector = new Vector2(0f, 0f);
        timeSinceJumped = 0;
    }

    void JumpUpdate()
    {
        movementVector.y = -gravity;
        if (!isJumping) return;
        float releaseMultiplayer = 1f;
        if (!jumpKey) releaseMultiplayer = jumpReleaseMultiplayer;
        timeSinceJumped += Time.deltaTime * releaseMultiplayer;
        jumpVector.y = jumpCurve.Evaluate(timeSinceJumped);
        movementVector.y += jumpVector.y;
    }

    void DashStart()
    {
        if (isDashing) return;
        timeSinceDashed = 0f;
        isDashing = true;

    }

    void DashUpdate()
    {
        if (!isDashing) return;

        if (dashAlloewedLeft > 0)
        {
            timeSinceDashed += Time.deltaTime;
            dashVector.x = dashCurveAcceleration.Evaluate(timeSinceDashed) * Mathf.Sign(lastXAxeDirection);
            movementVector.x += dashVector.x;

            if (timeSinceDashed > dashCurveAcceleration.keys[dashCurveAcceleration.keys.Length - 1].time)
            {
                isDashing = false;
                dashAlloewedLeft--;
            }
        }
        if (isGrounded) dashAlloewedLeft = 1;

    }

    void LevitationTest()
    {
        if (isJumping) return;
        if (levitationTransitionOn) return;
        LevitationStart();
    }

    void LevitationStart()
    {
        isLevitating = true;
        if (levitationPositionAbove) levitationPositionAbove = false;
        timeSinceLevitate = 0;
        levitationVector = new Vector2(0, 0);
    }

    void LevitationUpdate()
    {
        if (!isLevitating) return;
        if (!levitationTransitionOn && !levitationPositionAbove && isGrounded) levitationTransitionOn = true;
        if (gravity != 0) gravityBackUp = gravity;
        gravity = 0;
        if (!levitationPositionAbove && levitationTransitionOn)
        {
            timeSinceLevitate += Time.deltaTime;
            levitationVector.y = levitationOnCurve.Evaluate(timeSinceLevitate);
            movementVector.y += levitationVector.y;

            if (timeSinceLevitate > levitationOnCurve.keys[levitationOnCurve.keys.Length - 1].time)
            {
                levitationTransitionOn = false;
                levitationPositionAbove = true;
                timeSinceLevitate = 0f;
            }
        }
        else if (levitationPositionAbove && !levitationTransitionOn)
        {
            levitationVector.y = levitationCurve.Evaluate(timeSinceLevitate);
            timeSinceLevitate += Time.deltaTime;
            movementVector.y += levitationVector.y;

            if (timeSinceLevitate > levitationCurve.keys[levitationCurve.keys.Length - 1].time)
            {
                timeSinceLevitate = 0f;
            }
        }
        else if (!levitationPositionAbove && !levitationTransitionOn && !isGrounded)
        {
            gravity = gravityBackUp;
            isLevitating = false;
        }
    }

    void Move(Vector3 movement)
    {
        movement.y = raycaster.RaycastBoxVertical(movement.y);
        if (Mathf.Abs(movement.y) < movementThreshold) movement.y = 0f;
        movement.x = raycaster.RaycastBoxHorizontal(movement.x);
        if (Mathf.Abs(movement.x) < movementThreshold) movement.x = 0f;

        transform.Translate(movement);

    }

    void InputHandler()
    {
        jumpKey = Input.GetKey(jump);

        if (jumpKey && !jumpKeyDownBeforeUpdate)
        {
            jumpKeyDown = true;
            jumpKeyDownBeforeUpdate = true;
        }
        else if (jumpKey && jumpKeyDownBeforeUpdate) jumpKeyDown = false;
        else if (!jumpKey && jumpKeyDownBeforeUpdate)
        {
            jumpKeyUp = true;
            jumpKeyDownBeforeUpdate = false;
        }
        else if(!jumpKey && !jumpKeyDownBeforeUpdate) jumpKeyUp = false;

        dashKey = Input.GetKey(dash);

        if (dashKey && !dashKeyDownBeforeUpdate)
        {
            dashKeyDown = true;
            dashKeyDownBeforeUpdate = true;
        }
        else if (dashKey && dashKeyDownBeforeUpdate) dashKeyDown = false;
        else if (!dashKey && dashKeyDownBeforeUpdate) dashKeyDownBeforeUpdate = false;

        levitationKey = Input.GetKey(levitation);

        if (levitationKey && !levitationKeyDownBeforeUpdate)
        {
            levitationKeyDown = true;
            levitationKeyDownBeforeUpdate = true;
        }
        else if (levitationKey && levitationKeyDownBeforeUpdate) levitationKeyDown = false;
        else if (!levitationKey && levitationKeyDownBeforeUpdate) levitationKeyDownBeforeUpdate = false;

    }
}
