using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Data;
using System.Collections;

namespace WpfUtil.Behaviors {
    public class UpdateBindingAction : TriggerAction<DependencyObject> {

        public static DependencyProperty TargetObjectProperty = DependencyProperty.Register("BindingGroup", typeof(DependencyObject), typeof(UpdateBindingAction), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public DependencyObject TargetObject {
            get { return (DependencyObject)GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        protected override void Invoke(object parameter) {
            var obj = TargetObject ?? AssociatedObject;
            if (obj == null) return;
            BindingGroup group;
            DependencyObject dobj;
            if ((group = obj as BindingGroup) != null){
                group.UpdateSources();

            } else if ((dobj = obj as DependencyObject) != null) {
                if((group = (BindingGroup)dobj.GetValue(FrameworkElement.BindingGroupProperty)) != null){
                    group.UpdateSources();
                } else{
                    UpdateBindings(dobj);
                }
            }
            
        }

        private void UpdateBindings(DependencyObject obj) {
            var e = obj.GetLocalValueEnumerator();
            while (e.MoveNext()) {
                var value = e.Current;
                var expression = BindingOperations.GetBindingExpressionBase(obj, value.Property);
                if (expression != null) expression.UpdateSource();
            }
        }

    }

}
