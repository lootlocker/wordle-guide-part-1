using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public static class TimeManager
{
    private static DateTime _utcStartTime = DateTime.UtcNow;
    public static DateTime UtcNow => _utcStartTime.AddSeconds(Time.realtimeSinceStartup);

    public static void InitializeTime()
    {
        GetUtcTimeAsync().WrapErrors();
    }

    private static async Task GetUtcTimeAsync()
    {
        try
        {
            var client = new TcpClient();
            await client.ConnectAsync("time.nist.gov", 13);
            using var streamReader = new StreamReader(client.GetStream());
            var response = await streamReader.ReadToEndAsync();
            var utcDateTimeString = response.Substring(7, 17);
            _utcStartTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
        catch
        {
            // Handle errors here
        }
    }

    private static async void WrapErrors(this Task task)
    {
        await task;
    }
}