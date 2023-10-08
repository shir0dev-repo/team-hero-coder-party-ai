using TeamHeroCoderLibrary;

namespace PlayerCoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            PartyAIManager partyAIManager = new MyAI();
            partyAIManager.SetExchangePath("C:/Users/v0100/AppData/LocalLow/Wind Jester Games/Team Hero Coder");
            partyAIManager.StartReadingAndProcessingInfiniteLoop();
        }
    }

    public class MyAI : PartyAIManager
    {
        public override void ProcessAI()
        {
            Console.WriteLine("Processing AI!");
            int activeHeroID = PartyAIExtensions.ClassIDWithInitiative;

            PartyAIExtensions.HandleTurn(activeHeroID);
        }
    }

    public static class PartyAIExtensions
    {
        /// <summary>
        /// Field for active hero.
        /// </summary>
        public static Hero HeroWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative;

        /// <summary>
        /// Field for active hero's class ID.
        /// </summary>
        public static int ClassIDWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative.characterClassID;

        /// <summary>
        /// Get an item from a requested inventory.
        /// </summary>
        /// <param name="inventory">The target inventory to search from.</param>
        /// <param name="itemID">The queried item.</param>
        /// <returns>The requested item, or null if it does not exist.</returns>
        public static bool ContainsItem(this List<InventoryItem> inventory, int itemID)
        {
            return inventory.FirstOrDefault(item => item.id == itemID) != null;
        }

        /// <summary>
        /// Finds the first available enemy with health greater than zero, and performs the standard mana-less attack.
        /// </summary>
        public static void FallbackAttack()
        {
            Hero target = TeamHeroCoder.BattleState.foeHeroes.FirstOrDefault(hero => hero.health > 0)!;
            int defaultAbilityID = TeamHeroCoder.AbilityID.Attack;

            if (target != null) TeamHeroCoder.PerformHeroAbility(defaultAbilityID, target);
            else Console.WriteLine("Null target. No action taken. This will halt turn cycle.");
            
        }

        public static void HandleTurn(int currentHeroID)
        {
            switch (currentHeroID)
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
                    Console.WriteLine("No valid class given.");
                    FallbackAttack();
                    break;
            }
        }

        /*
         * By modularizing the classes this way, it is extremely easy to modify a specific classes priority for actions.
         * For example, if every class were to share the same function, it would hypothetically be possible to ask the system to
         * cast fireball as a fighter, which is not possible. If I wanted to completely rework my parties logic, all I have to do 
         * is go into the function designated to that class and rewrite it.
        */

        private static void HandleTurnFighter()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            FallbackAttack();
        }

        private static void HandleTurnWizard()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            FallbackAttack();
        }

        private static void HandleTurnCleric()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            //FallbackAttack();

            #region Shorthand Variables
            List<Hero> partyMembers = TeamHeroCoder.BattleState.playerHeroes;
            Hero targetHero;
            
            List<InventoryItem> playerInventory = TeamHeroCoder.BattleState.playerInventory;
            int elixirItemID = TeamHeroCoder.ItemID.Elixir;
            int elixirAbilityID = TeamHeroCoder.AbilityID.Elixir;

            #endregion

            //Declaration region for AbilityData, for ease of access later.
            #region ClericAbilityData

            AbilityData resurrection = new()
            {
                ID = TeamHeroCoder.AbilityID.Resurrection,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Resurrection),
            };

            AbilityData cureLight = new()
            {
                ID = TeamHeroCoder.AbilityID.CureLight,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.CureLight),
            };

            AbilityData cureSerious = new()
            {
                ID = TeamHeroCoder.AbilityID.CureSerious,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.CureSerious),
            };

            AbilityData faith = new()
            {
                ID = TeamHeroCoder.AbilityID.Faith,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Faith),
            };

            AbilityData brave = new()
            {
                ID = TeamHeroCoder.AbilityID.Brave,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Brave),
            };

            #endregion

            if(playerInventory.ContainsItem(elixirItemID) && HeroWithInitiative.mana <= HeroWithInitiative.maxMana * 0.3f)
            {
                targetHero = HeroWithInitiative;
                int useElixirID = TeamHeroCoder.AbilityID.Elixir;
                TeamHeroCoder.PerformHeroAbility(useElixirID, targetHero);
                return;
            }
            
            if (partyMembers.Any(ally => ally.health < ally.maxHealth * 0.5f))
            {
                targetHero = partyMembers.OrderBy(ally => ally.health / (float)ally.maxHealth).First();
                
                int cureSpellID =
            }
        }

        private static void HandleTurnRogue()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            FallbackAttack();
        }

        private static void HandleTurnMonk()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            FallbackAttack();
        }

        private static void HandleTurnAlchemist()
        {
            //Placeholder, acts as a failsafe if party member was added but player forgot to update class function.
            //Comment out when not in use.
            FallbackAttack();
        }
    }

    /// <summary>
    /// Helper struct to condense required ability data into one place. As a bonus, writing it this way more closely fits my coding style.
    /// </summary>
    public readonly struct AbilityData
    {
        public readonly int ID { get; init; }
        public readonly int Cost { get; init; }

        public AbilityData(int abilityID, int manaCost)
        {
            ID = abilityID;
            Cost = manaCost;
        }
    }
}
