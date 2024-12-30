namespace HttpObservable.Samples.Services
{
    public class SigninRequest
    {
        public SigninRequest(string u, string p)
        {
            this.username = u;
            this.password = p;
        }
        public string username { get; set; }
        public string password { get; set; }
    }
}
