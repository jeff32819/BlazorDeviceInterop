using System;
using Darnton.Blazor.DeviceInterop.Geolocation;

namespace BlazorDeviceTestRig;

public class LatLngHistory(GeolocationResult ev)
{
    public double Latitude { get; } = ev.Position.Coords.Latitude;
    public double Longitude { get; } = ev.Position.Coords.Longitude;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string TimestampString => $"{Timestamp:yyyy/MM/dd HH:mm:ss}";
}