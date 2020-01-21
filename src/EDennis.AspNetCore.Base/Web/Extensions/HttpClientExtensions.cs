﻿using Castle.Core.Configuration;
using DevExpress.Utils.Serializing.Helpers;
using EDennis.AspNetCore.Base.EntityFramework;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.Web {

    public static partial class HttpClientExtensions {

        public static ObjectResult Get<TResponseObject>(this HttpClient client,
            string relativeUrlFromBase,
            JsonSerializerOptions jsonSerializerOptions = null
            ) {
            return client.GetAsync<TResponseObject>(relativeUrlFromBase, jsonSerializerOptions).Result;
        }


        public static async Task<ObjectResult> GetAsync<TResponseObject>(
                this HttpClient client, string relativeUrlFromBase,
                    JsonSerializerOptions jsonSerializerOptions = null
                ) {


            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            var msg = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            AddOrUpdatePagingData(client, msg);

            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseObject>(response, jsonSerializerOptions);

            return objResult;

        }

        public static ObjectResult Get<TRequestObject, TResponseObject>(this HttpClient client, string relativeUrlFromBase, TRequestObject obj) {
            return client.GetAsync<TRequestObject, TResponseObject>(relativeUrlFromBase, obj).Result;
        }



        public static async Task<ObjectResult> GetAsync<TRequestObject, TResponseObject>(
                this HttpClient client, string relativeUrlFromBase, TRequestObject obj) {

            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the GET
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new BodyContent<TRequestObject>(obj)
            };

            AddOrUpdatePagingData(client, msg);

            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<TResponseObject>(response);

            return objResult;

        }




        public static StatusCodeResult GetStatusCodeResult(this HttpClient client, string relativeUrlFromBase) {
            return client.GetStatusCodeResultAsync(relativeUrlFromBase).Result;
        }



        public static async Task<StatusCodeResult> GetStatusCodeResultAsync(
                this HttpClient client, string relativeUrlFromBase) {

            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the GET
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            var response = await client.SendAsync(msg);
            var statusCode = response.StatusCode;

            return new StatusCodeResult((int)statusCode);

        }


        public static StatusCodeResult Configure(this HttpClient client, string relativeUrlFromBase,
                string jsonConfig) {
            return client.ConfigureAsync(relativeUrlFromBase, jsonConfig).Result;
        }

        public static async Task<StatusCodeResult> ConfigureAsync(this HttpClient client, string relativeUrlFromBase, string jsonConfig) {
            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);
            //build the request message object for the POST
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonConfig)
            };
            msg.Headers.Add(Constants.CONFIG_QUERY_KEY, "-");

            var response = await client.SendAsync(msg);

            return new StatusCodeResult((int)response.StatusCode);

        }

        public static ObjectResult Post<T>(this HttpClient client, string relativeUrlFromBase, T obj) {
            return client.PostAsync(relativeUrlFromBase, obj).Result;
        }

        public static async Task<ObjectResult> PostAsync<T>(
                this HttpClient client, string relativeUrlFromBase, T obj) {


            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the POST
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new BodyContent<T>(obj)
            };

            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<T>(response);

            return objResult;
        }


        public static ObjectResult Put<T>(this HttpClient client, string relativeUrlFromBase, T obj) {
            return client.PutAsync(relativeUrlFromBase, obj).Result;
        }


        public static async Task<ObjectResult> PutAsync<T>(
                this HttpClient client, string relativeUrlFromBase, T obj) {


            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the PUT
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Put,
                RequestUri = new Uri(url),
                Content = new BodyContent<T>(obj)
            };

            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<T>(response);
            return objResult;


        }



        public static ObjectResult Patch<T>(this HttpClient client, string relativeUrlFromBase, T obj) {
            return client.PatchAsync(relativeUrlFromBase, obj).Result;
        }


        public static async Task<ObjectResult> PatchAsync<T>(
                this HttpClient client, string relativeUrlFromBase, T obj) {


            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the PUT
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(url),
                Content = new BodyContent<T>(obj)
            };

            var response = await client.SendAsync(msg);
            var objResult = await GenerateObjectResult<T>(response);
            return objResult;


        }



        public static StatusCodeResult Delete<T>(this HttpClient client, string relativeUrlFromBase, T obj,
            bool flagAsUpdateFirst = false) {
            return client.DeleteAsync(relativeUrlFromBase, obj, flagAsUpdateFirst).Result;
        }


        public static async Task<StatusCodeResult> DeleteAsync<T>(
                this HttpClient client, string relativeUrlFromBase, T obj,
                bool flagAsUpdateFirst = false) {

            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the PUT
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url),
                Content = new BodyContent<T>(obj)
            };
            if (flagAsUpdateFirst)
                msg.Headers.Add("X-PreOperation", "Update");

            var response = await client.SendAsync(msg);

            return new StatusCodeResult((int)response.StatusCode);

        }


        public static StatusCodeResult Delete<T>(this HttpClient client, string relativeUrlFromBase,
                bool flagAsUpdateFirst = false) {
            return client.DeleteAsync<T>(relativeUrlFromBase, flagAsUpdateFirst).Result;
        }


        public static async Task<StatusCodeResult> DeleteAsync<T>(
                this HttpClient client, string relativeUrlFromBase,
                bool flagAsUpdateFirst = false) {

            var url = Url.Combine(client.BaseAddress.ToString(), relativeUrlFromBase);

            //build the request message object for the PUT
            var msg = new HttpRequestMessage {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            if (flagAsUpdateFirst)
                msg.Headers.Add("X-PreOperation", "Update");

            var response = await client.SendAsync(msg);

            return new StatusCodeResult((int)response.StatusCode);

        }


        public static ObjectResult Forward<T>(this HttpClient client, HttpRequest request, string relativeUrlFromBase) {
            var msg = request.ToHttpRequestMessage(client);
            var url = relativeUrlFromBase + (msg.Properties["QueryString"] ?? "");
            return ForwardRequest<T>(client, msg, url);
        }


        public static ObjectResult Forward<T>(this HttpClient client, HttpRequest request, T body, string relativeUrlFromBase) {
            var msg = request.ToHttpRequestMessage(client, body);
            var url = relativeUrlFromBase + (msg.Properties["QueryString"] ?? "");
            return ForwardRequest<T>(client, msg, url);
        }



        public static void AddOrUpdatePagingData(this HttpClient client,
                HttpRequestMessage msg, PagingData pagingData = null) {

            if (pagingData == null || pagingData.RecordCount < 0)
                return;

            if (client.DefaultRequestHeaders.Contains("X-RecordCount"))
                client.DefaultRequestHeaders.Remove("X-RecordCount");
            if (msg.Headers.Contains("X-RecordCount"))
                msg.Headers.Remove("X-RecordCount");
            msg.Headers.Add("X-RecordCount", pagingData.RecordCount.ToString());

        }


        public static void AddOrUpdateHeaders(this HttpClient client,
                HttpRequestMessage msg, Dictionary<string, StringValues> headers) {

            foreach (var key in headers.Keys) {
                if (client.DefaultRequestHeaders.Contains(key))
                    client.DefaultRequestHeaders.Remove(key);
                if (msg.Headers.Contains(key))
                    msg.Headers.Remove(key);
                msg.Headers.Add(key, headers[key].ToArray());
            }
        }






        //public static void SendReset(this HttpClient client, string instance) {
        //    client.SendResetAsync(instance).Wait();
        //}

        //public static async Task SendResetAsync(this HttpClient client, string instance) {
        //try {
        //var msg = new HttpRequestMessage {
        //    Method = HttpMethod.Delete,
        //    RequestUri = new Uri($"{client.BaseAddress.ToString()}?{Constants.TESTING_RESET_KEY}={instance}")
        //};
        //await client.SendAsync(msg);
        //return;
        //} catch {
        //    //ignore; client is already disposed.
        //}
        //try {
        //    using var tcp = new TcpClient(client.BaseAddress.Host, client.BaseAddress.Port) {
        //        SendTimeout = 500,
        //        ReceiveTimeout = 1000
        //    };
        //    var builder = new StringBuilder()
        //        .AppendLine($"{Constants.RESET_METHOD} /?{Constants.TESTING_RESET_KEY}={instance} HTTP/1.1")
        //        .AppendLine($"Host: {client.BaseAddress.Host}")
        //        .AppendLine("Connection: close")
        //        .AppendLine();
        //    var header = Encoding.ASCII.GetBytes(builder.ToString());
        //    using var stream = tcp.GetStream();
        //    await stream.WriteAsync(header, 0, header.Length);
        //}
        //catch (Exception)
        //{
        //    //ignore this exception, object is disposed.
        //}
        //}

        public static bool Ping(this HttpClient client, int timeoutSeconds = 5) {
            return client.PingAsync(timeoutSeconds).Result;
        }


        public static async Task<bool> PingAsync(this HttpClient client, int timeoutSeconds = 5) {

            var pingable = false;

            await Task.Run(() => {

                var port = client.BaseAddress.Port;
                var host = client.BaseAddress.Host;
                var sw = new Stopwatch();

                sw.Start();
                while (sw.ElapsedMilliseconds < (timeoutSeconds * 1000)) {
                    try {
                        using var tcp = new TcpClient(host, port);
                        var connected = tcp.Connected;
                        pingable = true;
                        break;
                    } catch (Exception ex) {
                        if (!ex.Message.Contains("No connection could be made because the target machine actively refused it"))
                            throw ex;
                        else
                            Thread.Sleep(1000);
                    }

                }

            });
            return pingable;
        }





        private static ObjectResult ForwardRequest<T>(this HttpClient client, HttpRequestMessage msg, string relativeUrlFromBase) {
            var url = new Url(client.BaseAddress)
                  .AppendPathSegment(relativeUrlFromBase);

            url = WebUtility.UrlDecode(url);

            var uri = url.ToUri();
            msg.RequestUri = uri;
            var response = client.SendAsync(msg).Result;
            var objResult = GenerateObjectResult<T>(response).Result;

            return objResult;

        }



        private static HttpRequestMessage ToHttpRequestMessage(this HttpRequest httpRequest, HttpClient client) {
            var msg = new HttpRequestMessage();
            msg
                .CopyMethod(httpRequest)
                .CopyHeaders(httpRequest, client)
                .CopyQueryString(httpRequest)
                .CopyCookies(httpRequest);

            return msg;
        }


        private static HttpRequestMessage ToHttpRequestMessage<T>(this HttpRequest httpRequest, HttpClient client, T body) {
            var msg = new HttpRequestMessage();
            msg
                .CopyMethod(httpRequest)
                .CopyHeaders(httpRequest, client)
                .CopyQueryString(httpRequest)
                .CopyCookies(httpRequest);

            var json = JToken.FromObject(body).ToString();
            msg.Content = new StringContent(json,
                    Encoding.UTF8, "application/json");

            return msg;
        }


        private static HttpRequestMessage CopyMethod(this HttpRequestMessage msg, HttpRequest req) {
            if (req.Method.ToUpper() == "POST")
                msg.Method = HttpMethod.Post;
            else if (req.Method.ToUpper() == "PUT")
                msg.Method = HttpMethod.Put;
            else if (req.Method.ToUpper() == "DELETE")
                msg.Method = HttpMethod.Delete;
            else if (req.Method.ToUpper() == "GET")
                msg.Method = HttpMethod.Get;
            else if (req.Method.ToUpper() == "HEAD")
                msg.Method = HttpMethod.Head;

            return msg;
        }


        private static HttpRequestMessage CopyHeaders(this HttpRequestMessage msg, HttpRequest req, HttpClient client) {
            var currentHeaders = client.DefaultRequestHeaders.Select(x => x.Key);
            var requestHeaders = req.Headers.Where(h => !h.Key.StartsWith("Content-"));
            var headers = requestHeaders.Where(h => !currentHeaders.Contains(h.Key));
            foreach (var header in headers)
                msg.Headers.Add(header.Key, header.Value.AsEnumerable());
            msg.Headers.Host = client.BaseAddress.Host;
            return msg;
        }


        private static HttpRequestMessage CopyQueryString(this HttpRequestMessage msg, HttpRequest req) {
            msg.Properties.Add("QueryString", req.QueryString);
            return msg;
        }


        //assumes that HttpClientHandler is set as such:
        //services.AddHttpClient<ColorApiClient>(options => {
        //    options.BaseAddress = new Uri("http://localhost:61006");
        //}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
        //    UseCookies = false
        //});        
        private static HttpRequestMessage CopyCookies(this HttpRequestMessage msg, HttpRequest req) {
            if (req.Cookies != null && req.Cookies.Count > 0) {
                var sb = new StringBuilder();
                foreach (var cookie in req.Cookies) {
                    sb.Append(cookie.Key);
                    sb.Append("=");
                    sb.Append(cookie.Value);
                    sb.Append("; ");
                }
                msg.Headers.Add("Cookie", sb.ToString().TrimEnd());
            }
            return msg;
        }


        private async static Task<ObjectResult> GenerateObjectResult<T>(HttpResponseMessage response,
            JsonSerializerOptions jsonSerializerOptions = null) {

            object value = null;

            int statusCode = (int)response.StatusCode;

            if (response.Content.Headers.ContentLength > 0) {
                var json = await response.Content.ReadAsStringAsync();


                if (statusCode < 299 && typeof(T) != typeof(string)) {
                    if (jsonSerializerOptions != null)
                        value = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
                    else {
                        var options = new JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        };
                        value = JsonSerializer.Deserialize<T>(json, options);
                    }
                } else {
                    value = json;
                }
            }

            return new ObjectResult(value) {
                StatusCode = statusCode
            };

        }




    }
}

