using System;
using System.Net;
using System.Threading;
using Godot.Collections;
using System.Text;
using System.IO;
using Game.Utils;
using System.Text.RegularExpressions;


namespace Terminal
{
    public struct RequestEntry
    {
        public string httpMethod;
        public bool recieveJSONData;
        public TerminalServer.RequestEntryFunc function;
    }
    public struct RequestEntryArgs
    {
        public HttpListenerContext context;
        public Dictionary<string, Godot.Object> jsonData;
        public string stringData;
    }
    public class TerminalServer
    {
        public static TerminalServer Instance { get { return _instance; } }
        private static TerminalServer _instance;

        Thread serverThread = new Thread(StartServerListening);
        static Mutex mutex = new Mutex();
        public HttpListener listener = new HttpListener();
        static int listeningInterval = 100;
        public string serverURL = "http://localhost:8080/";
        string projectRootPath = @"D:\Users\user\Documents\GODOT\Godot_Projects\warming-place";
        string sitePath = @"\x-scripts-editor\build";
        string rootPath { get { return projectRootPath + sitePath; } }

        GameEventsProvider gameEventsProvider = new GameEventsProvider();

        public delegate void RequestEntryFunc(RequestEntryArgs args);
        public System.Collections.Generic.Dictionary<string, RequestEntry> requestEntries = new System.Collections.Generic.Dictionary<string, RequestEntry>();

        public Regex filenameCheckRegex;


        public TerminalServer()
        {
            _instance = this;

            string pattern = @"^[\w\-. ]+\.[\w]{1,}$"; // Регулярное выражение для проверки названия файла с расширением
            filenameCheckRegex = new Regex(pattern);

            FillDefaultEntries();
        }

        void FillDefaultEntries()
        {
            AddRequestEntry("/post-run-code", "POST", EntryRunCode);
            AddRequestEntry("/post-event-sharing", "POST", EntryEventSharing);
        }

        void EntryRunCode(RequestEntryArgs args)
        {
            
        }
        void EntryEventSharing(RequestEntryArgs args)
        {
            EventsPackage recievedPck = EventsPackage.FromJSON((string)args.stringData);
            //string data = "{\"events\":[{\"type\":\"console_content_change\",\"data\":\"new data: " + count.ToString() + "\"}]}";

            if (recievedPck != null)
            {
                mutex.WaitOne();
                gameEventsProvider.ProcessEventsAndGatherInSelf(recievedPck);
                mutex.ReleaseMutex();
            }

            WriteStringInResponse(args.context, gameEventsProvider.eventsPackage.ToJSON());
            gameEventsProvider.eventsPackage.ClearEvents();
        }

        public void AddRequestEntry(string entryURL, string httpMethod, RequestEntryFunc func, bool recieveJSONData=false)
        {
            mutex.WaitOne();
            if (requestEntries.ContainsKey(entryURL)) return;
            RequestEntry newEntry = new RequestEntry();
            newEntry.httpMethod = httpMethod;
            newEntry.function = func;
            newEntry.recieveJSONData = recieveJSONData;
            requestEntries.Add(entryURL, newEntry);
            mutex.ReleaseMutex();
        }

        void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string entryKey = context.Request.Url.LocalPath;
                mutex.WaitOne();
                if (!requestEntries.ContainsKey(entryKey)) return;
                RequestEntry entry = requestEntries[entryKey];
                mutex.ReleaseMutex();
                RequestEntryArgs args = new RequestEntryArgs();
                args.context = context;
                args.jsonData = null;
                args.stringData = "";
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    string requestBody = reader.ReadToEnd();
                    args.stringData = requestBody;
                    if (entry.recieveJSONData)
                    {
                        Dictionary<string, Godot.Object> data = new Dictionary<string, Godot.Object>((Dictionary)Godot.JSON.Parse(requestBody).Result);
                        args.jsonData = data;
                    }
                }

                requestEntries[entryKey].function(args);
            }
            catch (Exception ex)
            {
                mutex.WaitOne();
                Console.WriteLine("Ошибка: " + ex.Message + "\n" + ex.StackTrace);
                mutex.ReleaseMutex();
            }
            finally { }
        }
        void WriteJSONDataInResponse(HttpListenerContext context, Dictionary<string, object> data)
        {
            string jsonData = Godot.JSON.Print(data, indent: "  ");
            WriteStringInResponse(context, jsonData);
        }
        void WriteStringInResponse(HttpListenerContext context, string data)
        {
            HttpListenerResponse response = context.Response;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;

            byte[] responseData = Encoding.UTF8.GetBytes(data);
            response.ContentLength64 = responseData.Length;
            response.OutputStream.Write(responseData, 0, responseData.Length);
            response.OutputStream.Close();
        }
        public void Start()
        {
            serverThread.Start();
        }
        public void GameReadyState()
        {
            gameEventsProvider.Initialize();
        }
        public void Close()
        {
            Instance.listener.Stop();
            //serverThread.Join();

            mutex.WaitOne();
            Console.WriteLine("Server stopped listening.");
            mutex.ReleaseMutex();
        }
        public void ResponseWithLocalFile(HttpListenerContext context, string urlFilePath)
        {
            // Формирование полного пути к запрашиваемому файлу
            string filePath = rootPath + urlFilePath;

            // Проверка существования файла
            if (System.IO.File.Exists(filePath))
            {
                // Чтение содержимого файла
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Отправка ответа с содержимым файла
                context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                context.Response.OutputStream.Close();
            }
            else
            {
                // Файл не найден
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
        }
        public static void StartServerListening()
        {
            // Путь к папке, содержащей index.html и другие файлы
            string rootPath = Instance.rootPath;

            // Создание объекта HttpListener

            // Указание префикса URL для прослушивания
            Instance.listener.Prefixes.Add(Instance.serverURL);

            try
            {
                // Запуск прослушивания
                Instance.listener.Start();
                Console.WriteLine(String.Format("Server started at \"{0}\" ...", Instance.serverURL));

                while (Instance.listener.IsListening)
                {
                    // Ожидание входящего запроса
                    HttpListenerContext context = Instance.listener.GetContext();

                    // Проверка на предварительный запрос CORS (OPTIONS)
                    if (context.Request.HttpMethod == "OPTIONS")
                    {
                        HandleCorsPreflightRequest(context);
                    }
                    else
                    {
                        HandleCorsActualRequest(context);
                    }

                    // Получение пути запрошенного URL
                    string requestedUrl = context.Request.Url.LocalPath;

                    // Проверка, является ли запрос корневым и, если да, замена на index.html
                    if (requestedUrl == "/" || requestedUrl == "")
                        requestedUrl = "/index.html";

                    string filename = requestedUrl.Substring(requestedUrl.LastIndexOf("/")+1);
                    if (Instance.filenameCheckRegex.IsMatch(filename))
                    {
                        Instance.ResponseWithLocalFile(context, requestedUrl);
                    }
                    else
                    {
                        Instance.ProcessRequest(context);
                    }
                    Thread.Sleep(listeningInterval);
                }
            }
            catch (Exception ex)
            {
                mutex.WaitOne();
                Console.WriteLine("Ошибка: " + ex.Message + "\n" + ex.StackTrace);
                mutex.ReleaseMutex();
            }
            finally
            {
                Instance.Close();
            }
        }
        static void HandleCorsPreflightRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            // Установка заголовков CORS для разрешения доступа из разных источников
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            response.StatusCode = (int)HttpStatusCode.OK;
        }

        static void HandleCorsActualRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Установка заголовков CORS для разрешения доступа из разных источников
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }
    }
}