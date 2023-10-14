using System.Collections.Generic;
using TeamHeroCoderLibrary;

namespace PlayerCoder;

/// <summary>
/// Extensions for shortening methods and increasing readability.
/// </summary>
static class PlayerCoderExtensions
{
    /// <summary>
    /// Get's a hero's mana as a percentage.
    /// </summary>
    /// <returns>Hero's mana as percentage.</returns>
    public static float GetManaPercent(this Hero hero) => hero.mana / (float)hero.maxMana;

    /// <summary>
    /// Get's a Hero's health in terms of a percentage.
    /// </summary>
    /// <returns>Hero's health as percent value.</returns>
    public static float GetHealthPercent(this Hero hero) => hero.health / (float)hero.maxHealth;

    /// <summary>
    /// Checks whether a hero has a given status effect.
    /// </summary>
    /// <param name="statusEffectID">Given status effect ID.</param>
    /// <returns>If the hero has the status effect.</returns>
    public static bool HasStatusEffect(this Hero hero, int statusEffectID) => hero.statusEffects.Any(effect => effect.id == statusEffectID);

    /// <summary>
    /// Get an item from a requested inventory.
    /// </summary>
    /// <param name="itemID">The specified item to search for.</param>
    /// <returns>If the inventory contains the item.</returns>
    public static bool ContainsItem(this List<InventoryItem> inventory, int itemID) => inventory.Any(item => item.id == itemID);

    /// <summary>
    /// Gets the hero who is up next in terms of initiative.
    /// </summary>
    /// <returns>Hero with most initiative.</returns>
    public static Hero GetNextHeroWithInitiative(this BattleState battleState)
        => battleState.playerHeroes
            .Concat(battleState.foeHeroes)
            .OrderBy(hero => hero.initiativePercent)
            .First();

    /// <summary>
    /// Gets the hero who is up next in terms of initiative.
    /// </summary>
    /// <param name="isAlliedHero">Is next hero an ally?</param>
    /// <returns>Hero with most initiative.</returns>
    public static Hero GetNextHeroWithInitiative(this BattleState battleState, out bool isAlliedHero)
    {
        Hero nextHero = battleState.playerHeroes
            .Concat(battleState.foeHeroes)
            .OrderBy(hero => hero.initiativePercent)
            .First();
        isAlliedHero = battleState.playerHeroes.Contains(nextHero);
        return nextHero;
    }

    /// <summary>
    /// Checks if a given hero has enough mana for an ability cast.
    /// </summary>
    /// <returns>Whether the hero has enough mana to cast the specified ability.</returns>
    public static bool EnoughManaForCast(this Hero hero, AbilityData abilityData) => hero.mana >= abilityData.Cost;


    /// <summary>
    /// Selects a target from a specified queue, then casts an ability.
    /// </summary>
    /// <param name="priorityQueue">The target priority queue.</param>
    /// <param name="abilityID">Defaults to basic attack, but can specify any ability.</param>
    public static void CastWithPriorityFoe(Queue<int> priorityQueue, int abilityID = 0)
    {
        while (priorityQueue.Count > 0)
        {
            int preferredID = priorityQueue.Dequeue();
            List<Hero> validTargets = TeamHeroCoder.BattleState.foeHeroes
                .Where(foe => foe.characterClassID == preferredID && foe.health > 0)
                .OrderBy(foe => foe.health)
                .ToList();

            if(validTargets.Count > 0)
            {
                TeamHeroCoder.PerformHeroAbility(abilityID, validTargets[0]);
                break;
            }
        }
    }

    public static void CastWithPriorityAlly(Queue<int> priorityQueue, int abilityID = -1)
    {
        while (priorityQueue.Count > 0)
        {
            int preferredID = priorityQueue.Dequeue();
            List<Hero> validTargets = TeamHeroCoder.BattleState.playerHeroes
                .Where(ally => ally.characterClassID == preferredID)
                .OrderBy(ally => ally.health)
                .ToList();

            if(validTargets.Count > 0 && TeamHeroCoder.AreAbilityAndTargetLegal(abilityID, validTargets[0], true))
            {
                TeamHeroCoder.PerformHeroAbility(abilityID, validTargets[0]);
                break;
            }
        }
}