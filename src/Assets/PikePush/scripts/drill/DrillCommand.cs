namespace PikePush.Drill
{
    // Period drill manual at docs/glossary/drill-commands.md.
    // Categories mirror DrillCommandGroup.
    public enum DrillCommand
    {
        // Movement
        Halt,
        ForwardMarch,
        MarchOn,

        // Pike postures
        OrderYourPike,
        AdvanceYourPike,
        ShoulderYourPike,
        ChargeYourPike,
        ChargeToTheRear,
        Port,
        LowPortYourPike,
        ShortenYourPike,
        ChargeForHorse,
        FormCircle,
        TrailYourPike,

        // Distancing
        ClosestOrder,
        CloseOrder,
        OrderSpacing,
        OpenOrder,
        DoubleDistance,
        TwiceDoubleDistance,
        FilesOpenOrder,
        FilesOpenOrderFromLeft,
        FilesOpenOrderFromMidst,
        RanksOpenOrder,
        RanksOpenOrderFromRear,
        RanksAndFilesOpenOrder,

        // Facings
        RightHandFace,
        LeftHandFace,
        LeftHandAboutFace,
        RightHandAboutFace,
        FaceToTheFront,
        RightHandIncline,
        LeftHandIncline,
        FaceToFrontAndRear,
        FaceToBothFlanks,

        // Doublings
        HalfFilesLeftDouble,
        HalfFilesRightDouble,
        HalfFilesRecover,
        EntireHalfFilesLeftDouble,
        EntireHalfFilesRightDouble,
        EntireHalfFilesOutwardsDouble,
        RanksRightDouble,
        RanksLeftDouble,
        RanksRecover,
        BringersUpDoubleFrontageLeft,
        BringersUpDoubleFrontageRight,
        BringersUpRecover,

        // Inversion / Filing on
        FilesFileOn,
        DoubleFilesFileOn,
        RanksFromLeftFileOn,
        RanksFromRightFileOn,
        FilesLeftDouble,
        RecoverTheBody,

        // Countermarching
        PrepareToCountermarchMaintainingGround,
        PrepareToCountermarchLosingGround,
        PrepareToCountermarchGainingGround,
        Countermarch,

        // Wheeling
        RightHandWheel,
        LeftHandWheel,
        WheelMidstRight,
        WheelMidstLeft,

        // Reforms
        Reform,
    }
}
