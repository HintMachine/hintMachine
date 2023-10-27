using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace HintMachine.ViewModels.Interfaces
{
    public abstract class ICloseableViewModel : ObservableObject
    {
        public event EventHandler CloseRequest;

        protected void CloseWindow()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
