using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Collections.Generic;

 
namespace HTTPServer
{
    class Rex
    {
        TcpListener Listener; // Объект, принимающий TCP-клиентов
    
        // Запуск сервера
        public Rex(int Port)
        {
            // Создаем "слушателя" для указанного порта
            // Listener = new TcpListener(IPAddress.Any, Port);
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
            Listener.Start(); // Запускаем его

            Console.WriteLine("Сервер запущен: http://127.0.0.1:" + Port);

            // В бесконечном цикле
            while (true)
            {
                // // Принимаем нового клиента
                // TcpClient client = Listener.AcceptTcpClient();
                // // Создаем поток
                // Thread thread = new Thread(new ParameterizedThreadStart(ClientThread));
                // // И запускаем этот поток, передавая ему принятого клиента
                // thread.Start(client);

                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
            }
        }
        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }
    
        // Остановка сервера
        ~Rex()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
            }
        }
    
        static void Main(string[] args)
        {
            // Определим нужное максимальное количество потоков
            // Пусть будет по 4 на каждый процессор
            int MaxThreadsCount = Environment.ProcessorCount * 4;
           // Установим максимальное количество рабочих потоков
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            // Установим минимальное количество рабочих потоков
            ThreadPool.SetMinThreads(2, 2);
            // Создадим новый сервер на порту 8000
            new Rex(8000);
        }


    }

    class Client
    {
        TcpClient tcpClient;
        NetworkStream netStream;

        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            this.tcpClient = Client;

            this.netStream = tcpClient.GetStream();

            if (netStream.CanRead) {
                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[Client.ReceiveBufferSize];

                // Read can return anything from 0 to numBytesToRead.
                // This method blocks until at least one byte is read.
                netStream.Read(bytes, 0, (int)Client.ReceiveBufferSize);

                // Returns the data received from the host to the console.
                string returndata = Encoding.UTF8.GetString(bytes);
                
                int indexOfChar = returndata.IndexOf("\n");
                if (indexOfChar == -1) return;
                
                string line = returndata.Substring(0, indexOfChar); // GET / HTTP/1.1

                // Console.WriteLine("This is what the host returned to you: " + line);
                
                // можно таким образом достать данные из строки
                // indexOfChar = line.IndexOf(" ");
                // string method = line.Substring(0, indexOfChar);
                // line = line.Substring(indexOfChar + 1, line.Length - (indexOfChar + 1));
                // indexOfChar = line.IndexOf(" ");
                // string path = line.Substring(0, indexOfChar);

                // а так проще, просто поделить строку по пробелам
                string[] words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string method = words[0];
                string path = words[1];
                string protocol = words[2];

                if (path != "/favicon.ico") {
                    Console.WriteLine("Method: " + method);
                    Console.WriteLine("Path: " + path);
                }

                Send(path);

            }else {
                Console.WriteLine("You cannot read data from this stream.");
                tcpClient.Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return;
            }
            netStream.Close();

        }

        public void Send(string path) {

            if (netStream.CanWrite) {
                string body = "<html><body><h1>Что-то пошло не так...</h1></body></html>";
                string type = "html";
                byte[] buffer;
                string response, file;
                // string boundary = "-------------573cf973d5228";
                if (path != null) {
                    // чтение из файла
                    if (path == "/") file = "html/index.html";
                    // else if (path == "/test" || path == "/test/") file = "html/test.html";

                    else if (path == "/json" || path == "/json/") {

                        file = "json/test.json";
                        type = "json";

                    }else if (path == "/logo.jpg" || path == "/logo.jpg/") {

                        file = "jpeg/index.html";

                    }else if (path == "/download/logo.jpg" || path == "/download/logo.jpg/" || path == "/static/img/logo.jpg" || path == "/static/img/logo.jpg/") {

                        file = "static/img/logo.jpg";
                        type = "jpeg";

                    }else if (path == "/favicon.ico" || path == "/favicon.ico/") {

                        file = "static/favicon.ico";
                        type = "icon";

                    }else {
                        // надо сделать что бы любой URl работал (run/test/temp.html)
                        file = "html/" + path.Replace("/", "") + ".html";
                        // if (File.Exists(file)) 
                        // file = "html/error.html";
                    }

                    if (File.Exists(file)) {
                        if (type == "jpeg" || type == "icon") {
                            // byte[] buff;
                            // получение массива байт из jpeg (так)
                            // Image image = Image.FromFile(file); // Bitmap image = new Bitmap(file); // можно и так
                            // MemoryStream memoryStream = new MemoryStream();
                            // image.Save(memoryStream, ImageFormat.Jpeg);
                            // buff = memoryStream.ToArray();
                
                            // получение массива байт из jpeg (или эдак)
                            // buff = File.ReadAllBytes(file);

                            // сохранение массива байт в jpeg
                            // MemoryStream ms = new MemoryStream(buff);
                            // Image returnImage = Image.FromStream(ms);
                            // returnImage.Save("static/img/pict.jpg", ImageFormat.Jpeg);
                           
                            // перевод массива байт в строку
                            // body = Encoding.Default.GetString(buff);

                            // получение массива байт из jpeg и перевод массива байт в строку
                            body = Encoding.Default.GetString(File.ReadAllBytes(file));

                        }else body = File.ReadAllText(file);
                    }else {
                        type = "html";
                        body = File.ReadAllText("html/error.html");
                    }

                    // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                    response = "HTTP/1.1 200\r\n";
                    
                    if (type == "html") response += "Content-Type: text/html";
                    else if (type == "json") response += "Content-Type: application/json";
                    else if (type == "jpeg") response += "Content-Type: image/jpeg";
                    else if (type == "icon") response += "Content-Type: image/x-icon";
              
                    if (type == "html" || type == "json") response += "; charset=utf-32\r\n";

                    response += "Content-Length: " + body.Length.ToString() + "\r\n\r\n";

                    response += body;

                    // Приведем строку к виду массива байт
                    buffer = Encoding.Default.GetBytes(response);
                    // Отправим его клиенту
                    netStream.Write(buffer,  0, buffer.Length);
                }else {
                    response = "HTTP/1.1 200\r\n";
                    buffer = Encoding.UTF8.GetBytes(response);
                    netStream.Write(buffer,  0, buffer.Length);
                }
            }else {
                Console.WriteLine ("Вы неможете записывать данные в этот stream.");
                tcpClient.Close ();

                // Закрываем объект класса tcpClient.
                netStream.Close ();
            }
        }

    }

}