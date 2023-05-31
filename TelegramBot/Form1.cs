using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;

namespace TelegramBot
{
    public partial class Form1 : Form
    {
        //string api_id = "26249405";  
        //string api_hash = "ce6bddc0f5a1d21d39ae5ca8e61623ba";

        string api_id = "26173383";
        string api_hash = "b4b2b8cf838d5ab4e2a073f522c92a6a";
        bool bStop = false;
        private WTelegram.Client _client;
        string defaultFilePath="";
        string logText = "";
        string logTextSuccess = "";
        string logTextFailed = "";
        string logTextAdded = "";
        List<string> listlogTextSuccess = new List<string>();
        List<string> listlogTextFailed = new List<string>();
        List<string> listlogTextAdded = new List<string>();
        List<string> listlogText = new List<string>();

        List<string> logTexts = new List<string>();
        List<TextBox> textBoxListLogs = new List<TextBox>();
        ProgressForm m_progressForm = new ProgressForm();       

        public Form1()
        {
            InitializeComponent();
            LoadApplicationInformation();
            this.comboBoxPhone.Text = global::TelegramBot.Properties.Settings.Default.phone_number;
            textBoxListLogs.Add(textBoxLog);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _client?.Dispose();
            Properties.Settings.Default.Save();
            SaveApplicationInformation();
        }

        private void LoadApplicationInformation()
        {
            try
            {
                string file_path = "App.cfg";
                string[] strs = File.ReadAllLines(file_path);

                textBoxContactFilePath.Text = strs[0];
                string[] s = strs[1].Split(' ');
                textBoxDelay.Text = s[0];
                if (s.Length > 1) textBoxDelayRange.Text = s[1];
                else textBoxDelayRange.Text = s[0];
                LabelDupes.Text = strs[2];
                 
                string[] item_strs = File.ReadAllLines(textBoxContactFilePath.Text);

                for (int i = 0; i < item_strs.Length; i++)
                {
                    listBoxFileContacts.Items.Add(item_strs[i]);
                }
                labelFileContactCount.Text = item_strs.Length.ToString();
               
                if (strs[3] == "1") checkBoxDontAppend.Checked = true;
                else checkBoxDontAppend.Checked = false;
                if (strs.Length > 4)
                    defaultFilePath = strs[4];
                if (strs.Length > 5)
                    badContactsFile.Text = strs[5];
                if (strs.Length > 6)
                    goodContactsFile.Text = strs[6];
                if (strs.Length > 7)
                    textBoxFrom.Text = strs[7];
                if (strs.Length > 8)
                    textBoxTo.Text = strs[8];
                if (strs.Length > 9)
                {
                    s = strs[9].Split(' ');
                    foreach(string ss in s)
                    {
                        comboBoxPhone.Items.Add(ss);
                    }
                }
                if (strs.Length > 10)
                {
                    textBoxGoodNumberPath.Text = strs[10];
                }
                if (strs.Length > 11)
                {
                    s = strs[11].Split(' ');
                    if (s.Length > 0) textBoxAddUser.Text = s[0];
                    if (s.Length > 1) textBoxAddPhone.Text = s[1];
                }
            }
            catch
            {
                textBoxContactFilePath.Text = "";
                listBoxFileContacts.Items.Clear();
                labelFileContactCount.Text = "0";
            }
            if (File.Exists(goodNumberpath))
            {
                string []s = File.ReadAllLines(goodNumberpath);
                listBoxGoodNumber.Items.AddRange(s);
            }
            if (File.Exists(textBoxGoodNumberPath.Text))
            {

            }
        }

        private void SaveApplicationInformation()
        {
            string file_path = "App.cfg";            
            File.WriteAllText(file_path, textBoxContactFilePath.Text + "\r\n");
            File.AppendAllText(file_path, textBoxDelay.Text + ' ' + textBoxDelayRange.Text + "\r\n");
            File.AppendAllText(file_path, LabelDupes.Text + "\r\n");
            if (checkBoxDontAppend.Checked)
                File.AppendAllText(file_path,"1\r\n");
            else
                File.AppendAllText(file_path, "0\r\n");
            File.AppendAllText(file_path, $"{defaultFilePath}\r\n"); 
            File.AppendAllText(file_path, $"{badContactsFile.Text}\r\n");
            File.AppendAllText(file_path, $"{goodContactsFile.Text}\r\n");
            File.AppendAllText(file_path, $"{textBoxFrom.Text}\r\n");
            File.AppendAllText(file_path, $"{textBoxTo.Text}\r\n");
            string s = comboBoxPhone.Items[0].ToString();
            for (int i = 0; i < comboBoxPhone.Items.Count; i++)
            {
                if (s.Contains(comboBoxPhone.Items[i].ToString())) continue;
                s += " " + comboBoxPhone.Items[i];
            }
            File.AppendAllText(file_path, $"{s}\r\n");
            File.AppendAllText(file_path, $"{textBoxGoodNumberPath.Text}\r\n");
            File.AppendAllText(file_path, $"{textBoxAddUser.Text} {textBoxAddPhone.Text}\r\n");
//            MessageBox.Show($"{textBoxAddUser.Text} {textBoxAddPhone.Text}\r\n");

        }

        private async void buttonLogin_ClickAsync(object sender, EventArgs e)
        {
            var SessionPath = Path.GetTempFileName();
            buttonLogin.Enabled = false;
            if (File.Exists("c:\\WTelegram.session"))
            {
                try
                {
                    File.Copy("c:\\WTelegram.session", "c:\\WTelegram.backup", true);
                }
                catch (Exception Ex)
                {

                }
            }
            if (File.Exists("c:\\WTelegram.backup"))
                File.Copy("c:\\WTelegram.backup", SessionPath,true);
            if (_client == null)
            {
                try
                {
                    _client = new WTelegram.Client(int.Parse(api_id), api_hash);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    _client = new WTelegram.Client(int.Parse(api_id), api_hash, SessionPath);
                }
            }
            try
            {
                await DoLogin(comboBoxPhone.Text);
            }
            catch(Exception ee)
            {
                _client.Dispose();
                _client = null;      
                MessageBox.Show(ee.Message);
                buttonLogin.Enabled = true;
                return;
            }
            if (comboBoxPhone.Items.Contains(comboBoxPhone.Text) == false)
                comboBoxPhone.Items.Add(comboBoxPhone.Text);
            comboBoxPhone.Enabled = false;
        }

        private async void buttonSendCode_ClickAsync(object sender, EventArgs e)
        {
            labelCode.Visible = textBoxCode.Visible = buttonSendCode.Visible = false;
            await DoLogin(textBoxCode.Text);
        }

        private async Task DoLogin(string loginInfo)
        {
            MessageBox.Show(loginInfo);
            string what = await _client.Login(loginInfo);
            if (what != null)
            {
                labelCode.Visible = textBoxCode.Visible = buttonSendCode.Visible = true;
                textBoxCode.Focus();
                AppendLogText($"From {loginInfo} a {what} is required...\r\n\r\n");
                MessageBox.Show($"A {what} is required...");
            }
            else
            {
                AppendLogText($"logged in to {loginInfo} successfully.\r\n");
                btnLogout.Enabled = true;

                // MessageBox.Show("Login Success!!!");

                await GetAllContactsAsync();
                await GetAllChannelAndGroupListAsync();
            }
        }

        private async Task GetAllContactsAsync()
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }
            
            var Contacts = await _client.Contacts_GetContacts();

            listBoxContact.Items.Clear();

            StatusLabel.Text = "Loading contacts...";
            statusStrip.Refresh();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = Contacts.users.Count;
            ProgressBar.Visible = true;
            buttonRefresh.Enabled = false;
            buttonContactBrowser.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            buttonStartResolving.Enabled = false;

            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;

