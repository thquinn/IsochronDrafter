using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using tcpServer;
using System.IO;

namespace IsochronDrafter
{
    public class DraftServer
    {
        private static readonly int NUM_RARES_IN_PACK = 1;
        private static readonly int NUM_UNCOMMONS_IN_PACK = 3;
        private static readonly int NUM_COMMONS_IN_PACK = 10;

        ServerWindow serverWindow;
        TcpServer server;

        List<string> commons = new List<string>();
        List<string> uncommons = new List<string>();
        List<string> rares = new List<string>();
        List<string> mythicRares = new List<string>();
        public string setName;
        public bool draftStarted = false;
        public int packNumber = 0;

        public ConcurrentDictionary<TcpServerConnection, string> aliases = new ConcurrentDictionary<TcpServerConnection,string>();
        public DraftState[] draftStates;

        public DraftServer(ServerWindow serverWindow, string setFilename)
        {
            this.serverWindow = serverWindow;
            ParseText(File.ReadAllText(setFilename));
            serverWindow.PrintLine("Loaded set: " + setName + ".");

            server = new TcpServer();
            server.Port = 10024;
            server.OnConnect += OnConnect;
            server.OnDisconnect += OnDisconnect;
            server.OnDataAvailable += OnDataAvailable;
            server.Open();
            serverWindow.PrintLine("Launched server on port " + server.Port + ". Accepting connections.");
        }

        private void OnConnect(TcpServerConnection connection)
        {
            string ipAndPort = GetAlias(connection);
            serverWindow.PrintLine("<" + ipAndPort + "> connected.");
            TrySendMessage(connection, "OK|HELLO");
        }
        private void OnDisconnect(TcpServerConnection connection)
        {
            string alias = GetAlias(connection);
            string tmp;
            bool removed = aliases.TryRemove(connection, out tmp);
            serverWindow.PrintLine("<" + alias + "> disconnected.");
            if (removed)
            {
                TrySendMessage("USER_DISCONNECTED|" + alias);
            }
            UpdateUserList();
        }
        private void OnDataAvailable(TcpServerConnection connection)
        {
            byte[] data = ReadStream(connection.Socket);

            if (data == null)
                return;
            string dataStr = Encoding.UTF8.GetString(data);
            HandleMessage(connection, dataStr);
        }
        protected byte[] ReadStream(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable)
            {
                byte[] data = new byte[client.Available];

                int bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(data, 0, data.Length);
                }
                catch (IOException)
                {
                }

