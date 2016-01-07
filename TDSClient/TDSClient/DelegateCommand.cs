using System;
using System.Windows.Input;
using System.Windows.Threading;


namespace TDSClient
{
    
    public class DelegateCommand : ICommand
    {
        #region Events & Delegates

        public delegate void RaisCanExecuteChangedEventDelegate();

        public event EventHandler CanExecuteChanged;

        #endregion Events & Delegates

        #region Global Variables

        private Predicate<object> m_actCanExecute;
        private Action<object> m_actExecuteMethod;
        private Dispatcher m_objUIDispatcher;

        #endregion Global Variables

        #region Constractor

        /// <summary>
        /// Full constractor, set the execution method and the execution condition method (Is Enabled)
        /// </summary>
        /// <param name="executeFunction">The Method to Execute</param>
        /// <param name="canExecuteFunction">The Method that defindes whether the command can be executed</param>
        public DelegateCommand(Action<object> p_actExecuteFunction, Predicate<object> p_actCanExecuteFunction, Dispatcher p_objUIDispatcher)
        {
            this.m_actExecuteMethod = p_actExecuteFunction;
            this.m_actCanExecute = p_actCanExecuteFunction;
            this.m_objUIDispatcher = p_objUIDispatcher;
        }

        /// <summary>
        /// Short constractor, set the execution method.
        /// The method will be allwayes accessable (Enabled)
        /// </summary>
        /// <param name="p_actExecuteFunction">The Method to Execute</param>
        public DelegateCommand(Action<object> p_actExecuteFunction, Dispatcher p_objUIDispatcher)
        {
            this.m_actExecuteMethod = p_actExecuteFunction;
            this.m_objUIDispatcher = p_objUIDispatcher;
        }

        #endregion Constractor

        #region ICommand Members

        /// <summary>
        /// This method definds whether the command can be executed or not
        /// </summary>
        /// <param name="parameter">Parameters for the Can Execute method</param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            // In case the user set execution validation method, use it, otherwise the method can be executed
            if (m_actCanExecute != null)
            {
                return m_actCanExecute(parameter);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Execute the requested functionality
        /// </summary>
        /// <param name="parameter">The parameters to the excecute method</param>
        public void Execute(object parameter)
        {
            //m_actExecuteMethod(parameter);
            App.Current.Dispatcher.BeginInvoke(m_actExecuteMethod, DispatcherPriority.Background, parameter);
        }

        #endregion ICommand Members

        #region Public Methods

        /// <summary>
        /// Rais an event about execution ability possible change
        /// </summary>
        public void RaisCanExecuteChanged()
        {
            if (m_objUIDispatcher != null)
            {
                m_objUIDispatcher.BeginInvoke(new RaisCanExecuteChangedEventDelegate(RaisCanExecuteChangedEvent));
            }
            else
            {
                RaisCanExecuteChangedEvent();
            }
        }

        private void RaisCanExecuteChangedEvent()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        #endregion Public Methods
    }
}