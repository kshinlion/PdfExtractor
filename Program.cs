using System;
using System.IO;
using UglyToad.PdfPig;

class Program
{
  static void Main(string[] args)
  {
    // Create a dictionary to store guest info
    Dictionary<string, string> guestInfo = new Dictionary<string, string>();

    // Add entries
    guestInfo["Filename"] = "";
    guestInfo["GuestName"] = "";
    guestInfo["PaymentDate"] = "";
    guestInfo["Amount"] = "";

    string filePath = getFilePath();
    guestInfo["Filename"] = extractFilename(filePath);

    string extractedText = textExtractor(filePath);
    guestName(extractedText);


    // Print values
    foreach (var entry in guestInfo)
    {
      Console.WriteLine($"{entry.Key}: {entry.Value}");
    }
  }

  static string getFilePath()
  {
    Console.WriteLine("Please enter the path of the PDF file:");
    string? filePath = Console.ReadLine();

    if (string.IsNullOrEmpty(filePath))
    {
      Console.WriteLine("File path cannot be null or empty.");
      return "error";
    }
    return filePath;
  }

  static string extractFilename(string fullPath)
  {
    return Path.GetFileName(fullPath);
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