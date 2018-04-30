using LANshare.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LANshare
{
    public interface ListWindow<T>
    {
        void setList(List<T> list);
        event EventHandler peopleButtonClick;
        event EventHandler transfersButtonClick;
        event EventHandler settingsButtonClick;

        void OnTransferWindowSelected();
        void OnPeopleWindowSelected();
        void OnSettingWindowSelected();

        
    }
}
