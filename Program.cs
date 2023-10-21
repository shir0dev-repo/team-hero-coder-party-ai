using TeamHeroCoderLibrary;
using AbilityID = TeamHeroCoderLibrary.TeamHeroCoder.AbilityID;
using HeroClassID = TeamHeroCoderLibrary.TeamHeroCoder.HeroClassID;
using ItemID = TeamHeroCoderLibrary.TeamHeroCoder.ItemID;
using StatusEffectID = TeamHeroCoderLibrary.TeamHeroCoder.StatusEffectID;
using Utils = PlayerCoder.PlayerCoderUtils;

namespace PlayerCoder;

class Program
{
    const string EXCHANGE_PATH = @"C:/Users/Shado/AppData/LocalLow/Wind Jester Games/Team Hero Coder";

    static void Main(string[] args)
    {
        PartyAIManager partyAIManager = new MyAI();
        partyAIManager.SetExchangePath(EXCHANGE_PATH);
        partyAIManager.StartReadingAndProcessingInfiniteLoop();
    }
}

public class MyAI : PartyAIManager
{
    private static Hero HeroWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative;

    private static List<InventoryItem> PlayerInventory => TeamHeroCoder.BattleState.playerInventory;

    private static List<Hero> AlivePartyMembers => TeamHeroCoder.BattleState.playerHeroes
            .Where(ally => ally.health > 0)
            .ToList();

    private static List<Hero> DeadPartyMembers => TeamHeroCoder.BattleState.playerHeroes
            .Except(AlivePartyMembers)
            .ToList();

    private static List<Hero> AlliesByHealth => AlivePartyMembers
            .OrderBy(ally => ally.GetHealthAsPercent())
            .ToList();

    private static List<Hero> AliveEnemies => TeamHeroCoder.BattleState.foeHeroes
        .Where(foe => foe.health > 0)
        .ToList();

    private static List<Hero> EnemiesByHealth => AliveEnemies
        .OrderBy(foe => foe.GetHealthAsPercent())
        .ToList();


    //Priority order of enemies that should be attacked.
    private readonly static Queue<int> _priorityQueue = new(new[]
        {
            HeroClassID.Cleric,
            HeroClassID.Alchemist,
            HeroClassID.Wizard,
            HeroClassID.Rogue,
            HeroClassID.Monk,
            HeroClassID.Fighter
        });

    public override void ProcessAI()
    {
        switch (HeroWithInitiative.characterClassID)
        {
            case var fighterID when fighterID == HeroClassID.Fighter:
                Console.WriteLine("The Fighter joins the fray!");
                HandleTurnFighter();
                break;

            case var wizardID when wizardID == HeroClassID.Wizard:
                Console.WriteLine("The Wizard conjures an entrance!");
                HandleTurnWizard();
                break;

            case var clericID when clericID == HeroClassID.Cleric:
                Console.WriteLine("The Cleric lights the way!");
                HandleTurnCleric();
                break;

            case var rogueID when rogueID == HeroClassID.Rogue:
                Console.WriteLine("The Rogue sneaks in!");
                HandleTurnRogue();
                break;

            case var monkID when monkID == HeroClassID.Monk:
                Console.WriteLine("The Monk solemnly appears!");
                HandleTurnMonk();
                break;

            case var alchemistID when alchemistID == HeroClassID.Alchemist:
                Console.WriteLine("The Alchemist brews an appearance!");
                HandleTurnAlchemist();
                break;

            default:
                Console.WriteLine("Your identity crisis fills you with rage. (Invalid class; attacking first available target.)");
                Utils.FailsafeAttack();
                break;
        }
    }

    #region Common

    /// <summary>
    /// Functionality shared with all classes.
    /// </summary>
    static bool HandleTurnCommon(float healthLimit = 0.2f, float manaLimit = 0.3f)
    {
        //If dead party members has a hero whose class ID is cleric, revive said hero.
        DeadPartyMembers.HasHero(foe => foe.characterClassID == HeroClassID.Cleric, out Hero? deadCleric);
        if (deadCleric != null && PlayerInventory.ContainsItem(ItemID.Revive))
        {
            Console.WriteLine("Guess you aren't very good at your job. (Used Revive on Cleric.)");
            return HeroWithInitiative.RequestAbilityCast(AbilityID.Revive, deadCleric);
        }

        //Check if players health is low and inventory contains a Potion.
        else if (HeroWithInitiative.GetHealthAsPercent() <= healthLimit && PlayerInventory.ContainsItem(ItemID.Potion))
        {
            Console.WriteLine("Feeling under the weather. (Hero used a health potion.)");
            return HeroWithInitiative.RequestAbilityCast(AbilityID.Potion, HeroWithInitiative);
        }

        //Check if hero's mana is low and inventory contains an Ether.
        else if (HeroWithInitiative.GetManaAsPercent() <= manaLimit && PlayerInventory.ContainsItem(ItemID.Ether))
        {
            Console.WriteLine("Feeling a bit parched. (Hero used a mana potion.)");
            return HeroWithInitiative.RequestAbilityCast(AbilityID.Ether, HeroWithInitiative);
        }

        else return false;
    }

    #endregion

    #region Fighter

