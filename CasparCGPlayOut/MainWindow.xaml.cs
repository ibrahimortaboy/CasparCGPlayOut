using Casparcg.Core.Device;
using Casparcg.Core.Network;
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
            string tc = "00:00:00:126";
            var ii = tc.IndexOf(':');
            ii = tc.IndexOf(':', ii + 1);
            ii = tc.IndexOf(':', ii + 1);
            tc = tc.Substring(0,ii);
            //System.Windows.MessageBox.Show(tc);



            bt_connect.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        }


        void caspar_Connected(object sender, NetworkEventArgs e)
        {
            cd.RefreshMediafiles();
            //cd.RefreshDatalist();
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
                    cd_connected();
                }
                else
                {
                    cd.Disconnect();
                    cd_disconnected();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "bt_connect_Click");
            }
        }
        private void cd_connected()
        {
            try
            {
                for (int x = 0; x < 50; x++)
                {
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    if (cd.IsConnected)
                        break;
                }

                if (cd.IsConnected)
                {
                    sp_connect.IsEnabled = false;
                    gr_control.IsEnabled = true;

                    bt_connect.IsEnabled = true;
                    bt_connect.Content = "Disconnect";
                    elp_connect.Fill = new SolidColorBrush(Colors.LightGreen);

                    txb_version.Text = cd.Version.ToString();
                    lb_mediaCount.Content = cd.Mediafiles.Count.ToString();

                    for (int i = 0; i < cd.Channels.Count; i++)
                    {
                        cmb_channel.Items.Add((i + 1).ToString());
                    }
                    if (cmb_channel.HasItems)
                        cmb_channel.SelectedIndex = 0;

                    lv_video.ItemsSource = cd.Mediafiles;
                    //cd.RefreshMediafiles();
                    ////lv_video.ItemsSource = MyList;
                    ////lv_video.Items.Add(MyList.ToArray());

                    //MessageBox.Show(cd.Mediafiles[66].FullName.ToString() +"\n"+ cd.Mediafiles[66].Timecode.ToString());
                }
                else
                {
                    System.Windows.MessageBox.Show("Caspar AMCP Client Failed Connected.");
                    bt_connect.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "cg_connected");
            }
        }
        private void cd_disconnected()
        {
            try
            {
                sp_connect.IsEnabled = true;
                gr_control.IsEnabled = false;

                bt_connect.IsEnabled = true;
                bt_connect.Content = "Connect";
                elp_connect.Fill = new SolidColorBrush(Colors.White);

                txb_version.Text = "";
                cmb_channel.Items.Clear();
                lb_mediaCount.Content = "";
                lv_video.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "cd_disconnected");
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
                if (lv_video.SelectedIndex != -1)
                    media = lv_video.SelectedItem.ToString();
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
                    //cd.Channels[Convert.ToInt32(channel)].Stop(Convert.ToInt32(layer));
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
