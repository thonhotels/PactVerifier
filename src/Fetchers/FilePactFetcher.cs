using System.IO;
using System.Threading.Tasks;

namespace Thon.Hotels.PactVerifier
{
    public class FilePactFetcher : PactFetcher
    {
        private string PactUri { get; }

        public FilePactFetcher(string pactUri)
        {
            PactUri = pactUri;
        }

        protected override Task<Result<string>> ReadPact(string _1, string _2) => Task.FromResult((Result<string>)new Ok<string>(File.ReadAllText(PactUri)));
    }
}
