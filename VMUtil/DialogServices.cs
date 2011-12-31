using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VMUtil {

    /// <summary>
    /// The DialogServices class is used to allow code to display various dialogs without
    /// knowledge of the user interface used.
    /// </summary>
    public static class DialogServices {


        #region Synchronization

        /// <summary>
        /// Creates a reference for a value.
        /// </summary>
        /// <remarks>
        /// The Ref class acts as an additional reference layer, effectively turning
        /// value types into reference types, and reference types into double reference
        /// types.  This is similar to the ref keyword for methods, but allows the effect
        /// for methods that don't declare ref, as well as places where ref cannot be declared.
        /// It also allows storing value types as references while avoiding boxing and unboxing
        /// operations.  The Value is implemented as a public field for optimization, as no
        /// protection is needed, as a Ref is designed to be used in place of a value, so
        /// anything with direct access to a Ref would otherwise have direct access to its
        /// value anyway.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the value being referenced.
        /// </typeparam>
        private class Ref<T>{
            public T Value;
        }

        /// <summary>
        /// Simplify creating a Ref type for complex and/or annonymous types.
        /// </summary>
        private static Ref<T> CreateRef<T>(T value){
            return new Ref<T> { Value = value};
        }

        /// <summary>
        /// Allows wrapping a generic Action method into what is expected from
        /// SendOrPostCallback while avoiding variable capture.
        /// </summary>
        private static void SyncMessage<T>(object messageState){
            var message = (Tuple<Action<T>, T>)messageState;
            message.Item1(message.Item2);
        }

        private static SynchronizationContext _Sync = null;

        public static void SetSynchronizationContext(SynchronizationContext sync) {
            _Sync = sync;
        }

        /// <summary>
        /// Attempts to synchronize calls to the DialogServices methods.
        /// </summary>
        private static void SendSyncMessage<T>(T state, Action<T> message){
            var sync = _Sync;
            if (sync == null){
                message(state);
            }else{
                sync.Send(SyncMessage<T> , Tuple.Create(message, state));
            }
            
        }

        #endregion

        #region Message Dialogs

        // A simple set of functions for displaying an unconditional message.
        // No feedback is generated for the source of the message.
        
        private static Action<string, string> _ShowMessage;

        /// <summary>
        /// Displays a modal dialog with a specified message.
        /// </summary>
        /// <param name="message">
        /// The message to display.
        /// </param>
        public static void ShowMessage(string message) {
            ShowMessage(message, string.Empty);
        }

        /// <summary>
        /// Displays a modal dialog with a specified message and title.
        /// </summary>
        /// <param name="message">
        /// The message to display.
        /// </param>
        /// <param name="title">
        /// The title for the dialog.
        /// </param>
        public static void ShowMessage(string message, string title) {
            var showMessage = _ShowMessage;
            if (showMessage != null) SendSyncMessage(new {showMessage, message, title}, t => t.showMessage(t.message, t.title));
        }


        /// <summary>
        /// Registers an action used to display a message dialog.
        /// </summary>
        /// <param name="message">
        /// An action which takes a message and a title and displays the message.
        /// </param>
        /// <remarks>
        /// This method is used by code with knowledge of the user interface to display
        /// a simple message dialog when code without knowledge of the user interface
        /// needs to display a message.
        /// </remarks>
        public static void RegisterMessageDialog(Action<string, string> message) {
            _ShowMessage = message;
        }

        #endregion

        #region Choice Dialogs



        private static Func<string, string, SimpleBooleanDialogStyle, bool> _SimpleBooleanDialog;

        /// <summary>
        /// Displays a message while allowing the choice one of two responses,
        /// typically representing acceptance or rejection.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <returns>True if accepted, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        /// A dialog handler has not been registered.
        /// </exception>
        public static bool SimpleBooleanDialog(string message) {
            return SimpleBooleanDialog(message, string.Empty, SimpleBooleanDialogStyle.OkCancel);
        }

        /// <summary>
        /// Displays a message with a title while allowing the choice of one of
        /// two responses, typically representing acceptance or rejection.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title for the dialog.</param>
        /// <returns>True if accepted, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        /// A dialog handler has not been registered.
        /// </exception>
        public static bool SimpleBooleanDialog(string message, string title) {
            return SimpleBooleanDialog(message, title, SimpleBooleanDialogStyle.OkCancel);
        }

        /// <summary>
        /// Displays a message allowing the choice of two responses as dictated
        /// by the dialogStyle parameter.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="dialogStyle">
        /// A SimpleBooleanDialogStyle that determines which choices appear.
        /// </param>
        /// <returns>True if accepted, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        /// A dialog handler has not been registered.
        /// </exception>
        public static bool SimpleBooleanDialog(string message, SimpleBooleanDialogStyle dialogStyle) {
            return _SimpleBooleanDialog(message, string.Empty, dialogStyle);
        }

        /// <summary>
        /// Displays a message with a title allowing the choice of two responses
        /// as dictated by the dialogStyle parameter.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="dialogStyle">
        /// A SimpleBooleanDialogStyle that determines which choices appear.
        /// </param>
        /// <returns>True if accepted, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        /// A dialog handler has not been registered.
        /// </exception>
        public static bool SimpleBooleanDialog(string message, string title, SimpleBooleanDialogStyle dialogStyle) {
            var simpleBooleanDialog = _SimpleBooleanDialog;
            if (simpleBooleanDialog != null) {
                var state = new {simpleBooleanDialog, message, title, dialogStyle, returnValue = new Ref<bool>() };
                SendSyncMessage(state, s => s.returnValue.Value = s.simpleBooleanDialog(s.message, s.title, s.dialogStyle));
                return state.returnValue.Value;
            }
            throw new InvalidOperationException("A dialog expecting a return value must be registered before use.");
        }

        /// <summary>
        /// Registers a function used to display a message and return a value
        /// representing the response.
        /// </summary>
        /// <param name="simpleBooleanDialog">
        /// A function which takes a message, title, and style, displays the
        /// message, and returns the response chosen.
        /// </param>
        /// <remarks>
        /// This method is used by code with knowledge of the user interface to display
        /// a simple message dialog when code without knowledge of the user interface
        /// needs to display a message.
        /// </remarks>
        public static void RegisterSimpleBooleanDialog(Func<string, string, SimpleBooleanDialogStyle, Boolean> simpleBooleanDialog){
            _SimpleBooleanDialog = simpleBooleanDialog;
        }
        #endregion

        #region Function Dialogs

        #endregion

    }
    
    /// <summary>
    /// The SimpleBooleanDialogStyle values determine whether to display
    /// Ok and Cancel or Yes and No as its boolean choices.
    /// </summary>
    public enum SimpleBooleanDialogStyle{
        /// <summary>
        /// Display Ok and Cancel options.
        /// </summary>
        OkCancel,
        /// <summary>
        /// Display Yes and No options.
        /// </summary>
        YesNo,
    }
}
