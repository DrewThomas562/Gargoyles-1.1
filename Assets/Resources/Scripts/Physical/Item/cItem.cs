using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cItem : cPhysicalObject {
    public int charges;
    public Texture pickupIcon;
    [HideInInspector]
    public GameObject owner;

    public enum enumItemState {
        Pickup,
        Inactive,
        Active
    }
    [HideInInspector]
    public enumItemState eItemState;

    // Typically, the first time a new item is found, that item stack is added to the other actor's inventory
    // The item becomes inactive and disappears from view
    virtual public void FirstPickup(GameObject OWNER) {
        owner = OWNER;
        SetItemState(enumItemState.Inactive);
    }

    // Typically, consecutive pickups add to the number of charges/uses in the player's inventory
    // The item becomes inactive and disappears from view
    virtual public void Stack(cItem PICKUP) {
        charges += PICKUP.charges;
        PICKUP.SetItemState(enumItemState.Inactive);
    }

    // When part of an inventory belt, the owner can select and activate items
    virtual public void Select() {
    }
    virtual public void Activate() {
        charges--;
        print("Activating item, charges remaining: "+charges);
        eItemState = enumItemState.Active;
    }

    // Deal with item state changes here
    virtual protected void SetItemState(enumItemState NEW_STATE) {
        if ( NEW_STATE == enumItemState.Inactive ) {
            eItemState = enumItemState.Inactive;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
