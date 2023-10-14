namespace PlayerCoder;

/// <summary>
/// Helper struct to condense required ability data into one place. As a bonus, writing it this way more closely fits my coding style.
/// </summary>
public readonly struct AbilityData
{
    /// <summary>
    /// The ID of the ability.
    /// </summary>
    public readonly int ID { get; init; }

    /// <summary>
    /// The mana cost of the ability.
    /// </summary>
    public readonly int Cost { get; init; }

    /// <summary>
    /// Constructor defaults mana cost to -1, in the case of abilities such as Elixir or Potion.
    /// </summary>
    /// <param name="abilityID">The internal ID of the ability.</param>
    /// <param name="manaCost">Cost of the ability. Leave empty if not mana-based ability.</param>
    public AbilityData(int abilityID, int manaCost = -1)
    {
        ID = abilityID;
        Cost = manaCost;
    }
}