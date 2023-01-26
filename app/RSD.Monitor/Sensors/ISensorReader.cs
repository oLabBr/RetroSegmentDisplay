using System;
using System.Collections.Generic;
using System.Text;

namespace RSD.Monitor.Sensors
{
    public enum SensorId
    {
        CPU_Temp,
        MB_Temp,
        HD1_Temp,
        CPU_Voltage
    }

    public interface ISensorReader
    {
        void Update();
        int GetSensorValue(SensorId id);
    }
}
