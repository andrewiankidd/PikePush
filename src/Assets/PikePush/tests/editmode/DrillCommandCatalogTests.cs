using System;
using System.Collections.Generic;
using NUnit.Framework;
using PikePush.Drill;

namespace PikePush.Tests.Drill
{
    public class DrillCommandCatalogTests
    {
        [Test]
        public void EveryCommand_HasAHumanLabel()
        {
            foreach (DrillCommand cmd in Enum.GetValues(typeof(DrillCommand)))
            {
                string label = DrillCommandCatalog.Label(cmd);
                Assert.IsFalse(string.IsNullOrWhiteSpace(label), $"{cmd} has no label");
                // Label should be human-readable, not the enum name; bare enum
                // names usually look like "ChargeYourPike" without spaces.
                Assert.AreNotEqual(cmd.ToString(), label, $"{cmd} label fell back to enum name");
            }
        }

        [Test]
        public void EveryCommand_BelongsToExactlyOneGroup()
        {
            // Every command's BlockRules.Group should map back into a populated
            // CommandsInGroup list. Anything that falls out means the catalog
            // and rules drifted apart.
            var groupCounts = new Dictionary<DrillCommandGroup, int>();
            foreach (DrillCommand cmd in Enum.GetValues(typeof(DrillCommand)))
            {
                var g = BlockRules.Group(cmd);
                groupCounts.TryGetValue(g, out int count);
                groupCounts[g] = count + 1;
            }

            foreach (DrillCommandGroup g in Enum.GetValues(typeof(DrillCommandGroup)))
            {
                int byCatalog = DrillCommandCatalog.CommandsInGroup(g).Length;
                int byRules = groupCounts.TryGetValue(g, out int n) ? n : 0;
                Assert.AreEqual(byRules, byCatalog,
                    $"Group {g}: catalog sees {byCatalog}, rules see {byRules}");
            }
        }

        [Test]
        public void TopLevelCommandsAreUnique()
        {
            var seen = new HashSet<DrillCommand>();
            foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
                Assert.IsTrue(seen.Add(cmd), $"{cmd} appears twice in TopLevelCommands");
        }

        [Test]
        public void TopLevelGroupsAreUnique()
        {
            var seen = new HashSet<DrillCommandGroup>();
            foreach (var g in DrillCommandCatalog.TopLevelGroups)
                Assert.IsTrue(seen.Add(g), $"{g} appears twice in TopLevelGroups");
        }

        [Test]
        public void HaltAndForwardMarchAndReform_AreTopLevel()
        {
            // These are the always-visible bar entries — losing them breaks
            // the "no submenu needed for the basics" promise.
            // CollectionAssert handles IEnumerable; NUnit's Assert.Contains
            // wants non-generic ICollection which HashSet<T> doesn't implement.
            CollectionAssert.Contains(DrillCommandCatalog.TopLevelCommands, DrillCommand.Halt);
            CollectionAssert.Contains(DrillCommandCatalog.TopLevelCommands, DrillCommand.ForwardMarch);
            CollectionAssert.Contains(DrillCommandCatalog.TopLevelCommands, DrillCommand.Reform);
        }

        // --- IsImplemented coverage --------------------------------------

        [Test]
        public void EveryTopLevelCommand_IsImplemented()
        {
            // Players first see top-level commands. If any of them are TODO
            // stubs, drill mode feels broken on first open. Lock them in.
            foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
            {
                Assert.IsTrue(DrillCommandCatalog.IsImplemented(cmd),
                    $"Top-level command {cmd} reports IsImplemented = false. " +
                    "Either implement its visual or stop surfacing it on the bar.");
            }
        }

        [Test]
        public void Doublings_AreStubs()
        {
            // Doublings need rank/file rearrangement choreography. All TODO
            // pending the animation suite — this test catches anyone
            // accidentally marking one implemented before the visual lands.
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.HalfFilesLeftDouble));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.RanksRightDouble));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.BringersUpDoubleFrontageLeft));
        }

        [Test]
        public void FilingAndInversion_AreStubs()
        {
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.FilesFileOn));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.RanksFromLeftFileOn));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.RecoverTheBody));
        }

        [Test]
        public void PrepareToCountermarch_AreStubs_ButCountermarchIsLive()
        {
            // Countermarch maps to a 180° about-face today; the prep stages
            // are flag-stubs.
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.PrepareToCountermarchMaintainingGround));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.PrepareToCountermarchLosingGround));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.PrepareToCountermarchGainingGround));
            Assert.IsTrue(DrillCommandCatalog.IsImplemented(DrillCommand.Countermarch));
        }

        [Test]
        public void NonCanonicalFacings_AreStubs()
        {
            // The four cardinal faces work; the inclines and split-faces don't.
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.RightHandIncline));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.LeftHandIncline));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.FaceToFrontAndRear));
            Assert.IsFalse(DrillCommandCatalog.IsImplemented(DrillCommand.FaceToBothFlanks));
        }
    }
}
