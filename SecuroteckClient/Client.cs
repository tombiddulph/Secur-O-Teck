using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace SecuroteckClient
{


    internal delegate TOut ParamsFunc<in TIn, out TOut>(params object[] args);

    #region Task 8 and beyond
    class Client
    {
        private const string Endpoint = "http://localhost:24702/api/";
        private const string TalkBack = "talkback/";
        private static readonly HttpClient _client = new HttpClient();



        private static Dictionary<string, ParamsFunc<string, string>> functions =
            new Dictionary<string, ParamsFunc<string, string>>();


        private static Dictionary<string, Action> MethodLookup = new Dictionary<string, Action>
        {
            {"TalkBack Lookup",  () => TalkBackHello() },
            {"TalkBack Sort", () => { TalkBackSort(null);} }
        };

        static async Task Main(string[] args)
        {

            //functions.Add("543543", TalkBackSort);
            Console.Write("Hello. What would you like to do? ");

            Console.CancelKeyPress += OnCancel;


            await ProcessInput(Console.ReadLine(), true);

            while (true)
            {
                Console.Write("What would you like to do next? ");
                await ProcessInput(Console.ReadLine());

            }
        }

        private static void OnCancel(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Press any key to exit");
            Console.Read();
            Environment.Exit(0);
        }

      

        public static async Task ProcessInput(string input, bool first = false)
        {
            if (!first)
            {
                Console.Clear();
            }

            Task task = null;


            foreach (var methodLookupKey in MethodLookup.Keys)
            {
                if (methodLookupKey.StartsWith(input))
                {
                    Action item = MethodLookup[methodLookupKey];
                }
            }

            switch (input)
            {
                case "TalkBack Hello":
                    {
                        task = TalkBackHello();
                        break;
                    }
                case "TalkBack Sort":
                    break;

                case "Exit":
                    {
                        Environment.Exit(0);
                        break;
                    }
                case "":
                case null:
                default:
                    {
                        Console.WriteLine("Unrecognized command");
                        break;
                    }
            }
        }

        private static async Task TalkBackHello()
        {
            var message = await _client.GetAsync($"{Endpoint}{TalkBack}hello");
            Console.WriteLine(await message.Content.ReadAsStringAsync());
        }

        private static async Task TalkBackSort(params int[] numbers)
        {

        }
    }
    #endregion
}
