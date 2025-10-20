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

    string extractedText = textExtractor(filePath);
    guestName(extractedText);

  }

  static string textExtractor(string inputPath)
  {
    string outputPath = "output.txt";
    string extractedText = "";

    using (var document = PdfDocument.Open(inputPath))
    {
      var firstPage = document.GetPage(1);
      extractedText = firstPage.Text;
    }

    File.WriteAllText(outputPath, extractedText);
    Console.WriteLine($"Text extracted to: {outputPath}");

    return extractedText;
  }

  static void guestName(string content)
  {
    int guestIndex = content.IndexOf("Guest");
    int departureIndex = content.IndexOf("Departure");

    if (guestIndex == -1 || departureIndex == -1 || departureIndex <= guestIndex)
    {
      Console.WriteLine("Could not locate guest name.");
      return;
    }

    string guestName = content.Substring(guestIndex + "Guest".Length, departureIndex - (guestIndex + "Guest".Length)).Trim();
    Console.WriteLine($"Guest Name: {guestName}");
  }
}