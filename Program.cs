using TeamHeroCoderLibrary;

namespace PlayerCoder;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        PartyAIManager partyAIManager = new MyAI();
        partyAIManager.SetExchangePath("C:/Users/Shado/AppData/LocalLow/Wind Jester Games/Team Hero Coder");
        partyAIManager.StartReadingAndProcessingInfiniteLoop();
    }
}

public class MyAI : PartyAIManager, ITurnHandler
{
    public override void ProcessAI()
    {
        Console.WriteLine("Processing AI!");
        int activeHeroID = IClassActionManager.HeroWithInitiative.characterClassID;

        (this as ITurnHandler).HandleTurn(activeHeroID);
    }
}