using UnityEngine;

//public class cLevelExit : cTile {

//    private void OnTriggerEnter2D(Collider2D OTHER){
//        if (OTHER.GetComponent<cPlayer>() != null){
//            Invoke("AdvanceLevel",0.5f);
//        }
//    }

//    private void AdvanceLevel() {
//        Application.LoadLevel(cGameManager.instance.sceneToLoad);
//    }
//}

public class cLevelExit : cTile {
    public int nextScene;

    private void OnTriggerEnter2D(Collider2D OTHER) {
        if (OTHER.GetComponent<cPlayer>() != null) {
            print("next scene!");
            Invoke("AdvanceLevel", 0.5f);
        }
    }

    private void AdvanceLevel() {
        Application.LoadLevel(nextScene);
    }
}