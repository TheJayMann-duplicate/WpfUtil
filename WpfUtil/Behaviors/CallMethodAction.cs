using System;
using System.Collections.Generic;
using System.Linq;
using Expression = System.Linq.Expressions.Expression;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Data;
using System.Dynamic;

namespace WpfUtil.Behaviors {
    /// <summary>
    /// This class calls a method on an object as a response to a trigger.
    /// </summary>
    /// <remarks>
    /// When an object of this class is invoked, the defined method is invoked for the
    /// target object.  No result occurs if the method does not exist on the target object.
    /// Currently only static objects and the statically defined methods of dynamic objects
    /// are supported, due to existing dynamic binders throwing exceptions when methods are
    /// missing, and the difficulty of creating a specific binder, as that would require
    /// making an InvokeMethod binder, which if failed would then require implementing
    /// a GetMember binder, then run the object returned either through an Invoke binder
    /// if dynamic, or attempt to invoke the invoke method if a delegate object.
    /// </remarks>
    public class CallMethodAction : TriggerAction<DependencyObject> {

        public CallMethodAction() {
            // Set a data binding so that TargetObject has a suitable default of
            // the data context, equivilant to <wb:CallMethodAction TargetObject="{Binding}"/>
            BindingOperations.SetBinding(this, TargetObjectProperty, new Binding());
        }

        /// <summary>
        /// This DependencyProperty represents the TargetObject property.
        /// </summary>
        public static DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject", typeof(object), typeof(CallMethodAction), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnTargetObjectChanged));


        private string _MethodName;
        private Type _ParameterType = typeof(void);
        private Action<object, object> _CallMethod;

        /// <summary>
        /// Gets or sets the target.  The target defines the method to be invoked.
        /// </summary>
        public object TargetObject {
            get { return GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }


        /// <summary>
        /// Gets or sets the name of the method to invoke.
        /// </summary>
        public string MethodName {
            get { return _MethodName; }
            set {
                if (_MethodName != value) {
                    _MethodName = value;
                    _CallMethod = Rebind();
                }
            }
        }

        /// <summary>
        /// Gets or sets the parameter type of the method to be invoked.  If invoking a parameterless
        /// method, set this to null or void.
        /// </summary>
        public Type ParameterType {
            get { return _ParameterType; }
            set {
                if (_ParameterType != value) {
                    _ParameterType = value;
                    _CallMethod = Rebind();
                }
            }
        }

        private static void OnTargetObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var @this = d as CallMethodAction;
            if (@this == null) return;

            var oldtype = e.OldValue == null ? null : e.OldValue.GetType();
            var newtype = e.NewValue == null ? null : e.NewValue.GetType();

            // Rebinding need only occur if the types change.  Even if the values change, if the
            // types remain the same, we can use the same cached MethodInfo.
            if (oldtype != newtype) {
                @this._CallMethod = @this.RebindStatic(newtype);
            }
        }

        protected override void Invoke(object parameter) {
            if(_ParameterType == null || _ParameterType == typeof(void) || _ParameterType.IsAssignableFrom(parameter.GetType())){
                _CallMethod(TargetObject, parameter);
            }
        }

        /// <summary>
        /// Wrapper around the RebindStatic and the currently nonexistant RebindDynamic methods.  This method
        /// encapsulates the code required to extract the information required to rebind, as well in the
        /// future to determine which method of rebinding, for when the method signature is changed.
        /// </summary>
        /// <returns>
        /// A delegate which takes a target object and a parameter and invokes the method on the target,
        /// passing the parameter to the method.
        /// </returns>
        private Action<object, object> Rebind() {
            var target = TargetObject;
            if (target == null) return (t, p) => { };
            var type = target.GetType();
            return RebindStatic(type);

        }

        /// <summary>
        /// Implementation for Rebind for static types.
        /// </summary>
        private Action<object, object> RebindStatic(Type type) {
            if (type == null || string.IsNullOrWhiteSpace(_MethodName)) return (t, p) => { };
            bool notype = _ParameterType == null || _ParameterType == typeof(void);
            var method = type.GetMethod(_MethodName, notype ? Type.EmptyTypes : new Type[] { _ParameterType });
            if (method == null) {
                return (t, p) => { };
            } else {
                return (t, p) => method.Invoke(t, notype ? null : new object[] { p });
            }
        }

    }
}
