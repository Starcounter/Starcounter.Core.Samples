
using System;
using System.Text;
using Starcounter;

namespace MinimalApp
{
    [Database]
    public class Monster
    {
        public virtual void Inserted()
        {
            Name = this.GetType().Name;
            LastSeen = DateTime.Now;
        }

        [Index]
        public virtual string Name { get; set; }
        public virtual DateTime LastSeen { get; set; }

        public virtual void Act()
        {
            Console.WriteLine("It seems to wait.");
        }
    }

    [Database]
    public class Grue : Monster
    {
        public override void Inserted()
        {
            base.Inserted();
            IsBored = true;
        }

        [Index]
        public virtual ulong StomachWeight { get; set; }
        public virtual bool HasEaten { get; set; }
        public virtual bool IsBored { get; set; }

        public void Eat(ulong kg)
        {
            StomachWeight += kg;
        }

        public override void Act()
        {
            if (HasEaten)
            {
                Console.WriteLine("It seems fed.");
                IsBored = true;
            }
            else
            {
                Eat(80);
                IsBored = false;
                Console.WriteLine("It eats an adventurer.");
            }
            if (IsBored)
            {
                Console.WriteLine("It's bored.");
            }
        }
    }
    
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var appHost = new AppHostBuilder()
                .AddCommandLine(args)
                .Build())
            {
                // Start the app host inside the using so that
                // we get cleanup if an exception occurs.
                appHost.Start();
                // We are now connected to the given database
                GrueTesting();
                PrintTableHierarchy();
            }
        }

        static void GrueTesting()
        {
            ulong objectNo = 0;

            Db.Transact(() => {
                var grue = Db.Insert<Grue>();
                grue.Inserted();
                objectNo = grue.GetObjectNo();
                Console.WriteLine("{0}: A {1}{2} appears in a puff of smoke.",
                    grue.LastSeen,
                    grue.IsBored ? "bored " : "",
                    grue.Name
                    );
                grue.Act();
            });

            Db.Transact(() => {
                var grue = Db.FromId<Grue>(objectNo);
                Console.WriteLine("A {1}{2} looms in the darkness, last seen {0}.",
                    grue.LastSeen,
                    grue.IsBored ? "bored " : "",
                    grue.Name
                    );
                grue.Act();
            });

            int grue_count = 0;
            Db.Transact(() => {
                foreach (var grue in Db.SQL<Grue>("SELECT g FROM Grue g"))
                {
                    grue_count++;
                    Console.WriteLine("{0} last seen {1}", grue.Name, grue.LastSeen);
                }
            });
            Console.WriteLine("Db.SQL<Grue>(\"SELECT...\") returns {0} rows", grue_count);
        }

        static void PrintTableHierarchy()
        {
            Db.Transact(() => {
                PrintTablesThatInherit(null, 0);
            });
        }

        static void PrintTablesThatInherit(Starcounter.Metadata.Table parent, int level)
        {
            string indent = (new StringBuilder(level).Insert(0, " ", level * 2)).ToString();
            var result = Db.SQL<Starcounter.Metadata.Table>("SELECT t FROM \"Starcounter.Metadata.Table\" t");
            foreach (var t in result)
            {
                if (object.Equals(t.Inherits, parent))
                {
                    Console.Write("{0}{1}", indent, t.Name);
                    PrintIndexesForTable(t);
                    Console.WriteLine();
                    PrintTablesThatInherit(t, level + 1);
                }
            }
        }

        static void PrintIndexesForTable(Starcounter.Metadata.Table parent)
        {
            var result = Db.SQL<Starcounter.Metadata.Index>("SELECT i FROM \"Starcounter.Metadata.Index\" i");
            int found = 0;
            foreach (var t in result)
            {
                if (t.Table.FullName == parent.FullName)
                {
                    if (found == 0)
                        Console.Write("\t\t[{0}", t.Name);
                    else
                        Console.Write(" {0}", t.Name);
                    found++;
                }
            }
            if (found > 0)
                Console.Write("]");
        }
    }
}
