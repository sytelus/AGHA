using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AGHA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void TestGHACorpus()
        {
            this.Show();

            Corpus c = new Corpus(@"C:\Shital\Visual Studio 2005\Projects\RelevencyEngine\AGHA\AGHA\bin\Debug\man_dog_corpus.txt");
            //richTextBox1.Text = c.GetBigramCountsVisual();

            AGHALearner learner = new AGHALearner(c.X1Count, c.X2Count, 5);
            learner.ProgressUpdate += new AGHALearner.ProgressUpdateDelegate(ProgressUpdate);

            ILearningDataSource dataSource = new TriArrayDataSource(c.XYValues); //new ArrayDataSource(c.BigramCounts);   //new DataSetSource(c.Data);
            learner.LearnFromData(dataSource);

            richTextBox1.Text += learner.LearningTimeSeconds.ToString() + " seconds \n";

            richTextBox1.Text += learner.Predict(c.XYValues, c.XYValues.YSum, 5).ToString() + "\n";

            learner.SaveToFile("GHALabSvd.bin");
            AGHALearner savedLearner = AGHALearner.OpenFromFile("GHALabSvd.bin");
            richTextBox1.Text += savedLearner.Predict(c.XYValues, c.XYValues.YSum, 5).ToString() + "\n";

            //richTextBox1.Text += learner.GetVisual();

            //c.XYValues.SaveToFile("testxy.bin");
            //TriArray t = TriArray.OpenFromFile("testxy.bin");
            //richTextBox1.Text += t.YSum.ToString() + "\n";

        }

        static int updateInterval = 0;
        private void ProgressUpdate(int singularVectorIndex, double s1Norm, double s2Norm, long trainingDataCount, string message)
        {
            updateInterval++;
            
            if (updateInterval >= 1000)
            {
                updateInterval = 0;
                label1.Text = singularVectorIndex.ToString() + '\t' + s1Norm.ToString() + '\t' + s2Norm.ToString() + '\t' + (trainingDataCount/1000000.0f) + '\t';
                this.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TestGHACorpus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            TriArray r = new TriArray(99072112, false, "(local)", "NetflixRatings",
//                @"SELECT c.ID As CID, m.ID AS MID, r.Rating
//                  FROM Rating r
//                  JOIN Movie m ON r.MovieID = m.MovieID
//                  JOIN Customer c ON r.CustomerID = c.CustomerID
//                  ORDER BY r.RatingDate
//                 ");
//            r.SaveToFile("ratings.bin");

            TriArray p = new TriArray(1408395, true, "(local)", "NetflixRatings",
                @"SELECT c.ID As CID, m.ID AS MID, r.Rating
                  FROM Probe r
                  JOIN Movie m ON r.MovieID = m.MovieID
                  JOIN Customer c ON r.CustomerID = c.CustomerID
                 ");
            p.SaveToFile("probe.bin");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Show();

            AGHALearner learner = new AGHALearner(480189, 17770, 5);
            learner.ProgressUpdate += new AGHALearner.ProgressUpdateDelegate(ProgressUpdate);

            TriArray trainingData = TriArray.OpenFromFile("ratings.bin");
            trainingData.CalculateYSum();
            ILearningDataSource dataSource = new TriArrayDataSource(trainingData); //new ArrayDataSource(c.BigramCounts);   //new DataSetSource(c.Data);
            learner.LearnFromData(dataSource);

            richTextBox1.Text += learner.LearningTimeSeconds.ToString() + " seconds \n";
            this.Refresh();

            learner.SaveToFile("RatingsLearner.bin");


            TriArray probeData = TriArray.OpenFromFile("probe.bin");
            richTextBox1.Text += learner.Predict(probeData, trainingData.YSum, 5).ToString() + "\n";
            probeData.SaveToFile("probe_pred");
        }
    }
}