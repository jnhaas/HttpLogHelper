using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpLogHelper
{
    public class PostLoggerModule : IHttpModule
    {
        HashSet<string> _UrlsToLog;
        HashSet<string> _FieldsToHide;
        HashSet<string> _FieldsToInclude;
        int _MaxQueryDataSize = 512;

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
        }

        bool _ConfigLoaded = false;
        private void LoadConfig()
        {
            if (_ConfigLoaded)
                return;
            _ConfigLoaded = true;
            NameValueCollection section = ConfigurationManager.GetSection("HttpLogHelper") as NameValueCollection ?? ConfigurationManager.AppSettings;

            string fieldsToHide = section["FieldsToHide"];
            if (fieldsToHide != null)
                _FieldsToHide = new HashSet<string>(fieldsToHide.Split(','), StringComparer.OrdinalIgnoreCase);
            else
            {
                _FieldsToHide = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _FieldsToHide.Add("Password");
                _FieldsToHide.Add("__RequestVerificationToken");
            }

            string fieldsToInclude = section["FieldsToInclude"];
            if (fieldsToInclude != null)
                _FieldsToInclude = new HashSet<string>(fieldsToInclude.Split(','), StringComparer.OrdinalIgnoreCase);
            else
                _FieldsToInclude = null;

            string urlList = section["UrlsToLog"];
            if (urlList != null)
                _UrlsToLog = new HashSet<string>(urlList.Split(','), StringComparer.OrdinalIgnoreCase);
            else
                _UrlsToLog = null;

            string maxPostDataSize = section["MaxQueryDataSize"];
            if (maxPostDataSize != null)
                int.TryParse(maxPostDataSize, out _MaxQueryDataSize);
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)sender;
                HttpRequest request = application.Request;
                HttpResponse response = application.Response;
                if (response == null || request == null)
                    return;
                if (!request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    return;
                LoadConfig();
                if (_UrlsToLog != null && !_UrlsToLog.Contains(request.Url.AbsolutePath))
                    return;
                LogPostData(request, response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while logging a post request: {0}", ex);
            }
        }

        private void LogPostData(HttpRequest request, HttpResponse response)
        {
            string postData = HttpUtility.UrlDecode(request.Form.ToString());
            if (!string.IsNullOrWhiteSpace(postData))
            {
                NameValueCollection postValues = HttpUtility.ParseQueryString(postData);
                FilterPostValues(ref postValues);
                postData = postValues.ToString();
                int queryLength = request.Url.Query.Length;
                if (queryLength > 0)
                    response.AppendToLog("&");
                if (postData.Length + queryLength > _MaxQueryDataSize)
                    postData = postData.Substring(0, _MaxQueryDataSize - queryLength);
                response.AppendToLog(postData);
            }
        }

        private void FilterPostValues(ref NameValueCollection queryValues)
        {
            if (_FieldsToInclude != null)
            {
                Dictionary<string, string> newQueryValues = new Dictionary<string, string>();
                foreach (string field in _FieldsToInclude)
                {
                    string value = queryValues[field];
                    if (value != null)
                        newQueryValues[field] = value;
                }
                queryValues.Clear();
                foreach (KeyValuePair<string, string> fieldValue in newQueryValues)
                    queryValues[fieldValue.Key] = fieldValue.Value;
            }
            if (_FieldsToHide != null)
                foreach (string field in _FieldsToHide)
                    ReplaceQueryValue(queryValues, field);
        }
        private static  void ReplaceQueryValue(NameValueCollection queryValues, string field)
        {
            if (queryValues[field] != null)
                queryValues[field] = "%Value%";
        }
    }
}
