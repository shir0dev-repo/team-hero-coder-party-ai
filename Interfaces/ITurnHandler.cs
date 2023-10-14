using TeamHeroCoderLibrary;

namespace PlayerCoder;

/// <summary>
/// Wrapper interface designed to decouple Hero methods from the base MyAI class.
/// Inherits from IClassActionManager to further modularize Hero management.
/// </summary>
public interface ITurnHandler : IClassActionManager
{
    /// <summary>
    /// First forking point of the turn handler. Decides which function to execute based on the Hero's class ID.
    /// </summary>
    /// <param name="activeHeroClassID">The current hero with initiative. If an invalid value is given, will default to using a regular attack.</param>
    virtual void HandleTurn(int activeHeroClassID)
    {
        switch (activeHeroClassID)
        {
            case var fighterID when fighterID == TeamHeroCoder.HeroClassID.Fighter:
                Console.WriteLine("This is a fighter.");
                HandleTurnFighter();
                break;

            case var wizardID when wizardID == TeamHeroCoder.HeroClassID.Wizard:
                Console.WriteLine("This is a wizard.");
                HandleTurnWizard();
                break;

            case var clericID when clericID == TeamHeroCoder.HeroClassID.Cleric:
                Console.WriteLine("This is a cleric.");
                HandleTurnCleric();
                break;

            case var rogueID when rogueID == TeamHeroCoder.HeroClassID.Rogue:
                Console.WriteLine("This is a rogue.");
                HandleTurnRogue();
                break;

            case var monkID when monkID == TeamHeroCoder.HeroClassID.Monk:
                Console.WriteLine("This is a monk.");
                HandleTurnMonk();
                break;

            case var alchemistID when alchemistID == TeamHeroCoder.HeroClassID.Alchemist:
                Console.WriteLine("This is an alchemist.");
                HandleTurnAlchemist();
                break;

            default:
                Console.WriteLine("Specified class is invalid.");
                FallbackAttack();
                break;
        }
    }
}