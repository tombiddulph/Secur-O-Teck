using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SecuroteckClient
{



    internal class User
    {
        public Guid ApiKey { get; set; }
        public string UserName { get; set; }
    }



    #region Task 8 and beyond
    class Client
    {
        private const string Endpoint = "http://localhost:24702/api/";
        private const string TalkBack = "talkback/";
        private const string UserController = "user/";
        private const string ProtectedController = "protected/";
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string SaveLocation = $"{Directory.GetCurrentDirectory()}/savedata.json";


        private static string ApiKey = string.Empty;
        private static string UserName = string.Empty;
        private static string ServerPublicKey;
        private static User Current = null;




        private static readonly Dictionary<string, Delegate> MethodLookup = new Dictionary<string, Delegate>
        {
            {"TalkBack Hello", new Func<Task>(TalkBackHello)},
            {"TalkBack Sort [", new Func<string[], Task>(TalkBackSort)},
            {"User Get", new Func<string[], Task>(UserGet)},
            {"User Post", new Func<string[], Task>(UserPost)},
            {"User Set", new Func<string[], Task>(UserSet)},
            {"User Delete", new Func<Task>(UserDelete)},
            {"Protected Hello", new Func<string[], Task>(ProtectedHello)},
            {"Protected SHA1", new Func<string[], Task>(ProtectedSha1)},
            {"Protected SHA256", new Func<string[], Task>(ProtectedSha256)},
            {"Protected Get PublicKey", new Func<Task>(ProtectedGetPublicKey) },
            {"Protected Sign", new Func<string[], Task>(ProtectedSignMessage) },
            {"Exit", new Action(() => OnCancel(null, null))}

        };




        static async Task Main(string[] args)
        {

            //_client.DefaultRequestHeaders.Add("Content-Type", new[] { "application/json" });

            if (File.Exists(SaveLocation))
            {
                Current = JsonConvert.DeserializeObject<User>(File.ReadAllText(SaveLocation));
            }

            //functions.Add("543543", TalkBackSort);

            Console.Write("Hello. What would you like to do? ");

            Console.CancelKeyPress += OnCancel;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;



            await ProcessInput(Console.ReadLine(), true);

            while (true)
            {
                Console.Write("What would you like to do next? ");
                await ProcessInput(Console.ReadLine());

            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("An unhandled exception occured ");
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("\n\n\n\nPress any key to continue");
            Console.ReadKey();
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



            var key = MethodLookup.Keys.FirstOrDefault(x => x.StartsWith(input) || input.Contains(x));

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Unrecognized command");
                return;
            }

            var item = MethodLookup[key];

            if (item.Method.GetParameters().Any())
            {
                if (item is Func<string[], Task> test)
                {
                    Console.WriteLine("Please Wait...");
                    await test(new[] { input });
                }



            }
            else
            {
                if (item is Func<Task> task)
                {
                    await task();
                }
            }

        }


        public static void TaskCompleted(IAsyncResult result)
        {

        }

        private static async Task TalkBackHello()
        {
            var request = _client.GetAsync($"{Endpoint}{TalkBack}hello");

            request.ContinueWith(task =>
            {
                task.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                {
                    Console.WriteLine(message.Result);
                }).Wait();
            }).Wait();


        }

        private static async Task TalkBackSort(params string[] args)
        {

            string param = args[0];
            param = param.Replace("TalkBack Sort", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);

            var split = param.Split(',');

            var sb = new StringBuilder();
            foreach (var s in split.Take(param.Length - 1))
            {
                sb.Append($"integers={s}&");
            }

            sb.Append($"integers={split.Last()}");

            Task<HttpResponseMessage> result = _client.GetAsync($"{Endpoint}{TalkBack}sort?{sb.ToString()}");
            string message = string.Empty;
            result.ContinueWith(x =>
            {
                Task<string> test = result.Result.Content.ReadAsStringAsync();
                test.ContinueWith(y => { Console.WriteLine(y.Result); }).Wait();
            }).Wait();






        }

        private static async Task UserGet(params string[] args)
        {
            string username = args[0].Replace("User Get", string.Empty).Trim();


            _client.GetAsync($"{Endpoint}{UserController}new?username={username}").ContinueWith(response =>
               {
                   response.Result.Content.ReadAsStringAsync().ContinueWith(
                       message =>
                       {
                           Console.WriteLine(message.Result);

                       }).Wait();
               }).Wait();
        }

        private static async Task UserPost(params string[] args)
        {
            string username = args[0].Replace("User Post", string.Empty).Trim();


            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


            _client.PostAsync($"{Endpoint}{UserController}new", content).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(
                        key =>
                        {
                            Console.WriteLine("Got API Key");
                            var result = JsonConvert.DeserializeObject<User>(key.Result);

                            ApiKey = result.ApiKey.ToString();
                            UserName = result.UserName;
                        }).Wait();

                }
                else
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(
                        y => { Console.WriteLine(y.Result); }).Wait();
                }

            }).Wait();
        }

        private static async Task UserSet(params string[] args)
        {
            string[] parameters = args[0].Replace("User Set", string.Empty).Split(' ');


            if (string.IsNullOrEmpty(parameters[0]))
            {
                //TODO ask john what to do
            }

            if (Guid.TryParse(parameters[1], out var apiKey))
            {
                try
                {
                    Current = new User { ApiKey = apiKey, UserName = parameters[0] };
                    File.WriteAllText(SaveLocation, JsonConvert.SerializeObject(Current));
                    Console.WriteLine("Stored");
                }
                catch (System.Security.SecurityException se)
                {

                }
            }
            else
            {
                //TODO ask john what to do
            }



        }

        private static async Task UserDelete()
        {
            if (Current == null && (ApiKey == null && UserName == null))
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }



            var request = new HttpRequestMessage(HttpMethod.Delete, ($"{Endpoint}{UserController}RemoveUser?username={UserName}"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(response =>
            {
                response.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                {
                    if (bool.TryParse(message.Result, out bool result))
                    {
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("False");
                    }
                }).Wait();
            }).Wait();

        }

        private static async Task ProtectedHello(params string[] args)
        {

            if (Current == null && (ApiKey == null && UserName == null))
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}hello"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                    {
                        Console.WriteLine(message.Result);
                    }).Wait();
                }
                else
                {
                    //TODO unauthorized
                }

            }).Wait(); ;


        }

        private static async Task ProtectedSha1(params string[] args)
        {
            if (ApiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }

            var text = args[0];



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha1?message={text}"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                    {
                        Console.WriteLine(message.Result);
                    }).Wait();
                }
                else
                {
                    //TODO unauthorized
                }

            }).Wait(); ;

        }

        private static async Task ProtectedSha256(params string[] args)
        {
            if (ApiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }

            var text = args[0];



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha256?message={text}"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                    {
                        Console.WriteLine(message.Result);
                    }).Wait();
                }
                else
                {
                    //TODO unauthorized
                }

            }).Wait(); ;
        }

        private async static Task ProtectedGetPublicKey()
        {
            if (ApiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}getpublickey"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(message =>
                    {
                        ServerPublicKey = message.Result;
                        Console.WriteLine("Got Public Key");
                    }).Wait();
                }
                else
                {
                    Console.WriteLine("Couldn’t Get the Public Key");
                }

            }).Wait(); ;
        }

        private static async Task ProtectedSignMessage(params string[] args)
        {
            if (ApiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }


            if (ServerPublicKey == null)
            {
                Console.WriteLine("Client doesn't yet have the public key");
                return;
            }


            var message = args[0].Replace("Protected Sign", string.Empty).Trim();

            if (string.IsNullOrEmpty(message))
            {
                //TODO handle this
            }




            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sign?message={message}"));
            request.Headers.Add(nameof(ApiKey), ApiKey);

            _client.SendAsync(request).ContinueWith(task =>
            {
                if (task.Result.StatusCode == HttpStatusCode.OK)
                {
                    task.Result.Content.ReadAsStringAsync().ContinueWith(result =>
                    {


                        var provider = new RSACryptoServiceProvider();
                        provider.FromXmlString(ServerPublicKey);
                        var decryptedBytes = provider.DecryptValue(Encoding.ASCII.GetBytes(result.Result));
                        var msg = BitConverter.ToString(decryptedBytes);

                        Console.WriteLine("Got Public Key");
                    }).Wait();
                }
                else
                {
                    Console.WriteLine("Couldn’t Get the Public Key");
                }

            }).Wait(); ;

        }

    }


 

    #endregion
}
