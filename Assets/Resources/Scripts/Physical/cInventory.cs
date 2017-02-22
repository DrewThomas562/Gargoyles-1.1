using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cInventory : MonoBehaviour {
    public List<cItem> itemList;
    public int selected;
    public cPlayer owner;
    public Rect ChargeRect = new Rect(32, 0, 32, 32);
    public Rect DrawingRect = new Rect(0, 0, 32, 32);

    // Use this for initialization
    virtual protected void Start () {
        selected = 0;
    }

    void OnGUI() {
        //GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 80, 150, 30), "Current Scene: " + Application.loadedLevel);
        //if ( !ItemIcon ) {
        //    Debug.LogError("Assign a Texture in the inspector.");
        //    return;
        //}
        int i=0;
        foreach ( cItem ITEM in itemList ) {
            DrawingRect.position = new Vector2(0f,i*32f);
            ChargeRect.position = new Vector2(32f, i * 32f);
            GUI.DrawTexture(DrawingRect, ITEM.pickupIcon, ScaleMode.StretchToFill, true, 1.0f);
            GUI.Label(ChargeRect, ""+ITEM.charges);
            i++;
        }
    }

    virtual public cItem FindItem<T>() {
        T FoundItem;
        foreach (cItem ITEM in itemList) {
            FoundItem = ITEM.GetComponent<T>();
            if (FoundItem != null) { return ITEM; }
        }
        return null;
    }
    virtual public void PickUp(cItem PICKUP) {
        if (PICKUP.eItemState != cItem.enumItemState.Pickup) { return; }
        foreach (cItem item in itemList) {
            if (item.tag == PICKUP.tag) {
                item.Stack(PICKUP);
                return;
            }
        }
        AddItem(PICKUP);
    }
    virtual protected void AddItem(cItem PICKUP) {
        PICKUP.FirstPickup(owner.gameObject);
        itemList.Add(PICKUP);
        if (itemList.Count == 1) { Select(0); }
    }

    virtual protected void Select(int QUICK_SELECT) {
        selected = QUICK_SELECT;
        if (selected == itemList.Count) { selected = 0; }
        if (selected < 0) { selected = itemList.Count - 1; }
    }
    virtual protected void SelectNext() {
        selected += 1;
        if (selected == itemList.Count) { selected = 0; }
    }
    virtual protected void SelectPrevious() {
        selected -= 1;
        if (selected < 0) { selected = itemList.Count-1; }
    }
}
