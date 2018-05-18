using System;

namespace PagarMePOS
{
    class Program
    {
        static void Main(string[] args)
        {
            var pos = new PosService();

            pos.Display("**  Welcome **").Wait();
            
            //pos.UpdateTables().Wait();

            //pos.CreateTransaction(199, PagarMe.Mpos.PaymentMethod.Credit).Wait();

            //pos.CreateTransaction(199, PagarMe.Mpos.PaymentMethod.Debit).Wait();

            Console.WriteLine("Ended");
            Console.Read();
        }
    }
}
