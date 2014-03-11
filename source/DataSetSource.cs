using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace AGHA
{
    class DataSetSource : ILearningDataSource 
    {
        DataTable _dataTable;
        public DataSetSource(DataSet dataSetTouse)
        {
            _dataTable = dataSetTouse.Tables[0];
        }

        int index = -1;

        #region ILearningDataSource Members

        void ILearningDataSource.Reset()
        {
            index = -1;
        }

        void ILearningDataSource.GetValues(out int x1, out int x2, out int y)
        {
            x1 = (int)_dataTable.Rows[index][0];
            x2 = (int)_dataTable.Rows[index][1];
            y = (int)_dataTable.Rows[index][2];
        }

        bool ILearningDataSource.Next(bool resetIfEndReached)
        {
            index++;
            bool endReached;
            endReached = (index >= _dataTable.Rows.Count);

            if (endReached && resetIfEndReached)
            {
                index = 0;
                endReached = (index >= _dataTable.Rows.Count);
            }

            return !endReached;
        }

        #endregion
    }
}
