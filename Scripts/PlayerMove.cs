using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody2D ch;
    private BoxCollider2D box;

    private Animator animator;

    public float xVelocity;
    public float jumpForce = 6f;
    public float jumpHoldForce = 1.9f;
    public float jumpHoldDuration = 0.1f;

    public float hangingJumpForce = 1.9f;

    [Header("状态")]
    public bool isOnGround;
    public bool isJump;
    public bool isHeadBlocked;
    public bool isHanging;
    public bool isAttackLight;
    public float attackLightDuartionTime = 0.6f;
    public float attackLightTime;
    public int jumpCount = 0;

    float jumpTime;
    [Header("环境监测")]   
    public LayerMask enemyLayer;
    public LayerMask groundLayer;
    public float leftFootOffsetX = 0.5f;
    public float rightFootOffsetX = 0.9f;
    public float footOffsetY = 0.7f;
    public float headClearance = 0.5f;
    public float groundDistance = 0.2f;
    float playerHeight;
    public float eyeHeight = 0.3f;
    public float grabDistance = 0.4f;
    public float reachOffset = 0.7f;

    bool jumpPressed;
    bool jumpHeld;
    bool crouchHeld;

    bool attackLight;

    Vector2 colliderStandSize;
    Vector2 colliderStandOffset;


    // Start is called before the first frame update
    void Start()
    {
        ch = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        playerHeight = box.size.y / 2;
        colliderStandOffset = box.offset;
        colliderStandSize = box.size;

    }
    RaycastHit2D raycast(Vector2 offset , Vector2 rayDirection, float length , LayerMask layer) {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + offset ,rayDirection ,length ,layer);
        Color color = hit ? Color.red : Color.green;
        Debug.DrawRay(pos+ offset , rayDirection* length, color);
        return hit;
    }
    private void Update()
    {
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");    
        crouchHeld = Input.GetButton("Crouch");
        attackLight = Input.GetButtonDown("Fire1");
        
        
    }
    void attack(){
        if(attackLight) {
            attackLightTime = Time.time + attackLightDuartionTime;
            animator.SetBool("attackLight" ,true);
        }else {
            if(Time.time > attackLightTime) {
                animator.SetBool("attackLight" ,false);
            }
        }
    }
    void lightDamage(){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + new Vector3(0.3f , 0 , 0) ,2f,enemyLayer);
        foreach(Collider2D collider in colliders){
            print(collider.gameObject.name);
        }
    }
    
    void physicsCheck(){
        RaycastHit2D leftCheck = raycast(new Vector2(-leftFootOffsetX, -footOffsetY),Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightCheck = raycast(new Vector2(rightFootOffsetX, -footOffsetY),Vector2.down, groundDistance, groundLayer);
        if(leftCheck || rightCheck){
            isOnGround = true;
            isJump = false;
            jumpCount = 0;
        }
        else {
            isOnGround = false;
            isJump = true;  
        }
        RaycastHit2D headCheck = raycast(new Vector2(0f,playerHeight), Vector2.up, headClearance,groundLayer);
        if (headCheck) 
            isHeadBlocked = true;
        else isHeadBlocked  = false;

        float direction = transform.localScale.x;
        Vector2 grabDir = new Vector2(direction, 0f);

        RaycastHit2D blockedCheck = raycast(new Vector2(leftFootOffsetX  * direction, playerHeight), grabDir, grabDistance ,groundLayer);
        RaycastHit2D wallCheck = raycast(new Vector2(leftFootOffsetX  * direction, eyeHeight), grabDir, grabDistance ,groundLayer);
        RaycastHit2D ledgeCheck = raycast(new Vector2((leftFootOffsetX + 0.1f) * direction, playerHeight), Vector2.down ,grabDistance, groundLayer);

        if(!isOnGround && ch.velocity.y < 0f && ledgeCheck && wallCheck && !blockedCheck) {

            Vector2 pos = transform.position;
            pos.x += (wallCheck.distance-0.05f) * direction;
            pos.y -= ledgeCheck.distance;

            transform.position = pos;
            ch.bodyType = RigidbodyType2D.Static;
            isHanging = true;
        }
        
    }
    private void FixedUpdate(){
        physicsCheck();
        GroundMov();
        JumpMovement();
        attack();

    }
    private void JumpMovement(){
        if(isHanging && jumpPressed) {
            ch.bodyType = RigidbodyType2D.Dynamic;
            ch.AddForce(new Vector2(0f, hangingJumpForce), ForceMode2D.Impulse);
            animator.SetFloat("jump",Mathf.Abs(jumpForce));
            isHanging = false;
        }
        if(jumpPressed && jumpCount <1) {
            ch.AddForce(new Vector2(0f, jumpForce) , ForceMode2D.Impulse);
           
            jumpCount ++;
        }
        if(isJump) {
            animator.SetFloat("jump", jumpForce);
        }else{
            animator.SetFloat("jump", 0);
        }
    }
    private void GroundMov() {
        if(isHanging){
            if(crouchHeld){
                ch.bodyType = RigidbodyType2D.Dynamic;
                isHanging = false;
            }
            return;
        }
        xVelocity = Input.GetAxis("Horizontal");
        if(xVelocity != 0) {
            ch.velocity = new Vector2(xVelocity * speed , ch.velocity.y);
            animator.SetFloat("runnig",Mathf.Abs(xVelocity));
        }
        FilpDirction();
    } 
    void FilpDirction() {
        if(xVelocity < 0) {
            transform.localScale = new Vector3(-1 , 1,1);
            leftFootOffsetX = 0.9f;
            rightFootOffsetX = 0.5f;
        }
        if(xVelocity > 0) {
            transform.localScale = new Vector3(1 , 1,1);
            leftFootOffsetX = 0.5f;
            rightFootOffsetX = 0.9f;
           
        }
    }
}
