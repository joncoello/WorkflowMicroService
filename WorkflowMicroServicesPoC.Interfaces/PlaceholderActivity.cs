using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Xaml;

namespace WorkflowMicroServicesPoC.Interfaces
{
    #region PlaceholderActivityTypeDescriptor

    public class PlaceholderActivityTypeDescriptor : ICustomTypeDescriptor
    {
        PropertyDescriptorCollection cachedProperties;
        Activity owner;

        public PlaceholderActivityTypeDescriptor(Activity owner)
        {
            this.owner = owner;
            this.Properties = new ActivityPropertyCollection(this);
        }

        public string Name
        {
            get;
            set;
        }

        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get;
            private set;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this.owner, true);
        }

        public string GetClassName()
        {
            if (this.Name != null)
            {
                return this.Name;
            }

            return TypeDescriptor.GetClassName(this.owner, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this.owner, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this.owner, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this.owner, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this.owner, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this.owner, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this.owner, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this.owner, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection result = this.cachedProperties;
            if (result != null)
            {
                return result;
            }

            PropertyDescriptorCollection dynamicProperties;
            if (attributes != null)
            {
                dynamicProperties = TypeDescriptor.GetProperties(this.owner, attributes, true);
            }
            else
            {
                dynamicProperties = TypeDescriptor.GetProperties(this.owner, true);
            }

            // initial capacity is Properties + Name + Body
            List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>(this.Properties.Count + 2);
            for (int i = 0; i < dynamicProperties.Count; i++)
            {
                PropertyDescriptor dynamicProperty = dynamicProperties[i];
                if (dynamicProperty.IsBrowsable)
                {
                    propertyDescriptors.Add(dynamicProperty);
                }
            }

            foreach (DynamicActivityProperty property in Properties)
            {
                if (string.IsNullOrEmpty(property.Name))
                {
                    throw new ArgumentNullException("property.Name");
                }
                if (property.Type == null)
                {
                    throw new ArgumentNullException("property.Type");
                }
                propertyDescriptors.Add(new DynamicActivityPropertyDescriptor(property, this.owner.GetType()));
            }

            result = new PropertyDescriptorCollection(propertyDescriptors.ToArray());
            this.cachedProperties = result;
            return result;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.owner;
        }

        class DynamicActivityPropertyDescriptor : PropertyDescriptor
        {
            AttributeCollection attributes;
            DynamicActivityProperty activityProperty;
            Type componentType;

            public DynamicActivityPropertyDescriptor(DynamicActivityProperty activityProperty, Type componentType)
                : base(activityProperty.Name, null)
            {
                this.activityProperty = activityProperty;
                this.componentType = componentType;
            }

            public override Type ComponentType
            {
                get
                {
                    return this.componentType;
                }
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    if (this.attributes == null)
                    {
                        AttributeCollection inheritedAttributes = base.Attributes;
                        Collection<Attribute> propertyAttributes = this.activityProperty.Attributes;
                        Attribute[] totalAttributes = new Attribute[inheritedAttributes.Count + propertyAttributes.Count + 1];
                        inheritedAttributes.CopyTo(totalAttributes, 0);
                        propertyAttributes.CopyTo(totalAttributes, inheritedAttributes.Count);
                        totalAttributes[inheritedAttributes.Count + propertyAttributes.Count] = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);
                        this.attributes = new AttributeCollection(totalAttributes);
                    }
                    return this.attributes;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.activityProperty.Type;
                }
            }

            public override object GetValue(object component)
            {
                PlaceholderActivity owner = component as PlaceholderActivity;
                if (owner == null || !owner.Properties.Contains(this.activityProperty))
                {
                    throw new ApplicationException("Invalid activity property");
                }

                return this.activityProperty.Value;
            }

            public override void SetValue(object component, object value)
            {
                PlaceholderActivity owner = component as PlaceholderActivity;
                if (owner == null || !owner.Properties.Contains(this.activityProperty))
                {
                    throw new ApplicationException("Invalid activity property");
                }

                this.activityProperty.Value = value;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            protected override void FillAttributes(IList attributeList)
            {
                if (attributeList == null)
                {
                    throw new ArgumentNullException("attributeList");
                }

                attributeList.Add(new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            }
        }

        class ActivityPropertyCollection : KeyedCollection<string, DynamicActivityProperty>
        {
            PlaceholderActivityTypeDescriptor parent;

            public ActivityPropertyCollection(PlaceholderActivityTypeDescriptor parent)
            {
                this.parent = parent;
            }

            protected override void InsertItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                if (this.Contains(item.Name))
                {
                    throw new ArgumentException("item");
                }

                InvalidateCache();
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                // We don't want self-assignment to throw. Note that if this[index] has the same
                // name as item, no other element in the collection can.
                if (!this[index].Name.Equals(item.Name) && this.Contains(item.Name))
                {
                    throw new ArgumentException("item");
                }

                InvalidateCache();
                base.SetItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                InvalidateCache();
                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                InvalidateCache();
                base.ClearItems();
            }

            protected override string GetKeyForItem(DynamicActivityProperty item)
            {
                return item.Name;
            }

            void InvalidateCache()
            {
                this.parent.cachedProperties = null;
            }
        }
    }

    #endregion

    #region PlaceholderActivity
    public class PlaceholderActivity : Activity, ICustomTypeDescriptor
    {
        PlaceholderActivityTypeDescriptor typeDescriptor;
        Collection<Attribute> attributes;

        public PlaceholderActivity(DynamicActivity dynamicActivity)
            : this()
        {
            this.ApplyDynamicActivity(dynamicActivity);
        }

        public PlaceholderActivity()
        {
            this.typeDescriptor = new PlaceholderActivityTypeDescriptor(this);
        }

        private void ApplyDynamicActivity(DynamicActivity dynamicActivity)
        {
            this.DisplayName = dynamicActivity.Name.Substring(dynamicActivity.Name.LastIndexOf('.') + 1);

            foreach (var item in dynamicActivity.Attributes)
            {
                this.Attributes.Add(item);
            }

            foreach (var item in dynamicActivity.Constraints)
            {
                this.Constraints.Add(item);
            }

            this.Implementation = dynamicActivity.Implementation;
            this.Name = dynamicActivity.Name;

            foreach (var item in dynamicActivity.Properties)
            {
                this.Properties.Add(item);
            }
        }

        [Browsable(false)]
        public string XAML
        {
            get
            {
                var activityBuilder = new ActivityBuilder();

                foreach (var item in this.Attributes)
                {
                    activityBuilder.Attributes.Add(item);
                }

                foreach (var item in this.Constraints)
                {
                    activityBuilder.Constraints.Add(item);
                }

                activityBuilder.Implementation = this.Implementation != null ? this.Implementation() : null;
                activityBuilder.Name = this.Name;

                foreach (var item in this.Properties)
                {
                    activityBuilder.Properties.Add(item);
                }

                VisualBasic.SetSettings(activityBuilder, VisualBasic.GetSettings(this));

                var sb = new StringBuilder();
                var xamlWriter = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(new StringWriter(sb), new XamlSchemaContext()));
                XamlServices.Save(xamlWriter, activityBuilder);

                return sb.ToString();
            }
            set
            {
                this.ApplyDynamicActivity(ActivityXamlServices.Load(new StringReader(value)) as DynamicActivity);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Name
        {
            get
            {
                return this.typeDescriptor.Name;
            }
            set
            {
                this.typeDescriptor.Name = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [DependsOn("Name")]
        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [DependsOn("Attributes")]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                return this.typeDescriptor.Properties;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DependsOn("Properties")]
        public new Collection<Constraint> Constraints
        {
            get
            {
                return base.Constraints;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XamlDeferLoad(typeof(FuncDeferringLoader), typeof(Activity))]
        [DefaultValue(null)]
        [Browsable(false)]
        [Ambient]
        public new Func<Activity> Implementation
        {
            get
            {
                return base.Implementation;
            }
            set
            {
                base.Implementation = value;
            }
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.typeDescriptor.GetAttributes();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.typeDescriptor.GetClassName();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return this.typeDescriptor.GetComponentName();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return this.typeDescriptor.GetConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return this.typeDescriptor.GetDefaultEvent();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return this.typeDescriptor.GetDefaultProperty();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return this.typeDescriptor.GetEditor(editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return this.typeDescriptor.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return this.typeDescriptor.GetEvents();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.typeDescriptor.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.typeDescriptor.GetProperties(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.typeDescriptor.GetPropertyOwner(pd);
        }
    }
    #endregion
}
