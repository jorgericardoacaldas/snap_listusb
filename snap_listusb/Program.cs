using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Globalization;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("\U0001F6E0️ Configuração da Loja");
        var site = new Site
        {
            id = AskInt("ID da loja: "),
            name = Ask("Nome da loja: "),
            city_name = Ask("Cidade: "),
            region_name = Ask("Estado (sigla): "),
            location = new Location
            {
                lat = AskDouble("Latitude: "),
                lon = AskDouble("Longitude: ")
            },
            check_interval = AskInt("Intervalo de checagem (segundos): ")
        };

        Console.WriteLine("\U0001F527 Configuração do Elasticsearch");
        var elasticsearch = new ElasticConfig
        {
            host = Ask("URL do Elasticsearch (ex: http://localhost:9200): "),
            apikey = Ask("API Key (pressione Enter se não tiver): ")
        };

        var scanner = new UsbDeviceScanner();
        var usbDevices = scanner.GetConnectedUsbDevices();

        var monitors = new List<Monitor>();

        Console.WriteLine("\n\U0001F50C Dispositivos USB conectados:");
        foreach (var device in usbDevices)
        {
            Console.WriteLine($"- {device.ProductName} (VID: {device.VendorId}, PID: {device.ProductId})");
            Console.Write("➕ Deseja adicionar ao YAML? (s/n): ");
            if (Console.ReadLine()?.ToLower() == "s")
            {
                monitors.Add(new Monitor
                {
                    type = "usb",
                    enabled = true,
                    device_type = "USB",
                    name = device.ProductName,
                    area = "Loja",
                    idVendor = device.VendorId,
                    idProduct = device.ProductId
                });
            }
        }

        Console.Write("\n\uD83D\uDD8A️ Deseja adicionar manualmente algum dispositivo USB? (s/n): ");
        if (Console.ReadLine()?.ToLower() == "s")
        {
            while (true)
            {
                var name = Ask("Nome do dispositivo: ");
                var vendor = AskInt("idVendor (ex: 1234): ");
                var product = AskInt("idProduct (ex: 5678): ");

                monitors.Add(new Monitor
                {
                    type = "usb",
                    enabled = true,
                    device_type = "USB",
                    name = name,
                    area = "Loja",
                    idVendor = vendor,
                    idProduct = product
                });

                Console.Write("Adicionar outro? (s/n): ");
                if (Console.ReadLine()?.ToLower() != "s") break;
            }
        }


        Console.Write("\n\U0001F310 Deseja adicionar monitoramento de host (ping)? (s/n): ");
        if (Console.ReadLine()?.ToLower() == "s")
        {
            while (true)
            {
                var name = Ask("Nome do dispositivo (ex: Gateway): ");
                var hostUrl = Ask("Host (ex: www.google.com.br): ");
                var area = Ask("Área (ex: Loja): ");
                var tipo = Ask("Tipo de dispositivo (ex: gateway): ");

                monitors.Add(new Monitor
                {
                    type = "host",
                    enabled = true,
                    device_type = tipo,
                    name = name,
                    area = area,
                    host = hostUrl
                });

                Console.Write("Adicionar outro host? (s/n): ");
                if (Console.ReadLine()?.ToLower() != "s") break;
            }
        }

        Console.Write("\n\U0001F9E0 Deseja adicionar monitoramento de aplicativos? (s/n): ");
        if (Console.ReadLine()?.ToLower() == "s")
        {
            while (true)
            {
                var name = Ask("Nome do aplicativo (ex: Chrome): ");
                var appName = Ask("Nome do processo ou appName: ");
                var area = Ask("Área (ex: Loja): ");

                monitors.Add(new Monitor
                {
                    type = "app",
                    enabled = true,
                    device_type = "Aplicativo",
                    name = name,
                    area = area,
                    appName = appName
                });

                Console.Write("Adicionar outro aplicativo? (s/n): ");
                if (Console.ReadLine()?.ToLower() != "s") break;
            }
        }


        var config = new YamlConfig
        {
            elasticsearch = elasticsearch,
            site = site,
            monitors = monitors
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yaml = serializer.Serialize(config);
        File.WriteAllText("smi_agent.yml", yaml, new UTF8Encoding(true));

        Console.WriteLine("\n✅ Arquivo YAML gerado com sucesso: smi_agent.yml");
    }

    static string Ask(string label)
    {
        Console.Write(label);
        return Console.ReadLine() ?? "";
    }

    static int AskInt(string label)
    {
        while (true)
        {
            Console.Write(label);
            if (int.TryParse(Console.ReadLine(), out var value)) return value;
            Console.WriteLine("❌ Valor inválido. Tente novamente.");
        }
    }

    static double AskDouble(string label)
    {
        while (true)
        {
            Console.Write(label);
            if (double.TryParse(Console.ReadLine(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) return value;
            Console.WriteLine("❌ Valor inválido. Tente novamente.");
        }
    }
}
