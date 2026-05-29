using NUnit.Framework;
using PikePush.Drill;

namespace PikePush.Tests.Drill
{
    public class BlockRulesTests
    {
        static BlockState DefaultState()
        {
            return new BlockState
            {
                Posture = PikePosture.Order,
                Spacing = SpacingOrder.Order,
                IsWheeling = false,
                IsMarching = false,
                IsEngaged = false,
            };
        }

        static BlockState EngagedState()
        {
            var s = DefaultState();
            s.IsEngaged = true;
            return s;
        }

        [Test]
        public void DefaultState_AllowsHaltAndForwardMarch()
        {
            var s = DefaultState();
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Halt, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.ForwardMarch, s));
        }

        [Test]
        public void DefaultState_AllowsFacings()
        {
            var s = DefaultState();
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.RightHandFace, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.LeftHandFace, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.LeftHandAboutFace, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.RightHandAboutFace, s));
        }

        [Test]
        public void DefaultState_AllowsAllSpacings()
        {
            var s = DefaultState();
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.ClosestOrder, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.CloseOrder, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.OrderSpacing, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.OpenOrder, s));
        }

        [Test]
        public void BracingPosture_BlocksForwardMarch()
        {
            var s = DefaultState();
            s.Posture = PikePosture.ChargeForHorse;
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.ForwardMarch, s));
        }

        [Test]
        public void BracingPosture_BlocksAllFacings()
        {
            var s = DefaultState();
            s.Posture = PikePosture.ChargeForHorse;
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandAboutFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandAboutFace, s));
        }

        [Test]
        public void BracingPosture_BlocksSpacingChanges()
        {
            var s = DefaultState();
            s.Posture = PikePosture.ChargeForHorse;
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.OpenOrder, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.CloseOrder, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.ClosestOrder, s));
        }

        [Test]
        public void BracingPosture_AllowsPostureChange()
        {
            var s = DefaultState();
            s.Posture = PikePosture.ChargeForHorse;
            // Need a way out of bracing — postures unlock you.
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.OrderYourPike, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.AdvanceYourPike, s));
        }

        [Test]
        public void BracingPosture_AllowsHalt()
        {
            var s = DefaultState();
            s.Posture = PikePosture.ChargeForHorse;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Halt, s));
        }

        [Test]
        public void ClosestOrder_BlocksFacings()
        {
            // Period rule — pikes would clash at shoulder-touching density.
            var s = DefaultState();
            s.Spacing = SpacingOrder.Closest;
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandAboutFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandAboutFace, s));
        }

        [Test]
        public void ClosestOrder_AllowsSpacingOut()
        {
            // You need an escape route from Closest.
            var s = DefaultState();
            s.Spacing = SpacingOrder.Closest;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.CloseOrder, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.OrderSpacing, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.OpenOrder, s));
        }

        [Test]
        public void Wheeling_BlocksFacings()
        {
            // A wheel is already rotating you — stacking a face on top is incoherent.
            var s = DefaultState();
            s.IsWheeling = true;
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandFace, s));
        }

        [Test]
        public void Wheeling_AllowsMarchOnAndHalt()
        {
            var s = DefaultState();
            s.IsWheeling = true;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.MarchOn, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Halt, s));
        }

        [Test]
        public void Engaged_BlocksMovementAndFacings()
        {
            // Once a block is in push-of-pike contact, it can't walk away
            // or pivot. The only sane orders are Halt, posture changes,
            // spacing changes (closer for push power, looser for breaks),
            // and Reform.
            var s = EngagedState();
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.ForwardMarch, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.MarchOn, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandWheel, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandWheel, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.RightHandFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.LeftHandAboutFace, s));
            Assert.IsFalse(BlockRules.AllowsCommand(DrillCommand.Countermarch, s));
        }

        [Test]
        public void Engaged_AllowsHaltAndPostureAndSpacing()
        {
            var s = EngagedState();
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Halt, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.AdvanceYourPike, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.ChargeYourPike, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.ClosestOrder, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.CloseOrder, s));
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Reform, s));
        }

        [Test]
        public void ReformAlwaysAllowed()
        {
            // Reform is the panic button — should never be gated out.
            var braced = DefaultState();
            braced.Posture = PikePosture.ChargeForHorse;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Reform, braced));

            var closest = DefaultState();
            closest.Spacing = SpacingOrder.Closest;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Reform, closest));

            var wheeling = DefaultState();
            wheeling.IsWheeling = true;
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Reform, wheeling));

            var engaged = EngagedState();
            Assert.IsTrue(BlockRules.AllowsCommand(DrillCommand.Reform, engaged));
        }
    }
}
