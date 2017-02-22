using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cBreakableWall : cTile {
    public int health;
    public enumDamageType weakness;

    public override void TakeDamage(cPhysicalObject INSTIGATOR, int DAMAGE, enumDamageType TYPE = enumDamageType.Normal, Vector2 DIRECTION = default(Vector2)) {
        if ( TYPE != weakness ) { print(this + " cannot be harmed by that damage type."); }
        else {
            health -= DAMAGE;
            if ( health <= 0 ) {
                print(this + " has broken!");
                Destroy(gameObject);
            }
            else {
                print(this + " is beginning to crumble");
            }
        }
    }
}
