using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;

namespace Main
{
    public enum Protocol
    {
        TCP = 6,
        UDP = 17,
        Unknown = -1
    };

    public partial class Form1 : Form
    {
        private Socket mainSocket;                         // Сокет, который захватывает все входящие пакеты
        private byte[] byteData = new byte[4096];
        private bool bContinueCapturing = false;           // Флаг для проверки, должны ли пакеты быть захвачены или нет

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //Получаем спсиок ip адресов
            IPHostEntry HosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HosyEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip in HosyEntry.AddressList)
                    cb.Items.Add(ip.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgv.Columns.Add("time", "Time");
            dgv.Columns.Add("source", "Source");
            dgv.Columns.Add("destination", "Destination");
            dgv.Columns.Add("protocol", "Protocol");
            dgv.Columns.Add("length", "Length");
        }

        private void but_Click(object sender, EventArgs e)
        {
            if(but.Text=="Старт")
            {
                if (!bContinueCapturing)
                {
                    but.Text = "Стоп";
                    bContinueCapturing = true;


                    mainSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Raw, ProtocolType.IP);

                    // Привязать сокет к выбранному IP-адресу
                    mainSocket.Bind(new IPEndPoint(IPAddress.Parse(cb.Text), 0));

                    // Установка параметров сокета
                    mainSocket.SetSocketOption(SocketOptionLevel.IP,           
                                               SocketOptionName.HeaderIncluded, 
                                               true);                           

                    byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
                    byte[] byOut = new byte[4] { 1, 0, 0, 0 }; // Захват исходящих пакетов

                 
                    mainSocket.IOControl(IOControlCode.ReceiveAll,             
                                         byTrue,
                                         byOut);

                    // Начать прием пакетов асинхронно
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
               
            }
            else
            {
                but.Text = "Старт";
                bContinueCapturing = false;
                mainSocket.Close();
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                

                ParseData(byteData, nReceived);

                if (bContinueCapturing)
                {
                    byteData = new byte[4096];

                    
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), null);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OnAddRow(IPHeader ipHeader)
        {
            if(dgv.Rows.Count>11)
            {
                dgv.Rows.RemoveAt(0);
                llb.RemoveAt(0);
            }

            dgv.Rows.Add(DateTime.Now.ToString("hh:mm:ss:fff")
           , ipHeader.SourceAddress,
           ipHeader.DestinationAddress,
           ipHeader.ProtocolType,
           ipHeader.TotalLength);
        }
        private delegate void AddRow(IPHeader ipHeader);

        List<ListBox> llb = new List<ListBox>();





        // Вспомогательная функция, которая возвращает информацию, содержащуюся в заголовке TCP 
        private ListBox MakeTCPNode(TCPHeader tcpHeader, IPHeader ipHeader)
        {
            ListBox tcpNode = new ListBox();

            tcpNode.Text = "TCP";
            tcpNode.Items.Add("TCP");
            tcpNode.Items.Add("Source Port: " + tcpHeader.SourcePort);
            tcpNode.Items.Add("Destination Port: " + tcpHeader.DestinationPort);
            tcpNode.Items.Add("Sequence Number: " + tcpHeader.SequenceNumber);

            if (tcpHeader.AcknowledgementNumber != "")
                tcpNode.Items.Add("Acknowledgement Number: " + tcpHeader.AcknowledgementNumber);

            tcpNode.Items.Add("Header Length: " + tcpHeader.HeaderLength);
            tcpNode.Items.Add("Flags: " + tcpHeader.Flags);
            tcpNode.Items.Add("Window Size: " + tcpHeader.WindowSize);
            tcpNode.Items.Add("Checksum: " + tcpHeader.Checksum);

            if (tcpHeader.UrgentPointer != "")
                tcpNode.Items.Add("Urgent Pointer: " + tcpHeader.UrgentPointer);

            tcpNode.Items.Add("IP");
            tcpNode.Items.Add("Identification: " + ipHeader.Identification);
            tcpNode.Items.Add("Flags: " + ipHeader.Flags);
            tcpNode.Items.Add("Fragmentation Offset: " + ipHeader.FragmentationOffset);
            tcpNode.Items.Add("Time to live: " + ipHeader.TTL);
            tcpNode.Items.Add("Ver ip: " + ipHeader.Version);
            tcpNode.Items.Add("Differntiated Services: " + ipHeader.DifferentiatedServices);
            tcpNode.Items.Add("Header Length: " + ipHeader.HeaderLength);


            return tcpNode;
        }

        // Вспомогательная функция, которая возвращает информацию, содержащуюся в заголовке UDP
        private ListBox MakeUDPNode(UDPHeader udpHeader, IPHeader ipHeader)
        {
            ListBox udpNode = new ListBox();

            udpNode.Text = "UDP";
            udpNode.Items.Add("Source Port: " + udpHeader.SourcePort);
            udpNode.Items.Add("Destination Port: " + udpHeader.DestinationPort);
            udpNode.Items.Add("Length: " + udpHeader.Length);
            udpNode.Items.Add("Checksum: " + udpHeader.Checksum);

            udpNode.Items.Add("Identification: " + ipHeader.Identification);
            udpNode.Items.Add("Flags: " + ipHeader.Flags);
            udpNode.Items.Add("Fragmentation Offset: " + ipHeader.FragmentationOffset);
            udpNode.Items.Add("Time to live: " + ipHeader.TTL);
            udpNode.Items.Add("Ver ip: " + ipHeader.Version);
            udpNode.Items.Add("Differntiated Services: " + ipHeader.DifferentiatedServices);
            udpNode.Items.Add("Header Length: " + ipHeader.HeaderLength);

            return udpNode;
        }

        // Вспомогательная функция, которая возвращает информацию, содержащуюся в заголовке DNS
        private ListBox MakeDNSNode(byte[] byteData, int nLength, IPHeader ipHeader)
        {
            DNSHeader dnsHeader = new DNSHeader(byteData, nLength);

            ListBox dnsNode = new ListBox();

            dnsNode.Items.Add("DNS");
           
            dnsNode.Items.Add("Identification: " + dnsHeader.Identification);
            dnsNode.Items.Add("Flags: " + dnsHeader.Flags);
            dnsNode.Items.Add("Questions: " + dnsHeader.TotalQuestions);
            dnsNode.Items.Add("Answer RRs: " + dnsHeader.TotalAnswerRRs);
            dnsNode.Items.Add("Authority RRs: " + dnsHeader.TotalAuthorityRRs);
            dnsNode.Items.Add("Additional RRs: " + dnsHeader.TotalAdditionalRRs);
            dnsNode.Items.Add("IP");
            dnsNode.Items.Add("Identification: " + ipHeader.Identification);
            dnsNode.Items.Add("Flags: " + ipHeader.Flags);
            dnsNode.Items.Add("Fragmentation Offset: " + ipHeader.FragmentationOffset);
            dnsNode.Items.Add("Time to live: " + ipHeader.TTL);
            dnsNode.Items.Add("Ver ip: " + ipHeader.Version);
            dnsNode.Items.Add("Differntiated Services: " + ipHeader.DifferentiatedServices);
            dnsNode.Items.Add("Header Length: " + ipHeader.HeaderLength);

            return dnsNode;
        }
        private void ParseData(byte[] byteData, int nReceived)
        {


            IPHeader ipHeader = new IPHeader(byteData, nReceived);

           AddRow ar = new AddRow(OnAddRow);


            dgv.Invoke(ar,ipHeader);

            switch(ipHeader.ProtocolType)
            {
                case Protocol.TCP:

                    TCPHeader tcpHeader = new TCPHeader(ipHeader.Data,             
                                                        ipHeader.MessageLength);
                    if (tcpHeader.DestinationPort == "53" || tcpHeader.SourcePort == "53")
                    {
                        llb.Add(MakeDNSNode(tcpHeader.Data, (int)tcpHeader.MessageLength, ipHeader));
                        break;
                    }

                    llb.Add(MakeTCPNode(tcpHeader,ipHeader));


                   

                    break;

                case Protocol.UDP:

                    UDPHeader udpHeader = new UDPHeader(ipHeader.Data,                                       
                                                       (int)ipHeader.MessageLength);                 

                    if (udpHeader.DestinationPort == "53" || udpHeader.SourcePort == "53")
                    {
                        llb.Add(MakeDNSNode(udpHeader.Data,Convert.ToInt32(udpHeader.Length) - 8, ipHeader));
                        break;
                    }

                    llb.Add(MakeUDPNode(udpHeader, ipHeader));

                    break;

                case Protocol.Unknown:
                    break;
            }         
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bContinueCapturing)
            {
                mainSocket.Close();
            }
        }

