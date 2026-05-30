using System.Collections.Generic;

namespace PikePush.Drill
{
    // Single source of truth for the player-facing command labels and the
    // group→commands mapping the categorised palette renders.
    public static class DrillCommandCatalog
    {
        public static string Label(DrillCommand cmd)
        {
            switch (cmd)
            {
                // Movement
                case DrillCommand.Halt:                              return "Halt";
                case DrillCommand.ForwardMarch:                      return "Forward March";
                case DrillCommand.MarchOn:                           return "March On";

                // Pike postures
                case DrillCommand.OrderYourPike:                     return "Order Your Pike";
                case DrillCommand.AdvanceYourPike:                   return "Advance Your Pike";
                case DrillCommand.ShoulderYourPike:                  return "Shoulder Your Pike";
                case DrillCommand.ChargeYourPike:                    return "Charge Your Pike";
                case DrillCommand.ChargeToTheRear:                   return "Charge to the Rear";
                case DrillCommand.Port:                              return "Port";
                case DrillCommand.LowPortYourPike:                   return "Low Port";
                case DrillCommand.ShortenYourPike:                   return "Shorten";
                case DrillCommand.ChargeForHorse:                    return "Charge for Horse";
                case DrillCommand.FormCircle:                        return "Form Circle";
                case DrillCommand.TrailYourPike:                     return "Trail Your Pike";

                // Distancing
                case DrillCommand.ClosestOrder:                      return "Closest Order";
                case DrillCommand.CloseOrder:                        return "Close Order";
                case DrillCommand.OrderSpacing:                      return "Order";
                case DrillCommand.OpenOrder:                         return "Open Order";
                case DrillCommand.DoubleDistance:                    return "Double Distance";
                case DrillCommand.TwiceDoubleDistance:               return "Twice Double Distance";
                case DrillCommand.FilesOpenOrder:                    return "Files Open";
                case DrillCommand.FilesOpenOrderFromLeft:            return "Files Open · L";
                case DrillCommand.FilesOpenOrderFromMidst:           return "Files Open · Mid";
                case DrillCommand.RanksOpenOrder:                    return "Ranks Open";
                case DrillCommand.RanksOpenOrderFromRear:            return "Ranks Open · Rear";
                case DrillCommand.RanksAndFilesOpenOrder:            return "Ranks & Files Open";

                // Facings
                case DrillCommand.RightHandFace:                     return "Right Face";
                case DrillCommand.LeftHandFace:                      return "Left Face";
                case DrillCommand.LeftHandAboutFace:                 return "About Face L";
                case DrillCommand.RightHandAboutFace:                return "About Face R";
                case DrillCommand.FaceToTheFront:                    return "Face to Front";
                case DrillCommand.RightHandIncline:                  return "Right Incline";
                case DrillCommand.LeftHandIncline:                   return "Left Incline";
                case DrillCommand.FaceToFrontAndRear:                return "Front & Rear";
                case DrillCommand.FaceToBothFlanks:                  return "Both Flanks";

                // Doublings
                case DrillCommand.HalfFilesLeftDouble:               return "Half Files · L";
                case DrillCommand.HalfFilesRightDouble:              return "Half Files · R";
                case DrillCommand.HalfFilesRecover:                  return "Half Files Recover";
                case DrillCommand.EntireHalfFilesLeftDouble:         return "Entire Half · L";
                case DrillCommand.EntireHalfFilesRightDouble:        return "Entire Half · R";
                case DrillCommand.EntireHalfFilesOutwardsDouble:     return "Entire Half · Out";
                case DrillCommand.RanksRightDouble:                  return "Ranks · R Double";
                case DrillCommand.RanksLeftDouble:                   return "Ranks · L Double";
                case DrillCommand.RanksRecover:                      return "Ranks Recover";
                case DrillCommand.BringersUpDoubleFrontageLeft:      return "Bringers Up · L";
                case DrillCommand.BringersUpDoubleFrontageRight:     return "Bringers Up · R";
                case DrillCommand.BringersUpRecover:                 return "Bringers Up Recover";

                // Filing / Inversion
                case DrillCommand.FilesFileOn:                       return "Files File On";
                case DrillCommand.DoubleFilesFileOn:                 return "Double Files On";
                case DrillCommand.RanksFromLeftFileOn:               return "Ranks · L File On";
                case DrillCommand.RanksFromRightFileOn:              return "Ranks · R File On";
                case DrillCommand.FilesLeftDouble:                   return "Files · L Double";
                case DrillCommand.RecoverTheBody:                    return "Recover the Body";

                // Countermarching
                case DrillCommand.PrepareToCountermarchMaintainingGround:  return "Prep CM · Maintain";
                case DrillCommand.PrepareToCountermarchLosingGround:       return "Prep CM · Losing";
                case DrillCommand.PrepareToCountermarchGainingGround:      return "Prep CM · Gaining";
                case DrillCommand.Countermarch:                            return "Countermarch";

                // Wheeling
                case DrillCommand.RightHandWheel:                    return "Right Wheel";
                case DrillCommand.LeftHandWheel:                     return "Left Wheel";
                case DrillCommand.WheelMidstRight:                   return "Wheel Mid · R";
                case DrillCommand.WheelMidstLeft:                    return "Wheel Mid · L";

                // Reforms
                case DrillCommand.Reform:                            return "Reform";
            }
            return cmd.ToString();
        }

