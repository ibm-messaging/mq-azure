/**********************************************************************/
/*   <copyright                                                       */
/*   notice="lm-source-program"                                       */
/*   pids="5724-H72,"                                                 */
/*   years="2007,2015"                                                */
/*   crc="2787562084" >                                               */
/*   Licensed Materials - Property of IBM                             */
/*                                                                    */
/*   5724-H72,                                                        */
/*                                                                    */
/*   (C) Copyright IBM Corp. 2007, 2015 All Rights Reserved.          */
/*                                                                    */
/*   US Government Users Restricted Rights - Use, duplication or      */
/*   disclosure restricted by GSA ADP Schedule Contract with          */
/*   IBM Corp.                                                        */
/*   </copyright>                                                     */
/**********************************************************************/
/*                                                                    */
/* Description: WorkerRole communicates with a WebRole using the      */
/*              Request/Reply messaging pattern over IBM MQ Messaging */
/*              provider. When a request comes, the WorkerRole queries*/
/*              SQL Server database replies to WebRole.               */
/**********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Data.SqlClient;
using System.Transactions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using IBM.WMQ;
using mqHelpers;

namespace mqSqlWorker
{
    /// <summary>
    /// Worker role class implementing interaction between MQ and SQL Server
    /// </summary>
    public class mqSqlWorkerRole : RoleEntryPoint
    {
        /// <summary>
        /// Utility class
        /// </summary>
        mqHelpers.Utilities util = null;
        /// <summary>
        /// Represents SQL Connection
        /// </summary>
        SqlConnection _sqlConnection = null;
        /// <summary>
        /// Represents queue manager connection
        /// </summary>
        MQQueueManager _mqConnection = null;
        MQQueue _mqRequestQueue = null;
        MQQueue _mqReplyQueue = null;

        bool bConnInitialized = false;
        /// <summary>
        /// Indicates we are connected to a database
        /// </summary>
        bool _dbConnected = false;
        /// <summary>
        /// Indicates we are connected to queue manager
        /// </summary>
        bool _qmConnected = false;

        /// <summary>
        /// Run method - a big while loop that handle requests from web role
        /// </summary>
        public override void Run()
        {
            // Diagnostics for debugging purpose
            Trace.TraceInformation("mqSqlWorker-Run entry point called");

            while (true)
            {
                // We sleep for a second before reading next request.
                Thread.Sleep(1000);

                // We are good to go as we all setup.
                if (bConnInitialized)
                {
                    //Get a message from RequestQ and Update database
                    TransactionScope ts = null;

                    try
                    {
                        // create a new transaction scope
                        ts = new TransactionScope();

                        // Get a message under a transaction
                        MQMessage mqMsg = new MQMessage();
                        MQGetMessageOptions gmo = new MQGetMessageOptions();
                        gmo.WaitInterval = MQC.MQWI_UNLIMITED;
                        gmo.Options |= MQC.MQGMO_WAIT | MQC.MQGMO_SYNCPOINT;

                        _mqRequestQueue.Get(mqMsg, gmo);
                        String msgData = mqMsg.ReadUTF();
                        Trace.TraceInformation("Incoming Data: " + msgData);

                        // The incoming data will be in two parts. One will be the command
                        // to execute and the second part will be data for the command. The
                        // : is the separator
                        String[] cmdPart = msgData.Split('=');

                        // Check if we have the request in correct format.
                        if (cmdPart.Length == 2)
                        {
                            // Yes, then split the first part of the command.
                            //String[] cmdPart = cmdWithData[0].Split('=');

                            // So check what command we have got
                            if (cmdPart[0].Equals(Constants.CMD_COMMAND_KEY) == true)
                            {
                                // Query College
                                if (cmdPart[1].Equals(Constants.CMD_QUERY_COLLEGE) == true)
                                {
                                    // We need to send back query list
                                    String jsonString = getRecordsFromDB();
                                    sendReply(jsonString, mqMsg.MessageId);
                                    Trace.TraceInformation("Reply for Query College sent" + ": " + jsonString);
                                }
                                else
                                {
                                    // Unrecognized request. Just trace out.
                                    Trace.TraceInformation("Unrecognized request");
                                }
                            }
                        }
                        ts.Complete();
                    }
                    catch (MQException mqex)
                    {
                        if (mqex.ReasonCode != MQC.MQRC_NO_MSG_AVAILABLE)
                        {
                            Trace.TraceInformation("Failed transaction: " + mqex.ToString());
                        }
                        else if (mqex.ReasonCode == MQC.MQRC_CONNECTION_BROKEN)
                        {
                            Trace.TraceInformation("Connection to queue manager broken. Reconnecting.");
                            bConnInitialized = false;
                            Initialize();
                        }
                    }
                    catch (System.Transactions.TransactionAbortedException tae)
                    {
                        Trace.TraceInformation("Failed transaction: " + tae);
                    }
                    catch (System.TimeoutException te)
                    {
                        Trace.TraceInformation("Failed transaction: " + te);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceInformation("Failed transaction: " + ex);
                    }

                    // Dispose Transaction scope
                    if (ts != null)
                        ts.Dispose();
                }
                else
                {
                    // If we have failied initialize during start up, then try now.
                    Initialize();
                }
            }
        }

        /// <summary>
        /// Convert data to Json string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        private string SerializeJSon<T>(T t)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            ds.WriteObject(stream, t);
            string jsonString = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            stream.Close();
            return jsonString;
        }

        /// <summary>
        /// Read records from database
        /// </summary>
        /// <returns></returns>
        private String getRecordsFromDB()
        {
            String recordsList = null;

            try
            {
                // SQL query to read college records from database
                SqlCommand sqlQury = new SqlCommand("SELECT * FROM University", _sqlConnection);
                SqlDataReader reader = sqlQury.ExecuteReader();

                // Build a list of colleges
                List<mqHelpers.UniversityData> ciList = new List<mqHelpers.UniversityData>();

                // Iterate through all records we have got and build a list
                while (reader.Read())
                {
                    UniversityData ci = new UniversityData();

                    ci.universityName = reader[Constants.SQL_COLMN_COLLEGE_NAME].ToString();
                    ci.universityAdress = reader[Constants.SQL_COLMN_COLLEGE_ADRESS].ToString();
                    ci.universityRating = reader[Constants.SQL_COLMN_COLLEGE_REATING].ToString();
                    ciList.Add(ci);
                }
                
                // Convert the list to a JSon object
                JSonHelper js = new JSonHelper();
                recordsList = js.ConvertObjectToJSon(ciList);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Failed reading records" + ex.ToString());
            }
            return recordsList;
        }

        /// <summary>
        /// Sends reply to reply queue
        /// </summary>
        /// <param name="replyString"></param>
        /// <param name="msgId"></param>
        private void sendReply(String replyString, byte[] msgId)
        {
            MQMessage msgReply = new MQMessage();
            msgReply.WriteUTF(replyString);
            msgReply.CorrelationId = msgId;
            msgReply.Expiry = 3000;
            MQPutMessageOptions mqPmo = new MQPutMessageOptions();
            mqPmo.Options |= MQC.MQPMO_SYNCPOINT;
            _mqReplyQueue.Put(msgReply,mqPmo);
        }


        /// <summary>
        /// Initialize connection to MQ queue manager and SQL database
        /// </summary>
        private void Initialize()
        {
            // If we have not connected, then it's time to do so now.
            if (!bConnInitialized)
            {
                try
                {
                    // Read connection string from app.config file.
                    String mqConnectionString = CloudConfigurationManager.GetSetting("IBMMQConnection");
                    String dbConnectionString = CloudConfigurationManager.GetSetting("SQLDBConnection");

                    // Parse connection string
                    util = new Utilities(mqConnectionString, dbConnectionString);
                    // Connect to MQ queue manager
                    if (ConnectToQueueManager())
                    {
                        // Connect to SQL database
                        if (ConnectToSqlServer())
                        {
                            // Initialize flag to indicate we successfully connected
                            bConnInitialized = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    bConnInitialized = false;
                    Trace.TraceInformation(ex.ToString());
                }
            }
        }

        /// <summary>
        /// OnStart call
        /// </summary>
        /// <returns></returns>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            // Read MQ and DB Configuration information
            Trace.TraceInformation("OnStart Called.");
            Initialize();
            return base.OnStart();
        }

        /// <summary>
        /// Connect to queue manager
        /// </summary>
        public bool ConnectToQueueManager()
        {
            Hashtable mqProps = new Hashtable();

            try
            {
                // Set connection properties. We will be connecting in managed mode
                mqProps.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                mqProps.Add(MQC.CONNECTION_NAME_PROPERTY, util.MqConnectionName);
                mqProps.Add(MQC.CHANNEL_PROPERTY, util.MqChannel);
                mqProps.Add(MQC.USER_ID_PROPERTY, util.MqUser);
                mqProps.Add(MQC.PASSWORD_PROPERTY, util.MqPassword);
                mqProps.Add(MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, true);

                Trace.TraceInformation( "QM: " + util.MqQueueManager + " Channel: " + util.MqChannel + 
                                        " Connection Name: " + util.MqConnectionName + " UID: " + util.MqUser + 
                                        " RQ: " + util.MqRequestQ + " RP: " + util.MqReplyQ);
                // Connect to queue manager
                _mqConnection = new MQQueueManager(util.MqQueueManager, mqProps);
                Trace.TraceInformation("Connected to queue manager");

                // Open request queue
                _mqRequestQueue = _mqConnection.AccessQueue(util.MqRequestQ, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                Trace.TraceInformation("Opened " + util.MqRequestQ + "for GET");
                
                // Open reply queue
                _mqReplyQueue = _mqConnection.AccessQueue(util.MqReplyQ, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
                Trace.TraceInformation("Opened " + util.MqReplyQ + "for PUT");

                Trace.TraceInformation("Initialization for messaging provider completed" );
                _qmConnected = true;
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Failed to connect to MQ. CompCode & ReasonCode" + ex.ToString());
                Trace.TraceInformation("Call Stack: " + ex.StackTrace);
                _qmConnected = false;
            }

            return _qmConnected;
        }

        /// <summary>
        /// Connect to SQL database
        /// </summary>
        private bool ConnectToSqlServer()
        {
            try
            {
                _sqlConnection = new SqlConnection();
                _sqlConnection.ConnectionString = util.DbConnection;
                _sqlConnection.Open();
                _dbConnected = true;
                Trace.TraceInformation("Connected to database");
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Failed to connect to DB. " + ex.ToString());
                _dbConnected = false;
            }

            return _dbConnected;
        }

        /// <summary>
        /// Handle OnStop event
        /// </summary>
        public override void OnStop()
        {
            Trace.TraceInformation("OnStop called.");

            // Close and disconnect queue manager resources
            if (_qmConnected)
            {
                try
                {
                    _mqRequestQueue.Close();
                    _mqReplyQueue.Close();
                    _mqConnection.Disconnect();
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("Failed to dispose MQ connection. " + ex.ToString());
                }
            }

            // Close SQL connection and dispose
            if (_dbConnected)
            {
                try
                {
                    _sqlConnection.Close();
                    _sqlConnection.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("Failed to dispose DB connection. " + ex.ToString());
                }
            }
            bConnInitialized = false;
        }
    }
}
