using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;

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
        private const string DefaultTopPage = "/index.html";
        private const string DefaultDocumentRootPath = "";
        private const string DefaultSpecificCulture = "ja";

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
        public string DocumentRootPath { get; set; }

        /// <summary>
        /// トップページを示します。
        /// </summary>
        public string TopPage { get; set; }

        /// <summary>
        /// 言語を示します。
        /// </summary>
        public string SpecificCulture { get; set; }


        public event EventHandler<Exception> OnError = delegate { };

        private HttpListener HttpListener;

        /// <summary>
        /// HTTPサーバを初期化します。
        /// </summary>
        /// <param name="ip">IPアドレス</param>
        /// <param name="port">ポート番号</param>
        /// <param name="root">ドキュメントルートパス</param>
        /// <param name="top">トップページファイル</param>
        public HttpServer()
        {
            IPAddress = DefaultIPAddress;
            Port = DefaultPort;
            DocumentRootPath = DefaultDocumentRootPath;
            TopPage = DefaultTopPage;
            SpecificCulture = DefaultSpecificCulture;
        }

        /// <summary>
        /// サーバを立ち上げ、応答可能な状態にします。
        /// </summary>
        public void Start()
        {
            Task.Run(() =>
            {
                var prefixes = new string[] { "http://" + IPAddress + ":" + Port + "/", };

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

                    var path = req.Url.LocalPath;
                    var url = DocumentRootPath + (path == @"/" ? TopPage : path);

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
                        OnError(this, e);
                    }

                    res.Close();
                }
            });
        }

        /// <summary>
        /// サーバを閉じます。
        /// </summary>
        public void Stop()
        {
            HttpListener.Stop();
        }
    }
}
