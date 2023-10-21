using TeamHeroCoderLibrary;
using static TeamHeroCoderLibrary.TeamHeroCoder;

namespace PlayerCoder;

/// <summary>
/// Extensions for shortening methods and increasing readability.
/// </summary>
static class PlayerCoderUtils
{
    /// <summary>
    /// Get's a hero's mana as a percentage.
    /// </summary>
    /// <returns>Hero's mana as percentage.</returns>
    public static float GetManaAsPercent(this Hero hero) => hero.mana / (float)hero.maxMana;

    /// <summary>
    /// Get's a Hero's health in terms of a percentage.
    /// </summary>
    /// <returns>Hero's health as percent value.</returns>
    public static float GetHealthAsPercent(this Hero hero) => hero.health / (float)hero.maxHealth;

    /// <summary>
    /// Checks whether a hero has a given status effect.
    /// </summary>
    /// <param name="statusEffectID">Given status effect ID.</param>
    /// <returns>If the hero has the status effect.</returns>
    public static bool HasStatusEffect(this Hero hero, int statusEffectID) => hero.statusEffects.Any(effect => effect.id == statusEffectID);

    /// <summary>
    /// Checks whether a hero has a given ability.
    /// </summary>
    /// <param name="abilityID">Given ability ID.</param>
    /// <returns>If the hero has the given ability.</returns>
    public static bool HasAbility(this Hero hero, int abilityID) => hero.abilityIDs.Contains(abilityID);

    /// <summary>
    /// Checks whether a hero has enough mana for an ability.
    /// </summary>
    /// <param name="abilityID">Given ability ID.</param>
    /// <returns>If hero's current mana is enough to cast the given ability.</returns>
    public static bool HasEnoughMana(this Hero hero, int abilityID) => hero.mana >= AbilityManaCost.GetManaCostForAbility(abilityID);

    /// <summary>
    /// Get an item from a requested inventory.
    /// </summary>
    /// <param name="itemID">The specified item to search for.</param>
    /// <returns>If the inventory contains the item.</returns>
    public static bool ContainsItem(this List<InventoryItem> inventory, int itemID) => inventory.Any(item => item.id == itemID);

    /// <summary>
    /// Iterates through collection, finding the first Hero to satisfy a predicate.
    /// </summary>
    /// <param name="heroes">Collection of heroes.</param>
    /// <param name="requestedHero">The first hero to satisfy the condition. Default value if none satisfy.</param>
    /// <returns>Whether any hero in collection satisfies the predicate.</returns>
    public static bool HasHero<TInput>(this IEnumerable<TInput> heroes, Func<Hero, bool> predicate, out Hero? requestedHero) where TInput : Hero
    {
        foreach (TInput hero in heroes)
        {
            if (predicate(hero))
            {
                requestedHero = hero;
                return true;
            }
        }

        requestedHero = default;
        return false;
    }

    /// <summary>
    /// If target is null, checks all conditions for hero to be able to cast a multi-target ability.<br></br>
    /// If target is not null, checks all conditions for hero to be able to cast a single target ability.
    /// </summary>
    /// <param name="hero">The given hero.</param>
    /// <param name="abilityID">The given ability.</param>
    /// <param name="targetHero">The target hero (null-safe) for the ability.</param>
    /// <returns>Whether the hero can cast the ability, given the context of if a target was specified or not.</returns>
    public static bool FullyCastable(this Hero hero, int abilityID, Hero? targetHero = null)
    {
        //If targetHero is not null, assume single target cast.
        if (targetHero != null)
        {
            //Return a bool whether all conditions are satisfied for a single target cast.
            return new bool[]
            {
                //True if ability and target are legal.
                AreAbilityAndTargetLegal(abilityID, targetHero, true),
                //True if hero is not silenced.
                !hero.HasStatusEffect(StatusEffectID.Silence),
                //True if hero has ability.
                hero.HasAbility(abilityID),
                //True if hero has enough mana to cast ability.
                hero.HasEnoughMana(abilityID),
                //True if targetHero is alive.
                targetHero.health > 0,
            }.All(check => check is true);
        }

        //If targetHero is null, assume multi-target cast.
        else
        {
            //Return a bool whether all conditions are satisfied for a multi-target cast.
            return new bool[]
            {
                //True if requires target.
                !AbilityID.HasTargetAsRequirement(abilityID),
                //True if hero is not silenced.
                !hero.HasStatusEffect(StatusEffectID.Silence),
                //True if hero has ability.
                hero.HasAbility(abilityID),
                //True if hero has enough mana to cast ability.
                hero.HasEnoughMana(abilityID),
            }.All(check => check is true);
        }
    }

