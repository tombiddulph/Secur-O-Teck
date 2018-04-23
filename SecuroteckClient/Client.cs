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
    /// <summary>
    /// Client written to the specification as defined the in the ACW 
    /// </summary>
    public class Client
    {
        private string _endpoint = "http://localhost:24702/api/";

        private const string TalkBack = "talkback/";
        private const string UserController = "user/";
        private const string ProtectedController = "protected/";
        private HttpClient _httpClient;
        private readonly string _saveLocation = $"{Directory.GetCurrentDirectory()}/savedata.json";
        private TimeSpan _timeout = TimeSpan.FromMinutes(1);

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

        private Dictionary<string, Delegate> _methodLookup;

        [HandleProcessCorruptedStateExceptions]
        public static async Task Main(string[] args)
        {
            var client = new Client();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += client.UnhandledExceptionHandler;



            client._httpClient = new HttpClient();
            client._httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            client._methodLookup = new Dictionary<string, Delegate>
            {
                {"TalkBack Hello", new Func<Task<string>>(client.TalkBackHello)},
                {"TalkBack Sort", new Func<string, Task<string>>(client.TalkBackSort)},
                {"User Get", new Func<string, Task<string>>(client.UserGet)},
                {"User Post", new Func<string, Task<string>>(client.UserPost)},
                {"User Set", new Func<string, string, Task<string>>(client.UserSet)},
                {"User Delete", new Func<Task<string>>(client.UserDelete)},
                {"Protected Hello", new Func<Task<string>>(client.ProtectedHello)},
                {"Protected SHA1", new Func<string, Task<string>>(client.ProtectedSha1)},
                {"Protected SHA256", new Func<string, Task<string>>(client.ProtectedSha256)},
                {"Protected Get PublicKey", new Func<Task<string>>(client.ProtectedGetPublicKey)},
                {"Protected Sign", new Func<string, Task<string>>(client.ProtectedSignMessage)},
                {"Protected AddFifty", new Func<string, Task<string>>(client.ProtectedAddFifty)},
                {"Exit", new Action(() => Environment.Exit(0))},
                {"Help", new Action(() =>
                {
                   client._methodLookup.Keys.Take(client._methodLookup.Count - 1).ToList().ForEach(Console.WriteLine);
                }) }
            };

            if (File.Exists(client._saveLocation))
            {
                try
                {
                    var text = File.ReadAllText(client._saveLocation);
                    if (!string.IsNullOrEmpty(text))
                    {
                        client._current = JsonConvert.DeserializeObject<User>(text, SerializerSettings);

                        Console.WriteLine(SerializationErrors.Any()
                            ? $"An error occurred loading stored credentials from file {SerializationErrors.First()}"
                            : $"Loaded user credentials from file Username: {client._current.UserName}");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load credentials from file: {e.Message}");
                }
            }

            await client.Run().ConfigureAwait(false);


        }

        private async Task Run()
        {
            Console.Write("Hello. What would you like to do? ");
            try
            {
                await ProcessInput(Console.ReadLine()).ConfigureAwait(false);
                this._timeout = TimeSpan.FromSeconds(15);
                while (true)
                {
                    Console.Write("What would you like to do next? ");
                    string text = Console.ReadLine();
                    Console.Clear();
                    await ProcessInput(text).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            this._httpClient.Dispose();
            Console.WriteLine("An unhandled exception occurred ");
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("\n\n\n\nPress any key to exit");
            Console.ReadKey();
        }

        private void OnCancel(object sender, ConsoleCancelEventArgs e)
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



            var key = _methodLookup.Keys.FirstOrDefault(method => method.StartsWith(input) || input.Contains(method));

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Unrecognized command");
                return;
            }

            object[] parameters = null;
            var item = _methodLookup[key];
            Console.WriteLine("...please wait...");
            try
            {
                int paramaterCount = item.Method.GetParameters().Length;
                if (paramaterCount > 0)
                {
                    input = input.Replace(key, string.Empty).Trim();

                    if (string.IsNullOrEmpty(input) && !key.Equals("TalkBack Sort"))
                    {
                        var names = item.Method.GetParameters().Select(name => name.Name).ToList();
                        Console.WriteLine($"Invalid command Missing parameter, expected parameter{(paramaterCount > 1 ? "s" : string.Empty)}: {string.Join(", ", names)}");
                        return;
                    }

                    parameters = paramaterCount != 1 ? input.Split(' ').Cast<object>().ToArray() : new object[] { input };
                }

                var result = await (Task<string>)item.DynamicInvoke(parameters);

                Console.WriteLine(string.IsNullOrEmpty(result) ? "No result from server." : result);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred processing {key}: {e.Message}");
            }
        }

        public async Task<string> TalkBackHello() => await GetRequest($"{_endpoint}{TalkBack}hello").ConfigureAwait(false);

        public async Task<string> TalkBackSort(string numbers)
        {
            numbers = numbers.ReplaceAll("[", "]");

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(numbers))
            {
                var split = numbers.Split(',');

                if (split.Length > 1)
                {
                    foreach (var s in split.Take(split.Length - 1))
                    {
                        sb.Append($"integers={s}&");
                    }
                }

                sb.Append($"integers={split.Last()}");
            }

            return await GetRequest($"{_endpoint}{TalkBack}sort?{sb}").ConfigureAwait(false);
        }

        private async Task<string> UserGet(string username)
        {
            return await GetRequest($"{_endpoint}{UserController}new?username={username}").ConfigureAwait(false);
        }

        private async Task<string> UserPost(string username)
        {
            var content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(username)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync($"{_endpoint}{UserController}new", content).ConfigureAwait(false);
            string resultString;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _current = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                resultString = "Got API Key";
            }
            else
            {
                resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return resultString;
        }

        private Task<string> UserSet(string username, string apiKey)
        {
            if (!Guid.TryParse(apiKey, out var key))
            {
                return Task.FromResult($"Invalid api key {apiKey}");
            }

            this._current = new User { UserName = username, ApiKey = key };
            File.WriteAllText(_saveLocation, JsonConvert.SerializeObject(_current));

            return Task.FromResult("Stored");
        }

        private async Task<string> UserDelete()
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, ($"{_endpoint}{UserController}RemoveUser?username={_current.UserName}"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            var outcome = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return bool.TryParse(outcome, out var result) ? result.ToString() : "false";
        }

        private async Task<string> ProtectedHello()
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{_endpoint}{ProtectedController}hello"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        }

        private async Task<string> ProtectedSha1(string message)
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{_endpoint}{ProtectedController}sha1?message={message}"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        }

        private async Task<string> ProtectedSha256(string message)
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{_endpoint}{ProtectedController}sha256?message={message}"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<string> ProtectedGetPublicKey()
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, ($"{_endpoint}{ProtectedController}getpublickey"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            string result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _serverPublicKey = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                result = "Got Public Key";
            }
            else
            {
                result = "Couldn’t Get the Public Key";
            }

            return result;
        }

        private async Task<string> ProtectedSignMessage(string message)
        {
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            if (_serverPublicKey == null)
            {
                return "Client doesn't yet have the public key";
            }

            if (string.IsNullOrEmpty(message))
            {
                Console.WriteLine("");
            }

            var request = new HttpRequestMessage(HttpMethod.Get,
                ($"{_endpoint}{ProtectedController}sign?message={message}"));
            request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var encrypted = (await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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
            if (_current == null)
            {
                return "You need to do a User Post or User Set first";
            }

            if (_serverPublicKey == null)
            {
                return "Client doesn't yet have the public key";
            }

            int num;
            if (string.IsNullOrEmpty(message) || !int.TryParse(message, out num))
            {
                return "A valid integer must be given!";
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
                        $"{_endpoint}{ProtectedController}addfifty?encryptedInteger={encryptedInt}&encryptedsymkey={key}&encryptedIV={iv}");
                    request.Headers.Add(nameof(_current.ApiKey), _current.ApiKey.ToString());

                    var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    resultString = string.Empty;

                    if (response.StatusCode != HttpStatusCode.OK) return resultString;
                    var encrypted = (await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    var decryptor = aes.CreateDecryptor();
                    var encryptedBytes = FromHexString(encrypted);

                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    try
                    {
                        resultString = BitConverter.ToInt32(decryptedBytes, 0).ToString();
                    }
                    catch (Exception e)
                    {
                        resultString = "An error occurred!";
                    }
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

            var response = _httpClient.SendAsync(request);
            var timeOut = Task.Delay(this._timeout);
            var resultTask = await Task.WhenAny(timeOut, response).ConfigureAwait(false);

            if (timeOut.IsCompleted || resultTask.Id == timeOut.Id)
            {
                return "Request timed out";
            }

            return await response.Result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<string> GetRequest(string path)
        {
            return await this.GetRequest(path, new(string name, string value)[0]).ConfigureAwait(false);
        }

        private async Task<string> PostRequest(string endpoint, HttpContent body)
        {



            var request = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = body
            });
            var timeOut = Task.Delay(this._timeout);

            var resulttask = await Task.WhenAny(request, timeOut).ConfigureAwait(false);

            if (timeOut.IsCompleted || resulttask.Id == timeOut.Id)
            {
                return "Request timed out";
            }

            return await request.Result.Content.ReadAsStringAsync().ConfigureAwait(false);



        }

        private static byte[] FromHexString(string input) => input?.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();
    }




}
