using System;
using System.Collections.Generic;
using System.Text;

namespace AGHA
{
    [Serializable]
    class SvdSet
    {
        //Following fields are made public for fast READ access. These arrays should not be directly written in to 
        //by code out side theis class!!
        public double[,] S1Vectors;
        public double[,] S2Vectors;
        public double[] S1SingularValues;
        public double[] S2SingularValues;
        public long[] TrainingDataCount;

        private double[] _s1NormSquares;
        private double[] _s2NormSquares;

        private double[] _x1Vector;
        private double[] _x2Vector;

        const bool PRE_INITIALIZE_ARRAYS = true;

        public SvdSet(int x1Count, int x2Count, int svCount)
        {
            S1Vectors = new double[x1Count, svCount];
            S2Vectors = new double[x2Count, svCount];

            InitializeArrayElement(S1Vectors, (PRE_INITIALIZE_ARRAYS) ? 0.001f : double.NaN);
            InitializeArrayElement(S2Vectors, (PRE_INITIALIZE_ARRAYS) ? 0.001f : double.NaN);

            _s1NormSquares = new double[svCount];
            _s2NormSquares = new double[svCount];

            if (PRE_INITIALIZE_ARRAYS)
            {
                for (int svIndex = 0; svIndex < svCount; svIndex++)
                {
                    for (int s1Index = 0; s1Index < S1Vectors.GetLength(0); s1Index++)
                        _s1NormSquares[svIndex] += (S1Vectors[s1Index, svIndex] * S1Vectors[s1Index, svIndex]);

                    for (int s2Index = 0; s2Index < S2Vectors.GetLength(0); s2Index++)
                        _s2NormSquares[svIndex] += (S2Vectors[s2Index, svIndex] * S2Vectors[s2Index, svIndex]);
                }
            }

            S1SingularValues = new double[svCount];
            S2SingularValues = new double[svCount];

            TrainingDataCount = new long[svCount];

            _x1Vector = new double[x1Count];
            _x2Vector = new double[x2Count];
        }

        private static void InitializeArrayElement(double[,] vectors, double initialValue)
        {
            for (int i = 0; i < vectors.GetLength(0); i++)
                for (int j = 0; j < vectors.GetLength(1); j++)
                    vectors[i, j] = initialValue;
        }

        private void SetS1VectorValue(int valueIndex, int vectorIndex, double elementValue)
        {
            //Subtract the previous square. If statement is faster than multiplication
            if ((S1Vectors[valueIndex, vectorIndex] != 0) && !double.IsNaN(S1Vectors[valueIndex, vectorIndex]))
            {
                _s1NormSquares[vectorIndex] -= (S1Vectors[valueIndex, vectorIndex] * S1Vectors[valueIndex, vectorIndex]);
            }

            S1Vectors[valueIndex, vectorIndex] = elementValue;

            //Update the norm square
            _s1NormSquares[vectorIndex] += (elementValue * elementValue);
        }

        public string GetVisual()
        {
            StringBuilder visual = new StringBuilder();
            for (int vectorIndex = 0; vectorIndex < _s1NormSquares.Length; vectorIndex++)
            {
                visual.Append(vectorIndex.ToString() + '\t' + S1SingularValues[vectorIndex].ToString() + '\t' + S2SingularValues[vectorIndex].ToString() + '\n');

                for (int s1ValueIndex = 0; s1ValueIndex < S1Vectors.GetLength(0); s1ValueIndex++)
                {
                    visual.Append(S1Vectors[s1ValueIndex, vectorIndex].ToString() + '\n'); 
                }

                visual.Append("---------------------------------------------------------\n");

                for (int s2ValueIndex = 0; s2ValueIndex < S2Vectors.GetLength(0); s2ValueIndex++)
                {
                    visual.Append(S2Vectors[s2ValueIndex, vectorIndex].ToString() + '\n');
                }
            }

            return visual.ToString();
        }

        public void UpdateSingularVectors(int x1, int x2, int y, int singularVectorIndexToUpdate)
        {
            if (!PRE_INITIALIZE_ARRAYS)
                InitializeSingularVectorValues(x1, x2);

            double x1Scaled = GetS2NormedValue(x2, singularVectorIndexToUpdate);
            double x2Scaled = GetS1NormedValue(x1, singularVectorIndexToUpdate);

            if (singularVectorIndexToUpdate > 0)
            {
                Array.Clear(_x1Vector, 0, _x1Vector.Length);
                Array.Clear(_x2Vector, 0, _x2Vector.Length);

                _x1Vector[x1] = x1Scaled;
                _x2Vector[x2] = x2Scaled;

                for (int singularVectorIndex = 0; singularVectorIndex < singularVectorIndexToUpdate; singularVectorIndex++)
                {
                    double x1Subtract = DotVectors(S1Vectors, singularVectorIndex, _x1Vector);
                    double x2Subtract = DotVectors(S2Vectors, singularVectorIndex, _x2Vector);

                    for (int i = 0; i < _x1Vector.Length; i++)
                        _x1Vector[i] -= S1Vectors[i, singularVectorIndex] * x1Subtract;

                    for (int i = 0; i < _x2Vector.Length; i++)
                        _x2Vector[i] -= S2Vectors[i, singularVectorIndex] * x2Subtract;
                }

                //Direct updates into vectors followed by norm square updates
                for (int i = 0; i < _x1Vector.Length; i++)
                    S1Vectors[i, singularVectorIndexToUpdate] += (y * _x1Vector[i]);

                for (int i = 0; i < _x2Vector.Length; i++)
                    S2Vectors[i, singularVectorIndexToUpdate] += (y * _x2Vector[i]);

                UpdateNormSquares(singularVectorIndexToUpdate);
            }
            else
            {
                SetS1VectorValue(x1, singularVectorIndexToUpdate, S1Vectors[x1, singularVectorIndexToUpdate] + (y * x1Scaled));
                SetS2VectorValue(x2, singularVectorIndexToUpdate, S2Vectors[x2, singularVectorIndexToUpdate] + (y * x2Scaled));
            }
        }

