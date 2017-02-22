using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cTile : cPhysicalObject {
    public enumBlockingType eBlockingType;

    public virtual bool CheckIfBlocked(cPhysicalObject OTHER, List<enumBlockingType> TRAVERSIBLE_BLOCKS) {
        if (eBlockingType == enumBlockingType.Interactive) {return InteractiveBlock(OTHER, TRAVERSIBLE_BLOCKS);}
        else if (eBlockingType == enumBlockingType.NonBlocking) { return false; }
        else if (eBlockingType == enumBlockingType.AllBlocking) { return true; }
        else {
            for (int i = 0; i < TRAVERSIBLE_BLOCKS.Count; i++) {
                if (TRAVERSIBLE_BLOCKS[i] == eBlockingType) {
                    return false;
                }
            }
            return true;
        }
    }
    public virtual bool InteractiveBlock(cPhysicalObject OTHER, List<enumBlockingType> TRAVERSIBLE_BLOCKS) {
        return false;
    }
}
