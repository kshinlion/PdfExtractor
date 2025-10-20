using System;
using System.IO;
using UglyToad.PdfPig;

class Program
{
  static void Main(string[] args)
  {
    Console.WriteLine("Please enter the path of the PDF file:");
    string? filePath = Console.ReadLine();

    if (string.IsNullOrEmpty(filePath))
    {
      Console.WriteLine("File path cannot be null or empty.");
      return;
    }

    try
    {
      textExtractor(filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
  }

  static void textExtractor(string inputPath)
  {
    string outputPath = "output.txt";
    string extractedText = "";

    using (var document = PdfDocument.Open(inputPath))
    {
      // Extract text from the first page only
      var firstPage = document.GetPage(1); // Get the first page
      extractedText = firstPage.Text; // Extract text from the first page
    }

    File.WriteAllText(outputPath, extractedText);
    Console.WriteLine($"Text extracted to: {outputPath}");
  }
}