using System;
using System.IO;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;


using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;


namespace GadgeteerApp1
{


    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   

        GT.Interfaces.Serial serie;
        uint step = 2;
        GT.Interfaces.DigitalOutput S11P3dirAx12;


        void ProgramStarted()
        {

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            displayText("Debug: ", GT.Color.Red);
            serie = new GT.Interfaces.Serial(GT.Socket.GetSocket(11, true, null, string.Empty), 200000, GT.Interfaces.Serial.SerialParity.None, GT.Interfaces.Serial.SerialStopBits.One, 8, GT.Interfaces.Serial.HardwareFlowControl.NotRequired, null);
            serie.Open();


            xBee.Configure(115200, GT.Interfaces.Serial.SerialParity.None, GT.Interfaces.Serial.SerialStopBits.One, 8);
            xBee.SerialLine.Open();
            xBee.SerialLine.DataReceived += new GT.Interfaces.Serial.DataReceivedEventHandler(SerialLine_DataReceived);

            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            button.ButtonReleased += new Button.ButtonEventHandler(button_ButtonReleased);

            S11P3dirAx12 = extender.SetupDigitalOutput(GT.Socket.Pin.Three, true); //False pour lire, True pour écrire
            S11P3dirAx12.Write(true);

            //Output to see if the controller has received the frame
            xBee.SerialLine.WriteLine("Connected");

        }



        void SerialLine_DataReceived(GT.Interfaces.Serial sender, System.IO.Ports.SerialData data)
        {
            //Communication avec l'IHM en xBee
            var font = Resources.GetFont(Resources.FontResources.NinaB);
            string msg = receiveBuffXbee();

            string rotate = "tourner";
            string speed = "vitesse";
            string avancer = "avancer";
            string reculer = "reculer";
            string droite = "droite";
            string gauche = "gauche";
            bool command_rotate = true;
            bool command_speed = true;
            bool command_avancer = true;
            bool command_reculer = true;
            bool command_droite = true;
            bool command_gauche = true;

            for (int i = 0; i < rotate.Length; i++)
            {
                if ((msg[i] != rotate[i]) || (msg.Length > 10))
                {
                    command_rotate = false;
                    break;
                }

            }

            for (int i = 0; i < speed.Length; i++)
            {
                if ((msg[i] != speed[i]) || (msg.Length > 10))
                {
                    command_speed = false;
                    break;
                }
            }

            for (int i = 0; i < avancer.Length; i++)
            {
                if ((msg[i] != avancer[i] || (msg.Length > 10)))
                {
                    command_avancer = false;
                    break;
                }

            }

            for (int i = 0; i < reculer.Length; i++)
            {
                if ((msg[i] != reculer[i] || (msg.Length > 10)))
                {
                    command_reculer = false;
                    break;
                }

            }

            for (int i = 0; i < droite.Length; i++)
            {
                if ((msg[i] != droite[i] || (msg.Length > 10)))
                {
                    command_droite = false;
                    break;
                }

            }

            for (int i = 0; i < gauche.Length; i++)
            {
                if ((msg[i] != gauche[i] || (msg.Length > 10)))
                {
                    command_gauche = false;
                    break;
                }

            }

            if ((command_gauche == true) && (command_speed == false) && (command_rotate == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(6));

                displayText("Gauche", GT.Color.White);
                turnLeft();
                Thread.Sleep(200);
            }

            if ((command_droite == true) && (command_speed == false) && (command_rotate == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(6));

                displayText("Droite" + value, GT.Color.White);
                turnRight();
                Thread.Sleep(200);
            }

            if ((command_reculer == true) && (command_speed == false) && (command_rotate == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(7));

                displayText("Reculer", GT.Color.White);
                goBackward();
                Thread.Sleep(200);
            }

            if ((command_avancer == true) && (command_speed == false) && (command_rotate == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(7));

                displayText("Avancer", GT.Color.White);
                goForward();
                Thread.Sleep(200);
            }

            if ((command_rotate == true) && (command_speed == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(7));
                displayText("Rotation: " + value + " degres", GT.Color.White);
                Thread.Sleep(200);
            }

            if ((command_speed == true) && (command_rotate == false))
            {
                int value = System.Convert.ToInt16(msg.Substring(7));
                displayText("Speed: " + value + " ms", GT.Color.White);

                setSpeed(value, 0x01);
                Thread.Sleep(10);
                setSpeed(value, 0x02);
                Thread.Sleep(200);
            }


        }

        void reinitServo()
        {
            byte[] buffer = new byte[] { 0xFF, 0xFF, 0X01, 0x05, 0x03, 0x1E, 0x00, 0x00, 0xD8 };
            serie.Write(buffer);
        }

