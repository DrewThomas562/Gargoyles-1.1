using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class cCrab : cPhysicalObject
{
    public enum enumDirection
    {
        North,
        East,
        South,
        West
    }
    public enum enumTurnDirection
    {
        Left,
        Right
    }
    public enumDirection eDirection;
    public enumTurnDirection eTurnDirection;

    protected override void Start()
    {
        base.Start();
        setMoveType(enumMoveType.Quadratic);
    }

    public override IEnumerator FinishTurn()
    {
        Vector2 ADJACENT = new Vector2();
        Collider2D[] DETECTEDOBJECTS;
        bool bMOVE_BLOCKED = false;
        // The turtle will move straight if not blocked
        // If blocked, it will try and turn in its preferred direction
        // If turning in its preferred direction meets another block, it will change preferred directions, and try the other direction
        // If unable to turn left or right, the turtle will try and go backwards, with the newly preferred turning direction
        // Finally if unable to move, the turn will be skipped
        int BLOCKED_COUNT = 0;

        while (true)
        {
            if (!bIsMoving)
            {
                if (BLOCKED_COUNT == 0)
                {
                    ADJACENT = GetAdjacent(eDirection);
                }
                else if (BLOCKED_COUNT == 1)
                {
                    print(BLOCKED_COUNT);
                    Turn(eTurnDirection);
                    ADJACENT = GetAdjacent(eDirection);
                }
                else if (BLOCKED_COUNT == 2)
                {
                    ChangePreferredTurnDirection();
                    Turn(eTurnDirection);
                    Turn(eTurnDirection);
                    ADJACENT = GetAdjacent(eDirection);
                }
                else if (BLOCKED_COUNT == 3)
                {
                    Turn(eTurnDirection);
                    ADJACENT = GetAdjacent(eDirection);
                }
                else {
                    yield return "end";
                    yield break;
                }
                DETECTEDOBJECTS = DetectObjects(ADJACENT);

                bMOVE_BLOCKED = false; // DEFAULT until blocked
                yield return null;
                foreach (Collider2D OTHER in DETECTEDOBJECTS)
                {
                    cTile TILE_OTHER = OTHER.GetComponent<cTile>();
                    if (TILE_OTHER != null)
                    {
                        bMOVE_BLOCKED = TILE_OTHER.CheckIfBlocked(GetComponent<cPhysicalObject>(), traversibleTerrain);
                    }
                }
                if (!bMOVE_BLOCKED)
                {
                    startMove(ADJACENT - (Vector2)transform.position, 0.2f);
                    while (bIsMoving) { yield return null; }
                    yield return "end";
                    yield break;
                }
                else { BLOCKED_COUNT++; }
            }
        }    //while loop closed    
    }
}

public partial class cTurtle
{

    void ChangePreferredTurnDirection()
    {
        if (eTurnDirection == enumTurnDirection.Left) { eTurnDirection = enumTurnDirection.Right; }
        else { eTurnDirection = enumTurnDirection.Left; }
    }
    void Turn(enumTurnDirection TURN_DIRECTION)
    {
        if (TURN_DIRECTION == enumTurnDirection.Left)
        {
            eDirection -= 1;
            rigidbody.rotation += 180;
        }
        else {
            eDirection += 1;
            rigidbody.rotation -= 180;
        }
        if (eDirection > enumDirection.West) { eDirection = enumDirection.North; }
        else if (eDirection < enumDirection.North) { eDirection = enumDirection.West; }
    }
    Vector2 GetAdjacent(enumDirection DIRECTION)
    {
        if (DIRECTION > enumDirection.West) { DIRECTION = enumDirection.North; }
        else if (DIRECTION < enumDirection.North) { DIRECTION = enumDirection.West; }

        if (DIRECTION == enumDirection.North)
            return (Vector2)transform.position + new Vector2(0, 1) * 0.32f;
        else if (DIRECTION == enumDirection.East)
            return (Vector2)transform.position + new Vector2(1, 0) * 0.32f;
        else if (DIRECTION == enumDirection.South)
            return (Vector2)transform.position + new Vector2(0, -1) * 0.32f;
        else //if ( DIRECTION == enumDirection.West )
            return (Vector2)transform.position + new Vector2(-1, 0) * 0.32f;
    }

    virtual protected void OnTriggerEnter2D(Collider2D OTHER)
    {
        cPlayer PLAYER_OTHER = OTHER.GetComponent<cPlayer>();
        if (PLAYER_OTHER != null)
        {
            PLAYER_OTHER.TakeDamage(this, new cPlayer().health / 2);
        }
    }
}