using UnityEngine;

namespace PikePush.Drill
{
    // Pure-logic check for "would this block's next position leave the
    // playable field?" Pulled out so Block.Update can stay short and the
    // boundary rule is testable without instantiating a GameObject.
    //
    // halfExtent is the per-axis distance from the field's centre at which
    // a block's centroid is considered out of bounds. DrillBootstrap derives
    // it from fieldSize minus a small margin so visuals don't clip the edge.
    public static class FieldBounds
    {
        public static bool IsOutside(Vector3 pos, float halfExtent)
        {
            return Mathf.Abs(pos.x) > halfExtent || Mathf.Abs(pos.z) > halfExtent;
        }
    }
}
