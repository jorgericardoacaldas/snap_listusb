using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Management;


public class UsbDeviceScanner
{
    
    public List<UsbDeviceInfo> GetConnectedUsbDevices()
    {
        var devices = new List<UsbDeviceInfo>();

var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'");

        foreach (ManagementObject device in searcher.Get())
        {
            devices.Add(new UsbDeviceInfo
            {
                VendorId = ExtractVendorId(device["DeviceID"]?.ToString()),
                ProductId = ExtractProductId(device["DeviceID"]?.ToString()),
                VendorName = "Unknown",
                ProductName = device["Name"]?.ToString() ?? "Unknown",
                SerialNumber = device["PNPDeviceID"]?.ToString() ?? "N/A"
            });
        }

        return devices;
    }

    private int ExtractVendorId(string deviceId)
    {
        var match = System.Text.RegularExpressions.Regex.Match(deviceId ?? "", @"VID_([0-9A-F]{4})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? Convert.ToInt32(match.Groups[1].Value, 16) : 0;
    }

    private int ExtractProductId(string deviceId)
    {
        var match = System.Text.RegularExpressions.Regex.Match(deviceId ?? "", @"PID_([0-9A-F]{4})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? Convert.ToInt32(match.Groups[1].Value, 16) : 0;
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