            for (int i = 0; i < Contacts.users.Count; i++)
            {
                ProgressBar.Value = i;
                if (i>0) 
                    ProgressBar.Value = i - 1;
                User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                string contact_name = user.username;
                if (contact_name == null || contact_name == "") continue;
                listBoxContact.Items.Add(contact_name);
            }
            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = "Ready";
            statusStrip.Refresh();
            labelContactCount.Text = listBoxContact.Items.Count.ToString();
        }

        private async Task GetAllChannelAndGroupListAsync(bool loadMemberCount=false)
        {
            labelChannelCount.Text = "0";
            labelGroupCount.Text = "0";
            labelMemberCount.Text = "0";

            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            listBoxChannel.Items.Clear();
            comboBoxChannel.Items.Clear();
            listBoxGroup.Items.Clear();
            comboBoxGroup.Items.Clear();
            listBoxMember.Items.Clear();
            textBoxSearchMember.Text = "";

            var dialogs = await _client.Messages_GetAllDialogs();

            StatusLabel.Text = "Loading groups && channels...";
            statusStrip.Refresh();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = dialogs.dialogs.Count();
            ProgressBar.Visible = true;
            buttonRefresh.Enabled = false;
            buttonContactBrowser.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            buttonStartResolving.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;
            ProgressBar.Value = 0;
            ProgressBar.Step = 1;
            int Counter = 0;
            foreach (Dialog dialog in dialogs.dialogs)
            {
                Counter++;
                ProgressBar.Value = Counter;
                ProgressBar.Value = Counter-1;
                // ProgressBar.PerformStep();
                
                var peer = dialogs.UserOrChat(dialog);
                
                string obj_string;
                if (peer.ToString() != null)
                    obj_string = peer.ToString()?.ToLower();
                else
                    continue;

                if (peer is User || obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                if (obj_string.IndexOf("channel") == 0)
                {
                    Entities.Channel channel = new Entities.Channel();
                    channel.Name = ((ChatBase)peer).Title;
                    channel.ID = ((ChatBase)peer).ID;

                    // var participants = await _client.Channels_GetAllParticipants((Channel)peer);

                    listBoxChannel.Items.Add(channel);
                    comboBoxChannel.Items.Add(channel.Name);
                }
                else
                {
                    Entities.Group group = new Entities.Group();
                    group.Name = ((ChatBase)peer).Title;
                    group.ID = ((ChatBase)peer).ID;

                    if (loadMemberCount)
                    {
                        var participants = await _client.Channels_GetAllParticipants((Channel)peer);
                        listBoxGroup.Items.Add(group + "(" + participants.users.Count + " members)");
                    }
                    else 
                        listBoxGroup.Items.Add(group);
                    comboBoxGroup.Items.Add(((ChatBase)peer).Title);
                }
            }

            StatusLabel.Text = "Ready";
            statusStrip.Refresh();
            ProgressBar.Visible= false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;

            labelChannelCount.Text = listBoxChannel.Items.Count.ToString();
            labelGroupCount.Text = listBoxGroup.Items.Count.ToString();
        }

        
        private async void buttonRefresh_ClickAsync(object sender, EventArgs e)
        {
            await GetAllContactsAsync();
            await GetAllChannelAndGroupListAsync();
        }

        private async void buttonCreateChannel_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (textBoxNewChannel.Text == "")
            {
                MessageBox.Show("Please Input New channel name.");
                return;
            }

            var res = await _client.Channels_CreateChannel(textBoxNewChannel.Text, textBoxNewChannel.Text);
            string log_text = textBoxNewChannel.Text;
            AppendLogText($"Created Channel {log_text}.\r\n\r\n");
            await GetAllChannelAndGroupListAsync ();
        }

        private async void buttonCreateGroup_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }
            
            if (textBoxNewGroup.Text == "")
            {
                MessageBox.Show("Please Input New Group name.");
                return;
            }

            await _client.Messages_CreateChat(null, textBoxNewGroup.Text);
            string log_text = textBoxNewGroup.Text;
            AppendLogText($"Created Group {log_text}.\r\n\r\n");
            await GetAllChannelAndGroupListAsync ();
        }

        private async void buttonAddUserChannel_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (textBoxAddUser1.Text == "")
            {
                MessageBox.Show("Please input user name");
                return;
            }

            if (comboBoxChannel.SelectedIndex == -1)
            {
                MessageBox.Show("Please select channel combo");
                return;
            }

            bool bExist = false;

            string phonenumber = textBoxAddUser1.Text.Replace(" ", "");
            phonenumber = phonenumber.Replace("+", "");

            User selectedUser = null;

            var Contacts = await _client.Contacts_GetContacts();

            for (int i = 0; i < Contacts.users.Count; i++)
            {
                User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                string contact_name = user.username;
                if (contact_name == null || contact_name == "") continue;

                string user_phone = user.phone?.Replace(" ", "");
                user_phone = user.phone?.Replace("+", "");
                string username = contact_name?.ToLower().Replace(" ", "");
                string inputname = textBoxAddUser1.Text?.ToLower().Replace(" ", "");

                if (username == inputname || phonenumber == user_phone)
                {
                    selectedUser = user;
                    bExist = true;
                    break;
                }
            }

            if (!bExist)
            {
                MessageBox.Show("Please input correct user name");
                return;
            }

            Channel selectedChannel = null;
            bExist = false;
            int nIndex = 0;

            var dialogs = await _client.Messages_GetAllDialogs(null);
            foreach (Dialog dialog in dialogs.dialogs)
            {
                var peer = dialogs.UserOrChat(dialog);
                string obj_string = peer.ToString().ToLower();
                if (peer is User || obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                if (obj_string.IndexOf("channel") == 0)
                {
                    if (nIndex == comboBoxChannel.SelectedIndex)
                    {
                        selectedChannel = (Channel)peer;
                        bExist = true;
                        break;
                    }
                    nIndex++;
                }
            }

            if (!bExist)
            {
                MessageBox.Show("Please select Channel combo");
                return;
            }

            try
            {
                await _client.AddChatUser(selectedChannel, selectedUser);
                string user_text = textBoxAddUser1.Text;
                string channel_text = comboBoxChannel.Text;
                AppendLogText($"Added user {user_text} to channel {channel_text}.\r\n\r\n");
                AppendLogSuccessAddText($"Added user {user_text} to channel {channel_text}.");
                MessageBox.Show("Add user to Channel Success!!!");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private async void buttonAddUserGroup_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (textBoxAddUser2.Text == "")
            {
                MessageBox.Show("Please input user name");
                return;
            }

            if (comboBoxGroup.SelectedIndex == -1)
            {
                MessageBox.Show("Please select group combo");
                return;
            }

            bool bExist = false;

            string phonenumber = textBoxAddUser2.Text.Replace(" ", "");
            phonenumber = phonenumber.Replace("+", "");

            User selectedUser = null;

            var Contacts = await _client.Contacts_GetContacts();

            for (int i = 0; i < Contacts.users.Count; i++)
            {
                User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                string contact_name = user.username;
                if (contact_name == null || contact_name == "") continue;

                string user_phone = user.phone?.Replace(" ", "");
                user_phone = user.phone?.Replace("+", "");
                string username = contact_name?.ToLower().Replace(" ", "");
                string inputname = textBoxAddUser2.Text?.ToLower().Replace(" ", "");

                if (username == inputname || phonenumber == user_phone)
                {
                    selectedUser = user;
                    bExist = true;
                    break;
                }
            }

            if (!bExist)
            {
                MessageBox.Show("Please input correct user name");
                return;
            }

            ChatBase selectedChat = null;
            bExist = false;
            int nIndex = 0;

            var dialogs = await _client.Messages_GetAllDialogs(null);
            foreach (Dialog dialog in dialogs.dialogs)
            {
                var peer = dialogs.UserOrChat(dialog);
                string obj_string = peer.ToString().ToLower();
                if (peer is User || obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                if (!(obj_string.IndexOf("channel") == 0))
                {
                    if (nIndex == comboBoxGroup.SelectedIndex)
                    {
                        bExist = true;
                        selectedChat = (ChatBase)peer;
                        break;
                    }
                    nIndex++;
                }
            }

            if (!bExist)
            {
                MessageBox.Show("Please select Group combo");
                return;
            }

            try
            {
                await _client.AddChatUser(selectedChat, selectedUser);
                string user_text = textBoxAddUser2.Text;
                string channel_text = comboBoxGroup.Text;
                AppendLogText($"Added user {user_text} to Group {channel_text}.\r\n\r\n");
                AppendLogSuccessAddText($"Added user {user_text} to Group {channel_text}.");
                MessageBox.Show("Add user to Group Success!!!");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private async void buttonResolveContact_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (radioButtonUser.Checked && textBoxAddUser.Text == "")
            {
                MessageBox.Show("Please input username!!!");
                return;
            }

            if (radioButtonPhone.Checked && textBoxAddPhone.Text == "")
            {
                MessageBox.Show("Please input phone number!!!");
                return;
            }

            try
            {
                if (radioButtonUser.Checked)
                {
                    string user_text = textBoxAddUser.Text;
                    AppendLogText($"\r\nResolving user {user_text} : {DateTime.Now} \r\n");

                    Contacts_ResolvedPeer user = await _client.Contacts_ResolveUsername(textBoxAddUser.Text);
                    AppendLogText($"{user_text} exist \r\n\r\n");
                    AppendLogSuccessText($"{user_text} exist");
                }
                else if (radioButtonPhone.Checked)
                {
                    string phone_text = textBoxAddPhone.Text;
                    AppendLogText($"\r\nResolving user {phone_text} : {DateTime.Now} \r\n");
                    Contacts_ResolvedPeer user = await _client.Contacts_ResolvePhone(textBoxAddPhone.Text);
                    AppendLogText($"{phone_text} exist \r\n\r\n");
                    AppendLogSuccessText($"{phone_text} exist.");
                }
            }
            catch
            {
                AppendLogText("The user does not exist.!!!");
                return;
            }
            AppendLogText("The user exists, but not added to contact list");
            await GetAllContactsAsync();

        }
        private async void buttonAddUserContact_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (radioButtonUser.Checked && textBoxAddUser.Text == "")
            {
                MessageBox.Show("Please input username!!!");
                return;
            }

            if (radioButtonPhone.Checked && textBoxAddPhone.Text == "")
            {
                MessageBox.Show("Please input phone number!!!");
                return;
            }

            try
            {
                if (radioButtonUser.Checked)
                {
                    Contacts_ResolvedPeer user = await _client.Contacts_ResolveUsername(textBoxAddUser.Text);
                    await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                    string user_text = textBoxAddUser.Text;
                    AppendLogText($"Added user {user_text} : {DateTime.Now} \r\n\r\n");
                    AppendLogSuccessAddText($"Added user {user_text}.");
                }
                else if (radioButtonPhone.Checked)
                {
                    Contacts_ResolvedPeer user = await _client.Contacts_ResolvePhone(textBoxAddPhone.Text);
                    await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                    string phone_text = textBoxAddPhone.Text;
                    AppendLogText($"Added phone number {phone_text} : {DateTime.Now} \r\n\r\n");
                    AppendLogSuccessAddText($"Added phone number {phone_text}.");
                }
            }
            catch
            {
                AppendLogText("The user does not exist.!!!");
                return;
            }

            await GetAllContactsAsync();
            AppendLogText("Seccess add the user to contact!!!");
        }

        private void buttonContactBrowser_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select file.";
            openFileDialog.InitialDirectory = defaultFilePath;
            openFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = openFileDialog.ShowDialog();
            int DupesFound = 0;

            if (dialogResult == DialogResult.OK)
            {
                textBoxContactFilePath.Text = openFileDialog.FileName;
                listBoxFileContacts.Items.Clear();

                string[] item_strs = File.ReadAllLines(openFileDialog.FileName);

                ProgressBar.Visible = true;
                buttonRefresh.Enabled = false;
                buttonContactBrowser.Enabled = false;
                buttonStartFileAdd.Enabled = false;
                buttonStartResolving.Enabled = false;
                menu3.Enabled = false;
                mnuSaveMembersToFile.Enabled = false;
                ProgressBar.Minimum = 0;
                ProgressBar.Maximum = item_strs.Length;
                StatusLabel.Text = $"Adding records...";
                statusStrip.Refresh();
                for (int i = 0; i < item_strs.Length; i++)
                {
                    ProgressBar.Value = i;
                    if (i>0) ProgressBar.Value = i-1;
                    if (listBoxContact.Items.Contains(item_strs[i]))
                        DupesFound++;
                    listBoxFileContacts.Items.Add(item_strs[i]);
                }
                ProgressBar.Visible = false;
                buttonRefresh.Enabled = true;
                buttonContactBrowser.Enabled = true;
                buttonStartFileAdd.Enabled = true;
                buttonStartResolving.Enabled = true;
                menu3.Enabled = true;
                mnuSaveMembersToFile.Enabled = true;
                StatusLabel.Text = $"Ready";
                statusStrip.Refresh();
                labelFileContactCount.Text = item_strs.Length.ToString();
                LabelDupes.Text = DupesFound.ToString();
                defaultFilePath = Path.GetDirectoryName(openFileDialog.FileName);
            }

            

            SaveApplicationInformation();
        }
        private async void buttonStartRangeAdd_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            long nFrom = 0, nTo = 0;
            try
            {
                nFrom = Convert.ToInt64(textBoxFrom.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct From phone number");
                textBoxFrom.Focus();
                return;
            }
            try
            {
                nTo = Convert.ToInt64(textBoxTo.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct To phone number");
                textBoxFrom.Focus();
                return;
            }
            int ncount = (int)(nTo - nFrom + 1);
//            MessageBox.Show("" + nFrom + " " + nTo + " " + ncount);
            if (ncount <= 0)
            {
                MessageBox.Show("Please enter correct phone number. To phone number must be bigger than From phone number");
            }
            buttonAddRange.Enabled = false;
            buttonRefresh.Enabled = false;
//            buttonContactBrowser.Enabled = false;
            buttonAddRange.Enabled = false;
//            buttonStartResolving.Enabled = false;
            buttonResolvingRange.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;

            int nSleep = 10000;
            try
            {
                nSleep = int.Parse(textBoxDelayRange.Text);
            }
            catch { }

            var Contacts = await _client.Contacts_GetContacts();

            List<string> m_PrevUserNames = new List<string>();
            List<string> m_PrevPhoneNumbers = new List<string>();
            if (checkBoxDontAppendRange.Checked)
            {
                for (int i = 0; i < Contacts.users.Count; i++)
                {
                    User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                    string contact_name = user.username;
                    if (contact_name != null && contact_name != "") m_PrevUserNames.Add(contact_name);
                    if (!(user.phone is null))
                    {
                        string phonenumber = user.phone.Replace(" ", "");
                        phonenumber = phonenumber.Replace("+", "");
                        m_PrevPhoneNumbers.Add(phonenumber);
                    }
                }
            }

            // PercentForm m_percentForm = new PercentForm();
            // m_percentForm.Show();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = ncount;
            ProgressBar.Visible = true;

            btnStop.Visible = true;
            bool contactAdded = false;
            for (int i = 0; i < ncount; i++)
            {
                if (bStop) break;
                contactAdded = false;
                string user_text = (nFrom + i).ToString();
                AppendLogText($"\r\n{i + 1}. Adding user {user_text} : {DateTime.Now} \r\n");

                if (checkBoxDontAppendRange.Checked)
                {
                    bool bExist = false;


                    for (int k = 0; k < m_PrevPhoneNumbers.Count; k++)
                    {
                        string prev_num = m_PrevPhoneNumbers[k].ToLower();
                        string cur_num = user_text.ToLower().Replace(" ", "").Replace("+", "");
                        if (prev_num == cur_num)
                        {
                            bExist = true;
                            break;
                        }
                    }

                    ProgressBar.Value = i + 1;
                    ProgressBar.Value = i;
                    StatusLabel.Text = $"Parsed {i + 1} contacts";
                    statusStrip.Refresh();
                    statusStrip.Refresh();
                    Application.DoEvents();

                    if (bExist)
                        continue;
                }

//                MessageBox.Show("" + (nFrom + i));
                Contacts_ResolvedPeer user;

                if (i % (int)(numericUpDownncheckRosolve.Value) == 0 && listBoxGoodNumber.Items.Count > 0 || (i == 0 && checkBoxResolveofGoodNumberStartOfThread.Checked)) 
                    foreach (string s in listBoxGoodNumber.Items)
                    {

                        string phone_text = s;
                        try
                        {
                            AppendLogText($"\r\nResolving user:{comboBoxPhone.Text} with good number, {phone_text} : {DateTime.Now} \r\n");
                            user = await _client.Contacts_ResolvePhone(phone_text);
                            AppendLogText($"Success \r\n\r\n");
                            AppendLogSuccessText($"Checking account Successed by good number:{phone_text}\r\n");
                            bStop = false;
                            break;
                        }
                        catch(Exception es)
                        {
                            AppendLogText($"Failed \r\nTrying to another good number\r\n");
                            AppendLogErrorText($"Checking account Failed by good number:{phone_text}: Error Code: {es.Message}");
                            if (checkBoxResolveStop.Checked)
                            {
                                bStop = true;
                            }
                        }

                    }
                if (bStop) return;
                try
                {
                    AppendLogText($"Trying to resolve contact - {user_text} \r\n");
                    user = await _client.Contacts_ResolvePhone(user_text);
                }
                catch (Exception ex)
                {
                    AppendLogText($"Error in resolving contact {user_text} : {ex.Message} \r\n");
                    AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}.");
                    if (badContactsFile.Text != "")
                        File.AppendAllText(badContactsFile.Text, user_text + "\n");
                    continue;
                }
                AppendLogText($"{user_text} exists\r\n");
                AppendLogSuccessText($"{user_text} exists.");
                if (goodContactsFile.Text != "")
                    File.AppendAllText(goodContactsFile.Text, user_text + "\n");

                string contact_name = user.User.username;
                if (contact_name == "" || contact_name == null) contact_name = user.User.first_name + " " + user.User.last_name;
                if (!(user.User.phone is null))
                {
                    string phonenumber = user.User.phone.Replace(" ", "");
                    phonenumber = phonenumber.Replace("+", "");

                    if (checkBoxDontAppendRange.Checked)
                    {

                        if (!m_PrevPhoneNumbers.Contains(phonenumber))
                        {
                            try
                            {
                                AppendLogText($"Adding the contact {user.User.first_name} {user.User.last_name} \r\n");
                                await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                            }
                            catch (Exception ex)
                            {
                                AppendLogText($"Error in adding contact {user_text} : {ex.Message} \r\n");
                                AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                                continue;
                            }
                            AppendLogText($"Contact {user_text} added successfully\r\n");
                            AppendLogSuccessAddText($"Contact {user_text} added successfully");
                            contactAdded = true;
                            listBoxContact.Items.Add(contact_name);
                            labelContactCount.Text = listBoxContact.Items.Count.ToString();
                            labelContactCount.Refresh();
                            m_PrevPhoneNumbers.Add(phonenumber);
                        }

                    }
                    else
                    {
                        try
                        {
                            await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                        }
                        catch (Exception ex)
                        {
                            AppendLogText($"Error in adding contact {user_text} : {ex.Message} \r\n");
                            AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                            continue;
                        }
                        AppendLogText($"Contact {user_text} added successfully\r\n");
                        AppendLogSuccessAddText($"Contact {user_text} added successfully.");
                        contactAdded = true;
                        listBoxContact.Items.Add(contact_name); contactAdded = true;
                        labelContactCount.Text = listBoxContact.Items.Count.ToString();
                        labelContactCount.Refresh();
                        m_PrevPhoneNumbers.Add(phonenumber);
                    }
                }

                /*catch (Exception ex)
                {
                    AppendLogText($"Error in Adding contact number {i} : {ex.Message}");
                }*/

                ProgressBar.Value = i + 1;
                ProgressBar.Value = i;
                StatusLabel.Text = $"Processed {i + 1} contacts";
                statusStrip.Refresh();

                if (i != (ncount - 1))
                {
                    string next_user_text = (nFrom + i).ToString();
                    AppendLogText($"Next task is adding user {next_user_text}\r\n\r\n");
                }
                else
                {
                    bStop = true;
                }

                DateTime m_StartTime = DateTime.Now;

                if (contactAdded)
                {
                    AppendLogText($"Waiting for {nSleep} sec before next\r\n");

                    while (true)
                    {
                        //AppendLogText($"Sleeping {nSleep} sec");
                        if (bStop) break;
                        DateTime m_CurrentTime = DateTime.Now;
                        if ((m_CurrentTime - m_StartTime).TotalSeconds > nSleep)
                            break;
                        else
                            Application.DoEvents();

                    }
                }
                Application.DoEvents();
            }

            AppendLogText($"\r\nAdding contact process completed\r\n");

            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonAddRange.Enabled = true;
            buttonResolvingRange.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = $"Ready";
            statusStrip.Refresh();
            btnStop.Visible = false;
            bStop = false;
            await GetAllContactsAsync();
            buttonAddRange.Enabled = true;
        }

        private async void buttonStartFileAdd_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (listBoxFileContacts.Items.Count < 1)
            {
                MessageBox.Show("Please load correct text file!!!");
                return;
            }
            buttonStartFileAdd.Enabled = false;
            buttonRefresh.Enabled = false;
            buttonContactBrowser.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            buttonStartResolving.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;

            int nSleep = 10000;
            try
            {
                nSleep = int.Parse(textBoxDelay.Text);
            }
            catch { }

            var Contacts = await _client.Contacts_GetContacts();

            List<string> m_PrevUserNames = new List<string>();
            List<string> m_PrevPhoneNumbers = new List<string>();
            if (checkBoxDontAppend.Checked)
            {
                for (int i = 0; i < Contacts.users.Count; i++)
                {
                    User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                    string contact_name = user.username;                   
                    if (contact_name != null && contact_name != "") m_PrevUserNames.Add(contact_name);                   
                    if (!(user.phone is null))
                    {
                        string phonenumber = user.phone.Replace(" ", "");
                        phonenumber = phonenumber.Replace("+", "");
                        m_PrevPhoneNumbers.Add(phonenumber);
                    }                    
                }
            }

            // PercentForm m_percentForm = new PercentForm();
            // m_percentForm.Show();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = listBoxFileContacts.Items.Count;
            ProgressBar.Visible = true;
            
            btnStop.Visible = true;
            bool contactAdded = false;
            for (int i = 0; i < listBoxFileContacts.Items.Count; i++)
            {
                if (bStop) break;
                contactAdded = false;
                string user_text = listBoxFileContacts.Items[i].ToString();
                AppendLogText($"\r\n{i+1}. Adding user {user_text} : {DateTime.Now} \r\n");

                if (checkBoxDontAppend.Checked)
                {
                    bool bExist = false;

                    if (radioButtonFileUser.Checked)
                    {
                        for (int k = 0; k < m_PrevUserNames.Count; k++)
                        {
                            string prev_name = m_PrevUserNames[k].ToLower();
                            string cur_name = user_text.ToLower();
                            if (prev_name == cur_name)
                            {
                                bExist = true;
                                break;
                            }
                        }
                    }
                    else if (radioButtonFilePhone.Checked)
                    {
                        for (int k = 0; k < m_PrevPhoneNumbers.Count; k++)
                        {
                            string prev_num = m_PrevPhoneNumbers[k].ToLower();
                            string cur_num = user_text.ToLower().Replace(" ", "").Replace("+", "");
                            if (prev_num == cur_num)
                            {
                                bExist = true;
                                break;
                            }
                        }
                    }

                    ProgressBar.Value = i+1;
                    ProgressBar.Value = i;
                    StatusLabel.Text = $"Parsed {i+1} contacts";
                    statusStrip.Refresh();
                    statusStrip.Refresh();
                    Application.DoEvents();

                    if (bExist)
                        continue;
                }


                Contacts_ResolvedPeer user;

                if (i % (int)(numericUpDownncheckRosolve.Value) == 0 && listBoxGoodNumber.Items.Count > 0 || (i == 0 && checkBoxResolveofGoodNumberStartOfThread.Checked))
                    foreach (string s in listBoxGoodNumber.Items)
                    {

                        string phone_text = s;
                        try
                        {
                            AppendLogText($"\r\nResolving user:{comboBoxPhone.Text} with good number, {phone_text} : {DateTime.Now} \r\n");
                            user = await _client.Contacts_ResolvePhone(phone_text);
                            AppendLogText($"Success \r\n\r\n");
                            AppendLogSuccessText($"Checking account Successed by good number:{phone_text}\r\n");
                            bStop = false;
                            break;
                        }
                        catch (Exception es)
                        {
                            AppendLogText($"Failed \r\nTrying to another good number\r\n");
                            AppendLogErrorText($"Checking account Failed by good number:{phone_text}: Error Code: {es.Message}");
                            if (checkBoxResolveStop.Checked)
                            {
                                bStop = true;
                            }
                        }

                    }
                if (bStop) return;

                if (radioButtonFileUser.Checked)
                {
                        
                    try
                    {
                        AppendLogText($"Trying to resolve contact - {user_text}\r\n");
                        user = await _client.Contacts_ResolveUsername(user_text);
                    }
                    catch (Exception ex)
                    {
                        AppendLogText($"Error in resolving contact {user_text} : {ex.Message}\r\n");
                        AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}.");
                        if (badContactsFile.Text != "")
                            File.AppendAllText(badContactsFile.Text, user_text + "\n");
                        continue;
                    }
                    AppendLogText($"{user_text} exists\r\n");
                    AppendLogSuccessText($"{user_text} exists.");
                    if (goodContactsFile.Text != "")
                        File.AppendAllText(goodContactsFile.Text, user_text + "\n");

                    string contact_name = user.User.username;
                    if (contact_name == "" || contact_name == null) contact_name = user.User.first_name + " " + user.User.last_name;


                    if (checkBoxDontAppend.Checked)
                    {
                        if (!m_PrevUserNames.Contains(contact_name))
                        {
                            try
                            {
                                AppendLogText($"Adding the contact {user.User.first_name} {user.User.last_name}\r\n");
                                await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                            }
                            catch (Exception ex)
                            {
                                AppendLogText($"Error in adding contact {user_text} : {ex.Message}\r\n");
                                AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                                continue;
                            }
                            AppendLogText($"Contact {user_text} added successfully\r\n");
                            AppendLogSuccessAddText($"Contact {user_text} added successfully.");

                            contactAdded = true;
                            listBoxContact.Items.Add(contact_name);
                            labelContactCount.Text = listBoxContact.Items.Count.ToString();
                            labelContactCount.Refresh();
                            m_PrevUserNames.Add(contact_name);
                        }
                    } else
                    {
                        try
                        {
                            AppendLogText($"Adding the contact {user.User.first_name} {user.User.last_name}\r\n");
                            await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                        }catch (Exception ex) 
                        {
                            AppendLogText($"Error in adding contact {user_text} : {ex.Message} \r\n");
                            AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                            continue;
                        }
                        AppendLogText($"Contact {user_text} added successfully\r\n");
                        AppendLogSuccessAddText($"Contact {user_text} added successfully.");
                        contactAdded = true;
                        listBoxContact.Items.Add(contact_name);
                        labelContactCount.Text = listBoxContact.Items.Count.ToString();
                        labelContactCount.Refresh();
                        m_PrevUserNames.Add(contact_name);
                    }
                }
                else if (radioButtonFilePhone.Checked)
                {
                    try
                    {
                        AppendLogText($"Trying to resolve contact - {user_text} \r\n");
                        user = await _client.Contacts_ResolvePhone(user_text);
                    }
                    catch(Exception ex) 
                    {
                        AppendLogText($"Error in resolving contact {user_text} : {ex.Message} \r\n");
                        AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}.");
                        if (badContactsFile.Text != "")
                            File.AppendAllText(badContactsFile.Text, user_text + "\n");
                        continue;
                    }
                    AppendLogText($"{user_text} exists\r\n");
                    AppendLogSuccessText($"{user_text} exists.");
                    if (goodContactsFile.Text != "")
                        File.AppendAllText(goodContactsFile.Text, user_text + "\n");

                    string contact_name = user.User.username;
                    if (contact_name == "" || contact_name == null) contact_name = user.User.first_name + " " + user.User.last_name;
                    if (!(user.User.phone is null))
                    {
                        string phonenumber = user.User.phone.Replace(" ", "");
                        phonenumber = phonenumber.Replace("+", "");

                        if (checkBoxDontAppend.Checked)
                        {

                            if (!m_PrevPhoneNumbers.Contains(phonenumber))
                            {
                                try
                                {
                                    AppendLogText($"Adding the contact {user.User.first_name} {user.User.last_name} \r\n"); 
                                    await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                                }
                                catch(Exception ex)
                                {
                                    AppendLogText($"Error in adding contact {user_text} : {ex.Message} \r\n");
                                    AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                                    continue;
                                }
                                AppendLogText($"Contact {user_text} added successfully\r\n");
                                AppendLogSuccessAddText($"Contact {user_text} added successfully.");
                                contactAdded = true;
                                listBoxContact.Items.Add(contact_name);
                                labelContactCount.Text = listBoxContact.Items.Count.ToString();
                                labelContactCount.Refresh();
                                m_PrevPhoneNumbers.Add(phonenumber);
                            }

                        }
                        else
                        {
                            try
                            {
                                await _client.Contacts_AddContact(user.User, user.User.first_name, user.User.last_name, user.User.phone);
                            }
                            catch (Exception ex)
                            {
                                AppendLogText($"Error in adding contact {user_text} : {ex.Message} \r\n");
                                AppendLogErrorText($"Error in adding contact {user_text} : {ex.Message}.");
                                continue;
                            }
                            AppendLogText($"Contact {user_text} added successfully\r\n");
                            AppendLogSuccessAddText($"Contact {user_text} added successfully.");
                            contactAdded = true; 
                            listBoxContact.Items.Add(contact_name); contactAdded = true;
                            labelContactCount.Text = listBoxContact.Items.Count.ToString();
                            labelContactCount.Refresh();
                            m_PrevPhoneNumbers.Add(phonenumber);
                        }
                    }
                }

                /*catch (Exception ex)
                {
                    AppendLogText($"Error in Adding contact number {i} : {ex.Message}");
                }*/

                ProgressBar.Value = i+1;
                ProgressBar.Value = i;
                StatusLabel.Text = $"Processed {i+1} contacts";
                statusStrip.Refresh();

                

                if (i != (listBoxFileContacts.Items.Count - 1))
                {
                    string next_user_text = listBoxFileContacts.Items[i + 1].ToString();
                    // AppendLogText($"Next task is adding user {next_user_text}\r\n\r\n");
                } else
                {
                    bStop = true;
                }

                DateTime m_StartTime = DateTime.Now;

                if (contactAdded)
                {
                    AppendLogText($"Waiting for {nSleep} sec before next\r\n");

                    while (true)
                    {
                        //AppendLogText($"Sleeping {nSleep} sec");
                        if (bStop) break;
                        DateTime m_CurrentTime = DateTime.Now;
                        if ((m_CurrentTime - m_StartTime).TotalSeconds > nSleep)
                            break;
                        else
                            Application.DoEvents();

                    }
                }
                Application.DoEvents();
            }
            AppendLogText($"\r\nAdding contact process completed\r\n");

            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = $"Ready";
            statusStrip.Refresh();
            btnStop.Visible = false;
            bStop = false;
            await GetAllContactsAsync();
            buttonStartFileAdd.Enabled = true;
        }

        private async void listBoxChannel_MouseDoubleClickAsync(object sender, MouseEventArgs e)
        {
            int index = this.listBoxChannel.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                labelMemberCount.Text = "0";
                
                listBoxMember.Items.Clear();

                Channel selectedChannel = null;
                bool bExist = false;
                int nIndex = 0;

                var dialogs = await _client.Messages_GetAllDialogs(null);
                foreach (Dialog dialog in dialogs.dialogs)
                {
                    var peer = dialogs.UserOrChat(dialog);
                    string obj_string = peer.ToString().ToLower();
                    if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                    if (obj_string.IndexOf("channel") == 0)
                    {
                        if (nIndex == index)
                        {
                            selectedChannel = (Channel)peer;
                            bExist = true;
                            break;
                        }
                        nIndex++;
                    }                    
                }

                if (!bExist)
                {
                    MessageBox.Show("Please select channel");
                    return;
                }

                m_progressForm.Show();

                try
                {
                    var users = (await _client.Channels_GetAllParticipants(selectedChannel)).users;
                    foreach (var user in users.Values)
                    {
                        try
                        {
                            string member_name = user.username;
                            if (member_name == null || member_name == "") continue;
                            listBoxMember.Items.Add(member_name);
                            Application.DoEvents();
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message, "Error");
                }

                m_progressForm.Hide();
                labelMemberCount.Text = listBoxMember.Items.Count.ToString();
                tabControl1.SelectedIndex = 3;
            }
        }

        private async void listBoxGroup_MouseDoubleClickAsync(object sender, MouseEventArgs e)
        {
            int index = this.listBoxGroup.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                labelMemberCount.Text = "0";
                
                listBoxMember.Items.Clear();

                ChatBase selectedChat = null;
                bool bExist = false;
                int nIndex = 0;

                var dialogs = await _client.Messages_GetAllDialogs(null);
                foreach (Dialog dialog in dialogs.dialogs)
                {
                    var peer = dialogs.UserOrChat(dialog);
                    string obj_string = peer.ToString().ToLower();
                    if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                    if (!(obj_string.IndexOf("channel") == 0)) 
                    {
                        if (nIndex == index)
                        {                            
                            selectedChat = (ChatBase)peer;
                            bExist = true;
                            break;
                        }
                        nIndex++;
                    }
                }

                if (!bExist)
                {
                    MessageBox.Show("Please select group");
                    return;
                }

                m_progressForm.Show();

                try
                {
                    var users = selectedChat is Channel channel
                            ? (await _client.Channels_GetAllParticipants(channel)).users
                            : (await _client.Messages_GetFullChat(selectedChat.ID)).users;
                    foreach (var user in users.Values)
                    {
                        try
                        {
                            string member_name = user.username;
                            if (member_name == null || member_name == "") continue;
                            listBoxMember.Items.Add(member_name);
                            Application.DoEvents();
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message, "Error");
                }

                m_progressForm.Hide();
                labelMemberCount.Text = listBoxMember.Items.Count.ToString();
                tabControl1.SelectedIndex = 3;
            }            
        }

        private void listBoxMember_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(listBoxMember.PointToScreen(e.Location));
            }
        }

        private void menu1_Click(object sender, EventArgs e)
        {
            string member_string = "";

            for (int i = 0; i < listBoxMember.Items.Count; i++)
            {
                member_string = member_string + listBoxMember.Items[i].ToString() + "\r\n";
            }

            Clipboard.SetText(member_string);
        }

        private void menu2_Click(object sender, EventArgs e)
        {
            string file_path = "";

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select save file.";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                file_path = saveFileDialog.FileName;
            }
            else return;

            string member_string = "";

            for (int i = 0; i < listBoxMember.Items.Count; i++)
            {
                member_string = member_string + listBoxMember.Items[i].ToString() + "\r\n";
            }

            File.WriteAllText(file_path, member_string);
        }

        private async void menu3_ClickAsync(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            string file_path = "";

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select save file.";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                file_path = saveFileDialog.FileName;
            }
            else return;

            // PercentForm m_percentForm = new PercentForm();
            // m_percentForm.Show();
            Application.DoEvents();

            int nTotalCount = 0;

            

            var dialogs = await _client.Messages_GetAllDialogs(null);
            

            foreach (Dialog dialog in dialogs.dialogs)
            {                
                var peer = dialogs.UserOrChat(dialog);
                if (peer.ToString() == null) continue;
                string obj_string = peer.ToString().ToLower();
                if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;
                
                if (!(obj_string.IndexOf("channel") == 0)) 
                {
                    nTotalCount++;
                }
            }

            if (nTotalCount == 0)
            {
                MessageBox.Show("There is no groups!!!");
                ProgressBar.Visible = false;
                buttonRefresh.Enabled = true;
                buttonContactBrowser.Enabled = true;
                buttonStartFileAdd.Enabled = true;
                buttonStartResolving.Enabled = true;
                menu3.Enabled = true;
                mnuSaveMembersToFile.Enabled = true;
                StatusLabel.Text = $"Ready";
                statusStrip.Refresh();
                btnStop.Visible = false;
                bStop = false;
                return;
            }

            int nCurrent = 0;
            
            bool bFirstLine = true;

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = dialogs.dialogs.Count();
            ProgressBar.Visible = true;
            buttonRefresh.Enabled = false;
            buttonContactBrowser.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            buttonStartResolving.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;
            btnStop.Visible = true;
            menu3.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            foreach (Dialog dialog in dialogs.dialogs)
            {
                var peer = dialogs.UserOrChat(dialog);
                if (peer.ToString() == null)
                {
                    nCurrent++; ProgressBar.Value = ProgressBar.Value + 1; continue;
                }
                string obj_string = peer.ToString().ToLower();
                if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0)
                {
                    nCurrent++; ProgressBar.Value = ProgressBar.Value + 1; continue;
                }
                var users = new Dictionary<long,User>();
                if (!(obj_string.IndexOf("channel") == 0))
                {
                    try
                    {
                        users = peer is Channel channel
                                        ? (await _client.Channels_GetAllParticipants(channel)).users
                                        : (await _client.Messages_GetFullChat(peer.ID)).users;
                    }
                    catch(Exception ex) {
                        AppendLogText($"Error in saving all : {ex.Message}\r\n");
                        nCurrent++; ProgressBar.Value = ProgressBar.Value + 1; continue;
                    }
                    foreach (var user in users.Values)
                    {
                        try
                        {
                            string member_name = user.username;
                            if (member_name == null || member_name == "") continue;

                            if (bFirstLine)
                            {
                                File.WriteAllText(file_path, member_name + "\r\n");
                                bFirstLine = false;
                            }
                            else
                                File.AppendAllText(file_path, member_name + "\r\n");
                        }
                        catch
                        {

                        }
                        Application.DoEvents();
                    }

                    

                    
                    nCurrent++;
                    Application.DoEvents();
                }

                ProgressBar.Value = ProgressBar.Value+1;
                StatusLabel.Text = $"Processing. Exported {nCurrent} groups/channels";
                statusStrip.Refresh();

                if (bStop)  
                    break;
            }

            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            menu3.Enabled = true;
            StatusLabel.Text = $"Ready";
            statusStrip.Refresh();
            btnStop.Visible = false;
            bStop = false;
        }

        private void AppendLogErrorText(string log_str)
        {
            log_str += $"{log_str} :{DateTime.Now} using the account number {comboBoxPhone.Text}\r\n";
            listlogTextFailed.Add(log_str);
            if (comboBoxShow.Text == "Show Bad Resolved") ShowLog(listlogTextFailed);
        }

        private void AppendLogSuccessText(string log_str)
        {
            log_str += $"{log_str} :{DateTime.Now} using the account number {comboBoxPhone.Text}\r\n";
            listlogTextSuccess.Add(log_str);
            if (comboBoxShow.Text == "Show Success Resoved") ShowLog(listlogTextSuccess);
        }
        private void AppendLogSuccessAddText(string log_str)
        {
            log_str += $"{log_str} :{DateTime.Now} using the account number {comboBoxPhone.Text}\r\n";
            listlogTextAdded.Add(log_str);
            if (comboBoxShow.Text == "Show added successfully") ShowLog(listlogTextAdded);
        }

        private void ShowLog(List<string> loglist)
        {
            int nMaxLogLines = 500;
            try
            {
                nMaxLogLines = int.Parse(textBoxLogMaxLines.Text);
            }
            catch { }
            textBoxLogMaxLines.Text = nMaxLogLines.ToString();

            if (loglist.Count % nMaxLogLines == 1)
            {
                textBoxLog.Text = "";
            }
            textBoxLog.Text += loglist.Last();
        }
        private void AppendLogText(string log_str)
        {
            log_str += $"{log_str} :{DateTime.Now} using the account number {comboBoxPhone.Text}\r\n";
            listlogText.Add(log_str);
            if (comboBoxShow.Text == "Show All")ShowLog(listlogText);
            Application.DoEvents();
        }

        private void textBoxSearchMember_TextChanged(object sender, EventArgs e)
        {
            bool bExist = false;
            for (int i = 0; i < listBoxMember.Items.Count; i++)
            {
                string item_str = listBoxMember.Items[i].ToString();
                if (textBoxSearchMember.Text != "" && item_str.Contains(textBoxSearchMember.Text))
                {
                    listBoxMember.SelectedIndex = i;
                    bExist = true;
                    break;
                }
            }

            if (!bExist) listBoxMember.SelectedIndex = -1;
        }

        private async void mnuSaveMembersToFile_Click(object sender, EventArgs e)
        {
            Control sourceControl = null;
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    sourceControl = owner.SourceControl;
                }
            }
            

            string file_path = "";

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select save file.";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                file_path = saveFileDialog.FileName;
            }
            else return;


            if (sourceControl.Name == "listBoxGroup")
            {
                ChatBase selectedChat = null;
                bool bExist = false;
                int nIndex = 0;

                var dialogs = await _client.Messages_GetAllDialogs(null);

                ProgressBar.Minimum = 0;
                ProgressBar.Visible = true;
                buttonRefresh.Enabled = false;
                buttonContactBrowser.Enabled = false;
                buttonStartFileAdd.Enabled = false;
                buttonStartResolving.Enabled = false;
                menu3.Enabled = false;
                mnuSaveMembersToFile.Enabled = false;
                ProgressBar.Maximum = dialogs.dialogs.Count();
                int Counter = 0;
                StatusLabel.Text = "Saving the list";
                statusStrip.Refresh();
                foreach (Dialog dialog in dialogs.dialogs)
                {
                    Counter++;
                    ProgressBar.Value = Counter;
                    if (Counter > 0) ProgressBar.Value = Counter-1;
                    var peer = dialogs.UserOrChat(dialog);
                    string obj_string = peer.ToString().ToLower();
                    if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                    if (!(obj_string.IndexOf("channel") == 0))
                    {
                        if (nIndex == listBoxGroup.SelectedIndex)
                        {
                            selectedChat = (ChatBase)peer;
                            bExist = true;
                            break;
                        }
                        nIndex++;
                    }
                }
                

                if (!bExist)
                {
                    ProgressBar.Visible = false;
                    buttonRefresh.Enabled = true;
                    buttonContactBrowser.Enabled = true;
                    buttonStartFileAdd.Enabled = true;
                    buttonStartResolving.Enabled = true;
                    menu3.Enabled = true;
                    mnuSaveMembersToFile.Enabled = true;
                    StatusLabel.Text = "Ready";
                    statusStrip.Refresh();
                    MessageBox.Show("Please select group");
                    return;
                }


                try
                {
                    var users = selectedChat is Channel channel
                            ? (await _client.Channels_GetAllParticipants(channel)).users
                            : (await _client.Messages_GetFullChat(selectedChat.ID)).users;
                    File.WriteAllText(file_path, "");


                    ProgressBar.Minimum = 0;
                    ProgressBar.Visible = true;
                    buttonRefresh.Enabled = false;
                    buttonContactBrowser.Enabled = false;
                    buttonStartFileAdd.Enabled = false;
                    buttonStartResolving.Enabled = false;
                    menu3.Enabled = false;
                    mnuSaveMembersToFile.Enabled = false;
                    ProgressBar.Maximum = users.Values.Count();
                    Counter = 0;

                    foreach (var user in users.Values)
                    {
                        Counter++;
                        ProgressBar.Value = Counter;
                        if (Counter>0) ProgressBar.Value = Counter-1;
                        try
                        {
                            string member_name = user.username;
                            if (member_name == null || member_name == "") continue;
                            File.AppendAllText(file_path, member_name + "\n");
                            Application.DoEvents();
                        }
                        catch
                        {

                        }
                    }
                    StatusLabel.Text = "Ready";
                    statusStrip.Refresh();
                    ProgressBar.Visible = false;
                    buttonRefresh.Enabled = true;
                    buttonContactBrowser.Enabled = true;
                    buttonStartFileAdd.Enabled = true;
                    buttonStartResolving.Enabled = true;
                    menu3.Enabled = true;
                    mnuSaveMembersToFile.Enabled = true;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message, "Error");
                    return;
                }
            }  else
            {
                Channel selectedChannel = null;
                bool bExist = false;
                int nIndex = 0;

                var dialogs = await _client.Messages_GetAllDialogs(null);
                foreach (Dialog dialog in dialogs.dialogs)
                {
                    var peer = dialogs.UserOrChat(dialog);
                    string obj_string = peer.ToString().ToLower();
                    if (obj_string.IndexOf('@') == 0 || obj_string.IndexOf("telegram") == 0) continue;

                    if (obj_string.IndexOf("channel") == 0)
                    {
                        if (nIndex == listBoxChannel.SelectedIndex)
                        {
                            selectedChannel = (Channel)peer;
                            bExist = true;
                            break;
                        }
                        nIndex++;
                    }
                }

                if (!bExist)
                {
                    MessageBox.Show("Please select channel");
                    return;
                }

                try
                {
                    var users = (await _client.Channels_GetAllParticipants(selectedChannel)).users;
                    foreach (var user in users.Values)
                    {
                        try
                        {
                            string member_name = user.username;
                            if (member_name == null || member_name == "") continue;
                            listBoxMember.Items.Add(member_name);
                            Application.DoEvents();
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message, "Error");
                    return;
                }
            }

            MessageBox.Show("Saved list to the file", "Done");
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            bStop = true;
            btnStop.Visible = false;
            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = "Ready";
            statusStrip.Refresh();

        }

        private async void LoadGroupCount_Click(object sender, EventArgs e)
        {
            LoadGroupCount.Enabled = false;
            await GetAllChannelAndGroupListAsync(true);
            LoadGroupCount.Enabled = true;
        }

        private void btnBadContacts_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select file.";
            saveFileDialog.InitialDirectory = defaultFilePath;
            saveFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                badContactsFile.Text = saveFileDialog.FileName;
                defaultFilePath = Path.GetDirectoryName(saveFileDialog.FileName);
            }

            SaveApplicationInformation();
        }

        private void btnGoodContacts_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select file.";
            saveFileDialog.InitialDirectory = defaultFilePath;
            saveFileDialog.Filter = "Text files(*.txt)|*.txt";
            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                goodContactsFile.Text = saveFileDialog.FileName;
                defaultFilePath = Path.GetDirectoryName(saveFileDialog.FileName);
            }

            SaveApplicationInformation();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (_client!=null)
                _client.Auth_LogOut();
            _client=null;
            btnLogout.Enabled = false;
            buttonLogin.Enabled = true;
            listBoxContact.Items.Clear();
            comboBoxChannel.Items.Clear();
            comboBoxGroup.Items.Clear();
            listBoxChannel.Items.Clear();
            listBoxGroup.Items.Clear();
            listBoxMember.Items.Clear();
            labelContactCount.Text = "0";
            labelChannelCount.Text = "0";
            labelGroupCount.Text = "0";
            labelMemberCount.Text = "0";
            textBoxLog.Clear();
            comboBoxPhone.Enabled = true;

        }

        private async void buttonStartResolving_Click(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            if (listBoxFileContacts.Items.Count < 1)
            {
                MessageBox.Show("Please load correct text file!!!");
                return;
            }
            bool Resolved = false;
            buttonStartFileAdd.Enabled = false;
            buttonRefresh.Enabled = false;
            buttonContactBrowser.Enabled = false;
            buttonStartFileAdd.Enabled = false;
            buttonStartResolving.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false; 
            int nSleep = 10000;
            try
            {
                nSleep = int.Parse(textBoxDelay.Text);
            }
            catch { }

            
            var Contacts = await _client.Contacts_GetContacts();

            List<string> m_PrevUserNames = new List<string>();
            List<string> m_PrevPhoneNumbers = new List<string>();
            if (checkBoxDontAppend.Checked)
            {
                for (int i = 0; i < Contacts.users.Count; i++)
                {
                    User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                    string contact_name = user.username;
                    if (contact_name != null && contact_name != "") m_PrevUserNames.Add(contact_name);
                    if (!(user.phone is null))
                    {
                        string phonenumber = user.phone.Replace(" ", "");
                        phonenumber = phonenumber.Replace("+", "");
                        m_PrevPhoneNumbers.Add(phonenumber);
                    }
                }
            }

            // PercentForm m_percentForm = new PercentForm();
            // m_percentForm.Show();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = listBoxFileContacts.Items.Count;
            ProgressBar.Visible = true;
            
            btnStop.Visible = true;
            bool contactAdded = false;
            for (int i = 0; i < listBoxFileContacts.Items.Count; i++)
            {
                if (bStop) break;
                contactAdded = false;
                string user_text = listBoxFileContacts.Items[i].ToString();
                AppendLogText($"\r\n{i + 1}. Resolving user {user_text} : {DateTime.Now} \r\n");

                if (checkBoxDontAppend.Checked)
                {
                    bool bExist = false;

                    if (radioButtonFileUser.Checked)
                    {
                        for (int k = 0; k < m_PrevUserNames.Count; k++)
                        {
                            string prev_name = m_PrevUserNames[k].ToLower();
                            string cur_name = user_text.ToLower();
                            if (prev_name == cur_name)
                            {
                                bExist = true;
                                break;
                            }
                        }
                    }
                    else if (radioButtonFilePhone.Checked)
                    {
                        for (int k = 0; k < m_PrevPhoneNumbers.Count; k++)
                        {
                            string prev_num = m_PrevPhoneNumbers[k].ToLower();
                            string cur_num = user_text.ToLower().Replace(" ", "").Replace("+", "");
                            if (prev_num == cur_num)
                            {
                                bExist = true;
                                break;
                            }
                        }
                    }

                    ProgressBar.Value = i + 1;
                    ProgressBar.Value = i;
                    StatusLabel.Text = $"Parsed {i + 1} contacts";
                    statusStrip.Refresh();
                    statusStrip.Refresh();
                    Application.DoEvents();

                    if (bExist)
                        continue;
                }


                Contacts_ResolvedPeer user;

                if (i % (int)(numericUpDownncheckRosolve.Value) == 0 && listBoxGoodNumber.Items.Count > 0 || (i == 0 && checkBoxResolveofGoodNumberStartOfThread.Checked))
                    foreach (string s in listBoxGoodNumber.Items)
                    {

                        string phone_text = s;
                        try
                        {
                            AppendLogText($"\r\nResolving user:{comboBoxPhone.Text} with good number, {phone_text} : {DateTime.Now} \r\n");
                            user = await _client.Contacts_ResolvePhone(phone_text);
                            AppendLogText($"Success \r\n\r\n");
                            AppendLogSuccessText($"Checking account Successed by good number:{phone_text}");
                            bStop = false;
                            break;
                        }
                        catch (Exception es)
                        {
                            AppendLogText($"Failed \r\nTrying to another good number\r\n");
                            AppendLogErrorText($"Checking account Failed by good number:{phone_text}: Error Code: {es.Message}");
                            if (checkBoxResolveStop.Checked)
                            {
                                bStop = true;
                            }
                        }

                    }
                if (bStop) return;

                if (radioButtonFileUser.Checked)
                {

                    try
                    {
                        AppendLogText($"Trying to resolve contact - {user_text}\r\n");
                        user = await _client.Contacts_ResolveUsername(user_text);
                    }
                    catch (Exception ex)
                    {
                        AppendLogText($"Error in resolving contact {user_text} : {ex.Message}\r\n");
                        AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}");
                        if (badContactsFile.Text != "")
                            File.AppendAllText(badContactsFile.Text, user_text + "\n");
                        
                        continue;
                    }
                    Resolved = true;
                    AppendLogText($"{user_text} exists\r\n");
                    AppendLogSuccessAddText($"{user_text} exists");


                    if (goodContactsFile.Text != "")
                        File.AppendAllText(goodContactsFile.Text, user_text + "\n");

                    
                }
                else if (radioButtonFilePhone.Checked)
                {
                    try
                    {
                        AppendLogText($"Trying to resolve contact - {user_text} \r\n");
                        user = await _client.Contacts_ResolvePhone(user_text);
                    }
                    catch (Exception ex)
                    {
                        AppendLogText($"Error in resolving contact {user_text} : {ex.Message} \r\n");
                        AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}");
                        if (badContactsFile.Text != "")
                            File.AppendAllText(badContactsFile.Text, user_text + "\n");
                        continue;
                    }
                    Resolved= true;
                    AppendLogText($"{user_text} exists\r\n");
                    AppendLogSuccessText($"{user_text} exists");

                    if (goodContactsFile.Text != "")
                        File.AppendAllText(goodContactsFile.Text, user_text + "\n");

                }

                /*catch (Exception ex)
                {
                    AppendLogText($"Error in Adding contact number {i} : {ex.Message}");
                }*/

                ProgressBar.Value = i + 1;
                ProgressBar.Value = i;
                StatusLabel.Text = $"Processed {i + 1} contacts";
                statusStrip.Refresh();



                if (i != (listBoxFileContacts.Items.Count - 1))
                {
                    string next_user_text = listBoxFileContacts.Items[i + 1].ToString();
                    // AppendLogText($"Next task is adding user {next_user_text}\r\n\r\n");
                }
                else
                {
                    bStop = true;
                }

                DateTime m_StartTime = DateTime.Now;

                if (Resolved)
                {
                    AppendLogText($"Waiting for {nSleep} sec before next\r\n");

                    while (true)
                    {
                        //AppendLogText($"Sleeping {nSleep} sec");
                        if (bStop) break;
                        DateTime m_CurrentTime = DateTime.Now;
                        if ((m_CurrentTime - m_StartTime).TotalSeconds > nSleep)
                            break;
                        else
                            Application.DoEvents();

                    }
                }

                Application.DoEvents();
            }
            AppendLogText($"\rResolving contact process completed\r\n");

            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonStartFileAdd.Enabled = true;
            buttonStartResolving.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = $"Ready";
            statusStrip.Refresh();
            btnStop.Visible = false;
            bStop = false;
            await GetAllContactsAsync();
            buttonStartFileAdd.Enabled = true;
        }

        private async void buttonResolvingRange_Click(object sender, EventArgs e)
        {
            if (_client == null || _client.User == null)
            {
                MessageBox.Show("You must login first.");
                return;
            }

            long nFrom = 0, nTo = 0;
            try
            {
                nFrom = Convert.ToInt64(textBoxFrom.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct From phone number");
                textBoxFrom.Focus();
                return;
            }
            try
            {
                nTo = Convert.ToInt64(textBoxTo.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct To phone number");
                textBoxFrom.Focus();
                return;
            }
            int ncount = (int)(nTo - nFrom + 1);
            //            MessageBox.Show("" + nFrom + " " + nTo + " " + ncount);
            if (ncount <= 0)
            {
                MessageBox.Show("Please enter correct phone number. To phone number must be bigger than From phone number");
            }
            buttonAddRange.Enabled = false;
            buttonRefresh.Enabled = false;
            //            buttonContactBrowser.Enabled = false;
            buttonAddRange.Enabled = false;
            //            buttonStartResolving.Enabled = false;
            buttonResolvingRange.Enabled = false;
            menu3.Enabled = false;
            mnuSaveMembersToFile.Enabled = false;

            int nSleep = 10000;
            try
            {
                nSleep = int.Parse(textBoxDelayRange.Text);
            }
            catch { }

            var Contacts = await _client.Contacts_GetContacts();

            List<string> m_PrevUserNames = new List<string>();
            List<string> m_PrevPhoneNumbers = new List<string>();
            if (checkBoxDontAppendRange.Checked)
            {
                for (int i = 0; i < Contacts.users.Count; i++)
                {
                    User user = Contacts.users[Contacts.users.Keys.ElementAt(i)];
                    string contact_name = user.username;
                    if (contact_name != null && contact_name != "") m_PrevUserNames.Add(contact_name);
                    if (!(user.phone is null))
                    {
                        string phonenumber = user.phone.Replace(" ", "");
                        phonenumber = phonenumber.Replace("+", "");
                        m_PrevPhoneNumbers.Add(phonenumber);
                    }
                }
            }

            // PercentForm m_percentForm = new PercentForm();
            // m_percentForm.Show();
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = ncount;
            ProgressBar.Visible = true;

            btnStop.Visible = true;
            bool Resolved = false;
            for (int i = 0; i < ncount; i++)
            {
                if (bStop) break;
                Resolved = false;
                string user_text = (nFrom + i).ToString();
//                AppendLogText($"\r\n{i + 1}. Adding user {user_text} : {DateTime.Now} \r\n");

                if (checkBoxDontAppendRange.Checked)
                {
                    bool bExist = false;

                    for (int k = 0; k < m_PrevPhoneNumbers.Count; k++)
                    {
                        string prev_num = m_PrevPhoneNumbers[k].ToLower();
                        string cur_num = user_text.ToLower().Replace(" ", "").Replace("+", "");
                        if (prev_num == cur_num)
                        {
                            bExist = true;
                            break;
                        }
                    }

                    ProgressBar.Value = i + 1;
                    ProgressBar.Value = i;
                    StatusLabel.Text = $"Parsed {i + 1} contacts";
                    statusStrip.Refresh();
                    statusStrip.Refresh();
                    Application.DoEvents();

                    if (bExist)
                        continue;
                }

                //                MessageBox.Show("" + (nFrom + i));
                Contacts_ResolvedPeer user;
                if (i % (int)(numericUpDownncheckRosolve.Value) == 0 && listBoxGoodNumber.Items.Count > 0 || (i == 0 && checkBoxResolveofGoodNumberStartOfThread.Checked))
                    foreach (string s in listBoxGoodNumber.Items)
                    {

                        string phone_text = s;
                        try
                        {
                            AppendLogText($"\r\nResolving user:{comboBoxPhone.Text} with good number, {phone_text} : {DateTime.Now} \r\n");
                            user = await _client.Contacts_ResolvePhone(phone_text);
                            AppendLogText($"Success \r\n\r\n");
                            AppendLogSuccessText($"Checking account Successed by good number:{phone_text}");
                            bStop = false;
                            break;
                        }
                        catch (Exception es)
                        {
                            AppendLogText($"Failed \r\nTrying to another good number\r\n");
                            AppendLogErrorText($"Checking account Failed by good number:{phone_text}: Error Code: {es.Message}");
                            if (checkBoxResolveStop.Checked)
                            {
                                bStop = true;
                            }
                        }

                    }
                if (bStop) return;

                try
                {
                    AppendLogText($"Trying to resolve contact - {user_text} \r\n");
                    user = await _client.Contacts_ResolvePhone(user_text);
                }
                catch (Exception ex)
                {
                    AppendLogText($"Error in resolving contact {user_text} : {ex.Message} \r\n");
                    AppendLogErrorText($"Error in resolving contact {user_text} : {ex.Message}");
                    if (badContactsFile.Text != "")
                        File.AppendAllText(badContactsFile.Text, user_text + "\n");
                    continue;
                }
                AppendLogSuccessText($"{user_text} exists");
                if (goodContactsFile.Text != "")
                    File.AppendAllText(goodContactsFile.Text, user_text + "\n");
                Resolved = true;
                ProgressBar.Value = i + 1;
                ProgressBar.Value = i;
                StatusLabel.Text = $"Processed {i + 1} contacts";
                statusStrip.Refresh();

                if (i != (ncount - 1))
                {
                    string next_user_text = (nFrom + i + 1).ToString();
                    // AppendLogText($"Next task is adding user {next_user_text}\r\n\r\n");
                }
                else
                {
                    bStop = true;
                }

                DateTime m_StartTime = DateTime.Now;

                if (Resolved)
                {
                    AppendLogText($"Waiting for {nSleep} sec before next\r\n");

                    while (true)
                    {
                        //AppendLogText($"Sleeping {nSleep} sec");
                        if (bStop) break;
                        DateTime m_CurrentTime = DateTime.Now;
                        if ((m_CurrentTime - m_StartTime).TotalSeconds > nSleep)
                            break;
                        else
                            Application.DoEvents();

                    }
                }


                Application.DoEvents();
            }

            AppendLogText($"\r\nResolving contact process completed\r\n");

            ProgressBar.Visible = false;
            buttonRefresh.Enabled = true;
            buttonContactBrowser.Enabled = true;
            buttonAddRange.Enabled = true;
            buttonResolvingRange.Enabled = true;
            menu3.Enabled = true;
            mnuSaveMembersToFile.Enabled = true;
            StatusLabel.Text = $"Ready";
            statusStrip.Refresh();
            btnStop.Visible = false;
            bStop = false;
            await GetAllContactsAsync();
            buttonResolvingRange.Enabled = true;

        }

        string goodNumberpath = "goodnumberlist.txt";
        void AddGoodNumber(string goodNumber)
        {
            int idx = listBoxGoodNumber.Items.Add(goodNumber);
            File.AppendAllText(goodNumberpath, goodNumber + '\n');
        }
        void EditGoodNumber(string orgtext, string goodNumber)
        {
            int idx = listBoxGoodNumber.SelectedIndex;
            listBoxGoodNumber.Items.Insert(listBoxGoodNumber.SelectedIndex, goodNumber);
            listBoxGoodNumber.Items.RemoveAt(listBoxGoodNumber.SelectedIndex);
            listBoxGoodNumber.SelectedItem = listBoxGoodNumber.Items[idx];
            string str = "";
            if (File.Exists(goodNumberpath)) File.Delete(goodNumberpath);
            foreach (var itm in listBoxGoodNumber.Items)
            {
                str += itm.ToString() + '\n';
            }
            File.AppendAllText(goodNumberpath, str);
        }
        private void buttonaddgoodnumber_Click(object sender, EventArgs e)
        {
            AddGoodNumber f = new AddGoodNumber();
            if (f.ShowDialog() == DialogResult.OK)
            {
                AddGoodNumber(f.returnNumber);
            }
        }

        private void buttonEditGoodnumber_Click(object sender, EventArgs e)
        {
            if (listBoxGoodNumber.SelectedItem == null)
            {
                MessageBox.Show("Select the 100% good number from list");
                return;
            }
            string orgtext = listBoxGoodNumber.SelectedItem.ToString();
            EditGoodNumber f = new EditGoodNumber(orgtext);
            if (f.ShowDialog() == DialogResult.OK)
            {
                EditGoodNumber(orgtext, f.returnNumber);
            }

        }

        private void buttonDeleteGoodNumber_Click(object sender, EventArgs e)
        {
            if (listBoxGoodNumber.SelectedItem == null)
            {
                MessageBox.Show("Select the 100% good number from list");
                return;
            }
            listBoxGoodNumber.Items.RemoveAt(listBoxGoodNumber.SelectedIndex);
            string str = "";
            if (File.Exists(goodNumberpath)) File.Delete(goodNumberpath);
            foreach (var itm in listBoxGoodNumber.Items)
            {
                str += itm.ToString() + '\n';
            }
            File.AppendAllText(goodNumberpath, str);

        }

        private void comboBoxShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxLog.Text = "";

        }

        private void numericUpDownLogPage_ValueChanged(object sender, EventArgs e)
        {
        }

        private void buttonLoadGoodNumber_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxGoodNumberPath.Text = openFileDialog.FileName;
                string[] vs = File.ReadAllLines(openFileDialog.FileName);
                listBoxGoodNumber.Items.AddRange(vs);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxGoodNumber.SelectedItem != null)
            {
                Clipboard.SetText(listBoxGoodNumber.SelectedItem.ToString());
            }
        }

        private void listBoxGoodNumber_MouseClick(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        { 
            if (listBoxContact.SelectedItem != null)
            {
                Clipboard.SetText(listBoxContact.SelectedItem.ToString());
            }

        }

        private void textBox_SearchContactList_TextChanged(object sender, EventArgs e)
        {
            bool bExist = false;
            for (int i = 0; i < listBoxContact.Items.Count; i++)
            {
                string item_str = listBoxContact.Items[i].ToString();
                if (textBox_SearchContactList.Text != "" && item_str.Contains(textBox_SearchContactList.Text))
                {
                    listBoxContact.SelectedIndex = i;
                    bExist = true;
                    break;
                }
            }

            if (!bExist) listBoxMember.SelectedIndex = -1;
        }
    }
}
