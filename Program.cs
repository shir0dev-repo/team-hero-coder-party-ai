using System.Collections;
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

    public class MyAI : PartyAIManager, ICombatHandler
    {
        public override void ProcessAI()
        {
            Console.WriteLine("Processing AI!");
            int activeHeroID = PartyAIExtensions.ClassIDWithInitiative;

            
        }

        void ICombatHandler.HandleTurn(int activeHeroClassID) {
            ICombatHandler handler = this;
            handler.HandleTurn(activeHeroClassID);
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

        }

        public static void HandleTurn(int currentHeroID)
        {

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

    public interface ICombatHandler : IClassActionManager
    {
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
                    Console.WriteLine("No valid class given.");
                    FallbackAttack();
                    break;
            }
        }
    }

    public interface IClassActionManager
    {
        private static Hero HeroWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative;
        virtual void HandleTurnFighter()
        {

        }

        virtual void HandleTurnWizard()
        {

        }

        virtual void HandleTurnCleric()
        {

            #region Shorthand Variables
            List<Hero> partyMembers = TeamHeroCoder.BattleState.playerHeroes;
            Hero targetHero;

            List<InventoryItem> playerInventory = TeamHeroCoder.BattleState.playerInventory;
            int elixirItemID = TeamHeroCoder.ItemID.Elixir;
            int elixirAbilityID = TeamHeroCoder.AbilityID.Elixir;

            int faithEffectID = TeamHeroCoder.StatusEffectID.Faith;
            int braveEffectID = TeamHeroCoder.StatusEffectID.Brave;

            #endregion

            //Declaration region for AbilityData, for ease of access later.
            #region ClericAbilityData

            AbilityData resurrectionAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Resurrection,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Resurrection),
            };

            AbilityData cureLightAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.CureLight,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.CureLight),
            };

            AbilityData cureSeriousAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.CureSerious,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.CureSerious),
            };

            AbilityData faithAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Faith,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Faith),
            };

            AbilityData braveAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Brave,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Brave),
            };

            #endregion

            #region Order of Execution

            //Firstly, check if cleric has more than 30% mana. If not, use a mana potion if available.
            if (playerInventory.ContainsItem(elixirItemID) && HeroWithInitiative.mana <= HeroWithInitiative.maxMana * 0.3f)
            {
                targetHero = HeroWithInitiative;
                int useElixirID = TeamHeroCoder.AbilityID.Elixir;
                TeamHeroCoder.PerformHeroAbility(useElixirID, targetHero);
                return;
            }

            //If low mana but no mana potions, use fallback attack.
            else if (!playerInventory.ContainsItem(elixirItemID) && HeroWithInitiative.mana <= HeroWithInitiative.maxMana * 0.3f)
            {
                FallbackAttack();
                return;
            }

            //Check if party member's health is lower than 50%. Then find the lowest health ally and use the correct corresponding cure spell;
            //serious if <= 20%, light if > 20% && < 50%.
            if (partyMembers.Any(ally => ally.health < ally.maxHealth * 0.5f, out List<Hero> lowHealthAllies))
            {
                //Gets the lowest health teammate who's health is not zero.
                targetHero = lowHealthAllies
                    .Where(ally => ally.health > 0)
                    .OrderBy(ally => ally.health / (float)ally.maxHealth)
                    .First();

                float healthAsPercentage = targetHero.health / (float)targetHero.maxHealth;
                TeamHeroCoder.PerformHeroAbility(healthAsPercentage <= 0.2f ? cureSeriousAbility.ID : cureLightAbility.ID, targetHero);
                return;
            }

            //Check if party contains a wizard, then if he has low mana or faith status. Apply if necessary.
            if (partyMembers.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard && 
            (ally.statusEffects.Any(effect => effect.id == faithEffectID) || ally.mana / (float)ally.maxMana <= 0.3f), out List<Hero> wizards))
            {
                targetHero = wizards.First();

                if (playerInventory.ContainsItem(elixirItemID) && targetHero.mana / targetHero.maxMana <= 0.3f)
                {
                    TeamHeroCoder.PerformHeroAbility(elixirAbilityID, targetHero);
                    return;
                }
                else if (HeroWithInitiative.mana > faithAbility.Cost)
                {
                    TeamHeroCoder.PerformHeroAbility(faithAbility.ID, targetHero);
                    return;
                }
            }

            //Check if party contains a fighter, and apply brave if he does not already have it
            if (partyMembers.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Fighter && 
            !ally.statusEffects.Any(effect => effect.id == braveEffectID), out List<Hero> fighters))
            {
                targetHero = fighters.First();
                if(HeroWithInitiative.mana > braveAbility.Cost)
                {
                    TeamHeroCoder.PerformHeroAbility(braveAbility.ID, targetHero);
                    return;
                }
            }

            FallbackAttack();

            #endregion
        }

        virtual void HandleTurnRogue()
        {

        }

        virtual void HandleTurnMonk()
        {

        }

        virtual void HandleTurnAlchemist()
        {

        }
        virtual void FallbackAttack()
        {
            Hero target = TeamHeroCoder.BattleState.foeHeroes.First(hero => hero.health > 0);
            int defaultAbilityID = TeamHeroCoder.AbilityID.Attack;

            if (target != null) TeamHeroCoder.PerformHeroAbility(defaultAbilityID, target);
            else Console.WriteLine("Null target. No action taken. This will halt turn cycle.");
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

    /// <summary>
    /// Extensions for System.Linq methods, allowing me to more effectively retain variables.
    /// </summary>
    static class LinqExtensions
    {
        public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate, out List<T> result)
        {
            bool satisfyCondition = false;
            result = new List<T>();
            foreach(T element in source)
            {
                if(predicate(element))
                {
                    result.Add(element);
                    satisfyCondition = true;
                }
            }

            return satisfyCondition;
        }
    }
}
