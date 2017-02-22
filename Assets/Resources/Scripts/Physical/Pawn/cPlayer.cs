using UnityEngine;
using System.Collections;

public partial class cPlayer : cPawn {
    public cInventory inventory;
    public cCamera cameraView;
    public float viewDistance;
    public Texture healthIcon;
    public Rect healthIconDisplay = new Rect(Screen.width-64, 0, 32, 32);
    public Rect healthValueDisplay = new Rect(Screen.width-32, 0, 32, 32);

    protected override void Start() {
        base.Start();

        SetMoveType(enumMoveType.Linear);
        inventory = Instantiate(inventory);
        cameraView = Instantiate(cameraView);
        cameraView.viewDist = viewDistance;
        inventory.owner = this;
    }

    private void OnGUI() {
        healthIconDisplay.position = new Vector2(Screen.width - 64f, 0);
        healthValueDisplay.position = new Vector2(Screen.width - 32f, 0);
        GUI.DrawTexture(healthIconDisplay, healthIcon, ScaleMode.StretchToFill, true, 1.0f);
        GUI.Label(healthValueDisplay, "" + health);
    }

    protected override IEnumerator RunMovePattern() {

        // Do not run calculations until the player is done moving
        while ( bIsMoving ) { yield return null; }

        // If the player is dead, go ahead and skip their turn
        if ( health < 1 ) {
            // End of turn
            bTurnInProgress = false;
            yield break;
        }

        // Now, wait for the player to make a legal move into an empty nearby space
        bool bMOVE_BLOCKED;
        do {
            int HORIZONTAL;
            int VERTICAL;
            bool TRIED_BOMB = false;

            // First, grab the player's directional input. Cannot move diagonally.
            do {
                VERTICAL = (int)(Input.GetAxisRaw("Vertical"));
                HORIZONTAL = (int)(Input.GetAxisRaw("Horizontal"));

                // If the player presses SPACE, skip turns
                if ( Input.GetKey(KeyCode.Space) ) {
                    // End of turn
                    bTurnInProgress = false;
                    yield break;
                }
                // If the player presses "F", try and activate a bomb
                else if ( Input.GetKeyDown(KeyCode.F) && !TRIED_BOMB ) {
                    TRIED_BOMB = true;
                    cItem BOMB_ITEM = inventory.FindItem<cPowderBomb>();
                    if ( BOMB_ITEM != null ) {
                        // If the player has at least one key, use it to unlock the door, and allow the player through
                        cPowderBomb BOMB = BOMB_ITEM.GetComponent<cPowderBomb>();
                        if ( BOMB.charges > 0 ) {
                            BOMB.Activate();
                            yield return new WaitForSeconds(moveSpeed);

                            // End of turn
                            bTurnInProgress = false;
                            yield break;
                        }
                    }
                    print("You have no bombs!");
                }
                yield return null;
            } while ( HORIZONTAL == 0 && VERTICAL == 0 );
            if ( HORIZONTAL != 0 ) { VERTICAL = 0; }

            // With player's input, determine if objects in selected adjacent cell are blocking. 
            // Only one blocking object is required to count as blocking
            Vector2 DESIRED_MOVE = new Vector2(HORIZONTAL, VERTICAL) * 0.32f;
            bMOVE_BLOCKED = CheckBlockingAdjacent(DESIRED_MOVE);

            // If there were no blocking objects in the adjacent cell, the player can move there.
            if ( !bMOVE_BLOCKED ) {
                StartMove(DESIRED_MOVE, moveSpeed);
                while ( bIsMoving ) { yield return null; }

                // End of turn
                bTurnInProgress = false;
                yield break;
            }

            // Repeat loop until the player makes a move into an open tile
            yield return null;
        } while (bMOVE_BLOCKED);
    }
}

public partial class cPlayer {
    public override void TakeDamage(cPhysicalObject INSTIGATOR, int DAMAGE, enumDamageType TYPE = enumDamageType.Normal, Vector2 DIRECTION = default(Vector2)) {
        health -= DAMAGE;
        print("OUCH! " + INSTIGATOR.name + " hit you for "+DAMAGE+" damage!");
        if ( health <= 0 ) {
            StartCoroutine(Died(INSTIGATOR));
        }
    }

    public virtual IEnumerator Died(cPhysicalObject INSTIGATOR) {
        print("You have been killed by "+INSTIGATOR.name);
        yield return new WaitForSeconds(3f);
        Application.LoadLevel(Application.loadedLevel);
        yield break;
    }

    virtual protected void OnTriggerEnter2D(Collider2D OTHER) {
        cItem ITEM_OTHER = OTHER.GetComponent<cItem>();
        if (ITEM_OTHER != null) {
            inventory.PickUp(ITEM_OTHER);
        }
    }
}