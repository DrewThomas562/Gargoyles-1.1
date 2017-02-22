using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cEvilStatue : cPawn {
    private enum enumFirstMove {
        Horizontal,
        Vertical
    }
    private enum enumHorizontal {
        Left,
        Center,
        Right
    }
    private enum enumVertical {
        Top,
        Center,
        Bottom
    }

    [SerializeField]
    private enumFirstMove eFirstMove;
    [SerializeField]
    private enumHorizontal preferenceHorizontal;
    [SerializeField]
    private enumVertical preferenceVertical;
    // TL|TC|TR
    // CL|CC|CR
    // BL|BC|BR

    protected override void Start() {
        base.Start();
        SetMoveType(enumMoveType.Quadratic);
    }

    virtual protected void OnTriggerEnter2D(Collider2D OTHER) {
        cPlayer PLAYER_OTHER = OTHER.GetComponent<cPlayer>();
        if ( PLAYER_OTHER != null ) {
            PLAYER_OTHER.TakeDamage(GetComponent<cPhysicalObject>(), 100, enumDamageType.Deathtouch);
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

    protected override IEnumerator RunMovePattern() {
        // Gargoyles try to move directly to the player
        // They can make 1 horizontal and 1 vertical move per turn, starting with their first move direction if possible
        // They cannot make 2 horizontal or 2 vertical moves in one turn
        // If direct line of sight is blocked, they will attempt to make diagonal moves toward the player in preferred directions

        // Do not run calculations until the gargoyle is done moving
        while ( bIsMoving ) { yield return null; }

        // First, figures out where the player is relative to the Gargoyle
        Vector2 TARGET_DESTINATION = FindObjectOfType<cPlayer>().movingTo-(Vector2)transform.position;

        // Then determine best case moves for getting to the player
        Vector2 HORIZONTAL_MOVE;
        Vector2 VERTICAL_MOVE;
        bool bSKIP_HORIZONTAL = false;
        bool bSKIP_VERTICAL = false;

        if ( TARGET_DESTINATION.x > 0.1 ) { HORIZONTAL_MOVE = new Vector2(0.32f, 0); }
        else if ( TARGET_DESTINATION.x < -0.1 ) { HORIZONTAL_MOVE = new Vector2(-0.32f, 0); }
        else { HORIZONTAL_MOVE = new Vector2(0,0); bSKIP_HORIZONTAL = true; }

        if ( TARGET_DESTINATION.y > 0.1 ) { VERTICAL_MOVE = new Vector2(0, 0.32f); }
        else if ( TARGET_DESTINATION.y < -0.1 ) { VERTICAL_MOVE = new Vector2(0, -0.32f); }
        else { VERTICAL_MOVE = new Vector2(0, 0); bSKIP_VERTICAL = true; }

        // No move case, skips their turn
        if ( bSKIP_HORIZONTAL && bSKIP_VERTICAL ) {
            bTurnInProgress = false;
            yield break;
        }
        
        // Orthogonal move case, the Gargoyle will attempt a diagonal move only if the single orthogonal move is blocked.
        if ( bSKIP_HORIZONTAL || bSKIP_VERTICAL ) {
            Vector2 ORTHOGONAL_TILE = HORIZONTAL_MOVE + VERTICAL_MOVE;
            Vector2 OVERRIDE_MOVE;
            // If the orthogonal move is blocked
            if ( CheckBlockingAdjacent(ORTHOGONAL_TILE) ) {
                // Check if the move can be maximized by overriding a skipped move, and moving diagonally toward the player
                while ( true ) { // NOTE: This is a breaking loop, intended to break before repeating.
                    // Horizontal case
                    if ( bSKIP_HORIZONTAL ) {
                        if ( preferenceHorizontal == enumHorizontal.Center ) { break; }
                        else if ( preferenceHorizontal == enumHorizontal.Left ) {
                            // Check for empty tiles to the left of the current position, and after a vertical move
                            OVERRIDE_MOVE = new Vector2(-0.32f, 0);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                // Tiles to the left are empty. Choose that override.
                                bSKIP_HORIZONTAL = false;
                                HORIZONTAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                            // If blocked off, check for empty tiles to the right of those positions
                            OVERRIDE_MOVE = new Vector2(0.32f, 0);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                // Tiles to the right are empty. Choose that override.
                                bSKIP_HORIZONTAL = false;
                                HORIZONTAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                        }
                        else {
                            OVERRIDE_MOVE = new Vector2(0.32f, 0);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_HORIZONTAL = false;
                                HORIZONTAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                            OVERRIDE_MOVE = new Vector2(-0.32f, 0);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_HORIZONTAL = false;
                                HORIZONTAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                        }
                    }
                    // Vertical case
                    if ( bSKIP_VERTICAL ) {
                        if ( preferenceVertical == enumVertical.Center ) { break; }
                        else if ( preferenceVertical == enumVertical.Bottom ) {
                            OVERRIDE_MOVE = new Vector2(0, -0.32f);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_VERTICAL = false;
                                VERTICAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                            OVERRIDE_MOVE = new Vector2(0, 0.32f);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_VERTICAL = false;
                                VERTICAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                        }
                        else {
                            OVERRIDE_MOVE = new Vector2(0, 0.32f);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_VERTICAL = false;
                                VERTICAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                            OVERRIDE_MOVE = new Vector2(0, -0.32f);
                            if ( !CheckBlockingAdjacent(OVERRIDE_MOVE) && !CheckBlockingAdjacent(ORTHOGONAL_TILE + OVERRIDE_MOVE) ) {
                                bSKIP_VERTICAL = false;
                                VERTICAL_MOVE = OVERRIDE_MOVE;
                                break;
                            }
                        }
                    }
                    break;
                }   // OVERRIDE LOOP
            }
        }
        // Diagonal move case, destination tile is the sum, HORIZONTAL_MOVE+VERTICAL_MOVE

        // Make moves to move toward the desired cell
        Vector2 PREFERRED_MOVE;
        Vector2 ALTERNATE_MOVE;
        bool bSKIP_PREFERRED;
        bool bSKIP_ALTERNATE;
        bool bMOVE_BLOCKED = false;

        // The first move will be made dependent on first move preference, one will be horizontal, the other vertical
        if ( eFirstMove == enumFirstMove.Horizontal ) {
            PREFERRED_MOVE = HORIZONTAL_MOVE;
            bSKIP_PREFERRED = bSKIP_HORIZONTAL;

            ALTERNATE_MOVE = VERTICAL_MOVE;
            bSKIP_ALTERNATE = bSKIP_VERTICAL;
        }
        else {
            PREFERRED_MOVE = VERTICAL_MOVE;
            bSKIP_PREFERRED = bSKIP_VERTICAL;

            ALTERNATE_MOVE = HORIZONTAL_MOVE;
            bSKIP_ALTERNATE = bSKIP_HORIZONTAL;
        }


        // First, try and move in the preferred direction
        bMOVE_BLOCKED = CheckBlockingAdjacent(PREFERRED_MOVE);
        if ( !bSKIP_PREFERRED && !bMOVE_BLOCKED ) {
            StartMove(PREFERRED_MOVE, moveSpeed);
            while ( bIsMoving ) { yield return null; }

            // Second phase move in remaining direction
            bMOVE_BLOCKED = CheckBlockingAdjacent(ALTERNATE_MOVE);
            if ( !bSKIP_ALTERNATE && !bMOVE_BLOCKED ) {
                StartMove(ALTERNATE_MOVE, moveSpeed);
                while ( bIsMoving ) { yield return null; }
            }

            // End of turn
            bTurnInProgress = false;
            yield break;
        }

        // Preferred move is blocked, try the alternate move
        bMOVE_BLOCKED = CheckBlockingAdjacent(ALTERNATE_MOVE);
        if ( !bSKIP_ALTERNATE && !bMOVE_BLOCKED ) {
            StartMove(ALTERNATE_MOVE, moveSpeed);
            while ( bIsMoving ) { yield return null; }

            // Second phase move in remaining direction
            bMOVE_BLOCKED = CheckBlockingAdjacent(PREFERRED_MOVE);
            if ( !bSKIP_PREFERRED && !bMOVE_BLOCKED ) {
                StartMove(PREFERRED_MOVE, moveSpeed);
                while ( bIsMoving ) { yield return null; }
            }

            // End of turn
            bTurnInProgress = false;
            yield break;
        }

        // Both moves are blocked. Gargoyle is unable to move this turn.
        bTurnInProgress = false;
        yield break;
    }
}