        // "Implemented" here means pressing the button produces a *visible*
        // effect — block movement, rotation, spacing transition, the brace
        // crouch, etc. Commands that flip internal state but have no visual
        // yet (most postures pending the animation suite, all doublings,
        // file-on / inversion choreography, prepare-countermarch staging,
        // non-canonical face variants) return false. The UI calls this on
        // every press and fires a TODO toast when it's false.
        public static bool IsImplemented(DrillCommand cmd)
        {
            switch (cmd)
            {
                // Movement
                case DrillCommand.Halt:
                case DrillCommand.ForwardMarch:
                case DrillCommand.MarchOn:

                // Postures with a visible effect (brace crouch)
                case DrillCommand.ChargeForHorse:

                // Distancing — the six numeric levels are wired to the spacing
                // multiplier. Directional variants (FilesOpenOrderFromLeft etc.)
                // collapse onto Open and have no choreography, so they're TODO.
                case DrillCommand.ClosestOrder:
                case DrillCommand.CloseOrder:
                case DrillCommand.OrderSpacing:
                case DrillCommand.OpenOrder:
                case DrillCommand.DoubleDistance:
                case DrillCommand.TwiceDoubleDistance:

                // Facings — the four cardinal turns rotate the block.
                case DrillCommand.RightHandFace:
                case DrillCommand.LeftHandFace:
                case DrillCommand.LeftHandAboutFace:
                case DrillCommand.RightHandAboutFace:

                // Wheeling — continuous rotation while marching.
                case DrillCommand.RightHandWheel:
                case DrillCommand.LeftHandWheel:
                case DrillCommand.WheelMidstRight:
                case DrillCommand.WheelMidstLeft:

                // Countermarch — maps to a 180° about-face for V1. The
                // 'Prepare to ...' staging commands are stubs (see default).
                case DrillCommand.Countermarch:

                // Reform — resets to Order spacing + Advance pike + halt.
                case DrillCommand.Reform:
                    return true;

                default:
                    return false;
            }
        }

        public static string GroupLabel(DrillCommandGroup g)
        {
            switch (g)
            {
                case DrillCommandGroup.Movement:     return "Move";
                case DrillCommandGroup.Postures:     return "Postures";
                case DrillCommandGroup.Distancing:   return "Distancing";
                case DrillCommandGroup.Facings:      return "Facings";
                case DrillCommandGroup.Doublings:    return "Doublings";
                case DrillCommandGroup.Filing:       return "Filing";
                case DrillCommandGroup.Countermarch: return "Countermarch";
                case DrillCommandGroup.Wheeling:     return "Wheeling";
                case DrillCommandGroup.Reforms:      return "Reform";
            }
            return g.ToString();
        }

        // Top-level slots shown on the bar in non-submenu mode.
        public static readonly DrillCommand[] TopLevelCommands =
        {
            DrillCommand.Halt,
            DrillCommand.ForwardMarch,
            DrillCommand.Reform,
        };

        // Group openers rendered next to the top-level commands. Order matters —
        // it's what the player sees left-to-right.
        public static readonly DrillCommandGroup[] TopLevelGroups =
        {
            DrillCommandGroup.Postures,
            DrillCommandGroup.Distancing,
            DrillCommandGroup.Facings,
            DrillCommandGroup.Doublings,
            DrillCommandGroup.Filing,
            DrillCommandGroup.Countermarch,
            DrillCommandGroup.Wheeling,
        };

        // All commands in a given group, in the order the submenu renders them.
        // Built once on first access — the underlying enum is static so caching
        // is safe.
        static readonly Dictionary<DrillCommandGroup, DrillCommand[]> groupCache
            = new Dictionary<DrillCommandGroup, DrillCommand[]>();

        public static DrillCommand[] CommandsInGroup(DrillCommandGroup g)
        {
            if (groupCache.TryGetValue(g, out var cached)) return cached;

            var list = new List<DrillCommand>();
            foreach (DrillCommand cmd in System.Enum.GetValues(typeof(DrillCommand)))
            {
                if (BlockRules.Group(cmd) == g)
                    list.Add(cmd);
            }
            var arr = list.ToArray();
            groupCache[g] = arr;
            return arr;
        }
    }
}
