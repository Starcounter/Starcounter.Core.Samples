
using System;
using System.Collections.Generic;
using Starcounter.Core;

namespace HelloWorldCore
{
    // NOTE! While we don't have to declare database classes as abstract,
    // by doing so we ensure that new Person() will fail to compile,
    // helping us to find the places we need to do Db.Insert<Person>().
    [Database]
    public abstract class Person
    {
        // NOTE! We need to declare database class fields using
        // public virtual (or abstract) properties with public
        // auto-implemented getter and setter.
        public abstract string FirstName { get; set; }
        public abstract string LastName { get; set; }
    }

    [Database]
    public abstract class Spender : Person
    {
        // This property won't be stored in the database since it fails
        // the requirements listed above (it's neither writeable nor virtual).
        public IEnumerable<Expense> Expenses
            => Db.SQL<Expense>("SELECT e FROM Expense e WHERE e.Spender = ?", this);

        // Current QP implementation can't do SUM(), but this is probably just as fast.
        //  => Db.SQL<decimal>("SELECT SUM(e.Amount) FROM Expense e WHERE e.Spender = ?", this).First;
        public decimal CurrentBalance
        {
            get
            {
                decimal sum = 0;
                foreach (var e in Expenses)
                    sum += e.Amount;
                return sum;
            }
        }
    }

    [Database]
    public abstract class Expense
    {
        // NOTE! Adding the [Index] attribute to a property
        // will cause an index to be created on it if needed.
        [Index]
        public virtual Spender Spender { get; set; }
        public virtual decimal Amount { get; set; }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            const string dbname = "HelloWorldCoreDatabase";

            // Make sure we have a database, create one if not.
            var options = Starcounter.Core.Options.StarcounterOptions.TryOpenExisting(dbname);
            if (options == null)
            {
                System.IO.Directory.CreateDirectory(dbname);
                Starcounter.Core.Bluestar.ScCreateDb.Execute(dbname);
            }

            using (var appHost = new Starcounter.Core.Hosting.AppHostBuilder()
                .UseDatabase(dbname)
                .AddCommandLine(args)
                .Build())
            {
                // Start the app host inside the using so that
                // we get cleanup if an exception occurs.
                appHost.Start();

                // We are now connected to the given database
                // and are free to access it.
                Db.Transact(() =>
                {
                    var anyone = Db.SQL<Spender>("SELECT s FROM Spender s").First;
                    if (anyone == null)
                    {
                        // NOTE! We must use Db.Insert<T>() instead of new T()
                        var p = Db.Insert<Spender>();
                        p.FirstName = "John";
                        p.LastName = "Doe";
                    }
                });
                
                Db.Transact(() =>
                {
                    foreach (var p in Db.SQL<Spender>("SELECT s FROM Spender s"))
                    {
                        Console.WriteLine("Found Spender {0} {1} with balance {2}",
                            p.FirstName, p.LastName, p.CurrentBalance);
                    }
                });
            }
        }
    }
}
