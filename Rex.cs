using System;
using System.IO;
using System.Net;
using System.Text;
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
                // Принимаем новых клиентов и передаем их на обработку новому экземпляру класса Client
                new Client(Listener.AcceptTcpClient());
            }
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
                netStream.Read (bytes, 0, (int)Client.ReceiveBufferSize);

                // Returns the data received from the host to the console.
                string returndata = Encoding.UTF8.GetString (bytes);
                
                int indexOfChar = returndata.IndexOf("\n");
                if (indexOfChar == -1) return;
                
                string line = returndata.Substring(0, indexOfChar); // GET / HTTP/1.1

                // Console.WriteLine ("This is what the host returned to you: " + line);
                
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
                    Console.WriteLine ("Method: " + method);
                    Console.WriteLine ("Path: " + path);

                    send(path);

                }else send(null);

            }else {
                Console.WriteLine ("You cannot read data from this stream.");
                tcpClient.Close ();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close ();
                return;
            }
            netStream.Close();

        }

        public void send(string path) {

            if (netStream.CanWrite) {
                string Html;
                byte[] Buffer;
                string Str;
                string file;
                if (path != null) {
                    Html = "<html><body><h1>Что-то пошло не так...</h1></body></html>";

                    // чтение из файла
                    if (path == "/") file = "pages/index.html";
                    else if (path == "/test" || path == "/test/") file = "pages/test.html";
                    else file = "pages/error.html";

                    if (File.Exists(file)) {
                        Html = File.ReadAllText(file);
                    }

                    // using (FileStream fstream = File.OpenRead($"{path}\note.txt"))
                    // {
                    //     // преобразуем строку в байты
                    //     byte[] array = new byte[fstream.Length];
                    //     // считываем данные
                    //     fstream.Read(array, 0, array.Length);
                    //     // декодируем байты в строку
                    //     string textFromFile = System.Text.Encoding.Default.GetString(array);
                    //     Console.WriteLine($"Текст из файла: {textFromFile}");
                    // }

                    // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                    Str = "HTTP/1.1 200\r\n";
                    // Str += "Accept: */*\r\n";
                    // Str += "Content-Type: application/json; charset=utf-8\r\n";
                    Str += "Content-Type: text/html; charset=utf-8\r\n";
                    Str += "Content-Length: " + Html.Length.ToString() + "\r\n";
                    Str += "\r\n" + Html;
                    // Приведем строку к виду массива байт
                    Buffer = Encoding.UTF8.GetBytes(Str);
                    // Отправим его клиенту
                    netStream.Write(Buffer,  0, Buffer.Length);
                }else {
                    Str = "HTTP/1.1 200\r\n";
                    Buffer = Encoding.UTF8.GetBytes(Str);
                    netStream.Write(Buffer,  0, Buffer.Length);
                }
            }else {
                Console.WriteLine ("You cannot write data to this stream.");
                tcpClient.Close ();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close ();
            }
        }

    }

}