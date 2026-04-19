using BECOSOFT.ThirdParty;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel.Channels;
using System.Web.Mvc;
namespace BECOSOFT.Web.Models
{
    public class Result<T>
    {
        public Result(bool isSuccess, T data, string message = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    } 
}