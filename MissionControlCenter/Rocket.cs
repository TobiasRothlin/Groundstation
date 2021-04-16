using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace MissionControlCenter
{
    public class Rocket
    {
        private string flightComputerIp;
        private int udpPort;
        private UdpClient udp;

        public event Action<RocketTelemetry> TelemetryUpdate;

        private DataLogger FlightData = new DataLogger("FlightLog", DataLogger.Type.csv);

        public Rocket(string flightComputerIp = "192.168.3.1", int udpPort = 2390)
        {
            this.flightComputerIp = flightComputerIp;
            this.udpPort = udpPort;
            FlightData.AppendLine("time,dynamicPitch,dynamicRoll,dynamicYaw,AcX,AcY,AcZ,GyX,GyY,GyZ\n");
        }

        public void launch()
        {
            sendData("L");
            startListener();
        }

        public void test()
        {
            sendData("T");
            startListener();
        }

        public void getRocketStatus()
        {
            sendData("S");
            startListener(0);
        }

        public void sendData(string data)
        {
            this.udp = new UdpClient(udpPort);
            udp.Connect(this.flightComputerIp, this.udpPort);
            var sendBytes = Encoding.ASCII.GetBytes(data);
            try
            {
                udp.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception r)
            {
                Console.WriteLine(r.ToString());
            }
            udp.Close();
        }

        public async void startListener(int peridticTelemetryUpdate = 10)
        {
            UdpClient receivingUdpClient = new UdpClient(2390);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            string returnData = "";
            try
            {
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                returnData = Encoding.ASCII.GetString(receiveBytes);

                int i = 0;
                while (returnData != "E")
                {
                    FlightData.AppendLine(returnData);
                    if(i >= peridticTelemetryUpdate)
                    {
                        TelemetryUpdate(new RocketTelemetry()
                        {
                            rawData = returnData
                            
                   
                        });
                        await Task.Delay(1);
                        i = 0;
                    }
                    i++;

                    receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    returnData = Encoding.ASCII.GetString(receiveBytes);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            receivingUdpClient.Close();
            FlightData.WriteFile();
        }
    }


}
