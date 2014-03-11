using System;
using System.Collections.Generic;
using System.Text;

namespace AGHA
{
    class TriArrayDataSource : ILearningDataSource
    {
        TriArray _ioValues;

        int _index = -1;
        public TriArrayDataSource(TriArray ioValues)
        {
            _ioValues = ioValues;
        }


        #region ILearningDataSource Members

        void ILearningDataSource.Reset()
        {
            _index = -1;
        }

        void ILearningDataSource.GetValues(out int x1, out int x2, out int y)
        {
            x1 = _ioValues.X1[_index];
            x2 = _ioValues.X2[_index];
            y = _ioValues.Y[_index];
        }

        bool ILearningDataSource.Next(bool resetIfEndReached)
        {
            _index++;
            bool endReached;
            endReached = (_index >= _ioValues.DataCount);

            if (endReached && resetIfEndReached)
            {
                _index = 0;
                endReached = (_index >= _ioValues.DataCount);
            }

            return !endReached;
        }

        #endregion
    }
}
