using ChatApp.Models;
using System.Net;

namespace ChatApp.Handlers
{
    public class BaseResult
    {
        public bool IsError { get; set; } = false;
        public string Message { get; set; } = Messages.Success;
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;
        public object? Data { get; set; } = null;
        public int Count { get; set; } = -1;
    }

    public class Messages
    {

        public const string Saved = "Saved Successfully";
        public const string Deleted = "Deleted Successfully";
        public const string Updated = "Update Successfully";
        public const string InvalidUser = "Invalid user name or password";
        public const string AccountLocked = "Account has been locked, Contact Administrator or ForgetPassword.";
        public const string Cancelled = "Cancel Successfully";
        public const string Completed = "Completed Successfully";
        public const string RecordNotFound = "Record not found";
        public const string ExistVendor = "Vendor Already exist";
        public const string Process = "Compiled Successfully";
        public const string ExistLink = "Vendor already exist";
        public const string Success = "Success";
    }

}