    /// <summary>
    /// Performs checks related to if target is null.<br></br>
    /// If null, checks are based on target-less abilities.<br></br>
    /// If not null, checks are based on targeted abilities.
    /// </summary>
    /// <param name="hero">The hero performing the ability cast.</param>
    /// <param name="abilityID">The casted ability.</param>
    /// <param name="target">The target (null-safe) to cast the ability on.</param>
    /// <returns>Whether the ability was successfully casted.</returns>
    public static bool RequestAbilityCast(this Hero hero, int abilityID, Hero? target = null)
    {
        //Checks whether ability can be cast by hero. Note that if target is null, it will default to the multi-cast conditions instead.
        bool castable = FullyCastable(hero, abilityID, target);

        //Should also be noted that if null is passed as target, FullyCastable() will always check if the ability requires a target.
        if (!castable)
        {
            Console.WriteLine("The ineptitude to cast your ability fills you with rage. (Hero is unable to cast ability.)");
            return false;
        }
        else
        {
            //If target is null, and spell is castable, guaranteed to be multi-casted spell.
            bool multiCastRequest = target == null;

            if (multiCastRequest)
            {
                PerformHeroAbility(abilityID, null);
                return true;
            }
            else
            {
                PerformHeroAbility(abilityID, target);
                return true;
            }
        }
    }

    /// <summary>
    /// Request for casting an ability on a preferred hero class. Only casts if all requirements are met.
    /// </summary>
    /// <param name="hero">The hero making the request.</param>
    /// <param name="abilityID">The requested ability.</param>
    /// <param name="targetPriorityQueue">The desired target priority.</param>
    /// <returns>Whether the iteration could both find a valid target and cast the ability on them.</returns>
    public static bool RequestAbilityCast(this Hero hero, int abilityID, Queue<int> targetPriorityQueue)
    {
        bool noEnemiesStanding = !TeamHeroCoder.BattleState.foeHeroes.Any(foe => foe.health > 0);

        if (noEnemiesStanding)
        {
            Console.WriteLine("The fight's over! Go home! (No enemies with health > 0 remaining.)");
            return false;
        }

        Console.WriteLine("Wanna see me iterate through a sorted queue of integers? (Starting priority iteration.)");

        int index = 0;
        while (targetPriorityQueue.Count > 0)
        {
            index++;
            int prioFoeID = targetPriorityQueue.Dequeue();
            Hero? foundHero = TeamHeroCoder.BattleState.foeHeroes.FirstOrDefault(foe => foe.characterClassID == prioFoeID);

            if (foundHero != null && FullyCastable(hero, abilityID, foundHero))
            {
                string consoleLog = string.Format("Wanna see me do it again? (Found target within queue after {0} attempt{1}.)", index, index > 1 ? "s" : "");
                Console.WriteLine(consoleLog);

                return RequestAbilityCast(hero, abilityID, foundHero);
            }
        }

        return false;
    }

    /// <summary>
    /// A fallback method called in case of error/no other action taken.
    /// </summary>
    public static void FailsafeAttack()
    {
        Hero? target = TeamHeroCoder.BattleState.foeHeroes
            .FirstOrDefault(hero => hero.health > 0);

        if (target != null)
        {
            Console.WriteLine("Your uncertainty drives your strike. (Hero casted default attack.)");
            PerformHeroAbility(AbilityID.Attack, target);
        }
        else
            Console.WriteLine("The invalidity of your target makes you go numb. (Null target; no action taken. This may halt turn cycle.)");
    }
}