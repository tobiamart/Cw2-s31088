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

    public virtual void Unload(double mass)
    {
        LoadMass -= mass;
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

        }
        else
        {
            base.Load(mass);
        }
    }

    public void Notify(){
        Console.WriteLine($"Próba przekroczenia bezpiecznej ładowności kontenera {SerialNumber}.");
    }
}

public class GasContainer : Container, IHazardNotifier{
    public GasContainer(double ownWeight, double height, double depth, double maxCapacity) : base("G", ownWeight, height, depth, maxCapacity){}

    public override void Unload(double mass){
        if(mass > 0.95*LoadMass){
            Notify();
        }
        else{
            base.Unload(mass);
        }
    }

    public void Notify(){
        Console.WriteLine($"Nie można wyładować więcej niż 95% gazu z kontenera: {SerialNumber}");
    }
}

public class Program
{
    public static void Main()
    {
        try
        {
           LiquidContainer container1 = new LiquidContainer(100, 20, 20, 2000, true);
           LiquidContainer container2 = new LiquidContainer(100, 20, 20, 2000, false);
           container2.Load(1000);
           container2.Load(1100);

           GasContainer container3 = new GasContainer(100, 20, 20, 1000);
           container3.Load(1000);
           container3.Unload(950);
        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}
