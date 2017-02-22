using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cProjectile : cPhysicalObject {
    // A projectile is spawned by the owner, and does damage on account of the instigator (may be the same as the owner)
    // Typically, the owner is not able to collide with the projectile until it first leaves thier collision box
    GameObject owner;
    GameObject instigator;
    public enumDamageType damageType;
    public int damage;
    public Vector2 velocity;
    public int lifeSpan;
    bool bCollideWithOwner;

    protected override void Start() {
        base.Start();
        SetMoveType(enumMoveType.Linear);
    }

    private void OnTriggerEnter2D(Collider2D OTHER) {
        // Ignore if the other object is the owner and the projectile is marked unable to collide with the owner
        // Typical case when the projectile spawns
        if ( OTHER.gameObject == owner && !bCollideWithOwner) { return; }

        // Determine if entering contact with a projectile target
        cPhysicalObject ACTOR_OTHER = OTHER.GetComponent<cPhysicalObject>();
        if ( ACTOR_OTHER != null ) {
            if ( ACTOR_OTHER.bProjectileTarget ) {
                // Projectile targets take damage, and the projectile is destroyed.
                if ( instigator == null ) { instigator = gameObject; }
                ACTOR_OTHER.TakeDamage(instigator.GetComponent<cPhysicalObject>(), damage, damageType, velocity);
            }
        }
    }

    // Once the projectile leaves the owner's collision box, the owner can collide with the projectile
    private void OnTriggerExit2D(Collider2D OTHER) {
        if ( OTHER.gameObject == owner ) { bCollideWithOwner = true; }
    }

    public void SetVelocity(Vector2 NEW_VELOCITY) {
        velocity = NEW_VELOCITY;
    }
    protected override IEnumerator RunMovePattern() {
        // Do not start next move until the projectile is done moving
        while ( bIsMoving ) { yield return null; }

        // Otherwise, continue moving at its projected velocity
        StartMove(velocity, 0.1f);
        while ( bIsMoving ) { yield return null; }

        // If the projectile's lifetime has expired, destroy it
        lifeSpan--;
        if ( lifeSpan == 0 ) {
            yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
        }

        // End of turn
        bTurnInProgress = false;
        yield break;
    }
}
