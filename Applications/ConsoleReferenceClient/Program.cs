/* ========================================================================
 * Copyright (c) 2005-2020 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace Quickstarts.ConsoleReferenceClient
{
    public static class Program
    {
        public static DirectoryInfo CurrentExeDir
        {
            get
            {
                string currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return new FileInfo(currentPath).Directory;
            }
        }

        /// <summary>
        /// Main entry point.
        /// </summary>
        public static async Task Main(string[] args)
        {
            IOutput console = new ConsoleOutput();
            console.WriteLine("OPC UA Console Reference Client");
            try
            {
                // Define the UA Client application
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationName = "Quickstart Console Reference Client";
                application.ApplicationType = ApplicationType.Client;

                // load the application configuration.
                await application.LoadApplicationConfiguration("ConsoleReferenceClient.Config.xml", silent: false);
                // check the application certificate.
                await application.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);

                // create the UA Client object and connect to configured server.
                UAClient uaClient = new UAClient(application.ApplicationConfiguration, console, ClientBase.ValidateResponse);
                bool connected = await uaClient.ConnectAsync();
                if (connected)
                {
                    // Run tests for available methods.
                    uaClient.ReadNodes();
                    uaClient.WriteNodes();
                    uaClient.Browse();
                    uaClient.CallMethod();

                    var logfile = new NLog.Targets.FileTarget("logfile")
                    {
                        FileName = Path.Combine(dirPath, dateTimeService.Now.ToString("yyyy_MM_dd_-_HH_mm_ss") + "_OpcUA_Wrapper.log")
                    };
                    configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);
                    */
                    var logFilePath = Path.Combine(CurrentExeDir.FullName, $"ConsoleLog_{DateTime.Now.ToString("yyyyMMddhhmmssffff")}.txt");
                    while (File.Exists(logFilePath))
                    {
                        logFilePath = Path.Combine(CurrentExeDir.FullName, $"ConsoleLog_{DateTime.Now.ToString("yyyyMMddhhmmssffff")}.txt");
                    }
                    output.WriteLine($"Logger is: {logFilePath}");
                    Utils.SetLogger(new OpcLogger(logFilePath, LogLevel.Debug));
                    //Utils.Logger = new OpcLogger(logFilePath, LogLevel.Debug);
                    output.WriteLine($"{Utils.Logger}");
                    var timesTaken = new List<TimeSpan>();
                    for (int i = 0; i < 200; i++)
                    {
                        var sw = Stopwatch.StartNew();
                        uaClient.Session.Read(null, 0, TimestampsToReturn.Both, new ReadValueIdCollection() { new ReadValueId() {
                                            AttributeId = Attributes.Value, NodeId= new NodeId("\"ItemsTest\".\"Items\"", 3)} }, out DataValueCollection results, out DiagnosticInfoCollection diagnosticInfos);
                        timesTaken.Add(sw.Elapsed);
                    }
                    var avgTicks = timesTaken.Average(el => el.Ticks);
                    var avgTime = TimeSpan.FromTicks((long)avgTicks);
                    var maxTime = timesTaken.Max();
                    var minTime = timesTaken.Min();

                    output.WriteLine($"avg: {avgTime}");
                    output.WriteLine($"min: {minTime}");
                    output.WriteLine($"max: {maxTime}");
                    bool throwNow = true;
                    if (throwNow)
                    {
                        throw new NotImplementedException("");
                    }
                    // enable subscription transfer
                    uaClient.ReconnectPeriod = 1000;
                            uaClient.ReconnectPeriodExponentialBackoff = 10000;
                            uaClient.Session.MinPublishRequestCount = 3;
                            uaClient.Session.TransferSubscriptionsOnReconnect = true;
                            var samples = new ClientSamples(output, ClientBase.ValidateResponse, quitEvent, verbose);
                            if (loadTypes)
                            {
                                var complexTypeSystem = await samples.LoadTypeSystemAsync(uaClient.Session).ConfigureAwait(false);
                            }

                            if (browseall || fetchall || jsonvalues || managedbrowseall)
                            {
                                NodeIdCollection variableIds = null;
                                NodeIdCollection variableIdsManagedBrowse = null;
                                ReferenceDescriptionCollection referenceDescriptions = null;
                                ReferenceDescriptionCollection referenceDescriptionsFromManagedBrowse = null;

                                if (browseall)
                                {
                                    output.WriteLine("Browse the full address space.");
                                    referenceDescriptions =
                                        await samples.BrowseFullAddressSpaceAsync(uaClient, Objects.RootFolder).ConfigureAwait(false);
                                    variableIds = new NodeIdCollection(referenceDescriptions
                                        .Where(r => r.NodeClass == NodeClass.Variable && r.TypeDefinition.NamespaceIndex != 0)
                                        .Select(r => ExpandedNodeId.ToNodeId(r.NodeId, uaClient.Session.NamespaceUris)));
                                }

                                if (managedbrowseall)
                                {
                                    output.WriteLine("ManagedBrowse the full address space.");
                                    referenceDescriptionsFromManagedBrowse =
                                        await samples.ManagedBrowseFullAddressSpaceAsync(uaClient, Objects.RootFolder).ConfigureAwait(false);
                                    variableIdsManagedBrowse = new NodeIdCollection(referenceDescriptionsFromManagedBrowse
                                        .Where(r => r.NodeClass == NodeClass.Variable && r.TypeDefinition.NamespaceIndex != 0)
                                        .Select(r => ExpandedNodeId.ToNodeId(r.NodeId, uaClient.Session.NamespaceUris)));
                                }

                                // treat managedBrowseall result like browseall results if the latter is missing
                                if (!browseall && managedbrowseall)
                                {
                                    referenceDescriptions = referenceDescriptionsFromManagedBrowse;
                                    browseall = managedbrowseall;
                                }

                                IList<INode> allNodes = null;
                                if (fetchall)
                                {
                                    allNodes = await samples.FetchAllNodesNodeCacheAsync(uaClient, Objects.RootFolder, true, true, false).ConfigureAwait(false);
                                    variableIds = new NodeIdCollection(allNodes
                                        .Where(r => r.NodeClass == NodeClass.Variable && r is VariableNode && ((VariableNode)r).DataType.NamespaceIndex != 0)
                                        .Select(r => ExpandedNodeId.ToNodeId(r.NodeId, uaClient.Session.NamespaceUris)));
                                }

                                if (jsonvalues && variableIds != null)
                                {
                                    var (allValues, results) = await samples.ReadAllValuesAsync(uaClient, variableIds).ConfigureAwait(false);
                                }

                                if (subscribe && (browseall || fetchall))
                                {
                                    // subscribe to 1000 random variables
                                    const int MaxVariables = 1000;
                                    NodeCollection variables = new NodeCollection();
                                    Random random = new Random(62541);
                                    if (fetchall)
                                    {
                                        variables.AddRange(allNodes
                                            .Where(r => r.NodeClass == NodeClass.Variable && r.NodeId.NamespaceIndex > 1)
                                            .Select(r => ((VariableNode)r))
                                            .OrderBy(o => random.Next())
                                            .Take(MaxVariables));
                                    }
                                    else if (browseall)
                                    {
                                        var variableReferences = referenceDescriptions
                                            .Where(r => r.NodeClass == NodeClass.Variable && r.NodeId.NamespaceIndex > 1)
                                            .Select(r => r.NodeId)
                                            .OrderBy(o => random.Next())
                                            .Take(MaxVariables)
                                            .ToList();
                                        variables.AddRange(uaClient.Session.NodeCache.Find(variableReferences).Cast<Node>());
                                    }

                                    await samples.SubscribeAllValuesAsync(uaClient,
                                        variableIds: new NodeCollection(variables),
                                        samplingInterval: 100,
                                        publishingInterval: 1000,
                                        queueSize: 10,
                                        lifetimeCount: 60,
                                        keepAliveCount: 2).ConfigureAwait(false);

                                    // Wait for DataChange notifications from MonitoredItems
                                    output.WriteLine("Subscribed to {0} variables. Press Ctrl-C to exit.", MaxVariables);

                                    // free unused memory
                                    uaClient.Session.NodeCache.Clear();

                                    waitTime = timeout - (int)DateTime.UtcNow.Subtract(start).TotalMilliseconds;
                                    DateTime endTime = waitTime > 0 ? DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(waitTime)) : DateTime.MaxValue;
                                    var variableIterator = variables.GetEnumerator();
                                    while (!quit && endTime > DateTime.UtcNow)
                                    {
                                        if (variableIterator.MoveNext())
                                        {
                                            try
                                            {
                                                var value = await uaClient.Session.ReadValueAsync(variableIterator.Current.NodeId).ConfigureAwait(false);
                                                output.WriteLine("Value of {0} is {1}", variableIterator.Current.NodeId, value);
                                            }
                                            catch (Exception ex)
                                            {
                                                output.WriteLine("Error reading value of {0}: {1}", variableIterator.Current.NodeId, ex.Message);
                                            }
                                        }
                                        else
                                        {
                                            variableIterator = variables.GetEnumerator();
                                        }
                                        quit = quitEvent.WaitOne(500);
                                    }
                                }
                                else
                                {
                                    quit = true;
                                }
                            }
                            else
                            {
                                int quitTimeout = 65_000;
                                if (enableDurableSubscriptions)
                                {
                                    quitTimeout = 150_000;
                                    uaClient.ReconnectPeriod = 500_000;
                                }

                                NodeId sessionNodeId = uaClient.Session.SessionId;
                                // Run tests for available methods on reference server.
                                samples.ReadNodes(uaClient.Session);
                                samples.WriteNodes(uaClient.Session);
                                samples.Browse(uaClient.Session);
                                samples.CallMethod(uaClient.Session);
                                samples.EnableEvents(uaClient.Session, (uint)quitTimeout);
                                samples.SubscribeToDataChanges(
                                    uaClient.Session, 60_000, enableDurableSubscriptions);

                                output.WriteLine("Waiting...");

                                // Wait for some DataChange notifications from MonitoredItems
                                int waitCounters = 0;
                                int checkForWaitTime = 1000;
                                int closeSessionTime = checkForWaitTime * 15;
                                int restartSessionTime = checkForWaitTime * 45;
                                bool stopNotQuit = false;
                                int stopCount = 0;
                                while (!quit && !stopNotQuit && waitCounters < quitTimeout)
                                {
                                    quit = quitEvent.WaitOne(checkForWaitTime);
                                    waitCounters += checkForWaitTime;
                                    if (enableDurableSubscriptions)
                                    {
                                        if (waitCounters == closeSessionTime)
                                        {
                                            if (uaClient.Session.SubscriptionCount == 1)
                                            {
                                                output.WriteLine("Closing Session at " + DateTime.Now.ToLongTimeString());
                                                uaClient.Session.Close(closeChannel: false);
                                            }
                                        }

                                        if (waitCounters == restartSessionTime)
                                        {
                                            output.WriteLine("Restarting Session at " + DateTime.Now.ToLongTimeString());
                                            await uaClient.DurableSubscriptionTransfer(
                                                serverUrl.ToString(),
                                                useSecurity: !noSecurity,
                                                quitCTS.Token);
                                        }

                                        if ( waitCounters > closeSessionTime && waitCounters < restartSessionTime )
                                        {
                                            Console.WriteLine("No Communication Interval " + stopCount.ToString());
                                            stopCount++;
                                        }
                                    }
                                }
                            }

                            output.WriteLine("Client disconnected.");

                    uaClient.Disconnect();
                }
                else
                {
                    console.WriteLine("Could not connect to server!");
                }

                console.WriteLine("\nProgram ended.");
                console.WriteLine("Press any key to finish...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                console.WriteLine(ex.Message);
            }
        }
    }
}
