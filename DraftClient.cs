using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public class DraftClient
    {
        DraftWindow draftWindow;
        EventDrivenTCPClient client;
        string alias;

        public DraftClient(DraftWindow draftWindow, string hostname, string alias)
        {
            this.draftWindow = draftWindow;
            this.alias = alias;

            IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
            client = new EventDrivenTCPClient(hostEntry.AddressList[0], 10024, false);
            client.DataEncoding = Encoding.UTF8;
            client.ConnectionStatusChanged += new EventDrivenTCPClient.delConnectionStatusChanged(client_ConnectionStatusChanged);
            client.DataReceived += new EventDrivenTCPClient.delDataReceived(client_DataReceived);
            
            client.Connect();
        }

        void client_ConnectionStatusChanged(EventDrivenTCPClient sender, EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connecting)
                draftWindow.PrintLine("Connecting...");
            else if (status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByHost || status == EventDrivenTCPClient.ConnectionStatus.DisconnectedByUser)
            {
                draftWindow.PrintLine("Connection failed: " + status.ToString());
                draftWindow.Invoke(new MethodInvoker(delegate
                {
                    draftWindow.ClearDraftPicker();
                    draftWindow.OpenConnectWindow();
                }));
                draftWindow.ClearCardPool();
            }
        }
        void client_DataReceived(EventDrivenTCPClient sender, object data)
        {
            string msgs = data as string;
            foreach (string msg in msgs.Split(';'))
                if (msg.Length > 0)
                    HandleMessage(msg);
            
        }
        private void HandleMessage(string msg)
        {
            string[] parts = msg.Split('|');
            if (parts[0] == "OK")
            {
                if (parts[1] == "HELLO")
                {
                    client.Send("VERSION|" + Util.version);
                }
                else if (parts[1] == "VERSION")
                {
                    draftWindow.PrintLine("Version OK.");
                    client.Send("ALIAS|" + alias);
                }
                else if (parts[1] == "ALIAS")
                {
                    draftWindow.PrintLine("Connected as " + alias + ".");
                }
                else if (parts[1] == "PICK")
                {
                    draftWindow.ClearDraftPicker();
                    draftWindow.EnableDraftPicker();
                }
            }
            else if (parts[0] == "ERROR")
            {
                if (parts[1] == "OLD_CLIENT_VERSION")
                    draftWindow.PrintLine("Your client is out of date. Please update to the latest version.");
                else if (parts[1] == "OLD_SERVER_VERSION")
                    draftWindow.PrintLine("The server is out of date. Please update it to the latest version.");
                else if (parts[1] == "ALIAS_IN_USE")
                    draftWindow.PrintLine("That alias is in use. Please choose another alias.");
                else if (parts[1] == "DRAFT_IN_PROGRESS")
                    draftWindow.PrintLine("A draft is in progress on that server. To rejoin, use the same alias you were using when it started.");
                else
                    draftWindow.PrintLine("Unknown error from server: " + parts[1]);
                client.Disconnect();
            }
            else if (parts[0] == "IMAGE_DIR")
            {
                Util.imageDirectory = parts[1];
                draftWindow.PrintLine("Set image directory.");
            }
            else if (parts[0] == "USER_CONNECTED")
            {
                if (parts[1] != alias)
                    draftWindow.PrintLine(parts[1] + " joined the lobby.");
            }
            else if (parts[0] == "USER_DISCONNECTED")
            {
                if (parts[1] != alias)
                    draftWindow.PrintLine(parts[1] + " left the lobby.");
            }
            else if (parts[0] == "USER_LIST")
            {
                parts[0] = "";
                string userList = string.Join(", ", parts).Substring(2);
                draftWindow.PrintLine("There are now " + (parts.Length - 1) + " users in the lobby: " + userList);
            }
            else if (parts[0] == "PACK_COUNT")
            {
                draftWindow.SetPackCounts(msg);
            }
            else if (parts[0] == "BOOSTER")
            {
                draftWindow.PopulateDraftPicker(msg);
                draftWindow.EnableDraftPicker();
            }
            else if (parts[0] == "CARD_POOL")
            {
                draftWindow.PrintLine("Loading draft in progress...");
                for (int i = 1; i < parts.Length; i++)
                    draftWindow.AddCardToPool(parts[i]);
                draftWindow.PrintLine("Loaded draft.");
            }
            else if (parts[0] == "DONE")
            {
                draftWindow.PrintLine("The draft has ended.");
            }
            else
                draftWindow.PrintLine("Unknown message from server: " + msg);
        }

        public void Pick(int index, string cardName)
        {
            client.Send("PICK|" + index);
            draftWindow.AddCardToPool(cardName);
        }
    }
}