        void button_ButtonReleased(Button sender, Button.ButtonState state)
        {
            if (step > 229)
            {
                display_T35.SimpleGraphics.Clear();
                step = 2;
                displayText("Debug: ", GT.Color.Red);

            }


            //First paw to move comes back to its initial position
            rotateEngine(150, 0x0B);
          
            Thread.Sleep(100);
            rotateEngine(200, 0x09);
            Thread.Sleep(400);
              rotateEngine(150, 0x07);
            getStatusPacket(0x07);
            displayText("Led: OFF", GT.Color.White);
            Thread.Sleep(200);



        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            //First paw to move => N°2
            setSpeed(20, 0x07);
            setSpeed(20, 0x09);
            setSpeed(20, 0x0B);
            rotateEngine(80, 0x0B);
            Thread.Sleep(100);
            rotateEngine(120, 0x09);
            Thread.Sleep(300);
            rotateEngine(200, 0x07);
            Thread.Sleep(500);
            displayText("Led: ON", GT.Color.White);
            Thread.Sleep(200);
        }

        public void displayText(string msg, GT.Color color)
        {
            var font = Resources.GetFont(Resources.FontResources.small);
            display_T35.SimpleGraphics.DisplayText(msg, font, color, 4, step);
            step += 12;

        }



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

