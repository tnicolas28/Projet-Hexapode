using System;
using Microsoft.SPOT;
using System.Threading;
using Gadgeteer;
using GTM = Gadgeteer.Modules;
using GT = Gadgeteer;

namespace Movement
{
    class MovementHexapod
    {

        private Gadgeteer.Modules.GHIElectronics.RS485 rs485;
        private Gadgeteer.Modules.GHIElectronics.Extender extender;

        GT.Interfaces.Serial serie;
        uint step = 2;
        GT.Interfaces.DigitalOutput S11P3dirAx12;

        public MovementHexapod() 
        {
            //---Initializing modules---//
            this.extender = new GTM.GHIElectronics.Extender(11);
            this.rs485 = new GTM.GHIElectronics.RS485(this.extender.ExtenderSocketNumber);
            serie = new GT.Interfaces.Serial(GT.Socket.GetSocket(11, true, null, string.Empty), 200000, GT.Interfaces.Serial.SerialParity.None, GT.Interfaces.Serial.SerialStopBits.One, 8, GT.Interfaces.Serial.HardwareFlowControl.NotRequired, null);
            serie.Open();

            S11P3dirAx12 = extender.SetupDigitalOutput(GT.Socket.Pin.Three, true); //False pour lire, True pour écrire
            S11P3dirAx12.Write(true);

        }

        //--- Actuator control ---//

        public void rotateEngine(int degre, byte id)
        {
            int valhex = degre * 0x3ff / 300;
            byte[] buffer = new byte[] { 0xFF, 0xFF, id, 0x05, 0x03, 0x1E, 0, 0, 0 };

            buffer[6] = (byte)(valhex & 0xff);
            buffer[7] = (byte)(valhex >> 8);

            calculateCheckSum(3, buffer);
            serie.Write(buffer);

        }

        public void setSpeed(int speed, byte id)//0-114)
        {

            int valhex = speed * 0x3ff / 114;
            byte[] buffer = new byte[] { 0xFF, 0xFF, id, 0x05, 0x03, 0x20, 0, 0, 0 };
            buffer[6] = (byte)(valhex & 0xFF);
            buffer[7] = (byte)(valhex >> 8);


            calculateCheckSum(3, buffer);
            serie.Write(buffer);


        }

        void calculateCheckSum(int nbParameters, byte[] buff)
        {
            int checkSum = 0;

            for (int i = 2; i < 5 + nbParameters; i++)
            {
                checkSum += buff[i];
            }

            buff[5 + nbParameters] = (byte)(~checkSum & 0xff);

        }

        //---Paw control---//
        //---Forward---// 
        void FirstMovementForward(int paw)
        {
            switch (paw)
            {
                case 1:
                    rotateEngine(120, 0x01);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x03);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x05);
                    break;
                case 2:
                    rotateEngine(120, 0x02);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x04);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x06);
                    break;
                case 3:
                    rotateEngine(120, 0x07);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x09);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x0B);
                    break;
                case 4:
                    rotateEngine(120, 0x08);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x0A);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x0C);
                    break;
                case 5:
                    rotateEngine(120, 0x0D);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x0F);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x11);
                    break;
                case 6:
                    rotateEngine(120, 0x0E);
                    Thread.Sleep(100);
                    rotateEngine(140, 0x10);
                    Thread.Sleep(300);
                    rotateEngine(180, 0x12);
                    break;
                default:
                    break;
            }
        }

        void SecondMovementForward(int paw)
        {

            switch (paw)
            {
                case 1:
                    rotateEngine(190, 0x01);
                    Thread.Sleep(300);
                    rotateEngine(170, 0x03);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x05);
                    break;
                case 2:
                    rotateEngine(190, 0x02);
                    Thread.Sleep(100);
                    rotateEngine(170, 0x04);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x06);
                    break;
                case 3:
                    rotateEngine(190, 0x07);
                    Thread.Sleep(100);
                    rotateEngine(170, 0x09);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x0B);
                    break;
                case 4:
                    rotateEngine(190, 0x08);
                    Thread.Sleep(100);
                    rotateEngine(170, 0x0A);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x0C);
                    break;
                case 5:
                    rotateEngine(190, 0x0D);
                    Thread.Sleep(100);
                    rotateEngine(170, 0x0F);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x11);
                    break;
                case 6:
                    rotateEngine(190, 0x0E);
                    Thread.Sleep(100);
                    rotateEngine(170, 0x10);
                    Thread.Sleep(300);
                    rotateEngine(150, 0x12);
                    break;
                default:
                    break;


            }
        }

        void goForward()
        {
            FirstMovementForward(3);
            Thread.Sleep(300);
            FirstMovementForward(4);
            Thread.Sleep(300);
            FirstMovementForward(5);
            Thread.Sleep(300);
            FirstMovementForward(6);
            Thread.Sleep(300);
            FirstMovementForward(1);
            Thread.Sleep(300);
            FirstMovementForward(2);

            Thread.Sleep(500);

            SecondMovementForward(1);
            Thread.Sleep(300);
            SecondMovementForward(2);
            Thread.Sleep(300);
            SecondMovementForward(6);
            Thread.Sleep(300);
            SecondMovementForward(5);
            Thread.Sleep(300);
            SecondMovementForward(4);
            Thread.Sleep(300);
            SecondMovementForward(3);
            Thread.Sleep(300);


        }
        //---Backward---//
        void firstMovementBackward() { 

        }

        void secondMovementBackward(){

        }
    }
}
