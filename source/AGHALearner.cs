using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace AGHA
{
    [Serializable]
    class AGHALearner
    {
        private const double S1_CONVERGENCE_THRESOLD = 1500000;
        private const double S2_CONVERGENCE_THRESOLD = 1500000;

        private int _x1Count;
        private int _x2Count;
        private int _svCount;
        private SvdSet _svdSet;

        public AGHALearner(int x1Count, int x2Count, int svCount)
        {
            _x1Count = x1Count;
            _x2Count = x2Count;
            _svCount = svCount;

            _svdSet = new SvdSet(_x1Count, _x2Count, _svCount);
        }


        public delegate void ProgressUpdateDelegate(int singularVectorIndex, double s1Norm, double s2Norm, long trainingDataCount, string message);
        [NonSerialized]
        private ProgressUpdateDelegate _ProgressUpdate;
        public event ProgressUpdateDelegate ProgressUpdate
        {
            add { _ProgressUpdate += value; }
            remove { _ProgressUpdate -= value; }
        }

        public int LearningTimeSeconds = 0;

        public void LearnFromData(ILearningDataSource dataSource)
        {
            DateTime startTime = DateTime.Now;

            dataSource.Reset();

            int currentSVIndex = 0;
            long trainingDataCount = 0;
            int checkPoint = 0;
            bool moreDataExists = dataSource.Next(true);
            double s1Norm = 0; double s2Norm = 0;

            while ((currentSVIndex < _svCount) && (moreDataExists))
            {
                int x1; int x2; int y;
                dataSource.GetValues(out x1, out x2, out y);

                if (y != 0)
                {

                    trainingDataCount += y;

                    _svdSet.UpdateSingularVectors(x1, x2, y, currentSVIndex);

                    checkPoint++;
                    if (checkPoint == 100)
                    {
                        checkPoint = 0;

                        s1Norm = _svdSet.GetS1VectorNorm(currentSVIndex);
                        s2Norm = _svdSet.GetS2VectorNorm(currentSVIndex);

                        _ProgressUpdate(currentSVIndex, s1Norm, s2Norm, trainingDataCount, null);
                    }

                    if ((s1Norm > S1_CONVERGENCE_THRESOLD) && (s2Norm > S2_CONVERGENCE_THRESOLD))
                    {
                        _svdSet.S1SingularValues[currentSVIndex] = _svdSet.GetS1VectorNorm(currentSVIndex) / trainingDataCount;
                        _svdSet.S2SingularValues[currentSVIndex] = _svdSet.GetS2VectorNorm(currentSVIndex) / trainingDataCount;
                        _svdSet.TrainingDataCount[currentSVIndex] = trainingDataCount;
                        _svdSet.NormalizeVectors(currentSVIndex);

                        //Move on to next vectors
                        trainingDataCount = 0;
                        currentSVIndex++;
                        s1Norm = 0; s2Norm = 0;
                        _ProgressUpdate(currentSVIndex, s1Norm, s2Norm, trainingDataCount, null);
                    }
                }

                moreDataExists = dataSource.Next(true);
            }

            LearningTimeSeconds = DateTime.Now.Subtract(startTime).Seconds;
        }

        public double Predict(TriArray xyValues, int totalValuesCount, int singularVectorCount)
        {
            return _svdSet.Predict(xyValues, totalValuesCount, singularVectorCount);
        }

        public string GetVisual()
        {
            return _svdSet.GetVisual();
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

        public static AGHALearner OpenFromFile(string filePath)
        {
            BinaryFormatter serializationFormatter = new BinaryFormatter();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                return (AGHALearner)serializationFormatter.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
        }

    }

    interface ILearningDataSource
    {
        void Reset();
        void GetValues(out int x1, out int x2, out int y);
        bool Next(bool resetIfEndReached);
    }
}
