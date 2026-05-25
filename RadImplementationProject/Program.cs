using CommandLine;
using RadImplementationProject.Tasks;
using RadImplementationProject.Hashing;

namespace RadImplementationProject
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<HashFunctionsTest, QuadraticSums>(args)
                .WithParsed<HashFunctionsTest>(HashFunctionsTest.Run)
                .WithParsed<QuadraticSums>(QuadraticSums.Run);
        }
    }
}
