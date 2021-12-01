using System.Collections.Immutable;
using BurstBotNET.Shared.Models.Game.Serializables;

namespace BurstBotNET.Shared.Extensions;

public static class BurstExtensions
{
    public static int GetValue(this IEnumerable<Card> cards)
    {
        return cards.Sum(card => card.GetValue().Max());
    }

    public static int GetRealizedValues(this IEnumerable<Card> cards, int? rem = null)
    {
        var cardList = cards.ToList();
        var hasAce = cardList.Any(card => card.Number == 1);
        if (hasAce)
        {
            var nonAceValues = cardList.Where(card => card.Number != 1).GetValue();
            if (rem.HasValue)
            {
                nonAceValues %= rem.Value;
            }

            cardList
                .Where(card => card.Number == 1)
                .Select(card => card.GetValue())
                .Select(values => values.Select(v =>
                {
                    if (rem.HasValue)
                    {
                        return v % rem.Value;
                    }
                    else
                    {
                        return v;
                    }
                }).ToImmutableList())
                .ToImmutableList()
                .ForEach(values =>
                {
                    var max = values.Max();
                    var min = values.Min();
                    nonAceValues += nonAceValues + max > 21 ? min : max;
                });
            return nonAceValues;
        }

        var value = cardList.GetValue();
        return rem.HasValue ? value % rem.Value : value;
    }
}