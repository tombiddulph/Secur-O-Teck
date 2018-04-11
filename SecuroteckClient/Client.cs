using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SecuroteckClient
{





    internal class User
    {
        public Guid ApiKey { get; set; }
        public string UserName { get; set; }
    }



  
    public class Client
    {
        //private const string Endpoint = "http://localhost:24702/api/";

        private const string Endpoint =
            "https://securoteck.azurewebsites.net/api/";
        private const string TalkBack = "talkback/";
        private const string UserController = "user/";
        private const string ProtectedController = "protected/";
        private HttpClient HttpClient;
        private readonly string SaveLocation = $"{Directory.GetCurrentDirectory()}/savedata.json";
        private TimeSpan timeout = TimeSpan.FromMinutes(1);

        private string _apiKey = string.Empty;
        private string _userName = string.Empty;
        private string _serverPublicKey;
        private User _current = null;


        private static readonly List<string> SerializationErrors = new List<string>();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Error = (s, e) =>
            {
                SerializationErrors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            }
        };

        private Dictionary<string, Delegate> MethodLookup;





        [HandleProcessCorruptedStateExceptions]
        static async Task Main(string[] args)
        {
            var client = new Client();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Console.CancelKeyPress += OnCancel;
            AppDomain.CurrentDomain.UnhandledException += client.UnhandledExceptionHandler;








            client.HttpClient = new HttpClient
            {
                //Timeout = TimeSpan.FromSeconds(15),
            };
            client.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            client.MethodLookup = new Dictionary<string, Delegate>
            {
                {"TalkBack Hello", new Func<Task<string>>(client.TalkBackHello)},
                {"TalkBack Sort [", new Func<string, Task<string>>(client.TalkBackSort)},
                {"User Get", new Func<string, Task<string>>(client.UserGet)},
                {"User Post", new Func<string, Task<string>>(client.UserPost)},
                {"User Set", new Func<string, Task<string>>(client.UserSet)},
                {"User Delete", new Func<Task<string>>(client.UserDelete)},
                {"Protected Hello", new Func<Task<string>>(client.ProtectedHello)},
                {"Protected SHA1", new Func<string, Task<string>>(client.ProtectedSha1)},
                {"Protected SHA256", new Func<string, Task<string>>(client.ProtectedSha256)},
                {"Protected Get PublicKey", new Func<Task<string>>(client.ProtectedGetPublicKey)},
                {"Protected Sign", new Func<string, Task<string>>(client.ProtectedSignMessage)},
                {"Protected AddFifty", new Func<string, Task<string>>(client.ProtectedAddFifty)},
                {"Exit", new Action(() => Environment.Exit(0))}
            };

            if (File.Exists(client.SaveLocation))
            {
                try
                {
                    var text = File.ReadAllText(client.SaveLocation);
                    if (!string.IsNullOrEmpty(text))
                    {
                        client._current = JsonConvert.DeserializeObject<User>(text, SerializerSettings);
                        Console.WriteLine($"Loaded user credentials from file Username: {client._current.UserName}");
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load credentials from file: {e.Message}");

                }
            }

            await client.Run();



        }


        private async Task Run()
        {
            Console.Write("Hello. What would you like to do? ");

            await ProcessInput(Console.ReadLine());
            this.timeout = TimeSpan.FromSeconds(15);
            while (true)
            {

                Console.Write("What would you like to do next? ");
                string text = Console.ReadLine();
                Console.Clear();
                await ProcessInput(text);

            }
        }


        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Console.WriteLine("terminating :(");
            }
            this.HttpClient.Dispose();
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


        private bool ValidateStoredCredentials()
        {
            if (_current == null)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return false;
            }

            return true;
        }

        public async Task ProcessInput(string input)
        {




            var key = MethodLookup.Keys.FirstOrDefault(method => method.StartsWith(input) || input.Contains(method));

            if (string.IsNullOrEmpty(key))
            {

            }

            object[] param = null;
            var item = MethodLookup[key];
            Console.WriteLine("...please wait...");
            try
            {
                string result;
                if (item.Method.GetParameters().Any())
                {
                    input = input.Replace(key, string.Empty);
                    param = new[] { input };
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine($"Unrecognized command: Missing parameter(s)");
                        return;
                    }

                }



                result = await (Task<string>)item.DynamicInvoke(param);


                Console.WriteLine(string.IsNullOrEmpty(result) ? "No result from server." : result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: { e.Message}");
            }
        }




        public async Task<string> TalkBackHello()
        {
            return await GetRequest($"{Endpoint}{TalkBack}hello");
        }

        public async Task<string> TalkBackSort(string param)
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

            return await GetRequest($"{Endpoint}{TalkBack}sort?{sb}");
        }

        private async Task<string> UserGet(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                //TODO ask john again
            }



            return await GetRequest($"{Endpoint}{UserController}new?username={username}");
        }

        private async Task<string> UserPost(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                //TODO ask john again
            }





            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var test = PostRequest($"{Endpoint}{UserController}new", content);


            var response = await HttpClient.PostAsync($"{Endpoint}{UserController}new", content);
            string resultString;

            if (response.StatusCode == HttpStatusCode.OK)
            {

                var result = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                resultString = "Got API Key";
                _apiKey = result.ApiKey.ToString();
                _userName = result.UserName;
            }
            else
            {
                resultString = await response.Content.ReadAsStringAsync();
            }

            return resultString;
        }

        private async Task<string> UserSet(string args)
        {

            string result = string.Empty;

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
                    _current = new User { ApiKey = apiKey, UserName = split[0] };
                    File.WriteAllText(SaveLocation, JsonConvert.SerializeObject(_current));
                    return "Stored";

                }
                catch (Exception e)
                {
                    Console.WriteLine("e.Message");
                }
            }
            else
            {
                //TODO ask john what to do
            }

            return string.Empty;

        }

        private async Task<string> UserDelete()
        {
            if (_current == null && (_apiKey == null && _userName == null))
            {
                return "You need to do a User Post or User Set first";
            }



            var request = new HttpRequestMessage(HttpMethod.Delete, ($"{Endpoint}{UserController}RemoveUser?username={_userName}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await HttpClient.SendAsync(request);

            var outcome = await response.Content.ReadAsStringAsync();




            return bool.TryParse(outcome, out var result) ? result.ToString() : "false";
        }

        private async Task<string> ProtectedHello()
        {

            if (_current == null && string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(_userName))
            {
                return "You need to do a User Post or User Set first";
            }



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}hello"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await HttpClient.SendAsync(request);


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

        private async Task<string> ProtectedSha1(string text)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }





            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha1?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);

            var response = await HttpClient.SendAsync(request);


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

        private async Task<string> ProtectedSha256(string text)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }



            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}sha256?message={text}"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await HttpClient.SendAsync(request);


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

        private async Task<string> ProtectedGetPublicKey()
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{Endpoint}{ProtectedController}getpublickey"));
            request.Headers.Add(nameof(_apiKey), _apiKey);


            var response = await HttpClient.SendAsync(request);


            string result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _serverPublicKey = await response.Content.ReadAsStringAsync();
                result = "Got Public Key";
            }
            else
            {
                result = "Couldn’t Get the Public Key";
                //TODO unauthorized
            }

            return result;
        }

        private async Task<string> ProtectedSignMessage(string message)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }


            if (_serverPublicKey == null)
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



            var response = await HttpClient.SendAsync(request);

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
                    provider.FromXmlString(_serverPublicKey);
                    result = provider.VerifyHash(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(message)), CryptoConfig.MapNameToOID("SHA1"), encrypted.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray());
                }


                return result ? "Message was successfully signed" : "Message was not successfully signed";
            }

            return string.Empty;
        }

        private async Task<string> ProtectedAddFifty(string message)
        {
            if (_apiKey == null)
            {
                return "You need to do a User Post or User Set first";
            }


            if (_serverPublicKey == null)
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
                provider.FromXmlString(_serverPublicKey);


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

                    var response = await HttpClient.SendAsync(request);

                    resultString = string.Empty;

                    if (response.StatusCode != HttpStatusCode.OK) return resultString;
                    var encrypted = (await response.Content.ReadAsStringAsync());

                    var decryptor = aes.CreateDecryptor();
                    var encryptedBytes = FromHexString(encrypted);

                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    resultString = BitConverter.ToInt32(decryptedBytes, 0).ToString();

                }
            }

            return resultString;
        }



        private async Task<string> GetRequest(string path, params (string name, string value)[] headers)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, path);

            if (headers.Any())
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.name, header.value);
                }
            }

            var response = HttpClient.SendAsync(request);
            var timeOut = Task.Delay(this.timeout);
            var resultTask = await Task.WhenAny(timeOut, response);


            if (timeOut.IsCompleted || resultTask.Id == timeOut.Id)
            {
                return "Request timed out";
            }



            return await response.Result.Content.ReadAsStringAsync();

        }

        private async Task<string> GetRequest(string path)
        {
            return await this.GetRequest(path, new(string name, string value)[0]);

        }

        private async Task<string> PostRequest(string endpoint, HttpContent body)
        {




            var request = HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = body
            });
            var timeOut = Task.Delay(this.timeout);

            var resulttask = await Task.WhenAny(request, timeOut);

            if (timeOut.IsCompleted || resulttask.Id == timeOut.Id)
            {
                return "Request timed out";
            }

            return await request.Result.Content.ReadAsStringAsync();




        }


        private static byte[] FromHexString(string input) => input?.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
    }





}
