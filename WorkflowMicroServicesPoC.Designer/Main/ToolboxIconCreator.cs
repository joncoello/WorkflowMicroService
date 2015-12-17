using System;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Toolbox;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace WorkflowMicroServicesPoC.Designer
{
    /// <summary>
    /// correctly populate icons for standard .net activities
    /// </summary>
    internal static class ToolboxIconCreator
    {
        public static void LoadToolboxIcons(IEnumerable<ToolboxItemWrapper> allActivityTypes, IList<ResourceReader> resourceReaders)
        {
            var builder = new AttributeTableBuilder();

            foreach (var type in allActivityTypes)
            {
                CreateToolboxBitmapAttributeForActivity(builder, type, resourceReaders.ToList());
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private static void CreateToolboxBitmapAttributeForActivity
            (AttributeTableBuilder builder, ToolboxItemWrapper builtInActivityType, IList<ResourceReader> resourceReaders)
        {
            Bitmap bitmap;

            foreach (var item in resourceReaders)
            {
                bitmap = ExtractBitmapResource(item, builtInActivityType.BitmapName);
                if (bitmap == null) continue;
                SetBitmapResource(builder, builtInActivityType, bitmap);
                return;
            }

            foreach (var item in resourceReaders)
            {
                bitmap = ExtractBitmapResource(item, "Default");
                if (bitmap == null) continue;
                SetBitmapResource(builder, builtInActivityType, bitmap);
                return;
            }
        }

        private static void SetBitmapResource(AttributeTableBuilder builder, ToolboxItemWrapper builtInActivityType, Bitmap bitmap)
        {
            var tbaType = typeof(ToolboxBitmapAttribute);
            var imageType = typeof(Image);
            var constructor = tbaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { imageType, imageType }, null);
            var tba = constructor.Invoke(new object[] { bitmap, bitmap }) as ToolboxBitmapAttribute;
            builder.AddCustomAttributes(Type.GetType(string.Format("{0}, {1}", builtInActivityType.ToolName, builtInActivityType.AssemblyName), true), tba);
        }

        private static Bitmap ExtractBitmapResource(ResourceReader resourceReader, string bitmapName)
        {
            IDictionaryEnumerator dictEnum = resourceReader.GetEnumerator();
            Bitmap bitmap = null;
            while (dictEnum.MoveNext())
            {
                if (!Equals(dictEnum.Key, bitmapName)) continue;
                bitmap = dictEnum.Value as Bitmap;
                var pixel = Color.FromArgb(-65281);

                if (bitmap != null) bitmap.MakeTransparent(pixel);
                break;
            }
            return bitmap;
        }
    }
}
