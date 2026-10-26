using Transoceanic.Primitives.Collections;

int checks = 0;
void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
    checks++;
}

float NoDraw(float maximum) => throw new Exception("Empty and singleton bags must not consume randomness.");
WeightedBag<string> bag = new();
Check(bag.Pick(NoDraw) is null, "Empty pick must return the default value.");
Check(!bag.TryPick(NoDraw, out string empty) && empty is null, "Empty TryPick must fail.");
bag.Add("first", 1f);
Check(bag.Pick(NoDraw) == "first", "Singleton pick must return its only item.");
bag.Add("second", 3f);

foreach ((float sample, string expected) in new[] { (0f, "first"), (0.999f, "first"), (1f, "second"), (3.999f, "second") })
{
    Check(bag.Pick(maximum =>
    {
        Check(maximum == 4f, "The random source must receive the current total weight.");
        return sample;
    }) == expected, "Weight boundaries must select the correct item.");
}

Check(bag.TryPick(_ => 1f, out string selected) && selected == "second", "TryPick must use the supplied random source.");
int draws = 0;
IEnumerable<string> multiple = bag.PickMultiple(3, _ => { draws++; return 0f; });
Check(draws == 0, "Repeated picks must remain lazy.");
Check(multiple.SequenceEqual(new[] { "first", "first", "first" }) && draws == 3, "Each repeated pick must consume one draw.");

List<float> totals = [];
Check(bag.PickDistinct(2, maximum => { totals.Add(maximum); return 0f; }).SequenceEqual(new[] { "first", "second" }), "Distinct picks must remove selected candidates.");
Check(totals.SequenceEqual(new[] { 4f, 3f }), "Distinct picks must update the remaining total weight.");
Check(bag.Count == 2 && bag.TotalWeight == 4f, "Distinct picks must preserve the original bag.");

bag.Add("third", 2f);
Check(bag.Pick(maximum => { Check(maximum == 6f, "Adding an item must invalidate the cached total."); return 5f; }) == "third", "New items must participate in selection.");
bag.Clear();
Check(bag.Count == 0 && bag.TotalWeight == 0f && bag.Pick(NoDraw) is null, "Clear must reset the bag and its cached total.");

string[] values = ["", "first", "second", "third"];
TOIterator<string> iterator = new(values, value => value.Length > 0);
Check(iterator.ToArray().SequenceEqual(new[] { "first", "second", "third" }), "Generic iteration must preserve order and apply its predicate.");
Check(iterator.Count() == 3 && iterator.Count(value => value.Length == 5) == 2, "Filtered counts must use the matching sequence.");
Check(iterator.TryGetFirst(out string first) && first == "first", "The first match must skip filtered values.");
HashSet<string> excluded = ["second"];
TOExclusiveIterator<string> exclusive = new(values, value => value.Length > 0, excluded);
Check(exclusive.ToArray().SequenceEqual(new[] { "first", "third" }), "Exclusion must compose with the predicate.");
Check(!TOIterator<string>.Empty.Any() && TOIterator<string>.Empty.Count() == 0, "Empty generic iteration must remain empty.");

Console.WriteLine($"Passed {checks} deterministic collection checks without Terraria dependencies.");
