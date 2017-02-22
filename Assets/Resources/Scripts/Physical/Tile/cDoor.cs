using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cDoor : cTile {

	public bool bLocked;

    public override bool InteractiveBlock(cPhysicalObject OTHER, List<enumBlockingType> TRAVERSIBLE_BLOCKS) {
        // If the door is unlocked, actors will not be blocked by it.
        // Otherwise, unless the other actor is a player with one or more keys in their inventory, the door will remain locked

        // If unlocked, do not block
        if ( !bLocked ) { return false; }

        // Otherwise, request a key to unlock the door
        else {
            // Only a player actor can hold a key. Ignore the actor unless it is a player.
            cPlayer PLAYER_OTHER = OTHER.GetComponent<cPlayer>();
            if ( PLAYER_OTHER != null ) {                
                // Check the player's inventory for key items
                cItem KEY_RING_ITEM = PLAYER_OTHER.inventory.FindItem<cKey>();
                if ( KEY_RING_ITEM != null ) {
                    // If the player has at least one key, use it to unlock the door, and allow the player through
                    cKey KEY_RING = KEY_RING_ITEM.GetComponent<cKey>();
                    if ( KEY_RING.charges > 0 ) {
                        print("You used a key to unlock the door.");
                        KEY_RING.Activate();
                        Unlock();
                        return false;
                    }
                }
                print("The door is locked. You need a key!");
            }
        }
        // Default case is to block unless a non-blocking case is met.
        return true;
    }
    private void Unlock() {
        bLocked = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
