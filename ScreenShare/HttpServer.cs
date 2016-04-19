using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.WebSockets;
//using WebSocket4Net;

using System.IO;
using System.IO.Compression;

namespace ScreenShare
{
    /// <summary>
    /// HTTPサーバを立ち上げます。
    /// </summary>
    class HttpServer
    {
        private const string DefaultIPAddress = "+";
        private const int DefaultPort = 8080;
        private const string DefaultIndexPath = @"/index.html";
        private const string DefaultRootPath = "";

        /// <summary>
        /// サーバのIPアドレスを取得、設定します。
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// サーバのポート番号を取得、設定します。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// クライアントのリクエストURLに対するレスポンスのパスを取得、設定します。
        /// </summary>
        public string RootPath { get; set; }

        private HttpListener HttpListener;
        private List<WebSocket> Clients = new List<WebSocket>();

        /// <summary>
        /// HTTPサーバを初期化します。
        /// </summary>
        /// <param name="ip">IPアドレス</param>
        /// <param name="port">ポート番号</param>
        /// <param name="root"></param>
        public HttpServer(string ip = DefaultIPAddress, int port = DefaultPort, string root = DefaultRootPath)
        {
            this.IPAddress = ip;
            this.Port = port;
            this.RootPath = root;
        }

        /// <summary>
        /// サーバを立ち上げ、応答可能な状態にします。
        /// </summary>
        public void Start()
        {
            Task.Run(() =>
            {
                var prefixes = new string[] { "http://" + this.IPAddress + ":" + this.Port + "/", };

                HttpListener = new HttpListener();
                foreach (var prefix in prefixes)
                    HttpListener.Prefixes.Add(prefix);
                HttpListener.Start();

                while (true)
                {
                    HttpListenerContext listenerContext;

                    try
                    {
                        listenerContext = HttpListener.GetContext();
                    }
                    catch
                    {
                        break;
                    }

                    var req = listenerContext.Request;
                    var res = listenerContext.Response;

                    var url = this.RootPath + (req.RawUrl == @"/" ? DefaultIndexPath : req.RawUrl);

                    try
                    {
                        byte[] buf;
                        using (var sr = new StreamReader(url))
                        {
                            buf = Encoding.UTF8.GetBytes(sr.ReadToEnd());
                        }

                        using (var ms = new MemoryStream(buf))
                        {
                            res.AddHeader("Content-Encoding", "gzip");

                            using (var gs = new GZipStream(res.OutputStream, CompressionLevel.Optimal))
                            {
                                ms.CopyTo(gs);
                                gs.Flush();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                    res.Close();
                }

                Debug.Log("Exit");
            });
        }

        /// <summary>
        /// サーバを閉じます。
        /// </summary>
        public void Close()
        {
            //HttpListener.Stop();
            HttpListener.Close();

            Debug.Log("HttpServer Closed.");
        }
    }
}
