using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cPowderBomb : cItem {
    [SerializeField] private GameObject placeableInstance;
    [SerializeField] private GameObject explosionProjectile;
    public List<Sprite> fuseTextures;
    public Sprite exploded;
    protected SpriteRenderer spriteHandle;
    protected int fuseFrame;

    protected override void Start() {
        base.Start();
        placeableInstance = gameObject;
    }

    public override void Activate() {
        charges--;
        GameObject PLACED = Instantiate(placeableInstance);
        PLACED.transform.position = owner.transform.position;
        PLACED.GetComponent<cPowderBomb>().SetFuse();
        print("Bomb placed!");
        print(PLACED);
    }

    private void SetFuse() {
        eItemState = enumItemState.Active;
        spriteHandle = GetComponent<SpriteRenderer>();
        spriteHandle.enabled = true;
        spriteHandle.sprite = fuseTextures[0];
        fuseFrame = 1;
    }

    private void Detonate() {
        spriteHandle.sprite = exploded;
        GameObject PROJECTILE_OBJECT = Instantiate(explosionProjectile);
        PROJECTILE_OBJECT.transform.position = transform.position;
        cProjectile PROJECTILE = PROJECTILE_OBJECT.GetComponent<cProjectile>();
        PROJECTILE.SetVelocity(new Vector2(0.32f, 0));
        PROJECTILE.bForceTurn = true;

        PROJECTILE_OBJECT = Instantiate(explosionProjectile);
        PROJECTILE_OBJECT.transform.position = transform.position;
        PROJECTILE = PROJECTILE_OBJECT.GetComponent<cProjectile>();
        PROJECTILE.SetVelocity(new Vector2(-0.32f, 0));
        PROJECTILE.bForceTurn = true;

        PROJECTILE_OBJECT = Instantiate(explosionProjectile);
        PROJECTILE_OBJECT.transform.position = transform.position;
        PROJECTILE = PROJECTILE_OBJECT.GetComponent<cProjectile>();
        PROJECTILE.SetVelocity(new Vector2(0, 0.32f));
        PROJECTILE.bForceTurn = true;

        PROJECTILE_OBJECT = Instantiate(explosionProjectile);
        PROJECTILE_OBJECT.transform.position = transform.position;
        PROJECTILE = PROJECTILE_OBJECT.GetComponent<cProjectile>();
        PROJECTILE.SetVelocity(new Vector2(0, -0.32f));
        PROJECTILE.bForceTurn = true;
    }

    protected override IEnumerator RunMovePattern() {
        // Only perform behavior if the item is active
        if ( eItemState != enumItemState.Active ) {
            bTurnInProgress = false;
            yield break;
        }

        // The bomb will traverse down each of its fuse textures one frame per turn, and explode after its last frame
        if ( fuseFrame < fuseTextures.Count ) {
            fuseFrame++;
            spriteHandle.sprite = fuseTextures[fuseFrame - 1];
            
            bTurnInProgress = false;
            yield break;
        }
        else {
            Detonate();
            yield return new WaitForSeconds(0.4f);
            
            Destroy(gameObject);
            bTurnInProgress = false;
            yield break;
        }
    }
}
