using System;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace WpfUtil.Behaviors {
    /// <summary>
    /// The CommandTrigger class waits for a routed command to be executed and invokes its
    /// actions.  Any commmand parameters specified are passed to its actions.
    /// </summary>
    /// <remarks>
    /// A CommandTrigger assumes it is the only handler for its specifyed routed command, and
    /// will set its Executed and CanExecute event handlers as handled.  If it is necessary
    /// for other objects to be notified, consider using PreviewExecuted and PreviewCanExecute.
    /// </remarks>
    public class CommandTrigger : TriggerBase<UIElement> {
        /// <summary>
        /// This defines the dependency property CanExecute.
        /// </summary>
        public static DependencyProperty CanExecuteProperty = DependencyProperty.Register("CanExecute", typeof(bool), typeof(CommandTrigger), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));

        private object _CommandParameter;

        /// <summary>
        /// Gets or sets the routed command to listen for.
        /// </summary>
        public ICommand Command {
            get { return _Binding.Command; }
            set { _Binding.Command = value; }
        }

        /// <summary>
        /// Gets or sets a command parameter to filter.
        /// </summary>
        /// <remarks>
        /// This is useful when the same command should trigger different actions
        /// based on what parameter is passed.  An enum value is best suited for
        /// this case.
        /// </remarks>
        public object CommandParameter {
            get { return _CommandParameter; }
            set { _CommandParameter = value; }
        }

        /// <summary>
        /// Gets or sets a value determining if the associated command can execute.
        /// </summary>
        /// <remarks>
        /// The default value is true.  It is best not to set this directly, but rather
        /// to have it set automatically, such as a property trigger or databinding.
        /// </remarks>
        public bool CanExecute {
            get { return (bool)GetValue(CanExecuteProperty); }
            set { SetValue(CanExecuteProperty, value); }
        }

        private CommandBinding _Binding;

        public CommandTrigger() {
            _Binding = new CommandBinding();
            _Binding.Executed += OnExecute;
            _Binding.CanExecute += OnCanExecute;
        }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.CommandBindings.Add(_Binding);
        }

        protected override void OnDetaching() {
            AssociatedObject.CommandBindings.Remove(_Binding);
            base.OnDetaching();
        }

        private void OnExecute(object sender, ExecutedRoutedEventArgs e) {
            // If the CommandParameter is not set, invoke the actions unconditionally.
            // Otherwise, only commands with matching parameters shall be triggered.
            if (_CommandParameter == null || _CommandParameter == e.Parameter) {
                InvokeActions(e.Parameter);
                e.Handled = true;
            }
        }

        private void OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = CanExecute;
            e.Handled = true;
        }
    }
}