using System;
using System.Collections.Generic;
using System.Xml;

public interface IHazardNotifier{
    void Notify();
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public class WrongContentsException : Exception
{
    public WrongContentsException(string message) : base(message) { }
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
    public string Contents { get; set; }

    protected Container(string type, double ownWeight, double height, double depth, double maxCapacity)
    {
        OwnWeight = ownWeight;
        Height = height;
        Depth = depth;
        MaxCapacity = maxCapacity;
        Contents = "empty";

        
        SerialNumber = GenerateUniqueSerialNumber(type);
    }

    private static string GenerateUniqueSerialNumber(string type)
    {
        string serial = $"KON-{type}-{counter++}";

        return serial;
    }

    public virtual void Load(double mass, string contents)
    {
        if (LoadMass + mass > MaxCapacity)
        {
            throw new OverfillException($"Nie można załadować {mass} kg. Przekroczona maksymalna ładowność kontenera {SerialNumber}.");
        }else if(contents != Contents && Contents != "empty") {
            throw new WrongContentsException($"Nie można załadować {contents} do kontenera: {SerialNumber}, ponieważ zawiera on już produkty: {Contents}");
        }
        LoadMass += mass;
        Contents = contents;
    }

    public virtual void Unload()
    {
        LoadMass = 0;
        Contents = "empty";
    }

    public override string ToString()
    {
        return $"Numer: {SerialNumber}, Ładunek: {Contents}, {LoadMass}/{MaxCapacity} kg, Waga własna: {OwnWeight} kg, Wymiary: {Height}x{Depth} cm";
    }
}

public class LiquidContainer: Container, IHazardNotifier{
    public bool IsHazardous{ get; set; }
    public LiquidContainer(double ownWeight, double height, double depth, double maxCapacity, bool isHazardous) : base("L", ownWeight, height, depth, maxCapacity){
        IsHazardous = isHazardous;
    }
    public override void Load(double mass, string contents){
        double allowedCapacity = IsHazardous ? MaxCapacity/2 : MaxCapacity * 0.9;

        if (LoadMass + mass > allowedCapacity){
            Notify();

        }
        else
        {
            base.Load(mass, contents);
        }
    }

    public void Notify(){
        Console.WriteLine($"Próba przekroczenia bezpiecznej ładowności kontenera: {SerialNumber}.");
    }
}

public class GasContainer : Container, IHazardNotifier{
    public GasContainer(double ownWeight, double height, double depth, double maxCapacity) : base("G", ownWeight, height, depth, maxCapacity){}

    public override void Unload(){
        LoadMass -= LoadMass*0.95;
        Contents = "empty";
    }

    public void Notify(){
        Console.WriteLine($"Niebezpieczne działanie: {SerialNumber}");
    }
}

public class RefridgeratorContainer: Container, IHazardNotifier{

    private static Dictionary<string, double> optimalTemperatures = new Dictionary<string, double>
    {
        {"Bananas", 13.3},
        {"Chocolate", 18},
        {"Fish", 2},
        {"Meat", -15},
        {"Ice cream", -18},
        {"Frozen pizza", -30},
    };

    public double Temperature { get; set; }
    public RefridgeratorContainer(double ownWeight, double height, double depth, double maxCapacity, double temperature) : base("C", ownWeight, height, depth, maxCapacity)
    {
        Temperature = temperature;
    }

    public override void Load(double mass, string contents)
    {
        if(!optimalTemperatures.ContainsKey(contents))
        {
            Console.WriteLine($"{contents} nie jest na liście produktów");
        }else if(optimalTemperatures[contents] < Temperature){
            Notify();
        }else base.Load(mass, contents);
    }

    public void Notify(){
        Console.WriteLine($"Zła temperatura dla zawartości: {SerialNumber}");
    }
}

public class Ship{
    public List<Container> Containers{ get; set; } = new List<Container>();
    public double MaxSpeed{ get; set; }
    public int MaxContainers{ get; set; }
    public double MaxWeight{get; set; } 

    public Ship(double maxSpeed, int maxContainers, double maxWeight){
        MaxSpeed = maxSpeed;
        MaxContainers = maxContainers;
        MaxWeight = maxWeight;
    }

    private double CalculateCurrentWeight(){
        double weight = 0;
        foreach(Container cont in Containers){
            weight += cont.OwnWeight + cont.LoadMass;
        }
        return weight;
    }

    public void AddContainer(Container container){
        double weight = CalculateCurrentWeight() + container.OwnWeight + container.LoadMass;

        if(weight > MaxWeight){
            throw new OverfillException($"Zbyt dużo wagi na kontenerowcu");
        }
        else if(Containers.Count + 1 > MaxContainers){
            throw new OverfillException($"Zbyt dużo kontenerów na kontenerowcu");
        }
        else
        {
        Containers.Add(container);
        }
    }

    public void RemoveContainer(Container container){
        if(Containers.Contains(container)){
            Containers.Remove(container);
        }
        else
        {
            Console.WriteLine($"Nie istnieje na statku kontener: {container.SerialNumber} do usunięcia");        
        }
    }

    public void TransferContainer(Container container, Ship ship){
        if(Containers.Contains(container)){
            Containers.Remove(container);
            ship.AddContainer(container);
        }
        else
        {
            Console.WriteLine($"Nie istnieje na statku kontener: {container.SerialNumber} do przeniesienia");        
        }
    }


    public override string ToString()
    {
        string containers = "";
        foreach(Container cont in Containers){
            containers += cont.SerialNumber + " ";
        }
        return $"Prędkość maksymalna: {MaxSpeed}, Zawartość: {containers}";
    }
}

public class Program
{
    public static void Main()
    {
        try
        {
           Container containerC = new RefridgeratorContainer(100, 150, 150, 2000, 14);
           Container containerG = new GasContainer(100, 150, 150, 2000);
           Container containerL = new LiquidContainer(100, 150, 150, 2000, true);

           containerC.Load(200, "Bananas");
           Console.WriteLine($"{containerC}");

           containerG.Load(200, "Helium");
           containerG.Unload();
           Console.WriteLine($"{containerG}");

           containerL.Load(1900, "Rocket Fuel");
           containerL.Load(1000, "Rocket Fuel");
           Console.WriteLine($"{containerL}");

           Ship ship = new Ship(100, 2, 10000);
           ship.AddContainer(containerC);
           ship.AddContainer(containerL);
           ship.AddContainer(containerG);
           Console.WriteLine($"{ship}");
           

        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }catch (WrongContentsException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}
