using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

public class Program
{
    public static void Main()
    {
        var parser = new FileIniDataParser();
        IniData data = parser.ReadFile("config.ini");
        string storeName = data["Store"]["Name"];
        string storeId = data["Store"]["Id"];

        string stationName = Environment.MachineName;

        var scanner = new UsbDeviceScanner();
        var devices = scanner.GetConnectedUsbDevices();

        foreach (var device in devices)
        {
            Console.WriteLine($"{device.VendorName} - {device.ProductName} | VID: {device.VendorId}, PID: {device.ProductId}, Serial: {device.SerialNumber}");
        }

        string path = Path.Combine(Directory.GetCurrentDirectory(), "usb_devices.csv");
        scanner.SaveToCsv(devices, path, storeName, storeId, stationName);
    }
}