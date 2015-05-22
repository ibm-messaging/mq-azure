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
/* Description: This application reads an Xml file and updates SQL    */
/*              database with data.                                   */
/**********************************************************************/
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml;

namespace PopulateDB
{
    class PopulateUniverDB
    {
        /// <summary>
        /// Represents SQL Connection
        /// </summary>
        SqlConnection _sqlConnection = null;
        /// <summary>
        /// Sql Server connection string
        /// </summary>
        String sqlConStr = null;
        /// <summary>
        /// Name of Xml file containing recrods to update into DB
        /// </summary>
        String RecordsFileName = null;

        static void Main(string[] args)
        {
            PopulateUniverDB pdb = new PopulateUniverDB();
            pdb.ReadARecordFromXmlFileAndUpdateDB();
        }

        private void Initialize()
        {
            sqlConStr = ConfigurationManager.AppSettings["SQLDBConnection"];
            RecordsFileName = ConfigurationManager.AppSettings["RecordsFile"];
        }

        /// <summary>
        /// As it says, establish a connection to SQL Server
        /// </summary>
        private void OpenSqlConnection()
        {
            _sqlConnection = new SqlConnection();
            _sqlConnection.ConnectionString = sqlConStr;
            _sqlConnection.Open();
        }

        /// <summary>
        /// Reads a record from given XML file and updates database.
        /// </summary>
        private void ReadARecordFromXmlFileAndUpdateDB()
        {
            try
            {
                Initialize();
                OpenSqlConnection();
                ClearRecords();
                AddRecords();
                CloseSqlConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Close and dispose
        /// </summary>
        private void CloseSqlConnection()
        {
            if (_sqlConnection != null)
            {
                _sqlConnection.Close();
                _sqlConnection.Dispose();
            }
        }

        /// <summary>
        /// Add new records to database
        /// </summary>
        private void AddRecords()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(RecordsFileName);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/UniversityList/University");
            string Name = "", Address = "", Rating = "";
            int counter = 1;

            // First count how many records we have and then increment the counter
            SqlCommand cmdQuery = new SqlCommand("SELECT COUNT(*) FROM University", _sqlConnection);
            counter = (int)cmdQuery.ExecuteScalar() + 1;

            foreach (XmlNode node in nodeList)
            {
                Name = node.SelectSingleNode("Name").InnerText;
                Address = node.SelectSingleNode("Address").InnerText;
                Rating = node.SelectSingleNode("Rating").InnerText;

                // Insert into DB
                SqlCommand cmdInsert = new SqlCommand("INSERT INTO University VALUES(@universityId, @universityName, @universityAddress, @universityRating)", _sqlConnection);

                cmdInsert.Parameters.AddWithValue("@universityId", counter);
                cmdInsert.Parameters.AddWithValue("@universityName", Name);
                cmdInsert.Parameters.AddWithValue("@universityAddress", Address);
                cmdInsert.Parameters.AddWithValue("@universityRating", Rating);

                cmdInsert.ExecuteNonQuery();
                counter++;
            }
        }

        /// <summary>
        /// Clean up all records
        /// </summary>
        private void ClearRecords()
        {
            SqlCommand cmdQuery = new SqlCommand("DELETE FROM University", _sqlConnection);
            cmdQuery.ExecuteNonQuery();
        }
    }
}