        public string receiveBuffXbee()
        {
            int buff = xBee.SerialLine.BytesToRead;

            byte[] buffer = new byte[buff];
            xBee.SerialLine.Read(buffer, 0, buff);

            string msg = new string(System.Text.Encoding.UTF8.GetChars(buffer));
            return msg;
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

        void getStatusPacket(byte id)
        {

            byte[] statusPacket = new byte[] { 0xFF, 0xFF, id, 0x02, 0x01, 0 };
            calculateCheckSum(0, statusPacket);
            Thread.Sleep(2);
            serie.Write(statusPacket); //On ping le servomoteur afin qu'il nous retourne une trame d'erreur   


            S11P3dirAx12.Write(false);//Passage en mode lecture
            Thread.Sleep(10);//On attend un certain délai avant de lire 

            int nbOctetReponse = serie.BytesToRead;
            int[] intDisplay = new int[nbOctetReponse];
            byte[] receivePacket = new byte[nbOctetReponse];
            string convert;
            serie.Read(receivePacket, 0, nbOctetReponse);
            displayText("Nombres d'octets recus : " + nbOctetReponse.ToString(), GT.Color.Magenta);

            for (int i = 0; i < nbOctetReponse; i++)
            {
                intDisplay[i] = (int)receivePacket[i];
                convert = intDisplay[i].ToString("X");
                displayText(convert, GT.Color.Green);
            }

            S11P3dirAx12.Write(true);


        }

        //--Movement section--//
        //--Forward--//

        void goForward()
        {
            //First paw to move => Paw n°3
            rotateEngine(200, 0x07);
            Thread.Sleep(100);
            rotateEngine(120, 0x09);
            Thread.Sleep(300);
            rotateEngine(80, 0x0B);
            Thread.Sleep(500);
            //Second paw to move => Paw n° 6
            rotateEngine(200, 0x0E);
            Thread.Sleep(100);
            rotateEngine(120, 0x10);
            Thread.Sleep(300);
            rotateEngine(80, 0x12);
            Thread.Sleep(500);
            //First paw to move comes back to its initial position
            rotateEngine(150, 0x07);
            Thread.Sleep(100);
            rotateEngine(200, 0x09);
            Thread.Sleep(300);
            rotateEngine(150, 0x0B);
            //Third paw to move => Paw n°1
            rotateEngine(200, 0x01);
            Thread.Sleep(100);
            rotateEngine(120, 0x03);
            Thread.Sleep(300);
            rotateEngine(80, 0x05);
            Thread.Sleep(500);
            //Second paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
            //Fourth paw to move => Paw n°4
            rotateEngine(200, 0x08);
            Thread.Sleep(100);
            rotateEngine(120, 0x0A);
            Thread.Sleep(300);
            rotateEngine(80, 0x0C);
            Thread.Sleep(500);
            //Third paw to move comes back to its initial position 
            rotateEngine(150, 0x01);
            Thread.Sleep(300);
            rotateEngine(200, 0x03);
            Thread.Sleep(300);
            rotateEngine(150, 0x05);
            Thread.Sleep(500);
            //Fifth paw to move => Paw n°5
            rotateEngine(200, 0x0D);
            Thread.Sleep(100);
            rotateEngine(120, 0x0F);
            Thread.Sleep(300);
            rotateEngine(80, 0x11);
            Thread.Sleep(500);
            //Fourth paw to move comes back to its initial position
            rotateEngine(150, 0x08);
            Thread.Sleep(100);
            rotateEngine(200, 0x0A);
            Thread.Sleep(300);
            rotateEngine(150, 0x0C);
            Thread.Sleep(500);
            //Sixth paw to move => Paw n°2
            rotateEngine(200, 0x02);
            Thread.Sleep(100);
            rotateEngine(120, 0x04);
            Thread.Sleep(300);
            rotateEngine(80, 0x06);
            //Fifth paw to move comes back to its initial position
            rotateEngine(150, 0x0D);
            Thread.Sleep(100);
            rotateEngine(200, 0x0F);
            Thread.Sleep(300);
            rotateEngine(150, 0x11);
            Thread.Sleep(500);
            //Sixth paw to move comes back to its initial position
            rotateEngine(150, 0x02);
            Thread.Sleep(100);
            rotateEngine(200, 0x04);
            Thread.Sleep(300);
            rotateEngine(150, 0x06);
        }

        //--Go backward--//
        void goBackward()
        {
            //First paw to move => N°2
            rotateEngine(80, 0x02);
            Thread.Sleep(100);
            rotateEngine(120, 0x04);
            Thread.Sleep(300);
            rotateEngine(80, 0x06);
            Thread.Sleep(500);
            //Second paw to move => N°5
            rotateEngine(80, 0x0D);
            Thread.Sleep(100);
            rotateEngine(120, 0x0F);
            Thread.Sleep(300);
            rotateEngine(80, 0x11);
            Thread.Sleep(500);
            //First paw to move comes back to its initial position
            rotateEngine(150, 0x02);
            Thread.Sleep(100);
            rotateEngine(200, 0x04);
            Thread.Sleep(300);
            rotateEngine(150, 0x06);
            Thread.Sleep(500);
            //Third paw to move => N°4
            rotateEngine(80, 0x08);
            Thread.Sleep(100);
            rotateEngine(120, 0x0A);
            Thread.Sleep(300);
            rotateEngine(80, 0x0C);
            Thread.Sleep(500);
            //Second paw to move comes back to its initial position
            rotateEngine(150, 0x0D);
            Thread.Sleep(100);
            rotateEngine(200, 0x0F);
            Thread.Sleep(300);
            rotateEngine(150, 0x11);
            //Fourth paw to move => N°1
            rotateEngine(80, 0x01);
            Thread.Sleep(100);
            rotateEngine(120, 0x03);
            Thread.Sleep(300);
            rotateEngine(80, 0x05);
            Thread.Sleep(500);
            //Third paw to move comes back to its initial position
            rotateEngine(150, 0x08);
            Thread.Sleep(100);
            rotateEngine(200, 0x0A);
            Thread.Sleep(300);
            rotateEngine(150, 0x0C);
            Thread.Sleep(500);
            //Fifth paw to move => N°6
            rotateEngine(80, 0x0E);
            Thread.Sleep(100);
            rotateEngine(120, 0x10);
            Thread.Sleep(300);
            rotateEngine(80, 0x12);
            Thread.Sleep(500);
            //Fourth paw to move comes back to its initial position
            rotateEngine(150, 0x01);
            Thread.Sleep(300);
            rotateEngine(200, 0x03);
            Thread.Sleep(300);
            rotateEngine(150, 0x05);
            Thread.Sleep(500);
            //Sixth paw to move => N°3
            rotateEngine(200, 0x09);
            Thread.Sleep(100);
            rotateEngine(120, 0x0B);
            Thread.Sleep(500);
            rotateEngine(80, 0x07);
            Thread.Sleep(500);
            //Fifth paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
            //Sixth paw to move comes back to its initial position
            rotateEngine(150, 0x07);
            Thread.Sleep(100);
            rotateEngine(200, 0x0B);
            Thread.Sleep(300);
            rotateEngine(150, 0x09);

        }

        void turnLeft()
        {
           //First paw to move => Paw n°3
            rotateEngine(200, 0x07);
            Thread.Sleep(100);
            rotateEngine(120, 0x09);
            Thread.Sleep(300);
            rotateEngine(80, 0x0B);
            Thread.Sleep(500);
           //Second paw to move => Paw n°6
            rotateEngine(80, 0x0E);
            Thread.Sleep(100);
            rotateEngine(120, 0x10);
            Thread.Sleep(300);
            rotateEngine(80, 0x12);
            Thread.Sleep(500);
          //First paw to move comes back to its initial position
            rotateEngine(150, 0x07);
            Thread.Sleep(100);
            rotateEngine(200, 0x09);
            Thread.Sleep(300);
            rotateEngine(150, 0x0B);
         //Third paw to move => Paw n°1
            rotateEngine(200, 0x01);
            Thread.Sleep(100);
            rotateEngine(120, 0x03);
            Thread.Sleep(300);
            rotateEngine(80, 0x05);
            Thread.Sleep(500);
        //Second paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
        //Fourth paw to move => Paw n°4
            rotateEngine(80, 0x08);
            Thread.Sleep(100);
            rotateEngine(120, 0x0A);
            Thread.Sleep(300);
            rotateEngine(80, 0x0C);
            Thread.Sleep(500);
        //Third paw to move comes back to its initial position 
            rotateEngine(150, 0x01);
            Thread.Sleep(300);
            rotateEngine(200, 0x03);
            Thread.Sleep(300);
            rotateEngine(150, 0x05);
            Thread.Sleep(500);
        //Fifth paw to move => N°5
            rotateEngine(200, 0x0D);
            Thread.Sleep(100);
            rotateEngine(120, 0x0F);
            Thread.Sleep(300);
            rotateEngine(80, 0x11);
            Thread.Sleep(500);
        //Fourth paw to move comes back to its initial position
            rotateEngine(150, 0x08);
            Thread.Sleep(100);
            rotateEngine(200, 0x0A);
            Thread.Sleep(300);
            rotateEngine(150, 0x0C);
            Thread.Sleep(500);
        //Sixth paw to move => N°2
            rotateEngine(80, 0x02);
            Thread.Sleep(100);
            rotateEngine(120, 0x04);
            Thread.Sleep(300);
            rotateEngine(80, 0x06);
            Thread.Sleep(500);
        //Fifth paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
        //Sixth paw to move comes back to its initial position
            rotateEngine(150, 0x02);
            Thread.Sleep(100);
            rotateEngine(200, 0x04);
            Thread.Sleep(300);
            rotateEngine(150, 0x06);
            Thread.Sleep(500);
       }

        void turnRight() {
            //First paw to move => Paw n°3
            rotateEngine(80, 0x07);
            Thread.Sleep(100);
            rotateEngine(120, 0x09);
            Thread.Sleep(300);
            rotateEngine(80, 0x0B);
            Thread.Sleep(500);
            //Second paw to move => Paw n°6
            rotateEngine(200, 0x0E);
            Thread.Sleep(100);
            rotateEngine(120, 0x10);
            Thread.Sleep(300);
            rotateEngine(80, 0x12);
            Thread.Sleep(500);
            //First paw to move comes back to its initial position
            rotateEngine(150, 0x07);
            Thread.Sleep(100);
            rotateEngine(200, 0x09);
            Thread.Sleep(300);
            rotateEngine(150, 0x0B);
            //Third paw to move => Paw n°1
            rotateEngine(80, 0x01);
            Thread.Sleep(100);
            rotateEngine(120, 0x03);
            Thread.Sleep(300);
            rotateEngine(80, 0x05);
            Thread.Sleep(500);
            //Second paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
            //Fourth paw to move => Paw n°4
            rotateEngine(200, 0x08);
            Thread.Sleep(100);
            rotateEngine(120, 0x0A);
            Thread.Sleep(300);
            rotateEngine(80, 0x0C);
            Thread.Sleep(500);
            //Third paw to move comes back to its initial position 
            rotateEngine(150, 0x01);
            Thread.Sleep(300);
            rotateEngine(200, 0x03);
            Thread.Sleep(300);
            rotateEngine(150, 0x05);
            Thread.Sleep(500);
            //Fifth paw to move => N°5
            rotateEngine(80, 0x0D);
            Thread.Sleep(100);
            rotateEngine(120, 0x0F);
            Thread.Sleep(300);
            rotateEngine(80, 0x11);
            Thread.Sleep(500);
            //Fourth paw to move comes back to its initial position
            rotateEngine(150, 0x08);
            Thread.Sleep(100);
            rotateEngine(200, 0x0A);
            Thread.Sleep(300);
            rotateEngine(150, 0x0C);
            Thread.Sleep(500);
            //Sixth paw to move => N°2
            rotateEngine(200, 0x02);
            Thread.Sleep(100);
            rotateEngine(120, 0x04);
            Thread.Sleep(300);
            rotateEngine(80, 0x06);
            Thread.Sleep(500);
            //Fifth paw to move comes back to its initial position
            rotateEngine(150, 0x0E);
            Thread.Sleep(100);
            rotateEngine(200, 0x10);
            Thread.Sleep(300);
            rotateEngine(150, 0x12);
            Thread.Sleep(500);
            //Sixth paw to move comes back to its initial position
            rotateEngine(150, 0x02);
            Thread.Sleep(100);
            rotateEngine(200, 0x04);
            Thread.Sleep(300);
            rotateEngine(150, 0x06);
            Thread.Sleep(500);

        }


    }
}
