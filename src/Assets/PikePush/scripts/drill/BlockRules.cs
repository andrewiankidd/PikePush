namespace PikePush.Drill
{
    public struct BlockState
    {
        public PikePosture Posture;
        public SpacingOrder Spacing;
        public bool IsWheeling;
        public bool IsMarching;
    }

    public static class BlockRules
    {
        public static bool AllowsCommand(DrillCommand cmd, BlockState state)
        {
            // Reform is the panic button — always allowed.
            if (cmd == DrillCommand.Reform) return true;

            // Halt is always allowed: you can always stop.
            if (cmd == DrillCommand.Halt) return true;

            // Posture changes are always allowed — they're the way out of any
            // locked state (incl. ChargeForHorse).
            if (IsPosture(cmd)) return true;

            bool braced = state.Posture == PikePosture.ChargeForHorse;
            bool closest = state.Spacing == SpacingOrder.Closest;
            bool wheeling = state.IsWheeling;

            // While bracing, the block is committed to its anti-cavalry stance.
            // No movement, no facing, no spacing changes — only re-posture or halt.
            if (braced)
                return false;

            // Facings rotate the whole formation 90/180. Pikes at Closest spacing
            // would clash; wheeling already controls yaw.
            if (IsFacing(cmd))
            {
                if (closest) return false;
                if (wheeling) return false;
                return true;
            }

            // Spacing changes are always allowed (other than the braced exclusion above).
            // Even from Closest you can spread back out.
            if (IsSpacing(cmd))
                return true;

            // Movement / wheel resumption: most legal at default state, MarchOn
            // is specifically how you leave a wheel.
            return true;
        }

        public static bool IsPosture(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.OrderYourPike:
                case DrillCommand.AdvanceYourPike:
                case DrillCommand.ShoulderYourPike:
                case DrillCommand.ChargeYourPike:
                case DrillCommand.ChargeToTheRear:
                case DrillCommand.Port:
                case DrillCommand.LowPortYourPike:
                case DrillCommand.ShortenYourPike:
                case DrillCommand.ChargeForHorse:
                case DrillCommand.FormCircle:
                case DrillCommand.TrailYourPike:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFacing(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.RightHandFace:
                case DrillCommand.LeftHandFace:
                case DrillCommand.LeftHandAboutFace:
                case DrillCommand.RightHandAboutFace:
                case DrillCommand.FaceToTheFront:
                case DrillCommand.RightHandIncline:
                case DrillCommand.LeftHandIncline:
                case DrillCommand.FaceToFrontAndRear:
                case DrillCommand.FaceToBothFlanks:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSpacing(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.ClosestOrder:
                case DrillCommand.CloseOrder:
                case DrillCommand.OrderSpacing:
                case DrillCommand.OpenOrder:
                case DrillCommand.DoubleDistance:
                case DrillCommand.TwiceDoubleDistance:
                case DrillCommand.FilesOpenOrder:
                case DrillCommand.FilesOpenOrderFromLeft:
                case DrillCommand.FilesOpenOrderFromMidst:
                case DrillCommand.RanksOpenOrder:
                case DrillCommand.RanksOpenOrderFromRear:
                case DrillCommand.RanksAndFilesOpenOrder:
                    return true;
                default:
                    return false;
            }
        }

        public static DrillCommandGroup Group(DrillCommand cmd)
        {
            if (IsPosture(cmd)) return DrillCommandGroup.Postures;
            if (IsFacing(cmd)) return DrillCommandGroup.Facings;
            if (IsSpacing(cmd)) return DrillCommandGroup.Distancing;

            switch (cmd)
            {
                case DrillCommand.Halt:
                case DrillCommand.ForwardMarch:
                case DrillCommand.MarchOn:
                    return DrillCommandGroup.Movement;

                case DrillCommand.HalfFilesLeftDouble:
                case DrillCommand.HalfFilesRightDouble:
                case DrillCommand.HalfFilesRecover:
                case DrillCommand.EntireHalfFilesLeftDouble:
                case DrillCommand.EntireHalfFilesRightDouble:
                case DrillCommand.EntireHalfFilesOutwardsDouble:
                case DrillCommand.RanksRightDouble:
                case DrillCommand.RanksLeftDouble:
                case DrillCommand.RanksRecover:
                case DrillCommand.BringersUpDoubleFrontageLeft:
                case DrillCommand.BringersUpDoubleFrontageRight:
                case DrillCommand.BringersUpRecover:
                    return DrillCommandGroup.Doublings;

                case DrillCommand.FilesFileOn:
                case DrillCommand.DoubleFilesFileOn:
                case DrillCommand.RanksFromLeftFileOn:
                case DrillCommand.RanksFromRightFileOn:
                case DrillCommand.FilesLeftDouble:
                case DrillCommand.RecoverTheBody:
                    return DrillCommandGroup.Filing;

                case DrillCommand.PrepareToCountermarchMaintainingGround:
                case DrillCommand.PrepareToCountermarchLosingGround:
                case DrillCommand.PrepareToCountermarchGainingGround:
                case DrillCommand.Countermarch:
                    return DrillCommandGroup.Countermarch;

                case DrillCommand.RightHandWheel:
                case DrillCommand.LeftHandWheel:
                case DrillCommand.WheelMidstRight:
                case DrillCommand.WheelMidstLeft:
                    return DrillCommandGroup.Wheeling;

                case DrillCommand.Reform:
                    return DrillCommandGroup.Reforms;
            }
            return DrillCommandGroup.Movement;
        }
    }
}
