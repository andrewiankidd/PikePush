using NUnit.Framework;
using PikePush.Drill;

namespace PikePush.Tests.Drill
{
    public class FactionContactTests
    {
        const float Radius = 4f;
        const float SqrInside = 3f * 3f;       // inside radius
        const float SqrOnEdge = Radius * Radius;
        const float SqrOutside = 6f * 6f;      // well outside radius

        [Test]
        public void SameFaction_NeverEngages()
        {
            Assert.IsFalse(FactionContact.ShouldEngage(Faction.Friendly, Faction.Friendly, SqrInside, Radius));
            Assert.IsFalse(FactionContact.ShouldEngage(Faction.Enemy,    Faction.Enemy,    SqrInside, Radius));
        }

        [Test]
        public void OpposingFactions_EngageInsideRadius()
        {
            Assert.IsTrue(FactionContact.ShouldEngage(Faction.Friendly, Faction.Enemy, SqrInside, Radius));
        }

        [Test]
        public void OpposingFactions_EngageOnTheBoundary()
        {
            // The boundary inclusive — we want contact-at-touch to count.
            Assert.IsTrue(FactionContact.ShouldEngage(Faction.Friendly, Faction.Enemy, SqrOnEdge, Radius));
        }

        [Test]
        public void OpposingFactions_DoNotEngageOutsideRadius()
        {
            Assert.IsFalse(FactionContact.ShouldEngage(Faction.Friendly, Faction.Enemy, SqrOutside, Radius));
        }

        [Test]
        public void Symmetric_FriendlyEnemy_EqualsEnemyFriendly()
        {
            bool a = FactionContact.ShouldEngage(Faction.Friendly, Faction.Enemy, SqrInside, Radius);
            bool b = FactionContact.ShouldEngage(Faction.Enemy, Faction.Friendly, SqrInside, Radius);
            Assert.AreEqual(a, b);
        }
    }
}
