// See https://aka.ms/new-console-template for more information

using System.Data;
using Dapper;
using Npgsql;
using Orders.Core.Repositories;

namespace Orders.ConsoleApp;

public static class Programm
{
    private static string _connStr = "Host=localhost;Port=5432;Database=route256;";
    
    public static void Main(string[] args)
    {
        //PrintData();

        //var repository = new OrdersRepository(_connStr);
        //var task = repository.GetAll();
        //var orders = task.Result;
        
    }

    private static IDbConnection OpenConnection(string connStr)  
    {  
        var conn = new NpgsqlConnection(connStr);  
        conn.Open();  
        return conn;  
    } 

    static void PrintData()  
    {  
        IList<dynamic> list;  
        //2.query  
        using (var conn = OpenConnection(_connStr))  
        {  
            var querySQL = @"SELECT * FROM orders;";
            list = conn.Query(querySQL).ToList(); //<Customer>(querySQL).ToList();  
        }  
        if (list.Count > 0)  
        {  
            Console.WriteLine($"items count {list.Count}'");
            /*foreach (var item in list)  
        {//print  
            Console.WriteLine($"{item.FirstName}'s email is {item.Email}");  
        }  */
        }  
        else  
        {  
            Console.WriteLine("the table is empty!");  
        }  
    }
}