        private void dgv_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            lb.Items.Clear();
            lb.Items.AddRange(llb[e.RowIndex].Items);
        }
    }
    public class DNSHeader
    {
        // Поля заголовка DNS
        private ushort usIdentification;       // Шестнадцать бит для идентификации 
        private ushort usFlags; // Шестнадцать бит для флагов DNS  

        private ushort usTotalQuestions;          // Шестнадцать бит, указывающих на количество записей 
                                                  //в списке вопросов  

        private ushort usTotalAnswerRRs;          // Шестнадцать бит, указывающих на количество записей
                                                  // записи в списке записей ресурса ответа

        private ushort usTotalAuthorityRRs;      // Шестнадцать бит, указывающих на количество записей
                                                 // записи в списке записей ресурса полномочий

        private ushort usTotalAdditionalRRs; // Шестнадцать бит, указывающих на количество записей
                                             // записи в списке записей дополнительных ресурсов
                                             // Завершение полей заголовка DNS      







        public DNSHeader(byte[] byBuffer, int nReceived)
        {
            MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            BinaryReader binaryReader = new BinaryReader(memoryStream);

          
            usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

           
            usFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

           
            usTotalQuestions = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

     
            usTotalAnswerRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

         
            usTotalAuthorityRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

         
            usTotalAdditionalRRs = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
        }

        public string Identification
        {
            get
            {
                return string.Format("0x{0:x2}", usIdentification);
            }
        }

        public string Flags
        {
            get
            {
                return string.Format("0x{0:x2}", usFlags);
            }
        }

        public string TotalQuestions
        {
            get
            {
                return usTotalQuestions.ToString();
            }
        }

        public string TotalAnswerRRs
        {
            get
            {
                return usTotalAnswerRRs.ToString();
            }
        }

        public string TotalAuthorityRRs
        {
            get
            {
                return usTotalAuthorityRRs.ToString();
            }
        }

        public string TotalAdditionalRRs
        {
            get
            {
                return usTotalAdditionalRRs.ToString();
            }
        }
    }
    public class IPHeader
    {
        // Поля заголовка IP
        private byte byVersionAndHeaderLength;  
        private byte byDifferentiatedServices;  
        private ushort usTotalLength;       
        private ushort usIdentification;        
        private ushort usFlagsAndOffset;        
        private byte byTTL;                     
        private byte byProtocol;                
        private short sChecksum;                
                                                
        private uint uiSourceIPAddress;         
        private uint uiDestinationIPAddress;    
                                                

        private byte byHeaderLength;
        private byte[] byIPData = new byte[4096];

      
        public IPHeader(byte[] byBuffer, int nReceived)
        {

            try
            {

                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);

                BinaryReader binaryReader = new BinaryReader(memoryStream);

        
                byVersionAndHeaderLength = binaryReader.ReadByte();


                byDifferentiatedServices = binaryReader.ReadByte();

             
                usTotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

             
                usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

               
                usFlagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

             
                byTTL = binaryReader.ReadByte();

             
                byProtocol = binaryReader.ReadByte();

             
                sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                uiSourceIPAddress = (uint)(binaryReader.ReadInt32());

                
                uiDestinationIPAddress = (uint)(binaryReader.ReadInt32());

                //Теперь мы вычисляем длину заголовка 

                byHeaderLength = byVersionAndHeaderLength;
                //Последние четыре бита поля версия и длина заголовка содержат
                //длина заголовка, мы выполняем некоторые простые двоичные арифметические операции, чтобы
                //извлечь их
                byHeaderLength <<= 4;
                byHeaderLength >>= 4;
                // Умножьте на четыре, чтобы получить точную длину заголовка
                byHeaderLength *= 4;

                // Скопируйте данные, переносимые датаграммой, в другой массив, чтобы
                //согласно протоколу, который выполняется в датаграмме IP
                Array.Copy(byBuffer,
                           byHeaderLength,  //начать копирование с конца заголовка
                           byIPData, 0,
                           usTotalLength - byHeaderLength);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MJsniffer", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public string Version
        {
            get
            {
                if ((byVersionAndHeaderLength >> 4) == 4)
                {
                    return "IP v4";
                }
                else if ((byVersionAndHeaderLength >> 4) == 6)
                {
                    return "IP v6";
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        public string HeaderLength
        {
            get
            {
                return byHeaderLength.ToString();
            }
        }

        public ushort MessageLength
        {
            get
            {
            
                return (ushort)(usTotalLength - byHeaderLength);
            }
        }

        public string DifferentiatedServices
        {
            get
            {
                return string.Format("0x{0:x2} ({1})", byDifferentiatedServices,
                    byDifferentiatedServices);
            }
        }

        public string Flags
        {
            get
            {
             
                int nFlags = usFlagsAndOffset >> 13;
                if (nFlags == 2)
                {
                    return "Don't fragment";
                }
                else if (nFlags == 1)
                {
                    return "More fragments to come";
                }
                else
                {
                    return nFlags.ToString();
                }
            }
        }

        public string FragmentationOffset
        {
            get
            {
              
                int nOffset = usFlagsAndOffset << 3;
                nOffset >>= 3;

                return nOffset.ToString();
            }
        }

        public string TTL
        {
            get
            {
                return byTTL.ToString();
            }
        }

        public Protocol ProtocolType
        {
            get
            {
              
                if (byProtocol == 6)        
                {
                    return Protocol.TCP;
                }
                else if (byProtocol == 17)  
                {
                    return Protocol.UDP;
                }
                else
                {
                    return Protocol.Unknown;
                }
            }
        }

        public string Checksum
        {
            get
            {
            
                return string.Format("0x{0:x2}", sChecksum);
            }
        }

        public IPAddress SourceAddress
        {
            get
            {
                return new IPAddress(uiSourceIPAddress);
            }
        }

        public IPAddress DestinationAddress
        {
            get
            {
                return new IPAddress(uiDestinationIPAddress);
            }
        }

        public string TotalLength
        {
            get
            {
                return usTotalLength.ToString();
            }
        }

        public string Identification
        {
            get
            {
                return usIdentification.ToString();
            }
        }

        public byte[] Data
        {
            get
            {
                return byIPData;
            }
        }
    }
    public class TCPHeader
    {
   
        private ushort usSourcePort;              
        private ushort usDestinationPort;         
        private uint uiSequenceNumber = 555;
        private uint uiAcknowledgementNumber = 555;
        private ushort usDataOffsetAndFlags = 555;
        private ushort usWindow = 555;            
        private short sChecksum = 555;            
                                                  
        private ushort usUrgentPointer;           

                                                 
        private byte byHeaderLength;            
        private ushort usMessageLength;          
        private byte[] byTCPData = new byte[4096];

        public TCPHeader(byte[] byBuffer, int nReceived)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                BinaryReader binaryReader = new BinaryReader(memoryStream);

   
                usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                uiSequenceNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

           
                uiAcknowledgementNumber = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());

           
                usDataOffsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

         
                usWindow = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

       
                sChecksum = (short)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

           
                usUrgentPointer = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

           
                byHeaderLength = (byte)(usDataOffsetAndFlags >> 12);
                byHeaderLength *= 4;

            
                usMessageLength = (ushort)(nReceived - byHeaderLength);

            
                Array.Copy(byBuffer, byHeaderLength, byTCPData, 0, nReceived - byHeaderLength);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MJsniff TCP" + (nReceived), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string SourcePort
        {
            get
            {
                return usSourcePort.ToString();
            }
        }

        public string DestinationPort
        {
            get
            {
                return usDestinationPort.ToString();
            }
        }

        public string SequenceNumber
        {
            get
            {
                return uiSequenceNumber.ToString();
            }
        }

        public string AcknowledgementNumber
        {
            get
            {
           
                if ((usDataOffsetAndFlags & 0x10) != 0)
                {
                    return uiAcknowledgementNumber.ToString();
                }
                else
                    return "";
            }
        }

        public string HeaderLength
        {
            get
            {
                return byHeaderLength.ToString();
            }
        }

        public string WindowSize
        {
            get
            {
                return usWindow.ToString();
            }
        }

        public string UrgentPointer
        {
            get
            {
             
                if ((usDataOffsetAndFlags & 0x20) != 0)
                {
                    return usUrgentPointer.ToString();
                }
                else
                    return "";
            }
        }

        public string Flags
        {
            get
            {
               
                int nFlags = usDataOffsetAndFlags & 0x3F;

                string strFlags = string.Format("0x{0:x2} (", nFlags);

               
                if ((nFlags & 0x01) != 0)
                {
                    strFlags += "FIN, ";
                }
                if ((nFlags & 0x02) != 0)
                {
                    strFlags += "SYN, ";
                }
                if ((nFlags & 0x04) != 0)
                {
                    strFlags += "RST, ";
                }
                if ((nFlags & 0x08) != 0)
                {
                    strFlags += "PSH, ";
                }
                if ((nFlags & 0x10) != 0)
                {
                    strFlags += "ACK, ";
                }
                if ((nFlags & 0x20) != 0)
                {
                    strFlags += "URG";
                }
                strFlags += ")";

                if (strFlags.Contains("()"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3);
                }
                else if (strFlags.Contains(", )"))
                {
                    strFlags = strFlags.Remove(strFlags.Length - 3, 2);
                }

                return strFlags;
            }
        }

        public string Checksum
        {
            get
            {
         
                return string.Format("0x{0:x2}", sChecksum);
            }
        }

        public byte[] Data
        {
            get
            {
                return byTCPData;
            }
        }

        public ushort MessageLength
        {
            get
            {
                return usMessageLength;
            }
        }
    }
    public class UDPHeader
    {

        private ushort usSourcePort;           
        private ushort usDestinationPort;      
        private ushort usLength;               
        private short sChecksum;               
                                               
                                               

        private byte[] byUDPData = new byte[4096];  

        public UDPHeader(byte[] byBuffer, int nReceived)
        {
            MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
            BinaryReader binaryReader = new BinaryReader(memoryStream);

      
            usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

    
            usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

     
            usLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

     
            sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

         
            Array.Copy(byBuffer,
                       8,               
                       byUDPData,
                       0,
                       nReceived - 8);
        }

        public string SourcePort
        {
            get
            {
                return usSourcePort.ToString();
            }
        }

        public string DestinationPort
        {
            get
            {
                return usDestinationPort.ToString();
            }
        }

        public string Length
        {
            get
            {
                return usLength.ToString();
            }
        }

        public string Checksum
        {
            get
            {
                return string.Format("0x{0:x2}", sChecksum);
            }
        }

        public byte[] Data
        {
            get
            {
                return byUDPData;
            }
        }
    }
}
