using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public struct TileKey {
//    public GameObject prefab;
//    public string legend;
//}

//public class cGameManager : MonoBehaviour {

//    public static cGameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.

//    public List<TileKey> TileMapper;
//    public int sceneToLoad;
//    public TextAsset mapFile;

//    // Use this for initialization
//    void Start() {
//        if (instance == null){
//            instance = this;
//        }
//        else if (instance != this){
//            instance.sceneToLoad = sceneToLoad;
//            Destroy(gameObject);
//        }

//        DontDestroyOnLoad(gameObject);
//        string[,] grid = CSVReader.SplitCsvGrid(mapFile.text);
//        for (int i = 0; i < grid.GetUpperBound(0); i++){
//            for (int j = 0; j < grid.GetUpperBound(1); j++){
//                CreateFromKey(grid[i, j], new Vector3(0.32f * i, -0.32f * j, 0), new Quaternion());}
//        }
//    }

//    void CreateFromKey(string KEY, Vector3 POSITION = new Vector3(), Quaternion ROTATION = new Quaternion()) {
//        //    Instantiate(Resources.Load("Prefabs/Floor", typeof(GameObject)), new Vector3(0.32f * i, -0.32f * j, 0), new Quaternion());
//        for (int i = 0; i < TileMapper.Count; i++) {
//            if (TileMapper[i].legend == KEY) {
//                Instantiate(TileMapper[i].prefab, POSITION, ROTATION);
//            }
//        }
//    }
//}

/// <summary>
/// 
/// Game Controller / Manager
/// 
/// The role of the Game Controller is to act as the mediator of the game...
/// Much like a chess player that moves each of his pieces, the Game Controller is aware of all game pieces
/// It is in charge of giving each board piece the time to make their move.
/// 
/// On Start, the Game Controller scans the heirarchy (established in Unity) for objects that can make moves on the board
/// Each eligible target is tacked onto the list of "turnBasedObjects"
/// 
/// Once all acting board pieces are established, the Game Controller starts its main Coroutine "RunTurnBased"
/// This coroutine moves up through the list of board pieces, initiating each piece's "FinishTurn" coroutine...
/// and waiting for the piece to declare the "end" of their turn
/// Alternatively if a board piece goes missing (AKA destroyed, or null), the game manager will regard that as the turn's "end"
/// At the end of the list, the Game Controller restarts the loop with the first board piece on the list
/// 
/// </summary>

[Serializable]
public class cGameManager : MonoBehaviour {
    public static cGameManager instance = null;             //Static instance of GameManager which allows it to be accessed by any other script.
    public List<cPhysicalObject> pieces;

    private bool bResetGameLoop = true;
    private Coroutine cofxnGameLoop;

    // Use this for initialization
    void Start() {
        instance = this;
    }

    public void AddBoardPiece(cPhysicalObject NEW_PIECE) {
        // Players go first, all other objects fall second
        if ( NEW_PIECE.GetComponent<cPlayer>() != null ) { pieces.Insert(0, NEW_PIECE); }
        else { pieces.Add(NEW_PIECE); }
    }

    private void Update() {
        if ( bResetGameLoop ) {
            bResetGameLoop = false;
            if ( cofxnGameLoop != null ) {
                StopCoroutine(cofxnGameLoop);
            }
            cofxnGameLoop = StartCoroutine(GameLoop());
        }
    }

    private IEnumerator GameLoop() {
        // GAME LOOP

        // While the game loop is in progress, pieces can be destroyed and removed from the dynamic list
        // However, foreach loops will crash if the array changes size before completing the iteration
        // Solve the problem by pre-allocating a fixed array of pieces to traverse each game loop
        cPhysicalObject[] NON_DYNAMIC_ARRAY_OF_PIECES = pieces.ToArray();

        // The game loop will pause specially for a board piece if that piece is a player
        cPlayer PLAYER_PIECE;

        // Traverse all board pieces in the level, and let them make moves
        foreach ( cPhysicalObject BOARD_PIECE in NON_DYNAMIC_ARRAY_OF_PIECES ) {
            // Ensure the piece still exists before taking action on it. (Might have been destroyed)
            if ( BOARD_PIECE == null ) { continue; }
            PLAYER_PIECE = BOARD_PIECE.GetComponent<cPlayer>();

            // Before the piece is able to make a move, it must wait until its current turn is completed
            while ( BOARD_PIECE.bTurnInProgress ) { yield return null; }

            // If the player is selected, wait until all objects have completed their turn before making another move
            if ( PLAYER_PIECE != null ) {
                foreach ( cPhysicalObject PIECE in NON_DYNAMIC_ARRAY_OF_PIECES ) {
                    while ( PIECE.bTurnInProgress ) { yield return null; }
                };
            }

            // Make the next move
            BOARD_PIECE.MakeMove();

            // If the player is selected, wait until the player completes their move before enemies move
            if ( PLAYER_PIECE != null ) {
                while ( BOARD_PIECE.bTurnInProgress && !PLAYER_PIECE.bIsMoving) { yield return null; }
            }

        } // Continue until all pieces have made their moves
        bResetGameLoop = true;
    }
}