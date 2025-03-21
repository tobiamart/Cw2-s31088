using System;
using System.Collections.Generic;

public interface IHazardNotifier{
    void Notify();
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public abstract class Container
{
    private static int counter = 1;
    private static HashSet<string> existingNumbers = new HashSet<string>();

    public string SerialNumber { get; set; }
    public double LoadMass { get; set; }
    public double OwnWeight { get; }
    public double Height { get; }
    public double Depth { get; }
    public double MaxCapacity { get; }

    protected Container(string type, double ownWeight, double height, double depth, double maxCapacity)
    {
        OwnWeight = ownWeight;
        Height = height;
        Depth = depth;
        MaxCapacity = maxCapacity;

        
        SerialNumber = GenerateUniqueSerialNumber(type);
    }

    private static string GenerateUniqueSerialNumber(string type)
    {
        string serial = $"KON-{type}-{counter++}";

        return serial;
    }

    public virtual void Load(double mass)
    {
        if (LoadMass + mass > MaxCapacity)
        {
            throw new OverfillException($"Nie można załadować {mass} kg. Przekroczona maksymalna ładowność kontenera {SerialNumber}.");
        }
        LoadMass += mass;
    }

    public void Unload()
    {
        LoadMass = 0;
    }

    public override string ToString()
    {
        return $"Numer: {SerialNumber}, Ładunek: {LoadMass}/{MaxCapacity} kg, Waga własna: {OwnWeight} kg, Wymiary: {Height}x{Depth} cm";
    }
}

public class LiquidContainer: Container, IHazardNotifier{
    public bool IsHazardous{ get; set; }
    public LiquidContainer(double ownWeight, double height, double depth, double maxCapacity, bool isHazardous) : base("L", ownWeight, height, depth, maxCapacity){
        IsHazardous = isHazardous;
    }
    public override void Load(double mass){
        double allowedCapacity = IsHazardous ? MaxCapacity/2 : MaxCapacity * 0.9;

        if (LoadMass + mass > allowedCapacity){
            Notify();
            throw new OverfillException($"Nie można załadować {mass} kg. Przekroczona maksymalna ładowność kontenera {SerialNumber}.");
        }
    }

    void Notify(){
        Console.Write($"Próba przekroczenia bezpiecznej ładowności kontenera {SerialNumber}.");
    }
}

public class Program
{
    public static void Main()
    {
        try
        {

        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}