    static void HandleTurnFighter()
    {
        if (HandleTurnCommon()) return;

        bool clericIsAlive = AlivePartyMembers.HasHero(ally => ally.characterClassID == HeroClassID.Cleric, out _);
        if (!clericIsAlive && !HeroWithInitiative.HasStatusEffect(StatusEffectID.Brave))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Brave, HeroWithInitiative))
            {
                Console.WriteLine("Knowing the mouse might one day leave its hole and get the cheese fills you with determination. (Fighter casted Brave on themself.)");
                return;
            }
        }

        if (HeroWithInitiative.RequestAbilityCast(AbilityID.QuickHit, _priorityQueue))
        {
            Console.WriteLine("Let's make this quick. (Fighter casted Quick Hit on the most preferred enemy.)");
            return;
        }

        Utils.FailsafeAttack();
    }

    #endregion

    #region Wizard

    static void HandleTurnWizard()
    {
        if (HandleTurnCommon()) return;

        //Check if fighter is alive, and doesn't already have doom.
        bool fighterWithoutDoom = AliveEnemies.HasHero(foe => foe.characterClassID == HeroClassID.Fighter && !foe.HasStatusEffect(StatusEffectID.Doom), out Hero? enemyFighter);
        if (fighterWithoutDoom && enemyFighter != null && HeroWithInitiative.RequestAbilityCast(AbilityID.Doom, enemyFighter))
        {
            Console.WriteLine("Impending doom approaches... (Wizard casted Doom on Fighter.)");
            return;
        }

        if (AliveEnemies.Count > 1 && HeroWithInitiative.RequestAbilityCast(AbilityID.Meteor))
        {
            Console.WriteLine("Let's make it rain! (Wizard casted Meteor on all foes.)");
            return;
        }

        if (HeroWithInitiative.RequestAbilityCast(AbilityID.MagicMissile, _priorityQueue))
        {
            Console.WriteLine("Do you believe in magic? (Wizard casted Magic Missle on the most preferred enemy.)");
            return;
        }

        Utils.FailsafeAttack();
    }

    #endregion

    #region Cleric

    static void HandleTurnCleric()
    {
        if (HandleTurnCommon()) return;

        //If any allies are dead, prioritize casting resurrection if possible.
        if (DeadPartyMembers.Count > 0 && HeroWithInitiative.HasEnoughMana(AbilityID.Resurrection))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Resurrection, DeadPartyMembers.First()))
            {
                Console.WriteLine("Heroes never die! (Casted Resurrection on fallen ally.)");
                return;
            }
        }

        Hero lowestHealthAlly = AlliesByHealth.First();

        //Find the lowest health ally and use the correct corresponding cure spell;
        //serious if <= 20%, light if > 25% && <= 50%.
        if (lowestHealthAlly.GetHealthAsPercent() <= 0.5f)
        {

        }


        //If party contains a wizard, apply faith status if needed.
        AlivePartyMembers.HasHero(ally => ally.characterClassID == HeroClassID.Wizard, out Hero? allyWizard);
        if (allyWizard != null && !allyWizard.HasStatusEffect(StatusEffectID.Faith))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Faith, allyWizard))
            {
                Console.WriteLine("Faith is a virtue. (Casted Faith on Wizard.)");
                return;
            }
            else Console.WriteLine("I see you've followed my teachings. (All wizards already have Faith status.)");
        }


        //If party contains a rogue, apply brave status if needed.
        AlivePartyMembers.HasHero(ally => ally.characterClassID == HeroClassID.Rogue, out Hero? allyRogue);
        if (allyRogue != null && !allyRogue.HasStatusEffect(StatusEffectID.Brave))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Brave, allyRogue))
            {
                Console.WriteLine("Fear is a natural thing. This isn't. (Casted Brave on Rogue.)");
                return;
            }
            else
                Console.WriteLine("Such courage! (All Rogues have Brave status.)");
        }

        if (allyWizard != null && !allyWizard.HasStatusEffect(StatusEffectID.Haste))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Haste, allyWizard))
            {
                Console.WriteLine("Lets crank up the tempo! (Cleric casted Haste on Wizard.)");
                return;
            }
        }
        else if (allyRogue != null && allyRogue.HasStatusEffect(StatusEffectID.Haste))
        {
            if (HeroWithInitiative.RequestAbilityCast(AbilityID.Haste, allyRogue))
            {
                Console.WriteLine("Lets crank up the tempo! (Cleric casted Haste on Rogue.)");
                return;
            }
        }
        else
            Console.WriteLine("Blink and you'll miss it! (Both Wizards and Rogues all have Haste.)");


        Utils.FailsafeAttack();
    }

    #endregion

    #region Rogue

    static void HandleTurnRogue()
    {
        Console.WriteLine("Functionality not Implemented for Rogue Class!");
        Utils.FailsafeAttack();
    }

    #endregion

    #region Monk

    static void HandleTurnMonk()
    {
        Console.WriteLine("Functionality not Implemented for Monk Class!");
        Utils.FailsafeAttack();
    }

    #endregion

    #region Alchemist

    static void HandleTurnAlchemist()
    {
        Console.WriteLine("Functionality not Implemented for Alchemist Class!");
        Utils.FailsafeAttack();
    }

    #endregion
}