using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;


class Program {
    static void Main(string[] args) {
        try {
            var carTableCreationQuery = @"
                CREATE TABLE IF NOT EXISTS Cars (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Model TEXT NOT NULL,
                    HourlyPrice REAL NOT NULL,
                    KmPrice REAL NOT NULL
                );
            ";
            var carController = new CarController("Data Source=main.db", "Cars", carTableCreationQuery);

            var clientTableCreationQuery = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Surname TEXT NOT NULL,
                    Email TEXT NOT NULL
                );
            ";
            var clientController = new ClientController("Data Source=main.db", "Clients", clientTableCreationQuery);

            while (true) {
                Console.WriteLine("Choose an option: 'add', 'print', or 'stop'.");
                var userCommand = Console.ReadLine()?.Trim().ToLower();

                switch (userCommand) {
                    case "add":
                        carController.AddItem();
                        break;
                    case "print":
                        carController.PrintItems();
                        break;
                    case "stop":
                        Console.WriteLine("Exiting the program.");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}

public abstract class TableController {
    protected string ConnectionString { get; }
    protected string TableName { get; }

    protected TableController(string connectionString, string tableName, string creationCommand) {
        ConnectionString = connectionString;
        TableName = tableName;
        InitializeTable(creationCommand);
    }

    private void InitializeTable(string creationCommand) {
        using (var connection = new SqliteConnection(ConnectionString)) {
            connection.Open();
            using (var command = connection.CreateCommand()) {
                command.CommandText = creationCommand;
                command.ExecuteNonQuery();
            }
            Console.WriteLine($"Table '{TableName}' initialized.");
        }
    }

    protected List<Dictionary<string, object>> GetAllItems() {
        var items = new List<Dictionary<string, object>>();
        using (var connection = new SqliteConnection(ConnectionString)) {
            connection.Open();
            using (var command = connection.CreateCommand()) {
                command.CommandText = $"SELECT * FROM {TableName}";
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++) {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        items.Add(row);
                    }
                }
            }
        }
        return items;
    }

    public abstract void AddToTable(params string[] attributes);
    public abstract void AddItem();
    public abstract void PrintItems();
}

public class CarController : TableController {
    public CarController(string connectionString, string tableName, string creationCommand)
        : base(connectionString, tableName, creationCommand) { }

    public override void AddToTable(params string[] attributes) {
        using (var connection = new SqliteConnection(ConnectionString)) {
            connection.Open();
            using (var command = connection.CreateCommand()) {
                command.CommandText = $"INSERT INTO {TableName}(Model, HourlyPrice, KmPrice) VALUES (@model, @hourlyPrice, @kmPrice)";
                command.Parameters.AddWithValue("@model", attributes[0]);
                command.Parameters.AddWithValue("@hourlyPrice", Convert.ToDouble(attributes[1]));
                command.Parameters.AddWithValue("@kmPrice", Convert.ToDouble(attributes[2]));
                command.ExecuteNonQuery();
            }
        }
    }

    public override void AddItem() {
        Console.Write("Enter car model: ");
        string model = Console.ReadLine();

        Console.Write("Enter hourly price: ");
        string hourlyPrice = Console.ReadLine();

        Console.Write("Enter kilometer price: ");
        string kmPrice = Console.ReadLine();

        AddToTable(model, hourlyPrice, kmPrice);
        Console.WriteLine("Car added successfully.");
    }

    public override void PrintItems() {
        var cars = GetAllItems();
        Console.WriteLine("Car List:");
        foreach (var car in cars) {
            Console.WriteLine($"Id: {car["Id"]}, Model: {car["Model"]}, Hourly Price: {car["HourlyPrice"]}, Km Price: {car["KmPrice"]}");
        }
    }
}

public class ClientController : TableController {
    public ClientController(string connectionString, string tableName, string creationCommand)
        : base(connectionString, tableName, creationCommand) { }

    public override void AddToTable(params string[] attributes) {
        using (var connection = new SqliteConnection(ConnectionString)) {
            connection.Open();
            using (var command = connection.CreateCommand()) {
                command.CommandText = $"INSERT INTO {TableName}(Name, Surname, Email) VALUES (@name, @surname, @email)";
                command.Parameters.AddWithValue("@name", attributes[0]);
                command.Parameters.AddWithValue("@surname", attributes[1]);
                command.Parameters.AddWithValue("@email", attributes[2]);
                command.ExecuteNonQuery();
            }
        }
    }

    public override void AddItem() {
        Console.Write("Enter client name: ");
        string name = Console.ReadLine();

        Console.Write("Enter client surname: ");
        string surname = Console.ReadLine();

        Console.Write("Enter client email: ");
        string email = Console.ReadLine();

        AddToTable(name, surname, email);
        Console.WriteLine("Client added successfully.");
    }

    public override void PrintItems() {
        var clients = GetAllItems();
        Console.WriteLine("Client List:");
        foreach (var client in clients) {
            Console.WriteLine($"Id: {client["Id"]}, Name: {client["Name"]}, Surname: {client["Surname"]}, Email: {client["Email"]}");
        }
    }
}
