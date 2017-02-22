using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class cTurtle : cPawn {
    public enum enumDirection {
        North,
        East,
        South,
        West
    }
    public enum enumTurnDirection {
        Left,
        Right
    }
    public enumDirection eDirection;
    public enumTurnDirection eTurnDirection;

    protected override void Start() {
        base.Start();
        SetMoveType(enumMoveType.Quadratic);
    }
}

public partial class cTurtle {

    virtual protected void OnTriggerEnter2D(Collider2D OTHER) {
        cPlayer PLAYER_OTHER = OTHER.GetComponent<cPlayer>();
        if ( PLAYER_OTHER != null ) {
            PLAYER_OTHER.TakeDamage(this, 50);
        }
    }
    public override void TakeDamage(cPhysicalObject INSTIGATOR, int DAMAGE, enumDamageType TYPE = enumDamageType.Normal, Vector2 DIRECTION = default(Vector2)) {
        health -= DAMAGE;
        print(this + " has been hit by " + INSTIGATOR + "... health remaining:" + health);
        if ( health <= 0 ) {
            StartCoroutine(Died(INSTIGATOR));
        }
    }
    public virtual IEnumerator Died(cPhysicalObject INSTIGATOR) {
        print(this + " has been killed by " + INSTIGATOR);
        Destroy(gameObject);
        yield break;
    }

    void ChangePreferredTurnDirection() {
        if ( eTurnDirection == enumTurnDirection.Left ) { eTurnDirection = enumTurnDirection.Right; }
        else { eTurnDirection = enumTurnDirection.Left; }
    }
    void Turn(enumTurnDirection TURN_DIRECTION) {
        if ( TURN_DIRECTION == enumTurnDirection.Left ) {
            eDirection -= 1;
            rigidbody.rotation += 90;
        }
        else {
            eDirection += 1;
            rigidbody.rotation -= 90;
        }
        if ( eDirection > enumDirection.West ) { eDirection = enumDirection.North; }
        else if ( eDirection < enumDirection.North ) { eDirection = enumDirection.West; }
    }
    Vector2 GetPositionFacing(enumDirection DIRECTION) {
        if ( DIRECTION > enumDirection.West ) { DIRECTION = enumDirection.North; }
        else if ( DIRECTION < enumDirection.North ) { DIRECTION = enumDirection.West; }

        if ( DIRECTION == enumDirection.North ) 
            return new Vector2(0, 1) * 0.32f;
        else if ( DIRECTION == enumDirection.East )
            return new Vector2(1, 0) * 0.32f;
        else if ( DIRECTION == enumDirection.South )
            return new Vector2(0, -1) * 0.32f;
        else //if ( DIRECTION == enumDirection.West )
            return new Vector2(-1, 0) * 0.32f;
    }

    protected override IEnumerator RunMovePattern() {

        // The turtle will move straight if not blocked
        // If blocked, it will try and turn in its preferred direction
        // If turning in its preferred direction meets another block, it will change preferred directions, and try the other direction
        // If unable to turn left or right, the turtle will try and go backwards, with the newly preferred turning direction
        // Finally if unable to move, the turn will be skipped

        // Do not run calculations until the turtle is done moving
        while ( bIsMoving ) { yield return null; }

        // First, the turtle checks to see if there is anything that is blocking him from moving forward.
        bool bMOVE_BLOCKED;

        for ( int ATTEMPT = 0; ATTEMPT < 4; ATTEMPT++ ) {
            // Try moving forward
            if ( ATTEMPT == 0 ) { }
            // Try turning in the preferred direction and moving forward
            else if ( ATTEMPT == 1 ) {
                Turn(eTurnDirection);
            }
            // Turn the other direction and try moving forward again (changing the preferred direction)
            else if ( ATTEMPT == 2 ) {
                ChangePreferredTurnDirection();
                Turn(eTurnDirection);
                Turn(eTurnDirection);
            }
            // Try moving back from where the turtle came from (retaining its newly preferred turning direction)
            else if ( ATTEMPT == 3 ) {
                Turn(eTurnDirection);
            }
            // The turtle is stuck. Turn back to the front, and give up the turn. (resets the preferred turning direction)
            else {
                ChangePreferredTurnDirection();
                Turn(eTurnDirection);
                Turn(eTurnDirection);

                // End of turn
                bTurnInProgress = false;
                yield break;
            }

            // Check to see if the adjacent cell is blocked by anything
            Vector2 DESIRED_MOVE = GetPositionFacing(eDirection);
            bMOVE_BLOCKED = CheckBlockingAdjacent(DESIRED_MOVE);

            // If there were no blocking objects in the adjacent cell, the turtle can move there.
            if ( !bMOVE_BLOCKED ) {
                StartMove(DESIRED_MOVE, moveSpeed);
                while ( bIsMoving ) { yield return null; }
                // End of turn
                bTurnInProgress = false;
                yield break;
            }
        }    //while loop closed    
    }
}