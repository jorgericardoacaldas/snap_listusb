using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class UsbDeviceScanner
{
    public List<UsbDeviceInfo> GetConnectedUsbDevices()
    {
         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return CheckWindows();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return CheckMac();
        }
        else{
            return new List<UsbDeviceInfo>();
        }
    }
    
    private List<UsbDeviceInfo> CheckMac()
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


    private List<UsbDeviceInfo> CheckWindows()
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
}