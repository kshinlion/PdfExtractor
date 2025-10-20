using System;
using System.IO;
using UglyToad.PdfPig;

class Program
{
  static void Main(string[] args)
  {
    string inputPath = "C:/General Files/Projects/Travel Expenses/08. Invoice Aug/Mr Changman Jo #3310 31 Aug - 30 Sep 2025.pdf";
    string outputPath = "output.txt";


    string extractedText = "";

    using (var document = PdfDocument.Open(inputPath))
    {
      foreach (var page in document.GetPages())
      {
        extractedText += page.Text + Environment.NewLine;
      }
    }

    File.WriteAllText(outputPath, extractedText);
    Console.WriteLine($"Text extracted to: {outputPath}");
  }
}