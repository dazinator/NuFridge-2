namespace NuFridge.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            var code = Startup.Run(args);
            return code;
        }
    }
}
