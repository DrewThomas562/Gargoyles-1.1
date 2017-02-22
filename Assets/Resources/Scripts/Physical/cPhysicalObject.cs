using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Declarations
[Serializable]
public partial class cPhysicalObject : MonoBehaviour {
    // Blocking detection
    public enum enumBlockingType {
        AllBlocking,
        Wall,
        Floor,
        Bridge,
        Fluid,
        Pit,
        NonBlocking,
        Interactive
    }
    public List<enumBlockingType> traversibleTerrain;
    public List<cPhysicalObject> nonBlocking;
    public bool bProjectileTarget;

    // Movement coroutine handling
    new protected Rigidbody2D rigidbody;
    public enum enumMoveType {
        Instant,
        Linear,
        Quadratic
    }
    enumMoveType eMoveType;
    private delegate IEnumerator delegateMoveType(Vector2 VECTOR, float MODIFIER);
    private event delegateMoveType selectedMoveType;
    private Coroutine cofxnMoveType;
    [HideInInspector]
    public bool bIsMoving;
    [HideInInspector]
    public Vector2 movingTo;
    public float moveSpeed;

    // Physical properties
    public enum enumDamageType {
        Normal,
        Deathtouch,
        Corrosive,
        Water,
        Frost,
        Fire,
        Blunt,
        Piercing,
        Explosive,
        Pressure,
        Drowning,
        Falling,
        Sound,
        Toxic
    }

    // Board piece AI coroutine handling
    public bool bForceTurn;
    [HideInInspector]
    public bool bTurnInProgress;
    private Coroutine cofxnMovePattern;

    protected virtual void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        movingTo = transform.position;
        SetMoveType(enumMoveType.Instant);
        StartCoroutine(PostBeginPlay());
    }

    protected virtual void Update() {
        if ( bForceTurn ) { bForceTurn = false; MakeMove(); }
    }

    public IEnumerator PostBeginPlay() {
        yield return new WaitForEndOfFrame();
        cGameManager.instance.AddBoardPiece(this);
        yield break;
    }
}

// Movement scripts
public partial class cPhysicalObject {
    private void OnDestroy() {
        print("Destroyed "+this);
        cGameManager.instance.pieces.Remove(this);
        bIsMoving = false;
        if ( cofxnMoveType != null ) { StopCoroutine(cofxnMoveType); }
        bTurnInProgress = false;
        if ( cofxnMovePattern != null ) { StopCoroutine(cofxnMovePattern); }
    }

    public void SetMoveType(enumMoveType MOVE_TYPE) {
        if (MOVE_TYPE == enumMoveType.Instant) { selectedMoveType = MoveTypeInstant; }
        else if (MOVE_TYPE == enumMoveType.Linear) { selectedMoveType = MoveTypeLinear; }
        else if (MOVE_TYPE == enumMoveType.Quadratic) { selectedMoveType = MoveTypeQuadratic; }
    }
    protected void StartMove(Vector2 TRANSLATION_VECTOR, float MODIFIER) {
        float x, y, z;
        x = Mathf.Round(transform.position.x / 0.32f) * 0.32f;
        y = Mathf.Round(transform.position.y / 0.32f) * 0.32f;
        z = 0;  // depth from camera

        transform.position = new Vector2(x,y);

        bIsMoving = true;
        if (cofxnMoveType != null) {
            StopCoroutine(cofxnMoveType);
        }
        cofxnMoveType = StartCoroutine(selectedMoveType(TRANSLATION_VECTOR, MODIFIER));
    }
    protected IEnumerator MoveTypeInstant(Vector2 TRANSLATION, float DELAY)
    {
        movingTo = rigidbody.position + TRANSLATION;
        yield return new WaitForSeconds(DELAY);
        rigidbody.MovePosition(movingTo);
        bIsMoving = false;
    }
    protected IEnumerator MoveTypeLinear(Vector2 TRANSLATION, float MOVE_TIME){
        float INVERSE_MOVE_TIME = 1f/MOVE_TIME;
        float START_TIME = Time.time;
        float END_TIME = START_TIME + MOVE_TIME;
        Vector2 START = rigidbody.position;
        movingTo = START + TRANSLATION;

        while (Time.time < END_TIME) {
            rigidbody.MovePosition(START + TRANSLATION * (Time.time-START_TIME)*INVERSE_MOVE_TIME);
            yield return null;
        }
        rigidbody.MovePosition(movingTo);
        bIsMoving = false;
    }
    protected IEnumerator MoveTypeQuadratic(Vector2 TRANSLATION, float MOVE_TIME) {
        float SINE_MOD = 180f / MOVE_TIME;
        float START_TIME = Time.time;
        float END_TIME = START_TIME + MOVE_TIME;
        Vector2 START = rigidbody.position;
        movingTo = START + TRANSLATION;

        while (Time.time < END_TIME) {
            rigidbody.MovePosition(START + TRANSLATION * (Mathf.Sin(Mathf.Deg2Rad * (-90f + (Time.time - START_TIME) * SINE_MOD))+1f)/2f);
            yield return null;
        }
        rigidbody.MovePosition(movingTo);
        bIsMoving = false;
    }
}

// AI inputs
public partial class cPhysicalObject {
    public Collider2D[] DetectObjects(Vector2 center) {
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(center);
        return hitColliders;
    }
    public bool CheckBlockingAdjacent(Vector2 TRANSLATION) {
        Vector2 ADJACENT = (Vector2)transform.position + TRANSLATION;
        Collider2D[] DETECTED_OBJECTS = DetectObjects(ADJACENT);

        foreach ( Collider2D OTHER in DETECTED_OBJECTS ) {
            cTile TILE_OTHER = OTHER.GetComponent<cTile>();
            if ( TILE_OTHER != null ) {
                if ( TILE_OTHER.CheckIfBlocked(GetComponent<cPhysicalObject>(), traversibleTerrain) ) {
                    return true;
                }
            }
        }
        return false;
    }
}

// Physical properties
public partial class cPhysicalObject {
    public virtual void TakeDamage(cPhysicalObject INSTIGATOR, int DAMAGE, enumDamageType TYPE = enumDamageType.Normal, Vector2 DIRECTION = default(Vector2)) {}
}

// Board piece AI action coroutines w/ handling
public partial class cPhysicalObject {
    // When all inputs are established, the piece can then be given the cue to make its move, which then executes its move patterns
    // This will stop any further calculations from a previous move, and start a new set.
    // Externally, objects can query whether or not the object is till performing its behavior pattern via "bTurnInProgress"
    public virtual void MakeMove() {
        bTurnInProgress = true;
        if ( cofxnMovePattern != null ) {
            StopCoroutine(cofxnMovePattern);
        }
        cofxnMovePattern = StartCoroutine(RunMovePattern());
    }
    
    // Board piece AI scripts go here and are managed internally
    protected virtual IEnumerator RunMovePattern() {
        // Execute behavior patterns here...
        yield return null;

        // End turn
        bTurnInProgress = false;
        yield break;
    }
}