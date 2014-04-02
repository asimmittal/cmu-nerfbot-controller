using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Thread tcpClientThread;
        bool isClientRunning = false;
        const int height = 120;
        const int width = 160;
        const int BPP = 3;

        const int bufSize = height * width * BPP;
        byte[] buffer = new byte[bufSize];
        
        public Form1()
        {
            InitializeComponent();            
        }

        private void clientThread()
        {
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(txtAddress.Text), 3000);
            client.Connect(serverEndPoint);
            NetworkStream clientStream = client.GetStream();
            isClientRunning = true;

            while (isClientRunning)
            {
                try
                {
                    clientStream.Read(buffer, 0, bufSize);
                    Bitmap bmp = copyDataToBitmap(buffer);
                    imageBox.Image = new Image<Bgr, byte>(bmp);
                }
                catch (Exception ex) { }
            }
        }

        public Bitmap copyDataToBitmap(byte[] data)
        {
            PixelFormat pForm;

            if (BPP == 3) pForm = PixelFormat.Format24bppRgb;
            else if (BPP == 1) pForm = PixelFormat.Format8bppIndexed;

            Bitmap bmpImage = new Bitmap(width, height, pForm);
            Rectangle rectI = new Rectangle(0, 0, bmpImage.Width, bmpImage.Height);
            BitmapData imageData = bmpImage.LockBits(rectI, ImageLockMode.WriteOnly, pForm);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, imageData.Scan0, data.Length);
            bmpImage.UnlockBits(imageData);

            return bmpImage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClientRunning = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            tcpClientThread = new Thread(clientThread);
            tcpClientThread.Start();
        }

    }
}
