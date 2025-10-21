using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;

class Program
{
  static void Main(string[] args)
  {
    var files = getFilePaths();
    if (files.Count == 0) { Console.WriteLine("No files to process. Exiting."); return; }

    var guestInfo = new Dictionary<string, string>
    {
      ["Filename"] = "",
      ["GuestName"] = "",
      ["PaymentDate"] = "",
      ["Amount"] = "",
      ["Remark"] = ""
    };

    string outputPath = "extracted_guest_info.txt";

    using (var sw = new StreamWriter(outputPath, false))
    {

      string sep = " / ";
      sw.WriteLine(string.Join(sep, guestInfo.Keys));

      foreach (var filePath in files)
      {

        dictionaryReset(guestInfo);

        Console.WriteLine($"Processing file: {filePath}");
        string extractedText = textExtractor(filePath);

        guestInfo["Filename"] = extractFilename(filePath);
        guestInfo["GuestName"] = guestName(extractedText);

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


        var row = guestInfo.Keys.Select(k => Sanitize(guestInfo.TryGetValue(k, out var v) ? v : ""));
        sw.WriteLine(string.Join(sep, row));


        foreach (var entry in guestInfo) Console.WriteLine($"{entry.Key}: {entry.Value}");
        Console.WriteLine("-----");
      }
    }

    Console.WriteLine($"Wrote results to: {outputPath}");
  }

  static string extractFilename(string fullPath)
  {
    return Path.GetFileName(fullPath);
  }

  static string textExtractor(string inputPath)
  {

    string extractedText = "";

    using (var document = PdfDocument.Open(inputPath))
    {
      foreach (var page in document.GetPages())
      {
        extractedText += page.Text + "\n"; // Preserve line breaks between pages
      }
    }

    return extractedText;
  }

  static string guestName(string content)
  {
    int guestIndex = content.IndexOf("Guest");
    int departureIndex = content.IndexOf("Departure");

    if (guestIndex == -1 || departureIndex == -1 || departureIndex <= guestIndex)
    {
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
      return ("", "");
    }

    // Look backward for date (dd MMM yyyy)
    string beforeAnchor = content.Substring(0, anchorIndex);
    var dateMatch = System.Text.RegularExpressions.Regex.Matches(beforeAnchor, @"\d{2} \w{3} \d{4}");
    string paymentDate = dateMatch.Count > 0 ? dateMatch[dateMatch.Count - 1].Value : "";

    // Look forward for amount
    string afterAnchor = content.Substring(anchorIndex);
    var amountMatch = System.Text.RegularExpressions.Regex.Match(afterAnchor, @"-?\d{1,3}(,\d{3})*(\.\d{2})");
    string amount = amountMatch.Success ? amountMatch.Value : "";

    return (paymentDate, amount);
  }

  static bool requiresManualCheck(string content)
  {
    int creditCardCount = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Card Receipt").Count;
    int creditTransferCount = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Transfer Receipt").Count;
    int creditCardRefund = System.Text.RegularExpressions.Regex.Matches(content, @"Credit Card Refund").Count;

    return creditCardCount != 1 || creditTransferCount > 0 || creditCardRefund > 0;
  }

  static List<string> getFilePaths()
  {
    Console.Write("Enter folder path: ");
    string folderPath = Console.ReadLine()?.Trim() ?? "";

    if (!Directory.Exists(folderPath))
    {
      Console.WriteLine("❌ Invalid folder path.");
      return new List<string>();
    }

    // Only PDFs by default; change pattern if you want all files
    var filePaths = new List<string>(Directory.GetFiles(folderPath, "*.pdf"));

    if (filePaths.Count == 0)
    {
      Console.WriteLine("No PDF files found in folder.");
    }

    return filePaths;
  }

  static void dictionaryReset(Dictionary<string, string> dict)
  {
    // copy keys so we can modify values safely even if keys change
    var keys = dict.Keys.ToList();

    if (keys.Count == 0)
    {
      // optional: seed default keys if dictionary is empty
      string[] defaults = { "Filename", "GuestName", "PaymentDate", "Amount", "Remark" };
      foreach (var k in defaults) dict[k] = "";
      return;
    }

    foreach (var k in keys)
    {
      dict[k] = "";
    }
  }

  static string Sanitize(string s)
  {
    if (s == null) return "";
    return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ").Trim();
  }

}
