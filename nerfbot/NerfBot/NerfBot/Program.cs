using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.Util;

namespace NerfBot
{
    class Program
    {
        class Win32Interop
        {
            [DllImport("crtdll.dll")]
            public static extern int _kbhit();
        }

        Emgu.CV.Capture camera;
        System.Timers.Timer tmr;
        const int width = 160;
        const int height = 120;
        Emgu.CV.Image<Bgr, byte> toSend = new Emgu.CV.Image<Bgr, byte>(width, height);

        private TcpListener tcpListener;
        private Thread listenThread;

        bool isServerRunning = false;
        bool isContentFresh = false;
        long frameCount = 0;

        static void Main(string[] args)
        {
            Program obj = new Program();
            obj.init();
            while (Win32Interop._kbhit() == 0) ;

            Console.WriteLine("---> Key Pressed. End of program <---");
            obj.isServerRunning = false;
        }

        private void init()
        {
            Console.WriteLine("---> Setting up TCP Server...");
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            listenThread = new Thread(new ThreadStart(listener));
            listenThread.Start();

            Console.WriteLine("---> Setting up Webcam...");
            camera = new Emgu.CV.Capture();
            camera.FlipHorizontal = true;

            Console.WriteLine("---> Starting camera timer...");
            tmr = new System.Timers.Timer(150);
            tmr.Elapsed += new ElapsedEventHandler(tmr_Elapsed);
            tmr.Start();
        }

        private void tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            Emgu.CV.Image<Bgr, byte> frame = camera.QueryFrame();
            toSend = frame.Copy();

            do
            {
            toSend = toSend.PyrDown();
            } while (toSend.Width != width);

            if(frameCount == 0) Console.WriteLine("------> New image frame from cam at " + toSend.Width + " x " + toSend.Height);
            frameCount++;

            if (frameCount%2 == 0) isContentFresh = false;
            else isContentFresh = true;
        }

        private void listener()
        {
            Console.WriteLine("------> Starting socket server...");
            tcpListener.Start();
            isServerRunning = true;

            while (isServerRunning)
            {
                Console.WriteLine("------> Waiting for connection from client...");
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(clientHandler));
                clientThread.Start(tcpClient);
            }

            Console.WriteLine("------> Killing socket server...");
        }

        private void clientHandler(object param)
        {
            Console.WriteLine("---------> Connection received, starting client thread");
            TcpClient client = (TcpClient)param;
            NetworkStream stream = client.GetStream();
            while (isServerRunning)
            {
                if (isContentFresh)
                {
                    byte[] data = toSend.Bytes;
                    try
                    {
                        stream.Write(data, 0, data.Length);
                        isContentFresh = false;
                        Thread.Sleep(5);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("---------> Connection closed");
                        break;                        
                    }

                    Console.WriteLine("---------> Sent image frame. Bytes: " + data.Length);
                }
            }

            Console.WriteLine("---------> Killing client thread...");
        }
    }
}
