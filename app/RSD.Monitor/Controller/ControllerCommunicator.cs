using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace RSD.Monitor.Controller
{
    /**
     * Comunica com o Controlador  
     */
    public class ControllerCommunicator
    {
        private SerialPort sPort = null;

        public ControllerCommunicator(String port)
        {
            this.sPort = new SerialPort(port, 19200);
        }

        /*
         * Inicializa a Comunicação com o Controller
         */
        public void Initializes()
        {
            this.sPort.Open();
            Thread.Sleep(100);
            this.SetValue(1, 100); // ENVIA VALORES FORAM DA ESCALA PARA LIMPAR
            this.SetValue(2, 100);
            this.SetValue(3, 100);
        }

        /*
         * Para a Comunição e Fehca a Porta
         */
        public void Stops() {
            this.sPort.Close();
        }

        /**
         * Indica se a Porta está Aberta
         */
        public bool IsOpen()
        {
            return this.sPort.IsOpen;
        }

        /**
         * Seta todos os valores para fora de escala e zera o display
         */
        public void Clear()
        {
            this.SetValue(1, 100);
            this.SetValue(2, 100);
            this.SetValue(3, 100);
        }

        /**
         * Liga/Desliga os Leds do Controlador
         */
        public void SetLed(int index, bool onOff)
        {
            String cmd = "L" + index + "" + (onOff ? 1 : 0);
            this.sPort.WriteLine(cmd);
            Thread.Sleep(1);
        }

        /**
         * Envia um valor para o Controlador
         */
        public void SetValue(int index, int value)
        {
            String cmd = "S" + index + "" + value;
            this.sPort.WriteLine(cmd);            
            Thread.Sleep(1);
        }
    }
}
