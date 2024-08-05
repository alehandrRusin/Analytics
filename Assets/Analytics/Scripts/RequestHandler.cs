using System;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics
{
    public static class RequestHandler
    {
        private const string RequestHeaderName = "Content-Type";
        private const string RequestHeaderValue = "application/json; charset=UTF-8";
        
        public static IDisposable SendPostRequest(string url, string json, Action<UnityWebRequest.Result> callback)
        {
            WWWForm form = new WWWForm();

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            byte[] postBytes = Encoding.UTF8.GetBytes(json);

            UploadHandler uploadHandler = new UploadHandlerRaw(postBytes);
            request.uploadHandler = uploadHandler;
            
            request.SetRequestHeader(RequestHeaderName, RequestHeaderValue);

            return request.SendWebRequest().AsAsyncOperationObservable().Subscribe(result =>
            {
                callback(request.result);
            });
        }
    }
}