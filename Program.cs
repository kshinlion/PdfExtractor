using System;
using System.IO;
using UglyToad.PdfPig;

class Program
{
  static void Main(string[] args)
  {
    //Initializing Dictionary
    Dictionary<string, string> guestInfo = new Dictionary<string, string>();
    guestInfo["Filename"] = "";
    guestInfo["GuestName"] = "";
    guestInfo["PaymentDate"] = "";
    guestInfo["Amount"] = "";
    guestInfo["Remark"] = "";

    string filePath = getFilePath();
    string extractedText = textExtractor(filePath);

    guestInfo["Filename"] = extractFilename(filePath);
    guestInfo["GuestName"] = guestName(extractedText);


    //Imitating Short Circuit logic
    if (guestInfo["GuestName"] == "")
    {
      Console.WriteLine("Cannot locate guest name");
      guestInfo["Remark"] = "Need Manual Check";
    }
    else if (requiresManualCheck(extractedText))
    {
      guestInfo["Remark"] = "Need Manual Check";
    }
    else
    {
      (var paymentDate, var amount) = extractPaymentDetails(extractedText);
      guestInfo["PaymentDate"] = paymentDate;
      guestInfo["Amount"] = amount;
    }

    // Printing values
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
      foreach (var page in document.GetPages())
      {
        extractedText += page.Text + "\n"; // Preserve line breaks between pages
      }
    }

    File.WriteAllText(outputPath, extractedText);
    Console.WriteLine($"Text extracted to: {outputPath}");

    return extractedText;
  }

  static string guestName(string content)
  {
    int guestIndex = content.IndexOf("Guest");
    int departureIndex = content.IndexOf("Departure");

    if (guestIndex == -1 || departureIndex == -1 || departureIndex <= guestIndex)
    {
      Console.WriteLine("Could not locate guest name.");
      return "";
    }

    string guestName = content.Substring(guestIndex + "Guest".Length, departureIndex - (guestIndex + "Guest".Length)).Trim();

    return guestName;
  }

  static (string paymentDate, string amount) extractPaymentDetails(string content)
  {
    string anchor = "Credit Card Receipt";
    int anchorIndex = content.IndexOf(anchor);

    if (anchorIndex == -1)
    {
      Console.WriteLine("Could not locate payment anchor.");
      return ("Need manual check", "Need manual check");
    }

    // Look backward for date (dd MMM yyyy)
    string beforeAnchor = content.Substring(0, anchorIndex);
    var dateMatch = System.Text.RegularExpressions.Regex.Matches(beforeAnchor, @"\d{2} \w{3} \d{4}");
    string paymentDate = dateMatch.Count > 0 ? dateMatch[dateMatch.Count - 1].Value : "Unknown";

    // Look forward for amount
    string afterAnchor = content.Substring(anchorIndex);
    var amountMatch = System.Text.RegularExpressions.Regex.Match(afterAnchor, @"-?\d{1,3}(,\d{3})*(\.\d{2})");
    string amount = amountMatch.Success ? amountMatch.Value : "Unknown";

    return (paymentDate, amount);
  }


  static bool requiresManualCheck(string content)
  {
    int creditCardCount = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Card Receipt").Count;
    int creditTransferCount = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Transfer Receipt").Count;
    int creditCardRefund = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Card Refund").Count;

    return creditCardCount != 1 || creditTransferCount > 0 || creditCardRefund > 0;
  }
}