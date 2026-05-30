using NUnit.Framework;
using PikePush.Drill;
using UnityEngine;

namespace PikePush.Tests.Drill
{
    public class FieldBoundsTests
    {
        const float HalfExtent = 35f;

        [Test]
        public void OriginIsInside()
        {
            Assert.IsFalse(FieldBounds.IsOutside(Vector3.zero, HalfExtent));
        }

        [Test]
        public void OnTheBoundary_StillInside()
        {
            // We want blocks to be able to reach the boundary without flipping
            // out of bounds — only points STRICTLY past it count as outside.
            Assert.IsFalse(FieldBounds.IsOutside(new Vector3( HalfExtent, 0,  0), HalfExtent));
            Assert.IsFalse(FieldBounds.IsOutside(new Vector3(-HalfExtent, 0,  0), HalfExtent));
            Assert.IsFalse(FieldBounds.IsOutside(new Vector3( 0,          0,  HalfExtent), HalfExtent));
            Assert.IsFalse(FieldBounds.IsOutside(new Vector3( 0,          0, -HalfExtent), HalfExtent));
        }

        [Test]
        public void JustPastBoundary_OnEachAxis_IsOutside()
        {
            float epsilon = 0.001f;
            Assert.IsTrue(FieldBounds.IsOutside(new Vector3( HalfExtent + epsilon, 0,  0), HalfExtent));
            Assert.IsTrue(FieldBounds.IsOutside(new Vector3(-HalfExtent - epsilon, 0,  0), HalfExtent));
            Assert.IsTrue(FieldBounds.IsOutside(new Vector3( 0,                    0,  HalfExtent + epsilon), HalfExtent));
            Assert.IsTrue(FieldBounds.IsOutside(new Vector3( 0,                    0, -HalfExtent - epsilon), HalfExtent));
        }

        [Test]
        public void YComponentIsIgnored()
        {
            // The field is a horizontal plane; only X/Z matter.
            var high = new Vector3(0f, 1000f, 0f);
            Assert.IsFalse(FieldBounds.IsOutside(high, HalfExtent));
        }
    }
}
