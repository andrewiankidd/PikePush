using UnityEngine;

namespace PikePush.Drill
{
    // Pure-logic helper for "are these two blocks engaged in contact?"
    // Pulled out so the rule is testable without instantiating GameObjects.
    public static class FactionContact
    {
        public static bool ShouldEngage(Faction a, Faction b, float sqrDistance, float contactRadius)
        {
            if (a == b) return false;
            return sqrDistance <= contactRadius * contactRadius;
        }

        // Convenience overload that pulls position + faction off the block
        // directly. Bounds-aware contact range falls back to a fixed minimum
        // so brand-new blocks (whose collider is sized in Start) still trigger.
        public static bool InContact(Block a, Block b, float minContactRadius)
        {
            if (a == null || b == null) return false;
            if (a.Faction == b.Faction) return false;

            Vector3 da = a.transform.position - b.transform.position;
            float sqrDist = da.x * da.x + da.z * da.z;
            float radius = Mathf.Max(minContactRadius, ApproxRadius(a) + ApproxRadius(b));
            return sqrDist <= radius * radius;
        }

        static float ApproxRadius(Block b)
        {
            var col = b.GetComponent<BoxCollider>();
            if (col == null) return 0f;
            // Half the larger XZ side. The previous half-diagonal was bigger
            // than the visual extent, so engagement fired with a noticeable
            // gap between blocks. Using half-side keeps contact aligned with
            // what the player sees touching.
            var size = col.size;
            return Mathf.Max(size.x, size.z) * 0.5f;
        }
    }
}
