using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;

namespace SecuroteckClient
{

    internal class AsyncSynchronizationContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback postback, object state)
        {
            try
            {
                postback(state);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred, error message: ");
                Console.WriteLine(e.Message);
            }
        }

        public override void Post(SendOrPostCallback postback, object state)
        {
            try
            {
                postback(state);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred, error message: ");
                Console.WriteLine(e.Message);
            }
        }
    }


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


        private static string _apiKey = string.Empty;
        private static string UserName = string.Empty;
        private static string ServerPublicKey;
        private static User Current = null;

        private static AsyncSynchronizationContext synchronizationContext;



        private static readonly Dictionary<string, Delegate> MethodLookup = new Dictionary<string, Delegate>
        {
            {"TalkBack Hello", new Func<Task>(TalkBackHello)},
            {"TalkBack Sort [", new Func<string, Task>(TalkBackSort)},
            {"User Get", new Func<string, Task>(UserGet)},
            {"User Post", new Func<string, Task>(UserPost)},
            {"User Set", new Func<string[], Task>(UserSet)},
            {"User Delete", new Func<Task>(UserDelete)},
            {"Protected Hello", new Func<Task>(ProtectedHello)},
            {"Protected SHA1", new Func<string, Task>(ProtectedSha1)},
            {"Protected SHA256", new Func<string, Task>(ProtectedSha256)},
            {"Protected Get PublicKey", new Func<Task>(ProtectedGetPublicKey) },
            {"Protected Sign", new Func<string, Task>(ProtectedSignMessage) },
            {"Protected AddFifty", new Func<string, Task>(ProtectedAddFifty) },
            {"Exit", new Action(() => OnCancel(null, null))}

        };

        private static readonly Dictionary<string, Delegate> MethodLookupRegex = new Dictionary<string, Delegate>
        {
            {"TalkBack Hello", new Func<Task>(TalkBackHello)},
            {@"TalkBack Sort \[[0-9]+(,[0-9]+)*\]", new Func<string, Task>(TalkBackSort)},
            {"User Get", new Func<string, Task>(UserGet)},
            {"User Post", new Func<string, Task>(UserPost)},
            {"User Set", new Func<string[], Task>(UserSet)},
            {"User Delete", new Func<Task>(UserDelete)},
            {"Protected Hello", new Func< Task>(ProtectedHello)},
            {"Protected SHA1", new Func<string, Task>(ProtectedSha1)},
            {"Protected SHA256", new Func<string, Task>(ProtectedSha256)},
            {"Protected Get PublicKey", new Func<Task>(ProtectedGetPublicKey) },
            {"Protected Sign", new Func<string, Task>(ProtectedSignMessage) },
            {"Protected AddFifty", new Func<string, Task>(ProtectedAddFifty) },
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
            synchronizationContext = new AsyncSynchronizationContext();



            await ProcessInput(Console.ReadLine());

            while (true)
            {

                Console.Write("What would you like to do next? ");
                string text = Console.ReadLine();
                Console.Clear();
                await ProcessInput(text);

            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Console.WriteLine("terminating :(");
            }
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



        public static async Task ProcessInput(string input)
        {


            if (null == SynchronizationContext.Current)
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }


            var results = MethodLookupRegex
                .Where(result => Regex.Match(input, result.Key, RegexOptions.Singleline).Success)
                .Select(result => result.Value).FirstOrDefault();

            var key = MethodLookup.Keys.FirstOrDefault(x => x.StartsWith(input) || input.Contains(x));

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Unrecognized command");
                return;
            }

            var item = MethodLookup[key];
            Console.WriteLine("...please wait...");
            input = input.Replace(key, string.Empty);
            try
            {

                switch (item)
                {
                    case Func<string[], Task> withParams:
                        await withParams(new[] { input });
                        break;
                    case Func<Task> withoutParams:
                        await withoutParams();
                        break;
                    default:
                        throw new InvalidOperationException();
                }


            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred:\n{ e.Message}");

            }



        }




        private static async Task TalkBackHello()
        {
            var request = await _client.GetAsync($"{Endpoint}{TalkBack}hello");

            string result = await request.Content.ReadAsStringAsync();
            Console.WriteLine(result);


        }

        private static async Task TalkBackSort(string param)
        {


            param = param.Replace("[", string.Empty).Replace("]", string.Empty);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(param))
            {

                var split = param.Split(',');

                if (split.Length > 1)
                {
                    foreach (var s in split.Take(split.Length - 1))
                    {
                        sb.Append($"integers={s}&");
                    }
                }

                sb.Append($"integers={split.Last()}");
            }


            var response = await _client.GetAsync($"{Endpoint}{TalkBack}sort?{sb}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());








        }

        private static async Task UserGet(string username)
        {


            var response = await _client.GetAsync($"{Endpoint}{UserController}new?username={username}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task UserPost(string username)
        {
            if (string.IsNullOrEmpty(username))
            {

            }


            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");



            var response = await _client.PostAsync($"{Endpoint}{UserController}new", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {

                var result = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                Console.WriteLine("Got API Key");
                _apiKey = result.ApiKey.ToString();
                UserName = result.UserName;
            }
            else
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
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
            if (Current == null && (_apiKey == null && UserName == null))
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }



            var request = new HttpRequestMessage(HttpMethod.Delete, ($"{Endpoint}{UserController}RemoveUser?username={UserName}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await _client.SendAsync(request);

            var outcome = await response.Content.ReadAsStringAsync();

            if (bool.TryParse(outcome, out var result))
            {
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("False");
            }



        }

        private static async Task ProtectedHello()
        {

            if (Current == null && string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(UserName))
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}hello"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);

            }
            else
            {
                //TODO unauthorized
            }



        }

        private static async Task ProtectedSha1(string text)
        {
            if (_apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }





            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha1?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            else
            {
                //TODO unauthorized
            }



        }

        private static async Task ProtectedSha256(string text)
        {
            if (_apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }





            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha256?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            else
            {
                //TODO unauthorized
            }


        }

        private static async Task ProtectedGetPublicKey()
        {
            if (_apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}getpublickey"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ServerPublicKey = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Got Public Key");
            }
            else
            {
                Console.WriteLine("Couldn’t Get the Public Key");
                //TODO unauthorized
            }


        }

        private static async Task ProtectedSignMessage(string message)
        {
            if (_apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }


            if (ServerPublicKey == null)
            {
                Console.WriteLine("Client doesn't yet have the public key");
                return;
            }




            if (string.IsNullOrEmpty(message))
            {
                //TODO handle this
            }




            var request = new HttpRequestMessage(HttpMethod.Get,
                ($"{Endpoint}{ProtectedController}sign?message={message}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);



            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var encrypted = (await response.Content.ReadAsStringAsync());
                var provider = new RSACryptoServiceProvider
                {
                    PersistKeyInCsp = false,
                    KeySize = 2048
                };

                provider.FromXmlString(ServerPublicKey);





                var result =
                    provider.VerifyHash(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(message)),
                        CryptoConfig.MapNameToOID("SHA1"),
                        encrypted.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray());


                Console.WriteLine(result ? "Message was successfully signed" : "Message was not successfully signed");
            }


        }

        private static async Task ProtectedAddFifty(string message)
        {
            if (_apiKey == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return;
            }


            if (ServerPublicKey == null)
            {
                Console.WriteLine("Client doesn't yet have the public key");
                return;
            }




            if (string.IsNullOrEmpty(message))
            {
                //TODO handle this
            }

            int num;
            if (!int.TryParse(message, out num))
            {
                //TODO handle this
            }


            var provider = new RSACryptoServiceProvider
            {
                PersistKeyInCsp = false,
                KeySize = 2048
            };

            provider.FromXmlString(ServerPublicKey);


            var encryptedInt = BitConverter.ToString(provider.Encrypt(BitConverter.GetBytes(num), true));

            var aes = new AesManaged();

            aes.GenerateKey();
            aes.GenerateIV();
            var key = BitConverter.ToString(provider.Encrypt(aes.Key, true));
            var iv = BitConverter.ToString(provider.Encrypt(aes.IV, true));



            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{Endpoint}{ProtectedController}addfifty?encryptedInteger={encryptedInt}&encryptedsymkey={key}&encryptedIV={iv}");
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var encrypted = (await response.Content.ReadAsStringAsync());

                var decryptor = aes.CreateDecryptor();
                var encryptedBytes = FromHexString(encrypted);

                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);


                try
                {
                    var result = BitConverter.ToInt32(decryptedBytes, 0);
                    Console.WriteLine(result);
                }
                catch (Exception e)
                {
                    if (e is ArgumentOutOfRangeException || e is ArgumentException)
                    {
                        Console.WriteLine("An error occurred!");
                    }
                }
            }
        }

        private static byte[] FromHexString(string input)
        {
            return input.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
        }

    }




    #endregion
}
