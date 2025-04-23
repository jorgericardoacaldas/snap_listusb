using System.Collections.Generic;

public class YamlConfig
{
    public ElasticConfig elasticsearch { get; set; }
    public Site site { get; set; }
    public List<Monitor> monitors { get; set; }
}

public class ElasticConfig
{
    public string host { get; set; }
    public string apikey { get; set; }
}

public class Site
{
    public int id { get; set; }
    public string name { get; set; }
    public string city_name { get; set; }
    public string region_name { get; set; }
    public Location location { get; set; }
    public int check_interval { get; set; }
}

public class Location
{
    public double lat { get; set; }
    public double lon { get; set; }
}

public class Monitor
{
    public string type { get; set; }
    public bool enabled { get; set; }
    public string device_type { get; set; }
    public string name { get; set; }
    public string area { get; set; }

    public string host { get; set; }

    public string appName { get; set; }
    public int idVendor { get; set; }
    public int idProduct { get; set; }
}