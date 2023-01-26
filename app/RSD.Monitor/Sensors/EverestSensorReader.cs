using System;
using System.Collections.Generic;
using System.Text;
using RSD.Monitor.Native;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;

namespace RSD.Monitor.Sensors
{
    public class EverestSensorReader : ISensorReader
    {
        private String lastData = "";
        private Dictionary<SensorId, string> extractedData
               = new Dictionary<SensorId, string>();

        public void Update()
        {
            this.lastData = "";

            int dataSize = (4 * 1024); // 10KB

            // Opens the Shared Memory Data
            IntPtr handle = SharedMemory.OpenFileMapping(SharedMemoryConsts.FILE_MAP_READ, false, "EVEREST_SensorValues");
            int lastError = Marshal.GetLastWin32Error();

            if (handle == SharedMemoryConsts.INVALID_HANDLE_VALUE)
            {
                throw new ExecutionEngineException("Cant open the Everest Sensor Data", new Exception("Error code: " + lastError));
            }

            IntPtr m_pBuff = SharedMemory.MapViewOfFile(handle, SharedMemoryConsts.FILE_MAP_READ, 0, 0, 0);

            // - Copys all data to the local app
            byte[] sensorData = new byte[dataSize];
            Marshal.Copy(m_pBuff, sensorData, 0, dataSize);
            this.lastData = "<data>";
            this.lastData += Encoding.Default.GetString(sensorData).Trim('\0');
            this.lastData += "</data>";
                  
            
            // - Closes
            SharedMemory.UnmapViewOfFile(m_pBuff);
            SharedMemory.CloseHandle(handle);

            // 
            this.ExtractData();
        }

        private void ExtractData()
        {
            if (!string.IsNullOrEmpty(lastData))
            {
                this.extractedData.Clear();

                StringReader dataReader = new StringReader(this.lastData);
                XmlReaderSettings readerSets = new XmlReaderSettings();
                readerSets.ConformanceLevel = ConformanceLevel.Auto;
                readerSets.ValidationType = ValidationType.None;

                XmlDocument reader = new XmlDocument();
                reader.LoadXml(
                        lastData
                    );

                // 
                XmlNodeList rootData = reader.GetElementsByTagName("data")
                        .Item(0)
                        .ChildNodes;
                foreach (XmlElement currentSensor in rootData)
                {
                    String sensorId = "";
                    String sensorValue = "";

                    // Extracts the Sensor Data
                    foreach (XmlElement sensorData in currentSensor.ChildNodes)
                    {
                        if (sensorData.Name == "id")
                        {
                            sensorId = sensorData.InnerText;
                        }
                        else if (sensorData.Name == "value")
                        {
                            sensorValue = sensorData.InnerText;
                        }
                    }

                    this.SetSensorValue(sensorId.Trim().ToUpper(), sensorValue.Trim());
                }
            }
        }

        private void SetSensorValue(String id, String value)
        {
            switch (id)
            {
                case "TMOBO":
                    this.extractedData.Add(SensorId.MB_Temp, value);
                    break;
                case "THDD1":
                    this.extractedData.Add(SensorId.HD1_Temp, value);
                    break;
                case "TCPU":
                    this.extractedData.Add(SensorId.CPU_Temp, value);
                    break;
                case "VCPU":
                    String nValue = ((int)decimal.Parse(value.Replace(".", ","))).ToString();
                    this.extractedData.Add(SensorId.CPU_Voltage, nValue);
                    break;
                default:
                    break;
            }
        }

        public int GetSensorValue(SensorId id)
        {
            int sValue = -1;

            if (this.extractedData.ContainsKey(id))
            {
                int.TryParse(this.extractedData[id], out sValue);
            }

            return sValue;
        }
    }
}
