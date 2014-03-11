using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace AGHA
{
    class Corpus
    {

        public readonly DataSet Data;
        public readonly int TotalBigrams = 0;
        public readonly byte[,] BigramCounts = new byte[14, 15];
        public readonly int X1Count;
        public readonly int X2Count;

        public readonly TriArray XYValues;

        public Corpus(string filePath)
        {
            Data = new DataSet();
            DataTable table = Data.Tables.Add("Main");
            table.Columns.Add("x1", typeof(int));
            table.Columns.Add("x2", typeof(int));
            table.Columns.Add("y", typeof(int));
            

            string allText = File.ReadAllText(filePath);
            string[] sentences = allText.Split(new char[] { '.', '?', ':', '!' }, StringSplitOptions.RemoveEmptyEntries);

            int x1Index = 0; int x2Index = 0;
            System.Collections.Generic.Dictionary<string, int> x1Map = new Dictionary<string, int>();
            System.Collections.Generic.Dictionary<string, int> x2Map = new Dictionary<string, int>();


            foreach (string sentence in sentences)
            {
                string[] words = sentence.Split(new char[] { ' ', '\t', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
                for(int i=0; i < words.Length-1; i++)
                {
                    words[i] = words[i].ToLower();
                    words[i+1] = words[i+1].ToLower();

                    if (!x1Map.ContainsKey(words[i]))
                    {
                        x1Map.Add(words[i], x1Index++);
                    }
                    if (!x2Map.ContainsKey(words[i+1]))
                    {
                        x2Map.Add(words[i+1], x2Index++);
                    }

                    table.Rows.Add(x1Map[words[i]], x2Map[words[i+1]], 1);
                    
                    BigramCounts[x1Map[words[i]], x2Map[words[i+1]]]++;
                    TotalBigrams++;
                }
            }

            X1Count = x1Map.Count;
            X2Count = x2Map.Count;

            int nonZeroBigramCounts = 0;
            for (int x1 = 0; x1 < X1Count; x1++)
                for (int x2 = 0; x2 < X2Count; x2++)
                    if (BigramCounts[x1, x2] != 0) nonZeroBigramCounts++;

            XYValues = new TriArray(nonZeroBigramCounts, true);

            int triArrayIndex = 0;
            for (int x1 = 0; x1 < X1Count; x1++)
                for (int x2 = 0; x2 < X2Count; x2++)
                {
                    if (BigramCounts[x1, x2] != 0)
                    {
                        XYValues.X1[triArrayIndex] = x1;
                        XYValues.X2[triArrayIndex] = (Int16)x2;
                        XYValues.Y[triArrayIndex] = (byte)BigramCounts[x1, x2];
                        triArrayIndex++;
                    }
                }

            XYValues.CalculateYSum();
        }

        public string GetBigramCountsVisual()
        {
            StringBuilder visual = new StringBuilder();

            for (int i = 0; i < BigramCounts.GetLength(0); i++)
            {
                for (int j = 0; j < BigramCounts.GetLength(1); j++)
                    visual.Append((BigramCounts[i, j]*1.0/TotalBigrams).ToString() + "\t");

                visual.Append("\n");
            }

            return visual.ToString();
        }
    }
}
