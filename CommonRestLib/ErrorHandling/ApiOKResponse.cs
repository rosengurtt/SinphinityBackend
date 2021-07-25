namespace CommonRestLib.ErrorHandling
{
    public class ApiOKResponse : ApiResponse
    {
        public object Result { get; }

        public ApiOKResponse(object result) : base(200)
        {
            Result = result;
        }
    }
}
