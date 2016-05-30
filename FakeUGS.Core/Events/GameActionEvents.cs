namespace FakeUGS.Core.Events
{
    public class BetPlaced : GameActionEventBase { }

    public class BetWon : GameActionEventBase { }

    public class BetLost : GameActionEventBase { }

    public class BetTied : GameActionEventBase { }

    public class BetPlacedFree : GameActionEventBase { }

    public class BetCancelled: GameActionEventBase { }

    public class BetAdjusted : GameActionEventBase { }
}
