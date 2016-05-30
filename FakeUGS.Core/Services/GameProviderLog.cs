﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

using AFT.RegoV2.Shared.Logging;

using Microsoft.Practices.Unity;

namespace FakeUGS.Core.Services
{
    public interface IGameProviderLog
    {
        void LogError(string message, Exception exception = null);
        void LogWarn(string message);
        void LogInfo(string message);
        string HeadersAsString(HttpRequestMessage request);
        string RequestAsString(HttpRequestMessage request);
    }
    public sealed class GameProviderLog : IGameProviderLog
    {
        private readonly IUnityContainer _container;
        private readonly ILog _log;
        private readonly IGameProviderLog _this;

        public GameProviderLog(IUnityContainer container)
        {
            _this = this;
            _container = container;

            _log = _container.Resolve<ILog>();
        }

        void IGameProviderLog.LogError(string message, Exception exception)
        {
            _log.Error(message, exception);
        }
        void IGameProviderLog.LogWarn(string message)
        {
            _log.Warn(message);
        }
        void IGameProviderLog.LogInfo(string message)
        {
            _log.Info(message);
        }
        string IGameProviderLog.HeadersAsString(HttpRequestMessage request)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine(request.Method+ " " + request.RequestUri);
                var context = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (context != null)
                {
                    var headers = context.Request.Headers;
                    headers.Keys.Cast<string>().Aggregate(sb, (b, k) => b.AppendLine(k + ": " + String.Join(",", headers[k])));
                }
                else
                {
                    request.Headers.Aggregate(sb, (b, h) => b.AppendLine(h.Key + ": " + String.Join(",", h.Value)));
                }
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        string IGameProviderLog.RequestAsString(HttpRequestMessage request)
        {
            try
            {
                var sb = new StringBuilder(_this.HeadersAsString(request));
                sb.AppendLine().AppendLine();
                var context = request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (context != null)
                {
                    if (context.Request.InputStream.Length > 0)
                    {
                        context.Request.InputStream.Position = 0;
                        using (StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            sb.AppendLine(reader.ReadToEnd());
                        }
                    }
                    else // if the request has already been processed
                    {
                        sb.AppendLine(HttpUtility.UrlDecode(context.Request.Form.ToString()));
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}