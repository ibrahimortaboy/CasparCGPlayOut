using Svt.Caspar;
using Svt.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CasparPlayOut
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CasparDevice cd = new CasparDevice();
        //CasparItem ci = new CasparItem(string.Empty);
        //Channel ch;
        string playState = "", channel = "", layer = "", media = "", loop = "";


        public MainWindow()
        {
            InitializeComponent();


            cd.Connected += new EventHandler<NetworkEventArgs>(caspar_Connected);
            cd.Disconnected += new EventHandler<NetworkEventArgs>(caspar_Disconnected);
            cd.FailedConnect += new EventHandler<NetworkEventArgs>(caspar_FailedConnected);
            //caspar.DataRetrieved += new EventHandler<DataEventArgs>(caspar_DataRetrieved);
            //caspar.UpdatedDatafiles += new EventHandler<EventArgs>(caspar_UpdatedDataFiles);
            //caspar.Settings.Hostname = txb_server.Text;
            //caspar.Settings.Port = Convert.ToInt32(txb_port.Text);
            //cd.Settings.AutoConnect = false;
            //caspar.Connect();
            //caspar.Connect(txb_server.Text, int.Parse(txb_port.Text));

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bt_connect.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }


        void caspar_Connected(object sender, NetworkEventArgs e)
        {
            cd.RefreshMediafiles();
            cd.RefreshDatalist();

            //MessageBox.Show("Caspar AMCP Client Connected");


        }
        void caspar_Disconnected(object sender, NetworkEventArgs e)
        {
            //MessageBox.Show("Caspar AMCP Client Disconnected");
        }
        void caspar_FailedConnected(object sender, NetworkEventArgs e)
        {
            //MessageBox.Show("Caspar AMCP Client Failed Connected");
        }


        private void bt_connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bt_connect.IsEnabled = false;

                if (!cd.IsConnected)
                {
                    cd.Connect(txb_server.Text, int.Parse(txb_port.Text));

                    bt_connect.IsEnabled = true;
                    bt_connect.Content = "Disconnect";
                    elp_connect.Fill = new SolidColorBrush(Colors.LightGreen);
                    sp_connect.IsEnabled = false;
                    gr_control.IsEnabled = true;

                    cd_read_version_channel_media();
                }
                else
                {
                    cd.Disconnect();

                    bt_connect.IsEnabled = true;
                    bt_connect.Content = "Connect";
                    elp_connect.Fill = new SolidColorBrush(Colors.White);
                    sp_connect.IsEnabled = true;
                    gr_control.IsEnabled = false;

                    cd_cleaar_version_channel_media();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "bt_connect_Click");
            }
        }
        private void cd_read_version_channel_media()
        {
            try
            {
                // cd version read.
                do
                {
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    txb_version.Text = cd.Version.ToString();
                }
                while (cd.Version.ToString().ToLower() == "unknown");

                // cd channel read.
                do
                {
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    for (int i = 0; i < cd.Channels.Count; i++)
                    {
                        cmb_channel.Items.Add((i + 1).ToString());
                    }
                    if (cmb_channel.HasItems)
                        cmb_channel.SelectedIndex = 0;
                }
                while (cd.Channels.Count <= 0);

                // cd media read.
                txb_videoCount.Text = cd.Mediafiles.Count.ToString();

                Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();

                lbx_video.ItemsSource = cd.Mediafiles;
                //lbx_video.ItemsSource = MyList;
                //lbx_video.Items.Add(MyList.ToArray());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "cd_read_channel_file");
            }
        }
        private void cd_cleaar_version_channel_media()
        {
            try
            {
                txb_version.Text = "";
                cmb_channel.Items.Clear();
                txb_videoCount.Text = "";
                lbx_video.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "cd_cleaar_channel_file");
            }
        }
        private void bt_control_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bt = sender as System.Windows.Controls.Button;

                playState = bt.Content.ToString().ToLower().Trim();
                channel = cmb_channel.SelectedItem.ToString();
                layer = cmb_layer.Text;
                if (lbx_video.SelectedIndex != -1)
                    media = lbx_video.SelectedItem.ToString();
                else
                    media = "";
                loop = "";

                bt_pause.Content = "Pause";
                bt_pause.IsEnabled = false;

                if (playState == "load")
                {
                    if (media != "")
                    {
                        cd_send_string(playState, channel, layer, media, loop);
                    }
                }
                else if (playState == "play")
                {
                    if (media != "")
                    {
                        loop = "loop";
                        cd_send_string(playState, channel, layer, media, loop);
                        bt_pause.IsEnabled = true;
                    }
                }
                else if (playState == "pause")
                {
                    cd_send_string(playState, channel, layer, media, loop);
                    bt.Content = "Resume";
                    bt_pause.IsEnabled = true;
                }
                else if (playState == "resume")
                {
                    cd_send_string(playState, channel, layer, media, loop);
                    bt_pause.IsEnabled = true;
                }
                else if (playState == "stop")
                {
                    cd_send_string(playState, channel, layer, media, loop);
                }
                else if (playState == "clear")
                {
                    layer = "";
                    cd_send_string(playState, channel, layer, media, loop);
                }


                ////ci = new CasparItem(-1, "xsmall");
                //ci.Clipname = "xsmall";
                //ci.VideoLayer = 10;
                //ci.Transition.Duration = 10;
                //ci.Loop = true;

                //ch = cd.Channels[0];
                //ch.Load(ci);
                //ch.Play(10);

                //cd.SendString("play 1-10 xsmall loop");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "bt_control_Click");
            }
        }
        private void cd_send_string(string action, string channel, string layer, string media, string loop)
        {
            if (layer != "")
                layer = "-" + layer;
            if (media != "")
            {
                //media = "\"" + media + "\"";
                media = media.Replace(@"\", "/");
            }

            try
            {
                cd.SendString(action + " " + channel + layer + " " + media + " " + loop);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "cd_send_string");
            }
            //MessageBox.Show(action + " " + channel + layer + " " + media + " " + loop);
        }





    }
}
