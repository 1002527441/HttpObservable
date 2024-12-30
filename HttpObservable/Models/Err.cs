namespace HttpObservable.Models
{
    public class Err
    {
        public string Message { get; set; } = null!;
        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }

}
