using TeamHeroCoderLibrary;
using PCE = PlayerCoder.PlayerCoderExtensions;

namespace PlayerCoder;

/// <summary>
/// Interface for dictating class functionality.
/// </summary>
public interface IClassActionManager
{
    protected static Hero HeroWithInitiative => TeamHeroCoder.BattleState.heroWithInitiative;

    private static List<InventoryItem> PlayerInventory => TeamHeroCoder.BattleState.playerInventory;

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

    #region Common

    /// <summary>
    /// Functionality shared with all classes.
    /// </summary>
    /// <returns>If the turn was passed to FinalizeAbilityCast().</returns>
    bool HandleTurnCommon()
    {
        int etherItemID = TeamHeroCoder.ItemID.Ether;
        int healthPotionID = TeamHeroCoder.ItemID.Potion;

        AbilityData etherAbility = new(TeamHeroCoder.AbilityID.Ether);
        AbilityData healthPotionAbility = new(TeamHeroCoder.AbilityID.Potion);

        //Check if players health is low and contains a potion.
        if (HeroWithInitiative.GetHealthPercent() < 0.2f && PlayerInventory.ContainsItem(healthPotionID))
        {
            //If the next hero is not an allied cleric
            Hero nextHero = TeamHeroCoder.BattleState.GetNextHeroWithInitiative(out bool isAlly);
            if (!isAlly || nextHero.characterClassID != TeamHeroCoder.HeroClassID.Cleric)
            {
                FinalizeAbilityCast(healthPotionAbility.ID, HeroWithInitiative);
                return true;
            }
        }

        if (HeroWithInitiative.GetManaPercent() < 0.3f && PlayerInventory.ContainsItem(etherItemID))
        {
            FinalizeAbilityCast(etherAbility.ID, HeroWithInitiative);
            return true;
        }

        return false;
    }

    #endregion

    #region Fighter

    void HandleTurnFighter()
    {
        if (HandleTurnCommon()) return;

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

        if (!HeroWithInitiative.HasStatusEffect(TeamHeroCoder.StatusEffectID.Brave))
        {
            FinalizeAbilityCast(braveAbility.ID, HeroWithInitiative);
            return;
        }

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

    #endregion

    #region Wizard

    void HandleTurnWizard()
    {

    }

    #endregion

    #region Cleric

    void HandleTurnCleric()
    {
        if (HandleTurnCommon()) return;

        int faithEffectID = TeamHeroCoder.StatusEffectID.Faith;
        int braveEffectID = TeamHeroCoder.StatusEffectID.Brave;

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

        //If any allies are dead, prioritize casting resurrection if possible.
        if (DeadPartyMembers.Count > 0 && HeroWithInitiative.EnoughManaForCast(resurrectionAbility))
        {
            FinalizeAbilityCast(resurrectionAbility.ID, DeadPartyMembers.First());
            return;
        }

        Console.WriteLine("Checking low health allies...");

        Hero lowestHealthAlly = LowHealthHerosSorted.First();



        //Find the lowest health ally and use the correct corresponding cure spell;
        //serious if <= 20%, light if > 20% && <= 50%.
        if (lowestHealthAlly.GetHealthPercent() <= 0.5f)
        {
            bool canCastCureLight = HeroWithInitiative.EnoughManaForCast(cureLightAbility);
            bool canCastCureSerious = HeroWithInitiative.EnoughManaForCast(cureSeriousAbility);

            if (lowestHealthAlly.GetHealthPercent() <= 0.25f && canCastCureSerious)
            {
                FinalizeAbilityCast(cureSeriousAbility.ID, lowestHealthAlly);
                return;
            }
            else
            {
                FinalizeAbilityCast(cureSeriousAbility.ID, lowestHealthAlly);
                return;
            }
        }

        bool partyHasWizard = AlivePartyMembers
            .Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard);

        //If party contains a wizard, apply faith status if needed.
        if (partyHasWizard)
        {
            Hero wizard = AlivePartyMembers
                .Where(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Wizard)
                .First(ally => ally.HasStatusEffect(faithEffectID));

            if (wizard != null && !wizard.HasStatusEffect(faithEffectID))
            {
                FinalizeAbilityCast(faithAbility.ID, wizard);
                return;
            }
        }

        bool partyHasFighter = AlivePartyMembers
            .Any(ally => ally.characterClassID == TeamHeroCoder.HeroClassID.Fighter);

        //If party contains a fighter, apply brave status if needed.
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
    }

    #endregion

    #region Rogue

    void HandleTurnRogue()
    {

    }

    #endregion

    #region Monk

    void HandleTurnMonk()
    {

    }

    #endregion

    #region Alchemist

    void HandleTurnAlchemist()
    {

    }

    #endregion

    /// <summary>
    /// Abilities get passed here to verify that they can actually be casted. If not, a default attack is performed instead.
    /// </summary>
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

    /// <summary>
    /// A fallback function called in case of illegal ability cast attempt.
    /// Defaults to attacking the lowest health enemy.
    /// </summary>
    void FallbackAttack()
    {
        Hero target = TeamHeroCoder.BattleState.foeHeroes
            .OrderBy(foe => foe.health)
            .First(hero => hero.health > 0);
        int defaultAbilityID = TeamHeroCoder.AbilityID.Attack;

        if (target != null) TeamHeroCoder.PerformHeroAbility(defaultAbilityID, target);
        else Console.WriteLine("Null target. No action taken. This will halt turn cycle.");
    }

    /// <summary>
    /// Overload for prioritizing a specific order of classes.
    /// </summary>
    /// <param name="classIDPriority">Priority queue of targeted classes.</param>
    void FallbackAttack(Queue<int> classIDPriority) => PCE.CastWithPriorityFoe(classIDPriority);


}