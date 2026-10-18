namespace MyProject.Core.Results
{
    public enum ResultStatus
    {
        Success,
        NotFound,
        Invalid,
        Conflict,
        Unauthorized,
        Forbidden,
        PaymentDeclined
    }

    // תוצאה של פעולה בשכבת ה-Service. ה-Controller מתרגם אותה לסטטוס HTTP
    public class Result
    {
        public ResultStatus Status { get; }
        public string? Error { get; }
        public bool IsSuccess => Status == ResultStatus.Success;

        protected Result(ResultStatus status, string? error)
        {
            Status = status;
            Error = error;
        }

        public static Result Success() => new(ResultStatus.Success, null);
        public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
        public static Result Invalid(string error) => new(ResultStatus.Invalid, error);
        public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
        public static Result Unauthorized(string error) => new(ResultStatus.Unauthorized, error);
        public static Result Forbidden(string error) => new(ResultStatus.Forbidden, error);
        public static Result PaymentDeclined(string error) => new(ResultStatus.PaymentDeclined, error);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(ResultStatus status, T? value, string? error) : base(status, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(ResultStatus.Success, value, null);
        public static new Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);
        public static new Result<T> Invalid(string error) => new(ResultStatus.Invalid, default, error);
        public static new Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error);
        public static new Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, default, error);
        public static new Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, default, error);
        public static new Result<T> PaymentDeclined(string error) => new(ResultStatus.PaymentDeclined, default, error);

        // מעביר כישלון מתוצאה מסוג אחר, עם אותו סטטוס ואותה הודעה
        public static Result<T> FromFailure(Result failure) => new(failure.Status, default, failure.Error);
    }
}
