using TeamHeroCoderLibrary;

namespace PlayerCoder
{
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

    public class MyAI : PartyAIManager, ICombatHandler
    {
        public override void ProcessAI()
        {
            Console.WriteLine("Processing AI!");
            int activeHeroID = IClassActionManager.HeroWithInitiative.characterClassID;

            (this as ICombatHandler).HandleTurn(activeHeroID);
        }
    }

    public static class PartyAIExtensions
    {
        /// <summary>
        /// Get an item from a requested inventory.
        /// </summary>
        /// <param name="itemID">The specified item to search for.</param>
        /// <returns>True if inventory contains item, false if not.</returns>
        public static bool ContainsItem(this List<InventoryItem> inventory, int itemID) => inventory.Any(item => item.id == itemID);
    }

    /// <summary>
    /// Wrapper interface designed to decouple Hero methods from the base MyAI class.
    /// Inherits from IClassActionManager to further modularize Hero management.
    /// </summary>
    public interface ICombatHandler : IClassActionManager
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
                    Console.WriteLine("No valid class given.");
                    FallbackAttack();
                    break;
            }
        }
    }

    /// <summary>
    /// Interface for designing class functionality.
    /// </summary>
    public interface IClassActionManager
    {
        protected static Hero HeroWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative;
        private static List<InventoryItem> PlayerInventory => TeamHeroCoder.BattleState.playerInventory;
        private static List<Hero> AlliedHeroes => TeamHeroCoder.BattleState.playerHeroes;

        private static List<Hero> AlivePartyMembers => TeamHeroCoder.BattleState.playerHeroes
                .Where(ally => ally.health > 0)
                .ToList();
        private static List<Hero> DeadPartyMembers => TeamHeroCoder.BattleState.playerHeroes
                .Where(ally => ally.health <= 0)
                .ToList();
        private static List<Hero> LowHealthHerosSorted => AlivePartyMembers
                .Where(ally => ally.health > 0)
                .OrderBy(ally => ally.GetHealthPercent())
                .ToList();

        //Quick lookup table for whether the party contains a specified class.
        private static Dictionary<int, bool> PartyClassLookup => new Dictionary<int, bool>()
        {
            { TeamHeroCoder.HeroClassID.Fighter, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Fighter) },
            { TeamHeroCoder.HeroClassID.Wizard, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard) },
            { TeamHeroCoder.HeroClassID.Cleric, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Cleric) },
            { TeamHeroCoder.HeroClassID.Rogue, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Rogue) },
            { TeamHeroCoder.HeroClassID.Monk, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Monk) },
            { TeamHeroCoder.HeroClassID.Alchemist, AlliedHeroes.Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Alchemist) },
        };

        void HandleTurnFighter()
        {
            AbilityData braveAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Brave,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.Brave),
            };
            AbilityData quickHitAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.QuickHit,
                Cost = TeamHeroCoder.AbilityManaCost.GetManaCostForAbility(TeamHeroCoder.AbilityID.QuickHit)
            };
            AbilityData etherAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Ether,
                Cost = -1,
            };

            int etherItemID = TeamHeroCoder.ItemID.Ether;

            //Priority order of enemies that should be attacked.
            Queue<int> PriorityQueue = new(new[]
                {
                TeamHeroCoder.HeroClassID.Cleric,
                TeamHeroCoder.HeroClassID.Alchemist,
                TeamHeroCoder.HeroClassID.Wizard,
                TeamHeroCoder.HeroClassID.Rogue,
                TeamHeroCoder.HeroClassID.Monk,
                TeamHeroCoder.HeroClassID.Fighter
                }
            );

            if (!HeroWithInitiative.HasStatusEffect(TeamHeroCoder.StatusEffectID.Brave))
            {
                FinalizeAbilityCast(braveAbility.ID, HeroWithInitiative);
                return;
            }

            if (PlayerInventory.ContainsItem(etherItemID) && HeroWithInitiative.GetManaPercent() < 0.2f)
            {
                FinalizeAbilityCast(etherAbility.ID, HeroWithInitiative);
                return;
            }

            if (HeroWithInitiative.mana >= quickHitAbility.Cost)
            {
                while (PriorityQueue.Count > 0)
                {
                    int prioFoeID = PriorityQueue.Dequeue();
                    Hero? foundHero = TeamHeroCoder.BattleState.foeHeroes.FirstOrDefault(foe => foe.characterClassID == prioFoeID);

                    if (foundHero != null)
                    {
                        FinalizeAbilityCast(quickHitAbility.ID, foundHero);
                        return;
                    }

                }
            }
        }

        void HandleTurnWizard()
        {

        }

        void HandleTurnCleric()
        {
            //Shortened variables to make code more readable.
            #region Shorthand Variables

            int etherItemID = TeamHeroCoder.ItemID.Ether;
            int faithEffectID = TeamHeroCoder.StatusEffectID.Faith;
            int braveEffectID = TeamHeroCoder.StatusEffectID.Brave;

            #endregion

            //Declaration region for AbilityData.
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

            AbilityData etherAbility = new()
            {
                ID = TeamHeroCoder.AbilityID.Ether,
                Cost = -1,
            };

            #endregion

            #region PartyData

            Hero lowestHealthAlly = LowHealthHerosSorted.First();
            bool partyHasWizard = AlivePartyMembers
                .Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard);
            bool partyHasFighter = AlivePartyMembers
                .Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Fighter);

            #endregion

            #region Order of Execution

            //If any allies are dead, prioritize casting resurrection if possible.
            if (DeadPartyMembers.Count > 0 && HeroWithInitiative.mana > resurrectionAbility.Cost)
            {
                FinalizeAbilityCast(resurrectionAbility.ID, DeadPartyMembers.First());
                return;
            }

            //Check if hero has more than 30% mana. If not, use a mana potion if available.
            Console.WriteLine("Checking current mana...");
            if (PlayerInventory.ContainsItem(etherItemID) && HeroWithInitiative.GetManaPercent() < 0.3f)
            {
                Console.WriteLine("Mana low! Using Mana Potion!");
                FinalizeAbilityCast(etherAbility.ID, HeroWithInitiative);
                return;
            }

            //If low mana but no mana potions, use fallback attack.
            else if (!PlayerInventory.ContainsItem(etherItemID) && HeroWithInitiative.GetManaPercent() < 0.3f)
            {
                Console.WriteLine("Mana low! Cannot use Mana Potion, attacking instead.");
                FallbackAttack();
                return;
            }

            Console.WriteLine("Mana OK!");


            //Check if party member's health is lower than 50%. Then find the lowest health ally and use the correct corresponding cure spell;
            //serious if <= 20%, light if > 20% && < 50%.
            Console.WriteLine("Checking low health allies...");
            if (lowestHealthAlly.GetHealthPercent() < 0.5f)
            {
                int healAbilityID = lowestHealthAlly.GetHealthPercent() <= 0.2f ?
                    cureSeriousAbility.ID : cureLightAbility.ID;
                FinalizeAbilityCast(healAbilityID, lowestHealthAlly);
                return;
            }

            //If party contains a wizard, check if he has low mana or faith status. Apply if necessary.
            if (partyHasWizard)
            {
                Hero wizard = AlivePartyMembers
                    .Where(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard)
                    .FirstOrDefault(ally => ally.GetManaPercent() < 0.3f || ally.HasStatusEffect(faithEffectID))!;

                if (wizard != null)
                {
                    if (wizard.GetManaPercent() <= 0.3f)
                    { 
                        FinalizeAbilityCast(etherAbility.ID, wizard);
                        return;
                    }
                    else if (!wizard.HasStatusEffect(faithEffectID))
                    {
                        FinalizeAbilityCast(faithAbility.ID, wizard);
                        return;
                    }
                }
            }

            //If party contains a fighter, apply brave if he does not already have it
            if (partyHasFighter)
            {
                Hero notBraveFighter = AlivePartyMembers
                    .Where(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Fighter)
                    .FirstOrDefault(fighter => !fighter.HasStatusEffect(braveEffectID))!;

                if (notBraveFighter != null)
                {
                    FinalizeAbilityCast(braveAbility.ID, notBraveFighter);
                    return;
                }
            }

            FallbackAttack();

            #endregion
        }


        void HandleTurnRogue()
        {

        }

        void HandleTurnMonk()
        {

        }

        void HandleTurnAlchemist()
        {

        }
        void FallbackAttack()
        {
            Hero target = TeamHeroCoder.BattleState.foeHeroes.First(hero => hero.health > 0);
            int defaultAbilityID = TeamHeroCoder.AbilityID.Attack;

            if (target != null) TeamHeroCoder.PerformHeroAbility(defaultAbilityID, target);
            else Console.WriteLine("Null target. No action taken. This will halt turn cycle.");
        }

        void FinalizeAbilityCast(int abilityID, Hero? targetHero)
        {
            if (targetHero == null)
            {
                FallbackAttack();
                return;
            }
            bool legalCast = TeamHeroCoder.AreAbilityAndTargetLegal(abilityID, targetHero, true);

            if (legalCast)
                TeamHeroCoder.PerformHeroAbility(abilityID, targetHero);
            else
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

    public readonly struct EnemyTargetPriority
    {
        public Queue<int> PriorityQueue { get; init; }

        public EnemyTargetPriority(params int[] attackPriority)
        {
            PriorityQueue = new Queue<int>(attackPriority);
        }
    }

    /// <summary>
    /// Extensions for shortening methods, allowing me to more effectively retain variables.
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// Get's a Hero's mana in terms of a percentage.
        /// </summary>
        /// <returns>Hero's mana as percent value.</returns>
        public static float GetManaPercent(this Hero hero) => hero.mana / (float)hero.maxMana;

        /// <summary>
        /// Get's a Hero's health in terms of a percentage.
        /// </summary>
        /// <returns>Hero's health as percent value.</returns>
        public static float GetHealthPercent(this Hero hero) => hero.health / (float)hero.maxHealth;

        /// <summary>
        /// Checks whether a Hero has a given status effect.
        /// </summary>
        /// <param name="statusEffectID">Given status effect ID.</param>
        /// <returns>True if hero has status effect, false if not.</returns>
        public static bool HasStatusEffect(this Hero hero, int statusEffectID) => hero.statusEffects.Any(effect => effect.id == statusEffectID);
    }
}
