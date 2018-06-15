﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Rigidbody rb;
    private float rotationSpeed = .2f;
    [SerializeField] private float climbSpeed;

    //shimy pipe
    Vector3 mySide;
    Vector3 farSide;

    private void Start(){
        rb = GetComponent<Rigidbody>();
    }

    public void FreeMovement(Vector3 movement, float speed){
        if (PlayerManager.instance.currentState != PlayerManager.PlayerState.Attacking){
            if (movement.x != 0 && movement.z != 0)
                speed -= speed / 3;
            
            Vector3 dir = PlayerManager.instance.playerCam.transform.position - transform.position;
            dir.y = 0;
            Quaternion rot = Quaternion.LookRotation(-dir);

            movement = PlayerManager.instance.playerCam.transform.TransformDirection(movement);
            movement.y = 0;
            Vector3 v = rb.velocity;

            v.x = movement.x * speed;
            v.z = movement.z * speed;
            rb.velocity = v;
       
            if (movement != Vector3.zero){
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), rotationSpeed);
                PlayerManager.instance.anim.SetBool("isMoving", true);
            }else
                PlayerManager.instance.anim.SetBool("isMoving", false);
        }
        else{
            PlayerManager.instance.anim.SetBool("isMoving", false);
        }
    }

    public void Jump(float jumpHeight){
        PlayerManager.instance.rb.velocity = Vector3.up * jumpHeight;
        PlayerManager.instance.anim.SetBool("isGrounded", false);
        PlayerManager.instance.anim.Play("Jump");
    }

    public IEnumerator ClimbLadder(Vector3 bottomPos, Vector3 topPos, Vector3 endPos){
        PlayerManager.instance.currentState = PlayerManager.PlayerState.Traversing;

        Vector3 start;
        Vector3 end;
        if (Vector3.Distance(transform.position, bottomPos) < Vector3.Distance(transform.position, topPos)){
            start = bottomPos;
            end = topPos;
        }
        else{
            start = topPos;
            end = bottomPos;
        }

        //play ready anim
        rb.isKinematic = true;
        transform.position = start;

        //move up
        while (Vector3.Distance(transform.position, end) > .1f){
            PlayerManager.instance.anim.SetBool("isClimbing",true);
            PlayerManager.instance.anim.speed = 1.5f;
            transform.position = Vector3.MoveTowards(transform.position, end, climbSpeed * Time.deltaTime);

            //rotate towards the ladder
            Vector3 tp = transform.position - bottomPos;
            tp.y = transform.position.y;
            Quaternion rot = Quaternion.LookRotation(-PlayerManager.instance.ladder.transform.forward);
            transform.rotation = rot;

            yield return new WaitForEndOfFrame();
        }

        PlayerManager.instance.anim.SetBool("isClimbing", false);
        PlayerManager.instance.anim.speed = 1f;

        if (end == topPos){
            while (Vector3.Distance(transform.position, endPos) > .1f){
                transform.position = Vector3.MoveTowards(transform.position, endPos, climbSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        rb.isKinematic = false;
        yield return new WaitForEndOfFrame();
        PlayerManager.instance.currentState = PlayerManager.PlayerState.FreeMovement;
        PlayerManager.instance.ladder = null;
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator ZipLine(){

        yield return null;
    }

    public IEnumerator ShimyPipeStart(GameObject pipe){
        rb.isKinematic = true;

        ShimyPipe pipeInfo = pipe.GetComponent<ShimyPipe>();
        if(Vector3.Distance(transform.position, pipeInfo.sideA.position) < Vector3.Distance(transform.position, pipeInfo.sideB.position)){
            mySide = pipeInfo.sideA.position;
            farSide = pipeInfo.sideB.position;
        }else{
            mySide = pipeInfo.sideB.position;
            farSide = pipeInfo.sideA.position;
        }

        while(Vector3.Distance(transform.position, mySide)>.1f){
            transform.position = Vector3.Lerp(transform.position, mySide, .1f);
            Quaternion rot = Quaternion.LookRotation(farSide - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, .5f);
            yield return new WaitForEndOfFrame();
        }
        PlayerManager.instance.currentState = PlayerManager.PlayerState.Traversing;
        yield return null;
    }

    public void ShimyPipe(Vector3 movement, ShimyPipe pipe){
        if(movement.z > 0){
            transform.position = Vector3.MoveTowards(transform.position, farSide, 5 * Time.deltaTime);
            Quaternion rot = Quaternion.LookRotation(farSide - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, .5f);
        }else if(movement.z < 0){
            transform.position = Vector3.MoveTowards(transform.position, mySide, 5 * Time.deltaTime);
            Quaternion rot = Quaternion.LookRotation(mySide - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, .5f);
        }
    }

    public void EndShimy(){
        PlayerManager.instance.currentState = PlayerManager.PlayerState.FreeMovement;
        rb.isKinematic = false;
    }
}