        private void UpdateNormSquares(int vectorIndex)
        {
            _s1NormSquares[vectorIndex] = 0;
            for (int valueIndex = 0; valueIndex < S1Vectors.GetLength(0); valueIndex++)
                _s1NormSquares[vectorIndex] += (S1Vectors[valueIndex, vectorIndex] * S1Vectors[valueIndex, vectorIndex]);

            _s2NormSquares[vectorIndex] = 0;
            for (int valueIndex = 0; valueIndex < S2Vectors.GetLength(0); valueIndex++)
                _s2NormSquares[vectorIndex] += (S2Vectors[valueIndex, vectorIndex] * S2Vectors[valueIndex, vectorIndex]);
        }

        private void InitializeSingularVectorValues(int x1, int x2)
        {
            if (double.IsNaN(S1Vectors[x1, 0]))
                for(int vectorIndex = 0; vectorIndex < S1Vectors.GetLength(1); vectorIndex++)
                    SetS1VectorValue(x1, vectorIndex, 0.001f);

            if (double.IsNaN(S2Vectors[x2, 0]))
                for (int vectorIndex = 0; vectorIndex < S2Vectors.GetLength(1); vectorIndex++)
                    SetS2VectorValue(x2, vectorIndex, 0.001f);
        }

        public double Predict(TriArray xyValues, int totalValuesCount, int singularVectorCount)
        {
            for (int dataIndex = 0; dataIndex < xyValues.DataCount; dataIndex++)
            {
                double sum = 0;
                for (int svIndex = 0; svIndex < singularVectorCount; svIndex++)
                    sum += (S1Vectors[xyValues.X1[dataIndex], svIndex] * S2Vectors[xyValues.X2[dataIndex], svIndex] * ((S1SingularValues[svIndex] + S2SingularValues[svIndex]) / 2));

                xyValues.YPred[dataIndex] = (byte) Math.Round(sum * totalValuesCount);
            }

            return xyValues.GetRMSError();
        }

        public static double DotVectors(double[,] v1, int v1Index, double[] v2)
        {
            double sum = 0;

            for (int i = 0; i < v2.Length; i++)
                sum += (v1[i, v1Index] * v2[i]);

            return sum;
        }

        private void SetS2VectorValue(int valueIndex, int vectorIndex, double elementValue)
        {
            //Subtract the previous square. If statement is faster than multiplication
            if ((S2Vectors[valueIndex, vectorIndex] != 0) && !double.IsNaN(S2Vectors[valueIndex, vectorIndex]))
            {
                _s2NormSquares[vectorIndex] -= (S2Vectors[valueIndex, vectorIndex] * S2Vectors[valueIndex, vectorIndex]);
            }

            S2Vectors[valueIndex, vectorIndex] = elementValue;

            //Update the norm square
            _s2NormSquares[vectorIndex] += (elementValue * elementValue);
        }

        private double GetS2NormedValue(int valueIndex, int vectorIndex)
        {
            if (_s2NormSquares[vectorIndex] == 0)
            {
                return S2Vectors[valueIndex, vectorIndex];
            }
            else
            {
                return S2Vectors[valueIndex, vectorIndex] / GetS2VectorNorm(vectorIndex);
            }
        }

        private double GetS1NormedValue(int valueIndex, int vectorIndex)
        {
            if (_s1NormSquares[vectorIndex] == 0)
            {
                return S1Vectors[valueIndex, vectorIndex];
            }
            else
            {
                return S1Vectors[valueIndex, vectorIndex] / GetS1VectorNorm(vectorIndex);
            }
        }

        public double GetS1VectorNorm(int vectorIndex)
        {
            return (double)Math.Sqrt(_s1NormSquares[vectorIndex]);
        }

        public double GetS2VectorNorm(int vectorIndex)
        {
            return (double)Math.Sqrt(_s2NormSquares[vectorIndex]);
        }


        public void NormalizeVectors(int vectorIndex)
        {
            double s1Norm = GetS1VectorNorm(vectorIndex);
            double s2Norm = GetS2VectorNorm(vectorIndex);

            for (int valueIndex = 0; valueIndex < S1Vectors.GetLength(0); valueIndex++)
                S1Vectors[valueIndex, vectorIndex] = S1Vectors[valueIndex, vectorIndex] / s1Norm;

            for (int valueIndex = 0; valueIndex < S2Vectors.GetLength(0); valueIndex++)
                S2Vectors[valueIndex, vectorIndex] = S2Vectors[valueIndex, vectorIndex] / s2Norm;
        }
    }
}
