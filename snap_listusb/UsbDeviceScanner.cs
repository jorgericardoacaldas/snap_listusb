using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

public class UsbDeviceScanner
{
    public List<UsbDeviceInfo> GetConnectedUsbDevices()
{
    var devices = new List<UsbDeviceInfo>();

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "ioreg",
            Arguments = "-p IOUSB -l",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    var blocks = Regex.Split(output, @"\+-o ");

    foreach (var block in blocks)
    {
        if (!block.Contains("idVendor") || !block.Contains("idProduct"))
            continue;

        var device = new UsbDeviceInfo();

        var idVendorMatch = Regex.Match(block, @"""idVendor""\s*=\s*(\d+)");
        if (idVendorMatch.Success)
            device.VendorId = int.Parse(idVendorMatch.Groups[1].Value);

        var idProductMatch = Regex.Match(block, @"""idProduct""\s*=\s*(\d+)");
        if (idProductMatch.Success)
            device.ProductId = int.Parse(idProductMatch.Groups[1].Value);

        var productMatch = Regex.Match(block, @"""(?:USB Product Name|kUSBProductString)""\s*=\s*""([^""]+)""");
        device.ProductName = productMatch.Success ? productMatch.Groups[1].Value : "Unknown";

        var vendorMatch = Regex.Match(block, @"""(?:USB Vendor Name|kUSBVendorString)""\s*=\s*""([^""]+)""");
        device.VendorName = vendorMatch.Success ? vendorMatch.Groups[1].Value : "Unknown";

        var serialMatch = Regex.Match(block, @"""(?:USB Serial Number|kUSBSerialNumberString)""\s*=\s*""([^""]+)""");
        device.SerialNumber = serialMatch.Success ? serialMatch.Groups[1].Value : "N/A";

        devices.Add(device);
    }

    return devices;
}


    public void SaveToCsv(List<UsbDeviceInfo> devices, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("VendorName,ProductName,VendorId,ProductId,SerialNumber");

            foreach (var device in devices)
            {
                writer.WriteLine($"\"{device.VendorName}\",\"{device.ProductName}\",{device.VendorId},{device.ProductId},\"{device.SerialNumber}\"");
            }
        }

        Console.WriteLine($"✅ CSV gerado com {devices.Count} dispositivos em: {filePath}");
    }
}
