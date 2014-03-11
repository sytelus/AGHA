using System;
using System.Collections.Generic;
using System.Text;

namespace AGHA
{
    class ArrayDataSource : ILearningDataSource 
    {
        byte[,] _data;
        int _x1 = -1;
        int _x2 = -1;
        public ArrayDataSource(byte[,] data)
        {
            _data = data;
        }

        #region ILearningDataSource Members

        void ILearningDataSource.Reset()
        {
            _x1 = -1;
            _x2 = -1;
        }

        void ILearningDataSource.GetValues(out int x1, out int x2, out int y)
        {
            x1 = _x1;
            x2 = _x2;
            y = _data[_x1, _x2];
        }


        static bool _x1x2 = false;
        bool ILearningDataSource.Next(bool resetIfEndReached)
        {
            if (_x1x2)
                return X1X2Next(resetIfEndReached);
            else
                return X2X1Next(resetIfEndReached);
        }

        private bool X1X2Next(bool resetIfEndReached)
        {
            if (_x1 == -1) _x1 = 0;

            _x2++;

            bool x1EndReached = _x1 >= _data.GetLength(0);
            bool x2EndReached = _x2 >= _data.GetLength(1);

            if (x2EndReached)
            {
                _x1++;
                _x2 = 0;

                x1EndReached = _x1 >= _data.GetLength(0);
                x2EndReached = _x2 >= _data.GetLength(1);
            }

            if (x1EndReached)
            {
                if (resetIfEndReached)
                {
                    _x1 = 0;
                    _x2 = 0;

                    x1EndReached = _x1 >= _data.GetLength(0);
                    x2EndReached = _x2 >= _data.GetLength(1);
                    
                    _x1x2 = !_x1x2;
                }
            }

            return !(x1EndReached && x2EndReached);
        }

        private bool X2X1Next(bool resetIfEndReached)
        {
            if (_x2 == -1) _x2 = 0;

            _x1++;

            bool x2EndReached = _x2 >= _data.GetLength(1);
            bool x1EndReached = _x1 >= _data.GetLength(0);

            if (x1EndReached)
            {
                _x2++;
                _x1 = 0;

                x2EndReached = _x2 >= _data.GetLength(1);
                x1EndReached = _x1 >= _data.GetLength(0);
            }

            if (x2EndReached)
            {
                if (resetIfEndReached)
                {
                    _x2 = 0;
                    _x1 = 0;

                    x2EndReached = _x2 >= _data.GetLength(1);
                    x1EndReached = _x1 >= _data.GetLength(0);

                    _x1x2 = !_x1x2;
                }
            }

            return !(x2EndReached && x1EndReached);
        }

        #endregion
    }
}
