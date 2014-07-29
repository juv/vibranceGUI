using System;

namespace gui.app.mvvm.model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
    }
}
