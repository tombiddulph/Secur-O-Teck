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
            {"TalkBack Hello", new Func<Task<string>>(TalkBackHello)},
            {"TalkBack Sort [", new Func<string, Task<string>>(TalkBackSort)},
            {"User Get", new Func<string, Task<string>>(UserGet)},
            {"User Post", new Func<string, Task<string>>(UserPost)},
            {"User Set", new Func<string, Task<string>>(UserSet)},
            {"User Delete", new Func<Task<string>>(UserDelete)},
            {"Protected Hello", new Func<Task<string>>(ProtectedHello)},
            {"Protected SHA1", new Func<string, Task<string>>(ProtectedSha1)},
            {"Protected SHA256", new Func<string, Task<string>>(ProtectedSha256)},
            {"Protected Get PublicKey", new Func<Task<string>>(ProtectedGetPublicKey) },
            {"Protected Sign", new Func<string, Task<string>>(ProtectedSignMessage) },
            {"Protected AddFifty", new Func<string, Task<string>>(ProtectedAddFifty) },
            {"Exit", new Action(() => Environment.Exit(0))}

        };




        static async Task Main(string[] args)
        {

         
            _client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            if (File.Exists(SaveLocation))
            {
                Current = JsonConvert.DeserializeObject<User>(File.ReadAllText(SaveLocation));
            }

            

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
            _client.Dispose();
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

                if (!item.Method.GetParameters().Any())
                {
                    input = null;
                }


                string result = await (Task<string>)item.DynamicInvoke(input);

                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("No result from server.");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred:\n{ e.Message}");

            }



        }




        private static async Task<string> TalkBackHello()
        {
            var request = await _client.GetAsync($"{Endpoint}{TalkBack}hello");

            return await request.Content.ReadAsStringAsync();
        }

        private static async Task<string> TalkBackSort(string param)
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
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> UserGet(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                //TODO ask john again
            }

            var response = await _client.GetAsync($"{Endpoint}{UserController}new?username={username}");

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> UserPost(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                //TODO ask john again
            }


            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");



            var response = await _client.PostAsync($"{Endpoint}{UserController}new", content);
            string resultString = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {

                var result = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                resultString = "Got API Key";
                _apiKey = result.ApiKey.ToString();
                UserName = result.UserName;
            }
            else
            {
                resultString = await response.Content.ReadAsStringAsync();
            }

            return resultString;
        }

        private static async Task<string> UserSet(string args)
        {

            string[] split = args.Split(' ');

            if (split.Length != 2)
            {

            }

            if (string.IsNullOrEmpty(split[0]))
            {
                //TODO ask john what to do
            }

            if (Guid.TryParse(split[1], out var apiKey))
            {
                try
                {
                    Current = new User { ApiKey = apiKey, UserName = split[0] };
                    File.WriteAllText(SaveLocation, JsonConvert.SerializeObject(Current));
                    return "Stored";

                }
                catch (System.Security.SecurityException se)
                {
                    return se.Message;
                }
            }
            else
            {
                //TODO ask john what to do
            }

            return string.Empty;

        }

        private static async Task<string> UserDelete()
        {
            if (Current == null && (_apiKey == null && UserName == null))
            {
                return "You need to do a User Post or User Set first";
            }



            var request = new HttpRequestMessage(HttpMethod.Delete, ($"{Endpoint}{UserController}RemoveUser?username={UserName}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await _client.SendAsync(request);

            var outcome = await response.Content.ReadAsStringAsync();




            return bool.TryParse(outcome, out var result) ? result.ToString() : "false";
        }

        private static async Task<string> ProtectedHello()
        {

            if (Current == null && string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(UserName))
            {
                return "You need to do a User Post or User Set first";
            }



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}hello"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);


            string result = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = await response.Content.ReadAsStringAsync();


            }
            else
            {
                //TODO unauthorized
            }

            return result;

        }

        private static async Task<string> ProtectedSha1(string text)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }





            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha1?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await _client.SendAsync(request);


            string result = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                //TODO unauthorized
            }

            return result;
        }

        private static async Task<string> ProtectedSha256(string text)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }

            

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha256?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);


            string result = string.Empty;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                //TODO unauthorized
            }

            return result;
        }

        private static async Task<string> ProtectedGetPublicKey()
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}getpublickey"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await _client.SendAsync(request);


            string result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ServerPublicKey = await response.Content.ReadAsStringAsync();
                result = "Got Public Key";
            }
            else
            {
                result = "Couldn’t Get the Public Key";
                //TODO unauthorized
            }

            return result;
        }

        private static async Task<string> ProtectedSignMessage(string message)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }


            if (ServerPublicKey == null)
            {
                return "Client doesn't yet have the public key";
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
                bool result;
                using (var provider = new RSACryptoServiceProvider
                {
                    PersistKeyInCsp = false,
                    KeySize = 2048
                })
                {
                    provider.FromXmlString(ServerPublicKey);





                    result = provider.VerifyHash(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(message)), CryptoConfig.MapNameToOID("SHA1"), encrypted.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray());
                }


                return result ? "Message was successfully signed" : "Message was not successfully signed";
            }

            return string.Empty;
        }

        private static async Task<string> ProtectedAddFifty(string message)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }


            if (ServerPublicKey == null)
            {
                return "Client doesn't yet have the public key";
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


            string resultString;
            using (var provider = new RSACryptoServiceProvider
            {
                PersistKeyInCsp = true,
                KeySize = 2048
            })
            {
                provider.FromXmlString(ServerPublicKey);


                var encryptedInt = BitConverter.ToString(provider.Encrypt(BitConverter.GetBytes(num), true));

                using (var aes = new AesManaged())
                {
                    aes.GenerateKey();
                    aes.GenerateIV();
                    var key = BitConverter.ToString(provider.Encrypt(aes.Key, true));
                    var iv = BitConverter.ToString(provider.Encrypt(aes.IV, true));



                    var request = new HttpRequestMessage(HttpMethod.Get,
                        $"{Endpoint}{ProtectedController}addfifty?encryptedInteger={encryptedInt}&encryptedsymkey={key}&encryptedIV={iv}");
                    request.Headers.Add(nameof(_apiKey), _apiKey);

                    var response = await _client.SendAsync(request);

                    resultString = string.Empty;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var encrypted = (await response.Content.ReadAsStringAsync());

                        var decryptor = aes.CreateDecryptor();
                        var encryptedBytes = FromHexString(encrypted);

                        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);


                        try
                        {
                            resultString = BitConverter.ToInt32(decryptedBytes, 0).ToString();

                        }
                        catch (Exception e)
                        {
                            if (e is ArgumentOutOfRangeException || e is ArgumentException)
                            {
                                resultString = "An error occurred!";
                            }
                        }
                    }
                }
            }

            return resultString;
        }


        private static async Task<string> GetRequest(string path)
        {
            string result;

            try
            {
                result = await (await _client.GetAsync(path)).Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured: {e.Message}");
                result = string.Empty;
              
            }

            return result;



        }
        private static byte[] FromHexString(string input)
        {
            return input.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
        }

    }




    #endregion
}
