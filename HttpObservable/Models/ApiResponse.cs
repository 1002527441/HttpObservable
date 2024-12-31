using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HttpObservable.Models
{
    public class ApiResponse<TData>
    {
        public int Code { get; set; }
        public bool Succeeded { get; set; }
        public TData? Data { get; set; }
        public ApiError? Error { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
    }

}
