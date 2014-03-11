using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.SqlClient;

namespace AGHA
{
    [Serializable]
    class TriArray
    {
        public readonly Int32[] X1;
        public readonly Int16[] X2;
        public readonly byte[] Y;
        public readonly byte[] YPred;
        public readonly int DataCount;
        public int YSum = 0;
        public TriArray(int size, bool createPreditionArray)
        {
            X1 = new Int32[size];
            X2 = new Int16[size];
            Y = new byte[size];

            DataCount = size;

            if (createPreditionArray)
            {
                YPred = new byte[size];
            }
        }

        public TriArray(int size, bool createPreditionArray, string sqlServerName, string databaseName, string sql)
            : this(size, createPreditionArray)
        {
            using (SqlConnection connection = new SqlConnection(string.Format(@"Server={0};Database={1};Trusted_Connection=True;", sqlServerName, databaseName))) 
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandTimeout = 0;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult))
                {
                    int index = 0;

                    // Call Read before accessing data.
                    while (reader.Read())
                    {
                        X1[index] = (Int32)reader.GetInt32(0);
                        X2[index] = (Int16)reader.GetInt32(1);
                        Y[index] = (byte)reader.GetInt32(2);

                        index++;
                    }

                    // Call Close when done reading.
                    reader.Close();
                }
            }
        }

        public void CalculateYSum()
        {
            YSum = 0;
            for (int dataIndex = 0; dataIndex < DataCount; dataIndex++)
            {
                YSum += Y[dataIndex];
            }

        }

        public void SaveToFile(string filePath)
        {
            BinaryFormatter serializationFormatter = new BinaryFormatter();
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                serializationFormatter.Serialize(fs, this);
            }
            finally
            {
                fs.Close();
            }
        }

        public static TriArray OpenFromFile(string filePath)
        {
            BinaryFormatter serializationFormatter = new BinaryFormatter();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                return (TriArray) serializationFormatter.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        public double GetRMSError()
        {
            double errorSquareSum = 0;
            for (int dataIndex = 0; dataIndex < DataCount; dataIndex++)
            {
                errorSquareSum += (Y[dataIndex] - YPred[dataIndex]) * (Y[dataIndex] - YPred[dataIndex]);
            }
            return (double) Math.Sqrt(errorSquareSum / DataCount);
        }
    }
}
