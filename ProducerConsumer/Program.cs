    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading;

namespace ProducerConsumer
{
    public struct ResStruct
    {
        public int bull;
        public int cow;

        public ResStruct(int bull, int cow)
        {
            this.bull = bull;
            this.cow = cow;
        }

        public void AddBull(int i)
        {
            cow += i;
        }
        public void AddCow(int i)
        {
            bull += i;
        }

        public override string ToString()
        {
            return $"Bull: {bull}; Cow: {cow}";
        }
    }

    /*
     * Закомментированный вариант создает проблему с синхронизацией завершения работы
     */

    class Program
    {
        public static BlockingCollection<string> Collection = new BlockingCollection<string>(10);
        public static BlockingCollection<string> TryingCollection = new BlockingCollection<string>();
        //public static BlockingCollection<ResStruct> ResultEnumerable = new BlockingCollection<ResStruct>(10);
        public static ObservableCollection<ResStruct> ResultEnumerable = new ObservableCollection<ResStruct>();
        public static CancellationTokenSource TokenSource = new CancellationTokenSource();
        public static CancellationToken StopIt = TokenSource.Token;
        

        public static bool StopEverything;

        Generator generator;

        static void Main(string[] args)
        {
            Generator generator = new Generator();
            Validator validator = new Validator();
            Judge judge = new Judge();

            var producer = new Thread(generator.Generate) {Name = "Producer"};
            var testProducer = new Thread(generator.GenerateTests) {Name = "testProducer"};
            var validatorThread = new Thread(validator.Check) {Name = "validatorThread"};
            //var judgeThread = new Thread(judge.WaitResult);
            producer.Start();
            testProducer.Start();
            validatorThread.Start();
            //judgeThread.Start();

            //judgeThread.Join();
            producer.Join();
            testProducer.Join();
            validatorThread.Join();
            
            Console.WriteLine("Есть результат");
            Console.ReadLine();
        }
    }

    public class Judge
    {
        private static readonly ResStruct trueResult = new ResStruct(4, 0);

        public Judge()
        {
            Program.ResultEnumerable.CollectionChanged += ResultEnumerableOnCollectionChanged;
        }

        private void ResultEnumerableOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach (var item in notifyCollectionChangedEventArgs.NewItems)
            {
                if (item.Equals(trueResult))
                {
                    Program.StopEverything = true;
                    Program.TokenSource.Cancel();
                    Console.WriteLine("Найден результат");
                }

            }
        }

        //public void WaitResult() { 
        //    while (!Program.StopEverything)
        //    {
        //        try
        //        {
        //            var Item = Program.ResultEnumerable.Take(Program.StopIt);
        //            if (Item.Equals(trueResult))
        //            {
        //                Program.StopEverything = true;
        //                Program.TokenSource.Cancel();
        //                Console.WriteLine("Найден результат");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}
    }


    public class Validator
    {
        public void Check()
        {
            while (!Program.StopEverything)
            {
                ResStruct rs = new ResStruct(0,0);
                try
                {
                    var item = Program.TryingCollection.Take(Program.StopIt);
                
                for(int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (item[i] == Program.Collection.First()[j])
                        {
                            if (i == j) rs.AddBull(1);
                            else rs.AddCow(1);
                        }
                    }
                }
                    Console.WriteLine(item + " " + rs);
                    Program.ResultEnumerable.Add(rs);
                }
                catch (Exception e)
                {
                }
            }
        }
    }

    public class Generator
    {
        //public bool NeedGenerate;
        Random rnd = new Random();

        public Generator()
        {
            //NeedGenerate = true;
        }


        public void Generate()
        {
            while (!Program.StopEverything)
            {
                string s = rnd.Next(0, 9).ToString();
                bool accepted;
                while (s.Length < 4)
                {
                    accepted = true;
                    char symbol = rnd.Next(0, 9).ToString().ToCharArray().First();
                    foreach (var q in s)
                    {
                        if (symbol == q) accepted = false;
                    }
                    if (accepted) s += symbol;
                }
                try
                {
                    Program.Collection.Add(s, Program.StopIt);
                }
                catch (Exception e)
                {
                }
            }
        }

        public void GenerateTests()
        {
            while (!Program.StopEverything)
            {
                string s = rnd.Next(0, 9).ToString();
                bool accepted;
                while (s.Length < 4)
                {
                    accepted = true;
                    char symbol = rnd.Next(0, 9).ToString().ToCharArray().First();
                    foreach (var q in s)
                    {
                        if (symbol == q) accepted = false;
                    }
                    if (accepted) s += symbol;
                }
                try
                {
                    Program.TryingCollection.Add(s, Program.StopIt);
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
   