                if (bytesRead < data.Length)
                {
                    byte[] lastData = data;
                    data = new byte[bytesRead];
                    Array.ConstrainedCopy(lastData, 0, data, 0, bytesRead);
                }
                return data;
            }
            return null;
        }
        private void HandleMessage(TcpServerConnection connection, string msg)
        {
            string[] parts = msg.Split('|');
            if (parts[0] == "VERSION")
            {
                int version = int.Parse(parts[1]);
                if (version < Util.version)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with old client version " + version + ".");
                    TrySendMessage(connection, "ERROR|OLD_CLIENT_VERSION|" + Util.version);
                }
                else if (version > Util.version)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with newer client version " + version + ". Please update the server.");
                    TrySendMessage(connection, "ERROR|OLD_SERVER_VERSION|" + Util.version);
                }
                else
                    TrySendMessage(connection, "OK|VERSION");
            }
            else if (parts[0] == "ALIAS")
            {
                if (aliases.Values.Contains(parts[1]))
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to connect with an in-use alias.");
                    TrySendMessage(connection, "ERROR|ALIAS_IN_USE");
                }
                else if (draftStarted && FindIndexOfDraftState(parts[1]) == -1)
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> attempted to join an in-progress.");
                    TrySendMessage(connection, "ERROR|DRAFT_IN_PROGRESS");
                }
                else
                {
                    serverWindow.PrintLine("<" + GetAlias(connection) + "> has new alias " + parts[1] + ".");
                    aliases.TryAdd(connection, parts[1]);
                    TrySendMessage(connection, "OK|ALIAS");
                    TrySendMessage("USER_CONNECTED|" + parts[1]);
                    if (draftStarted)
                    {
                        DraftState draftState = draftStates[FindIndexOfDraftState(parts[1])];
                        if (draftState.cardPool.Count > 0)
                            TrySendMessage(connection, "CARD_POOL|" + string.Join("|", draftState.cardPool));
                        if (draftState.boosters.Count > 0)
                            TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.boosters[0]));
                    }
                    else
                    {
                        UpdateUserList();
                    }
                }
            }
            else if (parts[0] == "PICK")
            {
                // Remove pick from pack and add to card pool.
                int draftIndex = FindIndexOfDraftState(aliases[connection]);
                DraftState draftState = draftStates[draftIndex];
                DraftState nextDraftState;
                if (packNumber % 2 == 1)
                    nextDraftState = draftStates[(draftIndex + 1) % draftStates.Length];
                else
                    nextDraftState = draftStates[(draftIndex + draftStates.Length - 1) % draftStates.Length];
                int pickIndex = int.Parse(parts[1]);
                List<string> booster = draftState.boosters[0];
                string pick = booster[pickIndex];
                draftState.cardPool.Add(pick);
                booster.RemoveAt(pickIndex);
                draftState.boosters.Remove(booster);
                TrySendMessage(connection, "OK|PICK");
                serverWindow.PrintLine("<" + draftState.alias + "> picked " + pick + ".");

                // Pass the pack to the next player, if not empty.
                if (booster.Count > 0)
                {
                    nextDraftState.boosters.Add(booster);
                    serverWindow.PrintLine("<" + nextDraftState.alias + "> got a new pack in their queue (now " + nextDraftState.boosters.Count + ").");
                    if (nextDraftState.boosters.Count == 1 && nextDraftState != draftState)
                        TrySendMessage(nextDraftState.alias, "BOOSTER|" + string.Join("|", booster));
                }
                else
                {
                    // Check if no one has any boosters.
                    bool packOver = true;
                    foreach (DraftState draftStateToCheck in draftStates)
                        if (draftStateToCheck.boosters.Count > 0)
                            packOver = false;
                    if (packOver)
                        StartNextPack();
                }

                // Current player gets the next booster in their queue, if any.
                if (draftState.boosters.Count > 0)
                {
                    TrySendMessage(connection, "BOOSTER|" + string.Join("|", draftState.boosters[0]));
                }
            }
            else
                serverWindow.PrintLine("<" + GetAlias(connection) + "> Unknown message: " + msg);
        }
        private string GetAlias(TcpServerConnection connection)
        {
            if (aliases.ContainsKey(connection))
                return aliases[connection];
            return (connection.Socket.Client.RemoteEndPoint as IPEndPoint).ToString();
        }
        private void ParseText(string txt)
        {
            string[] cardStrings = txt.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            setName = cardStrings[0];
            for (int i = 1; i < cardStrings.Length; i++)
            {
                string[] lines = cardStrings[i].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines[1] == "common")
                    commons.Add(lines[0]);
                else if (lines[1] == "uncommon")
                    uncommons.Add(lines[0]);
                else if (lines[1] == "rare")
                    rares.Add(lines[0]);
                else if (lines[1] == "mythic rare")
                    mythicRares.Add(lines[0]);
            }
        }

        public void StartNextPack()
        {
            packNumber++;
            if (packNumber == 1) // Begin the draft.
            {
                draftStarted = true;
                string[] shuffledAliases = aliases.Values.ToArray().OrderBy(x => Util.random.Next()).ToArray();
                draftStates = new DraftState[aliases.Count];
                for (int i = 0; i < shuffledAliases.Length; i++)
                {
                    string alias = shuffledAliases[i];
                    draftStates[i] = new DraftState(alias);
                }
            }
            else if (packNumber == 4) // End the draft.
            {
                serverWindow.PrintLine("The draft has ended.");
                TrySendMessage("DONE");
                return;
            }
            foreach (DraftState draftState in draftStates)
            {
                List<string> booster = GenerateBooster();
                draftState.AddBooster(booster);
                TrySendMessage(draftState.alias, "BOOSTER|" + string.Join("|", booster));
            }
            serverWindow.PrintLine("Passed out pack #" + packNumber + ".");
        }
        private List<string> GenerateBooster()
        {
            List<string> booster = new List<string>();

            // Add rares and mythic rares.
            int numRares = 0, numMythicRares = 0;
            for (int i = 0; i < NUM_RARES_IN_PACK; i++)
                if (Util.random.NextDouble() < .125)
                    numMythicRares++;
                else
                    numRares++;
            int[] mythicRareIndexes = Util.PickN(mythicRares.Count, numMythicRares);
            foreach (int i in mythicRareIndexes)
                booster.Add(mythicRares[i]);
            int[] rareIndexes = Util.PickN(rares.Count, numRares);
            foreach (int i in rareIndexes)
                booster.Add(rares[i]);
            // Add uncommons.
            int[] uncommonIndexes = Util.PickN(uncommons.Count, NUM_UNCOMMONS_IN_PACK);
            foreach (int i in uncommonIndexes)
                booster.Add(uncommons[i]);
            // Add commons.
            int[] commonIndexes = Util.PickN(commons.Count, NUM_COMMONS_IN_PACK);
            foreach (int i in commonIndexes)
                booster.Add(commons[i]);

            return booster;
        }
        private int FindIndexOfDraftState(string alias)
        {
            for (int i = 0; i < draftStates.Length; i++)
                if (draftStates[i].alias == alias)
                    return i;
            return -1;
        }

        private void TrySendMessage(string alias, string message)
        {
            if (aliases.Values.Contains(alias))
            {
                TrySendMessage(aliases.First(x => x.Value == alias).Key, message);
            }
        }
        private void TrySendMessage(TcpServerConnection connection, string message)
        {
            connection.sendData(message + ";");
        }
        private void TrySendMessage(string message)
        {
            server.Send(message + ";");
        }
        private void UpdateUserList()
        {
            serverWindow.PrintLine("There are now " + aliases.Count + " users in the lobby.");
            serverWindow.DraftButtonEnabled(aliases.Count > 0);
            TrySendMessage("USER_LIST|" + string.Join("|", aliases.Values));
        }
    }
}
