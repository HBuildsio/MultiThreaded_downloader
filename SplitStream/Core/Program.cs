using System;

namespace SplitStream
{
    class Program
    {
        static void Main(string[] args)
        {

            String nPath = "https://95-216-66-138.top/Getintopc.com/Adobe_After_Effects_v23.2.1.3.rar?md5=DBh4uQQ2EK8IHIghCzFgTg&expires=1746117187";
            String graphic = "https://mirror-hk.koddos.net/tdf/libreoffice/stable/25.2.2/win/x86_64/LibreOffice_25.2.2_Win_x86-64.msi";
            String path = "https://images.pexels.com/photos/1591447/pexels-photo-1591447.jpeg?cs=srgb&dl=pexels-guillaume-meurice-1591447.jpg&fm=jpg";

            Console.WriteLine("Writing Data"+"("+ ">_<"+")");
            
            String[] disposablePaths = { @"C:\Download\temp1", @"C:\Download\temp2" , @"C:\Download\temp3" };
            String Destination = @"H:\Downloads";
            int threadCount = Environment.ProcessorCount;
            var nd = new NewDownloader(path, Destination, threadCount);
            
            Console.WriteLine("Thread Count: " + threadCount);
            var elapsed = nd.DownloadFileAsync().Result;
            Console.WriteLine($"Download complete! ({">_<"}) - Time taken: {elapsed.TotalSeconds:F2} seconds");
            
            

        }

        

        
    }
}
