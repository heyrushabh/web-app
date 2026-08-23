namespace AnimalFactsAuthApp.Services;

public sealed class AnimalFactService
{
    private static readonly string[] Facts =
    [
        "Octopuses have three hearts.",
        "A group of flamingos is called a flamboyance.",
        "Sea otters sometimes hold hands while resting so they do not drift apart.",
        "Elephants can communicate using low-frequency sounds.",
        "A snail can have thousands of tiny teeth on its radula.",
        "Dolphins use distinctive whistles to identify one another.",
        "Crows can recognize individual human faces.",
        "Giraffes have the same number of neck vertebrae as humans: seven.",
        "Butterflies taste with sensors on their feet.",
        "Polar bears have black skin beneath their fur.",
        "Honeybees communicate food locations through a waggle dance.",
        "Wombat droppings are cube-shaped.",
        "Some penguin species offer pebbles during courtship.",
        "A blue whale's heart can weigh more than 100 kilograms.",
        "Ravens can imitate sounds, including human speech."
    ];

    public string GetRandom() => Facts[Random.Shared.Next(Facts.Length)];
}
